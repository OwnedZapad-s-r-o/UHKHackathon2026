"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import type { ExpressionSpecification, LngLatBoundsLike } from "maplibre-gl";
import { MapGeoJSON, useMap, type MapGeoJSONEvent } from "@/components/ui/map";
import { issueShareColor, type DistrictStats } from "@/lib/district-shading";
import { formatCzk } from "@/lib/facility-meta";
import type { RegionCollection, RegionFeature } from "@/types/api";

// Výplň okresů leží nad plochami a vodou, ale pod silnicemi, budovami a popisky
const BEFORE_LAYER = "waterway_label";

// Nad tímto přiblížením se okresy rozplynou — uživatel už řeší budovu, ne kraj
const FADE_START_ZOOM = 10.5;
const FADE_END_ZOOM = 13.5;

const SELECTED_OUTLINE = "#d90429";
const MASK_COLOR = "#f8fafc";

// Hranice se mění jen s importem dat — API je posílá s Cache-Control na den, tady se pamatují pro celou relaci
let regionsRequest: Promise<RegionCollection | null> | null = null;

function loadDistricts() {
  regionsRequest ??= fetch("/api/map/regions?type=Okres")
    .then((response) => (response.ok ? (response.json() as Promise<RegionCollection>) : null))
    .catch(() => null)
    .then((result) => {
      if (result === null) regionsRequest = null;
      return result;
    });

  return regionsRequest;
}

type DistrictLayerProps = {
  stats: Record<string, DistrictStats>;
  selectedCode: string | null;
  // Při vybraném objektu se kamera neřídí okresem (letí k budově) a klik do mapy okres nemění
  facilitySelected: boolean;
  home: { center: [number, number]; zoom: number };
  onSelect: (code: string) => void;
};

type Hover = { stats: DistrictStats; x: number; y: number };

// Okresy barevně podle podílu objektů se zjištěním; vybraný okres červeným obrysem, ostatní ustoupí
export function DistrictLayer({ stats, selectedCode, facilitySelected, home, onSelect }: DistrictLayerProps) {
  const { map, isLoaded } = useMap();
  const [districts, setDistricts] = useState<RegionCollection | null>(null);
  const [hover, setHover] = useState<Hover | null>(null);

  useEffect(() => {
    let cancelled = false;
    loadDistricts().then((loaded) => !cancelled && setDistricts(loaded));

    return () => {
      cancelled = true;
    };
  }, []);

  const fillPaint = useMemo(() => {
    const hasSelection = selectedCode !== null;
    const color = matchByCode(stats, (entry) => (hasSelection && entry.code !== selectedCode ? MASK_COLOR : issueShareColor(entry.issueShare)), MASK_COLOR);
    const opacity = matchByCode(stats, (entry) => (!hasSelection ? 0.55 : entry.code === selectedCode ? 0.3 : 0.75), 0);

    return {
      "fill-color": color,
      "fill-opacity": ["interpolate", ["linear"], ["zoom"], FADE_START_ZOOM, opacity, FADE_END_ZOOM, 0] as ExpressionSpecification,
    };
  }, [stats, selectedCode]);

  const linePaint = useMemo(() => {
    const selected = ["==", ["get", "code"], selectedCode ?? ""] as ExpressionSpecification;

    return {
      "line-color": ["case", selected, SELECTED_OUTLINE, selectedCode ? "#94a3b8" : "#ffffff"] as ExpressionSpecification,
      "line-width": ["case", selected, 3, 1.4] as ExpressionSpecification,
      "line-opacity": ["interpolate", ["linear"], ["zoom"], FADE_START_ZOOM, 1, FADE_END_ZOOM + 1, 0.25] as ExpressionSpecification,
    };
  }, [selectedCode]);

  const fillHoverPaint = useMemo(() => ({ "fill-color": "#1e3e62" }), []);

  // Výběr okresu přiblíží mapu na jeho hranice; „Celý kraj“ vrátí výřez kraje
  const previousCode = useRef<string | null>(null);
  useEffect(() => {
    if (!map || !isLoaded || !districts || facilitySelected) return;

    const feature = districts.features.find((f) => f.properties.code === selectedCode);
    if (feature) {
      map.fitBounds(bounds(feature), { padding: 48, duration: 900, pitch: 0, bearing: 0 });
    } else if (previousCode.current !== null) {
      map.flyTo({ center: home.center, zoom: home.zoom, pitch: 0, bearing: 0, duration: 900 });
    }

    previousCode.current = selectedCode;
  }, [map, isLoaded, districts, selectedCode, facilitySelected, home]);

  if (!districts) return null;

  function handleHover(event: MapGeoJSONEvent | null) {
    const code = event?.feature.properties?.code as string | undefined;
    const entry = code ? stats[code] : undefined;

    if (!event || !entry || !map || map.getZoom() > FADE_START_ZOOM + 1) {
      setHover(null);
      return;
    }

    const point = map.project([event.longitude, event.latitude]);
    setHover({ stats: entry, x: point.x, y: point.y });
  }

  function handleClick(event: MapGeoJSONEvent) {
    const code = event.feature.properties?.code as string | undefined;
    // Přiblížená mapa = uživatel kliká na budovy a ulice, ne na okresy
    if (!code || facilitySelected || !map || map.getZoom() > FADE_START_ZOOM + 1) return;

    onSelect(code);
  }

  return (
    <>
      <MapGeoJSON
        id="districts"
        data={districts}
        promoteId="code"
        interactive
        beforeId={BEFORE_LAYER}
        fillPaint={fillPaint}
        linePaint={linePaint}
        fillHoverPaint={fillHoverPaint}
        onHover={handleHover}
        onClick={handleClick}
      />
      {hover && <DistrictTooltip hover={hover} selected={hover.stats.code === selectedCode} />}
    </>
  );
}

function DistrictTooltip({ hover, selected }: { hover: Hover; selected: boolean }) {
  const { stats } = hover;

  return (
    <div
      className="pointer-events-none absolute z-20 w-56 -translate-x-1/2 -translate-y-[calc(100%+14px)] rounded-xl border border-slate-200 bg-white/97 p-3 text-hk-navy shadow-[0_12px_28px_rgba(11,25,44,0.18)]"
      style={{ left: hover.x, top: hover.y }}
    >
      <p className="text-[13px] font-bold">Okres {stats.name}</p>
      <p className="mt-0.5 text-[11px] text-slate-500">{Math.round(stats.issueShare * 100)} % objektů se zjištěním</p>
      <dl className="mt-2 grid grid-cols-3 gap-1 text-center text-[11px]">
        <Figure label="objektů" value={stats.total} />
        <Figure label="kritických" value={stats.critical} tone="text-red-700" />
        <Figure label="varování" value={stats.warning} tone="text-amber-700" />
      </dl>
      {stats.lossCzk !== null && stats.lossCzk > 0 && <p className="mt-2 text-[11px] text-slate-600">Odhad ztrát {formatCzk(stats.lossCzk)} / rok</p>}
      {!selected && <p className="mt-2 border-t border-slate-100 pt-1.5 text-[10.5px] text-slate-500">Kliknutím vyberete okres</p>}
    </div>
  );
}

function Figure({ label, value, tone = "text-hk-navy" }: { label: string; value: number; tone?: string }) {
  return (
    <div className="flex flex-col-reverse rounded-md bg-slate-50 py-1">
      <dt className="text-slate-500">{label}</dt>
      <dd className={`font-mono text-[13px] font-bold ${tone}`}>{value}</dd>
    </div>
  );
}

// ["match", ["get", "code"], "CZ0521", hodnota, …, výchozí] — hodnota pro každý okres podle jeho souhrnu
function matchByCode<T extends string | number>(stats: Record<string, DistrictStats>, value: (entry: DistrictStats) => T, fallback: T): ExpressionSpecification {
  const entries = Object.values(stats);
  if (entries.length === 0) return ["literal", fallback] as ExpressionSpecification;

  return ["match", ["get", "code"], ...entries.flatMap((entry) => [entry.code, value(entry)]), fallback] as unknown as ExpressionSpecification;
}

function bounds(feature: RegionFeature): LngLatBoundsLike {
  const polygons = feature.geometry.type === "Polygon" ? [feature.geometry.coordinates] : feature.geometry.coordinates;
  let minX = Infinity;
  let minY = Infinity;
  let maxX = -Infinity;
  let maxY = -Infinity;

  for (const [x, y] of polygons.flat(2)) {
    minX = Math.min(minX, x);
    minY = Math.min(minY, y);
    maxX = Math.max(maxX, x);
    maxY = Math.max(maxY, y);
  }

  return [
    [minX, minY],
    [maxX, maxY],
  ];
}
