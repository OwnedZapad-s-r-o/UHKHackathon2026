"use client";

import { Input } from "@/components/ui/input";
import { METRIC_META } from "@/lib/sensor-meta";
import { cn } from "@/lib/utils";
import type { SensorMetric } from "@/types/api";

// Síla signálu jako tři čárky (dBm z ohlášení zařízení)
export function SignalBars({ rssi }: { rssi: number | null }) {
  const level = rssi === null ? 0 : rssi >= -60 ? 3 : rssi >= -72 ? 2 : 1;
  const label = ["neznámý", "slabý", "dobrý", "silný"][level];

  return (
    <span className="inline-flex items-center gap-1.5 text-[11.5px] text-slate-500" title={rssi === null ? undefined : `${rssi} dBm`}>
      <span className="flex items-end gap-0.5" aria-hidden>
        {[1, 2, 3].map((bar) => (
          <span key={bar} className={cn("w-1 rounded-sm", bar <= level ? "bg-emerald-500" : "bg-slate-200")} style={{ height: 4 + bar * 3 }} />
        ))}
      </span>
      signál {label}
    </span>
  );
}

// Veličiny zařízení bez opakování (teplota na dvou uzlech = jeden štítek)
export function MetricChips({ metrics }: { metrics: SensorMetric[] }) {
  return (
    <span className="flex flex-wrap gap-1">
      {[...new Set(metrics)].map((metric) => (
        <span key={metric} className="rounded-full bg-slate-100 px-2 py-0.5 text-[11px] font-bold text-slate-600">
          {METRIC_META[metric].label}
        </span>
      ))}
    </span>
  );
}

// Nálepka emulovaného zařízení — v demu z ní uživatel opíše heslo stejně jako ze spodku skutečné krabičky
export function DeviceSticker({ title, rows }: { title: string; rows: { label: string; value: string; secret?: boolean }[] }) {
  return (
    <div className="w-fit rounded-lg border border-dashed border-slate-300 bg-[repeating-linear-gradient(135deg,#f8fafc_0,#f8fafc_6px,#f1f5f9_6px,#f1f5f9_12px)] p-3">
      <p className="text-[10.5px] font-bold tracking-wide text-slate-500 uppercase">{title}</p>
      <dl className="mt-1.5 grid grid-cols-[auto_auto] gap-x-4 gap-y-0.5 font-mono text-[12.5px]">
        {rows.map((row) => (
          <div key={row.label} className="contents">
            <dt className="text-slate-500">{row.label}</dt>
            <dd className={cn("font-bold", row.secret ? "text-hk-red" : "text-hk-navy")}>{row.value}</dd>
          </div>
        ))}
      </dl>
      <p className="mt-2 text-[10.5px] text-slate-400">Emulované zařízení — nálepku ukazujeme jen v demu.</p>
    </div>
  );
}

type CodeInputProps = Omit<React.ComponentProps<"input">, "value" | "onChange"> & {
  value: string;
  onValueChange: (value: string) => void;
  // Čidlo: kromě krátkého kódu jde vložit i celý naskenovaný IQRF Code — ten se nepřeformátuje
  allowLong?: boolean;
};

// Kód z nálepky ve tvaru XXXX-XXXX: velká písmena a pomlčka se doplní samy, mezery a překlepy v pomlčce nevadí
export function CodeInput({ value, onValueChange, allowLong, className, ...props }: CodeInputProps) {
  function handleChange(raw: string) {
    const normalized = raw.toUpperCase().replace(/[^0-9A-Z]/g, "");
    if (allowLong && normalized.length > 8) {
      onValueChange(raw.trim());
      return;
    }
    const short = normalized.slice(0, 8);
    onValueChange(short.length > 4 ? `${short.slice(0, 4)}-${short.slice(4)}` : short);
  }

  return (
    <Input
      value={value}
      onChange={(event) => handleChange(event.target.value)}
      autoComplete="off"
      autoCapitalize="characters"
      spellCheck={false}
      placeholder="XXXX-XXXX"
      className={cn("h-10 border-slate-300 bg-white font-mono tracking-widest", className)}
      {...props}
    />
  );
}

export function codeIsComplete(code: string) {
  return code.replace(/[^0-9a-z]/gi, "").length >= 8;
}
