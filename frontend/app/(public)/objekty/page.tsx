import type { Metadata } from "next";
import Link from "next/link";
import { ChevronRight, Info } from "lucide-react";
import { LiveBadge } from "@/components/live-badge";
import { DataModeBadge } from "@/components/data-mode-badge";
import { AIR_META, airLevel, type AirLevel } from "@/lib/air-quality";
import { CATEGORY_META } from "@/lib/facility-meta";
import { getPublicFacilities } from "@/lib/public-api";
import { readParam } from "@/lib/search-params";
import { cn } from "@/lib/utils";
import type { OpenFacility } from "@/types/api";
import { PublicGuide } from "../_components/public-guide";
import { SchoolSearch } from "../_components/school-search";
import { toSearchItems } from "../_components/search-items";
import { ObjectFilters } from "./_components/object-filters";
import { PublicMap } from "./_components/public-map";

export const metadata: Metadata = {
  title: "Najdi svou školu · SmartAudit",
  description: "Jak se dýchá ve školách, domovech a dalších budovách Královéhradeckého kraje — kvalita vnitřního vzduchu bez přihlášení.",
};

const LEVELS: AirLevel[] = ["good", "fair", "poor"];

export default async function ObjectsPage({ searchParams }: PageProps<"/objekty">) {
  const params = await searchParams;
  const query = readParam(params, "q")?.trim().toLocaleLowerCase("cs") ?? "";
  const district = readParam(params, "okres");
  const category = readParam(params, "druh");
  const byAir = readParam(params, "razeni") === "vzduch";

  const all = await getPublicFacilities();
  const facilities = all
    .filter((f) => !district || f.districtCode === district)
    .filter((f) => !category || f.category === category)
    .filter((f) => !query || `${f.name} ${f.municipalityName} ${f.code}`.toLocaleLowerCase("cs").includes(query))
    .toSorted(byAir ? byAirQuality : (a, b) => a.name.localeCompare(b.name, "cs"));

  const liveCount = facilities.filter((f) => f.hasLiveSensors).length;

  return (
    <main className="mx-auto max-w-6xl px-4 py-10 sm:px-6">
      <h1 className="text-[34px] leading-tight font-bold tracking-tight sm:text-[42px]">Jak se dýchá v budovách kraje</h1>
      <p className="mt-2 max-w-2xl text-[15px] leading-relaxed text-slate-600">
        Školy, domovy pro seniory, nemocnice a kulturní zařízení, které zřizuje Královéhradecký kraj. Najděte tu svou a podívejte se, jaký je v ní
        vzduch — CO₂, teplota a kdy větrat.
      </p>

      <form action="/objekty" className="mt-6 max-w-xl">
        {district && <input type="hidden" name="okres" value={district} />}
        {category && <input type="hidden" name="druh" value={category} />}
        <SchoolSearch
          items={toSearchItems(all)}
          defaultValue={readParam(params, "q")}
          placeholder="Název školy, domova nebo obec…"
          fieldClassName="flex h-12 items-center gap-2.5 rounded-lg border border-slate-300 bg-white px-3.5 focus-within:border-hk-blue focus-within:ring-3 focus-within:ring-hk-blue/15"
          iconClassName="size-4 text-slate-400"
          inputClassName="text-[15px]"
        />
      </form>

      {/* Mapa výsledku filtru a nad ní průvodce — ukáže v ní objekty, na které se ptáte */}
      <section className="relative isolate mt-6 h-[540px] overflow-hidden rounded-3xl border border-slate-200 bg-slate-100" aria-label="Mapa objektů">
        <PublicMap facilities={facilities} />
        <PublicGuide page="objekty" />
      </section>

      <div className="mt-6 flex flex-wrap items-center justify-between gap-4">
        <ObjectFilters />
        <AirSummary facilities={facilities} />
      </div>

      <p className="mt-4 flex items-start gap-2 rounded-xl bg-slate-50 px-4 py-3 text-[12.5px] leading-relaxed text-slate-600">
        <Info className="mt-0.5 size-4 shrink-0 text-hk-blue" aria-hidden />
        <span>
          Nejde o žebříček — budovy se liší provozem i počtem lidí. {liveCount > 0 ? `${liveCount} ${liveCount === 1 ? "objekt měří" : "objekty měří"} skutečnými čidly IQRF (odznak živě), ` : ""}
          ostatní hodnoty počítá fyzikální model z otevřených dat (ČÚZK + Data KHK + Open-Meteo), než kraj čidla nainstaluje.
        </span>
      </p>

      {facilities.length === 0 ? (
        <p className="mt-10 text-center text-slate-500">Hledání neodpovídá žádný objekt. Zkuste jiný název nebo obec.</p>
      ) : (
        <ul className="mt-6 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {facilities.map((facility) => (
            <li key={facility.id}>
              <FacilityCard facility={facility} />
            </li>
          ))}
        </ul>
      )}
    </main>
  );
}

function FacilityCard({ facility }: { facility: OpenFacility }) {
  const level = airLevel(facility.co2);
  const Icon = CATEGORY_META[facility.category].icon;

  return (
    <Link href={`/objekty/${facility.code}`} className="group flex h-full flex-col rounded-2xl border border-slate-200 bg-white p-4 transition-colors hover:border-hk-red/40">
      <div className="flex items-start gap-3">
        <span className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-slate-100 text-hk-navy">
          <Icon className="size-4.5" strokeWidth={1.75} aria-hidden />
        </span>
        <span className="min-w-0 flex-1">
          <span className="line-clamp-2 text-[14px] leading-snug font-bold group-hover:text-hk-red">{facility.name}</span>
          <span className="mt-0.5 block text-xs text-slate-500">
            {facility.kind} · {facility.municipalityName}
          </span>
        </span>
        <ChevronRight className="size-4 shrink-0 text-slate-300 group-hover:text-hk-red" aria-hidden />
      </div>

      <div className="mt-auto flex flex-wrap items-center gap-2 pt-4">
        {level && (
          <span className={cn("inline-flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-[12px] font-bold", AIR_META[level].pill)}>
            <span className={cn("size-1.5 rounded-full", AIR_META[level].dot)} aria-hidden />
            {AIR_META[level].label}
          </span>
        )}
        {facility.hasLiveSensors ? <LiveBadge /> : <DataModeBadge mode="OpenDataModel" compact className="text-[10.5px]" />}
        {facility.co2HoursOverLimit !== null && facility.co2HoursOverLimit > 0 && (
          <span className="ml-auto text-[11.5px] text-slate-500">{Math.round(facility.co2HoursOverLimit)} h nad limitem / týden</span>
        )}
      </div>
    </Link>
  );
}

// Kolik objektů je teď v jakém stavu — rychlý přehled nad seznamem
function AirSummary({ facilities }: { facilities: OpenFacility[] }) {
  return (
    <ul className="flex flex-wrap gap-3 text-[12.5px] text-slate-600" aria-label="Souhrn kvality vzduchu">
      {LEVELS.map((level) => (
        <li key={level} className="flex items-center gap-1.5">
          <span className={cn("size-2 rounded-full", AIR_META[level].dot)} aria-hidden />
          {AIR_META[level].label}
          <b className="text-hk-navy tabular-nums">{facilities.filter((f) => airLevel(f.co2) === level).length}</b>
        </li>
      ))}
    </ul>
  );
}

// Nejdřív budovy, kde byl vzduch za týden nejméně často nad limitem, pak podle aktuálního CO₂
function byAirQuality(a: OpenFacility, b: OpenFacility) {
  return (a.co2HoursOverLimit ?? Infinity) - (b.co2HoursOverLimit ?? Infinity) || (a.co2 ?? Infinity) - (b.co2 ?? Infinity);
}
