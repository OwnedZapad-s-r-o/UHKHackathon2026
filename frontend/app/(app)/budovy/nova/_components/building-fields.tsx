"use client";

import { Minus, Plus, Sparkles } from "lucide-react";
import { CATEGORY_META } from "@/lib/facility-meta";
import { cn } from "@/lib/utils";
import type { FacilityCategory } from "@/types/api";
import { CATEGORY_OPTIONS, ENERGY_LABEL, KIND_OPTIONS, energyCategoryOf } from "./building-options";

export type BuildingDraft = {
  name: string;
  category: FacilityCategory;
  kind: string | null;
  // null = převzít odhad / údaj z katastru
  floors: number | null;
  constructionYear: string;
};

type BuildingFieldsProps = {
  draft: BuildingDraft;
  onChange: (changes: Partial<BuildingDraft>) => void;
  nameSuggestions: string[];
  estimatedFloors: number;
  builtUpArea: number | null;
  cadastreYear: number | null;
};

const MAX_FLOORS = 30;

export function BuildingFields({ draft, onChange, nameSuggestions, estimatedFloors, builtUpArea, cadastreYear }: BuildingFieldsProps) {
  const floors = draft.floors ?? estimatedFloors;
  const year = draft.constructionYear.trim() === "" ? null : Number(draft.constructionYear);
  const validYear = year !== null && Number.isInteger(year) && year >= 1200 && year <= new Date().getFullYear() ? year : null;

  return (
    <div className="space-y-5">
      <fieldset>
        <legend className="text-xs font-bold text-slate-500">K čemu budova slouží</legend>
        <div className="mt-1.5 grid grid-cols-2 gap-2">
          {CATEGORY_OPTIONS.map((option) => {
            const Icon = CATEGORY_META[option.value].icon;
            const selected = draft.category === option.value;

            return (
              <button
                key={option.value}
                type="button"
                aria-pressed={selected}
                // Druh z jiné kategorie by nedával smysl — při změně se zruší
                onClick={() => onChange({ category: option.value, kind: selected ? draft.kind : null })}
                className={cn(
                  "flex items-start gap-2.5 rounded-xl border p-2.5 text-left transition-colors",
                  selected ? "border-hk-red bg-hk-red/5 ring-1 ring-hk-red" : "border-slate-200 bg-white hover:border-slate-300",
                )}
              >
                <Icon className={cn("mt-0.5 size-4.5 shrink-0", selected ? "text-hk-red" : "text-slate-400")} aria-hidden />
                <span className="min-w-0">
                  <span className="block text-[13px] font-bold text-hk-navy">{option.label}</span>
                  <span className="block text-[11px] leading-snug text-slate-500">{option.hint}</span>
                </span>
              </button>
            );
          })}
        </div>
      </fieldset>

      <fieldset>
        <legend className="text-xs font-bold text-slate-500">
          Druh budovy <span className="font-normal text-slate-400">(nepovinné — podle něj vznikne schéma místností)</span>
        </legend>
        <div className="mt-1.5 flex flex-wrap gap-1.5">
          {KIND_OPTIONS[draft.category].map((kind) => (
            <button
              key={kind}
              type="button"
              aria-pressed={draft.kind === kind}
              onClick={() => onChange({ kind: draft.kind === kind ? null : kind })}
              className={cn(
                "h-8 rounded-full border px-3 text-[12.5px] transition-colors",
                draft.kind === kind ? "border-hk-navy bg-hk-navy font-bold text-white" : "border-slate-200 bg-white text-hk-navy hover:border-slate-300",
              )}
            >
              {kind}
            </button>
          ))}
        </div>
      </fieldset>

      <div>
        <label htmlFor="building-name" className="text-xs font-bold text-slate-500">
          Název budovy
        </label>
        <input
          id="building-name"
          value={draft.name}
          onChange={(event) => onChange({ name: event.target.value })}
          placeholder="např. Tělocvična — Gymnázium Jičín"
          maxLength={300}
          className="mt-1.5 h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-[14px] text-hk-navy outline-none placeholder:text-slate-400 focus:border-hk-blue focus:ring-3 focus:ring-hk-blue/15"
        />
        {nameSuggestions.length > 0 && (
          <div className="mt-2 flex flex-wrap items-center gap-1.5">
            <Sparkles className="size-3.5 text-slate-400" aria-hidden />
            {nameSuggestions
              .filter((suggestion) => suggestion !== draft.name)
              .map((suggestion) => (
                <button
                  key={suggestion}
                  type="button"
                  onClick={() => onChange({ name: suggestion })}
                  className="max-w-full truncate rounded-full border border-dashed border-slate-300 bg-slate-50 px-2.5 py-1 text-[12px] text-hk-navy transition-colors hover:border-hk-red hover:text-hk-red"
                >
                  {suggestion}
                </button>
              ))}
          </div>
        )}
      </div>

      <div className="grid grid-cols-2 gap-3">
        <div>
          <p id="building-floors-label" className="text-xs font-bold text-slate-500">
            Nadzemní podlaží
          </p>
          <div className="mt-1.5 flex h-11 items-center rounded-xl border border-slate-300 bg-white" role="group" aria-labelledby="building-floors-label">
            <button
              type="button"
              aria-label="Ubrat podlaží"
              disabled={floors <= 1}
              onClick={() => onChange({ floors: floors - 1 })}
              className="flex size-11 items-center justify-center text-hk-navy disabled:opacity-30"
            >
              <Minus className="size-4" aria-hidden />
            </button>
            <output className="flex-1 text-center text-[15px] font-bold text-hk-navy tabular-nums" aria-live="polite">
              {floors}
            </output>
            <button
              type="button"
              aria-label="Přidat podlaží"
              disabled={floors >= MAX_FLOORS}
              onClick={() => onChange({ floors: floors + 1 })}
              className="flex size-11 items-center justify-center text-hk-navy disabled:opacity-30"
            >
              <Plus className="size-4" aria-hidden />
            </button>
          </div>
          <p className="mt-1 text-[11px] leading-snug text-slate-500">
            {draft.floors !== null ? "upraveno vámi" : builtUpArea !== null ? `odhad z plochy ${Math.round(builtUpArea)} m²` : "odhad — katastr podlaží nevede"}
          </p>
        </div>

        <div>
          <label htmlFor="building-year" className="text-xs font-bold text-slate-500">
            Rok výstavby
          </label>
          <input
            id="building-year"
            inputMode="numeric"
            value={draft.constructionYear}
            onChange={(event) => onChange({ constructionYear: event.target.value.replace(/\D/g, "").slice(0, 4) })}
            placeholder="neznám"
            aria-invalid={draft.constructionYear !== "" && validYear === null}
            className="mt-1.5 h-11 w-full rounded-xl border border-slate-300 bg-white px-3 text-[15px] font-bold text-hk-navy tabular-nums outline-none placeholder:font-normal placeholder:text-slate-400 focus:border-hk-blue focus:ring-3 focus:ring-hk-blue/15 aria-invalid:border-red-400"
          />
          <p className="mt-1 text-[11px] leading-snug text-slate-500">
            {draft.constructionYear !== "" && validYear === null
              ? "zadejte rok, např. 1974"
              : `${cadastreYear !== null && validYear === cadastreYear ? "z katastru · " : ""}${ENERGY_LABEL[energyCategoryOf(validYear)]}`}
          </p>
        </div>
      </div>
    </div>
  );
}
