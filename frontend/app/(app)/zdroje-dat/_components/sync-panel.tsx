"use client";

import { useState, useTransition } from "react";
import { DatabaseZap, ExternalLink, RefreshCw } from "lucide-react";
import { cn } from "@/lib/utils";
import type { OpenDataSync } from "@/types/api";
import { syncOpenData } from "../actions";

const time = new Intl.DateTimeFormat("cs-CZ", { dateStyle: "short", timeStyle: "medium" });
const number = new Intl.NumberFormat("cs-CZ");

// Akční panel živého napojení — stav zdrojů, katalog Data KHK a tlačítko pro okamžitou synchronizaci
export function SyncPanel({ initial }: { initial: OpenDataSync }) {
  const [sync, setSync] = useState(initial);
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  const checked = sync.summary.live + sync.summary.cache;
  const allLive = checked > 0 && sync.summary.cache === 0;
  const tone = checked === 0 ? "idle" : allLive ? "live" : sync.summary.live > 0 ? "partial" : "offline";

  function run() {
    setError(null);
    startTransition(async () => {
      const result = await syncOpenData();
      if ("error" in result) setError(result.error);
      else setSync(result.sync);
    });
  }

  return (
    <section aria-labelledby="sync-title" className="mt-5 rounded-2xl border border-slate-200 bg-white p-4.5">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div className="min-w-0">
          <div className="flex items-center gap-2">
            <span
              className={cn(
                "size-2.5 shrink-0 rounded-full",
                tone === "live" && "animate-pulse bg-emerald-500",
                tone === "partial" && "bg-amber-500",
                tone === "offline" && "bg-red-500",
                tone === "idle" && "bg-slate-300",
              )}
              aria-hidden
            />
            <h2 id="sync-title" className="text-[15px] font-bold">
              Živé napojení na API otevřených dat
            </h2>
            <span className="rounded-md border border-slate-200 bg-slate-50 px-1.5 py-0.5 font-mono text-[10.5px] text-slate-500">{sync.syncMode}</span>
          </div>
          <p className="mt-1 text-[13px] text-slate-600" aria-live="polite">
            {checked === 0
              ? "Zdroje se zatím živě nekontrolovaly — aplikace jede z dat stažených při startu (Live API, při výpadku lokální cache seed/opendata)."
              : `${sync.summary.live} z ${checked} zdrojů odpovídá živě${sync.summary.cache > 0 ? `, ${sync.summary.cache} jede z lokální cache` : ""}${
                  sync.summary.changed > 0 ? ` · ${sync.summary.changed} sad se u poskytovatele změnilo` : ""
                }.`}
            {sync.reimportRunning && " Nová data se právě stahují a přepočítává se pre-audit."}
          </p>
          <p className="mt-1 flex flex-wrap items-center gap-x-3 gap-y-0.5 font-mono text-[11px] text-slate-500">
            {sync.syncedAt && (
              <span>
                poslední synchronizace {time.format(new Date(sync.syncedAt))}
                {sync.durationMs !== null && ` · ${number.format(sync.durationMs)} ms`}
              </span>
            )}
            <a href={sync.catalog.url} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 hover:text-hk-red hover:underline">
              <DatabaseZap className="size-3" aria-hidden />
              katalog Data KHK
              {sync.catalog.datasetCount !== null && ` · ${sync.catalog.datasetCount} sad`}
              {sync.catalog.lastModified && ` · změněn ${sync.catalog.lastModified}`}
              {sync.catalog.available === false && " · neodpověděl"}
              <ExternalLink className="size-3" aria-hidden />
            </a>
          </p>
          {error && <p className="mt-1.5 text-[13px] font-bold text-red-800">{error}</p>}
        </div>

        <button
          type="button"
          onClick={run}
          disabled={isPending}
          className="inline-flex h-11 shrink-0 items-center justify-center gap-2 rounded-lg bg-hk-red px-4 text-[13.5px] font-bold text-white transition-colors hover:bg-hk-red-dark disabled:opacity-60"
        >
          <RefreshCw className={cn("size-4", isPending && "animate-spin")} aria-hidden />
          {isPending ? "Kontroluji zdroje (max. 5 s na zdroj)…" : "Synchronizovat živě z API Data KHK & ČÚZK"}
        </button>
      </div>
    </section>
  );
}
