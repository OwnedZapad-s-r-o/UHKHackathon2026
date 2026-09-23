import { redirect } from "next/navigation";
import { getFacilities, getSession } from "@/lib/api";
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
      <Sidebar session={session} counts={counts} />
      <div className="flex min-w-0 flex-1 flex-col">{children}</div>
    </div>
  );
}
