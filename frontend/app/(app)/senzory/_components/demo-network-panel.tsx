"use client";

import { useActionState } from "react";
import Link from "next/link";
import { Check, Loader2, Map as MapIcon, Zap } from "lucide-react";
import { createDemoNetwork, type DemoNetworkState } from "../actions";

// Testovací síť kraje: pět virtuálních gatewayí v pěti okresech jedním klikem — živá data bez hardwaru.
// Gatewaye se chovají jako skutečné (vlastní účet na brokeru, MQTT, IQRF JSON API), jen hardware nahrazuje aplikace.
export function DemoNetworkPanel({ running }: { running: boolean }) {
  const [state, formAction, isPending] = useActionState<DemoNetworkState, FormData>(() => createDemoNetwork(), null);
  const done = running || (state !== null && "created" in state);

  return (
    <section data-tour="senzory-sit" aria-labelledby="demo-network" className="rounded-2xl border border-hk-blue/20 bg-[#EFF4FA] p-5">
      <div className="flex flex-wrap items-start gap-4">
        <span className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-hk-blue text-white">
          <Zap className="size-5" aria-hidden />
        </span>
        <div className="min-w-0 flex-1">
          <h2 id="demo-network" className="text-[15px] font-bold">
            Testovací síť kraje — 5 budov v 5 okresech
          </h2>
          <p className="mt-0.5 text-[13px] leading-snug text-slate-600">
            Hradec Králové, Jičín, Trutnov, Náchod a Rychnov nad Kněžnou. Virtuální gatewaye posílají živá data stejnou cestou jako hardware.
            Čtyři jsou už v místnostech, rychnovská čeká na vaše přiřazení.
          </p>
          {state !== null && "error" in state && (
            <p role="alert" className="mt-2 text-[13px] text-red-800">
              {state.error}
            </p>
          )}
        </div>

        {done ? (
          <div className="flex flex-wrap items-center gap-2">
            <span className="inline-flex h-10 items-center gap-1.5 rounded-lg bg-emerald-50 px-3 text-[13px] font-bold text-emerald-800">
              <Check className="size-4" aria-hidden />
              {state !== null && "created" in state && state.created > 0 ? `Spuštěno ${state.created} gatewayí` : "Síť běží"}
            </span>
            <Link
              href="/mapa"
              className="inline-flex h-10 items-center gap-1.5 rounded-lg border border-slate-300 bg-white px-3 text-[13px] font-bold text-hk-navy transition-colors hover:bg-slate-50"
            >
              <MapIcon className="size-4" aria-hidden />
              Ukázat v mapě
            </Link>
          </div>
        ) : (
          <form action={formAction}>
            <button
              type="submit"
              disabled={isPending}
              className="inline-flex h-10 items-center gap-2 rounded-lg bg-hk-blue px-4 text-[13px] font-bold text-white transition-colors hover:bg-hk-navy disabled:opacity-60"
            >
              {isPending ? <Loader2 className="size-4 animate-spin" aria-hidden /> : <Zap className="size-4" aria-hidden />}
              {isPending ? "Spouštím…" : "Spustit testovací síť"}
            </button>
          </form>
        )}
      </div>
    </section>
  );
}
