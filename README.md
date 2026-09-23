# SmartAudit KHK

Energetický audit budov, které zřizuje Královéhradecký kraj: školy, domovy pro seniory,
nemocnice a kulturní zařízení. Aplikace stojí na otevřených datech kraje (25 sad z Data KHK),
ČÚZK a Open-Meteo. Telemetrie ze senzorů je zatím simulovaná.

## Spuštění

```bash
docker compose up --build
```

| Služba | Adresa |
|---|---|
| Web (Next.js) | http://localhost:3000 |
| API (ASP.NET) | http://localhost:8080, dokumentace http://localhost:8080/scalar |
| Databáze (PostGIS) | localhost:5432, `smartaudit` / `smartaudit` |

Při prvním startu API samo stáhne otevřená data, sestaví portfolio objektů, dotáhne karty
budov z ČÚZK a počasí z Open-Meteo a dopočítá 7 dní telemetrie. Trvá to zhruba minutu,
stav je vidět na `GET /health`. Na přihlašovací stránce jsou tlačítka **Demo** pro tři role
(kraj, správce budovy, veřejnost).

Bez internetu se data načtou ze snapshotů v `seed/opendata/`, které se přepisují po každém
úspěšném stažení.

Vlastní hesla a JWT tajemství nastavíte v souboru `.env`, vzor je v `.env.example`.

## Funkce

- **Mapa objektů kraje** — šedý podklad, piny podle stavu a druhu, filtr okresu a hledání. Po kliknutí na objekt
  mapa přeletí k budově a ukáže ji ve 3D: okolní zástavba podle výšek z OpenStreetMap (dlaždice CARTO),
  vybraná budova z půdorysu ČÚZK s odhadem výšky podle podlaží. Přepínač 2D / 3D je v ovládání mapy.
- **Inspekční panel** — přehled, telemetrie (grafy), audit, karta budovy a řez budovou; lze zavřít.
- **Portfolio a Anomálie** — tabulka a seznam zjištění seřazené podle odhadované ztráty.
- **PDF reporty** — audit jednoho objektu (s grafy) a přehled portfolia; tlačítka v panelu a na obrazovkách
  Portfolio, Anomálie a Reporty.
- **AI analýza** — Azure OpenAI navrhne opatření s návratností a dopis správci; výsledek je i v PDF.

### Zapnutí AI analýzy (Azure OpenAI)

Do `.env` doplňte tři hodnoty z Azure (prostředek Azure OpenAI → *Keys and Endpoint*, název nasazení
modelu z Azure AI Foundry → *Deployments*) a restartujte API:

```bash
AZURE_OPENAI_ENDPOINT=https://<nazev-zdroje>.openai.azure.com/
AZURE_OPENAI_API_KEY=<klíč>
AZURE_OPENAI_DEPLOYMENT=<název nasazení, např. gpt-4o>
```

Bez nich aplikace běží normálně a karta AI analýzy jen ukazuje, co doplnit. AI analýzu vidí kraj a správce
budovy, veřejnost ne (obsahuje částky v Kč).

## Struktura repozitáře

| Složka | Obsah |
|---|---|
| `backend/` | ASP.NET API (`SmartAudit.Api`) a doménová vrstva s EF Core (`SmartAudit.Core`) |
| `frontend/` | Next.js aplikace |
| `seed/opendata/` | Snapshoty otevřených dat pro start bez internetu |
| `docs/` | Katalog funkcí SmartAuditu (`SMARTAUDIT_FUNKCE.md`) |
| `research/kontext/` | Rešerše všech 165 sad Data KHK, minulých ročníků a nápadů, včetně skriptů, které ji vytvořily |
| `research/katalog/` | Manifest a schémata datových sad (skripty v TypeScriptu) |
| `research/analyza/` | Průzkumné analýzy v Pythonu |
| `research/prototypy/` | Dřívější HTML prototypy nad otevřenými daty |

Skripty v `research/` se spouštějí z kořene repozitáře, např. `python research/kontext/probe.py "Nemocnice"`.

## Architektura

```
web (Next.js :3000) ──/api/*──► api (ASP.NET net10 :8080) ──► db (PostgreSQL + PostGIS)
                                  ├─ DataPipelineService      stažení sad → portfolio → ČÚZK → počasí
                                  └─ SensorSimulationService  senzory → historie + živá data → audit
```

- `backend/SmartAudit.Core` — entity, EF konfigurace, migrace, doménové výpočty
  (`SensorSimulator`, `FacilityStatusEvaluator`, `ZoneLayout`, `SchoolCalendar`).
- `backend/SmartAudit.Api` — vertical slices (`Features/<Oblast>/<Akce>`), controllery, služby
  pro import (`Services/Import`), simulaci (`Services/Simulation`) a audit (`Services/Audit`).
- `frontend` — Next.js App Router, shadcn/ui, mapa z [mapcn](https://mapcn.dev) se šedým
  podkladem (`npm run map:gray` ho znovu vygeneruje do `public/map-styles/`).

## Lokální vývoj bez Dockeru

```bash
docker compose up -d db
dotnet run --project backend/SmartAudit.Api --launch-profile http
npm --prefix frontend run dev
```

Frontend čeká API na `http://localhost:5033` (proměnná `API_INTERNAL_URL`).

Nová migrace:

```bash
dotnet ef migrations add <Nazev> --project backend/SmartAudit.Core --startup-project backend/SmartAudit.Api
```

## Simulace telemetrie

Senzory zatím nejsou reálné. `SensorSimulator` je deterministický model: stejný objekt dává po
restartu stejná data. Vstupy bere z reálných dat, konkrétně z venkovní teploty (Open-Meteo),
stáří a plochy budovy (ČÚZK), počtu žáků (Data KHK) a kalendáře svátků. Zhruba 30 % objektů
dostane profil s problémem: víkendové přetápění, špatné větrání, nefunkční noční útlum nebo
příliš brzký ranní náběh. Audit (`FacilityStatusEvaluator`) pak tyto problémy hledá čitelnými
pravidly jen z naměřených dat. Reálný zdroj (IQRF/MQTT) se připojí implementací
`ISensorDataSource`.
