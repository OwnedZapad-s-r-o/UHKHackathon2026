import Link from "next/link";
import { ChevronRight, MapPinPlus } from "lucide-react";
import { STATUS_META } from "@/lib/facility-meta";
import { cn } from "@/lib/utils";
import type { FacilityListItem } from "@/types/api";

const SEVERITY: Record<FacilityListItem["status"], number> = { Critical: 0, Warning: 1, Nominal: 2 };

// Úvod správce budovy: jeho budovy jako seznam nad mapou — nejdřív ty se zjištěním, klik otevře inspekční panel
export function OwnBuildings({ facilities }: { facilities: FacilityListItem[] }) {
  const ordered = facilities.toSorted((a, b) => SEVERITY[a.status] - SEVERITY[b.status] || a.name.localeCompare(b.name, "cs"));

  return (
    <section
      data-tour="moje-budovy"
      aria-label="Moje budovy"
      className="absolute top-3 right-3 z-10 max-h-[calc(100%-1.5rem)] w-80 overflow-y-auto rounded-xl border border-slate-200 bg-white shadow-[0_8px_20px_rgba(11,25,44,0.12)]"
    >
      <h2 className="border-b border-slate-100 px-4 pt-3.5 pb-3 text-[13px] font-bold">Moje budovy</h2>

      {ordered.length === 0 ? (
        // Onboarding: správce bez budov má jedinou smysluplnou akci — přidat tu svoji
        <div className="px-4 py-4">
          <p className="text-[13px] font-bold text-hk-navy">Začněte svou budovou</p>
          <p className="mt-1 text-[12.5px] leading-snug text-slate-600">
            Najděte ji adresou nebo kliknutím do mapy. Obec, půdorys z katastru i stáří doplníme sami a audit ji vyhodnotí do minuty.
          </p>
          <Link
            href="/budovy/nova"
            className="mt-3 inline-flex h-10 w-full items-center justify-center gap-2 rounded-lg bg-hk-red px-4 text-[13px] font-bold text-white transition-colors hover:bg-hk-red-dark"
          >
            <MapPinPlus className="size-4" aria-hidden />
            Přidat první budovu
          </Link>
        </div>
      ) : (
        <ul>
          {ordered.map((facility) => {
            const StatusIcon = STATUS_META[facility.status].icon;

            return (
              <li key={facility.id} className="border-t border-slate-100 first:border-t-0">
                <Link href={`/mapa?objekt=${facility.id}`} scroll={false} className="group flex items-start gap-3 px-4 py-3 transition-colors hover:bg-slate-50">
                  <span className="min-w-0 flex-1">
                    <span className="block text-[13px] leading-snug font-bold group-hover:text-hk-red">{facility.name}</span>
                    <span className={cn("mt-1.5 inline-flex h-5.5 items-center gap-1 rounded-full border px-2 text-[11px] font-bold", STATUS_META[facility.status].pill)}>
                      <StatusIcon className="size-3" aria-hidden />
                      {STATUS_META[facility.status].label}
                    </span>
                    {facility.status !== "Nominal" && <span className="mt-1 block text-xs leading-snug text-slate-600">{facility.headline}</span>}
                  </span>
                  <ChevronRight className="mt-0.5 size-4 shrink-0 text-slate-300 group-hover:text-hk-red" aria-hidden />
                </Link>
              </li>
            );
          })}
        </ul>
      )}
    </section>
  );
}
