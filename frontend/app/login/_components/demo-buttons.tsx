"use client";

import { useActionState } from "react";
import { Building2, Landmark, Users } from "lucide-react";
import { demoLogin } from "../actions";

const PERSONAS = [
  { role: "Kraj", title: "Jsem z kraje", description: "Celé portfolio, ztráty v Kč, pořadí investic", icon: Landmark },
  { role: "Spravce", title: "Spravuji budovu", description: "Moje škola — anomálie a co s nimi", icon: Building2 },
  { role: "Verejnost", title: "Jsem z veřejnosti", description: "Jak se dýchá ve třídách, bez peněz", icon: Users },
] as const;

export function DemoButtons() {
  const [state, formAction, isPending] = useActionState(demoLogin, null);

  return (
    <form action={formAction} className="space-y-2">
      {PERSONAS.map(({ role, title, description, icon: Icon }) => (
        <button
          key={role}
          type="submit"
          name="role"
          value={role}
          disabled={isPending}
          className="flex w-full items-center gap-3 rounded-lg border p-3 text-left transition-colors hover:bg-muted disabled:opacity-50"
        >
          <Icon className="size-5 shrink-0 text-muted-foreground" aria-hidden />
          <span>
            <span className="block text-sm font-medium">{title}</span>
            <span className="block text-xs text-muted-foreground">{description}</span>
          </span>
        </button>
      ))}
      {state?.error && (
        <p role="alert" className="text-sm text-destructive">
          {state.error}
        </p>
      )}
    </form>
  );
}
