import { cn } from "@/lib/utils";
import type { Heatmap } from "@/types/api";

// Sekvenční škála v jednom odstínu (světlá → tmavá), ne duha — dataviz skill
const STEPS = ["bg-hk-blue/8", "bg-hk-blue/25", "bg-hk-blue/45", "bg-hk-blue/65", "bg-hk-blue/90"];

const valueFormat = new Intl.NumberFormat("cs-CZ", { maximumFractionDigits: 1 });

export function HeatmapGrid({ heatmap }: { heatmap: Heatmap }) {
  const { min, max } = heatmap;

  function step(value: number | null) {
    if (value === null || min === null || max === null || max === min) return null;
    const ratio = (value - min) / (max - min);
    return STEPS[Math.min(STEPS.length - 1, Math.floor(ratio * STEPS.length))];
  }

  return (
    <div>
      <div className="flex gap-2">
        <ul className="flex flex-col gap-0.75 pt-px text-[9.5px] text-slate-400">
          {heatmap.days.map((day) => (
            <li key={day.date} className={cn("h-3.5 leading-3.5 whitespace-nowrap", day.isFreeDay && "font-bold text-slate-500")}>
              {day.label}
            </li>
          ))}
        </ul>
        <div className="grid flex-1 grid-cols-24 gap-0.75">
          {heatmap.days.flatMap((day) =>
            day.hours.map((value, hour) => (
              <span
                key={`${day.date}-${hour}`}
                title={`${day.label} ${hour}:00 — ${value === null ? "bez dat" : `${valueFormat.format(value)} ${heatmap.unit}`}`}
                className={cn("block h-3.5 rounded-[3px]", step(value) ?? "bg-slate-100")}
              />
            )),
          )}
        </div>
      </div>

      <div className="mt-3 flex items-center gap-2.5 text-[10px] text-slate-400">
        <span className="font-mono">{min === null ? "—" : valueFormat.format(min)}</span>
        <span className="flex h-1.5 flex-1 overflow-hidden rounded-full">
          {STEPS.map((className) => (
            <span key={className} className={cn("flex-1", className)} />
          ))}
        </span>
        <span className="font-mono">
          {max === null ? "—" : valueFormat.format(max)} {heatmap.unit}
        </span>
      </div>
      <div className="mt-1 flex justify-between pl-14 font-mono text-[10px] text-slate-400">
        <span>0 h</span>
        <span>6 h</span>
        <span>12 h</span>
        <span>18 h</span>
        <span>24 h</span>
      </div>
    </div>
  );
}
