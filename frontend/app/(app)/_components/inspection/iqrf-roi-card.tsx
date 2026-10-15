import { Cpu } from "lucide-react";
import { formatCzk, formatNumber } from "@/lib/facility-meta";
import { cn } from "@/lib/utils";
import type { IqrfRoi } from "@/types/api";
import { PanelCard } from "./panel-card";

// Návratnost osazení budovy sítí MICRORISC IQRF — kolik sada stojí a za kolik měsíců se vrátí z úspory
export function IqrfRoiCard({ roi }: { roi: IqrfRoi }) {
  const moneyHidden = roi.estimatedAnnualSavingsCzk === null;
  const fast = roi.paybackMonths !== null && roi.paybackMonths <= 6;

  return (
    <PanelCard title="Návratnost nasazení čidel MICRORISC IQRF" meta={`${roi.gateways}× brána · ${roi.sensors}× čidlo`}>
      <div className="grid grid-cols-3 gap-2.5">
        <Tile label="Sada IQRF" value={formatCzk(roi.estimatedHardwareCostCzk)} note="bez sekání kabelů" />
        <Tile
          label="Roční úspora"
          value={moneyHidden ? "—" : formatCzk(roi.estimatedAnnualSavingsCzk)}
          note={moneyHidden ? "jen pro kraj a správce" : "regulace + telemetrie"}
        />
        <Tile
          label="Návratnost"
          value={roi.paybackMonths === null ? "—" : `${formatNumber(roi.paybackMonths)} měs.`}
          note={moneyHidden ? "jen pro kraj a správce" : fast ? "pod půl roku" : "z odhadu úspory"}
          highlight={fast}
        />
      </div>
      <p className="mt-3 flex items-start gap-2 text-[11.5px] leading-snug text-slate-500">
        <Cpu className="mt-0.5 size-3.5 shrink-0 text-hk-blue" aria-hidden />
        <span>
          {roi.gateways}× IQRF Gateway IQD-GW04 à {formatCzk(roi.gatewayUnitCzk)} + {roi.sensors}× bezdrátové čidlo teploty/CO₂ à {formatCzk(roi.sensorUnitCzk)} podle
          podlahové plochy z ČÚZK. Úspora: {roi.savingsBasis}.
        </span>
      </p>
    </PanelCard>
  );
}

function Tile({ label, value, note, highlight = false }: { label: string; value: string; note: string; highlight?: boolean }) {
  return (
    <div className={cn("rounded-xl border p-3", highlight ? "border-emerald-200 bg-emerald-50" : "border-slate-200 bg-white")}>
      <p className="text-[11px] text-slate-500">{label}</p>
      <p className={cn("mt-1 font-mono text-[17px] font-bold", highlight && "text-emerald-800")}>{value}</p>
      <p className="mt-0.5 text-[10.5px] text-slate-400">{note}</p>
    </div>
  );
}
