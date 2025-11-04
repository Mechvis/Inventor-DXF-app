param(
  [string]$Output = "artifacts/ReviewBundle.zip"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$repo = Resolve-Path (Join-Path $root "..") | Select-Object -ExpandProperty Path

Set-Location $repo
New-Item -ItemType Directory -Force -Path "artifacts" | Out-Null

$exclude = @('\.git($|\\)', '\.vs($|\\)', '\\bin\\', '\\obj\\', '\\TestResults\\', '\\packages\\', '\\artifacts\\')
$files = Get-ChildItem -Recurse -File | Where-Object {
  $path = $_.FullName
  -not ($exclude | Where-Object { $path -imatch $_ })
}

$zip = Resolve-Path (Join-Path $repo $Output)
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path $files.FullName -DestinationPath $zip -Force

# Summary manifest for reviewers
$manifest = @()
$manifest += "# Review bundle"
$manifest += "Created: $(Get-Date -Format s)"
$manifest += ""
$sl = Get-ChildItem -Recurse -Filter *.sln | Select-Object -First 1
if ($sl) { $manifest += "Solution: $($sl.FullName)" }
$addin = Get-ChildItem -Recurse -Filter *.addin
foreach ($a in $addin) { $manifest += "Addin: $($a.FullName)" }
$bundle = Get-ChildItem -Recurse -Include PackageContents.xml
foreach ($b in $bundle) { $manifest += "Bundle: $($b.FullName)" }
$manifest | Set-Content artifacts/REVIEW_MANIFEST.md -Encoding UTF8
