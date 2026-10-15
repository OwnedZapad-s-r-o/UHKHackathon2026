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
    <PanelCard title="Propojené otevřené datové sady" meta={`${datasets.length} sad · Čl. 7.3`}>
      {facility.objectLinks.length > 0 && (
        <ul className="mb-3 flex flex-wrap gap-1.5">
          {facility.objectLinks.map((link) => (
            <li key={link.url}>
              <a
                href={link.url}
                target="_blank"
                rel="noreferrer"
                className="inline-flex items-center gap-1 rounded-md border border-slate-200 bg-slate-50 px-2 py-1 text-[11px] font-bold text-hk-navy transition-colors hover:border-hk-red hover:text-hk-red"
              >
                {link.label}
                <ExternalLink className="size-3 text-slate-400" aria-hidden />
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
    <li className="flex items-start justify-between gap-3 py-2.5">
      <div className="min-w-0">
        <a href={dataset.catalogUrl} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 text-[12.5px] font-bold hover:text-hk-red hover:underline">
          <span className="truncate">{dataset.title}</span>
          <ExternalLink className="size-3 shrink-0 text-slate-400" aria-hidden />
        </a>
        <p className="text-[11px] text-slate-500">
          {dataset.provider} · {dataset.category}
        </p>
      </div>
      <div className="flex shrink-0 gap-2 font-mono text-[10.5px]">
        <a href={dataset.liveApiUrl} target="_blank" rel="noreferrer" className="text-hk-blue hover:text-hk-red hover:underline">
          živé API ↗
        </a>
        {dataset.dcatRecordUrl && (
          <a href={dataset.dcatRecordUrl} target="_blank" rel="noreferrer" className="text-hk-blue hover:text-hk-red hover:underline">
            DCAT ↗
          </a>
        )}
      </div>
    </li>
  );
}
