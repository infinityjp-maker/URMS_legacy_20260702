$ErrorActionPreference='Stop'
$branchFile = '.gh-run-scripts\_validate_branch.txt'
if(-not (Test-Path $branchFile)) { throw 'Validation branch file not found' }
$branch = (Get-Content $branchFile -Raw).Trim()
if([string]::IsNullOrWhiteSpace($branch)){ throw 'Validation branch name empty' }
Write-Output "Using branch: $branch"
try{ git rev-parse --verify $branch }catch{ & git fetch origin ($branch+':'+$branch) }
git checkout $branch
$ts = Get-Date -Format yyyyMMddHHmmss
$commitMsg = "ci: trigger selfheal validation $ts"
Write-Output "Creating empty commit and pushing to trigger workflow"
try{ git commit --allow-empty -m "$commitMsg" }catch{ Write-Output 'EMPTY_COMMIT_SKIPPED' }
git push origin $branch
# Poll for workflow run
$workflow_id = 241703246
$run = $null
$timeout = 1200 # seconds
$interval = 10
$elapsed = 0
Write-Output "Polling for workflow run for branch $branch (workflow id $workflow_id)"
while($elapsed -lt $timeout){ Start-Sleep -Seconds $interval; $elapsed += $interval; try{ $runsJson = gh api repos/infinityjp-maker/URMS/actions/workflows/$workflow_id/runs --jq ".workflow_runs | map(select(.head_branch==\"$branch\")) | .[0]" 2>$null }catch{ $runsJson = $null }
  if($runsJson -and $runsJson -ne 'null'){ $run = $runsJson; break }
}
if(-not $run){ throw 'No workflow run found for validation branch within timeout' }
$runObj = $run | ConvertFrom-Json
$runId = $runObj.id
$head_sha = $runObj.head_sha
Write-Output "Found run: $runId (sha: $head_sha)"
# Poll until completed
$status = $runObj.status
$elapsed = 0
$maxStatusTimeout = 7200 # seconds
while($status -ne 'completed' -and $elapsed -lt $maxStatusTimeout){ Start-Sleep -Seconds $interval; $elapsed += $interval; $runStatusJson = gh api repos/infinityjp-maker/URMS/actions/runs/$runId --jq '{id:.id,status:.status,conclusion:.conclusion,head_sha:.head_sha,created_at:.created_at}' ; $runObj = $runStatusJson | ConvertFrom-Json; $status = $runObj.status; Write-Output "status=$status" }
if($status -ne 'completed'){ throw 'Workflow run did not complete in time' }
Write-Output "Run completed with conclusion: $($runObj.conclusion)"
# Download logs
$logsDir = ".gh-run-scripts/logs_$runId"
New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
$dl = gh run download $runId --repo infinityjp-maker/URMS -D $logsDir 2>&1
if($LASTEXITCODE -ne 0){ Write-Output "Logs download may have failed: $dl" }
# Get jobs
$jobsJson = gh api repos/infinityjp-maker/URMS/actions/runs/$runId/jobs 2>$null
$jobsObj = $null
if($jobsJson -and $jobsJson -ne 'null'){ $jobsObj = $jobsJson | ConvertFrom-Json }
# Search logs for errors
$errors = @()
Get-ChildItem -Path $logsDir -Recurse -File -ErrorAction SilentlyContinue | ForEach-Object {
  $content = Get-Content -Path $_.FullName -Raw -ErrorAction SilentlyContinue
  if($null -ne $content){ $matches = Select-String -InputObject $content -Pattern 'Error|Exception|FAIL|fail|Traceback' -AllMatches -SimpleMatch -ErrorAction SilentlyContinue
    foreach($m in $matches){ $errors += @{file=$_.FullName; line=$m.Line} }
  }
}
# Compose report
$report = [PSCustomObject]@{
  runId = $runId
  head_sha = $head_sha
  conclusion = $runObj.conclusion
  created_at = $runObj.created_at
  jobs = $jobsObj
  logs_downloaded = (Test-Path $logsDir)
  log_dir = $logsDir
  error_excerpts = $errors
}
$report | ConvertTo-Json -Depth 10 | Out-File -FilePath .gh-run-scripts/selfheal_v2_validation_report.json -Encoding utf8
Write-Output 'VALIDATION_DONE'
