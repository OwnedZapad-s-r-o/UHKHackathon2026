"use client";

import { useEffect, useState } from "react";
import { LngLatBounds, type PaddingOptions } from "maplibre-gl";
import { MapMarker, MarkerContent, MarkerTooltip, useMap } from "@/components/ui/map";
import { STATUS_META } from "@/lib/facility-meta";
import { GUIDE_HIGHLIGHT_EVENT, GUIDE_HOVER_EVENT, type GuideHighlight } from "@/lib/guide-events";
import { cn } from "@/lib/utils";

type GuideHighlightsProps = {
  onSelect: (item: GuideHighlight) => void;
};

// Objekty z odpovědi průvodce jako očíslované značky (číslo = karta v průvodci) — mapa je všechny ukáže naráz.
// Vrstva leží nad běžnými špendlíky i tečkami, takže funguje v pohledu na kraj, okres i na veřejné mapě.
export function GuideHighlights({ onSelect }: GuideHighlightsProps) {
  const { map, isLoaded } = useMap();
  const [items, setItems] = useState<GuideHighlight[]>([]);
  const [hovered, setHovered] = useState<string | null>(null);

  useEffect(() => {
    function handleHighlight(event: Event) {
      setItems((event as CustomEvent<GuideHighlight[] | null>).detail ?? []);
    }

    function handleHover(event: Event) {
      setHovered((event as CustomEvent<string | null>).detail);
    }

    window.addEventListener(GUIDE_HIGHLIGHT_EVENT, handleHighlight);
    window.addEventListener(GUIDE_HOVER_EVENT, handleHover);
    return () => {
      window.removeEventListener(GUIDE_HIGHLIGHT_EVENT, handleHighlight);
      window.removeEventListener(GUIDE_HOVER_EVENT, handleHover);
    };
  }, []);

  // Nové značky = nový výřez: všechny objekty odpovědi v záběru, mapa srovnaná do 2D
  useEffect(() => {
    if (!map || !isLoaded || items.length === 0) return;

    if (items.length === 1) {
      map.flyTo({ center: [items[0].longitude, items[0].latitude], zoom: 14, pitch: 0, bearing: 0, padding: panelPadding(map.getContainer(), 0), duration: 1200, essential: true });
      return;
    }

    const bounds = new LngLatBounds();
    for (const item of items) bounds.extend([item.longitude, item.latitude]);
    map.fitBounds(bounds, { padding: panelPadding(map.getContainer(), 70), maxZoom: 13.5, pitch: 0, bearing: 0, duration: 1300, essential: true });
  }, [map, isLoaded, items]);

  return items.map((item) => {
    const active = hovered === item.id;

    return (
      <MapMarker key={item.id} longitude={item.longitude} latitude={item.latitude} anchor="center" onClick={() => onSelect(item)}>
        <MarkerContent>
          <span className="relative block cursor-pointer" style={{ zIndex: active ? 2 : 1 }}>
            <span className={cn("absolute inset-0 animate-ping rounded-full opacity-60", STATUS_META[item.status].dot, !active && "[animation-duration:2.4s]")} aria-hidden />
            <span
              className={cn(
                "animate-pin-drop relative flex items-center justify-center rounded-full border-2 border-white bg-hk-navy font-bold text-white tabular-nums shadow-[0_6px_14px_rgba(11,25,44,0.35)] transition-transform duration-150",
                active ? "size-9 scale-110 text-[14px]" : "size-7.5 text-[12.5px]",
              )}
              style={{ animationDelay: `${item.rank * 60}ms` }}
            >
              {item.rank}
              <span className={cn("absolute -right-0.5 -bottom-0.5 size-2.5 rounded-full border-2 border-white", STATUS_META[item.status].dot)} aria-hidden />
            </span>
          </span>
        </MarkerContent>
        <MarkerTooltip anchor="left" offset={[18, 0]} className="rounded-lg border border-slate-200 bg-white px-2.5 py-1.5 text-[12px] font-bold text-hk-navy shadow-md">
          {item.rank}. {item.name}
        </MarkerTooltip>
      </MapMarker>
    );
  });
}

// Místo, které nad mapou zabírá otevřený panel průvodce (vlevo, na mobilu dole) — výřez se mu vyhne
function panelPadding(container: HTMLElement, base: number): PaddingOptions {
  const padding = { top: base, right: base, bottom: base, left: base };
  return container.clientWidth >= 640 ? { ...padding, left: base + 400 } : { ...padding, bottom: base + Math.round(container.clientHeight * 0.55) };
}
