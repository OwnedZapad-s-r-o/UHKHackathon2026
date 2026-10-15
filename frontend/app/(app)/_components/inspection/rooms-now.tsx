import { LiveBadge } from "@/components/live-badge";
import { AIR_META, airLevel } from "@/lib/air-quality";
import { formatNumber } from "@/lib/facility-meta";
import { cn } from "@/lib/utils";
import type { BuildingSection } from "@/types/api";
import { PanelCard } from "./panel-card";

// Poslední hodnoty po místnostech — prostý seznam místo schématu budovy; místnost se skutečným čidlem má odznak živě
export function RoomsNow({ section }: { section: BuildingSection }) {
  if (section.zones.length === 0) return null;

  const liveCount = section.zones.filter((z) => z.live).length;

  return (
    <PanelCard title="Místnosti teď" meta={liveCount > 0 ? `${liveCount} se skutečným čidlem` : "modelová data"}>
      <ul className="divide-y divide-slate-100">
        {section.zones.map((zone) => {
          const level = airLevel(zone.co2);

          return (
            <li key={zone.id} className="flex items-center gap-3 py-2">
              <span className={cn("size-2 shrink-0 rounded-full", level ? AIR_META[level].dot : "bg-slate-300")} aria-hidden />
              <span className="min-w-0 flex-1">
                <span className="flex items-center gap-1.5">
                  <span className="truncate text-[13px] font-bold">{zone.name}</span>
                  {zone.live && <LiveBadge />}
                </span>
                <span className="block text-[11px] text-slate-500">
                  {zone.floor === 0 ? "přízemí" : `${zone.floor}. NP`}
                  {level && ` · ${AIR_META[level].label.toLowerCase()}`}
                </span>
              </span>
              <span className="shrink-0 text-right font-mono text-[12px] tabular-nums">
                <span className="block">{formatNumber(zone.temperature, "°C")}</span>
                <span className={cn("block", level ? AIR_META[level].text : "text-slate-500")}>
                  {formatNumber(zone.co2, "ppm")}
                  {zone.humidity !== null && <span className="text-slate-500"> · {formatNumber(zone.humidity, "%")}</span>}
                </span>
              </span>
            </li>
          );
        })}
      </ul>
    </PanelCard>
  );
}
