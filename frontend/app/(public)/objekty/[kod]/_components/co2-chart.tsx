import type { TelemetryPoint } from "@/types/api";

type Co2ChartProps = {
  // Nejhorší místnost v každé hodině
  points: TelemetryPoint[];
};

const WIDTH = 560;
const HEIGHT = 200;
const PADDING = { top: 12, right: 44, bottom: 24, left: 8 };

// CO₂ za 24 h: jedna čára, pásma 1 000 a 1 500 ppm jako vodítka — bez legendy, popisky přímo u čar
export function Co2Chart({ points }: Co2ChartProps) {
  if (points.length < 2) {
    return <p className="py-10 text-center text-[13px] text-slate-500">Za posledních 24 hodin zatím nejsou data.</p>;
  }

  const max = Math.max(1800, ...points.map((p) => p.value));
  const min = 400;
  const plotWidth = WIDTH - PADDING.left - PADDING.right;
  const plotHeight = HEIGHT - PADDING.top - PADDING.bottom;
  const start = new Date(points[0].timestamp).getTime();
  const end = new Date(points.at(-1)!.timestamp).getTime();

  const x = (timestamp: string) => PADDING.left + ((new Date(timestamp).getTime() - start) / Math.max(1, end - start)) * plotWidth;
  const y = (value: number) => PADDING.top + (1 - (value - min) / (max - min)) * plotHeight;

  const line = points.map((p, i) => `${i === 0 ? "M" : "L"}${x(p.timestamp).toFixed(1)},${y(p.value).toFixed(1)}`).join(" ");
  const last = points.at(-1)!;

  return (
    <svg viewBox={`0 0 ${WIDTH} ${HEIGHT}`} className="h-auto w-full" role="img" aria-label="CO₂ za posledních 24 hodin v nejhorší místnosti">
      {/* Pásmo „vydýchaný vzduch“ nad 1 500 ppm */}
      <rect x={PADDING.left} y={PADDING.top} width={plotWidth} height={Math.max(0, y(1500) - PADDING.top)} fill="#fef2f2" />
      <Guide y={y(1500)} label="1 500" color="#dc2626" />
      <Guide y={y(1000)} label="1 000" color="#d97706" />

      <path d={line} fill="none" stroke="#1e3e62" strokeWidth={2} strokeLinejoin="round" strokeLinecap="round" />
      <circle cx={x(last.timestamp)} cy={y(last.value)} r={3.5} fill="#e63946" />

      <text x={PADDING.left} y={HEIGHT - 6} fontSize={10.5} fill="#94a3b8">
        před 24 h
      </text>
      <text x={WIDTH - PADDING.right} y={HEIGHT - 6} fontSize={10.5} fill="#94a3b8" textAnchor="end">
        teď
      </text>
    </svg>
  );
}

// Vodorovná čára limitu s popiskem vpravo od grafu
function Guide({ y, label, color }: { y: number; label: string; color: string }) {
  return (
    <g>
      <line x1={PADDING.left} x2={WIDTH - PADDING.right} y1={y} y2={y} stroke={color} strokeDasharray="4 4" strokeWidth={1} opacity={0.6} />
      <text x={WIDTH - PADDING.right + 6} y={y + 3.5} fontSize={10.5} fill={color}>
        {label}
      </text>
    </g>
  );
}
