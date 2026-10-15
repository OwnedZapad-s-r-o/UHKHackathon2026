# 🏢 SmartAudit KHK

> **Energetický audit a inteligentní monitoring budov Královéhradeckého kraje.**  
> Aplikace propojující 25+ sad otevřených dat (Data KHK), katastr nemovitostí (ČÚZK INSPIRE WFS), meteorologická data (Open-Meteo), AI analýzy (Azure OpenAI) a senzorovou telemetrii.

---

## 📸 Ukázky rozhraní (Screenshots)

### 🗺️ 2D a 3D Mapa objektů kraje
*Celkový 2D pohled na mapu s budovami zřizovanými Královéhradeckým krajem (školy, domovy pro seniory, nemocnice, kulturní objekty):*

![2D Mapa objektů kraje](docs/images/02-mapa-2d-prehled.png)

*3D náhled na vybranou budovu (Střední uměleckoprůmyslová škola hudebních nástrojů a nábytku u UHK v Hradci Králové) s 3D extruzí půdorysu z ČÚZK a okolní zástavbou:*

![3D model budovy na mapě](docs/images/03-mapa-3d-budova.png)

---

### 📋 Inspekční panel & Telemetrie
*Živá telemetrie vybraného objektu (teplota, CO₂, příkon) s grafy průběhu za posledních 48 hodin:*

![Inspekční panel - Telemetrie](docs/images/04-inspekce-telemetrie.png)

---

### 🏢 Karta budovy ČÚZK & Schematický řez budovou
*Oficiální karta budovy s katastrálními daty z ČÚZK INSPIRE WFS a schematický řez budovou po podlažích a zónách s alokací senzorů a detekcí překročení limitů CO₂:*

![Karta budovy ČÚZK a Schematický řez](docs/images/05-inspekce-budova-rez-cuzk.png)

---

### 🤖 AI Analýza & Doporučení opatření (Azure OpenAI)
*Automatické vyhodnocení úsporných opatření s propočtem návratnosti v Kč a generováním podkladů pro správce:*

![AI Analýza a Audit](docs/images/06-inspekce-audit-ai.png)

---

### 📊 Portfolio a Anomálie
*Přehledové portfolio budov kraje s možností filtrace podle okresů a typů zřízení:*
![Portfolio objektů](docs/images/07-portfolio.png)

*Seznam detekovaných anomálií a plýtvání energií seřazený dle výše finanční ztráty:*
![Anomálie a energetické ztráty](docs/images/08-anomalie.png)

---

### 📄 PDF Reporty, Otevřené API & Zdroje Dat
*Exporty kompletních energetických auditů do PDF:*
![PDF Reporty](docs/images/09-reporty.png)

*Veřejné otevřené API (Scalar / OpenAPI documentation, CORS, bez klíče):*
![Otevřené API](docs/images/10-otevrena-data.png)

*Katalog využitých 25+ sad otevřených dat Královéhradeckého kraje:*
![Zdroje otevřených dat](docs/images/11-zdroje-dat.png)

---

## 🚀 Rychlé spuštění (Docker)

```bash
docker compose up --build
```

### Přístupné služby:

| Služba | URL | Popis |
|---|---|---|
| **Web UI (Next.js)** | [http://localhost:3000](http://localhost:3000) | Uživatelské rozhraní aplikace |
| **API Backend (ASP.NET)** | [http://localhost:8080](http://localhost:8080) | REST API backend |
| **Otevřené API Docs** | [http://localhost:3000/api/open/docs/](http://localhost:3000/api/open/docs/) | Interaktivní OpenAPI (Scalar) |
| **PostgreSQL / PostGIS** | `localhost:5432` | Databáze (`smartaudit` / `smartaudit`) |
| **MQTT broker (Mosquitto)** | `localhost:1883` | Připojení IQRF gateway (skutečná čidla) |

> ℹ️ **První spuštění:** Při prvním startu API automaticky stáhne otevřená data, sestaví portfolio objektů, dotáhne katastrální karty z ČÚZK, počasí z Open-Meteo a dopočítá 7 dní telemetrie. Trvá to zhruba 1 minutu, stav je vidět na `GET /health`.

Na přihlašovací stránce (`/login`) jsou tlačítka **Demo** pro tři role: **Kraj**, **Správce budovy** a **Veřejnost**.

Bez internetu se data načtou z lokálních snapshotů v `seed/opendata/`. Vlastní hesla a JWT tajemství lze nastavit v `.env` (vzor v `.env.example`).

---

## ✨ Hlavní Funkce

- **📡 Skutečná čidla IQRF:** IQRF gateway se přes MQTT připojí k aplikaci, ta sama najde uzly a senzory (teplota, CO₂, vlhkost, osvětlení, hluk, pohyb…) a po přiřazení do místnosti nahradí simulaci skutečným měřením. Návod v [docs/SENZORY_IQRF.md](docs/SENZORY_IQRF.md), emulátor gatewaye v `tools/iqrf-emulator/`.
- **🗺️ Mapa objektů kraje:** Šedý podklad (mapcn), okresy obarvené podle podílu objektů se zjištěním (klik vybere okres), piny podle stavu a druhu budovy a vyhledávání. Na startu není vybraný žádný objekt; po kliknutí mapa přeletí k budově a ukáže ji ve 3D (vybraná budova „vyroste“ v barvě stavu, okolí zprůhlední). Výběr zruší křížek, klik do mapy nebo Esc.
- **📋 Inspekční panel:** Přehled, telemetrie (živé grafy), auditní zjištění, karta budovy ČÚZK a unikátní schematický řez budovou podle zón.
- **⚡ Portfolio a Anomálie:** Přehledná tabulka a seznam zjištění seřazený podle odhadované roční finanční ztráty v Kč.
- **📄 PDF reporty:** Audit jednoho objektu (s grafy a lokalizační mapkou) i souhrnný přehled portfolia s mapou okresů.
- **🤖 AI analýza (Azure OpenAI):** Navrhne konkrétní opatření s vyčíslenou návratností a dopis správci. Výsledek se propisuje i do PDF auditního protokolu.
- **🌐 Otevřené API (`/api/open/v1`):** JSON, GeoJSON a CSV exporty pro vývojáře bez nutnosti přihlášení a API klíčů (licence CC BY 4.0).

---

## ⚙️ Zapnutí AI analýzy (Azure OpenAI)

Do `.env` doplňte přístupové údaje z Azure OpenAI a restartujte API:

```env
AZURE_OPENAI_ENDPOINT=https://<nazev-zdroje>.openai.azure.com/
AZURE_OPENAI_API_KEY=<klíč>
AZURE_OPENAI_DEPLOYMENT=<název nasazení, např. gpt-4o>
```

Bez nich aplikace běží normálně a karta AI analýzy informuje, jaké proměnné doplnit.

---

## 🏗️ Architektura a Struktura Repozitáře

```
web (Next.js :3000) ──/api/*──► api (ASP.NET net10 :8080) ──► db (PostgreSQL + PostGIS)
                                  ├─ DataPipelineService      stažení sad → portfolio → ČÚZK → počasí
                                  └─ SensorSimulationService  senzory → historie + živá data → audit
```

| Složka | Obsah |
|---|---|
| `backend/` | ASP.NET Core API (`SmartAudit.Api`) a doménová logika s EF Core (`SmartAudit.Core`) |
| `frontend/` | Next.js App Router (TypeScript, Tailwind CSS, shadcn/ui, MapLibre GL) |
| `seed/opendata/` | Snapshoty otevřených dat pro offline start |
| `docs/` | Dokumentace (`SMARTAUDIT_FUNKCE.md`) a obrázky (`docs/images/`) |
| `research/` | Rešerše 165 sad Data KHK, analýzy v Pythonu a datové manifesty |

---

## 💻 Lokální vývoj bez Dockeru

```bash
docker compose up -d db
dotnet run --project backend/SmartAudit.Api --launch-profile http
npm --prefix frontend run dev
```

Frontend čeká API na `http://localhost:5033` (proměnná `API_INTERNAL_URL`).

Nová migrace databáze:

```bash
dotnet ef migrations add <Nazev> --project backend/SmartAudit.Core --startup-project backend/SmartAudit.Api
```

---

## 📡 Simulace telemetrie

Senzory jsou generovány deterministickým modelem `SensorSimulator`. Vstupy bere z reálné venkovní teploty (Open-Meteo), stáří a plochy budovy (ČÚZK), počtu žáků/klientů (Data KHK) a kalendáře svátků. Zhruba 30 % objektů má simulační profil s problémem (víkendové přetápění, špatné větrání, nefunkční noční útlum). Audit (`FacilityStatusEvaluator`) tyto problémy detekuje pravidly nad naměřenými daty.

Jakmile místnost měří skutečné čidlo IQRF, simulace pro ni skončí a audit počítá se skutečnými hodnotami — viz [docs/SENZORY_IQRF.md](docs/SENZORY_IQRF.md).
