# Push a timestamp file to main to trigger push-based workflows, then monitor the self-heal workflow run and analyze logs.
$ErrorActionPreference = 'Stop'
$repo = 'infinityjp-maker/URMS'
$workflowPath = '.github/workflows/triage-dashboard-e2e-selfheal.yml'
$reportFile = '.gh-run-scripts/selfheal_report.json'
$logsDirBase = '.gh-run-scripts'

function Get-WorkflowId {
    try {
        $id = gh api repos/$repo/actions/workflows --jq ".workflows[] | select(.path==\"$workflowPath\") | .id" 2>$null
        if ($id) { return $id.Trim() }
    } catch { }
    return 241703246
}

# 1-3: checkout main, update, create timestamp file, commit, push
git checkout main
git pull --ff-only origin main
$path = '.github/selfheal_trigger.txt'
$dir = Split-Path $path
if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
$pushTime = Get-Date
$ts = $pushTime.ToString('o')
Set-Content -Path $path -Value $ts -Encoding utf8
git add $path
try { git commit -m 'chore(ci): trigger self-heal run via timestamp file' } catch { Write-Output 'no-changes-to-commit' }
git push origin main

# 4: find workflow id and new run created after pushTime
$wfId = Get-WorkflowId
$attempt = 0
$run = $null
while (-not $run -and $attempt -lt 120) {
    Start-Sleep -Seconds 5
    $attempt++
    try {
        $lines = gh api repos/$repo/actions/workflows/$wfId/runs --jq '.workflow_runs[] | select(.head_branch=="main") | {id,created_at,status,conclusion,html_url,logs_url} | @json' 2>$null
    } catch { $lines = $null }
    if (-not $lines) { continue }
    $firstMatch = ($lines -split "`n" | Where-Object { $_ -ne '' } | Select-Object -First 1)
    if (-not $firstMatch) { continue }
    $candidate = $firstMatch | ConvertFrom-Json
    try { $created = [datetime]::Parse($candidate.created_at) } catch { $created = Get-Date "1970-01-01" }
    if ($created -ge $pushTime) { $run = $candidate; break }
}
if (-not $run) { $out = @{ error='NO_NEW_RUN_DETECTED'; workflow_id=$wfId; pushed_at=$ts; generated_at=(Get-Date).ToString('o') }; $out|ConvertTo-Json -Depth 6 | Out-File $reportFile -Encoding utf8; exit 1 }
$runId = $run.id

# 4-5: poll run until completed
$poll=0
while ($true) {
    Start-Sleep -Seconds 10
    $info = gh api repos/$repo/actions/runs/$runId --jq '{id,html_url,status,conclusion,logs_url,run_started_at,head_sha,head_branch}' 2>$null | ConvertFrom-Json
    if ($info.status -eq 'completed') { break }
    $poll++
    if ($poll -gt 360) { $out = @{ error='POLL_TIMEOUT'; run_id=$runId; workflow_id=$wfId; generated_at=(Get-Date).ToString('o') }; $out|ConvertTo-Json -Depth 6 | Out-File $reportFile -Encoding utf8; exit 1 }
}

# 5: attempt to download logs
$logsDir = Join-Path $logsDirBase ("logs_$runId")
if (Test-Path $logsDir) { Remove-Item -Recurse -Force $logsDir }
New-Item -ItemType Directory -Path $logsDir | Out-Null
$dlOk = $false
try {
    gh run download $runId --repo $repo --log --dir $logsDir 2>$null
    $dlOk = (Get-ChildItem -Recurse -Path $logsDir | Measure-Object).Count -gt 0
} catch { $dlOk = $false }

if (-not $dlOk) {
    try {
        $token = gh auth token
        $zipPath = Join-Path $logsDirBase ("logs_$runId.zip")
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
                $reportError = Get-Content $zipPath -Raw -ErrorAction SilentlyContinue
                $dlOk = $false
            }
        }
    } catch { $reportError = $_.Exception.Message }
}

# analyze logs
$patterns = 'Error','Exception','Traceback','FAIL','failed','Playwright','selector','upload-artifact','409','Unhandled'
$matches = @()
if ($dlOk) {
    $files = Get-ChildItem -Recurse -Path $logsDir -File | Where-Object { $_.Length -gt 0 }
    foreach ($f in $files) {
        try {
            $res = Select-String -Path $f.FullName -Pattern $patterns -SimpleMatch -CaseSensitive:$false -List
        } catch { continue }
        foreach ($m in $res) {
            $matches += [ordered]@{ file = $f.FullName; lineNumber = $m.LineNumber; line = $m.Line.Trim(); matched = $m.Matches.Value }
        }
    }
}

# build report
$report = [ordered]@{
    workflow_id = [int]$wfId
    run = [ordered]@{ id = $info.id; html_url = $info.html_url; logs_url = $info.logs_url; conclusion = $info.conclusion; run_started_at = $info.run_started_at; head_sha = $info.head_sha; head_branch = $info.head_branch }
    pushed_at = $ts
    logs_downloaded = $dlOk
    logs_path = if ($dlOk) { $logsDir } else { $null }
    errors = $matches
    generated_at = (Get-Date).ToString('o')
}

if (-not $dlOk) {
    if ($reportError) { $report.logs_error = $reportError }
    $report.root_cause_class = 'GitHub Actions の内部エラー'
    $report.root_cause = 'Run exists but logs or job artifacts are not available (404 or empty jobs).'
} else {
    if ($matches.Count -eq 0) { $report.root_cause_class = '正常動作'; $report.root_cause = '正常動作 (エラー行は検出されませんでした)' }
    else { $report.root_cause_class = '調査要'; $report.root_cause = 'ログにエラー行あり。詳細を確認してください' }
}

$report.suggested_minimal_patch = if ($report.root_cause_class -eq 'GitHub Actions の内部エラー') { '再実行（push または手動）を試み、継続する場合は GitHub サポートにログ復旧を依頼' } else { '手動でテスト/コードの修正を提案' }

$report | ConvertTo-Json -Depth 8 | Out-File -FilePath $reportFile -Encoding utf8
Write-Output "REPORT_SAVED:$reportFile"
