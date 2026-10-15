import type { Metadata } from "next";
import { redirect } from "next/navigation";
import { getDemoOverview, getSession } from "@/lib/api";
import { TopBar } from "../_components/top-bar";
import { DemoWorkspace } from "./_components/demo-workspace";

export const metadata: Metadata = {
  title: "Demo správa · SmartAudit KHK",
  description: "Výroba virtuálních gatewayí a čidel, živé párování a oprava odhadu ztráty měřením.",
};

export default async function DemoPage() {
  const session = await getSession();
  // Veřejnost zařízení nepáruje a ztráty v Kč nevidí
  if (session?.role === "Verejnost") redirect("/mapa");

  const overview = await getDemoOverview();

  return (
    <>
      <TopBar title="Demo správa" />

      <main className="min-h-0 flex-1 overflow-y-auto p-6">
        <h1 className="text-[32px] leading-tight font-bold tracking-tight">Demo správa</h1>
        <p className="mt-1.5 max-w-3xl text-sm leading-normal text-slate-600">
          Vyrobte virtuální gateway nebo čidlo a projděte s ním párování jako se skutečným zařízením — heslo opíšete z nálepky. Až čidlo změří
          místnost, uvidíte, jak se opravil odhad ztráty. Zařízení hraje aplikace; v ukázce to řekněte nahlas.
        </p>

        <DemoWorkspace initial={overview} />
      </main>
    </>
  );
}
