# Varianta A pro Windows PowerShell — stejné jako expose-via-rpi.sh (OpenSSH je ve Windows 10/11 vestavěné).
# Použití:  .\expose-via-rpi.ps1 pi@smartaudit.nasedomena.cz
param(
    [Parameter(Mandatory = $true)][string]$Target,
    [int]$SshPort = 22,
    [int]$LocalWebPort = 3000,
    [int]$LocalApiPort = 8080,
    [int]$RemoteWebPort = 3000,
    [int]$RemoteApiPort = 8080
)

try {
    Invoke-WebRequest -Uri "http://localhost:$LocalWebPort/" -UseBasicParsing -TimeoutSec 5 | Out-Null
} catch {
    Write-Warning "Na http://localhost:$LocalWebPort nic neodpovídá — nejdřív: docker compose up --build"
}

Write-Host "Tunel ${Target}: RPi:$RemoteWebPort -> notebook:$LocalWebPort, RPi:$RemoteApiPort -> notebook:$LocalApiPort (Ctrl+C ukončí)"

while ($true) {
    ssh -N -p $SshPort `
        -o ExitOnForwardFailure=yes -o ServerAliveInterval=15 -o ServerAliveCountMax=3 `
        -R "127.0.0.1:${RemoteWebPort}:localhost:${LocalWebPort}" `
        -R "127.0.0.1:${RemoteApiPort}:localhost:${LocalApiPort}" `
        $Target
    Write-Host "Tunel spadl ($(Get-Date -Format HH:mm:ss)), znovu za 3 s..."
    Start-Sleep -Seconds 3
}
