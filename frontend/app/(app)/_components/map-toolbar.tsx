"use client";

import { useEffect, useState, type ReactNode } from "react";
import { Compass, Crosshair, Maximize2, Minimize2, Minus, Plus } from "lucide-react";
import { useMap } from "@/components/ui/map";

type MapToolbarProps = {
  // Výchozí výřez, na který vrací tlačítko „celý kraj“
  home: { center: [number, number]; zoom: number };
};

// Vlastní ovládání mapy ve stylu prototypu — nahrazuje výchozí MapControls z mapcn
export function MapToolbar({ home }: MapToolbarProps) {
  const { map } = useMap();
  const [isFullscreen, setIsFullscreen] = useState(false);

  // Synchronizace s prohlížečem — celou obrazovku lze opustit i klávesou Esc
  useEffect(() => {
    function handleChange() {
      setIsFullscreen(document.fullscreenElement !== null);
    }

    document.addEventListener("fullscreenchange", handleChange);
    return () => document.removeEventListener("fullscreenchange", handleChange);
  }, []);

  function zoomBy(delta: number) {
    map?.zoomTo(map.getZoom() + delta, { duration: 300 });
  }

  function flyHome() {
    map?.flyTo({ center: home.center, zoom: home.zoom, bearing: 0, pitch: 0, duration: 900 });
  }

  function resetNorth() {
    map?.resetNorthPitch({ duration: 300 });
  }

  function toggleFullscreen() {
    // Na celou obrazovku jde celý blok mapy i s legendou, ne jen plátno
    const container = map?.getContainer().closest("[data-map-root]");
    if (!container) return;

    if (document.fullscreenElement) {
      document.exitFullscreen();
    } else {
      container.requestFullscreen();
    }
  }

  return (
    <div className="absolute right-4 bottom-10 z-10 flex flex-col gap-2">
      <ToolbarGroup>
        <ToolbarButton label="Přiblížit" onClick={() => zoomBy(1)}>
          <Plus />
        </ToolbarButton>
        <ToolbarDivider />
        <ToolbarButton label="Oddálit" onClick={() => zoomBy(-1)}>
          <Minus />
        </ToolbarButton>
      </ToolbarGroup>

      <ToolbarGroup>
        <ToolbarButton label="Zobrazit celý kraj" onClick={flyHome}>
          <Crosshair />
        </ToolbarButton>
        <ToolbarDivider />
        <ToolbarButton label="Natočit na sever" onClick={resetNorth}>
          <Compass />
        </ToolbarButton>
        <ToolbarDivider />
        <ToolbarButton label={isFullscreen ? "Ukončit celou obrazovku" : "Celá obrazovka"} onClick={toggleFullscreen}>
          {isFullscreen ? <Minimize2 /> : <Maximize2 />}
        </ToolbarButton>
      </ToolbarGroup>
    </div>
  );
}

function ToolbarGroup({ children }: { children: ReactNode }) {
  return (
    <div className="flex flex-col overflow-hidden rounded-xl border border-slate-200 bg-white shadow-[0_8px_20px_rgba(11,25,44,0.12)] dark:border-white/10 dark:bg-neutral-900">
      {children}
    </div>
  );
}

function ToolbarDivider() {
  return <span className="mx-2 h-px bg-slate-200 dark:bg-white/10" aria-hidden />;
}

type ToolbarButtonProps = {
  label: string;
  onClick: () => void;
  children: ReactNode;
};

function ToolbarButton({ label, onClick, children }: ToolbarButtonProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      aria-label={label}
      title={label}
      className="flex size-9 items-center justify-center text-hk-navy transition-colors hover:bg-slate-100 focus-visible:ring-2 focus-visible:ring-hk-red focus-visible:outline-none focus-visible:ring-inset dark:text-white dark:hover:bg-white/10 [&_svg]:size-4 [&_svg]:stroke-[1.75]"
    >
      {children}
    </button>
  );
}
