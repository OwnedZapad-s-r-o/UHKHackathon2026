import { Suspense } from "react";
import { redirect } from "next/navigation";
import { getFacilities, getSession } from "@/lib/api";
import type { AuditStatus, FacilityListItem } from "@/types/api";
import { MobileNav } from "./_components/mobile-nav";
import { Sidebar } from "./_components/sidebar";
import { AppTour } from "./_components/tour/app-tour";

const SEVERITY: Record<AuditStatus, number> = { Critical: 0, Warning: 1, Nominal: 2 };

// Rozvržení podle prototypu: tmavě modrý postranní panel vlevo, obsah obrazovky vpravo, nad tím průvodce aplikací
export default async function AppLayout({ children }: LayoutProps<"/">) {
  const session = await getSession();
  if (!session) redirect("/login");
  // Aplikace je pracovní nástroj kraje a správců — veřejnost má svůj portál (kvalita vzduchu, bez žebříčků a Kč)
  if (session.role === "Verejnost") redirect("/objekty");

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

      {/* useSearchParams v průvodci potřebuje hranici Suspense, jinak by celé rozvržení čekalo na klienta */}
      <Suspense>
        <AppTour role={session.role} facility={tourFacility(facilities)} />
      </Suspense>
    </div>
  );
}

// Budova pro poslední krok průvodce: nejvážnější stav, v něm nejvyšší ztráta
function tourFacility(facilities: FacilityListItem[]) {
  const pick = facilities.toSorted((a, b) => SEVERITY[a.status] - SEVERITY[b.status] || (b.annualLossCzk ?? 0) - (a.annualLossCzk ?? 0))[0];
  return pick ? { id: pick.id, districtCode: pick.districtCode } : null;
}
