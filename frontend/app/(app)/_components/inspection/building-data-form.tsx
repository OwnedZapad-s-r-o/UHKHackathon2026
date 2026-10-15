"use client";

import { useActionState } from "react";
import { Check, Loader2 } from "lucide-react";
import { Input } from "@/components/ui/input";
import { updateBuildingData, type BuildingDataState } from "../building-actions";

type BuildingDataFormProps = {
  facilityId: string;
  // Aktuální opravy (prázdné = platí ČÚZK) a údaje z ČÚZK / odhadu jako nápověda
  values: { area: number | null; floors: number | null; year: number | null };
  cadastre: { area: number | null; floors: number; year: number | null };
};

// Oprava údajů, ze kterých se počítá ztráta — když katastr nesedí (chybí křídlo, nástavba, jiný rok výstavby).
// Prázdné pole = údaj z ČÚZK. Uložení hned přepočítá audit.
export function BuildingDataForm({ facilityId, values, cadastre }: BuildingDataFormProps) {
  const [state, formAction, isPending] = useActionState<BuildingDataState, FormData>(updateBuildingData.bind(null, facilityId), null);
  const edited = values.area !== null || values.floors !== null || values.year !== null;

  return (
    <form action={formAction} className="mt-3 space-y-3">
      <div className="grid grid-cols-3 gap-2.5">
        <Field name="area" label="Zastavěná plocha (m²)" value={values.area} placeholder={cadastre.area} />
        <Field name="floors" label="Podlaží" value={values.floors} placeholder={cadastre.floors} />
        <Field name="year" label="Rok výstavby" value={values.year} placeholder={cadastre.year} />
      </div>
      <p className="text-[11px] leading-snug text-slate-500">
        Prázdné pole = údaj z ČÚZK (šedě). Plocha je za celý objekt i s doplněnými budovami. Z roku výstavby se odvodí energetická kategorie.
      </p>

      <div className="flex flex-wrap items-center gap-2">
        <button
          type="submit"
          disabled={isPending}
          className="inline-flex h-9 items-center gap-1.5 rounded-lg bg-hk-navy px-3 text-[13px] font-bold text-white transition-colors hover:bg-hk-blue disabled:opacity-60"
        >
          {isPending ? <Loader2 className="size-4 animate-spin" aria-hidden /> : <Check className="size-4" aria-hidden />}
          Uložit a přepočítat ztrátu
        </button>
        {edited && (
          <button type="submit" formAction={(formData) => formAction(emptied(formData))} disabled={isPending} className="h-9 rounded-lg px-3 text-[13px] font-bold text-slate-600 hover:bg-slate-100">
            Vrátit údaje z ČÚZK
          </button>
        )}
        {state !== null && "saved" in state && !isPending && <span className="text-[12.5px] font-bold text-emerald-700">Uloženo — audit se přepočítá do pár sekund</span>}
      </div>

      {state !== null && "error" in state && (
        <p role="alert" className="text-[13px] font-bold text-red-700">
          {state.error}
        </p>
      )}
    </form>
  );
}

function Field({ name, label, value, placeholder }: { name: string; label: string; value: number | null; placeholder: number | null }) {
  return (
    <div className="space-y-1">
      <label htmlFor={`building-${name}`} className="text-[11px] text-slate-500">
        {label}
      </label>
      <Input
        id={`building-${name}`}
        name={name}
        inputMode="decimal"
        defaultValue={value ?? ""}
        placeholder={placeholder?.toLocaleString("cs-CZ") ?? "—"}
        className="h-9 border-slate-300 bg-white text-[13px]"
      />
    </div>
  );
}

// „Vrátit údaje z ČÚZK“ = odeslat všechna pole prázdná
function emptied(formData: FormData) {
  const empty = new FormData();
  for (const key of formData.keys()) empty.set(key, "");
  return empty;
}
