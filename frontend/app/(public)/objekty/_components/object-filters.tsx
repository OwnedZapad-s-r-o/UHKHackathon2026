"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { CATEGORY_META, DISTRICTS } from "@/lib/facility-meta";
import { withParams } from "@/lib/search-params";
import type { FacilityCategory } from "@/types/api";

const ALL = "vse";

const DISTRICT_ITEMS = [{ value: ALL, label: "Všechny okresy" }, ...DISTRICTS.map((d) => ({ value: d.code, label: `Okres ${d.name}` }))];

const CATEGORY_ITEMS = [
  { value: ALL, label: "Všechny druhy" },
  ...(Object.keys(CATEGORY_META) as FacilityCategory[]).map((key) => ({ value: key, label: CATEGORY_META[key].label })),
];

const SORT_ITEMS = [
  { value: "nazev", label: "Podle názvu" },
  { value: "vzduch", label: "Nejčerstvější vzduch" },
];

// Filtry veřejného přehledu — všechno v URL, aby šel odkaz poslat dál
export function ObjectFilters() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  function set(key: string, value: string | null) {
    router.push(withParams(pathname, searchParams, { [key]: value === ALL ? null : value }), { scroll: false });
  }

  return (
    <div className="flex flex-wrap gap-2">
      <FilterSelect label="Okres" items={DISTRICT_ITEMS} value={searchParams.get("okres") ?? ALL} onChange={(v) => set("okres", v)} />
      <FilterSelect label="Druh objektu" items={CATEGORY_ITEMS} value={searchParams.get("druh") ?? ALL} onChange={(v) => set("druh", v)} />
      <FilterSelect label="Řazení" items={SORT_ITEMS} value={searchParams.get("razeni") ?? "nazev"} onChange={(v) => set("razeni", v === "nazev" ? null : v)} />
    </div>
  );
}

type FilterSelectProps = {
  label: string;
  items: { value: string; label: string }[];
  value: string;
  onChange: (value: string) => void;
};

function FilterSelect({ label, items, value, onChange }: FilterSelectProps) {
  return (
    <Select items={items} value={value} onValueChange={(next) => next && onChange(next as string)}>
      <SelectTrigger aria-label={label} className="h-10 min-w-44 border-slate-300 bg-white text-[13.5px]">
        <SelectValue />
      </SelectTrigger>
      <SelectContent alignItemWithTrigger={false} className="w-max min-w-(--anchor-width) text-[13.5px]">
        {items.map((item) => (
          <SelectItem key={item.value} value={item.value}>
            {item.label}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
