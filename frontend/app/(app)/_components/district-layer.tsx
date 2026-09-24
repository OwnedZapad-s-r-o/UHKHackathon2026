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

// Zvýraznění vybraného okresu — koncové hodnoty animace (mapcn vrstvy: geojson-fill-{id}, geojson-line-{id})
const MASK_LAYER = "geojson-fill-district-mask";
const GLOW_LAYER = "geojson-line-district-glow";
const MASK_OPACITY = 0.78;
const GLOW_WIDTH = 10;
const GLOW_OPACITY = 0.35;
const HIGHLIGHT_MS = 1400;

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
    const opacity = selectedCode ? 0 : 0.65;

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
  const maskPaint = useMemo(() => ({ "fill-color": "#f8fafc", "fill-opacity": MASK_OPACITY }), []);
  const glowPaint = useMemo(() => ({ "line-color": SELECTED_OUTLINE, "line-width": GLOW_WIDTH, "line-blur": 8, "line-opacity": GLOW_OPACITY }), []);
  const outlinePaint = useMemo(() => ({ "line-color": SELECTED_OUTLINE, "line-width": 2.5 }), []);

  const mask = useMemo(() => (selectedFeature ? maskAround(selectedFeature) : EMPTY), [selectedFeature]);
  const outline = useMemo(() => (selectedFeature ? { type: "FeatureCollection" as const, features: [selectedFeature] } : EMPTY), [selectedFeature]);

  // Animace výběru okresu: okolí plynule zešedne a obrys dvakrát pulzne září
  useEffect(() => {
    if (!map || !isLoaded || !selectedCode) return;

    const start = performance.now();
    let frame = 0;

    function step(now: number) {
      if (!map!.getLayer(MASK_LAYER) || !map!.getLayer(GLOW_LAYER)) return;

      const t = Math.min(1, (now - start) / HIGHLIGHT_MS);
      const fade = 1 - Math.pow(1 - Math.min(1, t * 2.2), 3);
      const pulse = Math.sin(t * Math.PI * 2) ** 2 * (1 - t);

      map!.setPaintProperty(MASK_LAYER, "fill-opacity", MASK_OPACITY * fade);
      map!.setPaintProperty(GLOW_LAYER, "line-width", GLOW_WIDTH + 18 * pulse);
      map!.setPaintProperty(GLOW_LAYER, "line-opacity", GLOW_OPACITY + 0.45 * pulse);

      if (t < 1) frame = requestAnimationFrame(step);
    }

    frame = requestAnimationFrame(step);
    return () => cancelAnimationFrame(frame);
  }, [map, isLoaded, selectedCode]);

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
        districts.features.map((feature, index) => {
          const entry = stats[feature.properties.code];
          if (!entry) return null;
          const [longitude, latitude] = labelPoint(feature);

          return (
            <MapMarker key={feature.properties.code} longitude={longitude} latitude={latitude} onClick={() => onSelect(entry.code)}>
              <MarkerContent>
                <DistrictBubble stats={entry} delay={index * 60} />
              </MarkerContent>
            </MapMarker>
          );
        })}

      {hover && !showLabels && (!selectedCode || hover.stats.code === selectedCode) && <DistrictTooltip hover={hover} selected={hover.stats.code === selectedCode} />}
      {selectedStats && <SelectedDistrictChip stats={selectedStats} onClear={onClear} />}
    </>
  );
}

// Bublina okresu v pohledu na kraj: prstenec = složení stavů (kritické, varování, v pořádku), uprostřed počet objektů,
// pod ní název s bílým obrysem písma — bez rámečku, aby zakrývala co nejméně mapy
function DistrictBubble({ stats, delay }: { stats: DistrictStats; delay: number }) {
  const size = 46;
  const radius = 19;
  const circumference = 2 * Math.PI * radius;
  const nominal = stats.total - stats.critical - stats.warning;
  const segments = [
    { value: stats.critical, color: "#ef4444" },
    { value: stats.warning, color: "#f59e0b" },
    { value: nominal, color: "#10b981" },
  ];

  let offset = 0;

  return (
    <span className="animate-bubble-pop group flex cursor-pointer flex-col items-center" style={{ animationDelay: `${delay}ms` }}>
      <span className="relative flex items-center justify-center rounded-full bg-white shadow-[0_6px_16px_rgba(11,25,44,0.25)] transition-transform duration-200 group-hover:scale-110">
        <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`} className="-rotate-90" aria-hidden>
          {segments.map((segment) => {
            const length = stats.total > 0 ? (segment.value / stats.total) * circumference : 0;
            const dash = `${length} ${circumference - length}`;
            const circle = (
              <circle key={segment.color} cx={size / 2} cy={size / 2} r={radius} fill="none" stroke={segment.color} strokeWidth={5} strokeDasharray={dash} strokeDashoffset={-offset} />
            );
            offset += length;
            return circle;
          })}
        </svg>
        <span className="absolute font-mono text-[13px] font-bold text-hk-navy tabular-nums">{stats.total}</span>
      </span>
      <span className="mt-1 text-[12.5px] font-bold whitespace-nowrap text-hk-navy [text-shadow:0_0_3px_#fff,0_0_3px_#fff,0_0_6px_#fff]">{stats.name}</span>
      <span className="text-[10.5px] font-bold whitespace-nowrap text-red-700 [text-shadow:0_0_3px_#fff,0_0_3px_#fff]">
        {stats.critical + stats.warning > 0 ? `${stats.critical + stats.warning} se zjištěním` : "bez zjištění"}
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
