import type { SensorMetric } from "@/types/api";

// Veličiny čidel česky — stejné pořadí a jednotky jako SensorUnits v API
export const METRIC_META: Record<SensorMetric, { label: string; unit: string }> = {
  Temperature: { label: "Teplota", unit: "°C" },
  Co2: { label: "CO₂", unit: "ppm" },
  Humidity: { label: "Vlhkost", unit: "%" },
  Power: { label: "Příkon", unit: "kWh/h" },
  Voc: { label: "Těkavé látky (VOC)", unit: "VOC" },
  Illuminance: { label: "Osvětlení", unit: "lx" },
  Pressure: { label: "Tlak", unit: "hPa" },
  Pm25: { label: "Prach PM2,5", unit: "µg/m³" },
  Noise: { label: "Hluk", unit: "dB" },
  Motion: { label: "Pohyb", unit: "" },
  Other: { label: "Jiná veličina", unit: "" },
};

export const METRICS = Object.keys(METRIC_META) as SensorMetric[];

const relative = new Intl.RelativeTimeFormat("cs-CZ", { numeric: "auto" });

// „před 12 s“, „před 3 min“ — stáří posledního kontaktu gatewaye
export function formatAgo(timestamp: string | null, now = Date.now()) {
  if (!timestamp) return "zatím nikdy";

  const seconds = Math.round((new Date(timestamp).getTime() - now) / 1000);
  if (seconds > -60) return relative.format(Math.min(0, seconds), "second");
  if (seconds > -3600) return relative.format(Math.round(seconds / 60), "minute");
  if (seconds > -86400) return relative.format(Math.round(seconds / 3600), "hour");
  return relative.format(Math.round(seconds / 86400), "day");
}

export function formatSensorValue(value: number | null, metric: SensorMetric, unit: string) {
  if (value === null) return "—";
  if (metric === "Motion") return value > 0 ? "pohyb" : "klid";

  const digits = Math.abs(value) >= 100 ? 0 : 1;
  return `${value.toLocaleString("cs-CZ", { maximumFractionDigits: digits })}${unit ? ` ${unit}` : ""}`;
}

// Testovací síť kraje (backend VirtualGateways.Network) — podle nich se pozná, že už běží
export const DEMO_NETWORK_GATEWAYS = ["gw-hk-tyl", "gw-jc-moa", "gw-tu-gym", "gw-na-jir", "gw-rk-sps"];
