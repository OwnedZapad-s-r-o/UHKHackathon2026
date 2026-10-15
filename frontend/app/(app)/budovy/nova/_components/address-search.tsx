"use client";

import { useEffect, useId, useRef, useState } from "react";
import { Loader2, LocateFixed, MapPin } from "lucide-react";
import { cn } from "@/lib/utils";
import type { AddressSuggestion } from "@/types/api";

// Našeptávač se ptá až po krátké pauze v psaní — registr RÚIAN nemá dostávat dotaz na každé písmeno
const DEBOUNCE_MS = 220;

type AddressSearchProps = {
  value: string;
  onValueChange: (value: string) => void;
  onSelect: (suggestion: AddressSuggestion) => void;
  onLocateMe: (latitude: number, longitude: number) => void;
};

// Adresa z oficiálního registru RÚIAN: píše se „ulice číslo obec“ v libovolném pořadí, bez diakritiky to taky najde.
// Šipky + Enter vyberou návrh, Esc zavře. Kdo adresu nezná, klikne do mapy nebo použije svou polohu.
export function AddressSearch({ value, onValueChange, onSelect, onLocateMe }: AddressSearchProps) {
  const listId = useId();
  const [open, setOpen] = useState(false);
  const [active, setActive] = useState(-1);
  const [suggestions, setSuggestions] = useState<AddressSuggestion[]>([]);
  // Dotaz, ke kterému návrhy patří — během psaní jsou vidět ještě návrhy k předchozímu textu
  const [resultsFor, setResultsFor] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [locating, setLocating] = useState(false);
  // Hledá se jen to, co uživatel napsal — ne adresa, kterou do pole doplnilo kliknutí do mapy
  const [typed, setTyped] = useState(false);
  // Enter zmáčknutý dřív, než dorazily návrhy — po jejich příchodu se vybere první
  const enterPending = useRef(false);
  // Výběr z odpovědi hledání (Enter předběhl návrhy) — nejnovější onSelect bez restartu hledání
  const onSelectRef = useRef(onSelect);
  useEffect(() => {
    onSelectRef.current = onSelect;
  });

  const query = value.trim();

  useEffect(() => {
    if (!typed || query.length < 3) return;

    const controller = new AbortController();
    const timer = setTimeout(async () => {
      setLoading(true);
      try {
        const response = await fetch(`/budovy/nova/adresy?q=${encodeURIComponent(query)}`, { signal: controller.signal });
        const body = (await response.json()) as AddressSuggestion[] | { message: string };
        if (Array.isArray(body)) {
          setSuggestions(body);
          setResultsFor(query);
          setError(null);
          if (enterPending.current && body.length > 0) {
            enterPending.current = false;
            setTyped(false);
            setOpen(false);
            onSelectRef.current(body[0]);
            return;
          }
        } else {
          setSuggestions([]);
          setResultsFor(query);
          setError(body.message);
        }
        setActive(-1);
      } catch (e) {
        if ((e as Error).name !== "AbortError") setError("Našeptávač adres teď nefunguje. Klikněte na budovu v mapě.");
      } finally {
        if (!controller.signal.aborted) setLoading(false);
      }
    }, DEBOUNCE_MS);

    return () => {
      clearTimeout(timer);
      controller.abort();
    };
  }, [query, typed]);

  // Během načítání zůstanou vidět předchozí návrhy (žádné blikání); „nic nenalezeno“ až po odpovědi
  const expanded = open && typed && query.length >= 3 && (suggestions.length > 0 || (!loading && (error !== null || query.length >= 5)));

  function choose(suggestion: AddressSuggestion) {
    setTyped(false);
    setOpen(false);
    setActive(-1);
    onSelect(suggestion);
  }

  function handleKeyDown(event: React.KeyboardEvent<HTMLInputElement>) {
    if (event.key === "ArrowDown" || event.key === "ArrowUp") {
      event.preventDefault();
      setOpen(true);
      if (suggestions.length === 0) return;
      const step = event.key === "ArrowDown" ? 1 : -1;
      setActive((current) => (current + step + suggestions.length) % suggestions.length);
    } else if (event.key === "Enter") {
      // Enter nikdy neodešle celý formulář — buď vybere návrh, nebo první návrh
      event.preventDefault();
      const pick = suggestions[active] ?? suggestions[0];
      const typedNow = event.currentTarget.value.trim();
      if (typedNow.length >= 3 && resultsFor !== typedNow) enterPending.current = true;
      else if (expanded && pick) choose(pick);
    } else if (event.key === "Escape") {
      setOpen(false);
      setActive(-1);
    }
  }

  function locateMe() {
    if (!navigator.geolocation) {
      setError("Prohlížeč neumí zjistit polohu. Klikněte na budovu v mapě.");
      return;
    }

    setLocating(true);
    navigator.geolocation.getCurrentPosition(
      (position) => {
        setLocating(false);
        setError(null);
        onLocateMe(position.coords.latitude, position.coords.longitude);
      },
      () => {
        setLocating(false);
        setError("Polohu se nepodařilo zjistit (nejspíš není povolená). Klikněte na budovu v mapě.");
      },
      { enableHighAccuracy: true, timeout: 10_000 },
    );
  }

  return (
    <div className="space-y-2">
      <div className="relative">
        <div className="flex h-12 items-center gap-2 rounded-xl border border-slate-300 bg-white px-3 focus-within:border-hk-blue focus-within:ring-3 focus-within:ring-hk-blue/15">
          {loading ? <Loader2 className="size-4.5 shrink-0 animate-spin text-slate-400" aria-hidden /> : <MapPin className="size-4.5 shrink-0 text-hk-red" aria-hidden />}
          <input
            id="building-address"
            value={value}
            onChange={(event) => {
              setTyped(true);
              enterPending.current = false;
              onValueChange(event.target.value);
              setOpen(true);
            }}
            onFocus={() => setOpen(true)}
            onBlur={() => setOpen(false)}
            onKeyDown={handleKeyDown}
            placeholder="Ulice a číslo, obec — např. Komenského 63 Trutnov"
            role="combobox"
            aria-expanded={expanded}
            aria-controls={listId}
            aria-autocomplete="list"
            aria-activedescendant={expanded && active >= 0 ? `${listId}-${active}` : undefined}
            autoComplete="off"
            autoFocus
            className="h-full min-w-0 flex-1 bg-transparent text-[14px] text-hk-navy outline-none placeholder:text-slate-400"
          />
        </div>

        {expanded && (
          <ul
            id={listId}
            role="listbox"
            aria-label="Návrhy adres"
            className="absolute top-full right-0 left-0 z-30 mt-1 max-h-80 overflow-y-auto rounded-xl border border-slate-200 bg-white py-1 shadow-[0_12px_32px_rgba(11,25,44,0.14)]"
          >
            {suggestions.length === 0 ? (
              <li className="px-3.5 py-2.5 text-[13px] text-slate-500">
                {error ?? "Adresu v kraji jsme nenašli. Zkuste doplnit obec nebo číslo popisné — nebo klikněte na budovu v mapě."}
              </li>
            ) : (
              suggestions.map((suggestion, index) => {
                const [line, ...rest] = suggestion.label.split(", ");

                return (
                  <li
                    key={suggestion.key}
                    id={`${listId}-${index}`}
                    role="option"
                    aria-selected={index === active}
                    // Klik nesmí vzít fokus z pole dřív, než se výběr provede
                    onMouseDown={(event) => event.preventDefault()}
                    onClick={() => choose(suggestion)}
                    onMouseEnter={() => setActive(index)}
                    className={cn("flex cursor-pointer items-start gap-2.5 px-3.5 py-2", index === active && "bg-slate-100")}
                  >
                    <MapPin className="mt-0.5 size-3.5 shrink-0 text-slate-400" aria-hidden />
                    <span className="min-w-0">
                      <span className="block truncate text-[13px] font-bold text-hk-navy">{line}</span>
                      <span className="block truncate text-[11.5px] text-slate-500">{rest.join(", ")}</span>
                    </span>
                  </li>
                );
              })
            )}
          </ul>
        )}
      </div>

      <div className="flex flex-wrap items-center gap-x-3 gap-y-1.5 text-xs text-slate-500">
        <button
          type="button"
          onClick={locateMe}
          disabled={locating}
          className="inline-flex h-8 items-center gap-1.5 rounded-lg border border-slate-200 bg-white px-2.5 font-bold text-hk-navy transition-colors hover:border-slate-300 hover:bg-slate-50 disabled:opacity-60"
        >
          {locating ? <Loader2 className="size-3.5 animate-spin" aria-hidden /> : <LocateFixed className="size-3.5" aria-hidden />}
          Jsem u budovy — použít moji polohu
        </button>
        <span>nebo klikněte na budovu přímo v mapě</span>
      </div>

      {error && !expanded && (
        <p role="alert" className="text-xs text-red-700">
          {error}
        </p>
      )}
    </div>
  );
}
