"use client";

import { useActionState } from "react";
import { Loader2 } from "lucide-react";
import { Input } from "@/components/ui/input";
import { login } from "../actions";

export function LoginForm() {
  const [state, formAction, isPending] = useActionState(login, null);

  return (
    <form action={formAction} className="space-y-3.5">
      <div className="space-y-1.5">
        <label htmlFor="email" className="text-[13px] font-bold">
          E-mail
        </label>
        <Input id="email" name="email" type="email" autoComplete="username" required placeholder="vas@email.cz" className="h-11 border-slate-300 bg-white" />
      </div>
      <div className="space-y-1.5">
        <label htmlFor="password" className="text-[13px] font-bold">
          Heslo
        </label>
        <Input id="password" name="password" type="password" autoComplete="current-password" required className="h-11 border-slate-300 bg-white" />
      </div>
      {state?.error && (
        <p role="alert" className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-[13px] text-red-800">
          {state.error}
        </p>
      )}
      <button
        type="submit"
        disabled={isPending}
        className="inline-flex h-12 w-full items-center justify-center gap-2 rounded-xl bg-hk-navy text-[14px] font-bold text-white transition-colors hover:bg-hk-blue disabled:opacity-60"
      >
        {isPending && <Loader2 className="size-4 animate-spin" aria-hidden />}
        {isPending ? "Přihlašuji…" : "Přihlásit se"}
      </button>
    </form>
  );
}
