"use client";

import { useState, useTransition } from "react";
import Link from "next/link";
import { ArrowRight, Cpu, Flame, Loader2, RadioTower, StickyNote, Trash2 } from "lucide-react";
import { cn } from "@/lib/utils";
import type { DemoDevice, DemoStage } from "@/types/api";
import { DeviceSticker, MetricChips } from "../../senzory/_components/pairing-parts";
import { deleteDemoDevice } from "../actions";

// Kroky párování — gateway se nejdřív ohlašuje a spáruje heslem, čidlo je v dosahu a přidá se kódem
const GATEWAY_STEPS: { stage: DemoStage; label: string }[] = [
  { stage: "Starting", label: "Zapíná se" },
  { stage: "Announcing", label: "Ohlašuje se" },
  { stage: "Paired", label: "Spárováno" },
  { stage: "Reporting", label: "Posílá data" },
  { stage: "Assigned", label: "V místnosti" },
];

const NODE_STEPS: { stage: DemoStage; label: string }[] = [
  { stage: "InRange", label: "V dosahu" },
  { stage: "Paired", label: "V síti" },
  { stage: "Reporting", label: "Posílá data" },
  { stage: "Assigned", label: "V místnosti" },
];

export function DemoDeviceList({ devices, onChanged }: { devices: DemoDevice[]; onChanged: () => void }) {
  return (
    <section aria-labelledby="demo-devices" className="rounded-2xl border border-slate-200 bg-white">
      <div className="flex flex-wrap items-baseline justify-between gap-2 border-b border-slate-100 px-5 py-3.5">
        <h2 id="demo-devices" className="flex items-center gap-2 text-[14px] font-bold">
          <span className="size-2 animate-pulse rounded-full bg-emerald-500" aria-hidden />
          Vyrobená zařízení naživo
        </h2>
        <p className="text-xs text-slate-500">Stav se obnovuje každé 3 s — sledujte, jak zařízení postupuje párováním.</p>
      </div>

      {devices.length === 0 ? (
        <p className="px-5 py-5 text-[13px] text-slate-600">Zatím jste nic nevyrobili. Začněte formulářem Vyrobit gateway.</p>
      ) : (
        <ul className="divide-y divide-slate-100">
          {devices.map((device) => (
            <DeviceRow key={device.id} device={device} onChanged={onChanged} />
          ))}
        </ul>
      )}
    </section>
  );
}

function DeviceRow({ device, onChanged }: { device: DemoDevice; onChanged: () => void }) {
  const [showSticker, setShowSticker] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  const gateway = device.kind === "Gateway";
  const Icon = gateway ? RadioTower : Cpu;
  const removable = device.stage === "Starting" || device.stage === "Announcing" || device.stage === "InRange";

  function remove() {
    setError(null);
    startTransition(async () => {
      const result = await deleteDemoDevice(device.id);
      if (result?.error) setError(result.error);
      else onChanged();
    });
  }

  return (
    <li className="px-5 py-4">
      <div className="flex flex-wrap items-start gap-x-4 gap-y-2">
        <span className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-slate-100 text-hk-navy">
          <Icon className="size-4.5" strokeWidth={1.75} aria-hidden />
        </span>
        <div className="min-w-0 flex-1">
          <p className="flex flex-wrap items-center gap-x-2 text-[13.5px] font-bold">
            {gateway ? `Gateway ${device.model}` : `Čidlo ${device.nodes[0] ?? ""}`}
            <code className="font-mono text-[12px] font-normal text-slate-500">MID {device.serial}</code>
            {device.behavior === "Overheating" && (
              <span className="inline-flex items-center gap-1 rounded-full bg-red-50 px-2 py-0.5 text-[10.5px] text-red-800">
                <Flame className="size-3" aria-hidden />
                přetápí
              </span>
            )}
          </p>
          <p className="mt-1 flex flex-wrap items-center gap-x-3 gap-y-1 text-[11.5px] text-slate-500">
            {gateway ? `uzly: ${device.nodes.join(", ")}` : `u gatewaye ${device.parentName ?? "—"}`}
            <MetricChips metrics={device.metrics} />
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={() => setShowSticker(!showSticker)}
            aria-expanded={showSticker}
            className="inline-flex h-9 items-center gap-1.5 rounded-lg border border-slate-200 px-3 text-[12.5px] font-bold text-slate-600 transition-colors hover:bg-slate-50"
          >
            <StickyNote className="size-3.5" aria-hidden />
            Nálepka
          </button>
          {removable && (
            <button
              type="button"
              onClick={remove}
              disabled={isPending}
              aria-label="Vypnout a odebrat zařízení"
              title="Vypnout a odebrat"
              className="inline-flex size-9 items-center justify-center rounded-lg border border-slate-200 text-slate-500 transition-colors hover:bg-red-50 hover:text-red-700 disabled:opacity-50"
            >
              {isPending ? <Loader2 className="size-3.5 animate-spin" aria-hidden /> : <Trash2 className="size-3.5" aria-hidden />}
            </button>
          )}
        </div>
      </div>

      {showSticker && (
        <div className="mt-3">
          <DeviceSticker
            title={gateway ? `IQRF gateway ${device.model}` : "IQRF čidlo"}
            rows={[
              { label: "MID", value: device.serial },
              { label: gateway ? "Heslo" : "Kód", value: device.code, secret: true },
            ]}
          />
        </div>
      )}

      <StageSteps steps={gateway ? GATEWAY_STEPS : NODE_STEPS} stage={device.stage} />
      <NextStep device={device} />

      {error && (
        <p role="alert" className="mt-2 text-[13px] font-bold text-red-700">
          {error}
        </p>
      )}
    </li>
  );
}

function StageSteps({ steps, stage }: { steps: { stage: DemoStage; label: string }[]; stage: DemoStage }) {
  const current = steps.findIndex((s) => s.stage === stage);

  return (
    <ol className="mt-3 flex gap-1" aria-label="Průběh párování">
      {steps.map((step, index) => {
        const done = index < current || (index === current && stage === "Assigned");
        const active = index === current && stage !== "Assigned";
        return (
          <li key={step.stage} className="min-w-0 flex-1" aria-current={active ? "step" : undefined}>
            <span className={cn("block h-1.5 rounded-full", done ? "bg-emerald-500" : active ? "animate-pulse bg-amber-400" : "bg-slate-200")} />
            <span className={cn("mt-1 block truncate text-[11px]", done || active ? "font-bold text-slate-800" : "text-slate-400")}>{step.label}</span>
          </li>
        );
      })}
    </ol>
  );
}

// Co má uživatel udělat teď — s odkazem tam, kde se to dělá
function NextStep({ device }: { device: DemoDevice }) {
  const detail = device.gatewayId ? `/senzory/${device.gatewayId}` : "/senzory";
  const next: Record<DemoStage, { text: string; href?: string; link?: string }> = {
    Starting: { text: "Zařízení se zapíná, do 10 s se ohlásí na brokeru." },
    Announcing: { text: "Čeká na spárování. Na stránce Senzory ho připojte do budovy heslem z nálepky.", href: "/senzory", link: "Nová zařízení online" },
    InRange: { text: "Čidlo je v dosahu gatewaye. Na jejím detailu ho přidejte do sítě kódem z nálepky.", href: detail, link: "Detail gatewaye" },
    Paired: { text: "Spárováno. Gateway se ptá na čidla, první data přijdou do pár sekund.", href: detail, link: "Detail gatewaye" },
    Reporting: { text: `Posílá data (${device.channelCount} veličin). Přiřaďte uzel do místnosti — pak se opraví ztráta.`, href: detail, link: "Přiřadit do místnosti" },
    Assigned: {
      text: `Měří v budově ${device.facilityName ?? ""} (${device.assignedCount}/${device.channelCount} veličin v místnostech). Opravu ztráty vidíte níže.`,
      href: device.facilityId ? `/mapa?objekt=${device.facilityId}` : undefined,
      link: "Budova v mapě",
    },
  };
  const step = next[device.stage];

  return (
    <p className="mt-2 flex flex-wrap items-center gap-x-2 text-[12.5px] text-slate-600">
      {step.text}
      {step.href && step.link && (
        <Link href={step.href} className="inline-flex items-center gap-0.5 font-bold text-hk-blue hover:underline">
          {step.link}
          <ArrowRight className="size-3" aria-hidden />
        </Link>
      )}
    </p>
  );
}
