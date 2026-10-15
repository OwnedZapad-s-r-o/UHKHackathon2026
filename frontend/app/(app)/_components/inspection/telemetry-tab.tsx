import Link from "next/link";
import { RadioTower } from "lucide-react";
import { getHeatmap, getSection, getTelemetry } from "@/lib/api";
import { formatNumber } from "@/lib/facility-meta";
import type { FacilityDetail, Telemetry, TelemetryPoint } from "@/types/api";
import { EnergyBars } from "./energy-bars";
import { HeatmapGrid } from "./heatmap-grid";
import { PanelCard, StatTile } from "./panel-card";
import { RoomsNow } from "./rooms-now";
import { TemperatureChart } from "./temperature-chart";

export async function TelemetryTab({ facility }: { facility: FacilityDetail }) {
  const [temperature, energy, heatmap, section] = await Promise.all([
    getTelemetry(facility.id, "Temperature", { hours: 48 }),
    getTelemetry(facility.id, "Power", { bucket: "Day", hours: 24 * 7 }),
    getHeatmap(facility.id, "Power"),
    getSection(facility.id),
  ]);

  const averageTemperature = averageAcrossZones(temperature);
  const dailyEnergy = energy.series[0]?.points ?? [];
  const assessment = facility.assessment;
  const liveZones = temperature.series.filter((s) => s.live).map((s) => s.name);

  return (
    <>
      {liveZones.length > 0 && (
        <Link
          href="/senzory"
          className="mt-3.5 flex items-center gap-3 rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-emerald-900 transition-colors hover:bg-emerald-100"
        >
          <span className="relative flex size-8 shrink-0 items-center justify-center rounded-lg bg-emerald-600 text-white">
            <RadioTower className="size-4" aria-hidden />
            <span className="absolute -top-0.5 -right-0.5 size-2.5 animate-ping rounded-full bg-emerald-400" aria-hidden />
          </span>
          <span className="min-w-0 text-[12.5px]">
            <span className="block font-bold">Skutečná čidla IQRF</span>
            <span className="block truncate">{liveZones.join(", ")} — ostatní zóny zatím simulace</span>
          </span>
        </Link>
      )}

      <div className="mt-3.5 grid grid-cols-3 gap-2.5">
        <StatTile label="Teplota" value={formatNumber(assessment?.temperature)} note="°C · průměr zón" />
        <StatTile label="CO₂" value={formatNumber(assessment?.co2)} note="ppm · nejhorší zóna" tone={(assessment?.co2 ?? 0) > 1500 ? "warning" : "default"} />
        <StatTile label="Příkon" value={formatNumber(assessment?.power)} note="kWh/h" />
      </div>

      <RoomsNow section={section} />

      <PanelCard title="Teplota za 48 h" meta={`${temperature.series.length} zón · hodinový průměr`}>
        <TemperatureChart points={averageTemperature} setback={18} />
      </PanelCard>

      <PanelCard title="Spotřeba energie" meta="kWh / den">
        <EnergyBars points={dailyEnergy} />
      </PanelCard>

      <PanelCard title="Provozní heatmapa" meta="příkon · hodina × den">
        <HeatmapGrid heatmap={heatmap} />
      </PanelCard>
    </>
  );
}

// Jedna křivka za celý objekt — průměr všech zón v každé hodině
function averageAcrossZones(telemetry: Telemetry): TelemetryPoint[] {
  const byTimestamp = new Map<string, number[]>();

  for (const series of telemetry.series) {
    for (const point of series.points) {
      const values = byTimestamp.get(point.timestamp) ?? [];
      values.push(point.value);
      byTimestamp.set(point.timestamp, values);
    }
  }

  return [...byTimestamp.entries()]
    .map(([timestamp, values]) => ({ timestamp, value: values.reduce((a, b) => a + b, 0) / values.length }))
    .sort((a, b) => a.timestamp.localeCompare(b.timestamp));
}
