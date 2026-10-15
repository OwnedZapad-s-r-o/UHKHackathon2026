import Link from "next/link";
import { ArrowRight } from "lucide-react";
import { Logo, LogoSymbol } from "@/components/brand/logo";
import { getSession } from "@/lib/api";
import { PUBLIC_LINKS } from "./_components/public-links";
import { PublicMenu } from "./_components/public-menu";

const REPOSITORY_URL = "https://github.com/OwnedZapad-s-r-o/UHK_Hackathon_2026";

// Veřejná část bez přihlášení — landing page, hledání objektů a jejich karty.
// Tělo stránky má pevnou výšku (root layout), proto vlastní posuvný kontejner (s plynulým posunem na kotvy).
export default async function PublicLayout({ children }: LayoutProps<"/">) {
  const session = await getSession();

  return (
    <div className="min-h-0 flex-1 overflow-y-auto scroll-smooth bg-white text-hk-navy motion-reduce:scroll-auto print:overflow-visible">
      <header className="sticky top-0 z-30 border-b border-slate-200 bg-white/94 backdrop-blur-md print:hidden">
        <div className="relative mx-auto flex h-17 max-w-300 items-center gap-4 px-4 sm:px-6">
          <Link href="/" aria-label="SmartAudit — úvodní stránka">
            <Logo className="h-8 w-auto" />
          </Link>

          <nav className="ml-auto hidden items-center gap-1 md:flex" aria-label="Veřejná navigace">
            {PUBLIC_LINKS.map((link) => (
              <a key={link.label} href={link.href} className="rounded-lg px-3 py-2 text-[14px] text-slate-700 transition-colors hover:bg-slate-100 hover:text-hk-navy">
                {link.label}
              </a>
            ))}
          </nav>

          <Link
            href={session ? "/mapa" : "/login"}
            className="ml-auto inline-flex h-10 items-center gap-2 rounded-[10px] bg-hk-navy px-4 text-[14px] font-bold text-white transition-colors hover:bg-hk-blue focus-visible:outline-3 focus-visible:outline-offset-2 focus-visible:outline-[#8DAACD] md:ml-2"
          >
            {session ? "Do aplikace" : "Přihlásit"}
            <ArrowRight className="size-3.75" aria-hidden />
          </Link>
          <PublicMenu />
        </div>
      </header>

      {children}

      <footer className="border-t border-slate-200 bg-white print:hidden">
        <div className="mx-auto max-w-300 px-6 pt-14 pb-8">
          <div className="grid gap-9 text-[14px] sm:grid-cols-2 lg:grid-cols-4">
            <div>
              <Logo className="h-8 w-auto" />
              <p className="mt-4.5 max-w-[34ch] leading-[1.55] text-slate-600">
                Energetický audit a kvalita vnitřního vzduchu v budovách, které zřizuje Královéhradecký kraj.
              </p>
            </div>
            <FooterColumn title="Pro veřejnost">
              <Link href="/objekty" className={FOOTER_LINK}>
                Najdi svou školu
              </Link>
              <Link href="/objekty?razeni=vzduch" className={FOOTER_LINK}>
                Kvalita vzduchu
              </Link>
              <Link href="/login" className={FOOTER_LINK}>
                Přihlásit
              </Link>
            </FooterColumn>
            <FooterColumn title="Data">
              <a href="https://www.datakhk.cz" target="_blank" rel="noreferrer" className={FOOTER_LINK}>
                Otevřená data Královéhradeckého kraje
              </a>
              <a href="/api/open/docs/" className={FOOTER_LINK}>
                Otevřené API SmartAuditu
              </a>
              <span className="text-slate-600">ČÚZK · Open-Meteo · MŠMT</span>
            </FooterColumn>
            <FooterColumn title="Projekt">
              <a href={REPOSITORY_URL} target="_blank" rel="noreferrer" className={FOOTER_LINK}>
                Zdrojový kód na GitHubu
              </a>
              <span className="text-slate-600">Hackathon Královéhradeckého kraje 2026</span>
              <span className="text-slate-600">Licence dat CC BY 4.0</span>
            </FooterColumn>
          </div>
          <div className="mt-12 flex flex-wrap justify-between gap-2 border-t border-slate-100 pt-5 text-[12px] text-slate-500">
            <span className="flex items-center gap-2.5">
              <LogoSymbol small className="size-4" />
              SmartAudit · pro Královéhradecký kraj
            </span>
            <span>Hodnoty bez čidla počítá fyzikální model z otevřených dat (ČÚZK + Data KHK + Open-Meteo) a jsou tak označené.</span>
          </div>
        </div>
      </footer>
    </div>
  );
}

const FOOTER_LINK = "text-slate-600 transition-colors hover:text-hk-red";

function FooterColumn({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="flex flex-col gap-2.5">
      <p className="text-[13px] font-bold text-hk-navy">{title}</p>
      {children}
    </div>
  );
}
