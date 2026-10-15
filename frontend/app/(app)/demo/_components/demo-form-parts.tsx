"use client";

import { Flame, Snowflake } from "lucide-react";
import { METRIC_META } from "@/lib/sensor-meta";
import { cn } from "@/lib/utils";
import type { DemoBehavior, DemoOverview } from "@/types/api";

export type DemoTemplate = DemoOverview["templates"][number];

const BEHAVIORS: { value: DemoBehavior; label: string; note: string; icon: typeof Flame }[] = [
  { value: "Setback", label: "Útlum funguje", note: "v noci a o víkendu ~18 °C → ztráta klesne", icon: Snowflake },
  { value: "Overheating", label: "Přetápí", note: "mimo provoz ~22 °C → ztráta zůstane", icon: Flame },
];

// Co zařízení naměří mimo provoz budovy — podle toho se po přiřazení do místnosti opraví odhad ztráty
export function BehaviorPicker({ name, value, onChange }: { name: string; value: DemoBehavior; onChange: (value: DemoBehavior) => void }) {
  return (
    <fieldset>
      <legend className="text-xs text-slate-500">Co bude teploměr měřit</legend>
      <div className="mt-1 grid grid-cols-2 gap-2">
        {BEHAVIORS.map((behavior) => {
          const Icon = behavior.icon;
          const checked = value === behavior.value;
          return (
            <label
              key={behavior.value}
              className={cn(
                "cursor-pointer rounded-lg border p-2.5 transition-colors has-focus-visible:ring-2 has-focus-visible:ring-hk-blue/40",
                checked ? "border-hk-blue bg-[#EFF4FA]" : "border-slate-200 hover:bg-slate-50",
              )}
            >
              <input type="radio" name={name} value={behavior.value} checked={checked} onChange={() => onChange(behavior.value)} className="sr-only" />
              <span className="flex items-center gap-1.5 text-[13px] font-bold">
                <Icon className={cn("size-3.5", behavior.value === "Overheating" ? "text-red-600" : "text-hk-blue")} aria-hidden />
                {behavior.label}
              </span>
              <span className="mt-0.5 block text-[11.5px] leading-snug text-slate-500">{behavior.note}</span>
            </label>
          );
        })}
      </div>
    </fieldset>
  );
}

export function TemplateLabel({ template }: { template: DemoTemplate }) {
  return (
    <>
      <span className="text-[13px] font-bold">{template.label}</span>
      <span className="block text-[11.5px] text-slate-500">{template.metrics.map((m) => METRIC_META[m].label).join(" · ")}</span>
    </>
  );
}
