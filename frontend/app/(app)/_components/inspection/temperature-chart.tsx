"use client";

import { useState, type PointerEvent } from "react";
import type { TelemetryPoint } from "@/types/api";

const WIDTH = 440;
const HEIGHT = 150;
const PADDING = { top: 10, right: 8, bottom: 6, left: 30 };

const timeFormat = new Intl.DateTimeFormat("cs-CZ", { weekday: "short", hour: "2-digit", minute: "2-digit" });

type TemperatureChartProps = {
  points: TelemetryPoint[];
  // Doporučená teplota útlumu — referenční čára (null = bez čáry, např. u vlhkosti)
  setback: number | null;
  unit?: string;
  // Co křivka ukazuje — pro čtečky obrazovky
  label?: string;
  tickStep?: number;
  hours?: number;
};

// Jedna série → bez legendy; čára 2 px, hairline mřížka, hover křížek s hodnotou (dataviz skill)
export function TemperatureChart({ points, setback, unit = "°C", label = "Průměrná teplota", tickStep = 2, hours = 48 }: TemperatureChartProps) {
  const [hoverIndex, setHoverIndex] = useState<number | null>(null);

  if (points.length < 2) {
    return <p className="text-sm text-slate-500">Za posledních {hours} hodin nejsou data.</p>;
  }

  const values = points.map((p) => p.value);
  const reference = setback === null ? [] : [setback];
  const min = Math.floor(Math.min(...reference, ...values) / tickStep) * tickStep - tickStep / 2;
  const max = Math.ceil(Math.max(...values) / tickStep) * tickStep + tickStep / 2;
  const ticks = rangeStep(Math.ceil(min / tickStep) * tickStep, max, tickStep);

  const plotWidth = WIDTH - PADDING.left - PADDING.right;
  const plotHeight = HEIGHT - PADDING.top - PADDING.bottom;
  const x = (i: number) => PADDING.left + (i / (points.length - 1)) * plotWidth;
  const y = (v: number) => PADDING.top + (1 - (v - min) / (max - min)) * plotHeight;

  const line = points.map((p, i) => `${i === 0 ? "M" : "L"}${x(i).toFixed(1)},${y(p.value).toFixed(1)}`).join(" ");
  const hovered = hoverIndex !== null ? points[hoverIndex] : null;

  function handlePointer(event: PointerEvent<SVGSVGElement>) {
    const rect = event.currentTarget.getBoundingClientRect();
    const svgX = ((event.clientX - rect.left) / rect.width) * WIDTH;
    const index = Math.round(((svgX - PADDING.left) / plotWidth) * (points.length - 1));
    setHoverIndex(Math.min(points.length - 1, Math.max(0, index)));
  }

  return (
    <div className="relative">
      <svg
        viewBox={`0 0 ${WIDTH} ${HEIGHT}`}
        className="block h-auto w-full touch-none overflow-visible"
        role="img"
        aria-label={`${label} za ${hours} hodin, od ${formatValue(values[0])} do ${formatValue(values[values.length - 1])} ${unit}`}
        onPointerMove={handlePointer}
        onPointerLeave={() => setHoverIndex(null)}
      >
        {ticks.map((tick) => (
          <g key={tick}>
            <line x1={PADDING.left} x2={WIDTH - PADDING.right} y1={y(tick)} y2={y(tick)} className="stroke-slate-100" strokeWidth={1} />
            <text x={PADDING.left - 6} y={y(tick)} dy="0.32em" textAnchor="end" className="fill-slate-400 font-mono text-[10px]">
              {tick}
            </text>
          </g>
        ))}

        {setback !== null && (
          <>
            <line x1={PADDING.left} x2={WIDTH - PADDING.right} y1={y(setback)} y2={y(setback)} className="stroke-emerald-500" strokeWidth={1.5} />
            <text x={PADDING.left + 4} y={y(setback) - 5} className="fill-emerald-800 text-[10px]">
              Doporučený útlum ({setback} {unit})
            </text>
          </>
        )}

        <path d={line} fill="none" className="stroke-hk-blue" strokeWidth={2} strokeLinejoin="round" strokeLinecap="round" />

        {hovered && hoverIndex !== null && (
          <g>
            <line x1={x(hoverIndex)} x2={x(hoverIndex)} y1={PADDING.top} y2={HEIGHT - PADDING.bottom} className="stroke-slate-300" strokeWidth={1} />
            <circle cx={x(hoverIndex)} cy={y(hovered.value)} r={4} className="fill-hk-blue stroke-white" strokeWidth={2} />
          </g>
        )}
      </svg>

      {hovered && hoverIndex !== null && (
        <div
          className="pointer-events-none absolute top-0 rounded-md border border-slate-200 bg-white px-2 py-1 text-[11px] shadow-sm"
          style={{ left: `${(x(hoverIndex) / WIDTH) * 100}%`, transform: hoverIndex > points.length / 2 ? "translateX(-105%)" : "translateX(5%)" }}
        >
          <span className="text-slate-500">{timeFormat.format(new Date(hovered.timestamp))}</span>
          <span className="ml-2 font-mono font-bold text-hk-navy">
            {formatValue(hovered.value)} {unit}
          </span>
        </div>
      )}

      <div className="mt-1.5 flex justify-between pl-7.5 font-mono text-[10px] text-slate-400">
        {[1, 0.75, 0.5, 0.25].map((fraction) => (
          <span key={fraction}>−{hours * fraction} h</span>
        ))}
        <span>nyní</span>
      </div>
    </div>
  );
}

function rangeStep(from: number, to: number, step: number) {
  const result: number[] = [];
  for (let v = from; v <= to; v += step) result.push(v);
  return result;
}

function formatValue(value: number) {
  return value.toLocaleString("cs-CZ", { minimumFractionDigits: 1, maximumFractionDigits: 1 });
}
