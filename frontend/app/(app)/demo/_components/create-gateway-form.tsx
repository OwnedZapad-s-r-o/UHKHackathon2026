"use client";

import { useState, useTransition } from "react";
import Link from "next/link";
import { ArrowRight, Check, Factory, Loader2 } from "lucide-react";
import { cn } from "@/lib/utils";
import type { DemoBehavior } from "@/types/api";
import { DeviceSticker } from "../../senzory/_components/pairing-parts";
import { createDemoGateway, type DemoCreated } from "../actions";
import { BehaviorPicker, TemplateLabel, type DemoTemplate } from "./demo-form-parts";

const MAX_NODES = 3;

type CreateGatewayFormProps = {
  templates: DemoTemplate[];
  onCreated: () => void;
};

// Nová gateway „z výroby“: vybrané uzly, chování teploměrů → nálepka s MID a heslem
export function CreateGatewayForm({ templates, onCreated }: CreateGatewayFormProps) {
  const [selected, setSelected] = useState<string[]>(["ucebna"]);
  const [behavior, setBehavior] = useState<DemoBehavior>("Setback");
  const [created, setCreated] = useState<DemoCreated | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  function toggle(id: string) {
    setSelected((current) => (current.includes(id) ? current.filter((t) => t !== id) : current.length < MAX_NODES ? [...current, id] : current));
  }

  function submit(event: React.FormEvent) {
    event.preventDefault();
    setError(null);
    startTransition(async () => {
      const result = await createDemoGateway(selected, behavior);
      if ("error" in result) {
        setError(result.error);
        return;
      }
      setCreated(result.created);
      onCreated();
    });
  }

  return (
    <section aria-labelledby="demo-gateway" className="rounded-2xl border-2 border-hk-red/20 bg-white p-5">
      <h2 id="demo-gateway" className="flex items-center gap-2 text-[15px] font-bold">
        <Factory className="size-4.5 text-hk-blue" aria-hidden />
        Vyrobit gateway
      </h2>
      <p className="mt-1 text-[13px] text-slate-600">Zapne se a začne se ohlašovat jako nová krabička. Připojíte ji heslem z nálepky.</p>

      <form onSubmit={submit} className="mt-3 space-y-3">
        <fieldset>
          <legend className="text-xs text-slate-500">
            Uzly gatewaye (max. {MAX_NODES}) — každý uzel = jedna místnost
          </legend>
          <div className="mt-1 grid grid-cols-2 gap-2">
            {templates.map((template) => {
              const checked = selected.includes(template.id);
              return (
                <label
                  key={template.id}
                  className={cn(
                    "relative cursor-pointer rounded-lg border p-2.5 transition-colors has-focus-visible:ring-2 has-focus-visible:ring-hk-blue/40",
                    checked ? "border-hk-blue bg-[#EFF4FA]" : "border-slate-200 hover:bg-slate-50",
                  )}
                >
                  <input type="checkbox" checked={checked} onChange={() => toggle(template.id)} className="sr-only" />
                  {checked && <Check className="absolute top-2 right-2 size-3.5 text-hk-blue" aria-hidden />}
                  <TemplateLabel template={template} />
                </label>
              );
            })}
          </div>
        </fieldset>

        <BehaviorPicker name="gateway-behavior" value={behavior} onChange={setBehavior} />

        <button
          type="submit"
          disabled={isPending || selected.length === 0}
          className="inline-flex h-10 w-full items-center justify-center gap-2 rounded-lg bg-hk-red px-4 text-[13px] font-bold text-white transition-colors hover:bg-hk-red-dark disabled:opacity-50"
        >
          {isPending ? <Loader2 className="size-4 animate-spin" aria-hidden /> : <Factory className="size-4" aria-hidden />}
          {isPending ? "Vyrábím…" : "Vyrobit a zapnout"}
        </button>
        {error && (
          <p role="alert" className="text-[13px] font-bold text-red-700">
            {error}
          </p>
        )}
      </form>

      {created && (
        <div role="status" className="mt-4 space-y-2 border-t border-slate-100 pt-4">
          <DeviceSticker
            title="IQRF gateway IQD-GW04"
            rows={[
              { label: "MID", value: created.serial },
              { label: "Heslo", value: created.code, secret: true },
            ]}
          />
          <p className="text-[12.5px] text-slate-600">Do 10 s se ohlásí na stránce Senzory v kartě Nová zařízení online.</p>
          <Link href="/senzory" className="inline-flex items-center gap-1 text-[13px] font-bold text-hk-blue hover:underline">
            Spárovat na stránce Senzory
            <ArrowRight className="size-3.5" aria-hidden />
          </Link>
        </div>
      )}
    </section>
  );
}
