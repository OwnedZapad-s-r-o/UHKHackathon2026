import Image from "next/image";
import Link from "next/link";
import { ArrowRight } from "lucide-react";
import { getSession } from "@/lib/api";

const REPOSITORY_URL = "https://github.com/OwnedZapad-s-r-o/UHK_Hackathon_2026";

// Veřejná část bez přihlášení — landing page, hledání objektů a jejich karty.
// Tělo stránky má pevnou výšku (root layout), proto vlastní posuvný kontejner.
export default async function PublicLayout({ children }: LayoutProps<"/">) {
  const session = await getSession();

  return (
    <div className="min-h-0 flex-1 overflow-y-auto bg-white text-hk-navy">
      <header className="sticky top-0 z-30 border-b border-slate-200/80 bg-white/90 backdrop-blur print:hidden">
        <div className="mx-auto flex h-16 max-w-6xl items-center gap-6 px-4 sm:px-6">
          <Link href="/" className="flex items-center gap-3">
            <Image src="/brand/hk-logo-white.png" alt="Hradec Králové" width={330} height={142} priority className="h-8 w-auto invert" />
            <span className="leading-tight">
              <span className="block text-[15px] font-bold tracking-tight">SmartAudit</span>
              <span className="block text-[10.5px] text-slate-500">Královéhradecký kraj</span>
            </span>
          </Link>

          <nav className="ml-auto hidden items-center gap-5 text-[13.5px] text-slate-600 md:flex" aria-label="Veřejná navigace">
            <Link href="/objekty" className="hover:text-hk-red">
              Najdi svou školu
            </Link>
            <Link href="/objekty?razeni=vzduch" className="hover:text-hk-red">
              Kvalita vzduchu
            </Link>
            <a href="/api/open/docs/" className="hover:text-hk-red">
              Otevřená data
            </a>
          </nav>

          <Link
            href={session ? "/mapa" : "/login"}
            className="ml-auto inline-flex h-9 items-center gap-1.5 rounded-lg bg-hk-navy px-3.5 text-[13px] font-bold text-white transition-colors hover:bg-hk-blue md:ml-0"
          >
            {session ? "Do aplikace" : "Přihlásit"}
            <ArrowRight className="size-3.5" aria-hidden />
          </Link>
        </div>
      </header>

      {children}

      <footer className="border-t border-slate-200 bg-slate-50 print:hidden">
        <div className="mx-auto grid max-w-6xl gap-6 px-4 py-10 text-[13px] text-slate-600 sm:grid-cols-3 sm:px-6">
          <div>
            <p className="font-bold text-hk-navy">SmartAudit KHK</p>
            <p className="mt-1.5 leading-relaxed">Energetický audit a kvalita vnitřního vzduchu v budovách, které zřizuje Královéhradecký kraj.</p>
          </div>
          <div>
            <p className="font-bold text-hk-navy">Data</p>
            <ul className="mt-1.5 space-y-1">
              <li>
                <a href="https://www.datakhk.cz" target="_blank" rel="noreferrer" className="hover:text-hk-red">
                  Otevřená data Královéhradeckého kraje
                </a>
              </li>
              <li>
                <a href="/api/open/docs/" className="hover:text-hk-red">
                  Otevřené API SmartAuditu (CC BY 4.0)
                </a>
              </li>
              <li>ČÚZK · Open-Meteo · MŠMT</li>
            </ul>
          </div>
          <div>
            <p className="font-bold text-hk-navy">Projekt</p>
            <ul className="mt-1.5 space-y-1">
              <li>
                <a href={REPOSITORY_URL} target="_blank" rel="noreferrer" className="hover:text-hk-red">
                  Zdrojový kód na GitHubu
                </a>
              </li>
              <li>Hackathon Královéhradeckého kraje 2026</li>
            </ul>
          </div>
        </div>
      </footer>
    </div>
  );
}
