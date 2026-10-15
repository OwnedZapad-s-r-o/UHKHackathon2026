import type { Metadata } from "next";
import Link from "next/link";
import { MapPin } from "lucide-react";
import { DataModeBadge } from "@/components/data-mode-badge";
import { getFacilities, getSession } from "@/lib/api";
import { STATUS_META, formatCzk, formatNumber } from "@/lib/facility-meta";
import { facilityMapHref, readParam, withParams } from "@/lib/search-params";
import { cn } from "@/lib/utils";
import type { FacilityListItem } from "@/types/api";
import { KpiRow } from "../_components/kpi-row";
import { PortfolioReportLink } from "../_components/portfolio-report-link";
import { TopBar } from "../_components/top-bar";

export const metadata: Metadata = {
  title: "Anomálie · SmartAudit",
};

// Nejdřív kritické, v rámci stavu podle odhadované ztráty
const STATUS_ORDER = { Critical: 0, Warning: 1, Nominal: 2 } as const;

export default async function AnomaliesPage({ searchParams }: PageProps<"/anomalie">) {
  const params = await searchParams;
  const onlyCritical = readParam(params, "kriticke") === "1";

  const [facilities, session] = await Promise.all([getFacilities(), getSession()]);
  // Správce řeší zjištění svých budov, „anomálie“ je pohled kraje
  const title = session?.role === "Spravce" ? "Zjištění" : "Anomálie";
  const anomalies = facilities
    .filter((f) => (onlyCritical ? f.status === "Critical" : f.status !== "Nominal"))
    .toSorted((a, b) => STATUS_ORDER[a.status] - STATUS_ORDER[b.status] || (b.annualLossCzk ?? 0) - (a.annualLossCzk ?? 0));
  const nominalCount = facilities.filter((f) => f.status === "Nominal").length;

  return (
    <>
      <TopBar title={title}>
        <PortfolioReportLink onlyIssues />
      </TopBar>

      <main className="min-h-0 flex-1 overflow-y-auto p-6">
        <div className="flex flex-wrap items-end gap-4">
          <div>
            <h1 className="text-[32px] leading-tight font-bold tracking-tight">{title}</h1>
            <p className="mt-1.5 text-sm text-slate-600">
              Zjištění z pre-auditu otevřených dat a telemetrie (živé IQRF nebo fyzikální model) seřazená podle odhadovaného finančního dopadu.
            </p>
          </div>
          <Link
            href={withParams("/anomalie", params, { kriticke: onlyCritical ? null : "1" })}
            aria-pressed={onlyCritical}
            className={cn(
              "ml-auto inline-flex h-10 items-center gap-2 rounded-lg border px-3.5 text-sm transition-colors",
              onlyCritical ? "border-hk-blue bg-hk-blue text-white" : "border-slate-200 bg-white hover:bg-slate-100",
            )}
          >
            Jen kritické
          </Link>
        </div>

        <KpiRow facilities={facilities} />

        <ol className="mt-4 space-y-3">
          {anomalies.map((facility, index) => (
            <AnomalyCard key={facility.id} facility={facility} index={index} />
          ))}
        </ol>

        <p className="mt-4 text-xs text-slate-500">
          Ostatní objekty ({nominalCount}) jsou v normě. Stav vychází deterministicky z otevřených dat (ČÚZK, Data KHK, Registr smluv, Open-Meteo) a z měření; vyhodnocuje se průběžně po každém kroku.
        </p>
      </main>
    </>
  );
}

function AnomalyCard({ facility, index }: { facility: FacilityListItem; index: number }) {
  const status = STATUS_META[facility.status];
  const StatusIcon = status.icon;

  return (
    <li className="flex gap-4 rounded-2xl border border-slate-200 bg-white p-4.5">
      <span className="font-mono text-[13px] text-slate-400">{String(index + 1).padStart(2, "0")}</span>
      <div className="min-w-0 flex-1">
        <div className="flex flex-wrap items-center gap-2">
          <span className={cn("inline-flex h-6 items-center gap-1.5 rounded-full border px-2.5 text-[11px] font-bold", status.pill)}>
            <StatusIcon className="size-3" aria-hidden />
            {status.label}
          </span>
          <span className="font-mono text-[11px] text-slate-500">
            {facility.code} · {facility.districtName}
          </span>
        </div>
        <h2 className="mt-2 text-lg font-bold tracking-tight">{facility.name}</h2>
        <p className="mt-1 text-[13px] text-slate-600">{facility.headline}</p>
        <div className="mt-2 flex flex-wrap items-center gap-2">
          <DataModeBadge mode={facility.liveMeasurement ? "LiveIqrf" : "OpenDataModel"} compact />
          {facility.preAuditScore !== null && (
            <span className="font-mono text-[11px] text-slate-500">
              pre-audit {facility.preAuditScore}/100{facility.preAuditDriver && ` · ${facility.preAuditDriver.toLowerCase()}`}
            </span>
          )}
        </div>
      </div>
      <div className="flex shrink-0 flex-col items-end justify-between gap-3">
        {facility.annualLossCzk !== null && (
          <div className="text-right">
            <p className="text-[10.5px] text-slate-500">Dopad na rok</p>
            <p className="font-mono text-base font-bold text-red-800">{facility.annualLossCzk > 0 ? formatCzk(facility.annualLossCzk) : "—"}</p>
            {facility.iqrfPaybackMonths !== null && (
              <p className="mt-0.5 font-mono text-[11px] font-bold text-emerald-700" title="Návratnost sady čidel MICRORISC IQRF">
                IQRF návratnost {formatNumber(facility.iqrfPaybackMonths)} měs.
              </p>
            )}
          </div>
        )}
        <Link
          href={facilityMapHref(facility, "audit")}
          className="inline-flex h-9 items-center gap-1.5 rounded-lg border border-slate-200 px-3 text-[13px] transition-colors hover:bg-slate-100"
        >
          <MapPin className="size-3.5" aria-hidden />
          V mapě
        </Link>
      </div>
    </li>
  );
}
