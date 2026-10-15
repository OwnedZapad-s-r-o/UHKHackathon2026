import Link from "next/link";
import { ExternalLink, MapPinned, Plus } from "lucide-react";
import { LiveBadge } from "@/components/live-badge";
import { getSection, getSession } from "@/lib/api";
import { FEATURES } from "@/lib/features";
import { STATUS_META, formatNumber } from "@/lib/facility-meta";
import { type RawSearchParams, withParams } from "@/lib/search-params";
import { cn } from "@/lib/utils";
import type { BuildingSection, FacilityDetail } from "@/types/api";
import { BuildingDataForm } from "./building-data-form";
import { BuildingPartsList } from "./building-parts-list";
import { PanelCard } from "./panel-card";
import { RemoveBuildingButton } from "./remove-building-button";

const USE_LABEL: Record<string, string> = {
  publicServices: "občanská vybavenost",
  education: "vzdělávání",
  healthCare: "zdravotnictví",
  collectiveResidence: "hromadné bydlení",
  office: "administrativa",
};

export async function BuildingTab({ facility, searchParams }: { facility: FacilityDetail; searchParams: RawSearchParams }) {
  const [section, session] = await Promise.all([FEATURES.buildingSection ? getSection(facility.id) : null, getSession()]);
  const building = facility.building;
  const parts = facility.addedBuildingParts;
  const addedArea = parts.reduce((sum, part) => sum + part.areaSquareMeters, 0);
  // Upravovat budovy může kraj a správce (správce vidí jen objekty své organizace — jiné by detail ani nenačetl)
  const canEdit = session !== null && session.role !== "Verejnost";
  const editHref = (budova: string) => withParams("/mapa", searchParams, { panel: "budova", budova });

  return (
    <>
      <PanelCard title="Karta budovy" meta="ČÚZK INSPIRE Budovy">
        {building?.found ? (
          <dl className="grid grid-cols-2 gap-x-4 gap-y-3">
            <Item
              label="Rok výstavby"
              value={building.yearOverride?.toString() ?? building.constructionYear?.toString() ?? "neuveden"}
              note={building.yearOverride !== null ? `opraveno · ČÚZK ${building.constructionYear ?? "neuvádí"}` : undefined}
            />
            <Item label="Energetická kategorie (odhad)" value={building.editedEnergyCategory ?? building.energyCategory} />
            <Item label="Zastavěná plocha" value={formatNumber(effectiveArea(building, addedArea), "m²")} note={areaNote(building, addedArea)} />
            <Item
              label="Podlaží (odhad z plochy)"
              value={formatNumber(building.floorsOverride ?? building.estimatedFloors)}
              note={building.floorsOverride !== null ? `opraveno · odhad ${building.estimatedFloors}` : undefined}
            />
            <Item label="Způsob využití" value={USE_LABEL[building.currentUse ?? ""] ?? building.currentUse ?? "—"} />
            <div>
              <dt className="text-[11px] text-slate-500">Stavební objekt v RÚIAN</dt>
              <dd className="mt-0.5 text-[13px] font-bold">
                {building.cadastreUrl ? (
                  <a href={building.cadastreUrl} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 text-hk-red hover:underline">
                    {building.ruianCode}
                    <ExternalLink className="size-3" aria-hidden />
                  </a>
                ) : (
                  "—"
                )}
              </dd>
            </div>
          </dl>
        ) : (
          <p className="flex items-center gap-2 text-[13px] text-slate-600">
            <MapPinned className="size-4 text-slate-400" aria-hidden />
            Budovu se v ČÚZK nepodařilo dohledat podle adresního bodu.
          </p>
        )}
      </PanelCard>

      {canEdit && (
        <PanelCard title="Úpravy budovy" meta={building?.editedAt || parts.length > 0 ? "upravil správce" : undefined}>
          <h3 className="text-[13px] font-bold">Přilehlé budovy</h3>
          <p className="mt-0.5 text-[12.5px] leading-snug text-slate-600">
            Katastr vede k objektu jen budovu v místě adresy. Křídlo nebo pavilon zapsaný zvlášť doplníte kliknutím v mapě — jeho plocha se započítá do
            ztráty.
          </p>
          {parts.length > 0 && (
            <BuildingPartsList
              facilityId={facility.id}
              parts={parts.map((part) => ({
                id: part.id,
                areaSquareMeters: part.areaSquareMeters,
                source: part.source,
                replaceHref: editHref(`vymenit:${part.id}`),
                outlineHref: editHref(`obrys:${part.id}`),
              }))}
            />
          )}
          <Link
            href={editHref("doplnit")}
            scroll={false}
            className="mt-3 inline-flex h-9 items-center gap-1.5 rounded-lg border border-slate-300 px-3 text-[13px] font-bold text-hk-navy transition-colors hover:bg-slate-50"
          >
            <Plus className="size-4" aria-hidden />
            Doplnit přilehlou budovu
          </Link>

          <h3 className="mt-5 border-t border-slate-100 pt-4 text-[13px] font-bold">Údaje pro výpočet ztráty</h3>
          <BuildingDataForm
            key={building?.editedAt ?? "cuzk"}
            facilityId={facility.id}
            values={{ area: building?.areaOverride ?? null, floors: building?.floorsOverride ?? null, year: building?.yearOverride ?? null }}
            cadastre={{
              area: building ? (building.builtUpArea ?? 0) + addedArea || null : addedArea || null,
              floors: building?.estimatedFloors ?? 2,
              year: building?.constructionYear ?? null,
            }}
          />

          {facility.addedByManager && (
            <div className="mt-4 border-t border-slate-100 pt-1">
              <RemoveBuildingButton facilityId={facility.id} name={facility.name} />
            </div>
          )}
        </PanelCard>
      )}

      {section && (
        <PanelCard title="Řez budovou" meta={`${section.zones.length} zón · ${section.floors} podl.`}>
          <SectionGrid section={section} />
          <p className="mt-3 text-[11px] leading-relaxed text-slate-500">
            Schéma podle druhu zařízení a odhadu podlaží — otevřená data vnitřní dispozici budov neobsahují, nejde o skutečný plán.
          </p>
        </PanelCard>
      )}
    </>
  );
}

function Item({ label, value, note }: { label: string; value: string; note?: string }) {
  return (
    <div>
      <dt className="text-[11px] text-slate-500">{label}</dt>
      <dd className="mt-0.5 text-[13px] font-bold">{value}</dd>
      {note && <dd className="text-[11px] text-hk-blue">{note}</dd>}
    </div>
  );
}

type Building = NonNullable<FacilityDetail["building"]>;

// Plocha, se kterou počítá audit: oprava od správce, jinak ČÚZK + doplněné přilehlé budovy
function effectiveArea(building: Building, addedArea: number) {
  return building.areaOverride ?? (addedArea > 0 ? (building.builtUpArea ?? 0) + addedArea : building.builtUpArea);
}

function areaNote(building: Building, addedArea: number) {
  if (building.areaOverride !== null) return `opraveno · ČÚZK ${formatNumber(building.builtUpArea, "m²")}`;
  if (addedArea > 0) return `ČÚZK ${formatNumber(building.builtUpArea, "m²")} + doplněno ${formatNumber(addedArea, "m²")}`;
  return undefined;
}

// Podlaží shora dolů; zóna nad limitem CO₂ nese stavovou barvu + ikonu, ne jen barvu
function SectionGrid({ section }: { section: BuildingSection }) {
  const floors = Array.from({ length: section.floors }, (_, i) => section.floors - 1 - i);

  return (
    <div className="space-y-1.5">
      {floors.map((floor) => {
        const zones = section.zones.filter((z) => z.floor === floor);

        return (
          <div key={floor} className="flex items-stretch gap-1.5">
            <span className="flex w-10 shrink-0 items-center font-mono text-[10px] text-slate-400">{floor === 0 ? "přízemí" : `${floor}. NP`}</span>
            {zones.length === 0 ? (
              <span className="flex flex-1 items-center rounded-lg border border-dashed border-slate-200 px-2.5 text-[11px] text-slate-400">bez senzoru</span>
            ) : (
              zones.map((zone) => {
                const level = co2Level(zone.co2, section.co2Limit);
                const StatusIcon = level ? STATUS_META[level].icon : null;

                return (
                  <div
                    key={zone.id}
                    className={cn(
                      "min-w-0 flex-1 rounded-lg border px-2.5 py-2",
                      level ? STATUS_META[level].pill : "border-slate-200 bg-slate-50",
                      zone.live && "ring-2 ring-emerald-500/60",
                    )}
                    title={`${zone.name} · ${zone.wing}${zone.live ? " · skutečné čidlo IQRF" : ""}`}
                  >
                    <p className="flex items-center gap-1 truncate text-[11px] font-bold">
                      {StatusIcon && <StatusIcon className="size-3 shrink-0" aria-hidden />}
                      {zone.name}
                      {zone.live && <LiveBadge className="ml-auto" />}
                    </p>
                    <p className="mt-0.5 font-mono text-[11px]">
                      {formatNumber(zone.temperature, "°C")} · {formatNumber(zone.co2, "ppm")}
                      {zone.humidity !== null && ` · ${formatNumber(zone.humidity, "%")}`}
                    </p>
                  </div>
                );
              })
            )}
          </div>
        );
      })}
    </div>
  );
}

function co2Level(co2: number | null, limit: number) {
  if (co2 === null) return null;
  if (co2 > limit) return "Critical" as const;
  if (co2 > 1000) return "Warning" as const;
  return null;
}
