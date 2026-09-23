"use client";

import { useState } from "react";
import { FileDown } from "lucide-react";

type FacilityOption = {
  id: string;
  label: string;
  district: string;
};

// Výběr objektu pro PDF audit — objekty seskupené podle okresu
export function FacilityReportPicker({ options }: { options: FacilityOption[] }) {
  const [selectedId, setSelectedId] = useState(options[0]?.id ?? "");
  const districts = [...new Set(options.map((o) => o.district))];

  return (
    <div className="mt-4 space-y-3">
      <label className="block text-xs text-slate-500">
        Objekt
        <select
          value={selectedId}
          onChange={(e) => setSelectedId(e.target.value)}
          className="mt-1 block h-10 w-full rounded-lg border border-slate-300 bg-white px-3 text-sm text-hk-navy"
        >
          {districts.map((district) => (
            <optgroup key={district} label={`Okres ${district}`}>
              {options
                .filter((o) => o.district === district)
                .map((o) => (
                  <option key={o.id} value={o.id}>
                    {o.label}
                  </option>
                ))}
            </optgroup>
          ))}
        </select>
      </label>
      <a
        href={selectedId ? `/reporty/objekt/${selectedId}` : undefined}
        aria-disabled={!selectedId}
        className="inline-flex h-11 w-full items-center justify-center gap-2 rounded-lg bg-hk-accent px-4 text-sm font-bold text-white transition-colors hover:bg-hk-red aria-disabled:pointer-events-none aria-disabled:opacity-50"
      >
        <FileDown className="size-4" aria-hidden />
        Stáhnout PDF auditu
      </a>
    </div>
  );
}
