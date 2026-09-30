param(
    [string]$ConfigPath = (Join-Path $PSScriptRoot 'deploy.config.psd1')
)

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-NativeCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,
        [Parameter(Mandatory = $true)]
        [string]$StepName,
        [string]$WorkingDirectory
    )

    Write-Host "==> $StepName"
    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    if ($WorkingDirectory) {
        Push-Location $WorkingDirectory
    }
    try {
        $output = & $FilePath @Arguments 2>&1
    }
    finally {
        if ($WorkingDirectory) {
            Pop-Location
        }
        $ErrorActionPreference = $previousErrorActionPreference
    }
    if ($output) {
        $output | ForEach-Object { Write-Host $_ }
    }

    if ($LASTEXITCODE -ne 0) {
        throw "$StepName failed. Exit code: $LASTEXITCODE"
    }
}

if (-not (Test-Path $ConfigPath)) {
    throw "Config file not found: $ConfigPath"
}

$config = Import-PowerShellDataFile -Path $ConfigPath
foreach ($requiredKey in @('Host', 'User', 'SshKeyPath', 'RemoteTmpDir')) {
    if (-not $config.ContainsKey($requiredKey) -or [string]::IsNullOrWhiteSpace([string]$config[$requiredKey])) {
        throw "Missing required config key: $requiredKey"
    }
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$webDir = Join-Path $repoRoot 'PotionCraft.Web'
$distDir = Join-Path $webDir 'dist'
$archivePath = Join-Path $repoRoot 'artifacts\potioncraft-web-dist.tar.gz'
$servicePath = Join-Path $repoRoot 'deploy\service-files\potioncraft-web.service'
$remoteScriptLocalPath = Join-Path $repoRoot 'deploy\sh-scripts\potioncraft-web-remote-deploy.sh'
$npmPath = (Get-Command 'npm' -ErrorAction Stop).Source

$sshTarget = "{0}@{1}" -f $config.User, $config.Host
$remoteTmpDir = $config.RemoteTmpDir.TrimEnd('/')
$remoteArchivePath = "$remoteTmpDir/potioncraft-web-dist.tar.gz"
$remoteServicePath = "$remoteTmpDir/potioncraft-web.service"
$remoteScriptPath = "$remoteTmpDir/potioncraft-web-remote-deploy.sh"

if (Test-Path $distDir) {
    Remove-Item -Path $distDir -Recurse -Force
}
Invoke-NativeCommand -FilePath $npmPath -Arguments @('run', 'build') -StepName 'Build web' -WorkingDirectory $webDir

New-Item -Path (Split-Path $archivePath -Parent) -ItemType Directory -Force | Out-Null
if (Test-Path $archivePath) {
    Remove-Item -Path $archivePath -Force
}
Invoke-NativeCommand -FilePath 'tar' -Arguments @(
    '-czf', $archivePath,
    '-C', $webDir,
    'dist'
) -StepName 'Pack web dist'

$scpCommonArgs = @()
if ($config.ContainsKey('SshOptions') -and $config.SshOptions.Count -gt 0) {
    $scpCommonArgs += $config.SshOptions
}
$scpCommonArgs += @('-i', $config.SshKeyPath)

Invoke-NativeCommand -FilePath 'scp' -Arguments ($scpCommonArgs + @(
    $archivePath,
    "$sshTarget`:$remoteArchivePath"
)) -StepName 'Upload web archive'

Invoke-NativeCommand -FilePath 'scp' -Arguments ($scpCommonArgs + @(
    $servicePath,
    "$sshTarget`:$remoteServicePath"
)) -StepName 'Upload web service file'

Invoke-NativeCommand -FilePath 'scp' -Arguments ($scpCommonArgs + @(
    $remoteScriptLocalPath,
    "$sshTarget`:$remoteScriptPath"
)) -StepName 'Upload web deploy script'

$sshArgs = @()
if ($config.ContainsKey('SshOptions') -and $config.SshOptions.Count -gt 0) {
    $sshArgs += $config.SshOptions
}
$sshArgs += @('-i', $config.SshKeyPath, $sshTarget, "bash '$remoteScriptPath'")

Invoke-NativeCommand -FilePath 'ssh' -Arguments $sshArgs -StepName 'Run remote web deploy script'
Write-Host 'Web deploy completed successfully.'