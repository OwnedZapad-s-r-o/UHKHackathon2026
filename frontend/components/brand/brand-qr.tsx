import QRCode from "qrcode";
import { LogoSymbol } from "./logo";

const INK = "#0b192c";
const ACCENT = "#d90429";

// Moduly jsou malá zaoblená okna s mezerou — fasáda jako v logu. 88 % plochy modulu čtečky spolehlivě berou.
const MODULE = 0.88;
const MODULE_RADIUS = 0.3;

// Od verze 7 leží uprostřed zarovnávací značka, logo by ji zakrylo — pak QR bez loga
const MAX_VERSION_WITH_LOGO = 6;

type BrandQrProps = {
  value: string;
  className?: string;
  /** Popis pro čtečky obrazovky */
  label?: string;
};

/**
 * QR kód v identitě SmartAuditu: zaoblené moduly (okna fasády), vyhledávací značky jako okenní rámy,
 * pravá horní značka červená (vysunuté okno z loga) a symbol uprostřed.
 * Korekce chyb H (30 %) snese logo přes střed i opotřebovanou cedulku. Ověřeno dekodérem OpenCV
 * na velikostech od 3 px na modul.
 */
export function BrandQr({ value, className, label }: BrandQrProps) {
  const qr = QRCode.create(value, { errorCorrectionLevel: "H" });
  const size = qr.modules.size;

  // Světlá plocha pod logem: lichý počet modulů kolem středu, zhruba pětina strany
  const withLogo = qr.version <= MAX_VERSION_WITH_LOGO;
  const hole = withLogo ? oddAtLeast(Math.round(size * 0.24), 7) : 0;
  const holeStart = (size - hole) / 2;
  const inHole = (row: number, col: number) =>
    withLogo && row >= holeStart && row < holeStart + hole && col >= holeStart && col < holeStart + hole;

  const inset = (1 - MODULE) / 2;
  const modules: React.ReactNode[] = [];
  for (let row = 0; row < size; row++) {
    for (let col = 0; col < size; col++) {
      if (!qr.modules.get(row, col) || isFinder(row, col, size) || inHole(row, col)) continue;
      modules.push(
        <rect key={`${row}-${col}`} x={col + inset} y={row + inset} width={MODULE} height={MODULE} rx={MODULE_RADIUS} />,
      );
    }
  }

  const logoTile = hole - 1;

  return (
    <svg viewBox={`0 0 ${size} ${size}`} className={className} role="img" aria-label={label ?? `QR kód: ${value}`} shapeRendering="geometricPrecision">
      <rect width={size} height={size} fill="#ffffff" />
      <g fill={INK}>{modules}</g>

      <Finder x={0} y={0} color={INK} />
      <Finder x={size - 7} y={0} color={ACCENT} />
      <Finder x={0} y={size - 7} color={INK} />

      {withLogo && (
        <g transform={`translate(${holeStart + 0.5} ${holeStart + 0.5})`}>
          <rect width={logoTile} height={logoTile} rx={logoTile * 0.22} fill="#ffffff" stroke={INK} strokeWidth={0.35} />
          <LogoSymbol x={logoTile * 0.14} y={logoTile * 0.14} size={logoTile * 0.72} />
        </g>
      )}
    </svg>
  );
}

// Vyhledávací značka 7 × 7: ostrý okenní rám, uvnitř zaoblená tabule jako okna v logu.
// Vnější rohy musí zůstat ostré — z nich čtečky počítají perspektivu (zaoblený rám OpenCV nepřečte).
function Finder({ x, y, color }: { x: number; y: number; color: string }) {
  return (
    <g transform={`translate(${x} ${y})`}>
      <path fillRule="evenodd" fill={color} d={`M0 0H7V7H0Z ${roundedRect(1, 1, 5, 5, 0.5)}`} />
      <rect x={2} y={2} width={3} height={3} rx={0.75} fill={color} />
    </g>
  );
}

function isFinder(row: number, col: number, size: number) {
  const top = row < 7;
  const left = col < 7;
  return (top && left) || (top && col >= size - 7) || (left && row >= size - 7);
}

function oddAtLeast(value: number, min: number) {
  const odd = value % 2 === 0 ? value + 1 : value;
  return Math.max(odd, min);
}

function roundedRect(x: number, y: number, w: number, h: number, r: number) {
  return `M${x + r} ${y}H${x + w - r}A${r} ${r} 0 0 1 ${x + w} ${y + r}V${y + h - r}A${r} ${r} 0 0 1 ${x + w - r} ${y + h}H${x + r}A${r} ${r} 0 0 1 ${x} ${y + h - r}V${y + r}A${r} ${r} 0 0 1 ${x + r} ${y}Z`;
}
