import type { Metadata } from "next";
import Link from "next/link";
import { Search } from "lucide-react";
import { getFacilities } from "@/lib/api";
import { CATEGORY_META, STATUS_META, formatCzk, formatNumber } from "@/lib/facility-meta";
import { readParam, withParams } from "@/lib/search-params";
import { cn } from "@/lib/utils";
import type { FacilityListItem } from "@/types/api";
import { KpiRow } from "../_components/kpi-row";
import { TopBar } from "../_components/top-bar";

export const metadata: Metadata = {
  title: "Portfolio · SmartAudit KHK",
};

const STATUS_ORDER = { Critical: 0, Warning: 1, Nominal: 2 } as const;

const COLUMNS = [
  { key: "code", label: "ID objektu", align: "left" },
  { key: "name", label: "Objekt", align: "left" },
  { key: "district", label: "Okres", align: "left" },
  { key: "kind", label: "Druh", align: "left" },
  { key: "temperature", label: "Teplota °C", align: "right" },
  { key: "co2", label: "CO₂ ppm", align: "right" },
  { key: "power", label: "kWh/h", align: "right" },
  { key: "loss", label: "Ztráta / rok", align: "right" },
  { key: "status", label: "Stav", align: "left" },
] as const;

type SortKey = (typeof COLUMNS)[number]["key"];

export default async function PortfolioPage({ searchParams }: PageProps<"/portfolio">) {
  const params = await searchParams;
  const query = readParam(params, "q");
  const sortKey = (COLUMNS.find((c) => c.key === readParam(params, "razeni"))?.key ?? "status") as SortKey;
  const direction = readParam(params, "smer") === "desc" ? "desc" : "asc";

  const facilities = await getFacilities({ q: query, onlyIssues: readParam(params, "zjisteni") === "1" });
  const rows = sortFacilities(facilities, sortKey, direction);
  const moneyVisible = facilities.some((f) => f.annualLossCzk !== null);

  return (
    <>
      <TopBar title="Portfolio" />

      <main className="min-h-0 flex-1 overflow-y-auto p-6">
        <div className="flex flex-wrap items-end gap-4">
          <div>
            <h1 className="text-[32px] leading-tight font-bold tracking-tight">Portfolio objektů</h1>
            <p className="mt-1.5 text-sm text-slate-600">Objekty ve zřizovatelské působnosti Královéhradeckého kraje napojené na telemetrii.</p>
          </div>

          <form action="/portfolio" className="ml-auto">
            {readParam(params, "zjisteni") && <input type="hidden" name="zjisteni" value="1" />}
            <label className="flex h-10 items-center gap-2 rounded-lg border border-slate-300 bg-white px-3 text-slate-400 focus-within:border-hk-navy">
              <Search className="size-4" aria-hidden />
              <span className="sr-only">Hledat objekt</span>
              <input
                name="q"
                defaultValue={query}
                autoFocus={readParam(params, "hledat") === "1"}
                placeholder="Hledat objekt, ID, obec, ORP…"
                className="w-56 bg-transparent text-sm text-black outline-none placeholder:text-slate-400"
              />
            </label>
          </form>
        </div>

        <KpiRow facilities={facilities} />

        <div className="mt-4 overflow-x-auto rounded-2xl border border-slate-200 bg-white">
          <table className="w-full min-w-230 text-left">
            <thead>
              <tr className="border-b border-slate-200">
                {COLUMNS.filter((c) => c.key !== "loss" || moneyVisible).map((column) => {
                  const active = column.key === sortKey;
                  const nextDirection = active && direction === "asc" ? "desc" : "asc";

                  return (
                    <th key={column.key} scope="col" className={cn("px-4 py-3 text-[11px] font-normal", column.align === "right" && "text-right")} aria-sort={active ? (direction === "asc" ? "ascending" : "descending") : undefined}>
                      <Link
                        href={withParams("/portfolio", params, { razeni: column.key, smer: nextDirection })}
                        className={cn("hover:text-black", active ? "font-bold text-black" : "text-slate-500")}
                      >
                        {column.label}
                        {active && (direction === "asc" ? " ↑" : " ↓")}
                      </Link>
                    </th>
                  );
                })}
              </tr>
            </thead>
            <tbody>
              {rows.map((facility) => (
                <PortfolioRow key={facility.id} facility={facility} moneyVisible={moneyVisible} />
              ))}
            </tbody>
          </table>

          {rows.length === 0 && (
            <div className="p-10 text-center">
              <p className="font-bold">Žádný objekt neodpovídá hledání</p>
              <p className="mt-1 text-sm text-slate-500">Zkuste jiný název, ID kód nebo obec.</p>
            </div>
          )}

          <p className="border-t border-slate-100 px-4 py-3 text-xs text-slate-500">
            {rows.length} záznamů · klikem na název otevřete objekt v mapě
          </p>
        </div>
      </main>
    </>
  );
}

function PortfolioRow({ facility, moneyVisible }: { facility: FacilityListItem; moneyVisible: boolean }) {
  const status = STATUS_META[facility.status];
  const StatusIcon = status.icon;
  const CategoryIcon = CATEGORY_META[facility.category].icon;
  const href = `/?objekt=${facility.id}&panel=telemetrie`;

  return (
    <tr className="group border-b border-slate-100 text-[13px] last:border-0 hover:bg-slate-50">
      <td className="px-4 py-3.5 font-mono text-[11px] text-hk-blue">
        <Link href={href} className="hover:text-hk-red">
          {facility.code}
        </Link>
      </td>
      <td className="max-w-80 truncate px-4 py-3.5 font-bold">
        <Link href={href} className="hover:text-hk-red">
          {facility.name}
        </Link>
      </td>
      <td className="px-4 py-3.5 text-slate-700">{facility.districtName}</td>
      <td className="px-4 py-3.5 text-slate-500">
        <span className="inline-flex items-center gap-1.5">
          <CategoryIcon className="size-3.5" aria-hidden />
          {facility.kind}
        </span>
      </td>
      <td className={cn("px-4 py-3.5 text-right font-mono", (facility.temperature ?? 0) > 23 && "text-red-800")}>{formatNumber(facility.temperature)}</td>
      <td className={cn("px-4 py-3.5 text-right font-mono", (facility.co2 ?? 0) > 1500 && "text-amber-800")}>{formatNumber(facility.co2)}</td>
      <td className="px-4 py-3.5 text-right font-mono">{formatNumber(facility.power)}</td>
      {moneyVisible && <td className="px-4 py-3.5 text-right font-mono">{facility.annualLossCzk ? formatCzk(facility.annualLossCzk) : "—"}</td>}
      <td className="px-4 py-3.5">
        <span className={cn("inline-flex h-6 items-center gap-1.5 rounded-full border px-2.5 text-[11px] font-bold whitespace-nowrap", status.pill)}>
          <StatusIcon className="size-3" aria-hidden />
          {facility.headline}
        </span>
      </td>
    </tr>
  );
}

function sortFacilities(facilities: FacilityListItem[], key: SortKey, direction: "asc" | "desc") {
  const value = (f: FacilityListItem): string | number => {
    switch (key) {
      case "code":
        return f.code;
      case "name":
        return f.name;
      case "district":
        return f.districtName;
      case "kind":
        return f.kind;
      case "temperature":
        return f.temperature ?? -Infinity;
      case "co2":
        return f.co2 ?? -Infinity;
      case "power":
        return f.power ?? -Infinity;
      case "loss":
        return f.annualLossCzk ?? 0;
      case "status":
        return STATUS_ORDER[f.status];
    }
  };

  return facilities.toSorted((a, b) => {
    const x = value(a);
    const y = value(b);
    const result = typeof x === "number" && typeof y === "number" ? x - y : String(x).localeCompare(String(y), "cs");
    return direction === "asc" ? result : -result;
  });
}
