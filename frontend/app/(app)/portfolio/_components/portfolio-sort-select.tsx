"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { ArrowDownUp } from "lucide-react";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { withParams } from "@/lib/search-params";

type PortfolioSortSelectProps = {
  options: { value: string; label: string }[];
  sortKey: string;
  direction: "asc" | "desc";
};

// Řazení karet na úzké obrazovce — v tabulce se řadí klikem na hlavičku sloupce
export function PortfolioSortSelect({ options, sortKey, direction }: PortfolioSortSelectProps) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  function sort(key: string) {
    // Stejný sloupec podruhé = opačný směr
    const nextDirection = key === sortKey && direction === "asc" ? "desc" : "asc";
    router.push(withParams(pathname, searchParams, { razeni: key, smer: nextDirection }));
  }

  return (
    <div className="flex items-center gap-1.5">
      <ArrowDownUp className="size-3.5 text-slate-400" aria-hidden />
      <Select items={options} value={sortKey} onValueChange={(next) => next && sort(next as string)}>
        <SelectTrigger aria-label="Řadit podle" className="h-8.5 min-w-36 border-slate-200 bg-white text-[13px]">
          <SelectValue />
        </SelectTrigger>
        <SelectContent alignItemWithTrigger={false} align="end" className="w-max min-w-(--anchor-width) text-[13px]">
          {options.map((option) => (
            <SelectItem key={option.value} value={option.value}>
              {option.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}
