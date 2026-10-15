import { Suspense } from "react";
import { MousePointerClick } from "lucide-react";
import { getFacilities, getSession } from "@/lib/api";
import { districtStats } from "@/lib/district-shading";
import { readParam } from "@/lib/search-params";
import { cn } from "@/lib/utils";
import { FacilityMap } from "../_components/facility-map";
import { InspectionPanel, PANEL_TABS, type PanelTab } from "../_components/inspection/inspection-panel";
import { MapFilterBar } from "../_components/map-filter-bar";
import { MapGuide } from "../_components/map-guide";
import { NewBuildingNotice } from "../_components/new-building-notice";
import { OwnBuildings } from "../_components/own-buildings";
import { TopBar } from "../_components/top-bar";

export default async function MapPage({ searchParams }: PageProps<"/mapa">) {
  const params = await searchParams;

  const filter = {
    q: readParam(params, "q"),
    district: readParam(params, "okres"),
    onlyIssues: readParam(params, "zjisteni") === "1",
  };
  const filtered = Boolean(filter.q || filter.district || filter.onlyIssues);

  // Správce budovy vidí jen budovy své organizace (filtruje API) — pohled na kraj po okresech pro něj nemá smysl
  const session = await getSession();
  const ownBuildings = session?.role === "Spravce";

  // Barvy okresů se počítají z celého kraje — s aktivním filtrem je potřeba i nefiltrovaný seznam
  const [facilities, allFacilities] = await Promise.all([getFacilities(filter), filtered && !ownBuildings ? getFacilities() : null]);
  const stats = ownBuildings ? {} : districtStats(allFacilities ?? facilities);

  // Našeptávač hledání nabízí celý kraj (ne jen výsledek hledání), ale jen objekty, které přepínač „Jen se zjištěním“ ukazuje
  const suggestions = (allFacilities ?? facilities)
    .filter((f) => !filter.onlyIssues || f.status !== "Nominal")
    .map((f) => ({ id: f.id, code: f.code, name: f.name, municipalityName: f.municipalityName, districtName: f.districtName, status: f.status }));

  // Na startu není vybraný žádný objekt — výběr vzniká kliknutím (nebo odkazem s ?objekt=).
  // Výjimka: správce jediné budovy ji má otevřenou rovnou, nemá co vybírat.
  const singleBuilding = ownBuildings && facilities.length === 1 ? facilities[0] : null;
  const selected = facilities.find((f) => f.id === readParam(params, "objekt")) ?? singleBuilding;
  const tab = parseTab(readParam(params, "panel"));

  return (
    <>
      <TopBar title="Mapa objektů" />

      <div className="flex min-h-0 flex-1">
        <section className="@container flex min-w-0 flex-1 flex-col border-r border-slate-200 bg-white">
          <MapFilterBar
            selectedFacility={selected && selected !== singleBuilding ? { name: selected.name, districtCode: selected.districtCode, districtName: selected.districtName } : null}
            organizationName={ownBuildings ? (session?.organizationName ?? "Moje budovy") : null}
            suggestions={suggestions}
          />
          <div className="relative min-h-0 flex-1">
            <FacilityMap
              facilities={facilities}
              selectedId={selected?.id ?? null}
              selectedDistrict={filter.district ?? null}
              districtStats={stats}
              ownBuildings={ownBuildings}
            />

            {/* Správce bez budov nemá co filtrovat — místo hlášky vidí nabídku přidat první budovu */}
            {facilities.length === 0 && (filtered || !ownBuildings) && (
              <p className="absolute top-1/3 left-1/2 z-10 -translate-x-1/2 rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-600 shadow-sm">
                Filtru neodpovídá žádný objekt. Zkuste jiný okres nebo název.
              </p>
            )}

            {ownBuildings && !selected && <OwnBuildings facilities={facilities} />}

            {/* Průvodce: otázky vlastními slovy, odpovědi řídí mapu (okres, objekt, očíslované špendlíky) */}
            {session && <MapGuide role={session.role} selectedName={selected?.name ?? null} belowBreadcrumb={!ownBuildings} />}

            {selected && readParam(params, "nova") === "1" && <NewBuildingNotice name={selected.name} waiting={selected.headline === PENDING_HEADLINE} />}

            <div className={cn("pointer-events-none absolute top-3 right-3 z-10 flex flex-col items-end gap-2", ownBuildings && "hidden")}>
              <span className="rounded-md border border-slate-200 bg-white/92 px-2 py-1 font-mono text-[10.5px] text-slate-600">
                {facilities.length} objektů ve výběru
              </span>
              {!selected && facilities.length > 0 && (
                <p className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white/95 px-3 py-2 text-[12.5px] text-slate-600 shadow-[0_8px_20px_rgba(11,25,44,0.08)]">
                  <MousePointerClick className="size-4 text-hk-red" aria-hidden />
                  Klikněte na objekt v mapě
                </p>
              )}
            </div>
          </div>
        </section>

        {selected && (
          <aside data-tour="panel" className="w-[35%] max-w-130 min-w-90 shrink-0 overflow-x-hidden overflow-y-auto bg-slate-50" aria-label="Inspekční panel">
            {/* Suspense: mapa se ukáže hned, panel s telemetrií dorazí streamem */}
            <Suspense key={`${selected.id}-${tab}`} fallback={<PanelSkeleton />}>
              <InspectionPanel facilityId={selected.id} tab={tab} searchParams={params} closable={selected !== singleBuilding} />
            </Suspense>
          </aside>
        )}
      </div>
    </>
  );
}

// Hlavička objektu, který audit ještě nevyhodnotil (GetFacilities v API)
const PENDING_HEADLINE = "Čeká na první vyhodnocení";

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
