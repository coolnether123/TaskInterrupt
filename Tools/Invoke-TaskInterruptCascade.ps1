param(
    [string]$OutputPath,
    [string]$PlanPath,
    [switch]$Execute,
    [string]$JournalPath,
    [string]$ConfirmPlanSha256,
    [switch]$AllowPush
)

$ErrorActionPreference = 'Stop'
$repository = [System.IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$toolingRoot = [Environment]::GetEnvironmentVariable('RWT_CASCADE_TOOLING_ROOT')
if ([string]::IsNullOrWhiteSpace($toolingRoot))
{
    $toolingRoot = 'A:\Dev\RimWorld\Worktrees\RimWorld-Tooling\phase-a'
}
$generic = Join-Path $toolingRoot 'tools\Invoke-RimWorldCascade.ps1'
$manifest = Join-Path $repository 'Tools\CascadeManifest.json'

$arguments = @(
    '-Manifest', $manifest,
    '-Repository', $repository)
if (-not [string]::IsNullOrWhiteSpace($OutputPath))
{
    $arguments += @('-OutputPath', $OutputPath)
}
if ($Execute)
{
    $arguments += @('-Execute')
    if ([string]::IsNullOrWhiteSpace($JournalPath) -or
        [string]::IsNullOrWhiteSpace($ConfirmPlanSha256))
    {
        throw 'Execute requires JournalPath and ConfirmPlanSha256.'
    }
    $arguments += @(
        '-JournalPath', $JournalPath,
        '-ConfirmPlanSha256', $ConfirmPlanSha256)
    if ($AllowPush)
    {
        $arguments += '-AllowPush'
    }
}
if (-not [string]::IsNullOrWhiteSpace($PlanPath))
{
    $arguments += @('-PlanPath', $PlanPath)
}

& powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass `
    -File $generic @arguments
if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}
