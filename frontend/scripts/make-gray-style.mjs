// Vygeneruje šedou variantu výchozích stylů mapy (CARTO Positron / Dark Matter), které používá mapcn.
// Každou barvu v paint/layout (i uvnitř výrazů interpolate/match/step) převede na šedou podle jasu.
// Proč styl a ne CSS filtr grayscale: filtr by zešedil i naše barevné piny a vrstvy (stav objektu).
//
// Spuštění: node scripts/make-gray-style.mjs

import { mkdir, writeFile } from "node:fs/promises";

const SOURCES = {
  light: "https://basemaps.cartocdn.com/gl/positron-gl-style/style.json",
  dark: "https://basemaps.cartocdn.com/gl/dark-matter-gl-style/style.json",
};

const OUTPUT_DIR = new URL("../public/map-styles/", import.meta.url);

function parseColor(value) {
  const text = value.trim().toLowerCase();

  const hex = text.match(/^#([0-9a-f]{3,8})$/);
  if (hex) {
    let digits = hex[1];
    if (digits.length === 3 || digits.length === 4) {
      digits = [...digits].map((d) => d + d).join("");
    }
    if (digits.length !== 6 && digits.length !== 8) return null;

    const r = parseInt(digits.slice(0, 2), 16);
    const g = parseInt(digits.slice(2, 4), 16);
    const b = parseInt(digits.slice(4, 6), 16);
    const a = digits.length === 8 ? parseInt(digits.slice(6, 8), 16) / 255 : 1;
    return { r, g, b, a };
  }

  const rgb = text.match(/^rgba?\(([^)]+)\)$/);
  if (rgb) {
    const [r, g, b, a = "1"] = rgb[1].split(",").map((p) => p.trim());
    return { r: Number(r), g: Number(g), b: Number(b), a: Number(a) };
  }

  const hsl = text.match(/^hsla?\(([^)]+)\)$/);
  if (hsl) {
    const [h, s, l, a = "1"] = hsl[1].split(",").map((p) => parseFloat(p));
    return { ...hslToRgb(h, s / 100, l / 100), a: Number(a) };
  }

  return null;
}

function hslToRgb(h, s, l) {
  const k = (n) => (n + h / 30) % 12;
  const f = (n) => l - s * Math.min(l, 1 - l) * Math.max(-1, Math.min(k(n) - 3, 9 - k(n), 1));
  return { r: Math.round(f(0) * 255), g: Math.round(f(8) * 255), b: Math.round(f(4) * 255) };
}

// Relativní jas (Rec. 709) → šedá se zachovanou průhledností
function toGray(value) {
  const color = parseColor(value);
  if (!color) return value;

  const gray = Math.round(0.2126 * color.r + 0.7152 * color.g + 0.0722 * color.b);
  const alpha = Math.round(color.a * 1000) / 1000;

  return `rgba(${gray}, ${gray}, ${gray}, ${alpha})`;
}

function grayDeep(node) {
  if (typeof node === "string") return toGray(node);
  if (Array.isArray(node)) return node.map(grayDeep);
  if (node && typeof node === "object") {
    return Object.fromEntries(Object.entries(node).map(([key, value]) => [key, grayDeep(value)]));
  }
  return node;
}

async function buildGrayStyle(theme, url) {
  const response = await fetch(url);
  if (!response.ok) throw new Error(`Styl ${url} nešel stáhnout: ${response.status}`);

  const style = await response.json();

  style.name = `${style.name ?? theme} (SmartAudit gray)`;
  style.layers = style.layers.map((layer) => ({
    ...layer,
    ...(layer.paint && { paint: grayDeep(layer.paint) }),
    ...(layer.layout && { layout: grayDeep(layer.layout) }),
  }));

  await writeFile(new URL(`gray-${theme}.json`, OUTPUT_DIR), JSON.stringify(style));
  console.log(`✔ public/map-styles/gray-${theme}.json (${style.layers.length} vrstev)`);
}

await mkdir(OUTPUT_DIR, { recursive: true });

for (const [theme, url] of Object.entries(SOURCES)) {
  await buildGrayStyle(theme, url);
}
