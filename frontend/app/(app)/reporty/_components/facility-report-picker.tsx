"use client";

import { useState } from "react";
import { FileDown } from "lucide-react";
import { Combobox, ComboboxContent, ComboboxEmpty, ComboboxInput, ComboboxItem, ComboboxList } from "@/components/ui/combobox";

type FacilityOption = {
  value: string;
  label: string;
  district: string;
};

// Výběr objektu pro PDF audit — vyhledávací výběr (172 objektů), hledá v kódu a názvu
export function FacilityReportPicker({ options }: { options: FacilityOption[] }) {
  const [selected, setSelected] = useState<FacilityOption | null>(options[0] ?? null);

  return (
    <div className="mt-4 space-y-3">
      <div className="space-y-1">
        <p id="facility-picker-label" className="text-xs text-slate-500">
          Objekt
        </p>
        <Combobox items={options} value={selected} onValueChange={(next) => setSelected(next as FacilityOption | null)}>
          <ComboboxInput placeholder="Hledat objekt nebo kód…" aria-labelledby="facility-picker-label" className="h-10 w-full border-slate-300 bg-white" />
          <ComboboxContent>
            <ComboboxEmpty>Žádný objekt neodpovídá hledání.</ComboboxEmpty>
            <ComboboxList>
              {(item: FacilityOption) => (
                <ComboboxItem key={item.value} value={item}>
                  <span className="min-w-0 truncate">{item.label}</span>
                  <span className="ml-auto shrink-0 text-xs text-slate-400">{item.district}</span>
                </ComboboxItem>
              )}
            </ComboboxList>
          </ComboboxContent>
        </Combobox>
      </div>

      <a
        href={selected ? `/reporty/objekt/${selected.value}` : undefined}
        aria-disabled={!selected}
        className="inline-flex h-12 w-full items-center justify-center gap-2 rounded-lg bg-hk-red px-4 text-sm font-bold text-white transition-colors hover:bg-hk-red-dark aria-disabled:pointer-events-none aria-disabled:opacity-50"
      >
        <FileDown className="size-4" aria-hidden />
        Stáhnout PDF auditu
      </a>
    </div>
  );
}
