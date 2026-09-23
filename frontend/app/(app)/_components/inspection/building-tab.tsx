import { ExternalLink, MapPinned } from "lucide-react";
import { getSection } from "@/lib/api";
import { STATUS_META, formatNumber } from "@/lib/facility-meta";
import { cn } from "@/lib/utils";
import type { BuildingSection, FacilityDetail } from "@/types/api";
import { PanelCard } from "./panel-card";

const USE_LABEL: Record<string, string> = {
  publicServices: "občanská vybavenost",
  education: "vzdělávání",
  healthCare: "zdravotnictví",
  collectiveResidence: "hromadné bydlení",
  office: "administrativa",
};

export async function BuildingTab({ facility }: { facility: FacilityDetail }) {
  const section = await getSection(facility.id);
  const building = facility.building;

  return (
    <>
      <PanelCard title="Karta budovy" meta="ČÚZK INSPIRE Budovy">
        {building?.found ? (
          <dl className="grid grid-cols-2 gap-x-4 gap-y-3">
            <Item label="Rok výstavby" value={building.constructionYear?.toString() ?? "neuveden"} />
            <Item label="Energetická kategorie (odhad)" value={building.energyCategory} />
            <Item label="Zastavěná plocha" value={formatNumber(building.builtUpArea, "m²")} />
            <Item label="Podlaží (odhad z plochy)" value={formatNumber(building.estimatedFloors)} />
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

      <PanelCard title="Řez budovou" meta={`${section.zones.length} zón · ${section.floors} podl.`}>
        <SectionGrid section={section} />
        <p className="mt-3 text-[11px] leading-relaxed text-slate-500">
          Schéma podle druhu zařízení a odhadu podlaží — otevřená data vnitřní dispozici budov neobsahují, nejde o skutečný plán.
        </p>
      </PanelCard>
    </>
  );
}

function Item({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-[11px] text-slate-500">{label}</dt>
      <dd className="mt-0.5 text-[13px] font-bold">{value}</dd>
    </div>
  );
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
                    className={cn("min-w-0 flex-1 rounded-lg border px-2.5 py-2", level ? STATUS_META[level].pill : "border-slate-200 bg-slate-50")}
                    title={`${zone.name} · ${zone.wing}`}
                  >
                    <p className="flex items-center gap-1 truncate text-[11px] font-bold">
                      {StatusIcon && <StatusIcon className="size-3 shrink-0" aria-hidden />}
                      {zone.name}
                    </p>
                    <p className="mt-0.5 font-mono text-[11px]">
                      {formatNumber(zone.temperature, "°C")} · {formatNumber(zone.co2, "ppm")}
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
