# SmartAudit KHK na veřejné doméně přes Raspberry Pi (3 minuty)

Cíl: QR kódy na cedulkách (`/objekty/{kód}/qr`) fungují na mobilech diváků a otevřené API `/api/open/v1/*`
+ katalog DCAT-AP-CZ `/api/open/v1/catalog` je veřejně na internetu (Čl. 5.2b).

```
mobil diváka ──https──▶ Raspberry Pi (nginx, doména, Let's Encrypt)
                          ├─ tunel 127.0.0.1:3000 / 8080 ──▶ notebook (docker compose)   ← Varianta A
                          └─ při výpadku tunelu ──▶ mirror 127.0.0.1:8090 (export dat)    ← Varianta B
```

## 0. Notebook — veřejná doména do `.env`
```bash
echo "PUBLIC_URL=https://smartaudit.nasedomena.cz" >> .env
docker compose up --build -d
```
(QR kód jde přepnout i bez rebuildu: na stránce QR cedulky je nahoře přepínač domény, pamatuje si ho prohlížeč.)

## 1. Raspberry Pi — jednou (nginx, HTTPS, mirror jako služba)
```bash
scp -r deploy/raspberry-pi pi@smartaudit.nasedomena.cz:~/smartaudit-deploy
ssh pi@smartaudit.nasedomena.cz "cd ~/smartaudit-deploy && sudo bash setup-rpi.sh smartaudit.nasedomena.cz admin@nasedomena.cz"
```

## 2. Varianta A — tunel z notebooku (vše živě, Pi nic nekompiluje)
Git Bash / Linux / macOS:
```bash
./deploy/raspberry-pi/expose-via-rpi.sh pi@smartaudit.nasedomena.cz
```
Windows PowerShell:
```powershell
.\deploy\raspberry-pi\expose-via-rpi.ps1 pi@smartaudit.nasedomena.cz
```
Alternativa bez Pi: Cloudflare Tunnel — `cloudflared tunnel --url http://localhost:3000` (dočasná adresa),
nebo vlastní doména podle `cloudflared/config.yml`.

## 3. Varianta B — mirror otevřených dat (funguje, i když notebook spí)
```bash
python deploy/raspberry-pi/export-static-opendata.py --source http://localhost:3000 --upload pi@smartaudit.nasedomena.cz:/opt/smartaudit-mirror/data
```
Mirror (`opendata-mirror.py`, jen standardní Python) vystaví katalog, objekty (JSON/GeoJSON/CSV), pre-audit,
zdroje a veřejné karty `/objekty/{kód}`. Nginx na něj přepne automaticky při chybě 502/503/504 tunelu.

## Kontrola před prezentací
```bash
curl -I https://smartaudit.nasedomena.cz/            # X-SmartAudit-Source: live | mirror
curl https://smartaudit.nasedomena.cz/api/open/v1/catalog
curl https://smartaudit.nasedomena.cz/mirror/health
```
