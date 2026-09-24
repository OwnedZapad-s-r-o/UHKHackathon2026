import type { Metadata } from "next";
import Image from "next/image";
import { redirect } from "next/navigation";
import { getSession } from "@/lib/api";
import { DemoButtons } from "./_components/demo-buttons";
import { LoginForm } from "./_components/login-form";

export const metadata: Metadata = {
  title: "Přihlášení · SmartAudit KHK",
};

export default async function LoginPage() {
  if (await getSession()) redirect("/mapa");

  return (
    <main className="flex flex-1 items-center justify-center bg-muted/40 p-4">
      <div className="w-full max-w-md space-y-6 rounded-xl border bg-background p-6 shadow-sm">
        <div>
          <div className="flex items-center gap-3">
            <Image src="/brand/hk-logo-white.png" alt="Hradec Králové" width={330} height={142} priority className="h-10 w-auto invert" />
            <div className="leading-tight">
              <h1 className="text-2xl font-bold tracking-tight">SmartAudit</h1>
              <p className="text-xs text-muted-foreground">Královéhradecký kraj</p>
            </div>
          </div>
          <p className="mt-2 text-sm text-muted-foreground">
            Energetický audit budov, které kraj zřizuje — školy, domovy, nemocnice a kulturní zařízení.
          </p>
        </div>

        <section aria-labelledby="demo-heading" className="space-y-3">
          <h2 id="demo-heading" className="text-sm font-medium">
            Vyzkoušet bez registrace
          </h2>
          <DemoButtons />
        </section>

        <div className="flex items-center gap-3 text-xs text-muted-foreground">
          <span className="h-px flex-1 bg-border" />
          nebo
          <span className="h-px flex-1 bg-border" />
        </div>

        <LoginForm />

        {/* Otevřené API funguje i bez přihlášení */}
        <p className="text-center text-xs text-muted-foreground">
          Data jako otevřené API:{" "}
          <a href="/api/open/docs/" className="font-medium text-foreground underline underline-offset-2 hover:text-hk-red">
            dokumentace a stažení
          </a>
        </p>
      </div>
    </main>
  );
}
