import { Suspense } from "react";
import Link from "next/link";
import { PanelRightOpen } from "lucide-react";
import { getFacilities } from "@/lib/api";
import { readParam, withParams } from "@/lib/search-params";
import type { FacilityListItem } from "@/types/api";
import { FacilityMap } from "./_components/facility-map";
import { InspectionPanel, PANEL_TABS, type PanelTab } from "./_components/inspection/inspection-panel";
import { MapFilterBar } from "./_components/map-filter-bar";
import { TopBar } from "./_components/top-bar";

export default async function MapPage({ searchParams }: PageProps<"/">) {
  const params = await searchParams;

  const facilities = await getFacilities({
    q: readParam(params, "q"),
    district: readParam(params, "okres"),
    onlyIssues: readParam(params, "zjisteni") === "1",
  });

  const selected = facilities.find((f) => f.id === readParam(params, "objekt")) ?? defaultSelection(facilities);
  const tab = parseTab(readParam(params, "panel"));

  // Inspekční panel jde zavřít (?inspekce=0) — mapa se pak roztáhne na celou šířku
  const panelOpen = selected !== null && readParam(params, "inspekce") !== "0";

  return (
    <>
      <TopBar title="Mapa objektů" />

      <div className="flex min-h-0 flex-1">
        <section className="@container flex min-w-0 flex-1 flex-col border-r border-slate-200 bg-white">
          <MapFilterBar />
          <div className="relative min-h-0 flex-1">
            <FacilityMap facilities={facilities} selectedId={selected?.id ?? null} />

            {facilities.length === 0 && (
              <p className="absolute top-1/3 left-1/2 z-10 -translate-x-1/2 rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-600 shadow-sm">
                Filtru neodpovídá žádný objekt. Zkuste jiný okres nebo název.
              </p>
            )}

            <div className="pointer-events-none absolute top-3 right-3 z-10 flex flex-col items-end gap-2">
              <span className="rounded-md border border-slate-200 bg-white/92 px-2 py-1 font-mono text-[10.5px] text-slate-600">
                {facilities.length} objektů ve výběru
              </span>
              {selected && !panelOpen && (
                <Link
                  href={withParams("/", params, { inspekce: null })}
                  scroll={false}
                  className="pointer-events-auto inline-flex h-9 items-center gap-2 rounded-lg border border-slate-200 bg-white px-3 text-[13px] font-bold text-hk-navy shadow-[0_8px_20px_rgba(11,25,44,0.12)] hover:bg-slate-50"
                >
                  <PanelRightOpen className="size-4" aria-hidden />
                  Otevřít inspekční panel
                </Link>
              )}
            </div>
          </div>
        </section>

        {panelOpen && (
          <aside className="w-[35%] max-w-130 min-w-90 shrink-0 overflow-x-hidden overflow-y-auto bg-slate-50" aria-label="Inspekční panel">
            {/* Suspense: mapa se ukáže hned, panel s telemetrií dorazí streamem */}
            <Suspense key={`${selected.id}-${tab}`} fallback={<PanelSkeleton />}>
              <InspectionPanel facilityId={selected.id} tab={tab} searchParams={params} />
            </Suspense>
          </aside>
        )}
      </div>
    </>
  );
}

// Jako v prototypu: bez výběru se otevře nejzávažnější objekt (API řadí podle ztráty)
function defaultSelection(facilities: FacilityListItem[]) {
  return facilities.find((f) => f.status === "Critical") ?? facilities.find((f) => f.status === "Warning") ?? facilities[0] ?? null;
}

function parseTab(value: string | undefined): PanelTab {
  return PANEL_TABS.find((t) => t.key === value)?.key ?? "telemetrie";
}

function PanelSkeleton() {
  return (
    <div className="animate-pulse space-y-3.5 p-5" aria-label="Načítám objekt">
      <div className="h-3 w-40 rounded bg-slate-200" />
      <div className="h-52 rounded-2xl bg-white" />
      <div className="h-8 rounded bg-slate-200/60" />
      <div className="h-40 rounded-2xl bg-white" />
      <div className="h-40 rounded-2xl bg-white" />
    </div>
  );
}
