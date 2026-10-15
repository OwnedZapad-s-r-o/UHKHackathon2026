# SmartAudit KHK — nasazení z exportu (docker load + Cloudflare Tunnel)

Složka obsahuje vše, co cílový stroj potřebuje — nic se tu nesestavuje:

| Soubor | K čemu |
|---|---|
| `smartaudit-stack.tar(.gz)` | všechny image: api, web, mqtt, nginx, postgis, cloudflared |
| `docker-compose.prod.yml` | stack z image; ven vede jen nginx (port `HTTP_PORT`, výchozí 8088) |
| `.env` | náhodně vygenerovaná hesla, veřejná doména, token tunelu — **nesdílet** |
| `seed/opendata` | snímky otevřených dat pro start i bez sítě |
| `load-and-run.sh` / `.ps1` | `docker load` + `docker compose up -d` jedním příkazem |
| `who-is-visiting.ps1` | kdo přistupuje na web (IP, země, stránky, prohlížeč) |

## 1. Spuštění
```bash
docker load -i smartaudit-stack.tar
docker compose -f docker-compose.prod.yml up -d
```
nebo `./load-and-run.sh` (Linux) / `.\load-and-run.ps1` (Windows). Web: `http://localhost:8088`.

## 2. Cloudflare Tunnel
**S vlastní doménou:** Cloudflare Zero Trust → Networks → Tunnels → Create tunnel (Cloudflared) → zkopírujte token
do `.env` (`CLOUDFLARE_TUNNEL_TOKEN=…`) → Public Hostname: `smartaudit.vasedomena.cz`, Service `HTTP` → `nginx:80`.
Do `.env` dejte i `PUBLIC_URL=https://smartaudit.vasedomena.cz` (QR kódy, katalog DCAT-AP-CZ) a spusťte:
```bash
docker compose -f docker-compose.prod.yml --profile tunnel up -d
```
**Bez účtu (dočasná adresa):**
```bash
docker compose -f docker-compose.prod.yml --profile quicktunnel up -d
docker compose -f docker-compose.prod.yml logs cloudflared-quick   # https://….trycloudflare.com
```
Cloudflared běžící mimo Docker (na hostiteli) nasměrujte na `http://localhost:8088`.

## 3. Kdo přistupuje na web
```bash
docker compose -f docker-compose.prod.yml logs -f nginx       # živě, čitelný řádek
tail -f logs/nginx/access.json.log                            # JSON (jq, GoAccess)
```
```powershell
.\who-is-visiting.ps1 -Hours 24      # souhrn: návštěvníci, země, stránky, QR karty, chyby
.\who-is-visiting.ps1 -Follow        # živý výpis
```
Řádek logu: `IP [čas] ZEMĚ "GET /objekty/SKO-5205-01 HTTP/1.1" 200 5123 0.042s ref="…" ua="…"`.
Skutečnou IP a zemi návštěvníka posílá Cloudflare (`CF-Connecting-IP`, `CF-IPCountry`); nginx jim věří jen ze sítě Dockeru.

## Aktualizace
Na vývojovém počítači znovu `.\export-docker-images.ps1` (hesla v `.env` zůstanou), přeneste archiv a:
```bash
docker load -i smartaudit-stack.tar && docker compose -f docker-compose.prod.yml up -d
```
Data databáze jsou ve svazku `smartaudit_db_data` a aktualizace je nesmaže.
