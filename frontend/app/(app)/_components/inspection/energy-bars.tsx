import type { TelemetryPoint } from "@/types/api";

const dayFormat = new Intl.DateTimeFormat("cs-CZ", { weekday: "short", timeZone: "Europe/Prague" });
const valueFormat = new Intl.NumberFormat("cs-CZ", { maximumFractionDigits: 0 });

// Sloupce ≤ 24 px, zaoblený konec 4 px, hodnota na vrcholu, popisek dne pod osou (dataviz skill)
export function EnergyBars({ points }: { points: TelemetryPoint[] }) {
  if (points.length === 0) {
    return <p className="text-sm text-slate-500">Za posledních 7 dní nejsou data.</p>;
  }

  const max = Math.max(...points.map((p) => p.value));

  return (
    <ol className="flex h-28 items-end gap-2.5">
      {points.map((point, index) => {
        const isToday = index === points.length - 1;
        const heightPercent = max > 0 ? Math.max(2, (point.value / max) * 100) : 2;
        const day = dayFormat.format(new Date(point.timestamp));

        return (
          <li key={point.timestamp} className="flex h-full flex-1 flex-col items-center justify-end gap-1.5" title={`${day}: ${valueFormat.format(point.value)} kWh${isToday ? " (zatím)" : ""}`}>
            <span className="font-mono text-[9.5px] text-slate-500">{valueFormat.format(point.value)}</span>
            <span className={`block w-full max-w-6 rounded-t ${isToday ? "bg-hk-blue/40" : "bg-hk-blue"}`} style={{ height: `${heightPercent}%` }} />
            <span className="text-[10px] text-slate-400">{isToday ? "dnes" : day}</span>
          </li>
        );
      })}
    </ol>
  );
}
