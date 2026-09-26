import Link from "next/link";
import { ArrowRight, Loader2, TrendingDown, TrendingUp } from "lucide-react";
import { formatCzk } from "@/lib/facility-meta";
import { cn } from "@/lib/utils";
import type { LossCorrection } from "@/types/api";

// Budovy, kde teplotu změřilo skutečné čidlo: odhad ztráty před čidlem (ze simulace) → teď (z měření)
export function LossCorrections({ corrections }: { corrections: LossCorrection[] }) {
  return (
    <section aria-labelledby="loss-corrections" className="rounded-2xl border border-slate-200 bg-white">
      <div className="border-b border-slate-100 px-5 py-3.5">
        <h2 id="loss-corrections" className="text-[14px] font-bold">
          Jak čidla opravila odhad ztráty
        </h2>
        <p className="mt-0.5 text-xs text-slate-500">
          Bez čidla počítá audit ztrátu ze simulace. Místnost, kterou změří čidlo teploty, se počítá jen ze skutečných hodnot.
        </p>
      </div>

      {corrections.length === 0 ? (
        <p className="px-5 py-5 text-[13px] text-slate-600">
          Zatím žádná budova. Přiřaďte uzel s teploměrem do místnosti — ztrátu mění jen teplota, ne CO₂ ani hluk.
        </p>
      ) : (
        <ul className="divide-y divide-slate-100">
          {corrections.map((correction) => (
            <CorrectionRow key={correction.facilityId} correction={correction} />
          ))}
        </ul>
      )}
    </section>
  );
}

function CorrectionRow({ correction }: { correction: LossCorrection }) {
  const before = correction.lossBeforeCzk ?? 0;
  const delta = correction.lossNowCzk - before;
  const percent = before > 0 ? Math.round((delta / before) * 100) : null;
  // Audit se přepočítá do pár sekund po přiřazení — do té doby ukazuje ještě starou hodnotu
  const recalculating = new Date(correction.evaluatedAt) < new Date(correction.since);
  const coverage = correction.zoneCount > 0 ? Math.min(100, (correction.measuredZones / correction.zoneCount) * 100) : 0;
  const Trend = delta <= 0 ? TrendingDown : TrendingUp;

  return (
    <li className="px-5 py-4">
      <div className="flex flex-wrap items-baseline justify-between gap-2">
        <p className="min-w-0 text-[13.5px] font-bold">
          {correction.facilityName}
          <span className="ml-2 font-mono text-[11px] font-normal text-slate-500">{correction.facilityCode}</span>
        </p>
        <Link href={`/mapa?objekt=${correction.facilityId}`} className="inline-flex items-center gap-0.5 text-[12.5px] font-bold text-hk-blue hover:underline">
          Budova v mapě
          <ArrowRight className="size-3" aria-hidden />
        </Link>
      </div>

      <div className="mt-3 grid gap-3 sm:grid-cols-[1fr_auto_1fr_auto] sm:items-center">
        <div>
          <p className="text-[11px] text-slate-500">Před čidlem (odhad ze simulace)</p>
          <p className="font-mono text-[15px] font-bold text-slate-500 line-through decoration-slate-400/60">{formatCzk(before)}</p>
        </div>
        <ArrowRight className="hidden size-4 text-slate-400 sm:block" aria-hidden />
        <div>
          <p className="text-[11px] text-slate-500">Teď (z měření)</p>
          <p className={cn("flex items-center gap-1.5 font-mono text-[15px] font-bold", correction.lossNowCzk > 0 ? "text-red-800" : "text-emerald-700")}>
            {recalculating && <Loader2 className="size-3.5 animate-spin text-slate-400" aria-label="přepočítává se" />}
            {formatCzk(correction.lossNowCzk)}
            <span className="text-[11px] font-normal text-slate-500">/ rok</span>
          </p>
        </div>
        <p
          className={cn(
            "inline-flex w-fit items-center gap-1 rounded-full px-2.5 py-1 text-[12px] font-bold",
            delta < 0 ? "bg-emerald-50 text-emerald-800" : delta > 0 ? "bg-red-50 text-red-800" : "bg-slate-100 text-slate-600",
          )}
        >
          <Trend className="size-3.5" aria-hidden />
          {delta === 0 ? "beze změny" : `${delta > 0 ? "+" : "−"}${formatCzk(Math.abs(delta))}${percent !== null ? ` (${percent > 0 ? "+" : ""}${percent} %)` : ""}`}
        </p>
      </div>

      <div className="mt-3">
        <div className="flex justify-between text-[11.5px] text-slate-500">
          <span>
            Čidla teploty měří {correction.measuredZones} z {correction.zoneCount} místností
          </span>
          <span>zbytek budovy zatím simulace</span>
        </div>
        <div className="mt-1 h-1.5 rounded-full bg-slate-100">
          <div className="h-full rounded-full bg-hk-blue" style={{ width: `${coverage}%` }} />
        </div>
        <p className="mt-1.5 text-[11.5px] text-slate-500">
          {correction.virtualHistory
            ? "Virtuální čidlo dodalo naměřený týden, proto je oprava vidět hned. Skutečné čidlo ztrátu upřesňuje postupně 7 dní."
            : `Skutečné čidlo měří od ${new Date(correction.since).toLocaleString("cs-CZ", { timeZone: "Europe/Prague", dateStyle: "short", timeStyle: "short" })} — ztráta se upřesňuje postupně 7 dní.`}
        </p>
      </div>
    </li>
  );
}
