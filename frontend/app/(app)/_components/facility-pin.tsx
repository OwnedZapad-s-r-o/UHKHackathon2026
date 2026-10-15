import { CATEGORY_META, STATUS_META, formatCzk } from "@/lib/facility-meta";
import { cn } from "@/lib/utils";
import type { AuditStatus, FacilityCategory, FacilityListItem } from "@/types/api";

// Barvy kapky podle stavu (Tailwind 500 / 600 na obrys) — stejné jako 3D budova a legenda
const PIN_COLORS: Record<AuditStatus, { fill: string; stroke: string }> = {
  Nominal: { fill: "#10b981", stroke: "#059669" },
  Warning: { fill: "#f59e0b", stroke: "#d97706" },
  Critical: { fill: "#ef4444", stroke: "#dc2626" },
};

type FacilityPinProps = {
  category: FacilityCategory;
  status: AuditStatus;
  selected: boolean;
};

// Kapkovitý špendlík: hrot (spodní okraj, MapMarker anchor="bottom") ukazuje přesně na budovu,
// v hlavě bílá ikona druhu. Vybraný je větší a u hrotu pulzuje kroužek ve stavové barvě.
export function FacilityPin({ category, status, selected }: FacilityPinProps) {
  const Icon = CATEGORY_META[category].icon;
  const colors = PIN_COLORS[status];
  const size = selected ? { width: 38, height: 48, icon: "size-4.5", top: "top-[9px]" } : { width: 28, height: 36, icon: "size-3.5", top: "top-[6px]" };

  return (
    <span
      className="relative block cursor-pointer drop-shadow-[0_4px_6px_rgba(11,25,44,0.35)] transition-transform duration-150 hover:-translate-y-0.5 hover:scale-110"
      style={{ width: size.width, height: size.height }}
    >
      {selected && <span className={cn("absolute -bottom-1.5 left-1/2 size-4 -translate-x-1/2 animate-ping rounded-full", STATUS_META[status].dot)} aria-hidden />}
      <svg viewBox="0 0 28 36" width={size.width} height={size.height} className="relative" aria-hidden>
        <path
          d="M14 35c-.6 0-1.1-.3-1.4-.8C10 30 2 21.6 2 14 2 7.4 7.4 2 14 2s12 5.4 12 12c0 7.6-8 16-10.6 20.2-.3.5-.8.8-1.4.8Z"
          fill={colors.fill}
          stroke="#fff"
          strokeWidth={2}
        />
        <circle cx={14} cy={14} r={8.5} fill={colors.stroke} opacity={0.35} />
      </svg>
      <Icon className={cn("absolute left-1/2 -translate-x-1/2 text-white", size.icon, size.top)} strokeWidth={2.25} aria-hidden />
    </span>
  );
}

// Karta po najetí na špendlík — vpravo od něj, aby nezakryla budovu
export function FacilityTooltipContent({ facility }: { facility: FacilityListItem }) {
  const status = STATUS_META[facility.status];
  const StatusIcon = status.icon;

  return (
    <div className="w-64">
      <p className="text-[13px] leading-snug font-bold">{facility.name}</p>
      <p className="mt-0.5 font-mono text-[10.5px] text-slate-500">
        {facility.code} · {facility.municipalityName}
      </p>
      <p className={cn("mt-2 inline-flex items-center gap-1.5 rounded-full border px-2 py-0.5 text-[11px] font-bold", status.pill)}>
        <StatusIcon className="size-3" aria-hidden />
        {facility.headline}
      </p>
      {facility.annualLossCzk !== null && facility.annualLossCzk > 0 && (
        <p className="mt-1.5 text-[11.5px] text-slate-600">
          Odhad ztráty <b className="text-red-700">{formatCzk(facility.annualLossCzk)}</b> / rok
        </p>
      )}
      <p className="mt-2 border-t border-slate-100 pt-1.5 text-[10.5px] text-slate-500">Kliknutím zobrazíte budovu ve 3D</p>
    </div>
  );
}

// Štítek pod vybraným špendlíkem — název a stav, aby bylo jasné, co je vybrané
export function SelectedLabel({ facility }: { facility: FacilityListItem }) {
  const meta = STATUS_META[facility.status];

  return (
    <span className="flex max-w-64 items-center gap-2 rounded-lg bg-hk-navy px-2.5 py-1.5 text-white shadow-[0_10px_24px_rgba(11,25,44,0.35)]">
      <span className={cn("size-2 shrink-0 rounded-full", meta.dot)} aria-hidden />
      <span className="truncate text-[11.5px] font-bold">{facility.name}</span>
      <span className="shrink-0 text-[10.5px] text-white/70">{meta.label}</span>
    </span>
  );
}

// Třídy pro kontejner tooltipu mapcn — přebijí výchozí tmavou bublinu
export const TOOLTIP_CLASS = "rounded-xl border border-slate-200 bg-white px-3.5 py-3 text-hk-navy shadow-[0_14px_32px_rgba(11,25,44,0.18)]";
