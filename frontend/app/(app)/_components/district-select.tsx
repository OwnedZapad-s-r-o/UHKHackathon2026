"use client";

import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { DISTRICTS } from "@/lib/facility-meta";
import { cn } from "@/lib/utils";

// „Celý kraj“ má vlastní hodnotu — prázdnou hodnotu Base UI Select bere jako „nic nevybráno“
export const WHOLE_REGION = "kraj";

const ITEMS = [{ value: WHOLE_REGION, label: "Celý kraj" }, ...DISTRICTS.map((d) => ({ value: d.code, label: `Okres ${d.name}` }))];

type DistrictSelectProps = {
  // Řízený režim (lišta nad mapou) — hodnota a změna zvenku
  value?: string;
  onValueChange?: (value: string) => void;
  // Formulářový režim (Reporty) — hodnota se odešle skrytým polem s tímto jménem
  name?: string;
  className?: string;
};

// Vlastní (ne nativní) výběr okresu nad Base UI Select — stejný vzhled ve všech prohlížečích
export function DistrictSelect({ value, onValueChange, name, className }: DistrictSelectProps) {
  return (
    <Select
      items={ITEMS}
      name={name}
      value={value}
      defaultValue={value === undefined ? WHOLE_REGION : undefined}
      onValueChange={(next) => onValueChange?.((next as string | null) ?? WHOLE_REGION)}
    >
      <SelectTrigger aria-label="Okres" className={cn("border-slate-200 bg-white text-[13px] text-hk-navy hover:bg-slate-50", className)}>
        <SelectValue />
      </SelectTrigger>
      {/* Seznam podle nejdelšího názvu okresu, ne podle šířky tlačítka — jinak se názvy ořezávají */}
      <SelectContent alignItemWithTrigger={false} align="start" className="w-max min-w-(--anchor-width) text-[13px]">
        {ITEMS.map((item) => (
          <SelectItem key={item.value} value={item.value}>
            {item.label}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
