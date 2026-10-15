"""Koncepty loga SmartAudit — symboly (256×256) a lockupy se slovem SmartAudit v křivkách (Hradec Bold)."""
from pathlib import Path

from fontTools.pens.svgPathPen import SVGPathPen
from fontTools.pens.transformPen import TransformPen
from fontTools.ttLib import TTFont

HERE = Path(__file__).parent
FONT = Path(r"C:\Users\robot\Desktop\Uhk_hackathon\frontend\app\fonts\Hradec-Bold.woff2")
INK = "#000"


def svg(body, w=256, h=256):
    return f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {w} {h}" width="{w}" height="{h}">{body}</svg>\n'


# ---------- A: Únik — blok budovy, ze kterého uniká kus (ztráta tepla / nalezené zjištění)
A_BLOCK = ("M40,72 H104 A16,16 0 0 1 120,88 V120 A16,16 0 0 0 136,136 H168 A16,16 0 0 1 184,152 "
           "V216 A16,16 0 0 1 168,232 H40 A16,16 0 0 1 24,216 V88 A16,16 0 0 1 40,72 Z")
A_PIECE = '<rect x="180" y="28" width="48" height="48" rx="12"/>'


def concept_a(accent=INK):
    return f'<path d="{A_BLOCK}" fill="{INK}"/><g fill="{accent}">{A_PIECE}</g>'


# ---------- B: Půdorys S — písmeno S jako zdi půdorysu, v místnosti čidlo
B_S = "M200,40 H88 A32,32 0 0 0 56,72 V96 A32,32 0 0 0 88,128 H168 A32,32 0 0 1 200,160 V184 A32,32 0 0 1 168,216 H56"
B_DOT = '<rect x="136" y="72" width="28" height="28" rx="6"/>'


def concept_b(accent=INK):
    return (f'<path d="{B_S}" fill="none" stroke="{INK}" stroke-width="32" stroke-linecap="butt" stroke-linejoin="round"/>'
            f'<g fill="{accent}">{B_DOT}</g>')


# ---------- C: Okno pod lupou — čočka s křížem okna, jedna tabulka „vadná“
def concept_c(accent=INK):
    cx, cy, r = 108, 108, 76           # střed čočky, vnější poloměr
    ring = 26
    ri = r - ring                      # vnitřní poloměr = 50
    m = 7                              # půl šířky příčle
    # čtyři tabulky okna jako čtvrtkruhy uvnitř čočky oddělené příčlemi
    def pane(sx, sy):
        x0, y0 = cx + sx * m, cy + sy * m
        # roh u příčlí → po příčli k obvodu → oblouk → zpět
        import math
        a1 = math.asin(m / ri)
        # body na kružnici ri, kde končí vodorovná a svislá příčel
        px1, py1 = cx + sx * ri * math.cos(a1), cy + sy * m
        px2, py2 = cx + sx * m, cy + sy * ri * math.cos(a1)
        sweep = 1 if sx * sy > 0 else 0
        return f"M{x0:.2f},{y0:.2f} L{px1:.2f},{py1:.2f} A{ri},{ri} 0 0 {sweep} {px2:.2f},{py2:.2f} Z"
    panes = {k: pane(*k) for k in [(-1, -1), (1, -1), (-1, 1), (1, 1)]}
    ring_path = (f"M{cx - r},{cy} A{r},{r} 0 1 0 {cx + r},{cy} A{r},{r} 0 1 0 {cx - r},{cy} Z "
                 f"M{cx - ri},{cy} A{ri},{ri} 0 1 1 {cx + ri},{cy} A{ri},{ri} 0 1 1 {cx - ri},{cy} Z")
    # rukojeť 45°, zaoblený obdélník šířky 30
    handle = f'<rect x="{cx + 58}" y="{cy - 15}" width="92" height="30" rx="15" transform="rotate(45 {cx} {cy})"/>'
    body = f'<path d="{ring_path}" fill="{INK}" fill-rule="evenodd"/><g fill="{INK}">{handle}</g>'
    for k, d in panes.items():
        col = accent if k == (1, -1) else INK
        body += f'<path d="{d}" fill="{col}"/>'
    return body


# ---------- slovo SmartAudit v křivkách
def word_path(text, size, x, y):
    font = TTFont(FONT)
    gs, cmap = font.getGlyphSet(), font.getBestCmap()
    upm = font["head"].unitsPerEm
    s = size / upm
    out, pen_x = [], x
    for ch in text:
        g = cmap[ord(ch)]
        sp = SVGPathPen(gs)
        gs[g].draw(TransformPen(sp, (s, 0, 0, -s, pen_x, y)))
        out.append(sp.getCommands())
        pen_x += gs[g].width * s - size * 0.02       # lehce utažené prostrkání (-20/1000)
    return " ".join(out), pen_x


def lockup(symbol_body, accent=INK):
    word, end = word_path("SmartAudit", 118, 300, 170)
    khk, end2 = word_path("KHK", 52, end + 22, 170)
    w = int(end2 + 20)
    body = (f'<g>{symbol_body}</g><path d="{word}" fill="{INK}"/>'
            f'<path d="{khk}" fill="{accent}"/>')
    return svg(body, w, 256)


if __name__ == "__main__":
    for key, fn in {"a": concept_a, "b": concept_b, "c": concept_c}.items():
        (HERE / f"concept-{key}-v1.svg").write_text(svg(fn()), encoding="utf-8")
        (HERE / f"concept-{key}-lockup-v1.svg").write_text(lockup(fn()), encoding="utf-8")
        (HERE / f"concept-{key}-color-v1.svg").write_text(svg(fn("#d90429").replace('fill="#000"', 'fill="#0b192c"').replace('stroke="#000"', 'stroke="#0b192c"')), encoding="utf-8")
    print("hotovo")
