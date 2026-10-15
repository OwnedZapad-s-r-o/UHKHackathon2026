"use client";

import { useState, useTransition } from "react";
import Link from "next/link";
import { ArrowRight, Loader2, Plus, Thermometer } from "lucide-react";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { cn } from "@/lib/utils";
import type { DemoBehavior, DemoOverview } from "@/types/api";
import { DeviceSticker } from "../../senzory/_components/pairing-parts";
import { createDemoNode, type DemoCreated } from "../actions";
import { BehaviorPicker, TemplateLabel, type DemoTemplate } from "./demo-form-parts";

type CreateNodeFormProps = {
  templates: DemoTemplate[];
  gateways: DemoOverview["gateways"];
  onCreated: () => void;
};

// Nové čidlo v dosahu virtuální gatewaye → nálepka s MID a kódem pro IQRF SmartConnect
export function CreateNodeForm({ templates, gateways, onCreated }: CreateNodeFormProps) {
  const [gatewayId, setGatewayId] = useState<string | null>(gateways[0]?.id ?? null);
  const [template, setTemplate] = useState("ucebna");
  const [behavior, setBehavior] = useState<DemoBehavior>("Setback");
  const [created, setCreated] = useState<(DemoCreated & { gatewayId: string }) | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  // Vybraná gateway mohla mezitím zmizet (odebraná) — pak nabídnout první dostupnou
  const selectedGateway = gateways.find((g) => g.id === gatewayId) ?? gateways[0] ?? null;
  const options = gateways.map((g) => ({ value: g.id, label: `${g.name} · ${g.facilityName}` }));

  function submit(event: React.FormEvent) {
    event.preventDefault();
    if (!selectedGateway) return;
    setError(null);
    startTransition(async () => {
      const result = await createDemoNode(selectedGateway.id, template, behavior);
      if ("error" in result) {
        setError(result.error);
        return;
      }
      setCreated({ ...result.created, gatewayId: selectedGateway.id });
      onCreated();
    });
  }

  return (
    <section aria-labelledby="demo-node" className="rounded-2xl border border-slate-200 bg-white p-5">
      <h2 id="demo-node" className="flex items-center gap-2 text-[15px] font-bold">
        <Thermometer className="size-4.5 text-hk-blue" aria-hidden />
        Položit čidlo k gatewayi
      </h2>
      <p className="mt-1 text-[13px] text-slate-600">Čidlo se objeví v dosahu gatewaye. Do sítě ho přidáte kódem z nálepky.</p>

      {gateways.length === 0 ? (
        <p className="mt-3 rounded-lg bg-slate-50 p-3 text-[13px] text-slate-600">
          Zatím tu není žádná virtuální gateway. Vyrobte a spárujte gateway výše, nebo spusťte testovací síť na stránce{" "}
          <Link href="/senzory" className="font-bold text-hk-blue hover:underline">
            Senzory
          </Link>
          .
        </p>
      ) : (
        <form onSubmit={submit} className="mt-3 space-y-3">
          <div className="space-y-1">
            <label htmlFor="demo-node-gateway" className="text-xs text-slate-500">
              K jaké gatewayi
            </label>
            <Select items={options} value={selectedGateway?.id ?? null} onValueChange={(next) => setGatewayId(next as string | null)}>
              <SelectTrigger id="demo-node-gateway" className="h-10 w-full border-slate-300 bg-white text-[13px]">
                <SelectValue className="min-w-0 truncate" />
              </SelectTrigger>
              <SelectContent
                alignItemWithTrigger={false}
                align="start"
                sideOffset={8}
                className="max-h-[min(20rem,var(--available-height))] w-(--anchor-width) p-1.5 text-[13px]"
              >
                {options.map((option) => (
                  <SelectItem
                    key={option.value}
                    value={option.value}
                    className="py-2.5 pl-3 text-[13px] leading-5 [&>div]:min-w-0 [&>div]:shrink [&>div]:whitespace-normal"
                  >
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <fieldset>
            <legend className="text-xs text-slate-500">Druh čidla</legend>
            <div className="mt-1 grid grid-cols-2 gap-2">
              {templates.map((item) => (
                <label
                  key={item.id}
                  className={cn(
                    "cursor-pointer rounded-lg border p-2.5 transition-colors has-focus-visible:ring-2 has-focus-visible:ring-hk-blue/40",
                    template === item.id ? "border-hk-blue bg-[#EFF4FA]" : "border-slate-200 hover:bg-slate-50",
                  )}
                >
                  <input type="radio" name="demo-node-template" checked={template === item.id} onChange={() => setTemplate(item.id)} className="sr-only" />
                  <TemplateLabel template={item} />
                </label>
              ))}
            </div>
          </fieldset>

          <BehaviorPicker name="node-behavior" value={behavior} onChange={setBehavior} />

          <button
            type="submit"
            disabled={isPending}
            className="inline-flex h-10 w-full items-center justify-center gap-2 rounded-lg border border-hk-blue bg-white px-4 text-[13px] font-bold text-hk-blue transition-colors hover:bg-[#EFF4FA] disabled:opacity-50"
          >
            {isPending ? <Loader2 className="size-4 animate-spin" aria-hidden /> : <Plus className="size-4" aria-hidden />}
            {isPending ? "Pokládám…" : "Položit čidlo"}
          </button>
          {error && (
            <p role="alert" className="text-[13px] font-bold text-red-700">
              {error}
            </p>
          )}
        </form>
      )}

      {created && (
        <div role="status" className="mt-4 space-y-2 border-t border-slate-100 pt-4">
          <DeviceSticker
            title="IQRF čidlo"
            rows={[
              { label: "MID", value: created.serial },
              { label: "Kód", value: created.code, secret: true },
            ]}
          />
          <Link href={`/senzory/${created.gatewayId}`} className="inline-flex items-center gap-1 text-[13px] font-bold text-hk-blue hover:underline">
            Přidat do sítě na detailu gatewaye
            <ArrowRight className="size-3.5" aria-hidden />
          </Link>
        </div>
      )}
    </section>
  );
}
