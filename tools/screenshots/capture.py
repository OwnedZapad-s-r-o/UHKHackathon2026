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


def api(base, path, token=None, raw=False):
    request = urllib.request.Request(base + path)
    if token:
        request.add_header("Authorization", "Bearer " + token)
    with urllib.request.urlopen(request) as response:
        body = response.read()
    return body if raw else json.loads(body)


def demo_token(base):
    request = urllib.request.Request(base + "/api/auth/demo", data=b'{"role":"Kraj"}', headers={"Content-Type": "application/json"})
    with urllib.request.urlopen(request) as response:
        return json.loads(response.read())["token"]


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
        # Demo po přihlášení spouští průvodce aplikací — na snímcích nemá co dělat
        page.evaluate("['Kraj', 'Spravce', 'Verejnost'].forEach((role) => localStorage.setItem(`smartaudit:pruvodce:v1:${role}`, '1'))")

        def shot(path, name, wait=2500):
            if args.only and name not in args.only:
                return
            page.goto(args.web + path)
            page.wait_for_load_state("load")
            page.mouse.move(1430, 890)
            page.wait_for_timeout(wait)
            save(page.screenshot(), out, name)

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

        browser.close()

    if not args.only or "pdf" in args.only:
        pdf_sheet(api(args.api, f"/api/reports/facility/{critical['id']}.pdf", token, raw=True), out, "pdf")


if __name__ == "__main__":
    main()
