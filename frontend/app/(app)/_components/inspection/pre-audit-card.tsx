import { formatCzk, formatNumber } from "@/lib/facility-meta";
import { cn } from "@/lib/utils";
import type { PreAudit } from "@/types/api";
import { PanelCard } from "./panel-card";

// Vícekriteriální pre-audit z otevřených dat: pět parametrů s body, vysvětlením a prokliky na zdrojové sady
export function PreAuditCard({ preAudit }: { preAudit: PreAudit }) {
  return (
    <PanelCard title="Pre-audit z otevřených dat" meta={`${preAudit.score}/100 bodů`}>
      <p className="text-[12px] leading-snug text-slate-600">
        Hlavní příčina: <b className="text-hk-navy">{preAudit.dominantDriver}</b>. Stejná data dají vždy stejný výsledek — žádná náhoda, každý bod má svůj zdroj.
      </p>

      <ul className="mt-3 space-y-3">
        {preAudit.parameters.map((parameter) => {
          const share = parameter.maxScore > 0 ? parameter.score / parameter.maxScore : 0;

          return (
            <li key={parameter.key} className="border-t border-slate-100 pt-3">
              <div className="flex items-baseline gap-2">
                <span className="flex size-5 shrink-0 items-center justify-center rounded-md bg-hk-navy font-mono text-[10.5px] font-bold text-white">{parameter.key}</span>
                <p className="min-w-0 flex-1 text-[13px] font-bold">{parameter.name}</p>
                <span className="font-mono text-[11px] text-slate-500">{parameter.value}</span>
                <span className="w-11 text-right font-mono text-[12px] font-bold">
                  {parameter.score}/{parameter.maxScore}
                </span>
              </div>
              <div className="mt-1.5 h-1.5 overflow-hidden rounded-full bg-slate-100" aria-hidden>
                <div
                  className={cn("h-full rounded-full", share >= 0.66 ? "bg-red-500" : share >= 0.4 ? "bg-amber-500" : "bg-emerald-500")}
                  style={{ width: `${Math.max(4, share * 100)}%` }}
                />
              </div>
              <p className="mt-1.5 text-xs leading-relaxed text-slate-600">{parameter.explanation}</p>
              <p className="mt-1 flex flex-wrap gap-x-2 gap-y-0.5 text-[10.5px]">
                <span className="text-slate-400">Zdroje:</span>
                {parameter.sources.map((source) => (
                  <a key={source.key} href={source.catalogUrl} target="_blank" rel="noreferrer" className="text-hk-blue hover:text-hk-red hover:underline">
                    {source.title} ↗
                  </a>
                ))}
              </p>
            </li>
          );
        })}
      </ul>

      <dl className="mt-3.5 grid grid-cols-2 gap-x-4 gap-y-3 border-t border-slate-100 pt-3.5">
        <Value label="Měrná tepelná ztráta" value={`${formatNumber(preAudit.specificHeatLoss)} W/m²K`} />
        <Value label="Denostupně lokality" value={formatNumber(preAudit.degreeDays, "K·den")} />
        <Value label="Potřeba tepla (model)" value={formatNumber(preAudit.annualHeatDemandKwh / 1000, "MWh/rok")} />
        <Value
          label={preAudit.heatPriceFromContract ? "Cena tepla (Registr smluv)" : "Cena tepla (krajský medián)"}
          value={preAudit.heatPriceCzkPerMwh === null ? "jen pro kraj" : `${formatNumber(preAudit.heatPriceCzkPerMwh)} Kč/MWh`}
        />
        <Value label="Náklady na teplo (model)" value={preAudit.annualHeatCostCzk === null ? "jen pro kraj" : formatCzk(preAudit.annualHeatCostCzk)} />
        <Value label="Úspora chytrou regulací" value={formatNumber(preAudit.regulationSavingPercent, "%")} />
      </dl>
    </PanelCard>
  );
}

function Value({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-[11px] text-slate-500">{label}</dt>
      <dd className="mt-0.5 font-mono text-sm font-bold">{value}</dd>
    </div>
  );
}
