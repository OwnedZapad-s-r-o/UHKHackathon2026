# SmartAudit

Aplikace pro přehled budov, předběžné energetické vyhodnocení a senzorovou telemetrii IQRF.

## Spuštění

Požadavky: Docker a Docker Compose.

```powershell
Copy-Item .env.example .env
# V .env nastavte vlastní hesla a tajemství.
docker compose up -d --build
```

Web běží na http://localhost:3000. Otevřené API je na /api/open/v1, jeho dokumentace na /api/open/docs/.
První start provede migrace a připraví data. Snímky otevřených dat v seed/opendata slouží jako záloha při nedostupnosti poskytovatele.

Telemetrie bez připojeného zařízení je modelovaná. Virtuální zařízení slouží k demonstraci integrační cesty; jejich hodnoty nejsou měřením fyzického objektu. Energetické a finanční výsledky jsou odhady závislé na vstupních datech a předpokladech.

## Struktura

- backend: ASP.NET Core API, doménové výpočty, migrace a testy.
- frontend: Next.js, uživatelské rozhraní a statické prostředky aplikace.
- mqtt: konfigurace brokeru a připojení gatewayí.
- seed/opendata: snímky zdrojových otevřených dat.
- tools/iqrf-emulator: emulátory pro testování senzorové integrace.
- deploy: konfigurace a skripty nasazení.

Texty v backend/SmartAudit.Api/Resources/Guide jsou provozní znalostní báze aplikace.

## Testy

```powershell
dotnet test backend/SmartAudit.Tests/SmartAudit.Tests.csproj
```

## Export pro nasazení

```powershell
.\export-docker-images.ps1 -Compress
```

Výstup je v dist/smartaudit-docker. Soubor .env a exportované archivy nepatří do Gitu.
