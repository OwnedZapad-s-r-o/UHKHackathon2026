"use client";

import { useEffect } from "react";
import type { Feature, FeatureCollection } from "geojson";
import type { GeoJSONSource } from "maplibre-gl";
import { useMap } from "@/components/ui/map";
import type { AuditStatus, Footprint } from "@/types/api";

const SOURCE_ID = "selected-building";
const LAYER_ID = "selected-building-3d";

// Stavové barvy (Tailwind 500) — vybraná budova je barevná, okolní zástavba šedá
const STATUS_COLOR: Record<AuditStatus, string> = {
  Nominal: "#10b981",
  Warning: "#f59e0b",
  Critical: "#ef4444",
};

// Půdorys vybrané budovy z ČÚZK vytažený do výšky odhadu podlaží.
// MapGeoJSON z mapcn umí jen plochu a linii, proto vlastní vrstva fill-extrusion přes MapLibre.
export function SelectedBuilding3D({ footprint }: { footprint: Footprint | null }) {
  const { map, isLoaded } = useMap();

  useEffect(() => {
    if (!map || !isLoaded) return;

    const data: FeatureCollection = {
      type: "FeatureCollection",
      features: footprint?.geometry ? [footprint as unknown as Feature] : [],
    };
    const color = STATUS_COLOR[footprint?.properties.status ?? "Nominal"];

    const source = map.getSource(SOURCE_ID) as GeoJSONSource | undefined;
    if (source) {
      source.setData(data);
    } else {
      map.addSource(SOURCE_ID, { type: "geojson", data });
      map.addLayer({
        id: LAYER_ID,
        type: "fill-extrusion",
        source: SOURCE_ID,
        paint: {
          "fill-extrusion-color": color,
          "fill-extrusion-height": ["get", "heightMeters"],
          "fill-extrusion-base": 0,
          "fill-extrusion-opacity": 0.92,
        },
      });
    }

    map.setPaintProperty(LAYER_ID, "fill-extrusion-color", color);
  }, [map, isLoaded, footprint]);

  // Úklid při odchodu z mapy
  useEffect(() => {
    return () => {
      if (!map || !map.getStyle()) return;
      if (map.getLayer(LAYER_ID)) map.removeLayer(LAYER_ID);
      if (map.getSource(SOURCE_ID)) map.removeSource(SOURCE_ID);
    };
  }, [map]);

  return null;
}
