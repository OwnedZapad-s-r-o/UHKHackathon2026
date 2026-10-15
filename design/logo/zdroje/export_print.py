"""PNG (průhledné, 2400 px na šířku) a vektorové PDF všech lockupů přes headless Chrome."""
import os
import re
from pathlib import Path

from playwright.sync_api import sync_playwright

KIT = Path(__file__).parent / "kit"
MASTER = KIT / "master"
PNG = KIT / "png"
PDF = KIT / "print"
PNG.mkdir(exist_ok=True)
PDF.mkdir(exist_ok=True)

with sync_playwright() as p:
    browser = p.chromium.launch(channel="chrome", headless=True)
    page = browser.new_page()
    for svg in sorted(MASTER.glob("*.svg")):
        src = svg.read_text(encoding="utf-8")
        vb = [float(v) for v in re.search(r'viewBox="([^"]+)"', src).group(1).split()]
        w, h = vb[2], vb[3]
        dark = "on-dark" in svg.name or svg.name.endswith("-white.svg")
        # PNG: průhledné pozadí, šířka 2400 px (symboly 1024)
        out_w = 1024 if w == h else 2400
        scale = out_w / w
        page.set_viewport_size({"width": int(out_w), "height": int(round(h * scale))})
        page.set_content(f'<html><body style="margin:0;background:transparent">'
                         f'<img src="data:image/svg+xml;base64,{__import__("base64").b64encode(src.encode()).decode()}" '
                         f'style="width:{out_w}px;height:{h * scale}px;display:block"></body></html>')
        page.wait_for_timeout(100)
        page.screenshot(path=str(PNG / f"{svg.stem}.png"), omit_background=True)
        # PDF: vektorově, velikost stránky = kresba (1 jednotka = 0,25 mm → symbol 64 mm)
        mm = 0.25
        bg = "#0b192c" if dark else "transparent"
        page.set_content(f'<html><head><style>@page{{size:{w * mm}mm {h * mm}mm;margin:0}}html,body{{margin:0;background:{bg}}}'
                         f'svg{{width:{w * mm}mm;height:{h * mm}mm;display:block}}</style></head><body>{src}</body></html>')
        page.pdf(path=str(PDF / f"{svg.stem}.pdf"), width=f"{w * mm}mm", height=f"{h * mm}mm", print_background=dark)
        print("ok", svg.name)
os._exit(0)
