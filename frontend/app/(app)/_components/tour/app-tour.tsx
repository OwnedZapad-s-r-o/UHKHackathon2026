"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { ArrowLeft, ArrowRight, Check, Compass, X } from "lucide-react";
import { withParams } from "@/lib/search-params";
import { cn } from "@/lib/utils";
import type { Role } from "@/types/api";
import { tourSteps, type TourPhase, type TourStep } from "./tour-steps";

// Hotový (nebo přeskočený) průvodce se pamatuje v prohlížeči pro každou roli zvlášť.
// Demo účty ho dostanou po každém přihlášení (?pruvodce=1 z přihlášení), skutečné účty jen napoprvé.
const SEEN_KEY = (role: Role) => `smartaudit:pruvodce:v1:${role}`;
// Pokračování průvodce po otevření budovy — přežije navigaci, ne zavření karty
const NEXT_PHASE_KEY = "smartaudit:pruvodce:dalsi";

const CARD_WIDTH = 344;
const GAP = 16;
const SPOTLIGHT_PADDING = 6;

export type TourFacility = { id: string; districtCode: string };

type AppTourProps = {
  role: Role;
  // Budova pro poslední krok: nejvážnější zjištění, jinak první budova
  facility: TourFacility | null;
};

type ActiveTour = { phase: TourPhase; steps: TourStep[]; index: number };

export function AppTour({ role, facility }: AppTourProps) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [tour, setTour] = useState<ActiveTour | null>(null);
  // Průvodce aplikací se v jedné relaci stránky sám spustí jen jednou; znovu jen na výslovné ?pruvodce=1
  const started = useRef(false);

  // Kroky, jejichž prvek na stránce není (např. postranní panel na mobilu), se vynechají — počítadlo pak sedí
  const begin = useCallback(
    (phase: TourPhase) => {
      const steps = tourSteps(role, phase).filter((step) => !step.target || findTarget(step.target));
      if (steps.length > 0) setTour({ phase, steps, index: 0 });
    },
    [role],
  );

  // Kdy průvodce spustit: ?pruvodce=1 (demo, tlačítko v menu), první návštěva mapy, nebo pokračování v otevřené budově
  useEffect(() => {
    if (pathname !== "/mapa") return;

    if (readStorage(sessionStorage, NEXT_PHASE_KEY) === "objekt" && searchParams.get("objekt")) {
      // Panel budovy dorazí streamem — počkat, až je v DOM (nejdéle ~6 s)
      let attempts = 0;
      const timer = setInterval(() => {
        if (findTarget("panel-zalozky") || ++attempts > 30) {
          clearInterval(timer);
          removeStorage(sessionStorage, NEXT_PHASE_KEY);
          begin("objekt");
        }
      }, 200);
      return () => clearInterval(timer);
    }

    const forced = searchParams.get("pruvodce") === "1";
    if (!forced && (started.current || readStorage(localStorage, SEEN_KEY(role)))) return;

    // Krátká prodleva: mapa a legenda se musí vykreslit, aby je šlo zvýraznit
    const timer = setTimeout(
      () => {
        started.current = true;
        begin("aplikace");
        if (forced) router.replace(withParams(pathname, searchParams, { pruvodce: null }), { scroll: false });
      },
      forced ? 700 : 1200,
    );
    return () => clearTimeout(timer);
  }, [pathname, searchParams, router, role, begin]);

  const close = useCallback(() => {
    writeStorage(localStorage, SEEN_KEY(role), "1");
    setTour(null);
  }, [role]);

  const openFacility = useCallback(() => {
    close();
    if (!facility) return;
    writeStorage(sessionStorage, NEXT_PHASE_KEY, "objekt");
    const panel = role === "Verejnost" ? "telemetrie" : "audit";
    router.push(`/mapa?okres=${facility.districtCode}&objekt=${facility.id}&panel=${panel}`);
  }, [close, facility, role, router]);

  if (!tour) return null;

  const step = tour.steps[tour.index];
  const isLast = tour.index === tour.steps.length - 1;

  return (
    <TourOverlay
      // Klíč jen podle fáze: mezi kroky se zvýraznění plynule přesune z předchozího prvku na další
      key={tour.phase}
      step={step}
      index={tour.index}
      total={tour.steps.length}
      onBack={tour.index > 0 ? () => setTour({ ...tour, index: tour.index - 1 }) : undefined}
      onNext={isLast ? close : () => setTour({ ...tour, index: tour.index + 1 })}
      onClose={close}
      onFinale={step.finale && facility ? openFacility : undefined}
      isLast={isLast}
    />
  );
}

type TourOverlayProps = {
  step: TourStep;
  index: number;
  total: number;
  isLast: boolean;
  onBack?: () => void;
  onNext: () => void;
  onClose: () => void;
  onFinale?: () => void;
};

function TourOverlay({ step, index, total, isLast, onBack, onNext, onClose, onFinale }: TourOverlayProps) {
  // Počáteční poloha hned při vzniku kroku (běží jen v prohlížeči), ať karta neproblikne uprostřed
  const [layout, setLayout] = useState(() => ({ rect: step.target ? (findTarget(step.target)?.getBoundingClientRect() ?? null) : null, cardHeight: 220 }));
  const cardRef = useRef<HTMLDivElement>(null);
  const primaryRef = useRef<HTMLButtonElement>(null);
  const titleId = `pruvodce-${index}`;

  // Poloha zvýrazněného prvku — sleduje se průběžně (mapa mění velikost, panel dorazí streamem, okno se zúží)
  useEffect(() => {
    if (!step.target) return;
    const target = step.target;
    findTarget(target)?.scrollIntoView({ block: "nearest", inline: "nearest" });

    function measure() {
      const element = findTarget(target);
      setLayout({ rect: element ? element.getBoundingClientRect() : null, cardHeight: cardRef.current?.offsetHeight ?? 220 });
    }

    const frame = requestAnimationFrame(measure);
    const timer = setInterval(measure, 250);
    window.addEventListener("resize", measure);
    return () => {
      cancelAnimationFrame(frame);
      clearInterval(timer);
      window.removeEventListener("resize", measure);
    };
  }, [step.target]);

  useEffect(() => {
    primaryRef.current?.focus({ preventScroll: true });
  }, [index]);

  useEffect(() => {
    function handleKey(event: KeyboardEvent) {
      if (event.key === "Escape") onClose();
      else if (event.key === "ArrowRight") onNext();
      else if (event.key === "ArrowLeft") onBack?.();
    }

    window.addEventListener("keydown", handleKey);
    return () => window.removeEventListener("keydown", handleKey);
  }, [onBack, onNext, onClose]);

  // Úvodní karta (s body „co tu najdete“) má jiná tlačítka než ostatní kroky
  const intro = Boolean(step.highlights);
  const spotlight = step.target ? layout.rect : null;
  const centered = !spotlight;
  const position = spotlight ? placeCard(spotlight, layout.cardHeight) : null;

  return (
    <div className="fixed inset-0 z-100" role="dialog" aria-modal="true" aria-labelledby={titleId}>
      {/* Klik mimo kartu nic nedělá — průvodce se zavírá jen tlačítkem nebo Esc */}
      {centered ? (
        <div className="absolute inset-0 animate-in bg-hk-navy/60 backdrop-blur-[2px] fade-in duration-300" />
      ) : (
        <>
          <div className="absolute inset-0" />
          <div
            className="pointer-events-none absolute rounded-xl shadow-[0_0_0_9999px_rgba(11,25,44,0.58)] ring-2 ring-white/90 transition-[top,left,width,height] duration-300 ease-out"
            style={{
              top: spotlight.top - SPOTLIGHT_PADDING,
              left: spotlight.left - SPOTLIGHT_PADDING,
              width: spotlight.width + SPOTLIGHT_PADDING * 2,
              height: spotlight.height + SPOTLIGHT_PADDING * 2,
            }}
          />
        </>
      )}

      <div
        ref={cardRef}
        className={cn(
          "absolute animate-in rounded-2xl bg-white text-hk-navy shadow-[0_24px_60px_-12px_rgba(11,25,44,0.45)] fade-in zoom-in-95 duration-200",
          centered ? "top-1/2 left-1/2 w-[min(510px,calc(100vw-32px))] -translate-x-1/2 -translate-y-1/2" : "w-[min(350px,calc(100vw-32px))]",
        )}
        style={position ?? undefined}
      >
        <div className="flex items-center justify-between gap-3 px-5 pt-4">
          <span className="inline-flex items-center gap-1.5 text-[11px] font-bold tracking-wide text-hk-red uppercase">
            <Compass className="size-3.5" aria-hidden />
            Průvodce · {index + 1} / {total}
          </span>
          <button
            type="button"
            onClick={onClose}
            aria-label="Zavřít průvodce"
            title="Zavřít průvodce (Esc)"
            className="flex size-7 items-center justify-center rounded-md text-slate-400 transition-colors hover:bg-slate-100 hover:text-hk-navy"
          >
            <X className="size-4" />
          </button>
        </div>

        <div className="px-5 pt-2 pb-4">
          <h2 id={titleId} className={cn("font-bold tracking-tight", centered ? "text-[22px] leading-tight" : "text-[17px]")}>
            {step.title}
          </h2>
          <p className="mt-1.5 text-[13.5px] leading-relaxed text-slate-600">{step.body}</p>

          {step.highlights && (
            <ol className="mt-4 space-y-2.5">
              {step.highlights.map((item, i) => (
                <li key={item.title} className="flex items-start gap-3 rounded-xl bg-slate-50 px-3 py-2.5">
                  <span className="flex size-6 shrink-0 items-center justify-center rounded-full bg-hk-navy font-mono text-[11px] font-bold text-white">{i + 1}</span>
                  <span className="text-[13px] leading-snug">
                    <span className="block font-bold">{item.title}</span>
                    <span className="text-slate-600">{item.text}</span>
                  </span>
                </li>
              ))}
            </ol>
          )}
        </div>

        <div className="flex gap-1 px-5" aria-hidden>
          {Array.from({ length: total }, (_, i) => (
            <span key={i} className={cn("h-1 flex-1 rounded-full transition-colors", i <= index ? "bg-hk-red" : "bg-slate-200")} />
          ))}
        </div>

        <div className="flex flex-wrap items-center justify-between gap-3 px-5 pt-3.5 pb-4">
          {intro ? (
            <button type="button" onClick={onClose} className="text-[13px] text-slate-500 transition-colors hover:text-hk-navy">
              Teď ne
            </button>
          ) : (
            onBack && (
              <button
                type="button"
                onClick={onBack}
                className="inline-flex h-9 shrink-0 items-center gap-1.5 rounded-lg border border-slate-200 px-3 text-[13px] font-medium transition-colors hover:bg-slate-50"
              >
                <ArrowLeft className="size-3.5" aria-hidden />
                Zpět
              </button>
            )
          )}

          <div className="ml-auto flex items-center gap-3">
            {onFinale ? (
              <>
                <button
                  type="button"
                  onClick={onClose}
                  className="whitespace-nowrap px-2.5 py-1.5 text-[13px] font-medium text-slate-500 transition-colors hover:text-hk-navy"
                >
                  {step.finale?.skip}
                </button>
                <PrimaryButton ref={primaryRef} onClick={onFinale}>
                  {step.finale?.action}
                  <ArrowRight className="size-3.5" aria-hidden />
                </PrimaryButton>
              </>
            ) : (
              <PrimaryButton ref={primaryRef} onClick={onNext}>
                {intro ? "Začít prohlídku" : isLast ? "Hotovo" : "Další"}
                {isLast ? <Check className="size-3.5" aria-hidden /> : <ArrowRight className="size-3.5" aria-hidden />}
              </PrimaryButton>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

function PrimaryButton({ ref, onClick, children }: { ref: React.Ref<HTMLButtonElement>; onClick: () => void; children: React.ReactNode }) {
  return (
    <button
      ref={ref}
      type="button"
      onClick={onClick}
      className="inline-flex min-h-9 shrink-0 items-center justify-center gap-1.5 rounded-lg bg-hk-accent px-4 py-2 text-[13px] font-bold text-white whitespace-nowrap transition-colors hover:bg-hk-red focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-hk-red"
    >
      {children}
    </button>
  );
}

// Karta vedle zvýrazněného prvku: vpravo, vlevo, pod, nad — a když se nikam nevejde (celá mapa), uvnitř prvku vpravo nahoře
function placeCard(rect: DOMRect, cardHeight: number): React.CSSProperties {
  const width = Math.min(CARD_WIDTH, window.innerWidth - 32);
  const vw = window.innerWidth;
  const vh = window.innerHeight;
  const clampTop = (top: number) => Math.max(16, Math.min(top, vh - cardHeight - 16));
  const clampLeft = (left: number) => Math.max(16, Math.min(left, vw - width - 16));
  const middle = rect.top + rect.height / 2 - cardHeight / 2;

  if (rect.right + GAP + width <= vw - 16) return { top: clampTop(middle), left: rect.right + GAP };
  if (rect.left - GAP - width >= 16) return { top: clampTop(middle), left: rect.left - GAP - width };
  if (rect.bottom + GAP + cardHeight <= vh - 16) return { top: rect.bottom + GAP, left: clampLeft(rect.left) };
  if (rect.top - GAP - cardHeight >= 16) return { top: rect.top - GAP - cardHeight, left: clampLeft(rect.left) };
  return { top: clampTop(rect.top + 24), left: clampLeft(rect.right - width - 24) };
}

// Prvek může být na stránce dvakrát (postranní panel pro počítač i mobilní menu) — bere se ten viditelný
function findTarget(target: string) {
  return [...document.querySelectorAll<HTMLElement>(`[data-tour="${target}"]`)].find((element) => {
    const rect = element.getBoundingClientRect();
    return rect.width > 0 && rect.height > 0;
  });
}

// Úložiště prohlížeče může být zakázané (soukromé okno) — průvodce pak prostě funguje bez paměti
function readStorage(storage: Storage, key: string) {
  try {
    return storage.getItem(key);
  } catch {
    return null;
  }
}

function writeStorage(storage: Storage, key: string, value: string) {
  try {
    storage.setItem(key, value);
  } catch {
    // bez paměti
  }
}

function removeStorage(storage: Storage, key: string) {
  try {
    storage.removeItem(key);
  } catch {
    // bez paměti
  }
}
