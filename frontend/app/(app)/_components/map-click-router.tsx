"use client";

import { useEffect, useRef } from "react";
import type { MapMouseEvent } from "maplibre-gl";
import { useMap } from "@/components/ui/map";
import { DISTRICT_CLICK_MAX_ZOOM, DISTRICT_FILL_LAYER } from "./district-layer";

type MapClickRouterProps = {
  facilitySelected: boolean;
  districtSelected: boolean;
  onSelectDistrict: (code: string) => void;
  onClearDistrict: () => void;
  onDeselectFacility: () => void;
  // Editor budov má kliky i Esc pro sebe (výběr budovy, body obrysu) — výběr objektu a okresu se nemění
  disabled?: boolean;
};

// Jediné místo, které rozhoduje, co znamená klik do mapy — dřív ho zpracovala vrstva okresů i zrušení výběru
// zároveň a klik na špendlík spustil dvě navigace naráz:
// 1. klik na špendlík (HTML prvek v mapě) obslouží marker sám, tady se ignoruje,
// 2. při pohledu na kraj vybere klik okres pod kurzorem (mimo kraj vrátí celý kraj),
// 3. přiblížená mapa = klik do ulic a budov zruší výběr objektu.
// Esc ruší nejdřív objekt, pak okres.
export function MapClickRouter({ facilitySelected, districtSelected, onSelectDistrict, onClearDistrict, onDeselectFacility, disabled = false }: MapClickRouterProps) {
  const { map } = useMap();
  const latest = useRef({ facilitySelected, districtSelected, onSelectDistrict, onClearDistrict, onDeselectFacility, disabled });

  useEffect(() => {
    latest.current = { facilitySelected, districtSelected, onSelectDistrict, onClearDistrict, onDeselectFacility, disabled };
  });

  useEffect(() => {
    if (!map) return;

    function handleClick(event: MapMouseEvent) {
      const target = event.originalEvent.target as HTMLElement | null;
      if (target?.closest(".maplibregl-marker")) return;

      const state = latest.current;
      if (state.disabled) return;

      if (map!.getZoom() <= DISTRICT_CLICK_MAX_ZOOM && map!.getLayer(DISTRICT_FILL_LAYER)) {
        const code = map!.queryRenderedFeatures(event.point, { layers: [DISTRICT_FILL_LAYER] })[0]?.properties?.code as string | undefined;

        if (code) state.onSelectDistrict(code);
        else if (state.districtSelected) state.onClearDistrict();
        else if (state.facilitySelected) state.onDeselectFacility();
        return;
      }

      if (state.facilitySelected) state.onDeselectFacility();
    }

    function handleKey(event: KeyboardEvent) {
      if (event.key !== "Escape") return;

      const state = latest.current;
      if (state.disabled) return;
      if (state.facilitySelected) state.onDeselectFacility();
      else if (state.districtSelected) state.onClearDistrict();
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
