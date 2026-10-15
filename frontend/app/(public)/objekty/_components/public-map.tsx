"use client";

import { useEffect, useRef } from "react";
import { useRouter } from "next/navigation";
import { LngLatBounds } from "maplibre-gl";
import { GuideHighlights } from "@/components/guide/guide-highlights";
import { Map, MapMarker, MarkerContent, MarkerTooltip, useMap } from "@/components/ui/map";
import { AIR_META, airLevel, type AirLevel } from "@/lib/air-quality";
import { GUIDE_HIGHLIGHT_EVENT } from "@/lib/guide-events";
import { cn } from "@/lib/utils";
import type { OpenFacility } from "@/types/api";

// Stejný šedý podklad a výchozí výřez (celý kraj) jako mapa aplikace
const GRAY_STYLES = { light: "/map-styles/gray-light.json", dark: "/map-styles/gray-dark.json" };
const KHK_HOME = { center: [15.845, 50.352] as [number, number], zoom: 8.3 };

const LEVELS: AirLevel[] = ["good", "fair", "poor"];

type PublicMapProps = {
  facilities: OpenFacility[];
};

// Veřejná mapa objektů: tečka v barvě kvality vzduchu (ne stavu auditu — veřejnost zajímá, jak se dýchá),
// klik otevře kartu objektu. Nad ní pracuje průvodce (očíslované objekty z odpovědi).
export function PublicMap({ facilities }: PublicMapProps) {
  const router = useRouter();
  const codes = new globalThis.Map(facilities.map((f) => [f.id, f.code]));

  return (
    <div className="absolute inset-0">
      <Map center={KHK_HOME.center} zoom={KHK_HOME.zoom} styles={GRAY_STYLES} theme="light">
        <FitToFacilities facilities={facilities} />
        {facilities.map((facility) => {
          const level = airLevel(facility.co2);

          return (
            <MapMarker key={facility.id} longitude={facility.longitude} latitude={facility.latitude} anchor="center" onClick={() => router.push(`/objekty/${facility.code}`)}>
              <MarkerContent>
                <span
                  className={cn(
                    "block size-3 cursor-pointer rounded-full border-2 border-white shadow-[0_2px_5px_rgba(11,25,44,0.35)] transition-transform hover:scale-150",
                    level ? AIR_META[level].dot : "bg-slate-400",
                    facility.hasLiveSensors && "ring-2 ring-emerald-600 ring-offset-1",
                  )}
                />
              </MarkerContent>
              <MarkerTooltip anchor="left" offset={[12, 0]} className="rounded-lg border border-slate-200 bg-white px-2.5 py-1.5 text-hk-navy shadow-md">
                <span className="block text-[12px] font-bold">{facility.name}</span>
                <span className="block text-[11px] text-slate-500">
                  {level ? AIR_META[level].label : "Zatím bez měření"} · {facility.municipalityName}
                </span>
              </MarkerTooltip>
            </MapMarker>
          );
        })}
        <GuideHighlights onSelect={(item) => router.push(`/objekty/${codes.get(item.id) ?? ""}`)} />
      </Map>

      <ul className="pointer-events-none absolute right-3 bottom-3 z-10 flex flex-col gap-1 rounded-xl border border-slate-200 bg-white/95 px-3 py-2 text-[11.5px] text-slate-700 shadow-sm" aria-label="Legenda mapy">
        {LEVELS.map((level) => (
          <li key={level} className="flex items-center gap-2">
            <span className={cn("size-2.5 rounded-full", AIR_META[level].dot)} aria-hidden />
            {AIR_META[level].label}
          </li>
        ))}
      </ul>
    </div>
  );
}

// Výřez podle výsledku filtru (okres, druh, hledání). Když průvodce zrovna ukazuje své objekty, výřez nechává jemu.
function FitToFacilities({ facilities }: { facilities: OpenFacility[] }) {
  const { map, isLoaded } = useMap();
  const guideActive = useRef(false);
  const key = facilities.map((f) => f.id).join(",");

  useEffect(() => {
    function handleHighlight(event: Event) {
      guideActive.current = ((event as CustomEvent<unknown[] | null>).detail ?? []).length > 0;
    }

    window.addEventListener(GUIDE_HIGHLIGHT_EVENT, handleHighlight);
    return () => window.removeEventListener(GUIDE_HIGHLIGHT_EVENT, handleHighlight);
  }, []);

  useEffect(() => {
    if (!map || !isLoaded || guideActive.current || facilities.length === 0) return;

    const bounds = new LngLatBounds();
    for (const facility of facilities) bounds.extend([facility.longitude, facility.latitude]);
    map.fitBounds(bounds, { padding: 60, maxZoom: 14, duration: 900, essential: true });
    // Přepočítat jen při jiném výsledku filtru, ne při každém vykreslení
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [map, isLoaded, key]);

  return null;
}
