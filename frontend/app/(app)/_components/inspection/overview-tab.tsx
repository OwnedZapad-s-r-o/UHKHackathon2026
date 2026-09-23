import { Activity } from "lucide-react";
import { STATUS_META, formatCzk, formatNumber } from "@/lib/facility-meta";
import { cn } from "@/lib/utils";
import type { FacilityDetail } from "@/types/api";
import { StatTile } from "./panel-card";

const CO2_LIMIT = 1500;

export function OverviewTab({ facility }: { facility: FacilityDetail }) {
  const assessment = facility.assessment;

  if (!assessment) {
    return <p className="mt-4 text-sm text-slate-500">Objekt čeká na první vyhodnocení telemetrie.</p>;
  }

  const status = STATUS_META[assessment.status];
  const StatusIcon = status.icon;
  const moneyHidden = assessment.annualLossCzk === null;

  return (
    <>
      <div className="mt-3.5 grid grid-cols-2 gap-2.5">
        <StatTile label="Aktuální teplota" value={formatNumber(assessment.temperature, "°C")} note="průměr zón · útlum 18,0 °C" />
        <StatTile
          label="CO₂ v nejhorší zóně"
          value={formatNumber(assessment.co2, "ppm")}
          note={`limit ${formatNumber(CO2_LIMIT)} ppm`}
          tone={assessment.co2 !== null && assessment.co2 > CO2_LIMIT ? "warning" : "default"}
        />
        <StatTile label="Příkon" value={formatNumber(assessment.power, "kWh/h")} note={`${formatNumber(assessment.weeklyEnergyKwh)} kWh za 7 dní`} />
        <StatTile
          label="Odhad roční ztráty"
          value={moneyHidden ? "—" : formatCzk(assessment.annualLossCzk)}
          note={moneyHidden ? "jen pro kraj a správce budovy" : `potenciál úspory ${formatNumber(assessment.savingPercent)} %`}
          tone={!moneyHidden && (assessment.annualLossCzk ?? 0) > 0 ? "critical" : "default"}
        />
      </div>

      <section className={cn("mt-3.5 rounded-2xl border p-4.5", status.pill)}>
        <div className="flex items-center gap-2">
          <StatusIcon className="size-4.5" aria-hidden />
          <h3 className="text-sm font-bold">{assessment.headline}</h3>
          <span className="ml-auto flex items-center gap-1 font-mono text-[10px] opacity-70">
            <Activity className="size-3" aria-hidden />
            pravidla nad 7 dny telemetrie
          </span>
        </div>
        <p className="mt-3 text-[13px] leading-normal">{assessment.summary}</p>
        {!moneyHidden && (assessment.annualLossCzk ?? 0) > 0 && (
          <div className="mt-3.5 flex gap-4 border-t border-current/15 pt-3.5">
            <div>
              <p className="text-[10.5px] opacity-70">Odhad roční ztráty</p>
              <p className="mt-0.5 font-mono text-[15px] font-bold">{formatCzk(assessment.annualLossCzk)}</p>
            </div>
            <div>
              <p className="text-[10.5px] opacity-70">Potenciál úspory</p>
              <p className="mt-0.5 font-mono text-[15px] font-bold">{formatNumber(assessment.savingPercent)} %</p>
            </div>
          </div>
        )}
      </section>

      <ContextList facility={facility} />
    </>
  );
}

// Souvislosti z dalších otevřených sad — proč na objektu záleží víc nebo míň
function ContextList({ facility }: { facility: FacilityDetail }) {
  const rows: { label: string; value: string }[] = [];

  rows.push({
    label: "Hospodářsky a sociálně ohrožená obec",
    value: facility.territory.isThreatenedArea ? `ano — ${facility.territory.threatLevel ?? "ohrožená"}` : "ne",
  });

  if (facility.heritage.status !== "None") {
    rows.push({ label: "Památková ochrana", value: facility.heritage.name ?? "ano" });
  }

  if (facility.building?.constructionYear) {
    rows.push({ label: "Rok výstavby (ČÚZK)", value: `${facility.building.constructionYear} · ${facility.building.energyCategory}` });
  }

  if (facility.municipalityContact?.dataBox) {
    rows.push({ label: `Datová schránka obce ${facility.municipalityContact.municipality}`, value: facility.municipalityContact.dataBox });
  }

  return (
    <dl className="mt-3.5 divide-y divide-slate-100 rounded-2xl border border-slate-200 bg-white px-4.5">
      {rows.map((row) => (
        <div key={row.label} className="flex items-baseline justify-between gap-4 py-2.5">
          <dt className="text-xs text-slate-500">{row.label}</dt>
          <dd className="text-right text-[13px] font-bold">{row.value}</dd>
        </div>
      ))}
    </dl>
  );
}
