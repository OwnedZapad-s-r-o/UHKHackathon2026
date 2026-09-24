// Kvalita vnitřního vzduchu podle CO₂ — hranice podle běžných doporučení pro učebny a pobytové místnosti:
// do 1 000 ppm v pořádku, 1 000–1 500 ppm je čas vyvětrat, nad 1 500 ppm (limit vyhlášky) vydýchaný vzduch
export type AirLevel = "good" | "fair" | "poor";

export const AIR_META: Record<AirLevel, { label: string; advice: string; dot: string; pill: string; text: string }> = {
  good: {
    label: "Čerstvý vzduch",
    advice: "Vzduch je v pořádku.",
    dot: "bg-emerald-500",
    pill: "border-emerald-200 bg-emerald-50 text-emerald-800",
    text: "text-emerald-700",
  },
  fair: {
    label: "Čas vyvětrat",
    advice: "Vyvětrejte o nejbližší přestávce — nárazově, okna dokořán na 5 minut.",
    dot: "bg-amber-500",
    pill: "border-amber-200 bg-amber-50 text-amber-800",
    text: "text-amber-700",
  },
  poor: {
    label: "Vydýchaný vzduch",
    advice: "Vyvětrejte hned. Nad 1 500 ppm klesá soustředění a přibývá únavy a bolestí hlavy.",
    dot: "bg-red-500",
    pill: "border-red-200 bg-red-50 text-red-800",
    text: "text-red-700",
  },
};

export function airLevel(co2: number | null | undefined): AirLevel | null {
  if (co2 === null || co2 === undefined) return null;
  if (co2 > 1500) return "poor";
  if (co2 > 1000) return "fair";
  return "good";
}
