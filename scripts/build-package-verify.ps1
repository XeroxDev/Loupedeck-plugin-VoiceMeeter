param(
    [string]$ProjectPath = (Join-Path $PSScriptRoot '..\src\VoiceMeeterPlugin\VoiceMeeterPlugin.csproj'),
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '..\artifacts')
)

$ErrorActionPreference = 'Stop'

function Get-ProjectVersion {
    param([string]$CsprojPath)

    [xml]$projectXml = Get-Content -LiteralPath $CsprojPath
    $versionNode = $projectXml.SelectSingleNode('//Project/PropertyGroup/Version')
    if (-not $versionNode) {
        throw "Could not find <Version> in $CsprojPath"
    }

    return $versionNode.InnerText
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$projectFullPath = (Resolve-Path $ProjectPath).Path
$version = Get-ProjectVersion -CsprojPath $projectFullPath
$packageName = "VoiceMeeterPlugin-$version.lplug4"
$packagePath = Join-Path $OutputDirectory $packageName
$tool = Join-Path $env:USERPROFILE '.dotnet\tools\logiplugintool.exe'

if (-not (Test-Path -LiteralPath $tool)) {
    throw "logiplugintool.exe was not found at $tool"
}

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

Push-Location $repoRoot
try {
    $env:DOTNET_ROLL_FORWARD = 'LatestMajor'

    dotnet build $projectFullPath -c Release -t:Restore,Build
    & $tool pack (Join-Path $repoRoot 'bin\Release') $packagePath
    & $tool verify $packagePath

    Write-Host "Package ready: $packagePath"
}
finally {
    Pop-Location
}
