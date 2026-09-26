"use client";

import { useEffect, useState, useTransition } from "react";
import { Cpu, Loader2, Plus, StickyNote } from "lucide-react";
import type { NearbyNodes } from "@/types/api";
import { bondNode } from "../actions";
import { CodeInput, DeviceSticker, MetricChips, SignalBars, codeIsComplete } from "./pairing-parts";

// Dotaz jde přes MQTT až na gatewayi — stačí jednou za čas, ne s každou obnovou detailu
const REFRESH_MS = 15000;

type NearbyNodesSectionProps = {
  gatewayId: string;
  // Po přidání čidla obnovit detail — nový uzel a jeho veličiny se objeví v tabulce
  onBonded: () => void;
};

// Čidla v dosahu gatewaye, která ještě nejsou v její síti. Skutečný IQRF daemon seznam neumí — pak zbývá zadat kód z nálepky.
export function NearbyNodesSection({ gatewayId, onBonded }: NearbyNodesSectionProps) {
  const [nearby, setNearby] = useState<NearbyNodes | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  async function load() {
    const response = await fetch(`/senzory/${gatewayId}/v-dosahu`, { cache: "no-store" }).catch(() => null);
    if (response?.ok) setNearby((await response.json()) as NearbyNodes);
  }

  // Dotaz na gatewayi trvá i sekundy — detail se vykreslí hned a čidla v dosahu se doplní, až gateway odpoví
  useEffect(() => {
    const first = setTimeout(load, 0);
    const timer = setInterval(load, REFRESH_MS);
    return () => {
      clearTimeout(first);
      clearInterval(timer);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [gatewayId]);

  function handleBonded(address: number) {
    setMessage(`Čidlo je v síti jako uzel ${address}. Jeho veličiny se níže objeví za pár sekund.`);
    onBonded();
    load();
  }

  const nodes = nearby?.supported ? nearby.nodes : [];

  return (
    <section data-tour="gateway-v-dosahu" aria-labelledby="nearby-nodes" className="rounded-2xl border border-slate-200 bg-white">
      <div className="flex flex-wrap items-baseline justify-between gap-2 border-b border-slate-100 px-5 py-3.5">
        <h2 id="nearby-nodes" className="text-[11px] font-bold tracking-wide text-slate-500 uppercase">
          Čidla v dosahu této gatewaye
        </h2>
        <p className="text-xs text-slate-500">Čidlo přidáte do sítě kódem z jeho nálepky (IQRF SmartConnect).</p>
      </div>

      {message && (
        <p role="status" className="border-b border-slate-100 bg-emerald-50 px-5 py-2.5 text-[13px] font-bold text-emerald-800">
          {message}
        </p>
      )}

      {nearby === null ? (
        <p className="flex items-center gap-2 px-5 py-4 text-[13px] text-slate-500">
          <Loader2 className="size-4 animate-spin" aria-hidden />
          Ptám se gatewaye na čidla v dosahu…
        </p>
      ) : (
        <>
          {nearby.supported && nodes.length === 0 && <p className="px-5 py-4 text-[13px] text-slate-600">Žádné nové čidlo v dosahu. Všechna čidla, která gateway slyší, už jsou v síti.</p>}
          {!nearby.supported && (
            <p className="px-5 py-4 text-[13px] text-slate-600">Tahle gateway seznam čidel v dosahu neukazuje. Nové čidlo přidáte kódem z nálepky níže.</p>
          )}
          {nodes.length > 0 && (
            <ul className="divide-y divide-slate-100">
              {nodes.map((node) => (
                <NodeRow key={node.mid} gatewayId={gatewayId} node={node} onBonded={handleBonded} />
              ))}
            </ul>
          )}
        </>
      )}

      <ManualCode gatewayId={gatewayId} onBonded={handleBonded} />
    </section>
  );
}

function NodeRow({ gatewayId, node, onBonded }: { gatewayId: string; node: NearbyNodes["nodes"][number]; onBonded: (address: number) => void }) {
  const [open, setOpen] = useState(false);
  const [showSticker, setShowSticker] = useState(false);

  return (
    <li className="px-5 py-3">
      <div className="flex flex-wrap items-center gap-x-4 gap-y-2">
        <span className="flex size-8 shrink-0 items-center justify-center rounded-lg bg-slate-100 text-hk-navy">
          <Cpu className="size-4" strokeWidth={1.75} aria-hidden />
        </span>
        <div className="min-w-0 flex-1">
          <p className="text-[13.5px] font-bold">
            Čidlo <code className="font-mono text-[12px] font-normal text-slate-500">MID {node.mid}</code>
          </p>
          <p className="mt-1 flex flex-wrap items-center gap-x-3 gap-y-1">
            <SignalBars rssi={node.rssi} />
            <MetricChips metrics={node.metrics} />
          </p>
        </div>
        <div className="flex items-center gap-2">
          {node.label && (
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
              onClick={() => setOpen(true)}
              className="inline-flex h-9 items-center gap-1.5 rounded-lg bg-hk-red px-3 text-[12.5px] font-bold text-white transition-colors hover:bg-hk-red-dark"
            >
              <Plus className="size-3.5" aria-hidden />
              Přidat do sítě
            </button>
          )}
        </div>
      </div>

      {showSticker && node.label && (
        <div className="mt-3">
          <DeviceSticker title="IQRF čidlo" rows={[{ label: "MID", value: node.label.mid }, { label: "Kód", value: node.label.code, secret: true }]} />
        </div>
      )}

      {open && <CodeForm gatewayId={gatewayId} id={`code-${node.mid}`} onBonded={onBonded} onCancel={() => setOpen(false)} autoFocus />}
    </li>
  );
}

// Skutečný hardware: čidlo v seznamu nebude, kód z nálepky (nebo naskenovaný IQRF Code) se zadá ručně
function ManualCode({ gatewayId, onBonded }: { gatewayId: string; onBonded: (address: number) => void }) {
  const [open, setOpen] = useState(false);

  return (
    <div className="border-t border-slate-100 px-5 py-3">
      {open ? (
        <CodeForm gatewayId={gatewayId} id="code-manual" allowLong onBonded={onBonded} onCancel={() => setOpen(false)} autoFocus />
      ) : (
        <button type="button" onClick={() => setOpen(true)} className="text-[13px] font-bold text-hk-blue hover:underline">
          Mám kód z nálepky čidla, které tu není
        </button>
      )}
    </div>
  );
}

type CodeFormProps = {
  gatewayId: string;
  id: string;
  allowLong?: boolean;
  autoFocus?: boolean;
  onBonded: (address: number) => void;
  onCancel: () => void;
};

function CodeForm({ gatewayId, id, allowLong, autoFocus, onBonded, onCancel }: CodeFormProps) {
  const [code, setCode] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  function submit(event: React.FormEvent) {
    event.preventDefault();
    setError(null);
    startTransition(async () => {
      const result = await bondNode(gatewayId, code);
      if ("error" in result) setError(result.error);
      else onBonded(result.address);
    });
  }

  return (
    <form onSubmit={submit} className="mt-3 space-y-1">
      <label htmlFor={id} className="text-xs text-slate-500">
        Kód z nálepky čidla{allowLong ? " nebo naskenovaný IQRF Code" : ""}
      </label>
      <div className="flex flex-wrap items-center gap-2">
        <CodeInput id={id} value={code} onValueChange={setCode} allowLong={allowLong} aria-invalid={error !== null} className={allowLong ? "w-72" : "w-44"} autoFocus={autoFocus} />
        <button
          type="submit"
          disabled={!codeIsComplete(code) || isPending}
          className="inline-flex h-10 items-center gap-2 rounded-lg bg-hk-red px-4 text-[13px] font-bold text-white transition-colors hover:bg-hk-red-dark disabled:opacity-50"
        >
          {isPending && <Loader2 className="size-4 animate-spin" aria-hidden />}
          {isPending ? "Přidávám do sítě…" : "Přidat"}
        </button>
        <button type="button" onClick={onCancel} disabled={isPending} className="h-10 rounded-lg px-3 text-[13px] text-slate-600 hover:bg-slate-50 disabled:opacity-50">
          Zrušit
        </button>
      </div>
      {isPending && <p className="text-[11.5px] text-slate-500">Gateway páruje čidlo v síti IQMESH, trvá to pár sekund.</p>}
      {error && (
        <p role="alert" className="text-[13px] font-bold text-red-700">
          {error}
        </p>
      )}
    </form>
  );
}
