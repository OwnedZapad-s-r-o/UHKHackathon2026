"use client";

import { useActionState } from "react";
import { Button } from "@/components/ui/button";
import { login } from "../actions";

export function LoginForm() {
  const [state, formAction, isPending] = useActionState(login, null);

  return (
    <form action={formAction} className="space-y-3">
      <div className="space-y-1">
        <label htmlFor="email" className="text-sm font-medium">
          E-mail
        </label>
        <input id="email" name="email" type="email" autoComplete="username" required className="h-9 w-full rounded-md border bg-background px-3 text-sm" />
      </div>
      <div className="space-y-1">
        <label htmlFor="password" className="text-sm font-medium">
          Heslo
        </label>
        <input id="password" name="password" type="password" autoComplete="current-password" required className="h-9 w-full rounded-md border bg-background px-3 text-sm" />
      </div>
      {state?.error && (
        <p role="alert" className="text-sm text-destructive">
          {state.error}
        </p>
      )}
      <Button type="submit" disabled={isPending} className="w-full">
        {isPending ? "Přihlašuji…" : "Přihlásit se"}
      </Button>
    </form>
  );
}
