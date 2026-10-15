"""Finální artwork loga SmartAudit (koncept A — „Okno, kterým uniká teplo“).

Geometrie symbolu (plátno 256, mřížka 8):
  okna 64×48, poloměr 10, mezera 16; sloupce x = 40, 120; řady y = 48, 112, 176
  pravé horní okno je vysunuté o (+24, −24) → x 144, y 24 (akcentní barva)
  kresba zabírá x 40–208, y 24–224 (168 × 200), je o 4 jednotky vlevo a nahoře od středu = optické vyvážení
  vysunutého okna vpravo nahoře.
Malý řez (favicon, ≤ 24 px): mřížka 16 jednotek po 16 — okna 5×3, svislá mezera 1, vodorovná 2, posun (+2, −2).
Inverzní verze (na tmavé): okna ztenčená o 1,5 jednotky po obvodu (bílá na tmavé opticky „bobtná“).
"""
from pathlib import Path

import uharfbuzz as hb
from fontTools.pens.boundsPen import BoundsPen
from fontTools.pens.svgPathPen import SVGPathPen
from fontTools.pens.transformPen import TransformPen
from fontTools.ttLib import TTFont

HERE = Path(__file__).parent
OUT = HERE / "kit" / "master"
OUT.mkdir(parents=True, exist_ok=True)
FONT = Path(r"C:\Users\robot\Desktop\Uhk_hackathon\frontend\app\fonts\Hradec-Bold.woff2")

NAVY, RED, WHITE, RED_ON_DARK = "#0b192c", "#d90429", "#ffffff", "#e63946"
TITLE = "SmartAudit"


def rect(x, y, w, h, r, fill, inset=0.0):
    x, y, w, h, r = x + inset, y + inset, w - 2 * inset, h - 2 * inset, max(r - inset, 0)
    return f'<rect x="{x:g}" y="{y:g}" width="{w:g}" height="{h:g}" rx="{r:g}" fill="{fill}"/>'


def symbol(ink, accent, inset=0.0, dx=0, dy=0):
    wins = [(40, 48), (40, 112), (120, 112), (40, 176), (120, 176)]
    body = "".join(rect(x + dx, y + dy, 64, 48, 10, ink, inset) for x, y in wins)
    return body + rect(144 + dx, 24 + dy, 64, 48, 10, accent, inset)


def symbol_small(ink, accent):
    u = 16
    wins = [(1, 3), (1, 7), (8, 7), (1, 11), (8, 11)]
    body = "".join(rect(x * u, y * u, 5 * u, 3 * u, 12, ink) for x, y in wins)
    return body + rect(10 * u, 1 * u, 5 * u, 3 * u, 12, accent)


def doc(body, x, y, w, h, bg=None):
    bgr = f'<rect x="{x:g}" y="{y:g}" width="{w:g}" height="{h:g}" fill="{bg}"/>' if bg else ""
    return (f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="{x:g} {y:g} {w:g} {h:g}" width="{w:g}" height="{h:g}">'
            f"<title>{TITLE}</title>{bgr}{body}</svg>\n")


# ---------- text v křivkách s kerningem (HarfBuzz) ----------
_font = TTFont(FONT)
_gs = _font.getGlyphSet()
_upm = _font["head"].unitsPerEm
_CAP = _font["OS/2"].sCapHeight / _upm           # 0.700
_hbfont = hb.Font(hb.Face(hb.Blob.from_file_path(str(FONT.with_suffix(".ttf")))) if FONT.with_suffix(".ttf").exists() else None) if False else None


def _hb_font():
    import io
    buf = io.BytesIO()
    f = TTFont(FONT)
    f.flavor = None
    f.save(buf)
    return hb.Font(hb.Face(buf.getvalue()))


_HB = _hb_font()
_ORDER = _font.getGlyphOrder()


def text(s, cap, x, baseline, tracking=0.0):
    """Vrátí (svg path d, bbox) pro text s danou výškou verzálek; tracking v 1/1000 em."""
    size = cap / _CAP
    k = size / _upm
    buf = hb.Buffer()
    buf.add_str(s)
    buf.guess_segment_properties()
    hb.shape(_HB, buf, {"kern": True, "liga": True})
    pen_x, parts = x, []
    bp = BoundsPen(_gs)
    for info, pos in zip(buf.glyph_infos, buf.glyph_positions):
        name = _ORDER[info.codepoint]
        t = (k, 0, 0, -k, pen_x + pos.x_offset * k, baseline - pos.y_offset * k)
        sp = SVGPathPen(_gs)
        _gs[name].draw(TransformPen(sp, t))
        _gs[name].draw(TransformPen(bp, t))
        parts.append(sp.getCommands())
        pen_x += pos.x_advance * k + tracking * size / 1000
    return " ".join(parts), bp.bounds


# ---------- lockupy ----------
SYM_BOX = (40, 24, 208, 224)      # x0, y0, x1, y1 kresby symbolu
UNIT = 64                          # jednotka ochranné zóny = šířka okna


def horizontal(ink, accent, word_ink, inset=0.0):
    sx0, sy0, sx1, sy1 = SYM_BOX
    sh = sy1 - sy0                                   # 200
    cap = round(sh * 0.44)                           # výška verzálek slova = 88
    mid = sy0 + sh / 2 + 4                           # opticky na střed oken (vysunuté okno nahoře „nepočítá“)
    base = mid + cap / 2
    wx = sx1 + UNIT                                  # mezera symbol–slovo = 1 okno
    word, wb = text("SmartAudit", cap, wx, base, tracking=-12)
    body = symbol(ink, accent, inset) + f'<path d="{word}" fill="{word_ink}"/>'
    x0, x1 = sx0, wb[2]
    y0, y1 = min(sy0, wb[1]), max(sy1, wb[3])
    return body, (x0, y0, x1 - x0, y1 - y0)


def stacked(ink, accent, word_ink, inset=0.0):
    sx0, sy0, sx1, sy1 = SYM_BOX
    cap = 60
    word, wb = text("SmartAudit", cap, 0, 0, tracking=-12)
    base = sy1 + UNIT * 0.75 + cap                   # mezera symbol–slovo = ¾ okna
    shift = (sx0 + sx1) / 2 - (wb[2] - wb[0]) / 2 - wb[0]
    word, wb = text("SmartAudit", cap, shift, base, tracking=-12)
    body = symbol(ink, accent, inset) + f'<path d="{word}" fill="{word_ink}"/>'
    x0, x1 = min(sx0, wb[0]), max(sx1, wb[2])
    return body, (x0, sy0, x1 - x0, wb[3] - sy0)


def wordmark(word_ink):
    cap = 88
    word, wb = text("SmartAudit", cap, 0, cap, tracking=-12)
    body = f'<path d="{word}" fill="{word_ink}"/>'
    return body, (wb[0], wb[1], wb[2] - wb[0], wb[3] - wb[1])


def save(name, body, box, bg=None):
    import re
    x, y, w, h = (round(v, 2) for v in box)
    out = re.sub(r"-?\d+\.\d{3,}", lambda m: f"{float(m.group()):.2f}".rstrip("0").rstrip("."), doc(body, x, y, w, h, bg))
    (OUT / f"{name}.svg").write_text(out, encoding="utf-8")


if __name__ == "__main__":
    # symbol
    save("smartaudit-symbol", symbol(NAVY, RED), (0, 0, 256, 256))
    save("smartaudit-symbol-on-dark", symbol(WHITE, RED_ON_DARK, inset=1.5), (0, 0, 256, 256))
    save("smartaudit-symbol-small", symbol_small(NAVY, RED), (0, 0, 256, 256))
    save("smartaudit-symbol-small-on-dark", symbol_small(WHITE, RED_ON_DARK), (0, 0, 256, 256))
    # lockupy
    save("smartaudit-horizontal", *horizontal(NAVY, RED, NAVY))
    save("smartaudit-horizontal-on-dark", *horizontal(WHITE, RED_ON_DARK, WHITE, inset=1.5))
    save("smartaudit-stacked", *stacked(NAVY, RED, NAVY))
    save("smartaudit-stacked-on-dark", *stacked(WHITE, RED_ON_DARK, WHITE, inset=1.5))
    save("smartaudit-wordmark", *wordmark(NAVY))
    save("smartaudit-wordmark-on-dark", *wordmark(WHITE))
    # jednobarevné (akcent v barvě kresby — myšlenka drží posunem okna, ne barvou)
    save("smartaudit-horizontal-black", *horizontal("#000", "#000", "#000"))
    save("smartaudit-horizontal-white", *horizontal(WHITE, WHITE, WHITE, inset=1.5))
    print("hotovo:", sorted(p.name for p in OUT.glob("*.svg")))
