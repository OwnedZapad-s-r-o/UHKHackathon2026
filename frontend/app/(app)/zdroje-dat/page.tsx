import type { Metadata } from "next";
import { ExternalLink } from "lucide-react";
import { getDataSources, getOpenDataStatus } from "@/lib/api";
import { cn } from "@/lib/utils";
import type { DataSource } from "@/types/api";
import { TopBar } from "../_components/top-bar";
import { SyncPanel } from "./_components/sync-panel";

export const metadata: Metadata = {
  title: "Zdroje dat · SmartAudit",
  description: "Otevřená data, ze kterých SmartAudit vychází — odkazy na portál Data KHK a další zdroje.",
};

const STATUS_LABEL = {
  Running: "probíhá",
  Live: "aktuální",
  Snapshot: "ze zálohy",
  Failed: "nedostupné",
} as const;

const dateFormat = new Intl.DateTimeFormat("cs-CZ", { dateStyle: "short", timeStyle: "short" });

// Pravidla hackathonu vyžadují odkaz na použitá otevřená data (Čl. 7.3) — u každé sady katalog i živé API
export default async function DataSourcesPage() {
  const [data, sync] = await Promise.all([getDataSources(), getOpenDataStatus()]);

  return (
    <>
      <TopBar title="Zdroje dat" />

      <main className="min-h-0 flex-1 overflow-y-auto p-6">
        <h1 className="text-[32px] leading-tight font-bold tracking-tight">Zdroje dat</h1>
        <p className="mt-1.5 max-w-2xl text-sm leading-normal text-slate-600">
          {data.khkDatasetCount} datových sad z portálu Data KHK ({data.totalRecords.toLocaleString("cs-CZ")} záznamů) a {data.externalSourceCount} další
          otevřené zdroje. Každá sada má v aplikaci konkrétní úlohu a odkaz do katalogu poskytovatele.
        </p>

        <SyncPanel initial={sync} />

        <div className="mt-6 space-y-6">
          {data.groups.map((group) => (
            <section key={group.group} aria-labelledby={`group-${group.group}`}>
              <h2 id={`group-${group.group}`} className="mb-2 text-[11px] font-bold tracking-wide text-slate-500 uppercase">
                {group.group}
              </h2>
              <ul className="divide-y divide-slate-100 overflow-hidden rounded-2xl border border-slate-200 bg-white">
                {group.sources.map((source) => (
                  <li key={source.key} className="flex flex-col gap-1 px-4.5 py-3.5 sm:flex-row sm:items-start sm:justify-between sm:gap-6">
                    <div className="min-w-0">
                      <div className="flex flex-wrap items-center gap-x-2 gap-y-1">
                        <a
                          href={source.catalogUrl || source.portalUrl || source.serviceUrl}
                          target="_blank"
                          rel="noreferrer"
                          className="inline-flex items-center gap-1.5 text-[13.5px] font-bold hover:text-hk-red hover:underline"
                        >
                          {source.title}
                          <ExternalLink className="size-3 text-slate-400" aria-hidden />
                        </a>
                        {source.category && (
                          <span className="rounded-md border border-slate-200 bg-slate-50 px-1.5 py-px text-[10.5px] text-slate-500">{source.category}</span>
                        )}
                      </div>
                      <p className="mt-0.5 text-[13px] text-slate-600">{source.purpose}</p>
                      <SourceLinks source={source} />
                    </div>
                    <div className="shrink-0 font-mono text-[11px] text-slate-500 sm:text-right">
                      <p>{source.provider}</p>
                      {source.lastRecordCount !== null && <p>{source.lastRecordCount.toLocaleString("cs-CZ")} záznamů</p>}
                      {source.lastStatus && (
                        <p>
                          {STATUS_LABEL[source.lastStatus]}
                          {source.lastImportedAt && ` · ${dateFormat.format(new Date(source.lastImportedAt))}`}
                        </p>
                      )}
                      <LiveState source={source} />
                    </div>
                  </li>
                ))}
              </ul>
            </section>
          ))}
        </div>
      </main>
    </>
  );
}

// Proklik do katalogu (Data KHK / ČÚZK / Registr smluv / Open-Meteo), na živý endpoint a na záznam DCAT-AP-CZ
function SourceLinks({ source }: { source: DataSource }) {
  const catalogLabel = source.provider === "Data KHK" ? "Otevřít v katalogu Data KHK" : `Otevřít u poskytovatele (${source.provider})`;

  return (
    <p className="mt-1.5 flex flex-wrap gap-x-3 gap-y-0.5 font-mono text-[11px]">
      <a href={source.catalogUrl || source.portalUrl || source.serviceUrl} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1.5 text-hk-blue hover:text-hk-red hover:underline">
        {catalogLabel}
        <ExternalLink className="size-3 shrink-0" aria-hidden />
      </a>
      {source.liveApiUrl && (
        <a href={source.liveApiUrl} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1.5 text-hk-blue hover:text-hk-red hover:underline">
          Živé API
          <ExternalLink className="size-3 shrink-0" aria-hidden />
        </a>
      )}
      {source.dcatRecordUrl && (
        <a href={source.dcatRecordUrl} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1.5 text-hk-blue hover:text-hk-red hover:underline">
          Záznam DCAT-AP-CZ
          <ExternalLink className="size-3 shrink-0" aria-hidden />
        </a>
      )}
    </p>
  );
}

function LiveState({ source }: { source: DataSource }) {
  if (source.liveAvailable === null || !source.lastSyncedAt) return null;

  return (
    <p className={cn("mt-0.5 font-bold", source.liveAvailable ? "text-emerald-700" : "text-amber-700")}>
      {source.liveAvailable ? "Aktuální (Live API)" : "Lokální cache"}
      {source.liveAvailable && source.liveRecordCount !== null && ` · ${source.liveRecordCount.toLocaleString("cs-CZ")}`}
      {` · ${dateFormat.format(new Date(source.lastSyncedAt))}`}
    </p>
  );
}
