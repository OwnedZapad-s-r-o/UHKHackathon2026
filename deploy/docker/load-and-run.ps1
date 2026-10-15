# Na cílovém stroji (Windows): načte image z archivu a spustí stack.
#   .\load-and-run.ps1            # bez tunelu, nebo s tunelem, když je v .env CLOUDFLARE_TUNNEL_TOKEN
#   .\load-and-run.ps1 -Quick     # + dočasná adresa *.trycloudflare.com
param([switch]$Quick)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

$envLines = Get-Content .env
$archive = ($envLines | Where-Object { $_ -match '^SMARTAUDIT_ARCHIVE=' }) -replace '^SMARTAUDIT_ARCHIVE=', ''
if (-not $archive -or -not (Test-Path $archive)) {
    $archive = (Get-ChildItem -Filter "smartaudit-stack.tar*" | Select-Object -First 1).Name
}
if (-not $archive) { Write-Host "[CHYBA] Archiv s image nenalezen" -ForegroundColor Red; exit 1 }

Write-Host "docker load -i $archive" -ForegroundColor Yellow
docker load -i $archive
if ($LASTEXITCODE -ne 0) { exit 1 }

New-Item -ItemType Directory -Force -Path "logs\nginx" | Out-Null
$profiles = @()
if ($envLines | Where-Object { $_ -match '^CLOUDFLARE_TUNNEL_TOKEN=.+' }) { $profiles += @("--profile", "tunnel") }
if ($Quick) { $profiles += @("--profile", "quicktunnel") }

docker compose -f docker-compose.prod.yml @profiles up -d
if ($LASTEXITCODE -ne 0) { exit 1 }

$port = ($envLines | Where-Object { $_ -match '^HTTP_PORT=' }) -replace '^HTTP_PORT=', ''
if (-not $port) { $port = "8088" }
Write-Host "`nBezi: http://localhost:$port  (prvni start importuje otevrena data ~1-2 min)" -ForegroundColor Green
Write-Host "Navstevnici: docker compose -f docker-compose.prod.yml logs -f nginx" -ForegroundColor Gray
Write-Host "Souhrn:      .\who-is-visiting.ps1" -ForegroundColor Gray

if ($Quick) {
    Start-Sleep -Seconds 8
    $url = docker compose -f docker-compose.prod.yml logs cloudflared-quick 2>&1 | Select-String -Pattern 'https://[a-z0-9-]+\.trycloudflare\.com' | ForEach-Object { $_.Matches[0].Value } | Select-Object -Last 1
    Write-Host "Verejna adresa: $url" -ForegroundColor Cyan
}
