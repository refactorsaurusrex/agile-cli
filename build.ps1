param(
  [string]$Version = ''
)

$ErrorActionPreference = 'Stop'
$appName = "AgileCli"

if ($env:APPVEYOR_REPO_TAG -eq 'true') {
  $Version = [regex]::match($env:APPVEYOR_REPO_TAG_NAME,'[0-9]+\.[0-9]+\.[0-9]+').Groups[0].Value
  $lastPublishedVersion = [Version]::new((Find-Module -Name $appName | Select-Object -ExpandProperty Version))
  if ([Version]::new($Version) -le $lastPublishedVersion) {
    throw "Version must be greater than the last published version, which is 'v$lastPublishedVersion'."
  }
  Write-Host "Last published version: 'v$lastPublishedVersion'. Current version: 'v$Version'"
} elseif ($null -ne $env:APPVEYOR_BUILD_NUMBER) {
  $Version = "0.0.$env:APPVEYOR_BUILD_NUMBER"
} elseif ($Version -eq '') {
  $Version = "0.0.0"
}

Write-Host "Building version '$Version'..."

if (Test-Path "$PSScriptRoot\publish") {
  Remove-Item -Path "$PSScriptRoot\publish" -Recurse -Force
}

$publishOutputDir = "$PSScriptRoot\publish\$appName"
$proj = Get-ChildItem -Filter "$appName.csproj" -Recurse -Path $PSScriptRoot | Select-Object -First 1 -ExpandProperty FullName

foreach ($target in 'win-x64','linux-x64','osx-x64') {
  dotnet publish $proj --output $publishOutputDir -c Release --self-contained false -r $target
}

if ($LASTEXITCODE -ne 0) {
  throw "Failed to publish application."
}

Remove-Item "$publishOutputDir\*.pdb"

Import-Module "$publishOutputDir\$appName.dll"
$moduleInfo = Get-Module $appName
Install-Module WhatsNew
Import-Module WhatsNew
$cmdletNames = Export-BinaryCmdletNames -ModuleInfo $moduleInfo
$cmdletAliases = Export-BinaryCmdletAliases -ModuleInfo $moduleInfo

$manifestPath = "$publishOutputDir\$appName.psd1"

$newManifestArgs = @{
  Path = $manifestPath
}

$updateManifestArgs = @{
  Path = $manifestPath
  CopyRight = "(c) $((Get-Date).Year) Nick Spreitzer"
  Description = "Get Agile stats for your project from Jira!"
  Guid = '281a05ad-baf4-478b-9aac-76616e899d4d'
  Author = 'Nick Spreitzer'
  CompanyName = 'RAWR! Productions'
  ModuleVersion = $Version
  AliasesToExport = $cmdletAliases
  NestedModules = ".\$appName.dll"
  CmdletsToExport = $cmdletNames
  CompatiblePSEditions = @("Desktop","Core")
  HelpInfoUri = "https://github.com/refactorsaurusrex/agile-cli/wiki"
  PowerShellVersion = "6.0"
  PrivateData = @{
    Tags = 'agile','jira'
    LicenseUri = 'https://github.com/refactorsaurusrex/agile-cli/blob/master/LICENSE'
    ProjectUri = 'https://github.com/refactorsaurusrex/agile-cli'
  }
}

New-ModuleManifest @newManifestArgs
Update-ModuleManifest @updateManifestArgs
Remove-ModuleManifestComments $manifestPath -NoConfirm

Install-Module platyPS
Import-Module platyPS
$docs = "$PSScriptRoot\docs"
try {
  git clone https://github.com/refactorsaurusrex/agile-cli.wiki.git $docs
  if ($LASTEXITCODE -ne 0) {
    throw "Failed to clone wiki."
  }

  Switch-CodeFenceToYamlFrontMatter -Path $docs -NoConfirm
  New-ExternalHelp -Path $docs -OutputPath $publishOutputDir
} finally {
  if (Test-Path $docs) {
    Remove-Item -Path $docs -Recurse -Force
  }
}