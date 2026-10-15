"use client";

import { useEffect, useState } from "react";
import type { FeatureCollection, Geometry, Position } from "geojson";
import type { GeoJSONSource, PropertyValueSpecification, Map as MapLibreMap } from "maplibre-gl";
import { useMap } from "@/components/ui/map";
import type { AuditStatus, Footprint } from "@/types/api";

const TILE_SOURCE = "carto";
const TILE_LAYER = "building";
const BASE_BUILDINGS_LAYER = "building-3d";
const SOURCE_ID = "selected-building";
const GLOW_LAYER_ID = "selected-building-glow";
const LAYER_ID = "selected-building-3d";

// 3D budovy z dlaždic jsou vidět od přiblížení 14,5 — dřív nemá smysl hledat jejich výšku
const MIN_MATCH_ZOOM = 15;

// O kolik je barevná budova vyšší než šedá z podkladu, aby střecha nepřekrývala stejnou rovinu
const ROOF_CLEARANCE_METERS = 0.4;

// Budova při výběru „vyroste“ ze země — pohyb přitáhne oko k vybranému objektu
const GROW_MS = 700;

// Okolní zástavba při výběru ustoupí do pozadí
const DIMMED_BUILDINGS_OPACITY = 0.4;

// Stavové barvy (Tailwind 500) — vybraná budova je barevná, okolní zástavba šedá
const STATUS_COLOR: Record<AuditStatus, string> = {
  Nominal: "#10b981",
  Warning: "#f59e0b",
  Critical: "#ef4444",
};

// Půdorys se mění jen s katastrem: API ho posílá s Cache-Control na 2 dny (prohlížeč ho drží v HTTP cache)
// a tady se navíc pamatuje pro okamžité opakované kliknutí na stejný objekt
const footprintCache = new Map<string, Promise<Footprint | null>>();

// version = verze obrysu ze seznamu objektů: po doplnění nebo úpravě budovy vznikne nová adresa, stará cache se nepoužije
function loadFootprint(facilityId: string, version: string) {
  const key = `${facilityId}:${version}`;
  let pending = footprintCache.get(key);

  if (!pending) {
    pending = fetch(`/api/facilities/${facilityId}/footprint?v=${encodeURIComponent(version)}`)
      .then((response) => (response.ok ? (response.json() as Promise<Footprint>) : null))
      .catch(() => null);
    footprintCache.set(key, pending);
    // Neúspěch se nepamatuje — příští klik to zkusí znovu
    pending.then((footprint) => footprint === null && footprintCache.delete(key));
  }

  return pending;
}

type SelectedBuilding3DProps = {
  facilityId: string | null;
  version: string;
  status: AuditStatus;
};

// Vybraná budova ve 3D. Obrys je budova z ČÚZK a přilehlé budovy doplněné správcem (MultiPolygon) — každá má svou výšku.
// Podkladová mapa kreslí tutéž budovu šedě (OSM má v ČR půdorysy z katastru), proto:
// - obrys je z ČÚZK rozšířený o ~0,4 m (server) — barevné stěny leží vně šedých, nic se nepřekrývá,
// - výška se vezme ze skutečné budovy v dlaždicích (+0,4 m nad střechu); když tam není, odhad podle podlaží.
// Obarvit přímo budovu z dlaždic nejde: ID prvků v dlaždicích CARTO nejsou napříč dlaždicemi unikátní.
// Stav přichází ze seznamu objektů, ne s půdorysem — půdorys se cachuje, stav se mění průběžně.
export function SelectedBuilding3D({ facilityId, version, status }: SelectedBuilding3DProps) {
  const { map, isLoaded } = useMap();
  const [footprint, setFootprint] = useState<Footprint | null>(null);

  useEffect(() => {
    let cancelled = false;

    if (facilityId) {
      loadFootprint(facilityId, version).then((loaded) => !cancelled && setFootprint(loaded));
    }

    return () => {
      cancelled = true;
      setFootprint(null);
    };
  }, [facilityId, version]);

  useEffect(() => {
    if (!map || !isLoaded || !footprint?.geometry) return;

    const polygons = polygonsOf(footprint.geometry);
    const color = STATUS_COLOR[status];
    const baseOpacity = map.getLayer(BASE_BUILDINGS_LAYER) ? map.getPaintProperty(BASE_BUILDINGS_LAYER, "fill-extrusion-opacity") : undefined;
    // Výška každé budovy zvlášť: odhad z podlaží, dokud se nenajde skutečná v dlaždicích
    const targetHeights = polygons.map(() => footprint.properties.heightMeters);
    const resolved = polygons.map(() => false);
    let frame = 0;

    ensureLayers(map);
    map.setPaintProperty(LAYER_ID, "fill-extrusion-color", color);
    map.setPaintProperty(GLOW_LAYER_ID, "line-color", color);
    if (baseOpacity !== undefined) map.setPaintProperty(BASE_BUILDINGS_LAYER, "fill-extrusion-opacity", DIMMED_BUILDINGS_OPACITY);

    const start = performance.now();
    function grow(now: number) {
      const progress = Math.min(1, (now - start) / GROW_MS);
      const eased = 1 - Math.pow(1 - progress, 3);
      render(map!, polygons, targetHeights.map((height) => height * eased));
      if (progress < 1) frame = requestAnimationFrame(grow);
    }
    frame = requestAnimationFrame(grow);

    function refineHeight() {
      if (!map || resolved.every(Boolean) || map.getZoom() < MIN_MATCH_ZOOM) return;

      const tiles = map.querySourceFeatures(TILE_SOURCE, { sourceLayer: TILE_LAYER });
      let changed = false;

      polygons.forEach((polygon, index) => {
        if (resolved[index]) return;

        const tileHeights = tiles
          .filter((feature) => overlaps(feature.geometry, [polygon[0]]))
          .map((feature) => Number(feature.properties?.render_height))
          .filter((height) => Number.isFinite(height) && height > 0);

        // Nenalezená budova se zkusí znovu při dalším „idle“ — při skoku z jiné otevřené budovy běží první pokus
        // ještě nad dlaždicemi staré polohy a odhadnutá výška by pak zůstala nižší než šedá budova z podkladu
        if (tileHeights.length === 0) return;

        targetHeights[index] = Math.max(...tileHeights) + ROOF_CLEARANCE_METERS;
        resolved[index] = true;
        changed = true;
      });

      if (changed && performance.now() - start >= GROW_MS) render(map, polygons, targetHeights);
    }

    refineHeight();
    map.on("idle", refineHeight);

    return () => {
      cancelAnimationFrame(frame);
      map.off("idle", refineHeight);
      if (!map.getStyle()) return;

      render(map, [], []);
      if (baseOpacity !== undefined) {
        map.setPaintProperty(BASE_BUILDINGS_LAYER, "fill-extrusion-opacity", baseOpacity as PropertyValueSpecification<number>);
      }
    };
  }, [map, isLoaded, footprint, status]);

  return null;
}

// Vrstvy se kreslí před okolní zástavbou: zprůhledněné domy pak přes vybranou budovu jen prosvítají.
// Kdyby šla až po nich, hloubkový test by budovu schovanou za jiným domem nevykreslil vůbec.
function ensureLayers(map: MapLibreMap) {
  if (map.getSource(SOURCE_ID)) return;

  const beforeId = map.getLayer(BASE_BUILDINGS_LAYER)
    ? BASE_BUILDINGS_LAYER
    : map.getStyle().layers.find((layer) => layer.type === "symbol")?.id;

  map.addSource(SOURCE_ID, { type: "geojson", data: { type: "FeatureCollection", features: [] } });

  // Rozmazaná záře kolem paty budovy — budova je vidět i z dálky a v hustém bloku
  map.addLayer(
    {
      id: GLOW_LAYER_ID,
      type: "line",
      source: SOURCE_ID,
      paint: {
        "line-width": ["interpolate", ["linear"], ["zoom"], 14, 6, 18, 18],
        "line-blur": ["interpolate", ["linear"], ["zoom"], 14, 4, 18, 12],
        "line-opacity": 0.6,
      },
    },
    beforeId,
  );

  map.addLayer(
    {
      id: LAYER_ID,
      type: "fill-extrusion",
      source: SOURCE_ID,
      paint: {
        "fill-extrusion-height": ["get", "height"],
        "fill-extrusion-base": 0,
        "fill-extrusion-opacity": 1,
      },
    },
    beforeId,
  );
}

function render(map: MapLibreMap, polygons: Position[][][], heights: number[]) {
  const source = map.getSource(SOURCE_ID) as GeoJSONSource | undefined;
  if (!source) return;

  const data: FeatureCollection = {
    type: "FeatureCollection",
    features: polygons.map((coordinates, index) => ({ type: "Feature", geometry: { type: "Polygon", coordinates }, properties: { height: heights[index] } })),
  };

  source.setData(data);
}

// Jednotlivé budovy obrysu: Polygon = jedna, MultiPolygon = budova z ČÚZK a doplněné přilehlé budovy
function polygonsOf(geometry: Footprint["geometry"]): Position[][][] {
  const typed = geometry as Geometry | null;
  if (typed?.type === "Polygon") return [typed.coordinates];
  if (typed?.type === "MultiPolygon") return typed.coordinates;

  return [];
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
