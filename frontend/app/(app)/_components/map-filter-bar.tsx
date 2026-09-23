"use client";

import { useEffect, useState, type ReactNode } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Search } from "lucide-react";
import { InputGroup, InputGroupAddon, InputGroupInput } from "@/components/ui/input-group";
import { DISTRICTS } from "@/lib/facility-meta";
import { withParams } from "@/lib/search-params";
import { cn } from "@/lib/utils";
import { DistrictSelect, WHOLE_REGION } from "./district-select";

// Lišta nad mapou jako v prototypu: výběr okresu a hledání — obojí zapsané do URL.
// Podle šířky mapové části (container query): široká → tlačítka okresů, úzká → rozbalovací výběr.
export function MapFilterBar() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const district = searchParams.get("okres");
  const [query, setQuery] = useState(searchParams.get("q") ?? "");

  // Hledání se do URL propisuje s krátkým zpožděním, ať se nenačítá po každém písmenu
  useEffect(() => {
    const current = searchParams.get("q") ?? "";
    if (query === current) return;

    const timer = setTimeout(() => {
      router.replace(withParams(pathname, searchParams, { q: query.trim() || null }));
    }, 350);

    return () => clearTimeout(timer);
  }, [query, pathname, router, searchParams]);

  function selectDistrict(code: string | null) {
    router.push(withParams(pathname, searchParams, { okres: code }));
  }

  return (
    <div className="flex h-14 shrink-0 items-center gap-3 border-b border-slate-200 bg-white px-3.5">
      <div className="hidden min-w-0 items-center gap-1.5 @4xl:flex">
        <span className="mr-1 text-[11px] text-slate-500">Okres</span>
        <DistrictChip active={district === null} onClick={() => selectDistrict(null)}>
          Celý kraj
        </DistrictChip>
        {DISTRICTS.map((d) => (
          <DistrictChip key={d.code} active={district === d.code} onClick={() => selectDistrict(d.code)}>
            {d.name}
          </DistrictChip>
        ))}
      </div>

      <div className="@4xl:hidden">
        <DistrictSelect value={district ?? WHOLE_REGION} onValueChange={(next) => selectDistrict(next === WHOLE_REGION ? null : next)} className="h-8.5 min-w-40" />
      </div>

      <InputGroup className="ml-auto h-8.5 w-52 min-w-0 shrink border-slate-300 bg-white">
        <InputGroupAddon>
          <Search className="text-slate-400" aria-hidden />
        </InputGroupAddon>
        <InputGroupInput
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Hledat objekt…"
          aria-label="Hledat objekt"
          className="text-[13px]"
        />
      </InputGroup>
    </div>
  );
}

type DistrictChipProps = {
  active: boolean;
  onClick: () => void;
  children: ReactNode;
};

function DistrictChip({ active, onClick, children }: DistrictChipProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      aria-pressed={active}
      className={cn(
        "h-8.5 shrink-0 rounded-lg border px-2.5 text-[13px] whitespace-nowrap transition-colors",
        active ? "border-hk-blue bg-hk-blue font-bold text-white" : "border-slate-200 bg-white text-hk-navy hover:bg-slate-100",
      )}
    >
      {children}
    </button>
  );
}
