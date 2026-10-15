"""Manuál použití loga SmartAudit (HTML → PDF A4 na výšku, 2 strany, a PNG náhled)."""
import base64
import os
from pathlib import Path

from playwright.sync_api import sync_playwright

HERE = Path(__file__).parent
M = HERE / "kit" / "master"
OUT = HERE / "kit"
FONTS = Path(r"C:\Users\robot\Desktop\Uhk_hackathon\frontend\app\fonts")


def svg(name, style=""):
    s = (M / f"{name}.svg").read_text(encoding="utf-8")
    return s.replace("<svg ", f'<svg style="{style}" ', 1)


def b64(p):
    return base64.b64encode(Path(p).read_bytes()).decode()


def mis(label, inner, extra=""):
    return f'<div class="mis"><div class="mbox" style="{extra}">{inner}</div><div class="x">✕</div><span>{label}</span></div>'


H = "height:30px;width:auto"
SYM = (M / "smartaudit-symbol.svg").read_text(encoding="utf-8")

HTML = f"""<!doctype html><html lang="cs"><head><meta charset="utf-8"><style>
@font-face {{ font-family: Hradec; src: url(data:font/woff2;base64,{b64(FONTS / 'Hradec-Regular.woff2')}); font-weight: 400; }}
@font-face {{ font-family: Hradec; src: url(data:font/woff2;base64,{b64(FONTS / 'Hradec-Bold.woff2')}); font-weight: 700; }}
@page {{ size: A4; margin: 0; }}
* {{ box-sizing: border-box; }}
body {{ margin: 0; font-family: Hradec, sans-serif; color: #0b192c; font-size: 10pt; line-height: 1.45; }}
.pg {{ width: 210mm; height: 297mm; padding: 16mm 16mm 12mm; position: relative; page-break-after: always; overflow: hidden; }}
.pg:last-child {{ page-break-after: auto; }}
h1 {{ font-size: 24pt; letter-spacing: -.03em; margin: 0; }}
h2 {{ font-size: 12pt; margin: 5mm 0 2mm; letter-spacing: -.01em; display: flex; gap: 8px; align-items: baseline; }}
h2 small {{ font: 700 8pt Consolas, monospace; color: #d90429; letter-spacing: .1em; }}
p {{ margin: 0 0 2mm; }}
.muted {{ color: #64748b; }}
.head {{ display: flex; justify-content: space-between; align-items: flex-end; border-bottom: 2.5px solid #d90429; padding-bottom: 4mm; }}
.grid2 {{ display: grid; grid-template-columns: minmax(0,1fr) minmax(0,1fr); gap: 5mm; }}
.grid3 {{ display: grid; grid-template-columns: repeat(3, minmax(0,1fr)); gap: 4mm; }}
svg {{ max-width: 100%; }}
.tile {{ border-radius: 3mm; padding: 4mm; display: flex; align-items: center; justify-content: center; min-height: 22mm; background: #f1f5f9; }}
.tile.d {{ background: #0b192c; }}
.cap {{ font-size: 8.5pt; color: #64748b; margin-top: 1.5mm; }}
table {{ width: 100%; border-collapse: collapse; font-size: 9pt; }}
td, th {{ padding: 1.4mm 2mm; border-bottom: 1px solid #e2e8f0; text-align: left; vertical-align: middle; }}
th {{ font-size: 8pt; color: #64748b; font-weight: 400; }}
.sw {{ width: 9mm; height: 9mm; border-radius: 2mm; }}
.mono {{ font-family: Consolas, monospace; font-size: 8.5pt; }}
.clear {{ position: relative; display: inline-block; padding: 8mm; background:
  repeating-linear-gradient(45deg, rgba(217,4,41,.10) 0 2px, transparent 2px 6px); border: 1px dashed #d90429; }}
.clear .in {{ background: #fff; }}
.mis {{ position: relative; }}
.mbox {{ height: 18mm; border-radius: 2mm; background: #f1f5f9; display: flex; align-items: center; justify-content: center; overflow: hidden; }}
.mis .x {{ position: absolute; top: 1mm; right: 2mm; color: #d90429; font-weight: 700; }}
.mis span {{ font-size: 8.5pt; color: #475569; display: block; margin-top: 1mm; }}
.foot {{ position: absolute; bottom: 8mm; left: 16mm; right: 16mm; font-size: 8pt; color: #94a3b8; display: flex; justify-content: space-between; }}
ul {{ margin: 0; padding-left: 5mm; }} li {{ margin-bottom: 1mm; }}
</style></head><body>

<section class="pg">
  <div class="head"><div><div class="muted" style="font-size:9pt;letter-spacing:.12em;text-transform:uppercase">Manuál použití loga</div><h1>SmartAudit</h1></div>
    {svg('smartaudit-horizontal', 'height:15mm;width:auto')}</div>

  <h2><small>01</small> Myšlenka</h2>
  <p><b>Okno, kterým uniká teplo.</b> Fasáda šesti oken — jedno vyjíždí ven. Je to místo, kde kraj ztrácí teplo,
  a právě to SmartAudit najde. Značka přímo nese sdělení <i>„Méně tepla oknem. Víc peněz pro kraj.“</i>
  Myšlenka drží i v jedné barvě: nese ji posun okna, červená ho jen zdůrazňuje.</p>

  <h2><small>02</small> Verze</h2>
  <div class="grid3">
    <div><div class="tile">{svg('smartaudit-horizontal', 'height:11mm;width:auto')}</div><div class="cap"><b>Horizontální</b> — hlavní verze (web, PDF, prezentace)</div></div>
    <div><div class="tile">{svg('smartaudit-stacked', 'height:17mm;width:auto')}</div><div class="cap"><b>Skládaná</b> — úzké a vysoké plochy (cedulky, plakáty)</div></div>
    <div><div class="tile">{svg('smartaudit-symbol', 'height:16mm;width:auto')}</div><div class="cap"><b>Symbol</b> — ikona aplikace, favicon, avatar; do 24 px vždy <i>malý řez</i></div></div>
    <div><div class="tile d">{svg('smartaudit-horizontal-on-dark', 'height:11mm;width:auto')}</div><div class="cap"><b>Na tmavé</b> — bílá + světlejší červená #E63946, okna opticky ztenčená</div></div>
    <div><div class="tile">{svg('smartaudit-horizontal-black', 'height:11mm;width:auto')}</div><div class="cap"><b>Jednobarevná</b> — razítka, fax, gravura, laser</div></div>
    <div><div class="tile">{svg('smartaudit-wordmark', 'height:8mm;width:auto')}</div><div class="cap"><b>Jen slovo</b> — když symbol už stojí vedle (např. v záhlaví s ikonou)</div></div>
  </div>

  <h2><small>03</small> Ochranná zóna</h2>
  <div class="grid2" style="align-items:center">
    <div style="text-align:center"><div class="clear"><div class="in">{svg('smartaudit-horizontal', 'height:12mm;width:auto;display:block')}</div></div></div>
    <p>Kolem loga nechte volné místo o velikosti <b>jedné šířky okna (O)</b> ze symbolu — na každé straně.
    Zóna roste s logem, nikdy ji neměřte v pevných milimetrech. Mezi symbolem a slovem je v horizontální verzi
    přesně 1 O, ve skládané ¾ O — části lockupu nepřeskládávejte.</p>
  </div>

  <h2><small>04</small> Minimální velikost</h2>
  <table><tr><th>Verze</th><th>Obrazovka</th><th>Tisk</th></tr>
    <tr><td>Horizontální</td><td>120 px na šířku</td><td>30 mm na šířku</td></tr>
    <tr><td>Skládaná</td><td>72 px na šířku</td><td>18 mm na šířku</td></tr>
    <tr><td>Symbol</td><td>32 px (16–24 px jen <span class="mono">symbol-small</span> / favicon)</td><td>6 mm</td></tr></table>

  <h2><small>05</small> Symbol samostatně</h2>
  <div class="grid2" style="align-items:center">
    <div class="tile" style="justify-content:flex-start;gap:6mm;min-height:24mm">
      <div style="width:18mm;height:18mm;border-radius:4mm;background:#0b192c;display:flex;align-items:center;justify-content:center">{svg('smartaudit-symbol-on-dark', 'height:13mm;width:auto')}</div>
      {svg('smartaudit-symbol', 'height:12mm;width:auto')}
      {svg('smartaudit-symbol-small', 'height:6mm;width:auto')}
      {svg('smartaudit-symbol-small', 'height:4.2mm;width:auto')}</div>
    <p>Symbol stojí sám jako <b>ikona aplikace</b> (na navy dlaždici), avatar nebo znak uprostřed QR kódu.
    Pod 24 px vždy <b>malý řez</b> — širší okna na pixelové mřížce. Samotné slovo SmartAudit jen tam,
    kde symbol už stojí v blízkosti.</p>
  </div>
  <div class="foot"><span>SmartAudit · manuál loga · verze 1.1 (29. 9. 2026)</span><span>1 / 2</span></div>
</section>

<section class="pg">
  <h2 style="margin-top:0"><small>06</small> Barvy</h2>
  <table><tr><th></th><th>Název</th><th>HEX</th><th>RGB</th><th>CMYK (přibližně)</th><th>Pantone (nejbližší)</th><th>Použití</th></tr>
    <tr><td><div class="sw" style="background:#0b192c"></div></td><td><b>Navy kraje</b></td><td class="mono">#0B192C</td><td class="mono">11 25 44</td><td class="mono">75 43 0 83</td><td class="mono">≈ 5395 C</td><td>okna, slovo; kontrast na bílé 17,6 : 1</td></tr>
    <tr><td><div class="sw" style="background:#d90429"></div></td><td><b>Červená kraje</b></td><td class="mono">#D90429</td><td class="mono">217 4 41</td><td class="mono">0 98 81 15</td><td class="mono">≈ 199 C</td><td>vysunuté okno na světlé; 5,3 : 1 na bílé</td></tr>
    <tr><td><div class="sw" style="background:#e63946"></div></td><td><b>Červená na tmavé</b></td><td class="mono">#E63946</td><td class="mono">230 57 70</td><td class="mono">0 75 70 10</td><td class="mono">≈ 1788 C</td><td>jen na navy / tmavých plochách (4,2 : 1; #D90429 by měla jen 3,4 : 1)</td></tr>
    <tr><td><div class="sw" style="background:#fff;border:1px solid #cbd5e1"></div></td><td><b>Bílá</b></td><td class="mono">#FFFFFF</td><td class="mono">255 255 255</td><td class="mono">0 0 0 0</td><td>—</td><td>okna a slovo na tmavé</td></tr></table>
  <p class="muted" style="font-size:8.5pt;margin-top:2mm">CMYK a Pantone jsou převody z RGB — před tiskem ověřte na vzorníku, případně převezměte přesné hodnoty z manuálu jednotného vizuálního stylu kraje.</p>

  <h2><small>07</small> Povolená pozadí</h2>
  <div class="grid3">
    <div><div class="tile" style="background:#fff;border:1px solid #e2e8f0">{svg('smartaudit-horizontal', 'height:9mm;width:auto')}</div><div class="cap">Barevné na bílé / světle šedé</div></div>
    <div><div class="tile d">{svg('smartaudit-horizontal-on-dark', 'height:9mm;width:auto')}</div><div class="cap">Verze na tmavé na navy #0B192C</div></div>
    <div><div class="tile" style="background:#d90429">{svg('smartaudit-horizontal-white', 'height:9mm;width:auto')}</div><div class="cap">Bílá jednobarevná na červené kraje</div></div>
  </div>
  <p class="muted" style="font-size:8.5pt;margin-top:2mm">Na fotografii jen bílá nebo černá verze na klidném místě, jinak na navy podkladu.</p>

  <h2><small>08</small> Písmo</h2>
  <p><b>Hradec Bold</b> (slovo v logu, nadpisy) a <b>Hradec Regular</b> (text) — písmo jednotného vizuálního stylu,
  © 2020 Tomáš Brousil. V logu je slovo převedené na křivky a upravené prostrkání; nikdy ho nesázejte písmem znovu.
  Záložní písmo pro web: <span class="mono">Geist, system-ui, sans-serif</span>.
  <b>Licenci písma Hradec pro použití v logu je potřeba ověřit</b> u kraje / autora.</p>

  <h2><small>09</small> Nedělejte</h2>
  <div class="grid3">
    {mis('Nedeformujte (roztažení, zúžení)', svg('smartaudit-horizontal', 'height:14mm;width:30mm').replace('<svg ', '<svg preserveAspectRatio="none" ', 1))}
    {mis('Nepřebarvujte mimo paletu', svg('smartaudit-horizontal', 'height:9mm;width:auto;filter:hue-rotate(140deg) saturate(3)'))}
    {mis('Nevracejte okno do mřížky', SYM.replace('x="144" y="24"', 'x="120" y="48"').replace('<svg ', '<svg style="height:18mm;width:auto" ', 1))}
    {mis('Neotáčejte', svg('smartaudit-horizontal', 'height:9mm;width:auto;transform:rotate(-12deg)'))}
    {mis('Žádné stíny, obrysy, přechody', svg('smartaudit-horizontal', 'height:9mm;width:auto;filter:drop-shadow(3px 3px 2px rgba(0,0,0,.6))'))}
    {mis('Ne na neklidném pozadí', svg('smartaudit-horizontal', 'height:9mm;width:auto'), 'background:repeating-linear-gradient(60deg,#94a3b8 0 6px,#e2e8f0 6px 14px,#d90429 14px 17px)')}
  </div>

  <h2><small>10</small> Soubory</h2>
  <ul style="font-size:9pt">
    <li><span class="mono">master/*.svg</span> — zdrojové vektory (obrazovky, web, vývoj)</li>
    <li><span class="mono">print/*.pdf</span> — vektorové PDF pro tisk · <span class="mono">png/*.png</span> — průhledné PNG pro kancelář</li>
    <li><span class="mono">web/</span> — favicon.svg (sám se přepne pro tmavý režim), favicon.ico, ikony 180/192/512, maskable, webmanifest, <span class="mono">head-snippet.html</span></li>
    <li><span class="mono">varianty/</span> — černá, bílá, navy, čtvercový avatar · <span class="mono">prezentace/</span> — deska s ukázkami a kontexty</li>
  </ul>
  <div class="foot"><span>Značka je nová — před veřejným použitím doporučujeme rešerši ochranných známek.</span><span>2 / 2</span></div>
</section>
</body></html>"""

(OUT / "manual.html").write_text(HTML, encoding="utf-8")
with sync_playwright() as p:
    b = p.chromium.launch(channel="chrome", headless=True)
    pg = b.new_page(viewport={"width": 794, "height": 1123}, device_scale_factor=2)
    pg.set_content(HTML)
    pg.wait_for_timeout(600)
    pg.pdf(path=str(OUT / "SmartAudit-manual-loga.pdf"), format="A4", print_background=True, margin={"top": "0", "bottom": "0", "left": "0", "right": "0"})
    pg.screenshot(path=str(OUT / "manual-nahled.png"), full_page=True)
print("ok", flush=True)
os._exit(0)
