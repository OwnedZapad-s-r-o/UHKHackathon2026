"use client";

import { useState, type PointerEvent } from "react";
import { STATUS_META } from "@/lib/facility-meta";
import { hoverGuideItem } from "@/lib/guide-events";
import { cn } from "@/lib/utils";
import type { GuideChart as GuideChartData } from "@/types/api";

// Barvy grafů průvodce: hlavní řada v modré HK, srovnávací řada tlumeně šedá; hranice limitů v barvách stavu
// (jantar = vyvětrat, červená = limit) vždy s textovým popiskem — barva nikdy nenese význam sama
const PRIMARY = "#1e3e62";
const SECONDARY = "#94a3b8";
const THRESHOLD_COLORS = { warning: "#f59e0b", critical: "#ef4444", neutral: "#64748b" } as const;

const number = new Intl.NumberFormat("cs-CZ", { maximumFractionDigits: 1 });
const timeFormat = new Intl.DateTimeFormat("cs-CZ", { weekday: "short", hour: "numeric", minute: "2-digit", timeZone: "Europe/Prague" });
const dayFormat = new Intl.DateTimeFormat("cs-CZ", { weekday: "short", day: "numeric", month: "numeric", timeZone: "Europe/Prague" });
const hourFormat = new Intl.DateTimeFormat("cs-CZ", { hour: "numeric", hourCycle: "h23", timeZone: "Europe/Prague" });

type GuideChartProps = {
  chart: GuideChartData;
  onSelectFacility: (facilityId: string) => void;
};

// Graf v odpovědi průvodce: pořadí objektů (vodorovné sloupce) nebo průběh měření (čáry za 48 h)
export function GuideChart({ chart, onSelectFacility }: GuideChartProps) {
  return (
    <figure className="rounded-xl border border-slate-200 bg-white px-3 pt-2.5 pb-2">
      <figcaption className="flex items-baseline justify-between gap-2">
        <span className="text-[12px] font-bold text-hk-navy">{chart.title}</span>
        <span className="shrink-0 text-[10.5px] text-slate-400">{chart.unit}</span>
      </figcaption>
      {chart.kind === "Ranking" ? <RankingChart chart={chart} onSelect={onSelectFacility} /> : <SeriesChart chart={chart} />}
      {chart.note && <p className="mt-1.5 text-[10.5px] text-slate-400">{chart.note}</p>}
    </figure>
  );
}

// Vodorovné sloupce od nuly, hodnota vpravo textem. Najetí na řádek rozsvítí objekt v mapě, klik ho otevře.
function RankingChart({ chart, onSelect }: { chart: GuideChartData; onSelect: (facilityId: string) => void }) {
  const max = Math.max(...chart.bars.map((b) => b.value), ...chart.thresholds.map((t) => t.value * 1.05));

  return (
    <ol className="mt-2 space-y-1">
      {chart.bars.map((bar) => (
        <li key={bar.facilityId}>
          <button
            type="button"
            onClick={() => onSelect(bar.facilityId)}
            onMouseEnter={() => hoverGuideItem(bar.facilityId)}
            onMouseLeave={() => hoverGuideItem(null)}
            onFocus={() => hoverGuideItem(bar.facilityId)}
            onBlur={() => hoverGuideItem(null)}
            className="group grid w-full grid-cols-[1rem_minmax(0,7.5rem)_1fr_auto] items-center gap-2 rounded-md px-1 py-1 text-left hover:bg-slate-50"
            title={`${bar.label}: ${number.format(bar.value)} ${chart.unit}`}
          >
            <span className="text-[10.5px] font-bold text-slate-400 tabular-nums">{bar.rank}</span>
            <span className="flex min-w-0 items-center gap-1.5">
              <span className={cn("size-1.5 shrink-0 rounded-full", STATUS_META[bar.status].dot)} aria-hidden />
              <span className="truncate text-[11.5px] text-slate-700 group-hover:text-hk-navy">{bar.label}</span>
            </span>
            <span className="relative h-2.5 rounded-full bg-slate-100">
              <span
                className="animate-bar-grow absolute inset-y-0 left-0 rounded-full transition-[filter] group-hover:brightness-125"
                style={{ width: `${Math.max(2, (bar.value / max) * 100)}%`, background: PRIMARY, animationDelay: `${bar.rank * 50}ms` }}
              />
              {chart.thresholds.map((threshold) => (
                <span
                  key={threshold.label}
                  className="absolute -inset-y-0.5 w-0 border-l border-dashed"
                  style={{ left: `${(threshold.value / max) * 100}%`, borderColor: THRESHOLD_COLORS[threshold.tone] }}
                  aria-hidden
                />
              ))}
            </span>
            <span className="text-right text-[11px] font-bold text-hk-navy tabular-nums">{formatValue(bar.value, chart.unit)}</span>
          </button>
        </li>
      ))}
      {chart.thresholds.length > 0 && <ThresholdLegend chart={chart} />}
    </ol>
  );
}

const WIDTH = 332;
const HEIGHT = 150;
const PAD = { top: 10, right: 8, bottom: 20, left: 34 };

// Průběh za 48 h: jedna osa (jedna veličina), hranice limitů jako přerušované čáry, nad čarou pruh s křížovým kurzorem
function SeriesChart({ chart }: { chart: GuideChartData }) {
  const [hover, setHover] = useState<number | null>(null);
  const primary = chart.lines.find((l) => l.primary) ?? chart.lines[0];
  if (!primary || primary.points.length < 2) return null;

  const times = primary.points.map((p) => new Date(p.time).getTime());
  const values = chart.lines.flatMap((l) => l.points.map((p) => p.value)).concat(chart.thresholds.map((t) => t.value));
  const [yMin, yMax] = niceRange(Math.min(...values), Math.max(...values));
  const [tMin, tMax] = [Math.min(...times), Math.max(...times)];

  const x = (time: number) => PAD.left + ((time - tMin) / Math.max(1, tMax - tMin)) * (WIDTH - PAD.left - PAD.right);
  const y = (value: number) => PAD.top + (1 - (value - yMin) / Math.max(1e-9, yMax - yMin)) * (HEIGHT - PAD.top - PAD.bottom);
  const path = (points: { time: string; value: number }[]) =>
    points.map((p, i) => `${i === 0 ? "M" : "L"}${x(new Date(p.time).getTime()).toFixed(1)},${y(p.value).toFixed(1)}`).join("");

  const yTicks = [yMin, (yMin + yMax) / 2, yMax];
  const midnights = dayStarts(tMin, tMax);

  function handleMove(event: PointerEvent<SVGRectElement>) {
    const box = event.currentTarget.getBoundingClientRect();
    const time = tMin + ((event.clientX - box.left) / box.width) * (tMax - tMin);
    let nearest = 0;
    for (let i = 1; i < times.length; i++) if (Math.abs(times[i] - time) < Math.abs(times[nearest] - time)) nearest = i;
    setHover(nearest);
  }

  const hoverTime = hover !== null ? times[hover] : null;
  const hoverValues = hoverTime !== null ? chart.lines.map((line) => ({ line, point: line.points.find((p) => new Date(p.time).getTime() === hoverTime) })) : [];

  return (
    <div className="relative mt-1.5">
      {chart.lines.length > 1 && (
        <ul className="mb-1 flex flex-wrap gap-x-3 gap-y-0.5 text-[10.5px] text-slate-600" aria-label="Legenda">
          {chart.lines.map((line) => (
            <li key={line.label} className="flex items-center gap-1.5">
              <span className="h-0.5 w-3 rounded-full" style={{ background: line.primary ? PRIMARY : SECONDARY }} aria-hidden />
              {line.label}
            </li>
          ))}
        </ul>
      )}

      <svg viewBox={`0 0 ${WIDTH} ${HEIGHT}`} className="h-auto w-full overflow-visible" role="img" aria-label={`${chart.title}, ${primary.label}`}>
        {yTicks.map((tick) => (
          <g key={tick}>
            <line x1={PAD.left} x2={WIDTH - PAD.right} y1={y(tick)} y2={y(tick)} stroke="#e2e8f0" strokeWidth={1} />
            <text x={PAD.left - 5} y={y(tick)} dy="0.32em" textAnchor="end" className="fill-slate-400 text-[9px] tabular-nums">
              {number.format(Math.round(tick))}
            </text>
          </g>
        ))}
        {midnights.map((time) => (
          <g key={time}>
            <line x1={x(time)} x2={x(time)} y1={PAD.top} y2={HEIGHT - PAD.bottom} stroke="#f1f5f9" strokeWidth={1} />
            <text x={x(time) + 3} y={HEIGHT - 6} className="fill-slate-400 text-[9px]">
              {dayFormat.format(time)}
            </text>
          </g>
        ))}
        {chart.thresholds.map((threshold) => (
          <g key={threshold.label}>
            <line x1={PAD.left} x2={WIDTH - PAD.right} y1={y(threshold.value)} y2={y(threshold.value)} stroke={THRESHOLD_COLORS[threshold.tone]} strokeWidth={1} strokeDasharray="4 3" />
            <text x={WIDTH - PAD.right} y={y(threshold.value) - 3} textAnchor="end" className="fill-slate-500 text-[9px]">
              {threshold.label}
            </text>
          </g>
        ))}
        {chart.lines
          .toSorted((a, b) => Number(a.primary) - Number(b.primary))
          .map((line) => (
            <path
              key={line.label}
              d={path(line.points)}
              fill="none"
              stroke={line.primary ? PRIMARY : SECONDARY}
              strokeWidth={line.primary ? 2 : 1.5}
              strokeLinejoin="round"
              strokeLinecap="round"
              className="animate-line-draw"
              pathLength={1}
            />
          ))}
        {hoverTime !== null && (
          <g>
            <line x1={x(hoverTime)} x2={x(hoverTime)} y1={PAD.top} y2={HEIGHT - PAD.bottom} stroke="#94a3b8" strokeWidth={1} />
            {hoverValues.map(({ line, point }) =>
              point ? <circle key={line.label} cx={x(hoverTime)} cy={y(point.value)} r={4} fill={line.primary ? PRIMARY : SECONDARY} stroke="#fff" strokeWidth={2} /> : null,
            )}
          </g>
        )}
        {/* Zóna pro myš je větší než samotné čáry — kurzor najde nejbližší hodinu */}
        <rect
          x={PAD.left}
          y={PAD.top}
          width={WIDTH - PAD.left - PAD.right}
          height={HEIGHT - PAD.top - PAD.bottom}
          fill="transparent"
          onPointerMove={handleMove}
          onPointerLeave={() => setHover(null)}
        />
      </svg>

      {hoverTime !== null && (
        <div
          className="pointer-events-none absolute top-6 z-10 rounded-lg border border-slate-200 bg-white px-2.5 py-1.5 text-[11px] shadow-md"
          style={{ left: `${(x(hoverTime) / WIDTH) * 100}%`, transform: x(hoverTime) > WIDTH / 2 ? "translateX(calc(-100% - 8px))" : "translateX(8px)" }}
        >
          <p className="font-bold text-hk-navy">{timeFormat.format(hoverTime)}</p>
          {hoverValues.map(({ line, point }) => (
            <p key={line.label} className="flex items-center gap-1.5 whitespace-nowrap text-slate-600">
              <span className="h-0.5 w-2.5 rounded-full" style={{ background: line.primary ? PRIMARY : SECONDARY }} aria-hidden />
              {line.label}: <b className="text-hk-navy tabular-nums">{point ? formatValue(point.value, chart.unit) : "—"}</b>
            </p>
          ))}
        </div>
      )}

      {chart.lines.length === 1 && <p className="mt-0.5 text-[10.5px] text-slate-500">{primary.label}</p>}

      <details className="mt-1 text-[10.5px] text-slate-500">
        <summary className="cursor-pointer select-none hover:text-hk-navy">Zobrazit jako tabulku</summary>
        <div className="mt-1 max-h-36 overflow-y-auto">
          <table className="w-full text-left tabular-nums">
            <thead>
              <tr>
                <th className="py-0.5 font-bold">Čas</th>
                {chart.lines.map((line) => (
                  <th key={line.label} className="py-0.5 font-bold">
                    {line.label}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {primary.points.map((point) => (
                <tr key={point.time}>
                  <td className="py-0.5">{timeFormat.format(new Date(point.time))}</td>
                  {chart.lines.map((line) => (
                    <td key={line.label} className="py-0.5">
                      {formatValue(line.points.find((p) => p.time === point.time)?.value, chart.unit)}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </details>
    </div>
  );
}

function ThresholdLegend({ chart }: { chart: GuideChartData }) {
  return (
    <li className="flex flex-wrap gap-x-3 pt-1 pl-1 text-[10.5px] text-slate-500">
      {chart.thresholds.map((threshold) => (
        <span key={threshold.label} className="flex items-center gap-1.5">
          <span className="h-2.5 w-0 border-l border-dashed" style={{ borderColor: THRESHOLD_COLORS[threshold.tone] }} aria-hidden />
          {threshold.label} ({number.format(threshold.value)} {chart.unit})
        </span>
      ))}
    </li>
  );
}

function formatValue(value: number | undefined, unit: string) {
  if (value === undefined) return "—";
  if (unit === "Kč") return `${number.format(Math.round(value / 1000))} tis.`;
  return unit === "°C" ? `${number.format(value)} °C` : number.format(Math.round(value));
}

// Osa od „hezkého“ čísla k „hezkému“ — s rezervou, aby čára nelepila na okraj
function niceRange(min: number, max: number): [number, number] {
  const span = Math.max(1, max - min);
  const step = 10 ** Math.floor(Math.log10(span)) / 2;
  return [Math.floor((min - span * 0.08) / step) * step, Math.ceil((max + span * 0.08) / step) * step];
}

// Půlnoci v pražském čase mezi začátkem a koncem — svislé dělítko dnů
function dayStarts(from: number, to: number) {
  const result: number[] = [];
  for (let t = Math.ceil(from / 3600_000) * 3600_000; t <= to; t += 3600_000) {
    if (hourFormat.format(t) === "0") result.push(t);
  }
  return result;
}
