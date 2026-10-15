"use client";

import { useEffect } from "react";
import type { FeatureCollection, Geometry, Position } from "geojson";
import type { GeoJSONSource, Map as MapLibreMap } from "maplibre-gl";
import { useMap } from "@/components/ui/map";
import type { AuditStatus, Footprint } from "@/types/api";

const TILE_SOURCE = "carto";
const TILE_LAYER = "building";
const SOURCE_ID = "selected-building";
const LAYER_ID = "selected-building-3d";

// 3D budovy z dlaždic jsou vidět od přiblížení 14,5 — dřív nemá smysl hledat jejich výšku
const MIN_MATCH_ZOOM = 15;

// O kolik je barevná budova vyšší než šedá z podkladu, aby střecha nepřekrývala stejnou rovinu
const ROOF_CLEARANCE_METERS = 0.4;

// Stavové barvy (Tailwind 500) — vybraná budova je barevná, okolní zástavba šedá
const STATUS_COLOR: Record<AuditStatus, string> = {
  Nominal: "#10b981",
  Warning: "#f59e0b",
  Critical: "#ef4444",
};

// Vybraná budova ve 3D. Podkladová mapa kreslí tutéž budovu šedě (OSM má v ČR půdorysy z katastru), proto:
// - obrys je z ČÚZK rozšířený o ~0,4 m (server) — barevné stěny leží vně šedých, nic se nepřekrývá,
// - výška se vezme ze skutečné budovy v dlaždicích (+0,4 m nad střechu); když tam není, odhad podle podlaží.
// Obarvit přímo budovu z dlaždic nejde: ID prvků v dlaždicích CARTO nejsou napříč dlaždicemi unikátní.
// Stav přichází ze seznamu objektů, ne s půdorysem — půdorys je cachovaný 2 dny, stav se mění průběžně.
export function SelectedBuilding3D({ footprint, status }: { footprint: Footprint | null; status: AuditStatus }) {
  const { map, isLoaded } = useMap();

  useEffect(() => {
    if (!map || !isLoaded) return;

    const rings = outerRings(footprint?.geometry ?? null);
    const color = STATUS_COLOR[status];
    let resolved = false;

    ensureLayer(map);
    render(map, footprint, footprint?.properties.heightMeters ?? 0, color);

    function refineHeight() {
      if (resolved || !map || !footprint || rings.length === 0 || map.getZoom() < MIN_MATCH_ZOOM) return;

      const tileHeights = map
        .querySourceFeatures(TILE_SOURCE, { sourceLayer: TILE_LAYER })
        .filter((feature) => overlaps(feature.geometry, rings))
        .map((feature) => Number(feature.properties?.render_height))
        .filter((height) => Number.isFinite(height) && height > 0);

      if (tileHeights.length > 0) {
        render(map, footprint, Math.max(...tileHeights) + ROOF_CLEARANCE_METERS, color);
      }

      resolved = true;
    }

    refineHeight();
    map.on("idle", refineHeight);

    return () => {
      map.off("idle", refineHeight);
      if (map.getStyle()) render(map, null, 0, color);
    };
  }, [map, isLoaded, footprint, status]);

  return null;
}

function ensureLayer(map: MapLibreMap) {
  if (map.getSource(SOURCE_ID)) return;

  map.addSource(SOURCE_ID, { type: "geojson", data: { type: "FeatureCollection", features: [] } });
  map.addLayer({
    id: LAYER_ID,
    type: "fill-extrusion",
    source: SOURCE_ID,
    paint: {
      "fill-extrusion-height": ["get", "height"],
      "fill-extrusion-base": 0,
      "fill-extrusion-opacity": 1,
    },
  });
}

function render(map: MapLibreMap, footprint: Footprint | null, height: number, color: string) {
  const source = map.getSource(SOURCE_ID) as GeoJSONSource | undefined;
  if (!source) return;

  const data: FeatureCollection = {
    type: "FeatureCollection",
    features: footprint?.geometry ? [{ type: "Feature", geometry: footprint.geometry as Geometry, properties: { height } }] : [],
  };

  source.setData(data);
  map.setPaintProperty(LAYER_ID, "fill-extrusion-color", color);
}

// Vnější obrysy polygonu (Polygon i MultiPolygon)
function outerRings(geometry: Footprint["geometry"] | Geometry | null): Position[][] {
  if (!geometry) return [];

  const typed = geometry as Geometry;
  if (typed.type === "Polygon") return [typed.coordinates[0]];
  if (typed.type === "MultiPolygon") return typed.coordinates.map((polygon) => polygon[0]);

  return [];
}

// Budova z dlaždice patří k vybranému objektu, když její těžiště leží v obrysu ČÚZK, nebo naopak
// (OSM občas kreslí celý komplex jako jeden polygon, katastr po jednotlivých objektech)
function overlaps(geometry: Geometry, footprint: Position[][]) {
  return outerRings(geometry).some(
    (tileRing) => footprint.some((ring) => pointInRing(centroid(tileRing), ring)) || footprint.some((ring) => pointInRing(centroid(ring), tileRing)),
  );
}

function centroid(ring: Position[]): Position {
  const points = ring.length > 1 ? ring.slice(0, -1) : ring;
  const sum = points.reduce(([x, y], [px, py]) => [x + px, y + py], [0, 0]);

  return [sum[0] / points.length, sum[1] / points.length];
}

// Ray casting — bod v polygonu
function pointInRing([x, y]: Position, ring: Position[]) {
  let inside = false;

  for (let i = 0, j = ring.length - 1; i < ring.length; j = i++) {
    const [xi, yi] = ring[i];
    const [xj, yj] = ring[j];
    if (yi > y !== yj > y && x < ((xj - xi) * (y - yi)) / (yj - yi) + xi) inside = !inside;
  }

  return inside;
}
