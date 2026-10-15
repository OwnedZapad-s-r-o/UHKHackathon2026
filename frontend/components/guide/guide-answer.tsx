"use client";

import Link from "next/link";
import { ArrowRight, BookOpen, ChevronRight, Database, ExternalLink, ShieldCheck } from "lucide-react";
import { LiveBadge } from "@/components/live-badge";
import { AIR_META, airLevel } from "@/lib/air-quality";
import { STATUS_META, formatCzk } from "@/lib/facility-meta";
import { hoverGuideItem } from "@/lib/guide-events";
import { cn } from "@/lib/utils";
import type { GuideAnswer as GuideAnswerData, GuideDistrictCard, GuideFacilityCard, GuideSource } from "@/types/api";
import { GuideChart } from "./guide-chart";

type GuideAnswerProps = {
  answer: GuideAnswerData;
  // Poslední odpověď ukazuje i návrhy dalších otázek
  latest: boolean;
  onAsk: (question: string) => void;
  onOpenFacility: (card: GuideFacilityCard) => void;
  onOpenDistrict: (code: string) => void;
};

// Odpověď průvodce: krátký text, pod ním to, co mapa právě ukazuje (očíslované objekty, okresy),
// odkud odpověď pochází a kam pokračovat. Žádné bubliny chatu — karta jako v inspekčním panelu.
export function GuideAnswer({ answer, latest, onAsk, onOpenFacility, onOpenDistrict }: GuideAnswerProps) {
  const blocked = answer.mode === "Blocked";

  return (
    <div className="space-y-3">
      <div className={cn("rounded-xl border px-3.5 py-3 text-[13.5px] leading-relaxed", blocked ? "border-amber-200 bg-amber-50/70 text-amber-950" : "border-slate-200 bg-white text-slate-700")}>
        {blocked && (
          <p className="mb-1 flex items-center gap-1.5 text-[11.5px] font-bold text-amber-800">
            <ShieldCheck className="size-3.5" aria-hidden />
            Mimo účel průvodce
          </p>
        )}
        <RichText text={answer.answer} />
      </div>

      {(answer.charts ?? []).map((chart) => (
        <GuideChart
          key={chart.title}
          chart={chart}
          onSelectFacility={(id) => {
            const card = answer.facilities.find((f) => f.id === id);
            if (card) onOpenFacility(card);
          }}
        />
      ))}

      {answer.facilities.length > 0 && (
        <ol className="space-y-1.5" aria-label="Objekty v mapě">
          {answer.facilities.map((card) => (
            <li key={card.id}>
              <FacilityCard card={card} onOpen={() => onOpenFacility(card)} />
            </li>
          ))}
        </ol>
      )}

      {answer.districts.length > 0 && <DistrictList districts={answer.districts} onOpen={onOpenDistrict} />}

      {answer.links.length > 0 && (
        <div className="flex flex-wrap gap-1.5">
          {answer.links.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className="inline-flex h-8 items-center gap-1.5 rounded-lg bg-hk-navy px-3 text-[12px] font-bold text-white transition-colors hover:bg-hk-blue"
            >
              {link.label}
              <ArrowRight className="size-3.5" aria-hidden />
            </Link>
          ))}
        </div>
      )}

      {answer.sources.length > 0 && <Sources sources={answer.sources} />}

      {latest && answer.suggestions.length > 0 && (
        <div className="flex flex-wrap gap-1.5 pt-0.5" aria-label="Navazující otázky">
          {answer.suggestions.map((suggestion) => (
            <button
              key={suggestion}
              type="button"
              onClick={() => onAsk(suggestion)}
              className="rounded-full border border-slate-200 bg-white px-3 py-1.5 text-left text-[12px] text-slate-700 transition-colors hover:border-hk-red/40 hover:text-hk-red"
            >
              {suggestion}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

// Jediné formátování, které průvodce smí: **tučně**. Text se nikdy nevkládá jako HTML.
function RichText({ text }: { text: string }) {
  const parts = text.split(/\*\*(.+?)\*\*/g);

  return (
    <p className="whitespace-pre-line">
      {parts.map((part, index) => (index % 2 === 1 ? <strong key={index} className="font-bold text-hk-navy">{part}</strong> : part))}
    </p>
  );
}

function FacilityCard({ card, onOpen }: { card: GuideFacilityCard; onOpen: () => void }) {
  const level = airLevel(card.co2);

  return (
    <button
      type="button"
      onClick={onOpen}
      onMouseEnter={() => hoverGuideItem(card.id)}
      onMouseLeave={() => hoverGuideItem(null)}
      onFocus={() => hoverGuideItem(card.id)}
      onBlur={() => hoverGuideItem(null)}
      className="group flex w-full items-start gap-2.5 rounded-xl border border-slate-200 bg-white p-2.5 text-left transition-colors hover:border-hk-red/40"
    >
      <span className="relative mt-0.5 flex size-6 shrink-0 items-center justify-center rounded-full bg-hk-navy text-[11.5px] font-bold text-white tabular-nums">
        {card.rank}
        <span className={cn("absolute -right-0.5 -bottom-0.5 size-2 rounded-full border border-white", STATUS_META[card.status].dot)} aria-hidden />
      </span>
      <span className="min-w-0 flex-1">
        <span className="line-clamp-2 text-[13px] leading-snug font-bold text-hk-navy group-hover:text-hk-red">{card.name}</span>
        <span className="mt-0.5 block truncate text-[11.5px] text-slate-500">
          {card.municipalityName} · {card.headline}
        </span>
        <span className="mt-1.5 flex flex-wrap items-center gap-1.5">
          {level && (
            <span className={cn("inline-flex items-center gap-1 rounded-full border px-2 py-px text-[11px] font-bold", AIR_META[level].pill)}>
              CO₂ {Math.round(card.co2 ?? 0).toLocaleString("cs-CZ")} ppm
            </span>
          )}
          {card.co2HoursOverLimit !== null && card.co2HoursOverLimit > 0 && (
            <span className="text-[11px] text-slate-500">{Math.round(card.co2HoursOverLimit)} h nad limitem / týden</span>
          )}
          {card.annualLossCzk !== null && card.annualLossCzk > 0 && <span className="text-[11px] font-bold text-hk-red">{formatCzk(card.annualLossCzk)} / rok</span>}
          {card.hasLiveSensors && <LiveBadge />}
        </span>
      </span>
      <ChevronRight className="mt-1 size-4 shrink-0 text-slate-300 group-hover:text-hk-red" aria-hidden />
    </button>
  );
}

// Srovnání okresů: pruh = podíl budov se zjištěním, klik vybere okres v mapě
function DistrictList({ districts, onOpen }: { districts: GuideDistrictCard[]; onOpen: (code: string) => void }) {
  return (
    <ul className="divide-y divide-slate-100 rounded-xl border border-slate-200 bg-white" aria-label="Srovnání okresů">
      {districts.map((district) => {
        const share = district.facilities > 0 ? district.withIssues / district.facilities : 0;

        return (
          <li key={district.code}>
            <button type="button" onClick={() => onOpen(district.code)} className="group block w-full px-3 py-2 text-left hover:bg-slate-50">
              <span className="flex items-baseline justify-between gap-2 text-[12.5px]">
                <span className="font-bold text-hk-navy group-hover:text-hk-red">{district.name}</span>
                <span className="text-slate-500 tabular-nums">
                  {district.withIssues} z {district.facilities} se zjištěním
                </span>
              </span>
              <span className="mt-1.5 block h-1.5 overflow-hidden rounded-full bg-slate-100">
                <span className="block h-full rounded-full bg-gradient-to-r from-amber-400 to-hk-red" style={{ width: `${Math.max(4, share * 100)}%` }} />
              </span>
              {district.annualLossCzk !== null && district.annualLossCzk > 0 && (
                <span className="mt-1 block text-[11px] text-slate-500">ztráta odhadem {formatCzk(district.annualLossCzk)} / rok</span>
              )}
            </button>
          </li>
        );
      })}
    </ul>
  );
}

// „Odkud to vím“ — RAG nejen dělá, ale i ukazuje: kapitoly znalostí, datové sady, živá data
function Sources({ sources }: { sources: GuideSource[] }) {
  return (
    <details className="group rounded-xl border border-dashed border-slate-200 px-3 py-2 text-[12px] text-slate-600 open:bg-slate-50/60">
      <summary className="flex cursor-pointer list-none items-center gap-1.5 font-bold text-slate-500 select-none hover:text-hk-navy">
        <BookOpen className="size-3.5" aria-hidden />
        Odkud to vím
        <span className="font-normal text-slate-400">· {sources.length}</span>
        <ChevronRight className="ml-auto size-3.5 transition-transform group-open:rotate-90" aria-hidden />
      </summary>
      <ul className="mt-2 space-y-2">
        {sources.map((source) => (
          <li key={`${source.kind}-${source.title}`} className="flex gap-2">
            <Database className="mt-0.5 size-3.5 shrink-0 text-slate-400" aria-hidden />
            <span className="min-w-0">
              <span className="block text-[10px] font-bold tracking-wide text-slate-400 uppercase">{source.kind}</span>
              {source.url?.startsWith("https://") ? (
                <a href={source.url} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 font-bold text-hk-navy hover:text-hk-red">
                  {source.title}
                  <ExternalLink className="size-3" aria-hidden />
                </a>
              ) : (
                <span className="font-bold text-hk-navy">{source.title}</span>
              )}
              <span className="mt-0.5 block leading-snug">{source.snippet}</span>
            </span>
          </li>
        ))}
      </ul>
    </details>
  );
}
