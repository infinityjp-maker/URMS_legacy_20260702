$ErrorActionPreference = 'Stop'
$fixBranch = 'origin/fix/selfheal-permanent-patch-072035d'
$sha = (git rev-parse --short $fixBranch).Trim()
$br = "validate/selfheal-$sha"
Write-Output "Creating validation branch: $br"
git fetch origin --prune
git checkout -B $br origin/main
try {
  git merge --no-ff $fixBranch -m 'Merge PR#48 for validation'
} catch {
  Write-Output "Merge may have conflicts or already merged: $_"
}
git push -u origin $br
Set-Content -Path ".gh-run-scripts/_validate_branch.txt" -Value $br -Encoding UTF8
Write-Output $br
