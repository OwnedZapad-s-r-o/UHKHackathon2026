"use client";

import { useEffect } from "react";
import type { FeatureCollection } from "geojson";
import type { GeoJSONSource } from "maplibre-gl";
import { useMap } from "@/components/ui/map";
import type { FacilityListItem } from "@/types/api";

const SOURCE_ID = "facility-dots";
const LAYER_ID = "facility-dots";

// Nad silnicemi a výplní okresů, pod budovami a popisky míst
const BEFORE_LAYER = "building";

type FacilityDotsProps = {
  facilities: FacilityListItem[];
  visible: boolean;
};

// Pohled na celý kraj: objekty jen jako malé tečky ve stavové barvě, vykreslené přímo v mapě.
// Nejdou na ně klikat, takže klik vždy trefí okres; kritické objekty jsou větší, aby problémy zůstaly vidět.
export function FacilityDots({ facilities, visible }: FacilityDotsProps) {
  const { map, isLoaded } = useMap();

  useEffect(() => {
    if (!map || !isLoaded) return;

    map.addSource(SOURCE_ID, { type: "geojson", data: { type: "FeatureCollection", features: [] } });
    map.addLayer(
      {
        id: LAYER_ID,
        type: "circle",
        source: SOURCE_ID,
        layout: {
          // Kritické nahoře, aby je nepřekryly zelené
          "circle-sort-key": ["match", ["get", "status"], "Critical", 2, "Warning", 1, 0],
        },
        paint: {
          "circle-color": ["match", ["get", "status"], "Critical", "#ef4444", "Warning", "#f59e0b", "#10b981"],
          "circle-radius": ["match", ["get", "status"], "Critical", 5.5, "Warning", 4.5, 3.5],
          "circle-stroke-color": "#ffffff",
          "circle-stroke-width": 1.2,
        },
      },
      map.getLayer(BEFORE_LAYER) ? BEFORE_LAYER : undefined,
    );

    return () => {
      try {
        if (map.getLayer(LAYER_ID)) map.removeLayer(LAYER_ID);
        if (map.getSource(SOURCE_ID)) map.removeSource(SOURCE_ID);
      } catch {
        // styl se právě přenačítá
      }
    };
  }, [map, isLoaded]);

  useEffect(() => {
    if (!map || !isLoaded) return;

    const data: FeatureCollection = {
      type: "FeatureCollection",
      features: facilities.map((f) => ({
        type: "Feature",
        geometry: { type: "Point", coordinates: [f.longitude, f.latitude] },
        properties: { status: f.status },
      })),
    };

    (map.getSource(SOURCE_ID) as GeoJSONSource | undefined)?.setData(data);
  }, [map, isLoaded, facilities]);

  useEffect(() => {
    if (!map || !isLoaded || !map.getLayer(LAYER_ID)) return;
    map.setLayoutProperty(LAYER_ID, "visibility", visible ? "visible" : "none");
  }, [map, isLoaded, visible]);

  return null;
}
