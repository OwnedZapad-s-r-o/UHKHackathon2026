import { AlertTriangle, Check, HandHeart, Hospital, Landmark, School, Wind, type LucideIcon } from "lucide-react";
import type { AuditStatus, FacilityCategory } from "@/types/api";

// Stavové barvy z prototypu (= Tailwind 500 / 800 / 50 / 200) — vždy spolu s ikonou a popiskem
export const STATUS_META: Record<AuditStatus, { label: string; icon: LucideIcon; dot: string; pill: string }> = {
  Nominal: {
    label: "Optimální chod",
    icon: Check,
    dot: "bg-emerald-500",
    pill: "border-emerald-200 bg-emerald-50 text-emerald-800",
  },
  Warning: {
    label: "Varování",
    icon: Wind,
    dot: "bg-amber-500",
    pill: "border-amber-200 bg-amber-50 text-amber-800",
  },
  Critical: {
    label: "Kritická anomálie",
    icon: AlertTriangle,
    dot: "bg-red-500",
    pill: "border-red-200 bg-red-50 text-red-800",
  },
};

export const CATEGORY_META: Record<FacilityCategory, { label: string; icon: LucideIcon }> = {
  School: { label: "Školy", icon: School },
  Social: { label: "Sociální služby", icon: HandHeart },
  Health: { label: "Zdravotnictví", icon: Hospital },
  Culture: { label: "Kultura", icon: Landmark },
};

// Okresy kraje — kódy odpovídají sadě Hranice okresů
export const DISTRICTS = [
  { code: "CZ0521", name: "Hradec Králové" },
  { code: "CZ0522", name: "Jičín" },
  { code: "CZ0523", name: "Náchod" },
  { code: "CZ0524", name: "Rychnov n. Kn." },
  { code: "CZ0525", name: "Trutnov" },
];

const czk = new Intl.NumberFormat("cs-CZ", { style: "currency", currency: "CZK", maximumFractionDigits: 0 });
const number = new Intl.NumberFormat("cs-CZ", { maximumFractionDigits: 1 });

export function formatCzk(value: number | null | undefined) {
  return value === null || value === undefined ? "—" : czk.format(value);
}

export function formatNumber(value: number | null | undefined, unit?: string) {
  if (value === null || value === undefined) return "—";
  return unit ? `${number.format(value)} ${unit}` : number.format(value);
}

export function formatMillionsCzk(value: number | null | undefined) {
  return value === null || value === undefined ? "—" : `${number.format(value / 1_000_000)} mil. Kč`;
}
