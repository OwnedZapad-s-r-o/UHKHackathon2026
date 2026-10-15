import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { ChevronLeft } from "lucide-react";
import { Logo } from "@/components/brand/logo";
import { CATEGORY_META } from "@/lib/facility-meta";
import { getPublicFacilityByCode } from "@/lib/public-api";
import { configuredPublicUrl } from "@/lib/public-url";
import { requestOrigin } from "@/lib/request-origin";
import { cn } from "@/lib/utils";
import { PrintButton } from "./_components/print-button";
import { QrDomainSwitch, QrImage, QrShortUrl, QrTargetProvider } from "./_components/qr-target";

export const metadata: Metadata = {
  title: "QR kód na dveře · SmartAudit",
};

// Škála kvality vzduchu na cedulce — stejné hranice jako veřejná karta (lib/air-quality)
const LEVELS = [
  { label: "Čerstvý vzduch", range: "do 1 000 ppm", dot: "bg-emerald-500", tone: "border-emerald-200 bg-emerald-50 text-emerald-900" },
  { label: "Čas vyvětrat", range: "1 000–1 500 ppm", dot: "bg-amber-500", tone: "border-amber-200 bg-amber-50 text-amber-900" },
  { label: "Vydýchaný vzduch", range: "nad 1 500 ppm", dot: "bg-red-500", tone: "border-red-200 bg-red-50 text-red-900" },
];

// Tisková cedulka A5 na dveře třídy nebo ke vchodu — QR vede na veřejnou kartu objektu na veřejné doméně
// (PUBLIC_URL / NEXT_PUBLIC_PUBLIC_URL, nebo přepínač nad cedulkou), aby fungoval na mobilech diváků, ne na localhostu.
// Všechny rozměry jsou v cqw (procenta šířky listu), takže náhled na obrazovce je přesně zmenšený tisk.
export default async function QrPage({ params }: PageProps<"/objekty/[kod]/qr">) {
  const { kod } = await params;
  const facility = await getPublicFacilityByCode(kod);
  if (!facility) notFound();

  const path = `/objekty/${facility.code}`;
  const address = [facility.street, facility.houseNumber].filter(Boolean).join(" ");
  const CategoryIcon = CATEGORY_META[facility.category].icon;

  return (
    <QrTargetProvider configuredBase={configuredPublicUrl()} requestBase={await requestOrigin()} path={path}>
    <main className="bg-slate-100 px-4 py-8 print:bg-white print:p-0">
      {/* Tisk bez okrajů prohlížeče, list A5 na výšku, barvy pozadí se vytisknou */}
      <style>{"@page { size: A5 portrait; margin: 0 } @media print { html, body { print-color-adjust: exact; -webkit-print-color-adjust: exact } }"}</style>

      <div className="mx-auto mb-5 flex max-w-[148mm] flex-wrap items-center justify-between gap-3 print:hidden">
        <Link href={`/objekty/${facility.code}`} className="inline-flex items-center gap-1 text-[13px] text-slate-600 hover:text-hk-red">
          <ChevronLeft className="size-4" aria-hidden />
          Zpět na kartu objektu
        </Link>
        <div className="flex items-center gap-3">
          <span className="text-[12px] text-slate-500">A5 na výšku · bez okrajů</span>
          <PrintButton />
        </div>
      </div>

      <QrDomainSwitch />

      <article className="@container mx-auto flex aspect-148/210 w-full max-w-[148mm] flex-col overflow-hidden bg-white text-hk-navy shadow-[0_24px_60px_-20px_rgba(11,25,44,0.35)] print:h-[210mm] print:w-[148mm] print:max-w-none print:shadow-none">
        {/* Hlavička se značkou SmartAuditu */}
        <header className="flex items-center bg-hk-navy px-[7cqw] py-[4cqw] text-white">
          <Logo tone="dark" className="h-[7cqw] w-auto" />
        </header>
        <div className="h-[1.1cqw] bg-hk-red" aria-hidden />

        <div className="flex min-h-0 flex-1 flex-col px-[7cqw] pt-[4.5cqw]">
          <p className="text-[2.5cqw] font-bold tracking-[0.08em] text-hk-red uppercase">Kvalita vnitřního vzduchu</p>
          <h1 className="mt-[1.6cqw] text-[7.2cqw] leading-[1.02] font-bold tracking-[-0.03em]">Jak se dýchá v&nbsp;této budově?</h1>
          <p className="mt-[2.4cqw] max-w-[88%] text-[3.2cqw] leading-snug text-slate-600">
            Naskenujte telefonem a uvidíte CO₂, teplotu a kdy větrat. Bez přihlášení, bez aplikace.
          </p>

          {/* QR s rohovými značkami — napoví, kam namířit fotoaparát */}
          <div className="mt-[3.5cqw] flex items-center gap-[6cqw]">
            <div className="relative shrink-0 p-[3cqw]">
              <Corner className="top-0 left-0 border-t-[1.1cqw] border-l-[1.1cqw] rounded-tl-[2.5cqw]" />
              <Corner className="top-0 right-0 border-t-[1.1cqw] border-r-[1.1cqw] rounded-tr-[2.5cqw]" />
              <Corner className="bottom-0 left-0 border-b-[1.1cqw] border-l-[1.1cqw] rounded-bl-[2.5cqw]" />
              <Corner className="right-0 bottom-0 border-r-[1.1cqw] border-b-[1.1cqw] rounded-br-[2.5cqw]" />
              <QrImage label={`QR kód na kartu objektu ${facility.name}`} className="block w-[34cqw]" />
            </div>

            <ol className="flex flex-col gap-[3cqw] text-[2.9cqw] leading-snug">
              <Step number={1}>Otevřete fotoaparát telefonu</Step>
              <Step number={2}>Namiřte ho na čtverec vlevo</Step>
              <Step number={3}>Klepněte na odkaz, který se objeví</Step>
            </ol>
          </div>

          {/* Co na kartě uvidí */}
          <div className="mt-[3cqw] grid grid-cols-3 gap-[1.8cqw]">
            {LEVELS.map((level) => (
              <div key={level.label} className={cn("rounded-[1.8cqw] border-[0.3cqw] px-[2.2cqw] py-[1.8cqw]", level.tone)}>
                <p className="flex items-center gap-[1.2cqw] text-[2.3cqw] leading-tight font-bold whitespace-nowrap">
                  <span className={cn("size-[1.8cqw] shrink-0 rounded-full", level.dot)} aria-hidden />
                  {level.label}
                </p>
                <p className="mt-[0.6cqw] font-mono text-[2.1cqw] opacity-80">{level.range}</p>
              </div>
            ))}
          </div>

          {/* Budova — dole, aby cedulka šla poznat i z dálky podle nadpisu */}
          <div className="mt-auto flex items-end gap-[3cqw] border-t-[0.3cqw] border-slate-200 pt-[3cqw] pb-[4cqw]">
            <span className="flex size-[9cqw] shrink-0 items-center justify-center rounded-[2cqw] bg-hk-navy text-white">
              <CategoryIcon className="size-[4.8cqw]" aria-hidden />
            </span>
            <div className="min-w-0 flex-1">
              <p className="text-[2.3cqw] text-slate-500">
                {facility.kind} · okres {facility.districtName}
              </p>
              <p className="mt-[0.5cqw] line-clamp-2 text-[3.9cqw] leading-[1.15] font-bold">{facility.name}</p>
              {address && (
                <p className="mt-[0.5cqw] truncate text-[2.5cqw] text-slate-600">
                  {address}, {facility.municipalityName}
                </p>
              )}
            </div>
          </div>
        </div>

        <footer className="flex items-center justify-between gap-[3cqw] bg-slate-50 px-[7cqw] py-[2.4cqw] text-[2.2cqw] text-slate-500">
          <QrShortUrl />
          <span className="shrink-0">Otevřená data kraje · CC BY 4.0</span>
        </footer>
      </article>
    </main>
    </QrTargetProvider>
  );
}

function Corner({ className }: { className: string }) {
  return <span className={cn("absolute size-[7cqw] border-hk-red", className)} aria-hidden />;
}

function Step({ number, children }: { number: number; children: React.ReactNode }) {
  return (
    <li className="flex gap-[2cqw]">
      <span className="flex size-[4.6cqw] shrink-0 items-center justify-center rounded-full bg-hk-navy font-mono text-[2.4cqw] font-bold text-white">{number}</span>
      <span className="pt-[0.6cqw]">{children}</span>
    </li>
  );
}
