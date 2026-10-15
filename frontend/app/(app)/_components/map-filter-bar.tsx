"use client";

import { useRef, useState, type ReactNode } from "react";
import Link from "next/link";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { ChevronRight, Plus } from "lucide-react";
import { DISTRICTS } from "@/lib/facility-meta";
import { requestMapHome } from "@/lib/map-events";
import { withParams } from "@/lib/search-params";
import { cn } from "@/lib/utils";
import { DistrictSelect, WHOLE_REGION } from "./district-select";
import { FacilitySearch, type FacilitySuggestion } from "./facility-search";

type SelectedFacility = { name: string; districtCode: string; districtName: string };

// Lišta nad mapou jako v prototypu: výběr okresu a hledání — obojí zapsané do URL.
// Podle šířky mapové části (container query): široká → tlačítka okresů, úzká → rozbalovací výběr.
// Při otevřeném objektu místo výběru drobečková navigace „Celý kraj › Okres › objekt“ —
// výběr okresu by ukazoval jiný kontext, než ve kterém objekt leží.
// Správce budovy (organizationName) okresy ani hledání nepotřebuje — má jen své budovy.
// suggestions = objekty celého kraje pro našeptávač hledání.
type MapFilterBarProps = {
  selectedFacility: SelectedFacility | null;
  organizationName?: string | null;
  suggestions?: FacilitySuggestion[];
};

export function MapFilterBar({ selectedFacility, organizationName = null, suggestions = [] }: MapFilterBarProps) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const district = searchParams.get("okres");
  const [query, setQuery] = useState(searchParams.get("q") ?? "");
  const filterTimer = useRef<ReturnType<typeof setTimeout>>(undefined);

  // Text napsaný při otevřeném objektu mapu nefiltroval — po zavření objektu pole ukáže filtr, který opravdu platí
  const [hadSelection, setHadSelection] = useState(selectedFacility !== null);
  if (hadSelection !== (selectedFacility !== null)) {
    setHadSelection(selectedFacility !== null);
    if (selectedFacility === null) setQuery(searchParams.get("q") ?? "");
  }

  // Hledání se do URL propisuje s krátkým zpožděním, ať se nenačítá po každém písmenu.
  // S otevřeným objektem jen našeptává — filtr by otevřený objekt vyřadil z mapy a zavřel.
  function changeQuery(next: string) {
    setQuery(next);
    clearTimeout(filterTimer.current);
    if (selectedFacility) return;

    filterTimer.current = setTimeout(() => {
      router.replace(withParams(pathname, searchParams, { q: next.trim() || null }));
    }, 350);
  }

  // Návrh z našeptávače objekt rovnou otevře. Filtr textem ani okres se nepoužijí — objekt by v nich nemusel být.
  function selectFacility(facility: FacilitySuggestion) {
    clearTimeout(filterTimer.current);
    setQuery("");
    router.push(withParams(pathname, searchParams, { objekt: facility.id, q: null, okres: null, panel: null }));
  }

  // Jiný okres = jiný výřez mapy, dosavadní výběr objektu by v něm visel mimo záběr
  function selectDistrict(code: string | null) {
    router.push(withParams(pathname, searchParams, { okres: code, objekt: null, panel: null }));
  }

  if (organizationName !== null) {
    return (
      <div className="flex h-14 shrink-0 items-center gap-1 border-b border-slate-200 bg-white px-3.5 text-[13px]">
        {selectedFacility ? (
          <nav className="flex min-w-0 items-center gap-1" aria-label="Moje budovy">
            <button type="button" onClick={() => router.push(withParams(pathname, searchParams, { objekt: null, panel: null }))} className={CRUMB}>
              Moje budovy
            </button>
            <ChevronRight className="size-3.5 shrink-0 text-slate-400" aria-hidden />
            <span className="truncate px-1.5 font-bold text-hk-navy" aria-current="location">
              {selectedFacility.name}
            </span>
          </nav>
        ) : (
          <span className="truncate px-1.5 font-bold text-hk-navy">{organizationName}</span>
        )}
        <Link
          href="/budovy/nova"
          data-tour="pridat-budovu"
          className="ml-auto inline-flex h-9 shrink-0 items-center gap-1.5 rounded-lg border border-slate-200 bg-white px-3 font-bold text-hk-navy transition-colors hover:border-slate-300 hover:bg-slate-50"
        >
          <Plus className="size-4 text-hk-red" aria-hidden />
          Přidat budovu
        </Link>
      </div>
    );
  }

  return (
    <div data-tour="filtry" className="flex h-14 shrink-0 items-center gap-3 border-b border-slate-200 bg-white px-3.5">
      {selectedFacility ? (
        <nav className="flex min-w-0 items-center gap-1 text-[13px]" aria-label="Kde v kraji objekt leží">
          <button
            type="button"
            onClick={() => {
              // Okres se nemusí změnit (objekt otevřený z celého kraje) — přelet na kraj proto mapě řekneme přímo
              requestMapHome();
              selectDistrict(null);
            }}
            className={CRUMB}
          >
            Celý kraj
          </button>
          <ChevronRight className="size-3.5 shrink-0 text-slate-400" aria-hidden />
          <button type="button" onClick={() => selectDistrict(selectedFacility.districtCode)} className={cn(CRUMB, "whitespace-nowrap")}>
            Okres {selectedFacility.districtName}
          </button>
          <ChevronRight className="size-3.5 shrink-0 text-slate-400" aria-hidden />
          <span className="truncate px-1.5 font-bold text-hk-navy" aria-current="location">
            {selectedFacility.name}
          </span>
        </nav>
      ) : (
        <>
          <div className="hidden min-w-0 items-center gap-1.5 @4xl:flex">
            <span className="mr-1 text-[11px] text-slate-500">Okres</span>
            <DistrictChip active={district === null} onClick={() => selectDistrict(null)}>
              Celý kraj
            </DistrictChip>
            {DISTRICTS.map((d) => (
              <DistrictChip key={d.code} active={district === d.code} onClick={() => selectDistrict(d.code)}>
                {d.name}
              </DistrictChip>
            ))}
          </div>

          <div className="@4xl:hidden">
            <DistrictSelect value={district ?? WHOLE_REGION} onValueChange={(next) => selectDistrict(next === WHOLE_REGION ? null : next)} className="h-9 min-w-40" />
          </div>
        </>
      )}

      <FacilitySearch
        facilities={suggestions}
        value={query}
        onValueChange={changeQuery}
        onSelect={selectFacility}
        placeholder={selectedFacility ? "Přejít na jiný objekt…" : "Hledat objekt…"}
      />
    </div>
  );
}

type DistrictChipProps = {
  active: boolean;
  onClick: () => void;
  children: ReactNode;
};

function DistrictChip({ active, onClick, children }: DistrictChipProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      aria-pressed={active}
      className={cn(
        "h-9 shrink-0 rounded-lg border px-2.5 text-[13px] whitespace-nowrap transition-colors",
        active ? "border-hk-blue bg-hk-blue font-bold text-white" : "border-slate-200 bg-white text-hk-navy hover:bg-slate-100",
      )}
    >
      {children}
    </button>
  );
}

const CRUMB = "shrink-0 rounded-md px-1.5 py-1 text-slate-600 transition-colors hover:bg-slate-100 hover:text-hk-navy";
