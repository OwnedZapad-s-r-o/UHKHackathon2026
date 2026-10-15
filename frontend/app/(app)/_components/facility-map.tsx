"use client";

import { useEffect, useRef } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import {
  Map,
  MapMarker,
  MarkerContent,
  MarkerLabel,
  MarkerTooltip,
  useMap,
} from "@/components/ui/map";
import { CATEGORY_META, STATUS_META, formatCzk } from "@/lib/facility-meta";
import { withParams } from "@/lib/search-params";
import { cn } from "@/lib/utils";
import type {
  AuditStatus,
  FacilityCategory,
  FacilityListItem,
} from "@/types/api";
import { MapToolbar } from "./map-toolbar";
import { SelectedBuilding3D } from "./selected-building-3d";

// Šedý podklad vygenerovaný skriptem scripts/make-gray-style.mjs z výchozích stylů CARTO
const GRAY_STYLES = {
  light: "/map-styles/gray-light.json",
  dark: "/map-styles/gray-dark.json",
};

// Výchozí výřez — celý Královéhradecký kraj
const KHK_HOME = { center: [15.845, 50.352] as [number, number], zoom: 8.6 };

// Pohled na vybranou budovu — přiblížení a náklon, aby byla vidět ve 3D
const BUILDING_VIEW = { zoom: 17.2, pitch: 60, bearing: -20 };

// Pořadí vykreslení — pozdější marker leží nahoře, problémové objekty nesmí zmizet pod zelenými
const DRAW_ORDER: Record<AuditStatus, number> = {
  Nominal: 0,
  Warning: 1,
  Critical: 2,
};

type FacilityMapProps = {
  facilities: FacilityListItem[];
  selectedId: string | null;
};

export function FacilityMap({ facilities, selectedId }: FacilityMapProps) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  // Vybraný objekt se kreslí poslední, aby ho nepřekryly okolní piny
  const ordered = facilities.toSorted(
    (a, b) =>
      Number(a.id === selectedId) - Number(b.id === selectedId) ||
      DRAW_ORDER[a.status] - DRAW_ORDER[b.status],
  );
  const selected = facilities.find((f) => f.id === selectedId) ?? null;

  function select(id: string) {
    router.push(withParams(pathname, searchParams, { objekt: id }), {
      scroll: false,
    });
  }

  function deselect() {
    router.push(
      withParams(pathname, searchParams, { objekt: null, panel: null }),
      { scroll: false },
    );
  }

  return (
    <div data-map-root className="relative h-full w-full bg-slate-100">
      {/* Prototyp je světlý — mapa drží světlý šedý styl i při tmavém systému */}
      <Map
        center={KHK_HOME.center}
        zoom={KHK_HOME.zoom}
        styles={GRAY_STYLES}
        theme="light"
      >
        <MapToolbar home={KHK_HOME} />
        <SelectedBuilding3D
          facilityId={selected?.id ?? null}
          status={selected?.status ?? "Nominal"}
        />
        <FlyToSelected
          facilityId={selected?.id ?? null}
          longitude={selected?.longitude}
          latitude={selected?.latitude}
        />
        {selected && <DeselectOnMapClick onDeselect={deselect} />}

        {ordered.map((facility) => (
          <MapMarker
            key={facility.id}
            longitude={facility.longitude}
            latitude={facility.latitude}
            onClick={() => select(facility.id)}
          >
            <MarkerContent>
              <FacilityPin
                category={facility.category}
                status={facility.status}
                selected={facility.id === selectedId}
              />
              {facility.id === selectedId && (
                <MarkerLabel position="bottom" className="mt-3">
                  <SelectedLabel facility={facility} />
                </MarkerLabel>
              )}
            </MarkerContent>
            <MarkerTooltip>
              <p className="font-bold">{facility.name}</p>
              <p className="text-xs opacity-80">
                {facility.code} · {facility.municipalityName}
              </p>
              <p className="mt-1 text-xs">
                {STATUS_META[facility.status].label}: {facility.headline}
              </p>
              {facility.annualLossCzk !== null &&
                facility.annualLossCzk > 0 && (
                  <p className="text-xs">
                    Odhad ztráty {formatCzk(facility.annualLossCzk)} / rok
                  </p>
                )}
            </MarkerTooltip>
          </MapMarker>
        ))}
      </Map>

      <MapLegend facilities={facilities} />
    </div>
  );
}

// Po výběru objektu mapa přeletí k budově a nakloní se, aby byla vidět ve 3D;
// po zrušení výběru se jen srovná náklon — výřez zůstane, uživatel se nemá kde ztratit
function FlyToSelected({
  facilityId,
  longitude,
  latitude,
}: {
  facilityId: string | null;
  longitude?: number;
  latitude?: number;
}) {
  const { map, isLoaded } = useMap();
  const hadSelection = useRef(false);

  useEffect(() => {
    if (!map || !isLoaded) return;

    if (facilityId && longitude !== undefined && latitude !== undefined) {
      hadSelection.current = true;
      map.flyTo({
        center: [longitude, latitude],
        ...BUILDING_VIEW,
        duration: 1600,
        essential: true,
      });
    } else if (hadSelection.current) {
      hadSelection.current = false;
      map.easeTo({ pitch: 0, bearing: 0, duration: 700 });
    }
  }, [map, isLoaded, facilityId, longitude, latitude]);

  return null;
}

// Výběr se ruší klikem do prázdné mapy nebo klávesou Esc
function DeselectOnMapClick({ onDeselect }: { onDeselect: () => void }) {
  const { map } = useMap();
  const onDeselectRef = useRef(onDeselect);

  useEffect(() => {
    onDeselectRef.current = onDeselect;
  }, [onDeselect]);

  useEffect(() => {
    if (!map) return;

    // Piny jsou HTML prvky uvnitř mapy — jejich klik by se jinak počítal i jako klik do mapy
    function handleClick(event: { originalEvent: MouseEvent }) {
      const target = event.originalEvent.target as HTMLElement | null;
      if (target?.closest(".maplibregl-marker")) return;
      onDeselectRef.current();
    }

    function handleKey(event: KeyboardEvent) {
      if (event.key === "Escape") onDeselectRef.current();
    }

    map.on("click", handleClick);
    window.addEventListener("keydown", handleKey);

    return () => {
      map.off("click", handleClick);
      window.removeEventListener("keydown", handleKey);
    };
  }, [map]);

  return null;
}

// Štítek pod vybraným pinem (nad ním by zakrýval budovu) — název a stav, aby bylo jasné, co je vybrané
function SelectedLabel({ facility }: { facility: FacilityListItem }) {
  const meta = STATUS_META[facility.status];

  return (
    <span className="flex max-w-64 items-center gap-2 rounded-lg bg-hk-navy px-2.5 py-1.5 text-white shadow-[0_10px_24px_rgba(11,25,44,0.35)]">
      <span className={cn("size-2 shrink-0 rounded-full", meta.dot)} aria-hidden />
      <span className="truncate text-[11.5px] font-bold">{facility.name}</span>
      <span className="shrink-0 text-[10.5px] text-white/70">{meta.label}</span>
    </span>
  );
}

type FacilityPinProps = {
  category: FacilityCategory;
  status: AuditStatus;
  selected: boolean;
};

// Pin jako v prototypu: kolečko v barvě stavu, bílý okraj, ikona druhu.
// Vybraný objekt má jen malou tečku s pulzujícím kroužkem — velký pin by zakrýval jeho 3D budovu.
function FacilityPin({ category, status, selected }: FacilityPinProps) {
  const Icon = CATEGORY_META[category].icon;

  if (selected) {
    return (
      <span className="relative flex size-4 cursor-pointer items-center justify-center">
        <span
          className={cn(
            "absolute -inset-2 animate-ping rounded-full opacity-60",
            STATUS_META[status].dot,
          )}
          aria-hidden
        />
        <span
          className={cn(
            "relative size-4 rounded-full border-[3px] border-white shadow-[0_4px_12px_rgba(0,0,0,0.4)]",
            STATUS_META[status].dot,
          )}
        />
      </span>
    );
  }

  return (
    <span
      className={cn(
        "flex size-7 cursor-pointer items-center justify-center rounded-full border-2 border-white text-white shadow-[0_4px_10px_rgba(0,0,0,0.24)] transition-transform hover:scale-110",
        STATUS_META[status].dot,
      )}
    >
      <Icon className="size-3.5" strokeWidth={2} aria-hidden />
    </span>
  );
}

function MapLegend({ facilities }: { facilities: FacilityListItem[] }) {
  const statuses: AuditStatus[] = ["Critical", "Warning", "Nominal"];
  const categories: FacilityCategory[] = [
    "School",
    "Social",
    "Health",
    "Culture",
  ];

  return (
    <div className="absolute bottom-10 left-4 z-10 w-52 rounded-xl border border-slate-200 bg-white p-4 text-hk-navy shadow-[0_8px_20px_rgba(11,25,44,0.12)]">
      <p className="text-xs font-bold">Stav objektů</p>
      <ul className="mt-2 space-y-1.5 text-xs text-slate-700">
        {statuses.map((status) => (
          <li key={status} className="flex items-center gap-2">
            <span
              className={cn("size-2.5 rounded-full", STATUS_META[status].dot)}
            />
            <span className="flex-1">{STATUS_META[status].label}</span>
            <span className="tabular-nums text-slate-500">
              {facilities.filter((f) => f.status === status).length}
            </span>
          </li>
        ))}
      </ul>

      <p className="mt-4 text-xs font-bold">Druh objektu</p>
      <ul className="mt-2 space-y-1.5 text-xs text-slate-700">
        {categories.map((category) => {
          const Icon = CATEGORY_META[category].icon;

          return (
            <li key={category} className="flex items-center gap-2">
              <Icon
                className="size-3.5 text-slate-500"
                strokeWidth={1.75}
                aria-hidden
              />
              <span className="flex-1">{CATEGORY_META[category].label}</span>
              <span className="tabular-nums text-slate-500">
                {facilities.filter((f) => f.category === category).length}
              </span>
            </li>
          );
        })}
      </ul>
    </div>
  );
}
