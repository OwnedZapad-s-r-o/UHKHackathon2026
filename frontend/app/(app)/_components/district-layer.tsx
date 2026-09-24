"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import type { Feature, FeatureCollection, Polygon, Position } from "geojson";
import type { ExpressionSpecification, LngLatBoundsLike } from "maplibre-gl";
import { X } from "lucide-react";
import { MapGeoJSON, MapMarker, MarkerContent, useMap, type MapGeoJSONEvent } from "@/components/ui/map";
import { issueShareColor, type DistrictStats } from "@/lib/district-shading";
import { formatCzk } from "@/lib/facility-meta";
import type { RegionCollection, RegionFeature } from "@/types/api";

// Id vrstvy výplně okresů (mapcn: geojson-fill-{id}) — podle ní MapClickRouter pozná klik do okresu
export const DISTRICT_FILL_LAYER = "geojson-fill-districts";

// Do tohoto přiblížení klik do mapy vybírá okres; blíž už uživatel kliká na ulice a budovy
export const DISTRICT_CLICK_MAX_ZOOM = 11;

// Výplň okresů leží nad plochami a vodou, ale pod silnicemi, budovami a popisky
const FILL_BEFORE_LAYER = "waterway_label";

// Maska a obrys vybraného okresu jdou nad silnice, ale pod budovy a popisky míst
const HIGHLIGHT_BEFORE_LAYER = "building";

// Nad tímto přiblížením se okresy rozplynou — uživatel už řeší budovu, ne kraj
const FADE_START_ZOOM = 10.5;
const FADE_END_ZOOM = 13.5;

const SELECTED_OUTLINE = "#d90429";

// Obálka pro masku „vše kromě vybraného okresu“ — s rezervou kolem celého kraje
const MASK_EXTENT: Position[] = [
  [11.5, 48.3],
  [19.5, 48.3],
  [19.5, 51.8],
  [11.5, 51.8],
  [11.5, 48.3],
];

const EMPTY: FeatureCollection = { type: "FeatureCollection", features: [] };

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
  // Při vybraném objektu se kamera neřídí okresem (letí k budově)
  facilitySelected: boolean;
  home: { center: [number, number]; zoom: number };
  // Pohled na celý kraj: uprostřed každého okresu štítek se souhrnem, klik ho vybere
  showLabels: boolean;
  onSelect: (code: string) => void;
  onClear: () => void;
};

type Hover = { stats: DistrictStats; x: number; y: number };

// Okresy barevně podle podílu objektů se zjištěním. Vybraný okres: zbytek mapy zešedne maskou
// a okres má červený obrys se září; vlevo nahoře štítek se souhrnem a návratem na celý kraj.
export function DistrictLayer({ stats, selectedCode, facilitySelected, home, showLabels, onSelect, onClear }: DistrictLayerProps) {
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

  const selectedFeature = districts?.features.find((f) => f.properties.code === selectedCode) ?? null;

  // Bez výběru choropleth; s výběrem výplň zmizí a okres zvýrazní maska kolem něj
  const fillPaint = useMemo(() => {
    const opacity = selectedCode ? 0 : 0.55;

    return {
      "fill-color": matchByCode(stats, (entry) => issueShareColor(entry.issueShare), "#f8fafc"),
      "fill-opacity": ["interpolate", ["linear"], ["zoom"], FADE_START_ZOOM, opacity, FADE_END_ZOOM, 0] as ExpressionSpecification,
    };
  }, [stats, selectedCode]);

  const linePaint = useMemo(
    () => ({
      "line-color": selectedCode ? "#cbd5e1" : "#ffffff",
      "line-width": 1.4,
      "line-opacity": ["interpolate", ["linear"], ["zoom"], FADE_START_ZOOM, 1, FADE_END_ZOOM + 1, 0.25] as ExpressionSpecification,
    }),
    [selectedCode],
  );

  const fillHoverPaint = useMemo(() => ({ "fill-color": "#1e3e62" }), []);
  const maskPaint = useMemo(() => ({ "fill-color": "#f8fafc", "fill-opacity": 0.78 }), []);
  const glowPaint = useMemo(() => ({ "line-color": SELECTED_OUTLINE, "line-width": 10, "line-blur": 8, "line-opacity": 0.35 }), []);
  const outlinePaint = useMemo(() => ({ "line-color": SELECTED_OUTLINE, "line-width": 2.5 }), []);

  const mask = useMemo(() => (selectedFeature ? maskAround(selectedFeature) : EMPTY), [selectedFeature]);
  const outline = useMemo(() => (selectedFeature ? { type: "FeatureCollection" as const, features: [selectedFeature] } : EMPTY), [selectedFeature]);

  // Výběr okresu přiblíží mapu na jeho hranice; „Celý kraj“ vrátí výřez kraje
  const previousCode = useRef<string | null>(null);
  useEffect(() => {
    if (!map || !isLoaded || !districts || facilitySelected) return;

    if (selectedFeature) {
      map.fitBounds(bounds(selectedFeature), { padding: 56, duration: 900, pitch: 0, bearing: 0 });
    } else if (previousCode.current !== null) {
      map.flyTo({ center: home.center, zoom: home.zoom, pitch: 0, bearing: 0, duration: 900 });
    }

    previousCode.current = selectedCode;
  }, [map, isLoaded, districts, selectedFeature, selectedCode, facilitySelected, home]);

  if (!districts) return null;

  function handleHover(event: MapGeoJSONEvent | null) {
    const code = event?.feature.properties?.code as string | undefined;
    const entry = code ? stats[code] : undefined;
    // Nad špendlíkem má přednost jeho vlastní karta — dva tooltipy by se překrývaly
    const overMarker = (event?.originalEvent.originalEvent.target as HTMLElement | null)?.closest(".maplibregl-marker");

    if (!event || !entry || !map || overMarker || map.getZoom() > DISTRICT_CLICK_MAX_ZOOM) {
      setHover(null);
      return;
    }

    const point = map.project([event.longitude, event.latitude]);
    setHover({ stats: entry, x: point.x, y: point.y });
  }

  const selectedStats = selectedCode ? stats[selectedCode] : undefined;

  return (
    <>
      <MapGeoJSON
        id="districts"
        data={districts}
        promoteId="code"
        interactive
        beforeId={FILL_BEFORE_LAYER}
        fillPaint={fillPaint}
        linePaint={linePaint}
        fillHoverPaint={fillHoverPaint}
        onHover={handleHover}
      />
      {/* Pořadí komponent = pořadí vrstev: maska, záře, ostrý obrys */}
      <MapGeoJSON id="district-mask" data={mask} beforeId={HIGHLIGHT_BEFORE_LAYER} fillPaint={maskPaint} linePaint={false} />
      <MapGeoJSON id="district-glow" data={outline} beforeId={HIGHLIGHT_BEFORE_LAYER} fillPaint={false} linePaint={glowPaint} />
      <MapGeoJSON id="district-outline" data={outline} beforeId={HIGHLIGHT_BEFORE_LAYER} fillPaint={false} linePaint={outlinePaint} />

      {showLabels &&
        districts.features.map((feature) => {
          const entry = stats[feature.properties.code];
          if (!entry) return null;
          const [longitude, latitude] = labelPoint(feature);

          return (
            <MapMarker key={feature.properties.code} longitude={longitude} latitude={latitude} onClick={() => onSelect(entry.code)}>
              <MarkerContent>
                <DistrictLabel stats={entry} />
              </MarkerContent>
            </MapMarker>
          );
        })}

      {hover && !showLabels && (!selectedCode || hover.stats.code === selectedCode) && <DistrictTooltip hover={hover} selected={hover.stats.code === selectedCode} />}
      {selectedStats && <SelectedDistrictChip stats={selectedStats} onClear={onClear} />}
    </>
  );
}

// Štítek okresu v pohledu na kraj — název, počet objektů a kolik z nich má zjištění
function DistrictLabel({ stats }: { stats: DistrictStats }) {
  return (
    <span className="flex cursor-pointer flex-col items-center rounded-xl border border-slate-200 bg-white/95 px-3 py-1.5 text-center text-hk-navy shadow-[0_8px_20px_rgba(11,25,44,0.16)] transition-transform hover:-translate-y-0.5 hover:border-hk-red/50">
      <span className="text-[12.5px] font-bold whitespace-nowrap">{stats.name}</span>
      <span className="mt-0.5 flex items-center gap-2 text-[10.5px] whitespace-nowrap text-slate-500">
        {stats.total} objektů
        {stats.critical > 0 && (
          <span className="inline-flex items-center gap-1 font-bold text-red-700">
            <span className="size-1.5 rounded-full bg-red-500" aria-hidden />
            {stats.critical}
          </span>
        )}
        {stats.warning > 0 && (
          <span className="inline-flex items-center gap-1 font-bold text-amber-700">
            <span className="size-1.5 rounded-full bg-amber-500" aria-hidden />
            {stats.warning}
          </span>
        )}
      </span>
    </span>
  );
}

function SelectedDistrictChip({ stats, onClear }: { stats: DistrictStats; onClear: () => void }) {
  return (
    <div className="absolute top-3 left-3 z-10 flex items-center gap-3 rounded-xl border border-slate-200 bg-white/97 py-2 pr-2 pl-3 text-hk-navy shadow-[0_10px_24px_rgba(11,25,44,0.14)]">
      <span className="h-8 w-1 rounded-full bg-hk-red" aria-hidden />
      <span>
        <span className="block text-[13px] font-bold">Okres {stats.name}</span>
        <span className="block text-[11px] text-slate-500">
          {stats.total} objektů · {Math.round(stats.issueShare * 100)} % se zjištěním
        </span>
      </span>
      <button
        type="button"
        onClick={onClear}
        aria-label="Zobrazit celý kraj"
        title="Zobrazit celý kraj (Esc)"
        className="flex size-8 items-center justify-center rounded-lg text-slate-500 transition-colors hover:bg-slate-100 hover:text-hk-navy"
      >
        <X className="size-4" />
      </button>
    </div>
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

function outerRings(feature: RegionFeature): Position[][] {
  return feature.geometry.type === "Polygon" ? [feature.geometry.coordinates[0]] : feature.geometry.coordinates.map((polygon) => polygon[0]);
}

// Místo pro štítek: těžiště největšího kruhu okresu (vzorec plochy mnohoúhelníku)
function labelPoint(feature: RegionFeature): Position {
  const rings = outerRings(feature);
  let best: { area: number; point: Position } = { area: 0, point: rings[0][0] };

  for (const ring of rings) {
    let area = 0;
    let x = 0;
    let y = 0;

    for (let i = 0, j = ring.length - 1; i < ring.length; j = i++) {
      const cross = ring[j][0] * ring[i][1] - ring[i][0] * ring[j][1];
      area += cross;
      x += (ring[j][0] + ring[i][0]) * cross;
      y += (ring[j][1] + ring[i][1]) * cross;
    }

    if (Math.abs(area) > Math.abs(best.area)) best = { area, point: [x / (3 * area), y / (3 * area)] };
  }

  return best.point;
}

// Obdélník kolem kraje s dírou ve tvaru vybraného okresu — další kruhy polygonu jsou díry
function maskAround(feature: RegionFeature): FeatureCollection {
  const mask: Feature<Polygon> = { type: "Feature", properties: {}, geometry: { type: "Polygon", coordinates: [MASK_EXTENT, ...outerRings(feature)] } };
  return { type: "FeatureCollection", features: [mask] };
}

function bounds(feature: RegionFeature): LngLatBoundsLike {
  let minX = Infinity;
  let minY = Infinity;
  let maxX = -Infinity;
  let maxY = -Infinity;

  for (const [x, y] of outerRings(feature).flat()) {
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
