import Link from "next/link";
import { ChevronRight, ExternalLink, FileDown, X } from "lucide-react";
import { getFacilityDetail } from "@/lib/api";
import { STATUS_META, formatMillionsCzk, formatNumber } from "@/lib/facility-meta";
import { withParams, type RawSearchParams } from "@/lib/search-params";
import { cn } from "@/lib/utils";
import type { FacilityDetail } from "@/types/api";
import { AuditTab } from "./audit-tab";
import { BuildingTab } from "./building-tab";
import { OverviewTab } from "./overview-tab";
import { TelemetryTab } from "./telemetry-tab";

export const PANEL_TABS = [
  { key: "prehled", label: "Přehled" },
  { key: "telemetrie", label: "Telemetrie" },
  { key: "audit", label: "Audit" },
  { key: "budova", label: "Budova" },
] as const;

export type PanelTab = (typeof PANEL_TABS)[number]["key"];

type InspectionPanelProps = {
  facilityId: string;
  tab: PanelTab;
  searchParams: RawSearchParams;
};

export async function InspectionPanel({ facilityId, tab, searchParams }: InspectionPanelProps) {
  const facility = await getFacilityDetail(facilityId);

  return (
    <div className="p-5">
      <div className="mb-3 flex items-center gap-2 text-[11px] text-slate-500">
        Inspekční panel
        <ChevronRight className="size-3" aria-hidden />
        <span className="text-hk-navy">{facility.address.district}</span>
        <a
          href={`/reporty/objekt/${facility.id}`}
          title="Stáhnout PDF audit objektu"
          data-tour="panel-pdf"
          className="ml-auto inline-flex h-8 items-center gap-1.5 rounded-lg border border-slate-200 bg-white px-2.5 text-[12px] font-bold text-hk-navy transition-colors hover:bg-slate-100"
        >
          <FileDown className="size-3.5" aria-hidden />
          PDF
        </a>
        <Link
          href={withParams("/mapa", searchParams, { objekt: null, panel: null })}
          scroll={false}
          aria-label="Zrušit výběr objektu"
          title="Zrušit výběr objektu (Esc)"
          className="flex size-8 items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-500 transition-colors hover:bg-slate-100 hover:text-hk-navy"
        >
          <X className="size-4" />
        </Link>
      </div>

      <FacilityHeader facility={facility} />

      <nav data-tour="panel-zalozky" className="mt-4 flex gap-0.5 border-b border-slate-200" aria-label="Záložky objektu">
        {PANEL_TABS.map(({ key, label }) => (
          <Link
            key={key}
            href={withParams("/mapa", searchParams, { panel: key })}
            scroll={false}
            aria-current={tab === key ? "page" : undefined}
            className={cn(
              "px-3 pb-2.5 text-[13px] transition-colors hover:text-black",
              tab === key ? "font-bold text-black shadow-[inset_0_-2px_0_0_var(--color-hk-red)]" : "text-slate-500",
            )}
          >
            {label}
          </Link>
        ))}
      </nav>

      {tab === "prehled" && <OverviewTab facility={facility} />}
      {tab === "telemetrie" && <TelemetryTab facility={facility} />}
      {tab === "audit" && <AuditTab facility={facility} />}
      {tab === "budova" && <BuildingTab facility={facility} />}
    </div>
  );
}

function FacilityHeader({ facility }: { facility: FacilityDetail }) {
  const status = facility.assessment?.status ?? "Nominal";
  const StatusIcon = STATUS_META[status].icon;
  const capacity = capacityOf(facility);
  const grants = facility.territory.orpGrants;

  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-4.5">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <span className="inline-block rounded-md border border-slate-200 bg-slate-50 px-1.5 py-0.5 font-mono text-[10.5px] tracking-wide text-hk-blue">
            {facility.code}
          </span>
          <h2 className="mt-2.5 text-2xl leading-tight font-bold tracking-tight">{facility.name}</h2>
          <p className="mt-1.5 text-xs text-slate-500">{facility.address.label}</p>
        </div>
        <span className={cn("inline-flex h-6 shrink-0 items-center gap-1.5 rounded-full border px-2.5 text-[11px] font-bold", STATUS_META[status].pill)}>
          <StatusIcon className="size-3" aria-hidden />
          {STATUS_META[status].label}
        </span>
      </div>

      <dl className="mt-4.5 grid grid-cols-2 gap-x-4 gap-y-3.5 border-t border-slate-100 pt-4">
        <Field label="Zřizující odbor" value={facility.department} />
        <Field label={capacity.label} value={capacity.value} />
        <Field
          label={`Dotační rámec ORP ${grants?.orpName ?? ""}`.trim()}
          value={grants ? `${formatNumber(grants.count)} dotací · ${formatMillionsCzk(grants.total)}` : "—"}
        />
        <div>
          <dt className="text-[11px] text-slate-500">Stavební objekt (RÚIAN)</dt>
          <dd className="mt-0.5 text-[13px] font-bold">
            {facility.building?.cadastreUrl ? (
              <a href={facility.building.cadastreUrl} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 text-hk-red hover:underline">
                {facility.building.ruianCode}
                <ExternalLink className="size-3" aria-hidden />
              </a>
            ) : (
              "nenalezen v ČÚZK"
            )}
          </dd>
        </div>
      </dl>
    </div>
  );
}

function Field({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-[11px] text-slate-500">{label}</dt>
      <dd className="mt-0.5 text-[13px] font-bold">{value}</dd>
    </div>
  );
}

// Kapacita podle toho, co o objektu otevřená data říkají — žáci, lůžka, nebo nic
function capacityOf(facility: FacilityDetail) {
  const withStudents = facility.occupancy.find((o) => o.students !== null);
  if (withStudents) return { label: `Žáků (${withStudents.schoolYear})`, value: formatNumber(withStudents.students) };

  const withBeds = facility.occupancy.find((o) => o.bedCapacity !== null);
  if (withBeds) return { label: "Lůžek / ubytovaných", value: `${formatNumber(withBeds.bedCapacity)} / ${formatNumber(withBeds.residents)}` };

  return { label: "Kapacita", value: "neuvedena v datech" };
}
