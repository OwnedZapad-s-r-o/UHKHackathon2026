import type { Metadata } from "next";
import { ExternalLink } from "lucide-react";
import { getDataSources } from "@/lib/api";
import { TopBar } from "../_components/top-bar";

export const metadata: Metadata = {
  title: "Zdroje dat · SmartAudit KHK",
  description: "Otevřená data, ze kterých SmartAudit vychází — odkazy na portál Data KHK a další zdroje.",
};

const STATUS_LABEL = {
  Running: "probíhá",
  Live: "aktuální",
  Snapshot: "ze zálohy",
  Failed: "nedostupné",
} as const;

const dateFormat = new Intl.DateTimeFormat("cs-CZ", { dateStyle: "short", timeStyle: "short" });

// Pravidla hackathonu vyžadují odkaz na použitá otevřená data
export default async function DataSourcesPage() {
  const data = await getDataSources();

  return (
    <>
      <TopBar title="Zdroje dat" />

      <main className="min-h-0 flex-1 overflow-y-auto p-6">
        <h1 className="text-[32px] leading-tight font-bold tracking-tight">Zdroje dat</h1>
        <p className="mt-1.5 max-w-2xl text-sm leading-normal text-slate-600">
          {data.khkDatasetCount} datových sad z portálu Data KHK ({data.totalRecords.toLocaleString("cs-CZ")} záznamů) a {data.externalSourceCount} další
          otevřené zdroje. Každá sada má v aplikaci konkrétní úlohu.
        </p>

        <div className="mt-6 space-y-6">
          {data.groups.map((group) => (
            <section key={group.group} aria-labelledby={`group-${group.group}`}>
              <h2 id={`group-${group.group}`} className="mb-2 text-[11px] font-bold tracking-wide text-slate-500 uppercase">
                {group.group}
              </h2>
              <ul className="divide-y divide-slate-100 overflow-hidden rounded-2xl border border-slate-200 bg-white">
                {group.sources.map((source) => (
                  <li key={source.key} className="flex flex-col gap-1 px-4.5 py-3.5 sm:flex-row sm:items-start sm:justify-between sm:gap-6">
                    <div>
                      <a
                        href={source.portalUrl ?? source.serviceUrl}
                        target="_blank"
                        rel="noreferrer"
                        className="inline-flex items-center gap-1.5 text-[13.5px] font-bold hover:text-hk-red hover:underline"
                      >
                        {source.title}
                        <ExternalLink className="size-3 text-slate-400" aria-hidden />
                      </a>
                      <p className="mt-0.5 text-[13px] text-slate-600">{source.purpose}</p>
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
