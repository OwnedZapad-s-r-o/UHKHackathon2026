"use client";

import { useState, useTransition } from "react";
import { KeyRound, Landmark, Sparkles } from "lucide-react";
import type { AiAnalysis } from "@/types/api";
import { generateAiAudit } from "./ai-actions";

type AiAuditCardProps = {
  facilityId: string;
  configured: boolean;
  initialAnalysis: AiAnalysis | null;
};

const dateTime = new Intl.DateTimeFormat("cs-CZ", { dateStyle: "short", timeStyle: "short" });

// Karta „AI Auditor“ z prototypu — generuje návrh opatření a dopisu nad daty objektu
export function AiAuditCard({ facilityId, configured, initialAnalysis }: AiAuditCardProps) {
  const [analysis, setAnalysis] = useState(initialAnalysis);
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  function generate() {
    setError(null);
    startTransition(async () => {
      const result = await generateAiAudit(facilityId);
      if ("error" in result) setError(result.error);
      else setAnalysis(result.analysis);
    });
  }

  return (
    <section className="mt-3.5 rounded-2xl border border-slate-200 bg-white p-4.5" aria-labelledby={`ai-${facilityId}`}>
      <div className="flex items-center gap-2">
        <Sparkles className="size-4.5 text-hk-red" aria-hidden />
        <h2 id={`ai-${facilityId}`} className="text-lg font-bold tracking-tight">
          AI analýza
        </h2>
        {analysis && (
          <span className="ml-auto font-mono text-[10px] text-slate-400">
            {analysis.model} · {dateTime.format(new Date(analysis.createdAt))}
          </span>
        )}
      </div>

      {!configured ? (
        <p className="mt-3 flex gap-2 rounded-xl border border-dashed border-slate-300 bg-slate-50 p-3 text-[13px] leading-relaxed text-slate-600">
          <KeyRound className="mt-0.5 size-4 shrink-0 text-slate-400" aria-hidden />
          AI analýza zatím není nastavená. Po doplnění klíče OpenAI (OPENAI_API_KEY) nebo Azure OpenAI (AZURE_OPENAI_*) v souboru .env se tu objeví tlačítko
          pro vygenerování auditu.
        </p>
      ) : (
        <button
          type="button"
          onClick={generate}
          disabled={isPending}
          className="mt-3 inline-flex h-12 w-full items-center justify-center gap-2 rounded-lg bg-hk-red text-sm font-bold text-white transition-colors hover:bg-hk-red-dark disabled:opacity-60"
        >
          <Sparkles className="size-4" aria-hidden />
          {isPending ? "Model analyzuje objekt…" : analysis ? "Vygenerovat znovu" : "Generovat AI audit"}
        </button>
      )}

      {error && (
        <p role="alert" className="mt-3 text-[13px] text-red-800">
          {error}
        </p>
      )}

      {analysis?.fromCache && (
        <p role="status" className="mt-3 rounded-lg bg-slate-50 px-3 py-2 text-[12.5px] text-slate-600">
          Stav budovy se od poslední analýzy nezměnil — ukazujeme ji znovu, model se nevolal.
        </p>
      )}

      {analysis && <AnalysisBody analysis={analysis} />}
    </section>
  );
}

function AnalysisBody({ analysis }: { analysis: AiAnalysis }) {
  return (
    <div className="mt-4 space-y-3 text-[13px] leading-relaxed">
      <p>{analysis.summary}</p>
      <p>
        <span className="font-bold">Pravděpodobná příčina: </span>
        {analysis.rootCause}
      </p>

      <ol className="space-y-2">
        {analysis.recommendations.map((recommendation) => (
          <li key={recommendation.title} className="rounded-xl border border-slate-200 bg-slate-50 p-3">
            <p className="font-bold">{recommendation.title}</p>
            <p className="mt-0.5 text-xs text-slate-600">{recommendation.detail}</p>
            <p className="mt-1.5 font-mono text-[11px] text-slate-500">
              {recommendation.cost} · úspora {recommendation.saving} · návratnost {recommendation.payback}
            </p>
            {!recommendation.heritageSafe && (
              <p className="mt-1.5 inline-flex items-center gap-1 rounded-full border border-amber-200 bg-amber-50 px-2 py-0.5 text-[11px] font-bold text-amber-800">
                <Landmark className="size-3" aria-hidden />
                vyžaduje stanovisko památkové péče
              </p>
            )}
          </li>
        ))}
      </ol>

      {analysis.letterDraft && (
        <details className="rounded-xl border border-slate-200 p-3">
          <summary className="cursor-pointer font-bold">Návrh dopisu správci / zřizovateli</summary>
          <p className="mt-2 text-xs whitespace-pre-line text-slate-700">{analysis.letterDraft}</p>
        </details>
      )}

      <p className="text-[11px] text-slate-400">Návrh modelu z dat aplikace — před odesláním ověřte s energetickým specialistou.</p>
    </div>
  );
}
