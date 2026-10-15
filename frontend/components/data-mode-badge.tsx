import { DraftingCompass, Radio } from "lucide-react";
import { cn } from "@/lib/utils";
import type { DataMode } from "@/types/api";

// Popisek, odkud hodnoty budovy pocházejí — v tooltipu u odznaku a v reportech
export const MODEL_EXPLANATION =
  "Hodnoty jsou deterministicky počítány z reálné podlahové plochy a stáří budovy (ČÚZK INSPIRE), kapacity osob a investic (Data KHK), cen energií (Registr smluv) a živého počasí (Open-Meteo). Po spárování brány MICRORISC IQRF se model automaticky přepne na živou telemetrii.";

export const LIVE_EXPLANATION =
  "Teplotu a CO₂ měří bezdrátová čidla MICRORISC IQRF; brána je posílá přes MQTT (IQRF Standard Sensor). Místnosti bez čidla dál počítá fyzikální model z otevřených dat.";

const LABELS: Record<DataMode, { full: string; compact: string }> = {
  LiveIqrf: { full: "Živé měření IQRF (MQTT Standard Sensor)", compact: "Živé měření IQRF" },
  OpenDataModel: { full: "Fyzikální model z otevřených dat (ČÚZK + Data KHK + Open-Meteo)", compact: "Model z otevřených dat" },
};

// Dva režimy dat u každé budovy: živá telemetrie IQRF, nebo fyzikální model z otevřených dat (digitální dvojče)
export function DataModeBadge({ mode, compact = false, className }: { mode: DataMode; compact?: boolean; className?: string }) {
  const live = mode === "LiveIqrf";
  const Icon = live ? Radio : DraftingCompass;
  const label = compact ? LABELS[mode].compact : LABELS[mode].full;

  return (
    <span
      title={live ? LIVE_EXPLANATION : MODEL_EXPLANATION}
      className={cn(
        "inline-flex max-w-full items-center gap-1.5 rounded-full border px-2 py-0.5 text-[11px] font-bold",
        live ? "border-emerald-300 bg-emerald-50 text-emerald-800" : "border-hk-blue/25 bg-[#EFF4FA] text-hk-blue",
        className,
      )}
    >
      <Icon className={cn("size-3 shrink-0", live && "animate-pulse")} aria-hidden />
      <span className="truncate">{label}</span>
    </span>
  );
}
