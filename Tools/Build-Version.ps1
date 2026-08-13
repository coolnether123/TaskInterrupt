<#
.SYNOPSIS
Builds one Task Interrupt payload for the shared RimWorld cascade executor.

.DESCRIPTION
The cascade executor supplies the configuration as the first argument and
provides an external output root through RWT_CASCADE_BUILD_OUTPUT_ROOT. The
script delegates reference resolution and compilation to RimWorld-Tooling,
then copies only the validated DLL into the version-owned payload folder.
#>
param(
    [Parameter(Mandatory = $true)][string]$Configuration
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repository = [System.IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$toolingRoot = [Environment]::GetEnvironmentVariable('RWT_CASCADE_TOOLING_ROOT')
$outputRoot = [Environment]::GetEnvironmentVariable('RWT_CASCADE_BUILD_OUTPUT_ROOT')
if ([string]::IsNullOrWhiteSpace($toolingRoot) -or
    [string]::IsNullOrWhiteSpace($outputRoot))
{
    throw 'Task Interrupt cascade builds require the shared cascade environment.'
}

$toolingRoot = [System.IO.Path]::GetFullPath($toolingRoot)
$outputRoot = [System.IO.Path]::GetFullPath($outputRoot)
$project = Join-Path $repository 'Source\Mod.csproj'
$buildScript = Join-Path $toolingRoot 'tools\Invoke-RimWorldBuild.ps1'
$resultPath = Join-Path $outputRoot 'build-result.json'
$dependencies = if ($Configuration -eq '1.6')
{
    @('harmony', 'spine')
}
else
{
    @('harmony')
}

[System.IO.Directory]::CreateDirectory($outputRoot) | Out-Null

function Invoke-SharedBuild
{
    param([string[]]$DependencyIds)

    & powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass `
        -File $buildScript `
        -Project $project `
        -Configuration $Configuration `
        -Version $Configuration `
        -OutputRoot $outputRoot `
        -Engine MSBuild `
        -Dependency ($DependencyIds -join ',') `
        -ResultPath $resultPath | Out-Null

    if (-not (Test-Path -LiteralPath $resultPath -PathType Leaf))
    {
        throw "RimWorld-Tooling returned no build result for $Configuration."
    }

    return Get-Content -Raw -LiteralPath $resultPath | ConvertFrom-Json
}

$result = Invoke-SharedBuild -DependencyIds $dependencies

if (-not [bool]$result.Succeeded)
{
    throw "RimWorld $Configuration build failed with exit code $($result.ExitCode)."
}

$builtAssembly = Join-Path $outputRoot 'build\TaskInterrupt.dll'
if (-not (Test-Path -LiteralPath $builtAssembly -PathType Leaf))
{
    throw "TaskInterrupt.dll was not produced for RimWorld $Configuration."
}

$payloadRoot = Join-Path $repository "$Configuration\Assemblies"
$payloadAssembly = Join-Path $payloadRoot 'TaskInterrupt.dll'
[System.IO.Directory]::CreateDirectory($payloadRoot) | Out-Null
[System.IO.File]::Copy($builtAssembly, $payloadAssembly, $true)

Write-Output "Built and staged TaskInterrupt.dll for RimWorld $Configuration."
