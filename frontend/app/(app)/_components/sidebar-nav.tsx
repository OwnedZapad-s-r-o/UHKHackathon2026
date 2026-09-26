"use client";

import Link from "next/link";
import { usePathname, useSearchParams } from "next/navigation";
import { AlertTriangle, Braces, Building2, Database, FileDown, FlaskConical, MapPin, RadioTower, type LucideIcon } from "lucide-react";
import { cn } from "@/lib/utils";
import type { Role } from "@/types/api";

type SidebarNavProps = {
  counts: { portfolio: number; anomalies: number };
  role: Role;
};

// Položky jen pro role, které s nimi mohou něco dělat — veřejnost čidla nepřipojuje, slepá ulička by jen mátla
const MANAGEMENT_ONLY = new Set(["/senzory", "/demo"]);

export function SidebarNav({ counts, role }: SidebarNavProps) {
  const pathname = usePathname();
  const searchParams = useSearchParams();

  // Filtr „jen se zjištěním“ se nese mezi obrazovkami
  const issues = searchParams.get("zjisteni") === "1" ? "?zjisteni=1" : "";

  // Správce má jen svoje budovy — „portfolio“ a „anomálie“ jsou slova kraje
  const own = role === "Spravce";

  const items: { href: string; label: string; icon: LucideIcon; badge?: number }[] = [
    { href: "/mapa", label: "Mapa", icon: MapPin },
    { href: "/portfolio", label: own ? "Moje budovy" : "Portfolio", icon: Building2, badge: counts.portfolio },
    { href: "/anomalie", label: own ? "Zjištění" : "Anomálie", icon: AlertTriangle, badge: counts.anomalies },
    { href: "/reporty", label: "Reporty", icon: FileDown },
    { href: "/senzory", label: "Senzory", icon: RadioTower },
    { href: "/demo", label: "Demo správa", icon: FlaskConical },
    { href: "/zdroje-dat", label: "Zdroje dat", icon: Database },
    { href: "/otevrena-data", label: "Otevřené API", icon: Braces },
  ].filter((item) => role !== "Verejnost" || !MANAGEMENT_ONLY.has(item.href));

  return (
    <nav className="mt-8 flex flex-col gap-0.5" aria-label="Hlavní navigace">
      {items.map(({ href, label, icon: Icon, badge }) => {
        const active = pathname.startsWith(href);

        return (
          <Link
            key={href}
            href={`${href}${["/zdroje-dat", "/reporty", "/otevrena-data", "/senzory", "/demo"].includes(href) ? "" : issues}`}
            aria-current={active ? "page" : undefined}
            data-tour={`nav-${href.slice(1)}`}
            className={cn(
              "flex h-10.5 items-center gap-3 rounded-lg px-3 text-sm transition-colors hover:text-white",
              active ? "bg-white/8 font-bold text-white shadow-[inset_0_-3px_0_0_var(--color-hk-red)]" : "text-white/58",
            )}
          >
            <Icon className="size-4.5" strokeWidth={1.75} aria-hidden />
            <span className="flex-1">{label}</span>
            {badge !== undefined && <span className={cn("text-xs tabular-nums", active ? "text-white/70" : "text-white/40")}>{badge}</span>}
          </Link>
        );
      })}
    </nav>
  );
}
