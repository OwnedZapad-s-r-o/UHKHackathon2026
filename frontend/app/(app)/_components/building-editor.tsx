"use client";

import { useEffect, useRef, useState, useTransition } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { Feature, FeatureCollection, Geometry, Position } from "geojson";
import type { GeoJSONSource, MapLayerMouseEvent, MapMouseEvent, Map as MapLibreMap } from "maplibre-gl";
import { Check, Loader2, MousePointerClick, PenLine, RotateCcw, X } from "lucide-react";
import { useMap } from "@/components/ui/map";
import { withParams } from "@/lib/search-params";
import { addBuildingPart, replaceBuildingPart, type OutlinePolygon } from "./building-actions";

// Režim editoru z URL (?budova=…): doplnit novou budovu, vyměnit doplněnou za jinou, nebo ručně upravit její obrys
export type BuildingEditMode = { kind: "pick"; partId: string | null } | { kind: "outline"; partId: string };

export function parseBuildingEditMode(value: string | null): BuildingEditMode | null {
  if (value === "doplnit") return { kind: "pick", partId: null };

  const [action, id] = value?.split(":") ?? [];
  if (!id || !/^[0-9a-f-]{36}$/i.test(id)) return null;
  if (action === "vymenit") return { kind: "pick", partId: id };
  if (action === "obrys") return { kind: "outline", partId: id };

  return null;
}

// Budovy podkladové mapy (OpenStreetMap z dlaždic CARTO) — na ně se kliká
const BUILDING_LAYERS = ["building-3d", "building", "building-top"];

const SOURCE_HOVER = "editor-hover";
const SOURCE_OUTLINE = "editor-outline";
const SOURCE_POINTS = "editor-points";
const LAYER_HOVER = "editor-hover-fill";
const LAYER_OUTLINE_FILL = "editor-outline-fill";
const LAYER_OUTLINE_LINE = "editor-outline-line";
const LAYER_POINTS = "editor-points";

const COLOR = "#e11d48";

// Když budova v podkladové mapě chybí, začne se čtvercem 16 × 16 m v místě kliknutí
const NEW_OUTLINE_METERS = 16;

type Stage = "loading" | "pick" | "confirm" | "edit";

type BuildingEditorProps = {
  facilityId: string;
  mode: BuildingEditMode;
};

// Editor budov objektu přímo v mapě. Mapa se srovná do 2D (klikat na budovy a tahat body je tak přesnější),
// výběr objektu a okresu se mezitím nemění (MapClickRouter je vypnutý). Uložení přepočítá plochu i audit.
export function BuildingEditor({ facilityId, mode }: BuildingEditorProps) {
  const { map, isLoaded } = useMap();
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  const [stage, setStage] = useState<Stage>(mode.kind === "outline" ? "loading" : "pick");
  const [ring, setRing] = useState<Position[]>([]);
  const [drawn, setDrawn] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  // Tažení bodu mění obrys po pixelech — drží se v refu a do stavu se zapíše až po puštění myši
  const ringRef = useRef<Position[]>([]);
  const stageRef = useRef<Stage>(stage);

  function updateRing(next: Position[]) {
    ringRef.current = next;
    setRing(next);
    if (map) drawOutline(map, next, stageRef.current === "edit");
  }

  function changeStage(next: Stage) {
    stageRef.current = next;
    setStage(next);
    if (map) drawOutline(map, ringRef.current, next === "edit");
  }

  function close() {
    router.replace(withParams(pathname, searchParams, { budova: null }), { scroll: false });
  }

  // Ruční úprava: přesný obrys doplněné budovy z API (obrys pro 3D je o kus rozšířený)
  useEffect(() => {
    if (mode.kind !== "outline") return;
    let cancelled = false;

    fetch(`/api/facilities/${facilityId}/building-parts`, { cache: "no-store" })
      .then((response) => (response.ok ? (response.json() as Promise<{ id: string; footprint: Geometry }[]>) : []))
      .then((parts) => {
        if (cancelled) return;
        const part = parts.find((p) => p.id === mode.partId);
        const outer = part?.footprint.type === "Polygon" ? part.footprint.coordinates[0] : null;
        if (!outer) {
          setError("Doplněná budova se nenašla — mohl ji mezitím někdo odebrat.");
          return;
        }
        setDrawn(true);
        ringRef.current = outer.slice(0, -1);
        setRing(ringRef.current);
        stageRef.current = "edit";
        setStage("edit");
      })
      .catch(() => !cancelled && setError("Obrys budovy se nepodařilo načíst."));

    return () => {
      cancelled = true;
    };
  }, [facilityId, mode]);

  // Vrstvy editoru, 2D pohled a vypnuté přiblížení dvojklikem po dobu úprav
  useEffect(() => {
    if (!map || !isLoaded) return;

    addLayers(map);
    drawOutline(map, ringRef.current, stageRef.current === "edit");
    map.doubleClickZoom.disable();
    map.easeTo({ pitch: 0, bearing: 0, zoom: Math.max(map.getZoom(), 17.5), duration: 700, essential: true });

    return () => {
      map.getCanvas().style.cursor = "";
      map.doubleClickZoom.enable();
      map.dragPan.enable();
      if (!map.getStyle()) return;
      removeLayers(map);
      // Zpátky do 3D, kde je vidět výsledek
      map.easeTo({ pitch: 60, bearing: -20, duration: 800, essential: true });
    };
  }, [map, isLoaded]);

  // Výběr budovy z podkladové mapy: najetí ji zvýrazní, klik vybere (prázdné místo = nakreslit obrys)
  useEffect(() => {
    if (!map || !isLoaded || stage !== "pick") return;

    function buildingAt(event: MapMouseEvent) {
      const layers = BUILDING_LAYERS.filter((id) => map!.getLayer(id));
      const features = map!.queryRenderedFeatures(event.point, { layers });
      const point: Position = [event.lngLat.lng, event.lngLat.lat];
      for (const feature of features) {
        const outer = outerRingAt(feature.geometry, point);
        if (outer) return outer;
      }
      return null;
    }

    function handleMove(event: MapMouseEvent) {
      const outer = buildingAt(event);
      map!.getCanvas().style.cursor = outer ? "pointer" : "crosshair";
      setSource(map!, SOURCE_HOVER, outer ? [polygonFeature(outer)] : []);
    }

    function handleClick(event: MapMouseEvent) {
      const outer = buildingAt(event);
      setSource(map!, SOURCE_HOVER, []);
      setError(null);

      if (outer) {
        setDrawn(false);
        updateRing(outer.slice(0, -1));
        changeStage("confirm");
      } else {
        setDrawn(true);
        updateRing(squareAround([event.lngLat.lng, event.lngLat.lat], NEW_OUTLINE_METERS));
        changeStage("edit");
      }
    }

    map.getCanvas().style.cursor = "crosshair";
    map.on("mousemove", handleMove);
    map.on("click", handleClick);

    return () => {
      map.off("mousemove", handleMove);
      map.off("click", handleClick);
      map.getCanvas().style.cursor = "";
      setSource(map, SOURCE_HOVER, []);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps -- updateRing/changeStage pracují jen s refy a stavem
  }, [map, isLoaded, stage]);

  // Ruční úprava obrysu: tažení bodů, tažením bodu uprostřed hrany vznikne nový, pravý klik bod smaže
  useEffect(() => {
    if (!map || !isLoaded || stage !== "edit") return;

    // Obrys z API může dorazit až po přidání vrstev (editor otevřený nad už načtenou mapou) — nakreslit ho tady
    drawOutline(map, ringRef.current, true);

    let dragging: number | null = null;

    function handleDown(event: MapLayerMouseEvent) {
      const properties = event.features?.[0]?.properties;
      if (!properties) return;
      event.preventDefault();

      const index = Number(properties.index);
      if (properties.kind === "mid") {
        const next = [...ringRef.current];
        next.splice(index + 1, 0, [event.lngLat.lng, event.lngLat.lat]);
        updateRing(next);
        dragging = index + 1;
      } else {
        dragging = index;
      }
      map!.dragPan.disable();
    }

    function handleMove(event: MapMouseEvent) {
      if (dragging === null) return;
      const next = [...ringRef.current];
      next[dragging] = [event.lngLat.lng, event.lngLat.lat];
      ringRef.current = next;
      drawOutline(map!, next, true);
    }

    function handleUp() {
      if (dragging === null) return;
      dragging = null;
      map!.dragPan.enable();
      setRing(ringRef.current);
    }

    function handleContextMenu(event: MapLayerMouseEvent) {
      const properties = event.features?.[0]?.properties;
      if (properties?.kind !== "vertex" || ringRef.current.length <= 3) return;
      event.preventDefault();
      updateRing(ringRef.current.filter((_, i) => i !== Number(properties.index)));
    }

    function handleEnter() {
      map!.getCanvas().style.cursor = "grab";
    }

    function handleLeave() {
      if (dragging === null) map!.getCanvas().style.cursor = "";
    }

    map.on("mousedown", LAYER_POINTS, handleDown);
    map.on("mousemove", handleMove);
    map.on("mouseup", handleUp);
    map.on("contextmenu", LAYER_POINTS, handleContextMenu);
    map.on("mouseenter", LAYER_POINTS, handleEnter);
    map.on("mouseleave", LAYER_POINTS, handleLeave);

    return () => {
      map.off("mousedown", LAYER_POINTS, handleDown);
      map.off("mousemove", handleMove);
      map.off("mouseup", handleUp);
      map.off("contextmenu", LAYER_POINTS, handleContextMenu);
      map.off("mouseenter", LAYER_POINTS, handleEnter);
      map.off("mouseleave", LAYER_POINTS, handleLeave);
      map.dragPan.enable();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps -- updateRing pracuje jen s refem a stavem
  }, [map, isLoaded, stage]);

  // Esc zruší úpravy (výběr objektu zůstane — MapClickRouter má Esc po dobu úprav vypnutý)
  useEffect(() => {
    function handleKey(event: KeyboardEvent) {
      if (event.key === "Escape") close();
    }

    window.addEventListener("keydown", handleKey);
    return () => window.removeEventListener("keydown", handleKey);
  });

  function save() {
    const geometry: OutlinePolygon = { type: "Polygon", coordinates: [[...ring, ring[0]].map(([lng, lat]) => [lng, lat] as [number, number])] };
    setError(null);
    startTransition(async () => {
      const result = mode.partId ? await replaceBuildingPart(facilityId, mode.partId, geometry, drawn) : await addBuildingPart(facilityId, geometry, drawn);
      if (result?.error) {
        setError(result.error);
        return;
      }
      router.replace(withParams(pathname, searchParams, { budova: null, panel: "budova" }), { scroll: false });
    });
  }

  const area = ring.length >= 3 ? Math.round(areaSquareMeters(ring)) : 0;
  const replacing = mode.partId !== null;

  return (
    <div className="absolute top-3 left-1/2 z-20 w-[min(34rem,calc(100%-1.5rem))] -translate-x-1/2 rounded-2xl border border-slate-200 bg-white p-4 shadow-[0_12px_32px_rgba(11,25,44,0.18)]">
      <div className="flex items-start gap-3">
        <span className="flex size-9 shrink-0 items-center justify-center rounded-xl bg-rose-50 text-rose-600">
          {stage === "edit" ? <PenLine className="size-4.5" aria-hidden /> : <MousePointerClick className="size-4.5" aria-hidden />}
        </span>
        <div className="min-w-0 flex-1 text-[13px]">
          <p className="font-bold text-hk-navy">{TITLE[stage](replacing, area)}</p>
          <p className="mt-0.5 leading-snug text-slate-600">{HINT[stage]}</p>
          {error && (
            <p role="alert" className="mt-1.5 font-bold text-red-700">
              {error}
            </p>
          )}
        </div>
        <button type="button" onClick={close} aria-label="Zrušit úpravy budovy" className="rounded-lg p-1.5 text-slate-500 hover:bg-slate-100">
          <X className="size-4" aria-hidden />
        </button>
      </div>

      {(stage === "confirm" || stage === "edit") && (
        <div className="mt-3 flex flex-wrap items-center gap-2 border-t border-slate-100 pt-3">
          <button
            type="button"
            onClick={save}
            disabled={isPending || ring.length < 3}
            className="inline-flex h-9 items-center gap-1.5 rounded-lg bg-hk-red px-3 text-[13px] font-bold text-white transition-colors hover:bg-hk-red-dark disabled:opacity-50"
          >
            {isPending ? <Loader2 className="size-4 animate-spin" aria-hidden /> : <Check className="size-4" aria-hidden />}
            {stage === "confirm" ? (replacing ? "Vyměnit za tuto budovu" : "Přidat k objektu") : "Uložit obrys"}
          </button>
          {stage === "confirm" && (
            <button
              type="button"
              onClick={() => {
                setDrawn(true);
                changeStage("edit");
              }}
              className="inline-flex h-9 items-center gap-1.5 rounded-lg border border-slate-200 px-3 text-[13px] font-bold text-hk-navy hover:bg-slate-50"
            >
              <PenLine className="size-4" aria-hidden />
              Upravit obrys
            </button>
          )}
          {mode.kind === "pick" && (
            <button
              type="button"
              onClick={() => {
                updateRing([]);
                changeStage("pick");
              }}
              className="inline-flex h-9 items-center gap-1.5 rounded-lg px-3 text-[13px] text-slate-600 hover:bg-slate-100"
            >
              <RotateCcw className="size-4" aria-hidden />
              Vybrat jinou
            </button>
          )}
        </div>
      )}
    </div>
  );
}

const TITLE: Record<Stage, (replacing: boolean, area: number) => string> = {
  loading: () => "Načítám obrys budovy…",
  pick: (replacing) => (replacing ? "Klikněte na budovu, kterou má doplněná budova nahradit" : "Klikněte na budovu, která k objektu patří"),
  confirm: (_, area) => `Vybraná budova · ${area.toLocaleString("cs-CZ")} m²`,
  edit: (_, area) => `Obrys budovy · ${area.toLocaleString("cs-CZ")} m²`,
};

const HINT: Record<Stage, string> = {
  loading: "",
  pick: "Šedé budovy jsou z mapy OpenStreetMap. Když budova v mapě chybí, klikněte na místo, kde stojí, a obrys nakreslete.",
  confirm: "Plocha se přičte k zastavěné ploše z katastru a ztráta se přepočítá. Nesedí-li obrys, upravte ho.",
  edit: "Táhněte body obrysu. Nový bod přidáte tažením bodu uprostřed hrany, pravým klikem bod smažete.",
};

function addLayers(map: MapLibreMap) {
  if (map.getSource(SOURCE_OUTLINE)) return;

  const empty: FeatureCollection = { type: "FeatureCollection", features: [] };
  map.addSource(SOURCE_HOVER, { type: "geojson", data: empty });
  map.addSource(SOURCE_OUTLINE, { type: "geojson", data: empty });
  map.addSource(SOURCE_POINTS, { type: "geojson", data: empty });

  map.addLayer({ id: LAYER_HOVER, type: "fill", source: SOURCE_HOVER, paint: { "fill-color": COLOR, "fill-opacity": 0.25 } });
  map.addLayer({ id: LAYER_OUTLINE_FILL, type: "fill", source: SOURCE_OUTLINE, paint: { "fill-color": COLOR, "fill-opacity": 0.3 } });
  map.addLayer({ id: LAYER_OUTLINE_LINE, type: "line", source: SOURCE_OUTLINE, paint: { "line-color": COLOR, "line-width": 2.5 } });
  map.addLayer({
    id: LAYER_POINTS,
    type: "circle",
    source: SOURCE_POINTS,
    paint: {
      "circle-radius": ["case", ["==", ["get", "kind"], "vertex"], 6, 4],
      "circle-color": ["case", ["==", ["get", "kind"], "vertex"], "#ffffff", COLOR],
      "circle-opacity": ["case", ["==", ["get", "kind"], "vertex"], 1, 0.55],
      "circle-stroke-color": COLOR,
      "circle-stroke-width": ["case", ["==", ["get", "kind"], "vertex"], 2.5, 0],
    },
  });
}

function removeLayers(map: MapLibreMap) {
  for (const layer of [LAYER_POINTS, LAYER_OUTLINE_LINE, LAYER_OUTLINE_FILL, LAYER_HOVER]) {
    if (map.getLayer(layer)) map.removeLayer(layer);
  }
  for (const source of [SOURCE_POINTS, SOURCE_OUTLINE, SOURCE_HOVER]) {
    if (map.getSource(source)) map.removeSource(source);
  }
}

function setSource(map: MapLibreMap, id: string, features: Feature[]) {
  (map.getSource(id) as GeoJSONSource | undefined)?.setData({ type: "FeatureCollection", features });
}

// Obrys a (při úpravě) body: vrcholy a body uprostřed hran, za které jde vytáhnout nový vrchol
function drawOutline(map: MapLibreMap, ring: Position[], editing: boolean) {
  setSource(map, SOURCE_OUTLINE, ring.length >= 3 ? [polygonFeature([...ring, ring[0]])] : []);

  const points: Feature[] = editing
    ? ring.flatMap((point, index) => {
        const next = ring[(index + 1) % ring.length];
        const mid: Position = [(point[0] + next[0]) / 2, (point[1] + next[1]) / 2];
        return [
          { type: "Feature", geometry: { type: "Point", coordinates: point }, properties: { kind: "vertex", index } },
          { type: "Feature", geometry: { type: "Point", coordinates: mid }, properties: { kind: "mid", index } },
        ] satisfies Feature[];
      })
    : [];
  setSource(map, SOURCE_POINTS, points);
}

function polygonFeature(closedRing: Position[]): Feature {
  return { type: "Feature", geometry: { type: "Polygon", coordinates: [closedRing] }, properties: {} };
}

// Vnější obrys budovy pod kurzorem (MultiPolygon: ta část, na kterou se kliklo)
function outerRingAt(geometry: Geometry, point: Position): Position[] | null {
  const polygons = geometry.type === "Polygon" ? [geometry.coordinates] : geometry.type === "MultiPolygon" ? geometry.coordinates : [];
  const hit = polygons.find((polygon) => pointInRing(point, polygon[0])) ?? polygons[0];
  return hit?.[0] ?? null;
}

function pointInRing([x, y]: Position, ring: Position[]) {
  let inside = false;
  for (let i = 0, j = ring.length - 1; i < ring.length; j = i++) {
    const [xi, yi] = ring[i];
    const [xj, yj] = ring[j];
    if (yi > y !== yj > y && x < ((xj - xi) * (y - yi)) / (yj - yi) + xi) inside = !inside;
  }
  return inside;
}

const METERS_PER_DEGREE = 111_320;

function squareAround([lng, lat]: Position, meters: number): Position[] {
  const dLat = meters / 2 / METERS_PER_DEGREE;
  const dLng = meters / 2 / (METERS_PER_DEGREE * Math.cos((lat * Math.PI) / 180));
  return [
    [lng - dLng, lat - dLat],
    [lng + dLng, lat - dLat],
    [lng + dLng, lat + dLat],
    [lng - dLng, lat + dLat],
  ];
}

// Plocha obrysu v m² (stejný přepočet stupňů na metry jako na serveru)
function areaSquareMeters(ring: Position[]) {
  const scale = Math.cos((ring[0][1] * Math.PI) / 180);
  let sum = 0;
  for (let i = 0; i < ring.length; i++) {
    const [x1, y1] = ring[i];
    const [x2, y2] = ring[(i + 1) % ring.length];
    sum += x1 * y2 - x2 * y1;
  }
  return (Math.abs(sum) / 2) * METERS_PER_DEGREE * METERS_PER_DEGREE * scale;
}
