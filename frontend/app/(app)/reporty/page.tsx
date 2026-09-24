import type { Metadata } from "next";
import { Building2, FileDown, Landmark } from "lucide-react";
import { Checkbox } from "@/components/ui/checkbox";
import { getFacilities, getSession } from "@/lib/api";
import { DistrictSelect } from "../_components/district-select";
import { ExecutiveSummaryLink } from "../_components/executive-summary-link";
import { TopBar } from "../_components/top-bar";
import { FacilityReportPicker } from "./_components/facility-report-picker";

export const metadata: Metadata = {
  title: "Reporty · SmartAudit KHK",
};

export default async function ReportsPage() {
  const [facilities, session] = await Promise.all([getFacilities(), getSession()]);

  const options = facilities
    .toSorted((a, b) => a.districtName.localeCompare(b.districtName, "cs") || a.name.localeCompare(b.name, "cs"))
    .map((f) => ({ value: f.id, label: `${f.code} · ${f.name}`, district: f.districtName }));

  return (
    <>
      <TopBar title="Reporty" />

      <main className="min-h-0 flex-1 overflow-y-auto p-6">
        <h1 className="text-[32px] leading-tight font-bold tracking-tight">Reporty</h1>
        <p className="mt-1.5 max-w-2xl text-sm text-slate-600">
          PDF podklady z telemetrie a otevřených dat — audit jednoho objektu pro správce budovy, přehled portfolia pro investiční odbor
          a jednostránkový souhrn pro Radu kraje.
        </p>

        {session?.role === "Kraj" && (
          <section className="mt-6 flex flex-wrap items-center gap-4 rounded-2xl border-2 border-hk-red/20 bg-white p-5" aria-labelledby="report-summary">
            <span className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-red-50 text-hk-red">
              <Landmark className="size-5" aria-hidden />
            </span>
            <div className="min-w-0 flex-1">
              <h2 id="report-summary" className="text-lg font-bold tracking-tight">
                Souhrn pro Radu kraje — jedna strana A4
              </h2>
              <p className="mt-1 text-[13px] text-slate-600">
                Odhad ztráty celého kraje, mapa okresů, pět budov s nejvyšší ztrátou, pilot čidel IQRF s návratností a návrh usnesení.
              </p>
            </div>
            <ExecutiveSummaryLink className="inline-flex h-12 items-center justify-center gap-2 rounded-lg bg-hk-red px-5 text-sm font-bold text-white transition-colors hover:bg-hk-red-dark" />
          </section>
        )}

        <div className="mt-6 grid gap-4 lg:grid-cols-2">
          <section className="rounded-2xl border border-slate-200 bg-white p-5" aria-labelledby="report-facility">
            <div className="flex items-center gap-2.5">
              <Building2 className="size-5 text-hk-blue" aria-hidden />
              <h2 id="report-facility" className="text-lg font-bold tracking-tight">
                Energetický audit objektu
              </h2>
            </div>
            <p className="mt-2 text-[13px] text-slate-600">
              Zjištění, teplota za 48 h, spotřeba po dnech, provozní heatmapa a kontext z otevřených dat (ČÚZK, obsazenost, HSOO, památky).
            </p>
            <FacilityReportPicker options={options} />
          </section>

          <section className="rounded-2xl border border-slate-200 bg-white p-5" aria-labelledby="report-portfolio">
            <div className="flex items-center gap-2.5">
              <FileDown className="size-5 text-hk-blue" aria-hidden />
              <h2 id="report-portfolio" className="text-lg font-bold tracking-tight">
                Přehled portfolia
              </h2>
            </div>
            <p className="mt-2 text-[13px] text-slate-600">
              Souhrn kraje nebo okresu, odhad ztrát podle ORP a pořadí objektů podle závažnosti — podklad pro investiční odbor.
            </p>

            {/* GET formulář — výběr okresu a checkbox odesílají hodnoty skrytými poli (Base UI) */}
            <form action="/reporty/portfolio" className="mt-4 space-y-3">
              <div className="space-y-1">
                <p className="text-xs text-slate-500">Okres</p>
                <DistrictSelect name="okres" className="h-10 w-full text-sm" />
              </div>
              <label className="flex items-center gap-2.5 text-sm">
                <Checkbox name="zjisteni" value="1" className="data-checked:border-hk-blue data-checked:bg-hk-blue" />
                Jen objekty se zjištěním
              </label>
              <button
                type="submit"
                className="inline-flex h-12 w-full items-center justify-center gap-2 rounded-lg bg-hk-red px-4 text-sm font-bold text-white transition-colors hover:bg-hk-red-dark"
              >
                <FileDown className="size-4" aria-hidden />
                Stáhnout PDF portfolia
              </button>
            </form>
          </section>
        </div>
      </main>
    </>
  );
}
