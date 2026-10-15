import { STATUS_META, formatNumber } from "@/lib/facility-meta";
import { cn } from "@/lib/utils";
import type { FacilityDetail } from "@/types/api";
import { PanelCard } from "./panel-card";

const dateTime = new Intl.DateTimeFormat("cs-CZ", { dateStyle: "short", timeStyle: "short" });

export function AuditTab({ facility }: { facility: FacilityDetail }) {
  const assessment = facility.assessment;

  if (!assessment) {
    return <p className="mt-4 text-sm text-slate-500">Objekt čeká na první vyhodnocení telemetrie.</p>;
  }

  return (
    <>
      <PanelCard title="Zjištění auditu" meta={`vyhodnoceno ${dateTime.format(new Date(assessment.evaluatedAt))}`}>
        {assessment.findings.length === 0 ? (
          <p className="border-t border-slate-100 pt-3 text-[13px] text-slate-600">
            Bez zjištění — útlum mimo provoz funguje a CO₂ zůstává pod limitem.
          </p>
        ) : (
          <ul>
            {assessment.findings.map((finding) => {
              const Icon = STATUS_META[finding.severity].icon;

              return (
                <li key={finding.title} className="flex gap-3 border-t border-slate-100 py-3.5">
                  <Icon
                    className={cn(
                      "mt-0.5 size-4.5 shrink-0",
                      finding.severity === "Critical" && "text-red-800",
                      finding.severity === "Warning" && "text-amber-800",
                      finding.severity === "Nominal" && "text-slate-500",
                    )}
                    aria-hidden
                  />
                  <div className="min-w-0">
                    <p className="text-[13px] font-bold">{finding.title}</p>
                    <p className="mt-0.5 text-xs leading-relaxed text-slate-600">{finding.detail}</p>
                  </div>
                  <span className="ml-auto font-mono text-[10px] whitespace-nowrap text-slate-400">{finding.tag}</span>
                </li>
              );
            })}
          </ul>
        )}
      </PanelCard>

      <PanelCard title="Ukazatele za 7 dní">
        <dl className="grid grid-cols-2 gap-x-4 gap-y-3">
          <Indicator label="Hodin nad limitem CO₂" value={formatNumber(assessment.co2HoursOverLimit, "h")} />
          <Indicator label="Spotřeba celkem" value={formatNumber(assessment.weeklyEnergyKwh, "kWh")} />
          <Indicator label="Teplota v den útlumu (8–18 h)" value={formatNumber(assessment.freeDayMeanTemperature, "°C")} />
          <Indicator label="Teplota ve 2 h ráno" value={formatNumber(assessment.nightMeanTemperature, "°C")} />
        </dl>
      </PanelCard>

      <InvestmentContext facility={facility} />
    </>
  );
}

function Indicator({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-[11px] text-slate-500">{label}</dt>
      <dd className="mt-0.5 font-mono text-sm font-bold">{value}</dd>
    </div>
  );
}

// Podklady pro pořadí investic z otevřených dat — obsazenost, poptávka, ohrožená oblast, památky
function InvestmentContext({ facility }: { facility: FacilityDetail }) {
  const students = facility.occupancy.filter((o) => o.students !== null && o.schoolYear !== "neuvedeno");
  const newest = students[0];
  const oldest = students[students.length - 1];
  const admissions = facility.occupancy.find((o) => o.demandRatio !== null);
  const dormitory = facility.occupancy.find((o) => o.occupancyPercent !== null);

  const rows: { label: string; value: string }[] = [];

  if (newest && oldest && newest !== oldest) {
    const change = (newest.students ?? 0) - (oldest.students ?? 0);
    rows.push({
      label: `Počet žáků ${oldest.schoolYear} → ${newest.schoolYear}`,
      value: `${formatNumber(oldest.students)} → ${formatNumber(newest.students)} (${change >= 0 ? "+" : ""}${formatNumber(change)})`,
    });
  }

  if (admissions) {
    rows.push({ label: "Přihlášky na jedno místo", value: formatNumber(admissions.demandRatio) });
  }

  if (dormitory) {
    rows.push({ label: "Obsazenost domova mládeže", value: formatNumber(dormitory.occupancyPercent, "%") });
  }

  rows.push({
    label: "Ohrožená oblast (HSOO)",
    value: facility.territory.isThreatenedArea ? facility.territory.threatLevel ?? "ano" : "ne",
  });

  return (
    <PanelCard title="Kontext pro pořadí investic">
      <dl className="divide-y divide-slate-100">
        {rows.map((row) => (
          <div key={row.label} className="flex items-baseline justify-between gap-4 py-2.5">
            <dt className="text-xs text-slate-500">{row.label}</dt>
            <dd className="text-right text-[13px] font-bold">{row.value}</dd>
          </div>
        ))}
      </dl>
      {facility.heritage.constraint && (
        <p className="mt-3 rounded-xl border border-slate-200 bg-slate-50 p-3 text-xs leading-relaxed text-slate-600">
          <span className="font-bold text-hk-navy">{facility.heritage.name}.</span> {facility.heritage.constraint}
        </p>
      )}
    </PanelCard>
  );
}
