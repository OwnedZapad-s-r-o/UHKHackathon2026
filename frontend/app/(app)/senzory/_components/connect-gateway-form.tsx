"use client";

import { useActionState, useState } from "react";
import { PlugZap } from "lucide-react";
import { Combobox, ComboboxContent, ComboboxEmpty, ComboboxInput, ComboboxItem, ComboboxList } from "@/components/ui/combobox";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { createGateway } from "../actions";

type FacilityOption = { value: string; label: string; district: string };

const POLL_OPTIONS = [
  { value: "5", label: "každých 5 s (demo)" },
  { value: "15", label: "každých 15 s" },
  { value: "60", label: "každou minutu" },
  { value: "300", label: "každých 5 minut" },
];

type ConnectGatewayFormProps = {
  facilities: FacilityOption[];
  // Id gatewaye, která se už ozývá na brokeru — průvodce ho předvyplní
  suggestedGatewayId?: string;
};

export function ConnectGatewayForm({ facilities, suggestedGatewayId }: ConnectGatewayFormProps) {
  const [state, formAction, isPending] = useActionState(createGateway, null);
  const [facility, setFacility] = useState<FacilityOption | null>(null);

  return (
    <form action={formAction} className="mt-4 space-y-3.5">
      <Field label="Objekt" htmlFor="gateway-facility">
        <input type="hidden" name="facilityId" value={facility?.value ?? ""} />
        <Combobox items={facilities} value={facility} onValueChange={(next) => setFacility(next as FacilityOption | null)}>
          <ComboboxInput id="gateway-facility" placeholder="Hledat objekt nebo kód…" className="h-10 w-full border-slate-300 bg-white" />
          <ComboboxContent>
            <ComboboxEmpty>Žádný objekt neodpovídá hledání.</ComboboxEmpty>
            <ComboboxList>
              {(item: FacilityOption) => (
                <ComboboxItem key={item.value} value={item}>
                  <span className="min-w-0 truncate">{item.label}</span>
                  <span className="ml-auto shrink-0 text-xs text-slate-400">{item.district}</span>
                </ComboboxItem>
              )}
            </ComboboxList>
          </ComboboxContent>
        </Combobox>
      </Field>

      <div className="grid gap-3.5">
        <Field label="Název" htmlFor="gateway-name">
          <Input id="gateway-name" name="name" placeholder="např. Gateway 1. patro" className="h-10 border-slate-300 bg-white" required />
        </Field>
        <Field label="Id gatewaye (MQTT)" htmlFor="gateway-id" hint="část topicu gateway/{id}/iqrf">
          <Input id="gateway-id" name="gatewayId" defaultValue={suggestedGatewayId} placeholder="např. 0200-ab12" className="h-10 border-slate-300 bg-white font-mono" required />
        </Field>
      </div>

      <div className="grid gap-3.5">
        <Field label="Dotazovat senzory" htmlFor="gateway-poll">
          <Select items={POLL_OPTIONS} name="pollSeconds" defaultValue="15">
            <SelectTrigger id="gateway-poll" className="h-10 w-full border-slate-300 bg-white text-sm">
              <SelectValue />
            </SelectTrigger>
            <SelectContent alignItemWithTrigger={false} className="min-w-(--anchor-width)">
              {POLL_OPTIONS.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </Field>
        <Field label="Adresy uzlů" htmlFor="gateway-nodes" hint="prázdné = zjistí se z koordinátoru">
          <Input id="gateway-nodes" name="nodes" placeholder="např. 1, 2, 5" className="h-10 border-slate-300 bg-white font-mono" />
        </Field>
      </div>

      {state?.error && (
        <p role="alert" className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-[13px] text-red-800">
          {state.error}
        </p>
      )}

      <button
        type="submit"
        disabled={isPending}
        className="inline-flex h-11 w-full items-center justify-center gap-2 rounded-lg bg-hk-accent px-4 text-sm font-bold text-white transition-colors hover:bg-hk-red disabled:opacity-60"
      >
        <PlugZap className="size-4" aria-hidden />
        {isPending ? "Připojuji…" : "Připojit gateway"}
      </button>
    </form>
  );
}

function Field({ label, htmlFor, hint, children }: { label: string; htmlFor: string; hint?: string; children: React.ReactNode }) {
  return (
    <div className="space-y-1">
      <label htmlFor={htmlFor} className="flex items-baseline justify-between gap-2 text-xs text-slate-500">
        {label}
        {hint && <span className="text-[10.5px] text-slate-400">{hint}</span>}
      </label>
      {children}
    </div>
  );
}
