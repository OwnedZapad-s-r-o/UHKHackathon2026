# ==============================================================================
# Sestavení a export celého docker compose SmartAuditu do jednoho .tar (pro docker load)
# Image: smartaudit-api, smartaudit-web, smartaudit-mqtt, smartaudit-nginx (+ postgis, cloudflared)
# Výstup: složka dist\smartaudit-docker připravená k přenosu na server / Raspberry Pi
# ==============================================================================
#
# Příklady:
#   .\export-docker-images.ps1
#   .\export-docker-images.ps1 -PublicUrl https://smartaudit.nasedomena.cz -Compress
#   .\export-docker-images.ps1 -Platform linux/arm64          # Raspberry Pi 4/5 (64bit OS)
#   .\export-docker-images.ps1 -SkipBuild                      # jen znovu uložit už sestavené image

[CmdletBinding()]
param (
    [string]$Tag = "latest",
    [string]$OutputDir = "dist\smartaudit-docker",
    [string]$ArchiveName = "smartaudit-stack.tar",
    # Veřejná doména pro QR kódy (NEXT_PUBLIC_* se vkládá do webu při buildu) a IRI katalogu DCAT-AP-CZ
    [string]$PublicUrl = "",
    # Cílová platforma image, např. linux/amd64 (VPS) nebo linux/arm64 (Raspberry Pi); prázdné = jako tento Docker
    [string]$Platform = "",
    [string]$PostgisImage = "postgis/postgis:17-3.5",
    [string]$CloudflaredImage = "cloudflare/cloudflared:latest",
    [switch]$Compress,
    [switch]$SkipBuild,
    [switch]$NoCloudflared,
    [switch]$IncludeEmulators
)

$ErrorActionPreference = "Stop"
$Root = $PSScriptRoot
$DeployDir = Join-Path $Root "deploy\docker"
$Utf8NoBom = New-Object System.Text.UTF8Encoding $false

function Step([string]$Text) { Write-Host "`n$Text" -ForegroundColor Yellow }
function Ok([string]$Text) { Write-Host "[OK] $Text" -ForegroundColor Green }
function Fail([string]$Text) { Write-Host "[CHYBA] $Text" -ForegroundColor Red; exit 1 }

# Docker píše průběh buildu na stderr — ve Windows PowerShellu 5.1 by to při "Stop" ukončilo skript.
# O úspěchu rozhoduje jen exit code.
function Invoke-Docker([string[]]$Arguments, [string]$Message) {
    $previous = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        & docker @Arguments 2>&1 | ForEach-Object { if ($_ -is [System.Management.Automation.ErrorRecord]) { $_.Exception.Message } else { $_ } } | Out-Host
        $code = $LASTEXITCODE
    } finally {
        $ErrorActionPreference = $previous
    }
    if ($code -ne 0) { Fail $Message }
}

function Test-Docker([string[]]$Arguments) {
    $previous = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try { & docker @Arguments *> $null; return ($LASTEXITCODE -eq 0) } finally { $ErrorActionPreference = $previous }
}

function New-Secret([int]$Length = 48) {
    $bytes = New-Object byte[] ($Length * 2)
    [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
    $text = [Convert]::ToBase64String($bytes) -replace '[^A-Za-z0-9]', ''
    return $text.Substring(0, $Length)
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " SmartAudit KHK - Docker Export Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 1. Kontrola Dockeru a cílové platformy
if (-not (Test-Docker @("info"))) { Fail "Docker neběží nebo není nainstalován v PATH!" }

if (-not $Platform) {
    $Platform = (docker version --format '{{.Server.Os}}/{{.Server.Arch}}').Trim()
}
Write-Host "Platforma image: $Platform · tag: $Tag" -ForegroundColor Gray
if ($PublicUrl) { Write-Host "Veřejná doména: $PublicUrl" -ForegroundColor Gray }

$own = [ordered]@{
    "smartaudit-api:$Tag"   = @{ Context = "backend";            Dockerfile = "backend\Dockerfile" }
    "smartaudit-web:$Tag"   = @{ Context = "frontend";           Dockerfile = "frontend\Dockerfile" }
    "smartaudit-mqtt:$Tag"  = @{ Context = "mqtt";               Dockerfile = "mqtt\Dockerfile" }
    "smartaudit-nginx:$Tag" = @{ Context = "deploy\docker\nginx"; Dockerfile = "deploy\docker\nginx\Dockerfile" }
}
if ($IncludeEmulators) {
    $own["smartaudit-emulators:$Tag"] = @{ Context = "tools\iqrf-emulator"; Dockerfile = "tools\iqrf-emulator\Dockerfile" }
}

$external = @($PostgisImage)
if (-not $NoCloudflared) { $external += $CloudflaredImage }

# 2. Sestavení vlastních image
if ($SkipBuild) {
    Step "[1/5] Sestavení přeskočeno (-SkipBuild) — používám existující image"
    foreach ($image in $own.Keys) {
        if (-not (Test-Docker @("image", "inspect", $image))) { Fail "Image '$image' neexistuje — spusťte skript bez -SkipBuild" }
    }
} else {
    Step "[1/5] Sestavuji image ($($own.Count)×)..."
    foreach ($image in $own.Keys) {
        $spec = $own[$image]
        Write-Host "  → $image" -ForegroundColor White
        $buildArgs = @("build", "--platform", $Platform, "-t", $image, "-f", (Join-Path $Root $spec.Dockerfile))
        if ($image -like "smartaudit-web:*") {
            # Rewrites /api/* míří na kontejner api; veřejná doména pro QR kódy
            $buildArgs += @("--build-arg", "API_INTERNAL_URL=http://api:8080", "--build-arg", "NEXT_PUBLIC_PUBLIC_URL=$PublicUrl")
        }
        $buildArgs += (Join-Path $Root $spec.Context)
        Invoke-Docker $buildArgs "Sestavení image '$image' selhalo!"
    }
    Ok "Vlastní image sestaveny."
}

# 3. Image třetích stran pro stejnou platformu
Step "[2/5] Stahuji image třetích stran..."
foreach ($image in $external) {
    Write-Host "  → $image" -ForegroundColor White
    Invoke-Docker @("pull", "--platform", $Platform, $image) "Stažení '$image' selhalo (existuje pro $Platform?)"
}
Ok "Image třetích stran připraveny."

# 4. Export všech image do jednoho .tar
$OutputPath = Join-Path $Root $OutputDir
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null
$ArchivePath = Join-Path $OutputPath $ArchiveName
Get-ChildItem -Path $OutputPath -Filter "$ArchiveName*" -ErrorAction SilentlyContinue | Remove-Item -Force

$allImages = @($own.Keys) + $external
Step "[3/5] Ukládám $($allImages.Count) image do '$ArchiveName' (docker save)..."
Invoke-Docker (@("save", "--platform", $Platform, "-o", $ArchivePath) + $allImages) "Export do '$ArchivePath' selhal!"

if ($Compress) {
    Step "[+ Extra] Komprimuji do '$ArchiveName.gz' (docker load umí .tar.gz přímo)..."
    $gzPath = "$ArchivePath.gz"
    $source = [System.IO.File]::OpenRead($ArchivePath)
    $target = [System.IO.File]::Create($gzPath)
    $gzip = New-Object System.IO.Compression.GZipStream($target, [System.IO.Compression.CompressionLevel]::Optimal)
    try { $source.CopyTo($gzip) } finally { $gzip.Dispose(); $target.Dispose(); $source.Dispose() }
    Remove-Item $ArchivePath -Force
    $ArchivePath = $gzPath
}

# 5. Compose, nginx logy, snímky otevřených dat, skripty a .env
Step "[4/5] Připravuji složku pro nasazení..."
Copy-Item (Join-Path $DeployDir "docker-compose.prod.yml") $OutputPath -Force
Copy-Item (Join-Path $DeployDir "load-and-run.sh") $OutputPath -Force
Copy-Item (Join-Path $DeployDir "load-and-run.ps1") $OutputPath -Force
Copy-Item (Join-Path $DeployDir "who-is-visiting.ps1") $OutputPath -Force
Copy-Item (Join-Path $DeployDir "README.md") (Join-Path $OutputPath "README.md") -Force
New-Item -ItemType Directory -Force -Path (Join-Path $OutputPath "logs\nginx") | Out-Null

$seedTarget = Join-Path $OutputPath "seed"
if (Test-Path $seedTarget) { Remove-Item $seedTarget -Recurse -Force }
New-Item -ItemType Directory -Force -Path $seedTarget | Out-Null
Copy-Item (Join-Path $Root "seed\opendata") $seedTarget -Recurse -Force

# Skripty pro Linux musí mít LF konce řádků
$shPath = Join-Path $OutputPath "load-and-run.sh"
[System.IO.File]::WriteAllText($shPath, ([System.IO.File]::ReadAllText($shPath) -replace "`r`n", "`n"), $Utf8NoBom)

# .env s náhodnými tajemstvími — jen poprvé, při dalším exportu zůstane (jinak by nesedělo heslo k existující databázi)
$envPath = Join-Path $OutputPath ".env"
if (-not (Test-Path $envPath)) {
    $archiveFile = Split-Path $ArchivePath -Leaf
    $envText = @"
# SmartAudit KHK — produkční prostředí (vygenerováno $(Get-Date -Format "yyyy-MM-dd HH:mm")). Soubor obsahuje tajemství, nesdílejte ho.
SMARTAUDIT_TAG=$Tag
SMARTAUDIT_ARCHIVE=$archiveFile
POSTGIS_IMAGE=$PostgisImage
CLOUDFLARED_IMAGE=$CloudflaredImage
ASPNETCORE_ENVIRONMENT=Production

# Veřejná adresa (QR kódy, katalog DCAT-AP-CZ) — např. https://smartaudit.nasedomena.cz
PUBLIC_URL=$PublicUrl

# Port nginxu na tomto stroji (na něj míří Cloudflare Tunnel); 127.0.0.1 = jen pro tunel na stejném stroji
HTTP_BIND=0.0.0.0
HTTP_PORT=8088

# Cloudflare Tunnel s vlastní doménou (Zero Trust → Networks → Tunnels → token); prázdné = bez tunelu
CLOUDFLARE_TUNNEL_TOKEN=

POSTGRES_PASSWORD=$(New-Secret 32)
JWT_SECRET=$(New-Secret 64)
MQTT_BACKEND_PASSWORD=$(New-Secret 32)
MQTT_DEVICE_SECRET=$(New-Secret 48)
MQTT_PROVISIONING_PASSWORD=$(New-Secret 24)
IQRF_BROKER_PUBLIC_HOST=
MQTT_TLS_PORT=8883

# AI analýza (nepovinné)
OPENAI_API_KEY=
OPENAI_MODEL=gpt-4o
AZURE_OPENAI_ENDPOINT=
AZURE_OPENAI_API_KEY=
AZURE_OPENAI_DEPLOYMENT=
"@
    [System.IO.File]::WriteAllText($envPath, ($envText -replace "`r`n", "`n"), $Utf8NoBom)
    Ok ".env vygenerován s náhodnými hesly"
} else {
    $envContent = [System.IO.File]::ReadAllText($envPath)
    $envContent = $envContent -replace "(?m)^SMARTAUDIT_ARCHIVE=.*$", "SMARTAUDIT_ARCHIVE=$(Split-Path $ArchivePath -Leaf)"
    if ($PublicUrl) { $envContent = $envContent -replace "(?m)^PUBLIC_URL=.*$", "PUBLIC_URL=$PublicUrl" }
    [System.IO.File]::WriteAllText($envPath, $envContent, $Utf8NoBom)
    Ok ".env už existuje — hesla ponechána"
}

# 6. Výsledek
$fileInfo = Get-Item $ArchivePath
$fileSizeMb = [math]::Round($fileInfo.Length / 1MB, 1)
$archiveFile = $fileInfo.Name

Step "[5/5] Hotovo!"
Write-Host "----------------------------------------" -ForegroundColor Gray
Write-Host "Složka:   " -NoNewline; Write-Host (Resolve-Path $OutputPath) -ForegroundColor Cyan
Write-Host "Archiv:   " -NoNewline; Write-Host "$archiveFile ($fileSizeMb MB, $Platform)" -ForegroundColor Cyan
Write-Host "Image:    " -NoNewline; Write-Host ($allImages -join ", ") -ForegroundColor Cyan
Write-Host "----------------------------------------" -ForegroundColor Gray

Write-Host "`nNasazení na cílovém stroji:" -ForegroundColor White
Write-Host "1. Přeneste celou složku (archiv, .env, docker-compose.prod.yml, seed/), např.:" -ForegroundColor Gray
Write-Host "   scp -r `"$OutputDir`" user@server:~/smartaudit" -ForegroundColor Green
Write-Host "2. Na serveru:" -ForegroundColor Gray
Write-Host "   cd ~/smartaudit && docker load -i $archiveFile" -ForegroundColor Green
Write-Host "   docker compose -f docker-compose.prod.yml up -d" -ForegroundColor Green
Write-Host "   (nebo vše najednou: ./load-and-run.sh  /  .\load-and-run.ps1)" -ForegroundColor Gray
Write-Host "3. Cloudflare Tunnel → služba http://nginx:80 (token do .env, pak --profile tunnel)" -ForegroundColor Gray
Write-Host "   nebo rychle bez účtu: docker compose -f docker-compose.prod.yml --profile quicktunnel up -d" -ForegroundColor Gray
Write-Host "4. Kdo přistupuje na web:" -ForegroundColor Gray
Write-Host "   docker compose -f docker-compose.prod.yml logs -f nginx" -ForegroundColor Green
Write-Host "   .\who-is-visiting.ps1   (souhrn z logs/nginx/access.json.log)" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
