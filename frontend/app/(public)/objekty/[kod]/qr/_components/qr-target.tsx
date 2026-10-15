"use client";

import { createContext, useContext, useState, useSyncExternalStore, type ReactNode } from "react";
import { Globe, RotateCcw } from "lucide-react";
import { BrandQr } from "@/components/brand/brand-qr";

// Cílová doména QR kódů se dá před prezentací přepnout za běhu (např. na veřejnou doménu z Raspberry Pi)
// a pamatuje si ji tento prohlížeč. Pořadí: přepínač → NEXT_PUBLIC_PUBLIC_URL → adresa ze serveru / window.location.origin.
const STORAGE_KEY = "smartaudit.qrBaseUrl";
const listeners = new Set<() => void>();

function subscribe(callback: () => void) {
  listeners.add(callback);
  window.addEventListener("storage", callback);
  return () => {
    listeners.delete(callback);
    window.removeEventListener("storage", callback);
  };
}

function readOverride() {
  try {
    return window.localStorage.getItem(STORAGE_KEY) ?? "";
  } catch {
    return "";
  }
}

function writeOverride(value: string) {
  try {
    if (value) window.localStorage.setItem(STORAGE_KEY, value);
    else window.localStorage.removeItem(STORAGE_KEY);
  } catch {
    // Soukromé okno bez úložiště — přepnutí platí jen do obnovení stránky
  }
  listeners.forEach((listener) => listener());
}

// http(s) adresa bez cesty a koncového lomítka, jinak null
function normalize(input: string) {
  const trimmed = input.trim();
  if (!trimmed) return null;

  try {
    const url = new URL(/^https?:\/\//i.test(trimmed) ? trimmed : `https://${trimmed}`);
    return url.protocol === "http:" || url.protocol === "https:" ? url.origin : null;
  } catch {
    return null;
  }
}

type QrTargetValue = { base: string; defaultBase: string; overridden: boolean; path: string };

const QrTargetContext = createContext<QrTargetValue | null>(null);

function useQrTarget() {
  const value = useContext(QrTargetContext);
  if (!value) throw new Error("QR komponenty musí být uvnitř QrTargetProvider.");
  return value;
}

// configuredBase = PUBLIC_URL / NEXT_PUBLIC_PUBLIC_URL ze serveru, requestBase = adresa požadavku (pro první vykreslení)
export function QrTargetProvider({ configuredBase, requestBase, path, children }: { configuredBase: string | null; requestBase: string; path: string; children: ReactNode }) {
  const override = useSyncExternalStore(subscribe, readOverride, () => "");
  const origin = useSyncExternalStore(subscribe, () => window.location.origin, () => requestBase);

  const defaultBase = (process.env.NEXT_PUBLIC_PUBLIC_URL || configuredBase || origin).replace(/\/+$/, "");
  const base = override || defaultBase;

  return <QrTargetContext.Provider value={{ base, defaultBase, overridden: override !== "", path }}>{children}</QrTargetContext.Provider>;
}

export function QrImage({ label, className }: { label: string; className?: string }) {
  const { base, path } = useQrTarget();

  return <BrandQr value={`${base}${path}`} label={label} className={className} />;
}

export function QrShortUrl() {
  const { base, path } = useQrTarget();

  return <span className="truncate font-mono">{`${base}${path}`.replace(/^https?:\/\//, "")}</span>;
}

// Nenápadný přepínač nad cedulkou (při tisku skrytý)
export function QrDomainSwitch() {
  const { base, defaultBase, overridden } = useQrTarget();
  const [draft, setDraft] = useState("");
  const [error, setError] = useState<string | null>(null);

  function apply(event: React.FormEvent) {
    event.preventDefault();
    const normalized = normalize(draft);
    if (!normalized) {
      setError("Zadejte adresu jako https://smartaudit.nasedomena.cz");
      return;
    }
    setError(null);
    setDraft("");
    writeOverride(normalized);
  }

  return (
    <form onSubmit={apply} className="mx-auto mb-4 flex max-w-[148mm] flex-wrap items-center gap-2 text-[12px] text-slate-500 print:hidden">
      <Globe className="size-3.5 shrink-0" aria-hidden />
      <span>
        QR vede na <b className="font-mono text-hk-navy">{base}</b>
        {overridden && " (přepnuto)"}
      </span>
      <label className="sr-only" htmlFor="qr-domain">
        Cílová doména QR kódu
      </label>
      <input
        id="qr-domain"
        value={draft}
        onChange={(event) => setDraft(event.target.value)}
        placeholder="https://smartaudit.nasedomena.cz"
        inputMode="url"
        className="h-8 min-w-0 flex-1 rounded-md border border-slate-300 bg-white px-2 font-mono text-[12px] text-hk-navy outline-none focus:border-hk-blue"
      />
      <button type="submit" className="h-8 rounded-md border border-slate-300 bg-white px-2.5 font-bold text-hk-navy transition-colors hover:bg-slate-100">
        Použít
      </button>
      {overridden && (
        <button
          type="button"
          onClick={() => writeOverride("")}
          title={`Zpět na ${defaultBase}`}
          className="inline-flex h-8 items-center gap-1 rounded-md px-2 text-slate-500 transition-colors hover:bg-slate-200 hover:text-hk-navy"
        >
          <RotateCcw className="size-3.5" aria-hidden />
          Výchozí
        </button>
      )}
      {error && <p className="w-full text-red-800">{error}</p>}
    </form>
  );
}
