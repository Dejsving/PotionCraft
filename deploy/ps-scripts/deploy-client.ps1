param(
    [string]$Configuration = 'Release',
    [string]$Runtime = 'linux-x64',
    [string]$ConfigPath = (Join-Path $PSScriptRoot 'deploy.config.psd1')
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-NativeCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,
        [Parameter(Mandatory = $true)]
        [string]$StepName
    )

    Write-Host "==> $StepName"
    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    $output = & $FilePath @Arguments 2>&1
    $ErrorActionPreference = $previousErrorActionPreference
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
$sshTarget = "{0}@{1}" -f $config.User, $config.Host
$remoteTmpDir = $config.RemoteTmpDir.TrimEnd('/')

$projectPath = Join-Path $repoRoot 'PotionCraft.Client\PotionCraft.Client.csproj'
$publishDir = Join-Path $repoRoot 'artifacts\publish\client'
$archivePath = Join-Path $repoRoot 'artifacts\potioncraft-client-linux-x64.tar.gz'
$servicePath = Join-Path $repoRoot 'deploy\service-files\potioncraft-client.service'
$remoteScriptLocalPath = Join-Path $repoRoot 'deploy\sh-scripts\potioncraft-client-remote-deploy.sh'

$remoteArchivePath = "$remoteTmpDir/potioncraft-client-linux-x64.tar.gz"
$remoteServicePath = "$remoteTmpDir/potioncraft-client.service"
$remoteScriptPath = "$remoteTmpDir/potioncraft-client-remote-deploy.sh"

if (Test-Path $publishDir) {
    Remove-Item -Path $publishDir -Recurse -Force
}
New-Item -Path $publishDir -ItemType Directory -Force | Out-Null

Invoke-NativeCommand -FilePath 'dotnet' -Arguments @(
    'publish',
    $projectPath,
    '--configuration', $Configuration,
    '--runtime', $Runtime,
    '--self-contained', 'true',
    '-o', $publishDir
) -StepName 'Publish client'

if (Test-Path $archivePath) {
    Remove-Item -Path $archivePath -Force
}
Invoke-NativeCommand -FilePath 'tar' -Arguments @(
    '-czf',
    $archivePath,
    '-C', $publishDir,
    '.'
) -StepName 'Pack client tar.gz'

$scpCommonArgs = @()
if ($config.ContainsKey('SshOptions') -and $config.SshOptions.Count -gt 0) {
    $scpCommonArgs += $config.SshOptions
}
$scpCommonArgs += @('-i', $config.SshKeyPath)

Invoke-NativeCommand -FilePath 'scp' -Arguments ($scpCommonArgs + @(
    $archivePath,
    "$sshTarget`:$remoteArchivePath"
)) -StepName 'Upload client archive'

Invoke-NativeCommand -FilePath 'scp' -Arguments ($scpCommonArgs + @(
    $servicePath,
    "$sshTarget`:$remoteServicePath"
)) -StepName 'Upload client service file'

Invoke-NativeCommand -FilePath 'scp' -Arguments ($scpCommonArgs + @(
    $remoteScriptLocalPath,
    "$sshTarget`:$remoteScriptPath"
)) -StepName 'Upload client deploy script'

$sshArgs = @()
if ($config.ContainsKey('SshOptions') -and $config.SshOptions.Count -gt 0) {
    $sshArgs += $config.SshOptions
}
$sshArgs += @('-i', $config.SshKeyPath, $sshTarget, "bash '$remoteScriptPath'")

Invoke-NativeCommand -FilePath 'ssh' -Arguments $sshArgs -StepName 'Run remote client deploy script'

Write-Host 'Client deploy completed successfully.'
