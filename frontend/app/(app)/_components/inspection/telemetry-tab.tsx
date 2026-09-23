import { getHeatmap, getTelemetry } from "@/lib/api";
import { formatNumber } from "@/lib/facility-meta";
import type { FacilityDetail, Telemetry, TelemetryPoint } from "@/types/api";
import { EnergyBars } from "./energy-bars";
import { HeatmapGrid } from "./heatmap-grid";
import { PanelCard, StatTile } from "./panel-card";
import { TemperatureChart } from "./temperature-chart";

export async function TelemetryTab({ facility }: { facility: FacilityDetail }) {
  const [temperature, energy, heatmap] = await Promise.all([
    getTelemetry(facility.id, "Temperature", { hours: 48 }),
    getTelemetry(facility.id, "Power", { bucket: "Day", hours: 24 * 7 }),
    getHeatmap(facility.id, "Power"),
  ]);

  const averageTemperature = averageAcrossZones(temperature);
  const dailyEnergy = energy.series[0]?.points ?? [];
  const assessment = facility.assessment;

  return (
    <>
      <div className="mt-3.5 grid grid-cols-3 gap-2.5">
        <StatTile label="Teplota" value={formatNumber(assessment?.temperature)} note="°C · průměr zón" />
        <StatTile label="CO₂" value={formatNumber(assessment?.co2)} note="ppm · nejhorší zóna" tone={(assessment?.co2 ?? 0) > 1500 ? "warning" : "default"} />
        <StatTile label="Příkon" value={formatNumber(assessment?.power)} note="kWh/h" />
      </div>

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
