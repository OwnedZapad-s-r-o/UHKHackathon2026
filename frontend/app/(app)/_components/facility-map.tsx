"use client";

import { useEffect, useRef, useState } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { GuideHighlights } from "@/components/guide/guide-highlights";
import { Map, MapMarker, MarkerContent, MarkerLabel, MarkerTooltip, useMap } from "@/components/ui/map";
import type { DistrictStats } from "@/lib/district-shading";
import { MAP_HOME_EVENT } from "@/lib/map-events";
import { withParams } from "@/lib/search-params";
import type { AuditStatus, FacilityListItem } from "@/types/api";
import { BuildingEditor, parseBuildingEditMode } from "./building-editor";
import { DistrictLayer } from "./district-layer";
import { FacilityDots } from "./facility-dots";
import { FacilityPin, FacilityTooltipContent, SelectedLabel, TOOLTIP_CLASS } from "./facility-pin";
import { MapClickRouter } from "./map-click-router";
import { MapLegend } from "./map-legend";
import { MapToolbar } from "./map-toolbar";
import { SelectedBuilding3D } from "./selected-building-3d";

// Šedý podklad vygenerovaný skriptem scripts/make-gray-style.mjs z výchozích stylů CARTO
const GRAY_STYLES = {
  light: "/map-styles/gray-light.json",
  dark: "/map-styles/gray-dark.json",
};

// Výchozí výřez — celý Královéhradecký kraj
export const KHK_HOME = { center: [15.845, 50.352] as [number, number], zoom: 8.6 };

// Pohled na vybranou budovu — přiblížení a náklon, aby byla vidět ve 3D
const BUILDING_VIEW = { zoom: 17.2, pitch: 60, bearing: -20 };

// Pořadí vykreslení — pozdější marker leží nahoře, problémové objekty nesmí zmizet pod zelenými
const DRAW_ORDER: Record<AuditStatus, number> = {
  Nominal: 0,
  Warning: 1,
  Critical: 2,
};

// Od tohoto přiblížení se místo teček ukážou špendlíky i bez výběru okresu (uživatel si přiblížil sám)
const PIN_ZOOM = 10.5;

// Tooltip vpravo od hlavy špendlíku (anchor bottom → hlava je ~18 px nad bodem), aby nezakryl budovu
const TOOLTIP_OFFSET: [number, number] = [18, -18];

type FacilityMapProps = {
  facilities: FacilityListItem[];
  selectedId: string | null;
  selectedDistrict: string | null;
  districtStats: Record<string, DistrictStats>;
  // Správce budovy: jen jeho budovy — bez okresů a teček, výřez na jeho budovy
  ownBuildings?: boolean;
};

export function FacilityMap({ facilities, selectedId, selectedDistrict, districtStats, ownBuildings = false }: FacilityMapProps) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  // Vybraný objekt se kreslí poslední, aby ho nepřekryly okolní špendlíky
  const ordered = facilities.toSorted((a, b) => Number(a.id === selectedId) - Number(b.id === selectedId) || DRAW_ORDER[a.status] - DRAW_ORDER[b.status]);
  const selected = facilities.find((f) => f.id === selectedId) ?? null;
  const [zoomedIn, setZoomedIn] = useState(false);

  // Dvě úrovně: celý kraj = štítky okresů a tečky, vybraný okres (nebo objekt, nebo přiblížení) = špendlíky.
  // Seznam objektů je při vybraném okresu už filtrovaný, takže se ukážou jen špendlíky toho okresu.
  const showPins = ownBuildings || selectedDistrict !== null || selected !== null || zoomedIn;
  const home = ownBuildings ? viewOf(facilities) : KHK_HOME;

  // Úpravy budov vybraného objektu (?budova=doplnit | vymenit:{id} | obrys:{id}) — mapa patří editoru:
  // bez 3D zvýraznění (překrývalo by body obrysu), bez špendlíků a bez kliků, které mění výběr
  const editMode = selected ? parseBuildingEditMode(searchParams.get("budova")) : null;

  function navigate(changes: Record<string, string | null>) {
    router.push(withParams(pathname, searchParams, changes), { scroll: false });
  }

  return (
    <div data-map-root data-tour="mapa" className="relative h-full w-full bg-slate-100">
      {/* Prototyp je světlý — mapa drží světlý šedý styl i při tmavém systému */}
      <Map center={home.center} zoom={home.zoom} styles={GRAY_STYLES} theme="light">
        <MapToolbar home={home} />
        {!ownBuildings && (
          <>
            <DistrictLayer
              stats={districtStats}
              selectedCode={selectedDistrict}
              facilitySelected={selected !== null}
              home={KHK_HOME}
              showLabels={!showPins}
              onSelect={(code) => navigate({ okres: code, objekt: null, panel: null })}
              onClear={() => navigate({ okres: null })}
            />
            <FacilityDots facilities={facilities} visible={!showPins} />
          </>
        )}
        <ZoomThreshold threshold={PIN_ZOOM} onChange={setZoomedIn} />
        <SelectedBuilding3D facilityId={editMode ? null : (selected?.id ?? null)} version={selected?.footprintVersion ?? "0"} status={selected?.status ?? "Nominal"} />
        {selected && editMode && <BuildingEditor key={`${selected.id}-${searchParams.get("budova")}`} facilityId={selected.id} mode={editMode} />}
        <FlyToSelected
          facilityId={selected?.id ?? null}
          longitude={selected?.longitude}
          latitude={selected?.latitude}
          districtSelected={selectedDistrict !== null}
          home={home}
        />
        <MapClickRouter
          facilitySelected={selected !== null}
          districtSelected={selectedDistrict !== null}
          onSelectDistrict={(code) => navigate({ okres: code, objekt: null, panel: null })}
          onClearDistrict={() => navigate({ okres: null, objekt: null, panel: null })}
          onDeselectFacility={() => navigate({ objekt: null, panel: null })}
          disabled={editMode !== null}
        />

        {/* Očíslované objekty z odpovědi průvodce — klik otevře objekt i s jeho okresem */}
        {!editMode && <GuideHighlights onSelect={(item) => navigate({ okres: ownBuildings ? null : item.districtCode, objekt: item.id })} />}

        {showPins && !editMode && ordered.map((facility, index) => {
          const isSelected = facility.id === selectedId;

          return (
            <MapMarker
              key={facility.id}
              longitude={facility.longitude}
              latitude={facility.latitude}
              anchor="bottom"
              onClick={() => navigate({ objekt: facility.id })}
            >
              <MarkerContent>
                <FacilityPin category={facility.category} status={facility.status} selected={isSelected} dropDelay={Math.min(index * 14, 450)} />
                {isSelected && (
                  <MarkerLabel position="bottom" className="mt-2">
                    <SelectedLabel facility={facility} />
                  </MarkerLabel>
                )}
              </MarkerContent>
              {/* Vybraný objekt má štítek — tooltip by jen zakrýval jeho budovu */}
              {!isSelected && (
                <MarkerTooltip anchor="left" offset={TOOLTIP_OFFSET} className={TOOLTIP_CLASS}>
                  <FacilityTooltipContent facility={facility} />
                </MarkerTooltip>
              )}
            </MapMarker>
          );
        })}
      </Map>

      <MapLegend facilities={facilities} showDistricts={!ownBuildings && selected === null && !zoomedIn} showCategories={!ownBuildings} />
    </div>
  );
}

// Výřez na budovy správce: střed a přiblížení podle toho, jak daleko od sebe leží (jedna budova = ulice kolem ní)
export function viewOf(facilities: FacilityListItem[]) {
  if (facilities.length === 0) return KHK_HOME;

  const longitudes = facilities.map((f) => f.longitude);
  const latitudes = facilities.map((f) => f.latitude);
  const span = Math.max(Math.max(...longitudes) - Math.min(...longitudes), (Math.max(...latitudes) - Math.min(...latitudes)) * 1.6);
  const zoom = span < 0.004 ? 15.5 : Math.min(15.5, Math.max(KHK_HOME.zoom, Math.log2(360 / span) - 1));

  return {
    center: [(Math.max(...longitudes) + Math.min(...longitudes)) / 2, (Math.max(...latitudes) + Math.min(...latitudes)) / 2] as [number, number],
    zoom,
  };
}

// Hlásí jen překročení prahu přiblížení — ne každý krok zoomu, jinak by se mapa překreslovala při každém pohybu kolečkem
function ZoomThreshold({ threshold, onChange }: { threshold: number; onChange: (above: boolean) => void }) {
  const { map, isLoaded } = useMap();

  useEffect(() => {
    if (!map || !isLoaded) return;

    let above = map.getZoom() >= threshold;
    onChange(above);

    function handleZoom() {
      const next = map!.getZoom() >= threshold;
      if (next === above) return;
      above = next;
      onChange(next);
    }

    map.on("zoom", handleZoom);
    return () => {
      map.off("zoom", handleZoom);
    };
  }, [map, isLoaded, threshold, onChange]);

  return null;
}

type FlyToSelectedProps = {
  facilityId: string | null;
  longitude?: number;
  latitude?: number;
  districtSelected: boolean;
  home: { center: [number, number]; zoom: number };
};

// Po výběru objektu mapa přeletí k budově a nakloní se, aby byla vidět ve 3D;
// po zrušení výběru se jen srovná náklon — výřez zůstane, uživatel se nemá kde ztratit.
// Je-li vybraný okres, kameru po zavření objektu vede DistrictLayer (přelet zpět na celý okres).
// „Celý kraj“ v drobečkové navigaci (requestMapHome) přeletí na celý kraj hned, ještě než se změní URL —
// náklon se pak už nesrovnává, easeTo by přelet přerušil.
function FlyToSelected({ facilityId, longitude, latitude, districtSelected, home }: FlyToSelectedProps) {
  const { map, isLoaded } = useMap();
  const hadSelection = useRef(false);
  const homeRequested = useRef(false);

  useEffect(() => {
    if (!map || !isLoaded) return;

    function flyHome() {
      homeRequested.current = true;
      map!.flyTo({ center: home.center, zoom: home.zoom, pitch: 0, bearing: 0, duration: 1100, essential: true });
    }

    window.addEventListener(MAP_HOME_EVENT, flyHome);
    return () => window.removeEventListener(MAP_HOME_EVENT, flyHome);
  }, [map, isLoaded, home]);

  useEffect(() => {
    if (!map || !isLoaded) return;

    if (facilityId && longitude !== undefined && latitude !== undefined) {
      hadSelection.current = true;
      homeRequested.current = false;
      map.flyTo({ center: [longitude, latitude], ...BUILDING_VIEW, duration: 1600, essential: true });
    } else if (hadSelection.current) {
      hadSelection.current = false;
      if (!districtSelected && !homeRequested.current) map.easeTo({ pitch: 0, bearing: 0, duration: 700, essential: true });
      homeRequested.current = false;
    }
  }, [map, isLoaded, facilityId, longitude, latitude, districtSelected]);

  return null;
}
