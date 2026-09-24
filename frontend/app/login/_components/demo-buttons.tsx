"use client";

import { useActionState, useState } from "react";
import { ArrowRight, Building2, Landmark, Loader2 } from "lucide-react";
import { cn } from "@/lib/utils";
import { demoLogin } from "../actions";

const PERSONAS = [
  {
    role: "Kraj",
    title: "Jsem z kraje",
    description: "Všechny budovy kraje, ztráty v Kč a pořadí investic",
    icon: Landmark,
    tile: "bg-red-50 text-hk-red",
  },
  {
    role: "Spravce",
    title: "Spravuji budovu",
    description: "Moje škola nebo domov — co měření odhalilo a jak připojit čidla",
    icon: Building2,
    tile: "bg-[#EFF4FA] text-hk-blue",
  },
] as const;

// Demo přihlášení podle role. publicOnly = nenápadný odkaz „vyzkoušet aplikaci jako veřejnost“ v kartě pro veřejnost.
export function DemoButtons({ publicOnly = false }: { publicOnly?: boolean }) {
  const [state, formAction, isPending] = useActionState(demoLogin, null);
  // Která volba se právě přihlašuje — točící kolečko jen u ní, ostatní se zašedí
  const [chosen, setChosen] = useState<string | null>(null);

  if (publicOnly) {
    return (
      <form action={formAction} className="mt-2 text-center">
        <button
          type="submit"
          name="role"
          value="Verejnost"
          disabled={isPending}
          className="inline-flex items-center gap-1.5 text-[12.5px] text-amber-900 underline-offset-2 hover:underline disabled:opacity-60"
        >
          {isPending && <Loader2 className="size-3.5 animate-spin" aria-hidden />}
          nebo vyzkoušet celou aplikaci jako veřejnost
        </button>
        {state?.error && (
          <p role="alert" className="mt-1 text-[12.5px] text-red-700">
            {state.error}
          </p>
        )}
      </form>
    );
  }

  return (
    <form action={formAction} className="mt-6 space-y-3">
      {PERSONAS.map(({ role, title, description, icon: Icon, tile }) => {
        const loading = isPending && chosen === role;

        return (
          <button
            key={role}
            type="submit"
            name="role"
            value={role}
            disabled={isPending}
            onClick={() => setChosen(role)}
            className={cn(
              "group flex w-full items-center gap-4 rounded-2xl border border-slate-200 bg-white p-4 text-left transition-all hover:border-hk-blue/40 hover:shadow-[0_12px_28px_-12px_rgba(11,25,44,0.25)] focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-hk-blue",
              isPending && !loading && "opacity-50",
            )}
          >
            <span className={cn("flex size-11 shrink-0 items-center justify-center rounded-xl", tile)}>
              <Icon className="size-5" aria-hidden />
            </span>
            <span className="min-w-0 flex-1">
              <span className="block text-[15px] font-bold text-hk-navy">{title}</span>
              <span className="mt-0.5 block text-[13px] leading-snug text-slate-600">{description}</span>
            </span>
            {loading ? (
              <Loader2 className="size-4.5 shrink-0 animate-spin text-hk-red" aria-label="Přihlašuji" />
            ) : (
              <ArrowRight className="size-4.5 shrink-0 text-slate-300 transition-all group-hover:translate-x-0.5 group-hover:text-hk-red" aria-hidden />
            )}
          </button>
        );
      })}
      {state?.error && (
        <p role="alert" className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-[13px] text-red-800">
          {state.error}
        </p>
      )}
    </form>
  );
}
