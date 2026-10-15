"use client";

import { useState, useTransition } from "react";
import { KeyRound, Loader2 } from "lucide-react";
import { Combobox, ComboboxContent, ComboboxEmpty, ComboboxInput, ComboboxItem, ComboboxList } from "@/components/ui/combobox";
import { Input } from "@/components/ui/input";
import { claimGateway } from "../actions";
import { CodeInput, codeIsComplete } from "./pairing-parts";

export type FacilityOption = { value: string; label: string; district: string };

type PairDeviceFormProps = {
  serial: string;
  facilities: FacilityOption[];
  onCancel: () => void;
};

// Párování gatewaye jako u Wi-Fi: kam patří → heslo z nálepky → připojit. Po úspěchu server přesměruje na detail gatewaye,
// kde se čeká na první data a přiřazují se uzly do místností.
export function PairDeviceForm({ serial, facilities, onCancel }: PairDeviceFormProps) {
  // Správce s jedinou budovou ji nemusí vybírat
  const [facility, setFacility] = useState<FacilityOption | null>(facilities.length === 1 ? facilities[0] : null);
  const [name, setName] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  const facilityName = facility?.label.split(" · ")[1];
  const canSubmit = facility !== null && codeIsComplete(password) && !isPending;

  function submit(event: React.FormEvent) {
    event.preventDefault();
    if (!facility) return;
    setError(null);
    startTransition(async () => {
      const result = await claimGateway(serial, password, facility.value, name);
      if (result?.error) setError(result.error);
    });
  }

  return (
    <form onSubmit={submit} className="mt-3 grid gap-3 rounded-xl border border-slate-200 bg-slate-50 p-4 md:grid-cols-2">
      <div className="space-y-1">
        <label htmlFor={`pair-facility-${serial}`} className="text-xs text-slate-500">
          1. Kam gateway patří
        </label>
        <Combobox items={facilities} value={facility} onValueChange={(next) => setFacility(next as FacilityOption | null)}>
          <ComboboxInput id={`pair-facility-${serial}`} placeholder="Hledat objekt nebo kód…" className="h-10 w-full border-slate-300 bg-white" />
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
      </div>

      <div className="space-y-1">
        <label htmlFor={`pair-name-${serial}`} className="text-xs text-slate-500">
          Název (nepovinné)
        </label>
        <Input
          id={`pair-name-${serial}`}
          value={name}
          onChange={(event) => setName(event.target.value)}
          maxLength={100}
          placeholder={facilityName ? `Gateway – ${facilityName}` : "např. Gateway 1. patro"}
          className="h-10 border-slate-300 bg-white"
        />
      </div>

      <div className="space-y-1 md:col-span-2">
        <label htmlFor={`pair-password-${serial}`} className="text-xs text-slate-500">
          2. Heslo z nálepky
        </label>
        <div className="flex flex-wrap items-center gap-2">
          <CodeInput
            id={`pair-password-${serial}`}
            value={password}
            onValueChange={setPassword}
            aria-describedby={`pair-hint-${serial}`}
            aria-invalid={error !== null}
            className="w-44"
            autoFocus
          />
          <button
            type="submit"
            disabled={!canSubmit}
            className="inline-flex h-10 items-center gap-2 rounded-lg bg-hk-red px-4 text-[13px] font-bold text-white transition-colors hover:bg-hk-red-dark disabled:opacity-50"
          >
            {isPending ? <Loader2 className="size-4 animate-spin" aria-hidden /> : <KeyRound className="size-4" aria-hidden />}
            {isPending ? "Připojuji…" : "Připojit"}
          </button>
          <button type="button" onClick={onCancel} disabled={isPending} className="h-10 rounded-lg px-3 text-[13px] text-slate-600 hover:bg-white disabled:opacity-50">
            Zrušit
          </button>
        </div>
        <p id={`pair-hint-${serial}`} className="text-[11.5px] text-slate-500">
          {isPending
            ? "Ověřuji heslo a předávám zařízení přístup k brokeru…"
            : "Najdete ho na nálepce na spodní straně zařízení vedle sériového čísla. Velká písmena a pomlčku doplníme sami."}
        </p>
        {error && (
          <p role="alert" className="text-[13px] font-bold text-red-700">
            {error}
          </p>
        )}
      </div>
    </form>
  );
}
