"use client";

import { useEffect } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Map, MapMarker, MarkerContent, MarkerTooltip, useMap } from "@/components/ui/map";
import { CATEGORY_META, STATUS_META, formatCzk } from "@/lib/facility-meta";
import { withParams } from "@/lib/search-params";
import { cn } from "@/lib/utils";
import type { AuditStatus, FacilityCategory, FacilityListItem } from "@/types/api";
import { MapToolbar } from "./map-toolbar";

// Šedý podklad vygenerovaný skriptem scripts/make-gray-style.mjs z výchozích stylů CARTO
const GRAY_STYLES = {
  light: "/map-styles/gray-light.json",
  dark: "/map-styles/gray-dark.json",
};

// Výchozí výřez — celý Královéhradecký kraj
const KHK_HOME = { center: [15.845, 50.352] as [number, number], zoom: 8.6 };

// Pořadí vykreslení — pozdější marker leží nahoře, problémové objekty nesmí zmizet pod zelenými
const DRAW_ORDER: Record<AuditStatus, number> = { Nominal: 0, Warning: 1, Critical: 2 };

type FacilityMapProps = {
  facilities: FacilityListItem[];
  selectedId: string | null;
};

export function FacilityMap({ facilities, selectedId }: FacilityMapProps) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  const ordered = facilities.toSorted((a, b) => DRAW_ORDER[a.status] - DRAW_ORDER[b.status]);
  const selected = facilities.find((f) => f.id === selectedId) ?? null;

  function select(id: string) {
    // Výběr objektu na mapě vždy znovu otevře inspekční panel
    router.push(withParams(pathname, searchParams, { objekt: id, inspekce: null }), { scroll: false });
  }

  return (
    <div data-map-root className="relative h-full w-full bg-slate-100">
      {/* Prototyp je světlý — mapa drží světlý šedý styl i při tmavém systému */}
      <Map center={KHK_HOME.center} zoom={KHK_HOME.zoom} styles={GRAY_STYLES} theme="light">
        <MapToolbar home={KHK_HOME} />
        {selected && <KeepSelectedInView longitude={selected.longitude} latitude={selected.latitude} />}

        {ordered.map((facility) => (
          <MapMarker key={facility.id} longitude={facility.longitude} latitude={facility.latitude} onClick={() => select(facility.id)}>
            <MarkerContent>
              <FacilityPin category={facility.category} status={facility.status} selected={facility.id === selectedId} />
            </MarkerContent>
            <MarkerTooltip>
              <p className="font-bold">{facility.name}</p>
              <p className="text-xs opacity-80">
                {facility.code} · {facility.municipalityName}
              </p>
              <p className="mt-1 text-xs">
                {STATUS_META[facility.status].label}: {facility.headline}
              </p>
              {facility.annualLossCzk !== null && facility.annualLossCzk > 0 && (
                <p className="text-xs">Odhad ztráty {formatCzk(facility.annualLossCzk)} / rok</p>
              )}
            </MarkerTooltip>
          </MapMarker>
        ))}
      </Map>

      <MapLegend facilities={facilities} />
    </div>
  );
}

// Když se objekt vybere mimo mapu (portfolio, anomálie), posune se na něj výřez
function KeepSelectedInView({ longitude, latitude }: { longitude: number; latitude: number }) {
  const { map, isLoaded } = useMap();

  useEffect(() => {
    if (!map || !isLoaded) return;
    if (map.getBounds().contains([longitude, latitude])) return;

    map.flyTo({ center: [longitude, latitude], zoom: Math.max(map.getZoom(), 11), duration: 900 });
  }, [map, isLoaded, longitude, latitude]);

  return null;
}

type FacilityPinProps = {
  category: FacilityCategory;
  status: AuditStatus;
  selected: boolean;
};

// Pin jako v prototypu: kolečko v barvě stavu, bílý okraj, ikona druhu; vybraný je černý s červeným prstencem
function FacilityPin({ category, status, selected }: FacilityPinProps) {
  const Icon = CATEGORY_META[category].icon;

  return (
    <span
      className={cn(
        "flex cursor-pointer items-center justify-center rounded-full border-2 border-white text-white transition-transform hover:scale-110",
        selected
          ? "size-9 bg-black shadow-[0_8px_18px_rgba(0,0,0,0.34)] outline-2 outline-offset-2 outline-hk-red"
          : cn("size-7 shadow-[0_4px_10px_rgba(0,0,0,0.24)]", STATUS_META[status].dot),
      )}
    >
      <Icon className={selected ? "size-4" : "size-3.5"} strokeWidth={2} aria-hidden />
    </span>
  );
}

function MapLegend({ facilities }: { facilities: FacilityListItem[] }) {
  const statuses: AuditStatus[] = ["Critical", "Warning", "Nominal"];
  const categories: FacilityCategory[] = ["School", "Social", "Health", "Culture"];

  return (
    <div className="absolute bottom-10 left-4 z-10 w-52 rounded-xl border border-slate-200 bg-white p-4 text-hk-navy shadow-[0_8px_20px_rgba(11,25,44,0.12)]">
      <p className="text-xs font-bold">Stav objektů</p>
      <ul className="mt-2 space-y-1.5 text-xs text-slate-700">
        {statuses.map((status) => (
          <li key={status} className="flex items-center gap-2">
            <span className={cn("size-2.5 rounded-full", STATUS_META[status].dot)} />
            <span className="flex-1">{STATUS_META[status].label}</span>
            <span className="tabular-nums text-slate-500">{facilities.filter((f) => f.status === status).length}</span>
          </li>
        ))}
      </ul>

      <p className="mt-4 text-xs font-bold">Druh objektu</p>
      <ul className="mt-2 space-y-1.5 text-xs text-slate-700">
        {categories.map((category) => {
          const Icon = CATEGORY_META[category].icon;

          return (
            <li key={category} className="flex items-center gap-2">
              <Icon className="size-3.5 text-slate-500" strokeWidth={1.75} aria-hidden />
              <span className="flex-1">{CATEGORY_META[category].label}</span>
              <span className="tabular-nums text-slate-500">{facilities.filter((f) => f.category === category).length}</span>
            </li>
          );
        })}
      </ul>
    </div>
  );
}
