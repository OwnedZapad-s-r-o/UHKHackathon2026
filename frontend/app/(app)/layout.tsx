import { redirect } from "next/navigation";
import { getFacilities, getSession } from "@/lib/api";
import { MobileNav } from "./_components/mobile-nav";
import { Sidebar } from "./_components/sidebar";

// Rozvržení podle prototypu: černý postranní panel vlevo, obsah obrazovky vpravo
export default async function AppLayout({ children }: LayoutProps<"/">) {
  const session = await getSession();
  if (!session) redirect("/login");

  const facilities = await getFacilities();
  const counts = {
    portfolio: facilities.length,
    anomalies: facilities.filter((f) => f.status !== "Nominal").length,
  };

  return (
    <div className="flex min-h-0 flex-1 bg-slate-50 text-hk-navy">
      {/* Od šířky lg stálý panel, na mobilu lišta s vysouvacím menu (stejný obsah) */}
      <Sidebar session={session} counts={counts} className="hidden lg:flex" />
      <div className="flex min-w-0 flex-1 flex-col">
        <MobileNav>
          <Sidebar session={session} counts={counts} className="h-full" />
        </MobileNav>
        {children}
      </div>
    </div>
  );
}
