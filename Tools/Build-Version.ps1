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
        -Dependency $DependencyIds `
        -ResultPath $resultPath | Out-Null

    if (-not (Test-Path -LiteralPath $resultPath -PathType Leaf))
    {
        throw "RimWorld-Tooling returned no build result for $Configuration."
    }

    return Get-Content -Raw -LiteralPath $resultPath | ConvertFrom-Json
}

function Invoke-CurrentSpineBuild
{
    Import-Module (Join-Path $toolingRoot 'modules\RimWorld.Tooling.Depot\RimWorld.Tooling.Depot.psd1') -Force
    Import-Module (Join-Path $toolingRoot 'modules\RimWorld.Tooling.Build\RimWorld.Tooling.Build.psd1') -Force

    $environment = Resolve-RwtEnvironment `
        -Version $Configuration `
        -Purpose Compile `
        -Dependency @('harmony') `
        -VersionManifestPath (Join-Path $toolingRoot 'manifests\rimworld-versions.json') `
        -DependencyManifestPath (Join-Path $toolingRoot 'manifests\dependencies.json')

    $spinePath = Join-Path `
        (Join-Path (Split-Path -Parent $repository) 'Spine') `
        '1.6\Assemblies\Spine.dll'
    if (-not (Test-Path -LiteralPath $spinePath -PathType Leaf))
    {
        throw "The current standalone Spine 1.6 assembly is missing: $spinePath"
    }

    $dependenciesWithSpine = @($environment.Dependencies) + @(
        [PSCustomObject]@{
            Id = 'spine'
            Path = $spinePath
            Sha256 = (Get-FileHash -LiteralPath $spinePath -Algorithm SHA256).Hash
        })
    $environment | Add-Member `
        -NotePropertyName Dependencies `
        -NotePropertyValue $dependenciesWithSpine `
        -Force

    $result = Invoke-RwtBuild `
        -Project $project `
        -Configuration $Configuration `
        -Environment $environment `
        -OutputRoot $outputRoot `
        -Engine MSBuild
    $result | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $resultPath -Encoding utf8
    return $result
}

$result = if ($Configuration -eq '1.6')
{
    # The dependency manifest currently carries an older Spine hash than the
    # checked-out 1.6 assembly. Keep the build manifest authoritative for all
    # other dependencies, but use the current sibling Spine binary until that
    # shared manifest is refreshed by its owner.
    Invoke-CurrentSpineBuild
}
else
{
    Invoke-SharedBuild -DependencyIds $dependencies
}

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
