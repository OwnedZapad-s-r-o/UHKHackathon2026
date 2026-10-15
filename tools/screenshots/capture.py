"""Snímky obrazovky pro landing page a README.

Předpoklad: běží `docker compose up` a pro živou obrazovku Senzory i emulátor gatewaye
(`python tools/iqrf-emulator/emulator.py --gateway demo-gw`).

    pip install -r tools/screenshots/requirements.txt
    python tools/screenshots/capture.py

Výstup: frontend/public/landing/*.webp (1440 × 900, telefon 390 × 844).
"""

import argparse
import io
import json
import urllib.request
from pathlib import Path

import pymupdf
from PIL import Image
from playwright.sync_api import sync_playwright

ROOT = Path(__file__).resolve().parents[2]

# Průvodce se pamatuje pro každou roli a stránku zvlášť (app-tour.tsx) — na snímcích nemá co dělat
SKIP_TOURS = """
['Kraj', 'Spravce', 'Verejnost'].forEach((role) => {
  localStorage.setItem(`smartaudit:pruvodce:v1:${role}`, '1');
  ['senzory', 'gateway'].forEach((phase) => localStorage.setItem(`smartaudit:pruvodce:v1:${role}:${phase}`, '1'));
})
"""


def api(base, path, token=None, raw=False, method=None, body=None):
    data = json.dumps(body).encode() if body is not None else None
    request = urllib.request.Request(base + path, data=data, method=method)
    if data is not None:
        request.add_header("Content-Type", "application/json")
    if token:
        request.add_header("Authorization", "Bearer " + token)
    with urllib.request.urlopen(request) as response:
        body = response.read()
    return body if raw or not body else json.loads(body)


def demo_token(base, role="Kraj"):
    return api(base, "/api/auth/demo", method="POST", body={"role": role})["token"]


def save(png_bytes, out, name):
    Image.open(io.BytesIO(png_bytes)).convert("RGB").save(out / f"{name}.webp", "WEBP", quality=82, method=6)
    print(f"[OK] {name}")


def pdf_sheet(pdf_bytes, out, name):
    # Dvě strany PDF vedle sebe na šedém pozadí ve formátu 1440 × 900
    document = pymupdf.open(stream=pdf_bytes, filetype="pdf")
    pages = [Image.open(io.BytesIO(document[i].get_pixmap(dpi=90).tobytes("png"))) for i in range(min(2, document.page_count))]
    height = 820
    pages = [p.resize((int(p.width * height / p.height), height)) for p in pages]
    sheet = Image.new("RGB", (1440, 900), (226, 232, 240))
    x = (1440 - sum(p.width for p in pages) - 40 * (len(pages) - 1)) // 2
    for page in pages:
        sheet.paste(page, (x, 40))
        x += page.width + 40
    buffer = io.BytesIO()
    sheet.save(buffer, "PNG")
    save(buffer.getvalue(), out, name)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--web", default="http://localhost:3000")
    parser.add_argument("--api", default="http://localhost:8080")
    parser.add_argument("--facility", default="SKO-5205-01", help="kód objektu pro 3D a veřejnou kartu")
    parser.add_argument("--out", default=str(ROOT / "frontend" / "public" / "landing"))
    parser.add_argument("--only", nargs="*", help="jen vybrané snímky, např. --only mapa-okresy pdf")
    args = parser.parse_args()

    out = Path(args.out)
    out.mkdir(parents=True, exist_ok=True)

    facilities = api(args.api, "/api/open/v1/facilities")
    hero = next(f for f in facilities if f["code"] == args.facility)
    critical = next((f for f in facilities if f["status"] == "Critical"), hero)
    token = demo_token(args.api)
    gateways = api(args.api, "/api/sensors/gateways", token)["gateways"]

    with sync_playwright() as p:
        browser = p.chromium.launch(channel="chrome", headless=True)
        page = browser.new_page(viewport={"width": 1440, "height": 900})

        page.goto(args.web + "/login")
        page.get_by_role("button", name="Jsem z kraje").click()
        page.wait_for_url("**/mapa**")
        page.evaluate(SKIP_TOURS)

        def shot(path, name, wait=2500, target=None, prepare=None):
            if args.only and name not in args.only:
                return
            target = target or page
            target.goto(args.web + path)
            target.wait_for_load_state("load")
            target.mouse.move(1430, 890)
            target.wait_for_timeout(wait)
            if prepare:
                prepare(target)
            save(target.screenshot(), out, name)

        # Zahřátí: první načtení podkladových dlaždic CARTO je pomalé, snímek by zachytil prázdnou mapu
        page.goto(args.web + f"/mapa?objekt={hero['id']}")
        page.wait_for_timeout(12000)

        shot("/mapa", "mapa-okresy", 7000)
        shot(f"/mapa?objekt={hero['id']}&panel=telemetrie", "mapa-3d", 12000)
        shot(f"/mapa?objekt={critical['id']}&panel=audit", "inspekce", 9000)
        if gateways:
            shot(f"/senzory/{gateways[0]['id']}", "senzory", 6000)
        shot("/otevrena-data", "otevrene-api")

        # Další obrazovky pro prezentaci a README (landing je nepoužívá)
        shot("/mapa?okres=CZ0521", "okres-vybrany", 6000)
        shot("/portfolio", "portfolio")
        shot("/anomalie", "anomalie")
        shot("/zdroje-dat", "zdroje-dat")
        shot("/objekty", "objekty")
        shot(f"/objekty/{hero['code']}/qr", "qr-cedulka")
        shot("/", "landing")
        shot(f"/objekty/{hero['code']}", "verejna-karta")

        page.set_viewport_size({"width": 390, "height": 844})
        shot(f"/objekty/{hero['code']}", "karta-mobil")

        # Obrazovky správce budovy: párování, demo zařízení a úpravy budov
        manager = browser.new_page(viewport={"width": 1440, "height": 900})
        manager.goto(args.web + "/login")
        manager.get_by_role("button", name="Spravuji budovu").click()
        manager.wait_for_url("**/mapa**")
        manager.evaluate(SKIP_TOURS)
        manager_token = demo_token(args.api, "Spravce")

        shot("/demo", "demo-sprava", 4000, manager)

        if not args.only or "parovani" in args.only:
            # Čerstvě „vyrobená“ gateway se do 10 s ohlásí v Nových zařízeních online; po snímku se zase smaže
            device = api(args.api, "/api/demo/gateways", manager_token, method="POST", body={"templates": ["ucebna", "chodba"], "behavior": "Setback"})

            def open_pairing(target):
                connect = target.get_by_role("button", name="Připojit do mé budovy").first
                connect.wait_for(timeout=30000)
                connect.click()
                target.get_by_text("1. Kam gateway patří").wait_for()
                target.get_by_text("1. Kam gateway patří").scroll_into_view_if_needed()
                target.mouse.move(1430, 890)
                target.wait_for_timeout(800)

            try:
                shot("/senzory", "parovani", 2000, manager, open_pairing)
            finally:
                api(args.api, f"/api/demo/{device['id']}", manager_token, method="DELETE")

        def pick_address(target):
            target.get_by_role("combobox").fill("Komenského 63 Trutnov")
            target.get_by_role("option").first.click(timeout=15000)
            target.mouse.move(1430, 890)
            target.wait_for_timeout(8000)

        shot("/budovy/nova", "nova-budova", 3000, manager, pick_address)

        # Editor obrysu na první budově správce, která má doplněnou přilehlou budovu
        own = api(args.api, "/api/facilities", manager_token)
        edited = next(((f, parts[0]) for f in own if (parts := api(args.api, f"/api/facilities/{f['id']}/building-parts", manager_token))), None)
        if edited:
            facility, part = edited

            # Jako uživatel: odkaz v záložce Budova — při přímém otevření URL by přelet k objektu přebil 2D pohled editoru
            def open_outline(target):
                target.locator(f'a[href*="budova=obrys%3A{part["id"]}"], a[href*="budova=obrys:{part["id"]}"]').first.click()
                target.mouse.move(1430, 890)
                target.wait_for_timeout(3000)

            shot(f"/mapa?objekt={facility['id']}&panel=budova", "budova-obrys", 12000, manager, open_outline)

        manager.close()
        page.close()
        browser.close()

    if not args.only or "pdf" in args.only:
        pdf_sheet(api(args.api, f"/api/reports/facility/{critical['id']}.pdf", token, raw=True), out, "pdf")


if __name__ == "__main__":
    main()
