"use client";

import { useCallback, useRef, useState, useTransition, type ReactNode } from "react";
import { Check, Loader2, MapPinPlus } from "lucide-react";
import { cn } from "@/lib/utils";
import type { AddressSuggestion, FacilityCategory, FacilityListItem, Place } from "@/types/api";
import { KHK_HOME, viewOf } from "../../../_components/facility-map";
import { createBuilding } from "../../actions";
import { AddressSearch } from "./address-search";
import { BuildingFields, type BuildingDraft } from "./building-fields";
import { nameSuggestions } from "./building-options";
import { PlaceCard } from "./place-card";
import { PlaceMap, type PinnedPoint } from "./place-map";

type AddBuildingFormProps = {
  organizationName: string | null;
  defaultCategory: FacilityCategory;
  ownFacilities: FacilityListItem[];
};

// Mapa začíná u budov organizace, ale oddálená na čtvrť — nová budova bývá opodál, ne na stejné adrese
const START_MAX_ZOOM = 14;

type PlaceResult = { ok: true; place: Place } | { ok: false; message: string };

async function fetchPlace(params: URLSearchParams, signal: AbortSignal): Promise<PlaceResult> {
  const response = await fetch(`/budovy/nova/misto?${params}`, { signal });
  const body = (await response.json().catch(() => null)) as Place | { message?: string } | null;

  if (!response.ok || body === null || !("territory" in body)) {
    return { ok: false, message: (body as { message?: string } | null)?.message ?? "Místo se nepodařilo dohledat. Zkuste to znovu." };
  }

  return { ok: true, place: body };
}

// Tři kroky na jedné obrazovce vedle mapy: kde budova stojí → co je to za budovu → přidat.
// Všechno, co jde zjistit z dat (obec, okres, půdorys, stáří, památky), se dohledá samo — správce jen kontroluje.
export function AddBuildingForm({ organizationName, defaultCategory, ownFacilities }: AddBuildingFormProps) {
  const start = ownFacilities.length === 0 ? KHK_HOME : viewOf(ownFacilities);
  const home = { center: start.center, zoom: Math.min(start.zoom, START_MAX_ZOOM) };

  const [address, setAddress] = useState("");
  const [point, setPoint] = useState<PinnedPoint | null>(null);
  const [focusKey, setFocusKey] = useState(0);
  const [place, setPlace] = useState<Place | null>(null);
  const [placeLoading, setPlaceLoading] = useState(false);
  const [placeError, setPlaceError] = useState<string | null>(null);
  const [draft, setDraft] = useState<BuildingDraft>({ name: "", category: defaultCategory, kind: null, floors: null, constructionYear: "" });
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();
  const request = useRef<AbortController | null>(null);
  // Rok z katastru se předvyplní, dokud ho správce sám nepřepíše
  const yearEdited = useRef(false);

  // Posouvání špendlíku posílá dotazy rychle za sebou — platí jen odpověď na poslední
  const locate = useCallback(async (params: URLSearchParams, options: { fly: boolean; fillAddress: boolean }) => {
    request.current?.abort();
    const controller = new AbortController();
    request.current = controller;

    setPlaceLoading(true);
    setPlaceError(null);

    try {
      const result = await fetchPlace(params, controller.signal);
      if (!result.ok) {
        setPlace(null);
        setPlaceError(result.message);
        return;
      }

      const found = result.place;
      setPlace(found);
      setPoint({ latitude: found.latitude, longitude: found.longitude });
      if (options.fly) setFocusKey((key) => key + 1);
      if (options.fillAddress) setAddress(found.address?.label ?? "");
      if (!yearEdited.current) setDraft((current) => ({ ...current, constructionYear: found.building?.constructionYear?.toString() ?? "" }));
    } catch (error) {
      if ((error as Error).name !== "AbortError") setPlaceError("Místo se nepodařilo dohledat. Zkontrolujte připojení a zkuste to znovu.");
    } finally {
      if (!controller.signal.aborted) setPlaceLoading(false);
    }
  }, []);

  const pickOnMap = useCallback(
    (picked: PinnedPoint) => {
      setPoint(picked);
      locate(new URLSearchParams({ lat: String(picked.latitude), lon: String(picked.longitude) }), { fly: false, fillAddress: true });
    },
    [locate],
  );

  function selectAddress(suggestion: AddressSuggestion) {
    setAddress(suggestion.label);
    locate(new URLSearchParams({ klic: suggestion.key }), { fly: true, fillAddress: false });
  }

  function locateMe(latitude: number, longitude: number) {
    setPoint({ latitude, longitude });
    setFocusKey((key) => key + 1);
    locate(new URLSearchParams({ lat: String(latitude), lon: String(longitude) }), { fly: false, fillAddress: true });
  }

  function changeDraft(changes: Partial<BuildingDraft>) {
    if (changes.constructionYear !== undefined) yearEdited.current = true;
    setDraft((current) => ({ ...current, ...changes }));
    setSubmitError(null);
  }

  const estimatedFloors = place?.building?.estimatedFloors[draft.category] ?? (draft.category === "Health" ? 4 : 2);
  const year = draft.constructionYear === "" ? null : Number(draft.constructionYear);
  const yearValid = year === null || (year >= 1200 && year <= new Date().getFullYear());
  const duplicate = place?.neighbors.some((n) => n.sameBuilding && n.own) ?? false;
  const missing = !place ? "Nejdřív najděte budovu — adresou nebo kliknutím do mapy." : duplicate ? "Tahle budova už v mapě je." : draft.name.trim().length < 3 ? "Doplňte název budovy." : !yearValid ? "Opravte rok výstavby." : null;

  function submit() {
    if (!place || missing) return;
    setSubmitError(null);

    startTransition(async () => {
      const result = await createBuilding({
        name: draft.name,
        category: draft.category,
        kind: draft.kind,
        latitude: place.latitude,
        longitude: place.longitude,
        street: place.address?.street ?? null,
        houseNumber: place.address?.houseNumber ?? null,
        psc: place.address?.psc ?? null,
        // Bez úpravy správce spočítá odhad podlaží server ze stejného katastru
        floors: draft.floors,
        constructionYear: year,
      });
      if (result?.error) setSubmitError(result.error);
    });
  }

  return (
    <div className="flex min-h-0 flex-1 flex-col lg:flex-row">
      <section className="order-2 flex min-h-0 w-full shrink-0 flex-col border-slate-200 bg-slate-50 lg:order-1 lg:w-[460px] lg:border-r" aria-label="Údaje o budově">
        <div className="flex-1 space-y-7 overflow-y-auto p-5">
          <header>
            <h1 className="text-[22px] leading-tight font-bold tracking-tight">Přidejte budovu do mapy</h1>
            <p className="mt-1.5 text-[13px] text-slate-600">
              Stačí budovu najít. Obec, okres, půdorys, stáří a památkovou ochranu doplníme z otevřených dat — vy jen zkontrolujete.
            </p>
          </header>

          <Step number={1} title="Kde budova stojí" done={place !== null && !duplicate}>
            <AddressSearch value={address} onValueChange={setAddress} onSelect={selectAddress} onLocateMe={locateMe} />
            <PlaceCard place={place} loading={placeLoading} error={placeError} />
          </Step>

          <Step number={2} title="Co je to za budovu" done={place !== null && draft.name.trim().length >= 3} dimmed={!place}>
            <BuildingFields
              draft={draft}
              onChange={changeDraft}
              nameSuggestions={nameSuggestions(organizationName, draft.kind, place, ownFacilities.length > 0)}
              estimatedFloors={estimatedFloors}
              builtUpArea={place?.building?.builtUpArea ?? null}
              cadastreYear={place?.building?.constructionYear ?? null}
            />
          </Step>

          <Step number={3} title="Co se stane po přidání" dimmed={!place}>
            <ul className="space-y-1.5 text-[13px] text-slate-600">
              <li>Budova se objeví v mapě a mezi vašimi budovami{organizationName ? ` (${organizationName})` : ""}.</li>
              <li>Dostane simulovaná čidla teploty, CO₂ a příkonu s týdnem historie — skutečná IQRF čidla připojíte v Senzorech.</li>
              <li>Energetický audit ji vyhodnotí do minuty a zařadí do reportů.</li>
            </ul>
          </Step>
        </div>

        <footer className="border-t border-slate-200 bg-white p-4">
          {submitError && (
            <p role="alert" className="mb-3 rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-[13px] text-red-800">
              {submitError}
            </p>
          )}
          <button
            type="button"
            onClick={submit}
            disabled={missing !== null || isPending || placeLoading}
            className="inline-flex h-12 w-full items-center justify-center gap-2 rounded-lg bg-hk-red px-4 text-sm font-bold text-white transition-colors hover:bg-hk-red-dark disabled:opacity-50"
          >
            {isPending ? <Loader2 className="size-4 animate-spin" aria-hidden /> : <MapPinPlus className="size-4" aria-hidden />}
            {isPending ? "Přidávám budovu…" : "Přidat budovu do mapy"}
          </button>
          {missing && <p className="mt-2 text-center text-xs text-slate-500">{missing}</p>}
        </footer>
      </section>

      <div className="relative order-1 h-72 shrink-0 lg:order-2 lg:h-auto lg:flex-1">
        <PlaceMap
          home={home}
          point={point}
          focusKey={focusKey}
          place={place}
          previewName={draft.name}
          category={draft.category}
          ownFacilities={ownFacilities}
          onPick={pickOnMap}
        />
      </div>
    </div>
  );
}

type StepProps = {
  number: number;
  title: string;
  done?: boolean;
  // Krok, na který ještě nedošlo — vidět je (správce ví, co ho čeká), ale ustoupí do pozadí
  dimmed?: boolean;
  children: ReactNode;
};

function Step({ number, title, done = false, dimmed = false, children }: StepProps) {
  return (
    <section className={cn("space-y-3 transition-opacity", dimmed && "opacity-45")} aria-label={`Krok ${number}: ${title}`}>
      <h2 className="flex items-center gap-2.5 text-[14px] font-bold text-hk-navy">
        <span
          className={cn(
            "flex size-6 shrink-0 items-center justify-center rounded-full text-[12px] font-bold",
            done ? "bg-emerald-500 text-white" : "bg-hk-navy text-white",
          )}
          aria-hidden
        >
          {done ? <Check className="size-3.5" strokeWidth={3} /> : number}
        </span>
        {title}
      </h2>
      {children}
    </section>
  );
}
