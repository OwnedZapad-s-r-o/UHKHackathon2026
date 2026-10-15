import { ExternalLink } from "lucide-react";
import type { DatasetLink, FacilityDetail } from "@/types/api";
import { PanelCard } from "./panel-card";

const VISIBLE = 6;

// Sady otevřených dat, ze kterých objekt a jeho pre-audit vznikly — proklik do katalogu a na živé API (Čl. 7.3 pravidel)
export function LinkedDatasetsCard({ facility }: { facility: FacilityDetail }) {
  const datasets = facility.linkedDatasets;
  if (datasets.length === 0 && facility.objectLinks.length === 0) return null;

  const visible = datasets.slice(0, VISIBLE);
  const rest = datasets.slice(VISIBLE);

  return (
    <PanelCard>
      <div className="mb-3 space-y-1">
        <h2 className="text-lg leading-snug font-bold tracking-tight">Propojené otevřené datové sady</h2>
        <p className="font-mono text-[10.5px] text-slate-500">{datasets.length} sad · Čl. 7.3</p>
      </div>
      {facility.objectLinks.length > 0 && (
        <ul className="mb-3 flex flex-wrap gap-1.5">
          {facility.objectLinks.map((link) => (
            <li key={link.url} className="min-w-0 max-w-full">
              <a
                href={link.url}
                target="_blank"
                rel="noreferrer"
                className="inline-flex max-w-full items-center gap-1 rounded-md border border-slate-200 bg-slate-50 px-2 py-1 text-[11px] font-bold text-hk-navy transition-colors hover:border-hk-red hover:text-hk-red"
              >
                <span className="min-w-0 break-words">{link.label}</span>
                <ExternalLink className="size-3 shrink-0 text-slate-400" aria-hidden />
              </a>
            </li>
          ))}
        </ul>
      )}

      <ul className="divide-y divide-slate-100 border-t border-slate-100">
        {visible.map((dataset) => (
          <DatasetRow key={dataset.key} dataset={dataset} />
        ))}
      </ul>

      {rest.length > 0 && (
        <details className="group">
          <summary className="cursor-pointer list-none border-t border-slate-100 pt-2.5 text-[12px] font-bold text-hk-red hover:underline">
            <span className="group-open:hidden">Zobrazit dalších {rest.length} sad</span>
            <span className="hidden group-open:inline">Skrýt</span>
          </summary>
          <ul className="divide-y divide-slate-100">
            {rest.map((dataset) => (
              <DatasetRow key={dataset.key} dataset={dataset} />
            ))}
          </ul>
        </details>
      )}
    </PanelCard>
  );
}

function DatasetRow({ dataset }: { dataset: DatasetLink }) {
  return (
    <li className="space-y-2 py-3">
      <div className="min-w-0">
        <a href={dataset.catalogUrl} target="_blank" rel="noreferrer" className="inline-flex max-w-full items-start gap-1.5 text-[12.5px] leading-snug font-bold hover:text-hk-red hover:underline">
          <span className="min-w-0 break-words">{dataset.title}</span>
          <ExternalLink className="mt-0.5 size-3 shrink-0 text-slate-400" aria-hidden />
        </a>
        <p className="mt-0.5 text-[11px] text-slate-500">
          {dataset.provider} · {dataset.category}
        </p>
      </div>
      <div className="flex flex-wrap items-center gap-x-3 gap-y-1 font-mono text-[10.5px]">
        <a href={dataset.liveApiUrl} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 whitespace-nowrap text-hk-blue hover:text-hk-red hover:underline">
          živé API
          <ExternalLink className="size-3 shrink-0" aria-hidden />
        </a>
        {dataset.dcatRecordUrl && (
          <a href={dataset.dcatRecordUrl} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 whitespace-nowrap text-hk-blue hover:text-hk-red hover:underline">
            DCAT
            <ExternalLink className="size-3 shrink-0" aria-hidden />
          </a>
        )}
      </div>
    </li>
  );
}
