"""Ukázky loga v reálných kontextech SmartAuditu (sidebar s logem kraje, hlavička PDF, QR cedulka, úvodní obrazovka)."""
import base64
import os
from pathlib import Path

from playwright.sync_api import sync_playwright

HERE = Path(__file__).parent
M = HERE / "kit" / "master"
OUT = HERE / "kit" / "prezentace"
FONTS = Path(r"C:\Users\robot\Desktop\Uhk_hackathon\frontend\app\fonts")


def svg(name):
    return (M / f"{name}.svg").read_text(encoding="utf-8").split("?>")[-1]


def b64(p):
    return base64.b64encode(Path(p).read_bytes()).decode()


QR = """<svg viewBox="0 0 29 29" width="100%" height="100%" shape-rendering="crispEdges">""" + "".join(
    f'<rect x="{x}" y="{y}" width="1" height="1" fill="#0b192c"/>'
    for y in range(29) for x in range(29)
    if (x < 7 and y < 7 and (x in (0, 6) or y in (0, 6) or (2 <= x <= 4 and 2 <= y <= 4)))
    or (x > 21 and y < 7 and (x in (22, 28) or y in (0, 6) or (24 <= x <= 26 and 2 <= y <= 4)))
    or (x < 7 and y > 21 and (x in (0, 6) or y in (22, 28) or (2 <= x <= 4 and 24 <= y <= 26)))
    or (x > 7 and y > 7 and ((x * 7 + y * 13 + x * y) % 5 < 2))
) + "</svg>"

HTML = f"""<!doctype html><html lang="cs"><head><meta charset="utf-8"><style>
@font-face {{ font-family: Hradec; src: url(data:font/woff2;base64,{b64(FONTS / 'Hradec-Regular.woff2')}); font-weight: 400; }}
@font-face {{ font-family: Hradec; src: url(data:font/woff2;base64,{b64(FONTS / 'Hradec-Bold.woff2')}); font-weight: 700; }}
* {{ box-sizing: border-box; }}
body {{ margin: 0; width: 1600px; height: 1000px; background: #eef2f6; font-family: Hradec, sans-serif; color: #0b192c;
  display: grid; grid-template-columns: 1fr 1fr; grid-template-rows: 1fr 1fr; gap: 28px; padding: 28px; }}
.panel {{ position: relative; border-radius: 22px; overflow: hidden; background: #dfe6ee; display: flex; align-items: center; justify-content: center; }}
.tag {{ position: absolute; left: 16px; bottom: 14px; font: 13px Hradec; background: #fff; border-radius: 999px; padding: 5px 12px; color: #475569; }}
/* 1 sidebar */
.side {{ width: 300px; height: 420px; background: #0b192c; border-radius: 14px; padding: 26px 22px; color: #fff; box-shadow: 0 30px 60px -25px rgba(0,0,0,.5); }}
.endorse {{ display: flex; align-items: center; gap: 14px; }}
.endorse img {{ height: 40px; }}
.endorse .div {{ width: 1.5px; height: 40px; background: rgba(255,255,255,.25); }}
.side .sa {{ height: 34px; margin-top: 20px; }}
.nav {{ margin-top: 28px; display: grid; gap: 6px; }}
.nav div {{ padding: 11px 14px; border-radius: 10px; font-size: 16px; color: rgba(255,255,255,.75); }}
.nav .on {{ background: rgba(255,255,255,.08); color: #fff; box-shadow: inset 0 -3px 0 #d90429; }}
/* 2 PDF */
.page {{ width: 420px; height: 420px; background: #fff; box-shadow: 0 30px 60px -25px rgba(0,0,0,.4); padding: 30px 34px; }}
.ph {{ display: flex; justify-content: space-between; align-items: center; border-bottom: 3px solid #d90429; padding-bottom: 14px; }}
.ph svg {{ height: 30px; width: auto; }}
.ph .m {{ font-size: 11px; color: #64748b; text-align: right; line-height: 1.4; }}
.page h3 {{ font-size: 22px; margin: 22px 0 4px; letter-spacing: -.02em; }}
.page .s {{ font-size: 12px; color: #64748b; }}
.kpi {{ display: grid; grid-template-columns: repeat(3,1fr); gap: 10px; margin-top: 18px; }}
.kpi div {{ background: #f1f5f9; border-radius: 8px; padding: 10px; font-size: 11px; color: #64748b; }}
.kpi b {{ display: block; font-size: 18px; color: #0b192c; margin-top: 3px; }}
.kpi .r b {{ color: #d90429; }}
.lines div {{ height: 8px; background: #eef2f6; border-radius: 4px; margin-top: 10px; }}
/* 3 QR cedulka */
.card {{ width: 330px; height: 420px; background: #fff; border-radius: 14px; overflow: hidden; box-shadow: 0 30px 60px -25px rgba(0,0,0,.45); }}
.card .top {{ background: #0b192c; padding: 18px 20px; display: flex; justify-content: space-between; align-items: center; border-bottom: 4px solid #d90429; }}
.card .top img {{ height: 30px; }}
.card .top svg {{ height: 24px; width: auto; }}
.card .in {{ padding: 16px 22px; }}
.card .k {{ font-size: 11px; letter-spacing: .12em; color: #d90429; font-weight: 700; text-transform: uppercase; }}
.card h4 {{ font-size: 26px; line-height: 1.05; margin: 6px 0 12px; letter-spacing: -.03em; }}
.card .qr {{ width: 150px; height: 150px; padding: 8px; border: 3px solid #0b192c; border-radius: 10px; }}
.card .row {{ display: flex; gap: 14px; align-items: center; }}
.card ol {{ font-size: 12px; padding-left: 16px; margin: 0; color: #334155; line-height: 1.6; }}
/* 4 úvodní obrazovka */
.hero {{ width: 680px; height: 380px; background: #0b192c; border-radius: 14px; padding: 34px 40px; color: #fff; position: relative; overflow: hidden; box-shadow: 0 30px 60px -25px rgba(0,0,0,.5); }}
.hero .bar {{ display: flex; justify-content: space-between; align-items: center; }}
.hero .bar svg {{ height: 30px; width: auto; }}
.hero .bar span {{ font-size: 14px; color: rgba(255,255,255,.7); margin-left: 22px; }}
.hero h2 {{ font-size: 44px; line-height: 1.05; letter-spacing: -.04em; margin: 54px 0 14px; max-width: 470px; }}
.hero p {{ color: rgba(255,255,255,.7); font-size: 16px; max-width: 430px; }}
.hero .btn {{ display: inline-block; margin-top: 16px; background: #d90429; padding: 12px 20px; border-radius: 10px; font-weight: 700; font-size: 15px; }}
.hero .big {{ position: absolute; right: -40px; bottom: -40px; width: 300px; opacity: .08; }}
.hero .big svg {{ width: 100%; height: auto; }}
</style></head><body>
<div class="panel">
  <div class="side">
    <div class="sa">{svg('smartaudit-horizontal-on-dark').replace('<svg ', '<svg style="height:34px;width:auto" ')}</div>
    <div class="nav"><div class="on">Mapa</div><div>Portfolio</div><div>Anomálie</div><div>Senzory</div><div>Otevřené API</div></div>
  </div><span class="tag">Aplikace — postranní panel</span></div>
<div class="panel">
  <div class="page">
    <div class="ph">{svg('smartaudit-horizontal')}<div class="m">Audit objektu SKO-5207-04<br>vyhodnoceno 26. 9. 2026</div></div>
    <h3>Masarykova obchodní akademie, Jičín</h3><div class="s">17. listopadu 220 · okres Jičín · kritická anomálie</div>
    <div class="kpi"><div>Teplota o víkendu<b>24,4 °C</b></div><div>Útlum má být<b>18 °C</b></div><div class="r">Ztráta za rok<b>137 200 Kč</b></div></div>
    <div class="lines"><div style="width:92%"></div><div style="width:84%"></div><div style="width:88%"></div><div style="width:60%"></div></div>
  </div><span class="tag">PDF audit objektu — hlavička</span></div>
<div class="panel">
  <div class="card">
    <div class="top">{svg('smartaudit-horizontal-on-dark')}</div>
    <div class="in"><div class="k">Kvalita vnitřního vzduchu</div><h4>Jak se dýchá v této třídě?</h4>
      <div class="row"><div class="qr">{QR}</div><ol><li>Otevřete fotoaparát</li><li>Namiřte na čtverec</li><li>Klepněte na odkaz</li></ol></div></div>
  </div><span class="tag">QR cedulka na dveře třídy</span></div>
<div class="panel">
  <div class="hero">
    <div class="bar"><div style="display:flex;align-items:center">{svg('smartaudit-horizontal-on-dark')}</div><span>Mapa · Portfolio · Pro veřejnost</span></div>
    <h2>Kolik kraj protopí v budovách, které nikdo neměří?</h2>
    <p>SmartAudit to zjistí z otevřených dat a čidel IQRF.</p><span class="btn">Vstoupit do dema →</span>
    <div class="big">{svg('smartaudit-symbol-on-dark')}</div>
  </div><span class="tag">Úvodní stránka</span></div>
</body></html>"""

(OUT / "kontexty.html").write_text(HTML, encoding="utf-8")
with sync_playwright() as p:
    b = p.chromium.launch(channel="chrome", headless=True)
    pg = b.new_page(viewport={"width": 1600, "height": 1000}, device_scale_factor=2)
    pg.set_content(HTML)
    pg.wait_for_timeout(500)
    pg.screenshot(path=str(OUT / "kontexty.png"))
print("ok")
os._exit(0)
