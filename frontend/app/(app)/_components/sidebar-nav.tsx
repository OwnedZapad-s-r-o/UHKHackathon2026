"use client";

import Link from "next/link";
import { usePathname, useSearchParams } from "next/navigation";
import { AlertTriangle, Braces, Building2, Database, FileDown, MapPin, RadioTower, type LucideIcon } from "lucide-react";
import { cn } from "@/lib/utils";

type SidebarNavProps = {
  counts: { portfolio: number; anomalies: number };
};

export function SidebarNav({ counts }: SidebarNavProps) {
  const pathname = usePathname();
  const searchParams = useSearchParams();

  // Filtr „jen se zjištěním“ se nese mezi obrazovkami
  const issues = searchParams.get("zjisteni") === "1" ? "?zjisteni=1" : "";

  const items: { href: string; label: string; icon: LucideIcon; badge?: number }[] = [
    { href: "/", label: "Mapa", icon: MapPin },
    { href: "/portfolio", label: "Portfolio", icon: Building2, badge: counts.portfolio },
    { href: "/anomalie", label: "Anomálie", icon: AlertTriangle, badge: counts.anomalies },
    { href: "/reporty", label: "Reporty", icon: FileDown },
    { href: "/senzory", label: "Senzory", icon: RadioTower },
    { href: "/zdroje-dat", label: "Zdroje dat", icon: Database },
    { href: "/otevrena-data", label: "Otevřené API", icon: Braces },
  ];

  return (
    <nav className="mt-8 flex flex-col gap-0.5" aria-label="Hlavní navigace">
      {items.map(({ href, label, icon: Icon, badge }) => {
        const active = href === "/" ? pathname === "/" : pathname.startsWith(href);

        return (
          <Link
            key={href}
            href={`${href}${["/zdroje-dat", "/reporty", "/otevrena-data", "/senzory"].includes(href) ? "" : issues}`}
            aria-current={active ? "page" : undefined}
            className={cn(
              "flex h-10.5 items-center gap-3 rounded-lg px-3 text-sm transition-colors hover:text-white",
              active ? "bg-neutral-900 font-bold text-white shadow-[inset_0_-3px_0_0_var(--color-hk-red)]" : "text-white/58",
            )}
          >
            <Icon className="size-4.5" strokeWidth={1.75} aria-hidden />
            <span className="flex-1">{label}</span>
            {badge !== undefined && <span className={cn("text-xs tabular-nums", active ? "text-white/70" : "text-neutral-600")}>{badge}</span>}
          </Link>
        );
      })}
    </nav>
  );
}
