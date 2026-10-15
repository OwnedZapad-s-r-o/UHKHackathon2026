"""v2: A = fasáda s oknem, kterým uniká teplo; B = S půdorys (beze změny); C = termokontura."""
from build import svg, lockup, concept_b, INK
from pathlib import Path
HERE = Path(__file__).parent

def rr(x, y, w, h, r, fill):
    return f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{r}" fill="{fill}"/>'

# A: fasáda 2×3 oken (okna 64×48, mezera 16), pravé horní okno vyjíždí ven (+24, −24)
def concept_a(accent=INK):
    x0, y0, w, h, g = 44, 18, 64, 48, 16
    out = ""
    for row in range(3):
        for col in range(2):
            x, y = x0 + col * (w + g), y0 + 24 + row * (h + g) + 8
            if (row, col) == (0, 1):
                out += rr(x + 24, y - 24, w, h, 10, accent)
            else:
                out += rr(x, y, w, h, 10, INK)
    return out

# C: termokontura — dvě soustředné zaoblené kontury sbíhající se k pravému hornímu rohu, horké jádro
def ring(x, y, w, h, r, t):
    o = f"M{x+r},{y} H{x+w-r} A{r},{r} 0 0 1 {x+w},{y+r} V{y+h-r} A{r},{r} 0 0 1 {x+w-r},{y+h} H{x+r} A{r},{r} 0 0 1 {x},{y+h-r} V{y+r} A{r},{r} 0 0 1 {x+r},{y} Z"
    xi, yi, wi, hi, ri = x + t, y + t, w - 2 * t, h - 2 * t, max(r - t, 2)
    i = f"M{xi+ri},{yi} H{xi+wi-ri} A{ri},{ri} 0 0 1 {xi+wi},{yi+ri} V{yi+hi-ri} A{ri},{ri} 0 0 1 {xi+wi-ri},{yi+hi} H{xi+ri} A{ri},{ri} 0 0 1 {xi},{yi+hi-ri} V{yi+ri} A{ri},{ri} 0 0 1 {xi+ri},{yi} Z"
    return o + " " + i

def concept_c(accent=INK):
    d = ring(24, 24, 208, 208, 56, 24) + " " + ring(84, 64, 108, 108, 30, 22)
    return f'<path d="{d}" fill="{INK}" fill-rule="evenodd"/>' + rr(124, 88, 44, 44, 11, accent)

if __name__ == "__main__":
    for key, fn in {"a": concept_a, "b": concept_b, "c": concept_c}.items():
        (HERE / f"concept-{key}-v2.svg").write_text(svg(fn()), encoding="utf-8")
        (HERE / f"concept-{key}-lockup-v2.svg").write_text(lockup(fn()), encoding="utf-8")
        col = svg(fn("#d90429")).replace('fill="#000"', 'fill="#0b192c"').replace('stroke="#000"', 'stroke="#0b192c"')
        (HERE / f"concept-{key}-color-v2.svg").write_text(col, encoding="utf-8")
