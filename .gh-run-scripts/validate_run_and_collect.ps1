param()
$ErrorActionPreference='Stop'
$branch = Get-Content .gh-run-scripts\_validate_branch.txt -Raw | ForEach-Object { $_.Trim() } 
if([string]::IsNullOrWhiteSpace($branch)){ throw 'Validation branch name not found' }
Write-Output "Using branch: $branch"
# Trigger workflow
$wf='triage-dashboard-e2e-selfheal.yml'
Write-Output "Dispatching workflow $wf on $branch"
gh workflow run $wf --ref $branch
# Poll for the run
$workflow_id = 241703246
$run = $null
$timeout = 1200 # seconds
$interval = 10
$elapsed = 0
while($elapsed -lt $timeout){ Start-Sleep -Seconds $interval; $elapsed += $interval; $runsJson = gh api repos/infinityjp-maker/URMS/actions/workflows/$workflow_id/runs --jq ".workflow_runs | map(select(.head_branch==\"$branch\")) | .[0]" 2>$null; if($runsJson -and $runsJson -ne 'null'){ $run = $runsJson; break } }
if(-not $run){ throw 'No workflow run found for validation branch within timeout' }
# parse run info
$runObj = $run | ConvertFrom-Json
$runId = $runObj.id
$head_sha = $runObj.head_sha
# Poll until completed
$status = $runObj.status
while($status -ne 'completed' -and $elapsed -lt ($timeout*2)){
  Start-Sleep -Seconds $interval
  $elapsed += $interval
  $run = gh api repos/infinityjp-maker/URMS/actions/runs/$runId --jq '{id:.id,status:.status,conclusion:.conclusion,head_sha:.head_sha,created_at:.created_at}'
  $runObj = $run | ConvertFrom-Json
  $status = $runObj.status
}
if($status -ne 'completed'){ throw 'Workflow run did not complete in time' }
# Download logs
$logsDir = ".gh-run-scripts/logs_$runId"
New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
gh run download $runId --repo infinityjp-maker/URMS -D $logsDir --name "logs_$runId" || Write-Output 'download-failed'
# Get jobs
$jobs = gh api repos/infinityjp-maker/URMS/actions/runs/$runId/jobs --jq '{total_count:.total_count,jobs:.jobs}' 2>$null
# Extract error lines from logs
$errors = @()
Get-ChildItem -Path $logsDir -Recurse -Filter *.txt -ErrorAction SilentlyContinue | ForEach-Object {
  $content = Get-Content -Path $_.FullName -Raw -ErrorAction SilentlyContinue
  if($content -match 'Error|Exception|FAIL|fail|Traceback'){
    $matches = Select-String -InputObject $content -Pattern 'Error|Exception|FAIL|fail|Traceback' -AllMatches
    foreach($m in $matches){ $errors += @{file=$_.FullName; line=$m.Line } }
  }
}
# Compose report
$report = @{ runId = $runId; head_sha = $head_sha; conclusion = $runObj.conclusion; jobs = ($jobs | ConvertFrom-Json); logs_downloaded = (Test-Path $logsDir); error_excerpts = $errors }
$report | ConvertTo-Json -Depth 10 | Out-File -FilePath .gh-run-scripts/selfheal_validation_report.json -Encoding utf8
Write-Output 'VALIDATION_DONE'
