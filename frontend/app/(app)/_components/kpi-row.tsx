import { formatCzk } from "@/lib/facility-meta";
import { cn } from "@/lib/utils";
import type { FacilityListItem } from "@/types/api";

// Čtyři dlaždice z prototypu — sdílí je Portfolio i Anomálie
export function KpiRow({ facilities }: { facilities: FacilityListItem[] }) {
  const critical = facilities.filter((f) => f.status === "Critical").length;
  const warning = facilities.filter((f) => f.status === "Warning").length;
  const moneyVisible = facilities.some((f) => f.annualLossCzk !== null);
  const loss = facilities.reduce((sum, f) => sum + (f.annualLossCzk ?? 0), 0);

  return (
    <div className="mt-5 grid grid-cols-2 gap-3 lg:grid-cols-4">
      <Tile label="Objektů v evidenci" value={facilities.length.toLocaleString("cs-CZ")} />
      <Tile label="Kritických anomálií" value={critical.toString()} className="text-red-800" />
      <Tile label="Varování" value={warning.toString()} className="text-amber-800" />
      <Tile
        label="Součet odhadovaných ztrát / rok"
        value={moneyVisible ? formatCzk(loss) : "jen pro kraj"}
        className={moneyVisible ? "font-mono text-red-800" : "text-base text-slate-400"}
      />
    </div>
  );
}

function Tile({ label, value, className }: { label: string; value: string; className?: string }) {
  return (
    <div className="rounded-xl border border-slate-200 bg-white p-4">
      <p className="text-[11px] text-slate-500">{label}</p>
      <p className={cn("mt-1.5 text-2xl font-bold tracking-tight", className)}>{value}</p>
    </div>
  );
}
