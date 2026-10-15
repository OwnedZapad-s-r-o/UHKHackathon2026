import type { Metadata } from "next";
import Link from "next/link";
import { Search } from "lucide-react";
import { InputGroup, InputGroupAddon, InputGroupInput } from "@/components/ui/input-group";
import { getFacilities } from "@/lib/api";
import { CATEGORY_META, STATUS_META, formatCzk, formatNumber } from "@/lib/facility-meta";
import { readParam, withParams } from "@/lib/search-params";
import { cn } from "@/lib/utils";
import type { FacilityListItem } from "@/types/api";
import { KpiRow } from "../_components/kpi-row";
import { PortfolioReportLink } from "../_components/portfolio-report-link";
import { TopBar } from "../_components/top-bar";
import { PortfolioSortSelect } from "./_components/portfolio-sort-select";

export const metadata: Metadata = {
  title: "Portfolio · SmartAudit KHK",
};

const STATUS_ORDER = { Critical: 0, Warning: 1, Nominal: 2 } as const;

// Sloupce tabulky; `className` schovává méně důležité sloupce podle šířky obsahu (container query na <main>)
const COLUMNS = [
  { key: "code", label: "ID objektu", align: "left", className: "" },
  { key: "name", label: "Objekt", align: "left", className: "" },
  { key: "district", label: "Okres", align: "left", className: "hidden @6xl:table-cell" },
  { key: "kind", label: "Druh", align: "left", className: "hidden @7xl:table-cell" },
  { key: "temperature", label: "Teplota °C", align: "right", className: "" },
  { key: "co2", label: "CO₂ ppm", align: "right", className: "" },
  { key: "power", label: "kWh/h", align: "right", className: "hidden @5xl:table-cell" },
  { key: "loss", label: "Ztráta / rok", align: "right", className: "" },
  { key: "status", label: "Stav", align: "left", className: "" },
] as const;

type SortKey = (typeof COLUMNS)[number]["key"];

const SORT_OPTIONS = COLUMNS.map((c) => ({ value: c.key, label: c.label }));

export default async function PortfolioPage({ searchParams }: PageProps<"/portfolio">) {
  const params = await searchParams;
  const query = readParam(params, "q");
  const sortKey = (COLUMNS.find((c) => c.key === readParam(params, "razeni"))?.key ?? "status") as SortKey;
  const direction = readParam(params, "smer") === "desc" ? "desc" : "asc";

  const facilities = await getFacilities({ q: query, onlyIssues: readParam(params, "zjisteni") === "1" });
  const rows = sortFacilities(facilities, sortKey, direction);
  const moneyVisible = facilities.some((f) => f.annualLossCzk !== null);
  const columns = COLUMNS.filter((c) => c.key !== "loss" || moneyVisible);

  return (
    <>
      <TopBar title="Portfolio">
        <PortfolioReportLink onlyIssues={readParam(params, "zjisteni") === "1"} />
      </TopBar>

      <main className="@container min-h-0 flex-1 overflow-y-auto p-4 @3xl:p-6">
        <div className="flex flex-wrap items-end gap-4">
          <div className="min-w-0">
            <h1 className="text-[28px] leading-tight font-bold tracking-tight @3xl:text-[32px]">Portfolio objektů</h1>
            <p className="mt-1.5 text-sm text-slate-600">Objekty ve zřizovatelské působnosti Královéhradeckého kraje napojené na telemetrii.</p>
          </div>

          <form action="/portfolio" className="w-full @3xl:ml-auto @3xl:w-72">
            {readParam(params, "zjisteni") && <input type="hidden" name="zjisteni" value="1" />}
            <InputGroup className="h-10 w-full border-slate-300 bg-white">
              <InputGroupAddon>
                <Search className="text-slate-400" aria-hidden />
              </InputGroupAddon>
              <InputGroupInput
                name="q"
                defaultValue={query}
                autoFocus={readParam(params, "hledat") === "1"}
                placeholder="Hledat objekt, ID, obec, ORP…"
                aria-label="Hledat objekt"
              />
            </InputGroup>
          </form>
        </div>

        <KpiRow facilities={facilities} />

        {/* Úzký obsah: karty s řazením ve výběru */}
        <section className="mt-4 @3xl:hidden" aria-label="Objekty">
          <div className="mb-2.5 flex items-center justify-between gap-3">
            <p className="text-xs text-slate-500">{rows.length} záznamů</p>
            <PortfolioSortSelect options={SORT_OPTIONS.filter((o) => o.value !== "loss" || moneyVisible)} sortKey={sortKey} direction={direction} />
          </div>
          <ul className="space-y-2.5">
            {rows.map((facility) => (
              <li key={facility.id}>
                <PortfolioCard facility={facility} moneyVisible={moneyVisible} />
              </li>
            ))}
          </ul>
          {rows.length === 0 && <EmptyState />}
        </section>

        {/* Široký obsah: tabulka, méně důležité sloupce přibývají se šířkou */}
        <div className="mt-4 hidden overflow-hidden rounded-2xl border border-slate-200 bg-white @3xl:block">
          <table className="w-full text-left">
            <thead>
              <tr className="border-b border-slate-200">
                {columns.map((column) => {
                  const active = column.key === sortKey;
                  const nextDirection = active && direction === "asc" ? "desc" : "asc";

                  return (
                    <th
                      key={column.key}
                      scope="col"
                      className={cn("px-3 py-3 text-[11px] font-normal whitespace-nowrap first:pl-4 last:pr-4", column.align === "right" && "text-right", column.className)}
                      aria-sort={active ? (direction === "asc" ? "ascending" : "descending") : undefined}
                    >
                      <Link href={withParams("/portfolio", params, { razeni: column.key, smer: nextDirection })} className={cn("hover:text-black", active ? "font-bold text-black" : "text-slate-500")}>
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

          {rows.length === 0 && <EmptyState />}

          <p className="border-t border-slate-100 px-4 py-3 text-xs text-slate-500">{rows.length} záznamů · klikem na název otevřete objekt v mapě</p>
        </div>
      </main>
    </>
  );
}

function facilityHref(facility: FacilityListItem) {
  return `/?objekt=${facility.id}&panel=telemetrie`;
}

function StatusPill({ facility }: { facility: FacilityListItem }) {
  const status = STATUS_META[facility.status];
  const StatusIcon = status.icon;

  return (
    <span className={cn("inline-flex h-6 max-w-full items-center gap-1.5 rounded-full border px-2.5 text-[11px] font-bold", status.pill)}>
      <StatusIcon className="size-3 shrink-0" aria-hidden />
      <span className="truncate">{facility.headline}</span>
    </span>
  );
}

function PortfolioRow({ facility, moneyVisible }: { facility: FacilityListItem; moneyVisible: boolean }) {
  const CategoryIcon = CATEGORY_META[facility.category].icon;
  const href = facilityHref(facility);

  return (
    <tr className="border-b border-slate-100 text-[13px] last:border-0 hover:bg-slate-50">
      <td className="py-3 pr-3 pl-4 font-mono text-[11px] whitespace-nowrap text-hk-blue">
        <Link href={href} className="hover:text-hk-red">
          {facility.code}
        </Link>
      </td>
      {/* Název zabere zbytek šířky a ořízne se; okres a druh jsou pod ním, dokud nemají vlastní sloupec */}
      <td className="w-full max-w-0 px-3 py-3">
        <Link href={href} className="block truncate font-bold hover:text-hk-red" title={facility.name}>
          {facility.name}
        </Link>
        <span className="block truncate text-[11.5px] text-slate-500 @7xl:hidden">
          <span className="@6xl:hidden">{facility.districtName} · </span>
          {facility.kind}
        </span>
      </td>
      <td className="hidden px-3 py-3 whitespace-nowrap text-slate-700 @6xl:table-cell">{facility.districtName}</td>
      <td className="hidden px-3 py-3 text-slate-500 @7xl:table-cell">
        <span className="inline-flex items-center gap-1.5 whitespace-nowrap">
          <CategoryIcon className="size-3.5" aria-hidden />
          {facility.kind}
        </span>
      </td>
      <td className={cn("px-3 py-3 text-right font-mono whitespace-nowrap", (facility.temperature ?? 0) > 23 && "text-red-800")}>{formatNumber(facility.temperature)}</td>
      <td className={cn("px-3 py-3 text-right font-mono whitespace-nowrap", (facility.co2 ?? 0) > 1500 && "text-amber-800")}>{formatNumber(facility.co2)}</td>
      <td className="hidden px-3 py-3 text-right font-mono whitespace-nowrap @5xl:table-cell">{formatNumber(facility.power)}</td>
      {moneyVisible && <td className="px-3 py-3 text-right font-mono whitespace-nowrap">{facility.annualLossCzk ? formatCzk(facility.annualLossCzk) : "—"}</td>}
      <td className="max-w-56 py-3 pr-4 pl-3">
        <StatusPill facility={facility} />
      </td>
    </tr>
  );
}

function PortfolioCard({ facility, moneyVisible }: { facility: FacilityListItem; moneyVisible: boolean }) {
  return (
    <Link href={facilityHref(facility)} className="block rounded-2xl border border-slate-200 bg-white p-4 transition-colors hover:border-hk-red/40">
      <p className="font-mono text-[11px] text-hk-blue">{facility.code}</p>
      <p className="mt-0.5 text-[14px] leading-snug font-bold">{facility.name}</p>
      <p className="mt-0.5 text-xs text-slate-500">
        {facility.districtName} · {facility.kind}
      </p>
      <div className="mt-2.5">
        <StatusPill facility={facility} />
      </div>
      <dl className="mt-3 grid grid-cols-3 gap-2 border-t border-slate-100 pt-3 text-[12px]">
        <Value label="Teplota" value={formatNumber(facility.temperature, "°C")} />
        <Value label="CO₂" value={formatNumber(facility.co2, "ppm")} />
        {moneyVisible ? <Value label="Ztráta / rok" value={facility.annualLossCzk ? formatCzk(facility.annualLossCzk) : "—"} /> : <Value label="Příkon" value={formatNumber(facility.power, "kWh/h")} />}
      </dl>
    </Link>
  );
}

function Value({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex min-w-0 flex-col-reverse">
      <dt className="text-[11px] text-slate-500">{label}</dt>
      <dd className="truncate font-mono font-bold">{value}</dd>
    </div>
  );
}

function EmptyState() {
  return (
    <div className="p-10 text-center">
      <p className="font-bold">Žádný objekt neodpovídá hledání</p>
      <p className="mt-1 text-sm text-slate-500">Zkuste jiný název, ID kód nebo obec.</p>
    </div>
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
