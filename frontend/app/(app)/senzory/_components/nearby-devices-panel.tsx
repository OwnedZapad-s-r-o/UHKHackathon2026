"use client";

import { useEffect, useState } from "react";
import { RadioTower, StickyNote } from "lucide-react";
import { cn } from "@/lib/utils";
import type { NearbyDevice } from "@/types/api";
import { DeviceSticker, MetricChips, SignalBars } from "./pairing-parts";
import { PairDeviceForm, type FacilityOption } from "./pair-device-form";

// Nové zařízení se ohlašuje každých 10 s — obnovou po 5 s se objeví nejpozději pár sekund po zapnutí
const REFRESH_MS = 5000;

type NearbyDevicesPanelProps = {
  initial: NearbyDevice[];
  facilities: FacilityOption[];
};

// „Nová zařízení online“: gatewaye, které jsou zapnuté, ale ještě je nikdo nepřipojil — připojí se heslem z nálepky
export function NearbyDevicesPanel({ initial, facilities }: NearbyDevicesPanelProps) {
  const [devices, setDevices] = useState(initial);
  const [pairing, setPairing] = useState<string | null>(null);

  useEffect(() => {
    const timer = setInterval(async () => {
      const response = await fetch("/senzory/nova-zarizeni", { cache: "no-store" }).catch(() => null);
      if (!response?.ok) return;
      const next = (await response.json()) as NearbyDevice[];
      // Rozpracované párování nechat na místě, i když se zařízení mezitím přestalo ohlašovat (právě se páruje).
      // Pořadí podle serialu — řádky pod rukou neposkakují.
      setDevices((current) => {
        const open = current.find((d) => d.serial === pairing);
        const merged = open && !next.some((d) => d.serial === open.serial) ? [...next, open] : next;
        return merged.toSorted((a, b) => a.serial.localeCompare(b.serial));
      });
    }, REFRESH_MS);
    return () => clearInterval(timer);
  }, [pairing]);

  return (
    <section data-tour="senzory-nove" aria-labelledby="nearby" className="rounded-2xl border border-emerald-200 bg-white">
      <div className="flex flex-wrap items-baseline justify-between gap-2 border-b border-slate-100 px-5 py-3.5">
        <h2 id="nearby" className="flex items-center gap-2 text-[14px] font-bold">
          <span className={cn("size-2 rounded-full", devices.length > 0 ? "animate-pulse bg-emerald-500" : "bg-slate-300")} aria-hidden />
          Nová zařízení online
          {devices.length > 0 && <span className="rounded-full bg-emerald-50 px-2 py-0.5 text-[11.5px] text-emerald-800">{devices.length}</span>}
        </h2>
        <p className="text-xs text-slate-500">Zapnutá gateway, kterou ještě nikdo nepřipojil. Připojíte ji heslem z nálepky.</p>
      </div>

      {devices.length === 0 ? (
        <p className="px-5 py-4 text-[13px] text-slate-600">
          Žádné nové zařízení. Zapněte gateway a počkejte asi 30 s, až se ohlásí — nebo ji připojte ručně formulářem.
        </p>
      ) : (
        <ul className="divide-y divide-slate-100">
          {devices.map((device) => (
            <DeviceRow
              key={device.serial}
              device={device}
              facilities={facilities}
              open={pairing === device.serial}
              onOpen={() => setPairing(device.serial)}
              onClose={() => setPairing(null)}
            />
          ))}
        </ul>
      )}
    </section>
  );
}

type DeviceRowProps = {
  device: NearbyDevice;
  facilities: FacilityOption[];
  open: boolean;
  onOpen: () => void;
  onClose: () => void;
};

function DeviceRow({ device, facilities, open, onOpen, onClose }: DeviceRowProps) {
  const [showSticker, setShowSticker] = useState(false);
  const sensorCount = device.nodes.reduce((sum, node) => sum + node.metrics.length, 0);

  return (
    <li className="px-5 py-3.5">
      <div className="flex flex-wrap items-center gap-x-4 gap-y-2">
        <span className="relative flex size-9 shrink-0 items-center justify-center rounded-lg bg-emerald-50 text-emerald-700">
          <RadioTower className="size-4.5" strokeWidth={1.75} aria-hidden />
          <span className="absolute -top-0.5 -right-0.5 size-2.5 animate-pulse rounded-full border-2 border-white bg-emerald-500" aria-hidden />
        </span>
        <div className="min-w-0 flex-1">
          <p className="flex flex-wrap items-center gap-x-2 text-[13.5px] font-bold">
            IQRF gateway {device.model}
            <code className="font-mono text-[12px] font-normal text-slate-500">MID {device.serial}</code>
            {device.emulated && (
              <span className="rounded-full border border-hk-blue/25 bg-[#EFF4FA] px-2 py-0.5 text-[10.5px] text-hk-blue" title="Zařízení hraje emulátor nebo aplikace — chová se jako skutečné">
                emulované
              </span>
            )}
          </p>
          <p className="mt-1 flex flex-wrap items-center gap-x-3 gap-y-1">
            <SignalBars rssi={device.rssi} />
            <span className="text-[11.5px] text-slate-500">
              {device.nodes.length} {device.nodes.length === 1 ? "uzel" : device.nodes.length < 5 ? "uzly" : "uzlů"} · {sensorCount}{" "}
              {sensorCount === 1 ? "čidlo" : sensorCount < 5 ? "čidla" : "čidel"}
            </span>
            <MetricChips metrics={device.nodes.flatMap((n) => n.metrics)} />
          </p>
        </div>
        <div className="flex items-center gap-2">
          {device.label && (
            <button
              type="button"
              onClick={() => setShowSticker(!showSticker)}
              aria-expanded={showSticker}
              className="inline-flex h-9 items-center gap-1.5 rounded-lg border border-slate-200 px-3 text-[12.5px] font-bold text-slate-600 transition-colors hover:bg-slate-50"
            >
              <StickyNote className="size-3.5" aria-hidden />
              Nálepka
            </button>
          )}
          {!open && (
            <button
              type="button"
              onClick={onOpen}
              className="inline-flex h-9 items-center gap-1.5 rounded-lg bg-hk-red px-3 text-[12.5px] font-bold text-white transition-colors hover:bg-hk-red-dark"
            >
              Připojit do mé budovy
            </button>
          )}
        </div>
      </div>

      {showSticker && device.label && (
        <div className="mt-3">
          <DeviceSticker
            title={`IQRF gateway ${device.label.model}`}
            rows={[
              { label: "MID", value: device.label.serial },
              { label: "Heslo", value: device.label.password, secret: true },
            ]}
          />
        </div>
      )}

      {open && <PairDeviceForm serial={device.serial} facilities={facilities} onCancel={onClose} />}
    </li>
  );
}
