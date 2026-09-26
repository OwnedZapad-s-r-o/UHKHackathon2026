import { ISSUE_SHARE_COLORS, ISSUE_SHARE_LABELS } from "@/lib/district-shading";
import { CATEGORY_META, STATUS_META } from "@/lib/facility-meta";
import { cn } from "@/lib/utils";
import type { AuditStatus, FacilityCategory, FacilityListItem } from "@/types/api";

const STATUSES: AuditStatus[] = ["Critical", "Warning", "Nominal"];
const CATEGORIES: FacilityCategory[] = ["School", "Social", "Health", "Culture"];

type MapLegendProps = {
  facilities: FacilityListItem[];
  showDistricts: boolean;
  showCategories?: boolean;
};

// Legenda mapy: stav (barva špendlíku), okresy (barva výplně), druh (ikona ve špendlíku).
// Škála okresů jen v pohledu na kraj — u vybraného objektu je výplň okresů vybledlá a škála by jen mátla.
// Správce budovy vidí jen stav svých budov — druhy objektů kraje ho nezajímají.
export function MapLegend({ facilities, showDistricts, showCategories = true }: MapLegendProps) {
  return (
    <div data-tour="legenda" className="absolute bottom-10 left-4 z-10 w-52 rounded-xl border border-slate-200 bg-white p-4 text-hk-navy shadow-[0_8px_20px_rgba(11,25,44,0.12)]">
      <p className="text-xs font-bold">Stav objektů</p>
      <ul className="mt-2 space-y-1.5 text-xs text-slate-700">
        {STATUSES.map((status) => (
          <li key={status} className="flex items-center gap-2">
            <span className={cn("size-2.5 rounded-full", STATUS_META[status].dot)} />
            <span className="flex-1">{STATUS_META[status].label}</span>
            <span className="text-slate-500 tabular-nums">{facilities.filter((f) => f.status === status).length}</span>
          </li>
        ))}
      </ul>

      {showDistricts && (
        <>
          <p className="mt-4 text-xs font-bold">Okresy — podíl se zjištěním</p>
          <div className="mt-2 flex gap-0.5" aria-hidden>
            {ISSUE_SHARE_COLORS.map((color) => (
              <span key={color} className="h-2.5 flex-1 rounded-[3px] border border-slate-200" style={{ background: color }} />
            ))}
          </div>
          <p className="mt-1 flex justify-between text-[10.5px] text-slate-500">
            <span>{ISSUE_SHARE_LABELS[0]}</span>
            <span>{ISSUE_SHARE_LABELS.at(-1)}</span>
          </p>
        </>
      )}

      {showCategories && (
        <>
          <p className="mt-4 text-xs font-bold">Druh objektu</p>
          <ul className="mt-2 space-y-1.5 text-xs text-slate-700">
            {CATEGORIES.map((category) => {
              const Icon = CATEGORY_META[category].icon;

              return (
                <li key={category} className="flex items-center gap-2">
                  <span className="flex size-4.5 items-center justify-center rounded-full bg-hk-navy text-white">
                    <Icon className="size-2.5" strokeWidth={2.5} aria-hidden />
                  </span>
                  <span className="flex-1">{CATEGORY_META[category].label}</span>
                  <span className="text-slate-500 tabular-nums">{facilities.filter((f) => f.category === category).length}</span>
                </li>
              );
            })}
          </ul>
        </>
      )}
    </div>
  );
}
