"use client";

import { useEffect, useState } from "react";
import { Image as ImageIcon, Map as MapIcon, MousePointerClick } from "lucide-react";
import { Map, MapGeoJSON, MapMarker, MarkerContent, MarkerLabel, MarkerTooltip, useMap } from "@/components/ui/map";
import { CATEGORY_META } from "@/lib/facility-meta";
import { cn } from "@/lib/utils";
import type { FacilityCategory, FacilityListItem, Place } from "@/types/api";
import { FacilityPin, TOOLTIP_CLASS } from "../../../_components/facility-pin";
import { MapToolbar } from "../../../_components/map-toolbar";

const GRAY_STYLES = {
  light: "/map-styles/gray-light.json",
  dark: "/map-styles/gray-dark.json",
};

// Ortofoto ČÚZK (otevřená data) — na leteckém snímku se vlastní budova pozná mnohem snáz než na šedé mapě
const AERIAL_TILES = "https://ags.cuzk.gov.cz/arcgis1/rest/services/ORTOFOTO_WM/MapServer/tile/{z}/{y}/{x}";
const AERIAL_SOURCE = "aerial";
const AERIAL_LAYER = "aerial";

// Přiblížení, na kterém jde poznat jednotlivé budovy
const BUILDING_ZOOM = 18;

export type PinnedPoint = { latitude: number; longitude: number };

type PlaceMapProps = {
  home: { center: [number, number]; zoom: number };
  point: PinnedPoint | null;
  // Mění se, když má mapa k bodu přeletět (výběr adresy, moje poloha) — ne při kliknutí, to uživatel právě mířil
  focusKey: number;
  place: Place | null;
  previewName: string;
  category: FacilityCategory;
  ownFacilities: FacilityListItem[];
  onPick: (point: PinnedPoint) => void;
};

// Mapa formuláře: klik = „tady je moje budova“, špendlík jde přetáhnout, obrys z katastru ukáže, kterou budovu
// aplikace našla. Šedé špendlíky jsou budovy, které už správce v mapě má.
export function PlaceMap({ home, point, focusKey, place, previewName, category, ownFacilities, onPick }: PlaceMapProps) {
  const [aerial, setAerial] = useState(false);
  const Icon = CATEGORY_META[category].icon;

  return (
    <div data-map-root className="relative h-full w-full bg-slate-100">
      <Map center={home.center} zoom={home.zoom} styles={GRAY_STYLES} theme="light">
        <MapToolbar home={home} />
        <AerialLayer visible={aerial} />
        <PickOnClick onPick={onPick} />
        <FlyToPoint point={point} focusKey={focusKey} />

        {place?.building && (
          <MapGeoJSON
            id="new-building-footprint"
            data={{ type: "Feature", geometry: place.building.footprint, properties: {} }}
            fillPaint={{ "fill-color": "#d90429", "fill-opacity": aerial ? 0.25 : 0.18 }}
            linePaint={{ "line-color": "#d90429", "line-width": 2.5 }}
          />
        )}

        {ownFacilities.map((facility) => (
          <MapMarker key={facility.id} longitude={facility.longitude} latitude={facility.latitude} anchor="bottom">
            <MarkerContent className="opacity-60 grayscale">
              <FacilityPin category={facility.category} status={facility.status} selected={false} />
            </MarkerContent>
            <MarkerTooltip anchor="left" offset={[18, -18]} className={TOOLTIP_CLASS}>
              <p className="text-[12.5px] font-bold">{facility.name}</p>
              <p className="mt-0.5 text-[11px] text-slate-500">Tuhle budovu už v mapě máte</p>
            </MarkerTooltip>
          </MapMarker>
        ))}

        {point && (
          <MapMarker longitude={point.longitude} latitude={point.latitude} anchor="bottom" draggable onDragEnd={(lngLat) => onPick({ latitude: lngLat.lat, longitude: lngLat.lng })}>
            <MarkerContent>
              <span key={`${point.latitude}-${point.longitude}`} className="animate-pin-drop block origin-bottom">
                <span className="relative block cursor-grab drop-shadow-[0_4px_6px_rgba(11,25,44,0.35)] active:cursor-grabbing" style={{ width: 38, height: 48 }}>
                  <span className="absolute -bottom-1.5 left-1/2 size-4 -translate-x-1/2 animate-ping rounded-full bg-hk-red" aria-hidden />
                  <svg viewBox="0 0 28 36" width={38} height={48} className="relative" aria-hidden>
                    <path d="M14 35c-.6 0-1.1-.3-1.4-.8C10 30 2 21.6 2 14 2 7.4 7.4 2 14 2s12 5.4 12 12c0 7.6-8 16-10.6 20.2-.3.5-.8.8-1.4.8Z" fill="#d90429" stroke="#fff" strokeWidth={2} />
                  </svg>
                  <Icon className="absolute top-[9px] left-1/2 size-4.5 -translate-x-1/2 text-white" strokeWidth={2.25} aria-hidden />
                </span>
              </span>
              <MarkerLabel position="bottom" className="mt-2">
                <span className="block max-w-56 truncate rounded-md border border-slate-200 bg-white px-2 py-1 text-[11.5px] font-bold text-hk-navy shadow-sm">
                  {previewName.trim() || "Nová budova"}
                </span>
              </MarkerLabel>
            </MarkerContent>
          </MapMarker>
        )}
      </Map>

      <div className="pointer-events-none absolute top-3 left-3 z-10 flex flex-col items-start gap-2">
        <button
          type="button"
          onClick={() => setAerial((current) => !current)}
          aria-pressed={aerial}
          className="pointer-events-auto inline-flex h-9 items-center gap-2 rounded-lg border border-slate-200 bg-white px-3 text-[12.5px] font-bold text-hk-navy shadow-[0_8px_20px_rgba(11,25,44,0.12)] transition-colors hover:bg-slate-50"
        >
          {aerial ? <MapIcon className="size-4" aria-hidden /> : <ImageIcon className="size-4" aria-hidden />}
          {aerial ? "Mapa" : "Letecký snímek"}
        </button>
        <p
          className={cn(
            "inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white/95 px-3 py-2 text-[12.5px] text-slate-600 shadow-[0_8px_20px_rgba(11,25,44,0.08)]",
            point && "hidden sm:inline-flex",
          )}
        >
          <MousePointerClick className="size-4 text-hk-red" aria-hidden />
          {point ? "Není to ono? Klikněte na správnou budovu, nebo špendlík přetáhněte" : "Klikněte na svou budovu v mapě"}
        </p>
      </div>
    </div>
  );
}

// Klik do mapy umístí špendlík; kurzor zaměřovač napovídá, že se tu míří
function PickOnClick({ onPick }: { onPick: (point: PinnedPoint) => void }) {
  const { map, isLoaded } = useMap();

  useEffect(() => {
    if (!map || !isLoaded) return;

    function handleClick(event: { lngLat: { lat: number; lng: number } }) {
      onPick({ latitude: event.lngLat.lat, longitude: event.lngLat.lng });
    }

    const canvas = map.getCanvas();
    canvas.style.cursor = "crosshair";
    map.on("click", handleClick);

    return () => {
      canvas.style.cursor = "";
      map.off("click", handleClick);
    };
  }, [map, isLoaded, onPick]);

  return null;
}

function FlyToPoint({ point, focusKey }: { point: PinnedPoint | null; focusKey: number }) {
  const { map, isLoaded } = useMap();
  const latitude = point?.latitude;
  const longitude = point?.longitude;

  useEffect(() => {
    if (!map || !isLoaded || focusKey === 0 || latitude === undefined || longitude === undefined) return;

    map.flyTo({ center: [longitude, latitude], zoom: Math.max(map.getZoom(), BUILDING_ZOOM), pitch: 0, bearing: 0, duration: 1400, essential: true });
    // Přelet jen při nové žádosti (focusKey), ne při každém posunu špendlíku
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [map, isLoaded, focusKey]);

  return null;
}

// Letecký snímek pod popisky mapy, aby názvy ulic a čísla domů zůstaly čitelné
function AerialLayer({ visible }: { visible: boolean }) {
  const { map, isLoaded } = useMap();

  useEffect(() => {
    if (!map || !isLoaded || !visible) return;

    if (!map.getSource(AERIAL_SOURCE)) {
      map.addSource(AERIAL_SOURCE, { type: "raster", tiles: [AERIAL_TILES], tileSize: 256, maxzoom: 20, attribution: "Ortofoto © ČÚZK" });
    }

    // Nad šedé plochy, silnice i budovy podkladu (jinak by snímek zakryly), pod první popisky za nimi
    const layers = map.getStyle().layers;
    const lastBuilding = layers.findLastIndex((layer) => "source-layer" in layer && layer["source-layer"] === "building");
    const firstLabel = layers.slice(lastBuilding + 1).find((layer) => layer.type === "symbol")?.id;
    map.addLayer({ id: AERIAL_LAYER, type: "raster", source: AERIAL_SOURCE }, firstLabel);

    return () => {
      if (map.getLayer(AERIAL_LAYER)) map.removeLayer(AERIAL_LAYER);
    };
  }, [map, isLoaded, visible]);

  return null;
}
