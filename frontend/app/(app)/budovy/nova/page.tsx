import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { ArrowLeft } from "lucide-react";
import { getFacilities, getSession } from "@/lib/api";
import type { FacilityCategory } from "@/types/api";
import { TopBar } from "../../_components/top-bar";
import { AddBuildingForm } from "./_components/add-building-form";

export const metadata: Metadata = {
  title: "Přidat budovu · SmartAudit KHK",
};

// Onboarding správce budovy: vlastní budovu přidá do mapy sám. Kraj má portfolio z otevřených dat, přidávání je jen pro správce.
export default async function NewBuildingPage() {
  const session = await getSession();
  if (session?.role !== "Spravce") redirect("/mapa");

  const facilities = await getFacilities();

  return (
    <>
      <TopBar title="Přidat budovu">
        <Link href="/mapa" className="inline-flex h-9 items-center gap-1.5 rounded-lg px-2.5 text-[13px] font-bold text-slate-600 transition-colors hover:bg-slate-100 hover:text-hk-navy">
          <ArrowLeft className="size-4" aria-hidden />
          Zpět do mapy
        </Link>
      </TopBar>

      <AddBuildingForm organizationName={session.organizationName} defaultCategory={mostCommonCategory(facilities.map((f) => f.category))} ownFacilities={facilities} />
    </>
  );
}

// Nová budova bývá stejného druhu jako ostatní budovy organizace (škola přidává tělocvičnu, ne nemocnici)
function mostCommonCategory(categories: FacilityCategory[]): FacilityCategory {
  const counts = new Map<FacilityCategory, number>();
  for (const category of categories) counts.set(category, (counts.get(category) ?? 0) + 1);

  return [...counts.entries()].toSorted((a, b) => b[1] - a[1])[0]?.[0] ?? "School";
}
