import Link from "next/link";
import { Compass, LogOut, Search } from "lucide-react";
import { logout } from "@/app/login/actions";
import { Logo } from "@/components/brand/logo";
import { cn } from "@/lib/utils";
import type { Role, Session } from "@/types/api";
import { IssuesToggle } from "./issues-toggle";
import { SidebarNav } from "./sidebar-nav";

const ROLE_LABEL: Record<Role, string> = {
  Kraj: "Krajský úřad",
  Spravce: "Správce budovy",
  Verejnost: "Veřejnost",
};

type SidebarProps = {
  session: Session;
  counts: { portfolio: number; anomalies: number };
  className?: string;
};

export function Sidebar({ session, counts, className }: SidebarProps) {
  // Hledání a filtr „jen se zjištěním“ jsou pro stovky budov kraje, správce jich má pár
  const ownBuildings = session.role === "Spravce";

  return (
    <aside className={cn("flex w-58 shrink-0 flex-col overflow-y-auto bg-hk-navy px-4 pt-7 pb-5 text-white", className)}>
      <Link href="/mapa" className="block px-2" aria-label="SmartAudit — mapa objektů">
        <Logo tone="dark" className="h-8 w-auto" />
      </Link>

      <SidebarNav counts={counts} role={session.role} />

      <div className="mx-2 my-5 h-px bg-white/10" aria-hidden />

      <nav className="flex flex-col gap-0.5" aria-label="Rychlé akce">
        {!ownBuildings && (
          <>
            <Link
              href="/portfolio?hledat=1"
              className="flex h-10.5 items-center gap-3 rounded-lg px-3 text-sm text-white/55 transition-colors hover:text-white"
            >
              <Search className="size-4.5" strokeWidth={1.75} aria-hidden />
              Hledat objekt
            </Link>
            <IssuesToggle />
          </>
        )}
        <Link
          href="/mapa?pruvodce=1"
          data-tour="pruvodce"
          className="flex h-10.5 items-center gap-3 rounded-lg px-3 text-sm text-white/55 transition-colors hover:text-white"
        >
          <Compass className="size-4.5" strokeWidth={1.75} aria-hidden />
          Průvodce aplikací
        </Link>
      </nav>

      <div className="mt-auto flex items-center gap-2.5 px-2">
        <span className="flex size-8.5 shrink-0 items-center justify-center rounded-full border border-white/12 bg-white/8 text-xs font-bold text-white/70">
          {initials(session.displayName)}
        </span>
        <span className="min-w-0 flex-1 leading-tight">
          <span className="block truncate text-[13px]">{session.displayName}</span>
          <span className="block truncate text-[11px] text-white/50" title={session.organizationName ?? undefined}>
            {session.organizationName ?? ROLE_LABEL[session.role]}
          </span>
        </span>
        <form action={logout}>
          <button type="submit" aria-label="Odhlásit" title="Odhlásit" className="rounded-md p-1.5 text-white/50 transition-colors hover:bg-white/8 hover:text-white">
            <LogOut className="size-4" strokeWidth={1.75} />
          </button>
        </form>
      </div>
    </aside>
  );
}

function initials(name: string) {
  return name
    .split(/[\s·.]+/)
    .filter((part) => part.length > 0 && part[0] === part[0].toUpperCase())
    .slice(0, 2)
    .map((part) => part[0])
    .join("");
}
