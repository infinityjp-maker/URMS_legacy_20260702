# Diagnostic workflow: create branch, push workflow file, dispatch workflow, poll run, download logs, analyze, write report
$ErrorActionPreference = 'Stop'
$repo = 'infinityjp-maker/URMS'
$workflowPath = '.github/workflows/diagnostic-minimal.yml'
$reportFile = '.gh-run-scripts/diagnostic_report.json'
$logsDirBase = '.gh-run-scripts'

# 1. create branch
$ts = Get-Date -Format 'yyyyMMddHHmmss'
$branch = "diag/gha-minimal-check-$ts"
Write-Output "Creating branch $branch"
git checkout -b $branch

# 2-4. add workflow and push
git add $workflowPath
try { git commit -m 'chore(ci): add diagnostic minimal workflow' } catch { Write-Output 'no-change'; }
git push -u origin $branch

# 5. dispatch workflow on the new branch
Write-Output "Dispatching workflow $workflowPath on ref $branch"
$dispatchResp = gh api --method POST repos/$repo/actions/workflows/$workflowPath/dispatches -f ref=$branch 2>&1
if ($LASTEXITCODE -ne 0) { Write-Output "Dispatch failed: $dispatchResp" }
$dispatchTime = Get-Date

# 6. poll for run
$run = $null
$attempt = 0
while (-not $run -and $attempt -lt 120) {
    Start-Sleep -Seconds 5
    $attempt++
    try {
        $lines = gh api repos/$repo/actions/workflows/$workflowPath/runs --jq '.workflow_runs[] | select(.head_branch=="'+$branch+'") | {id,created_at,status,conclusion,html_url,logs_url} | @json' 2>$null
    } catch { $lines = $null }
    if (-not $lines) { continue }
    $first = ($lines -split "`n" | Where-Object { $_ -ne '' } | Select-Object -First 1)
    if (-not $first) { continue }
    $candidate = $first | ConvertFrom-Json
    try { $created = [datetime]::Parse($candidate.created_at) } catch { $created = Get-Date "1970-01-01" }
    if ($created -ge $dispatchTime) { $run = $candidate; break }
}
if (-not $run) { $out = @{ error='NO_RUN_DETECTED'; branch=$branch; dispatched_at=$dispatchTime.ToString('o'); generated_at=(Get-Date).ToString('o') }; $out|ConvertTo-Json -Depth 6 | Out-File $reportFile -Encoding utf8; Write-Output "NO_RUN_DETECTED"; exit 1 }
$runId = $run.id
Write-Output ("Detected run {0}: {1}" -f $runId, $run.html_url)

# 6b: poll until completed
$poll = 0
while ($true) {
    Start-Sleep -Seconds 5
    $info = gh api repos/$repo/actions/runs/$runId --jq '{id,html_url,status,conclusion,logs_url,run_started_at,head_sha,head_branch}' 2>$null | ConvertFrom-Json
    if ($info.status -eq 'completed') { break }
    $poll++
    if ($poll -gt 360) { $out = @{ error='POLL_TIMEOUT'; run_id=$runId; generated_at=(Get-Date).ToString('o') }; $out|ConvertTo-Json -Depth 6 | Out-File $reportFile -Encoding utf8; Write-Output 'POLL_TIMEOUT'; exit 1 }
}

# prepare report base
$report = [ordered]@{
    diagnostic_branch = $branch
    workflow_path = $workflowPath
    run = [ordered]@{ id=$info.id; html_url=$info.html_url; status=$info.status; conclusion=$info.conclusion; logs_url=$info.logs_url; run_started_at=$info.run_started_at }
    logs_downloaded = $false
    logs_path = $null
    errors = @()
    root_cause = $null
    suggested_patch = $null
    generated_at = (Get-Date).ToString('o')
}

# if success
if ($info.conclusion -eq 'success') {
    $report.root_cause = '正常動作'
    $report.suggested_patch = 'なし: GitHub Actions はジョブを生成可能'
    $report.logs_downloaded = $false
    $report.errors = @()
    $report | ConvertTo-Json -Depth 8 | Out-File $reportFile -Encoding utf8
    Write-Output "SUCCESS: report written to $reportFile"
    exit 0
}

# failure: attempt to download logs
$logsDir = Join-Path $logsDirBase ("diag_logs_$runId")
if (Test-Path $logsDir) { Remove-Item -Recurse -Force $logsDir }
New-Item -ItemType Directory -Path $logsDir | Out-Null
$dlOk = $false
try {
    # try gh run download for artifacts (may not include logs)
    gh run download $runId --repo $repo --dir $logsDir 2>$null
} catch { }

# try API logs
try {
    $token = gh auth token
    $zipPath = Join-Path $logsDirBase ("diag_logs_$runId.zip")
    $url = "https://api.github.com/repos/$repo/actions/runs/$runId/logs"
    $args = @('-L','-H',"Authorization: token $token",$url,'-o',$zipPath)
    Start-Process -FilePath 'curl' -ArgumentList $args -NoNewWindow -Wait -PassThru | Out-Null
    if (Test-Path $zipPath) {
        $size = (Get-Item $zipPath).Length
        if ($size -gt 1000) {
            Expand-Archive -Path $zipPath -DestinationPath $logsDir -Force
            Remove-Item $zipPath -Force
            $dlOk = $true
        } else {
            $report.logs_error = Get-Content $zipPath -Raw -ErrorAction SilentlyContinue
        }
    }
} catch { $report.logs_error = $_.Exception.Message }

$report.logs_downloaded = $dlOk
if ($dlOk) { $report.logs_path = $logsDir }

# search for error keywords if we have logs
$patterns = 'Error','Exception','Traceback','FAIL','failed','Playwright','selector','upload-artifact','409','Unhandled'
if ($dlOk) {
    $matches = @()
    $files = Get-ChildItem -Recurse -Path $logsDir -File | Where-Object { $_.Length -gt 0 }
    foreach ($f in $files) {
        try {
            $res = Select-String -Path $f.FullName -Pattern $patterns -SimpleMatch -CaseSensitive:$false -List
        } catch { continue }
        foreach ($m in $res) {
            $matches += [ordered]@{ file = $f.FullName; lineNumber = $m.LineNumber; line = $m.Line.Trim(); matched = $m.Matches.Value }
        }
    }
    $report.errors = $matches
    if ($matches.Count -eq 0) { $report.root_cause = 'ログ取得成功だがエラー行は検出されず' ; $report.suggested_patch = '確認: 実際の self-heal ジョブを観察' }
    else { $report.root_cause = 'ログにエラー行あり'; $report.suggested_patch = '検査と修正を実施' }
} else {
    $report.root_cause = 'ログが取得できないか存在しない (可能性: GitHub Actions の内部問題)'
    $report.suggested_patch = '再実行または GitHub サポートへお問い合わせ'
}

$report | ConvertTo-Json -Depth 10 | Out-File $reportFile -Encoding utf8
Write-Output "REPORT_SAVED:$reportFile"
