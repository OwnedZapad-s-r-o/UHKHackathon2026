# Kdo přistupuje na web — souhrn z JSON logu nginxu (logs/nginx/access.json.log).
#   .\who-is-visiting.ps1                 # posledních 24 h
#   .\who-is-visiting.ps1 -Hours 1 -Top 20
#   .\who-is-visiting.ps1 -Follow         # živě, řádek po řádku
param(
    [int]$Hours = 24,
    [int]$Top = 10,
    [switch]$Follow,
    [string]$LogFile = (Join-Path $PSScriptRoot "logs\nginx\access.json.log")
)

if (-not (Test-Path $LogFile)) { Write-Host "Log $LogFile zatím neexistuje — nikdo ještě nepřišel?" -ForegroundColor Yellow; exit 0 }

function Format-Visit($v) {
    $color = if ($v.status -ge 500) { "Red" } elseif ($v.status -ge 400) { "Yellow" } else { "Gray" }
    Write-Host ("{0}  {1,-39} {2,-3} {3} {4,-50} {5}  {6}" -f ([datetime]$v.time).ToString("HH:mm:ss"), $v.ip, $v.country, $v.method, $v.path, $v.status, $v.userAgent) -ForegroundColor $color
}

if ($Follow) {
    Get-Content $LogFile -Wait -Tail 20 | ForEach-Object { try { Format-Visit ($_ | ConvertFrom-Json) } catch { } }
    exit 0
}

$since = (Get-Date).AddHours(-$Hours)
$visits = Get-Content $LogFile | ForEach-Object { try { $_ | ConvertFrom-Json } catch { } } | Where-Object { $_ -and ([datetime]$_.time) -ge $since }

Write-Host "Za posledních $Hours h: $($visits.Count) požadavků od $(@($visits | Select-Object -ExpandProperty ip -Unique).Count) návštěvníků" -ForegroundColor Cyan

Write-Host "`nNávštěvníci (IP · země · požadavků · poslední prohlížeč):" -ForegroundColor White
$visits | Group-Object ip | Sort-Object Count -Descending | Select-Object -First $Top | ForEach-Object {
    $last = $_.Group | Select-Object -Last 1
    "{0,-39} {1,-3} {2,6}×  {3}" -f $_.Name, $last.country, $_.Count, $last.userAgent
}

Write-Host "`nZemě:" -ForegroundColor White
$visits | Group-Object country | Sort-Object Count -Descending | Select-Object -First $Top | ForEach-Object { "{0,-4} {1,6}×" -f ($(if ($_.Name) { $_.Name } else { "?" })), $_.Count }

Write-Host "`nNejčastější stránky:" -ForegroundColor White
$visits | Where-Object { $_.path -notlike "/api/*" } | Group-Object { ($_.path -split '\?')[0] } | Sort-Object Count -Descending | Select-Object -First $Top | ForEach-Object { "{0,6}×  {1}" -f $_.Count, $_.Name }

Write-Host "`nOtevřené API a QR karty:" -ForegroundColor White
$visits | Where-Object { $_.path -like "/api/open/*" -or $_.path -like "/objekty/*" } | Group-Object { ($_.path -split '\?')[0] } | Sort-Object Count -Descending | Select-Object -First $Top | ForEach-Object { "{0,6}×  {1}" -f $_.Count, $_.Name }

$errors = @($visits | Where-Object { $_.status -ge 400 })
if ($errors.Count -gt 0) {
    Write-Host "`nChyby (4xx/5xx): $($errors.Count)" -ForegroundColor Yellow
    $errors | Group-Object status | ForEach-Object { "  {0}: {1}×" -f $_.Name, $_.Count }
}
