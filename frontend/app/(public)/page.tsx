import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import {
  AlertCircle,
  AlertTriangle,
  ArrowRight,
  Braces,
  Building2,
  Check,
  Database,
  FileSearch,
  FileText,
  Landmark,
  Map as MapIcon,
  Radio,
  Search,
  Users,
  Wind,
  type LucideIcon,
} from "lucide-react";
import { ISSUE_SHARE_COLORS } from "@/lib/district-shading";
import { getPublicDatasets, getPublicFacilities } from "@/lib/public-api";
import { cn } from "@/lib/utils";
import { SchoolSearch } from "./_components/school-search";
import { toSearchItems } from "./_components/search-items";
import { PublicGuide } from "./_components/public-guide";

export const metadata: Metadata = {
  title: "SmartAudit · energetický audit budov kraje",
  description:
    "Kraj topí ve stovkách budov a neví, kde plýtvá. SmartAudit to zjistí z otevřených dat a čidel IQRF — pro kraj, správce budov i veřejnost.",
};

// Snímky obrazovek generuje tools/screenshots/capture.py (1440 × 900, telefon 390 × 844)
const SHOT = { width: 1440, height: 900 };
const PHONE = { width: 390, height: 844 };

// Kotvy sekcí — hlavička je lepivá, nadpis sekce nesmí zajet pod ni
const ANCHOR = "scroll-mt-17";

export default async function LandingPage() {
  const [facilities, datasets] = await Promise.all([getPublicFacilities().catch(() => []), getPublicDatasets().catch(() => null)]);

  const total = facilities.length;
  const critical = facilities.filter((f) => f.status === "Critical").length;
  const warning = facilities.filter((f) => f.status === "Warning").length;
  const nominal = total - critical - warning;
  const datasetCount = datasets ? datasets.khkDatasetCount + datasets.externalSourceCount : 29;
  const liveCount = facilities.filter((f) => f.hasLiveSensors).length;

  return (
    <main>
      {/* Úvod */}
      <section id="top" className="overflow-hidden bg-hk-navy text-white">
        <div className="mx-auto grid max-w-300 items-center gap-x-12 gap-y-14 px-6 pt-22 pb-24 lg:grid-cols-2">
          <div>
            <p className="text-[13px] font-bold tracking-[0.04em] text-white/72 uppercase">Hackathon Královéhradeckého kraje 2026 · otevřená data</p>
            <h1 className="mt-4.5 text-[clamp(38px,5.2vw,64px)] leading-[1.08] font-bold tracking-[-0.035em]">Kolik kraj protopí v budovách, které nikdo neměří?</h1>
            <p className="mt-7 max-w-[56ch] text-[18px] leading-[1.7] text-white/80">
              SmartAudit najde ve školách, domovech a nemocnicích kraje přetápění o víkendu, vydýchané třídy i chybějící noční útlum. Stojí na
              otevřených datech kraje a skutečných čidlech IQRF.
            </p>
            <div className="mt-10 flex flex-wrap gap-3.5">
              <PrimaryButton href="/login" size="lg">
                Vyzkoušet demo
              </PrimaryButton>
              <a href="#verejnost" className={cn(OUTLINE_BUTTON, "h-14.5 px-6 text-[17px]")}>
                <Search className="size-4.5" aria-hidden />
                Najít svou školu
              </a>
            </div>
          </div>

          {/* Aplikace pro kraj a přes ni telefon s veřejnou kartou */}
          <div className="relative py-10 pl-[16%]">
            <div className="overflow-hidden rounded-2xl bg-white shadow-[0_30px_80px_-20px_rgba(0,0,0,0.6)]">
              <Image
                src="/landing/mapa-3d.webp"
                alt="Mapa objektů: vybraná budova ve 3D v barvě stavu a inspekční panel s telemetrií"
                {...SHOT}
                priority
                sizes="(min-width: 1024px) 480px, 84vw"
                className="h-auto w-full"
              />
            </div>
            <div className="absolute bottom-0 left-0 w-[30%] rounded-[22px] bg-white p-1 shadow-[0_24px_60px_-16px_rgba(0,0,0,0.6)]">
              <div className="aspect-390/780 overflow-hidden rounded-[18px]">
                <Image src="/landing/karta-mobil.webp" alt="Veřejná karta školy v telefonu" {...PHONE} sizes="170px" className="size-full object-cover object-top" />
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Živá čísla z otevřeného API */}
      <section className="border-b border-slate-200 bg-white">
        <div className="mx-auto grid max-w-300 items-start gap-x-16 gap-y-12 px-6 py-18 lg:grid-cols-2">
          <div>
            <p className="flex items-center gap-2 text-[13px] text-slate-500">
              <span className="size-2 rounded-full bg-emerald-500" aria-hidden />
              Živě z otevřeného API
            </p>
            <p className="mt-4 text-[clamp(28px,3.6vw,44px)] leading-[1.1] font-bold tracking-[-0.03em]">
              <span className="font-mono text-hk-red">{critical + warning}</span> {fromPreposition(total)} <span className="font-mono">{total}</span> budov kraje má zjištění z telemetrie.
            </p>
            <div className="mt-7 flex h-3.5 gap-0.75 overflow-hidden rounded-full" aria-hidden>
              <span className="bg-red-500" style={{ flex: critical }} />
              <span className="bg-amber-500" style={{ flex: warning }} />
              <span className="bg-emerald-500" style={{ flex: nominal }} />
            </div>
            <div className="mt-3.5 flex flex-wrap gap-x-6 gap-y-2 text-[13px] text-slate-700">
              <StatusCount icon={AlertCircle} tone="text-red-600" value={critical} label="kritických" />
              <StatusCount icon={AlertTriangle} tone="text-amber-600" value={warning} label="varování" />
              <StatusCount icon={Check} tone="text-emerald-600" value={nominal} label="v pořádku" />
            </div>
            <TextLink href="/login" className="mt-6">
              Ukázat je na mapě
            </TextLink>
          </div>

          <div className="grid grid-cols-2 gap-px overflow-hidden rounded-2xl border border-slate-200 bg-slate-200">
            <BigFigure value={datasetCount} label="otevřených datových sad, každá s konkrétní úlohou" />
            <BigFigure value={liveCount} label={`${buildings(liveCount)} se živým čidlem IQRF, ostatní počítá model`} />
            <p className="col-span-full bg-slate-50 px-6 py-4.5 text-[13px] leading-normal text-slate-600">
              Kde čidla zatím nejsou, počítá hodnoty fyzikální model z otevřených dat (ČÚZK + Data KHK + Open-Meteo) a aplikace to vždy označí.
            </p>
          </div>
        </div>
      </section>

      {/* Jak to funguje */}
      <section className="bg-slate-50">
        <div className="mx-auto max-w-300 px-6 py-22">
          <h2 className="text-[clamp(28px,3.4vw,40px)] font-bold tracking-[-0.03em]">Jak to funguje</h2>
          <ol className="mt-11 grid gap-10 md:grid-cols-3">
            <Step number={1} icon={Database} title="Otevřená data">
              Kraj zveřejňuje, co zřizuje: školy, domovy, nemocnice. SmartAudit k tomu připojí katastr ČÚZK, počty žáků, počasí, prázdniny,
              památkovou ochranu a dotace.
            </Step>
            <Step number={2} icon={Radio} title="Čidla IQRF">
              Gateway v budově posílá teplotu, CO₂ a vlhkost. Aplikace sama najde uzly a senzory, správce přiřadí místnost. Kde čidla zatím
              nejsou, počítá model.
            </Step>
            <Step number={3} icon={FileSearch} title="Audit a opatření" accent>
              Pravidla najdou přetápění, špatné větrání a chybějící noční útlum. Kraj dostane pořadí budov podle ztráty, správce PDF audit a AI
              návrh opatření.
            </Step>
          </ol>
        </div>
      </section>

      {/* Funkce */}
      <section className="bg-white">
        <div className="mx-auto flex max-w-300 flex-col gap-24 px-6 py-24">
          <div className="grid items-center gap-x-16 gap-y-10 lg:grid-cols-2">
            <div className="max-w-110">
              <Eyebrow icon={MapIcon}>Mapa kraje</Eyebrow>
              <h2 className={FEATURE_TITLE}>Celý kraj na jedné mapě</h2>
              <p className="mt-4 text-[16px] leading-relaxed text-slate-600">
                Nejdřív kraj po okresech, pak budovy v okrese, nakonec jedna budova ve 3D. Okresy mají barvu podle toho, kolik budov má problém.
              </p>
              <div className="mt-5.5 flex flex-col gap-2">
                <span className="text-[12px] text-slate-500">Podíl budov se zjištěním</span>
                <span className="flex gap-0.75" aria-hidden>
                  {ISSUE_SHARE_COLORS.map((color) => (
                    <span key={color} className="h-2.5 flex-1 rounded-[3px]" style={{ background: color }} />
                  ))}
                </span>
                <span className="flex justify-between font-mono text-[11px] text-slate-500">
                  <span>méně</span>
                  <span>více</span>
                </span>
              </div>
            </div>
            <Screenshot src="/landing/mapa-okresy.webp" alt="Mapa kraje po okresech s bublinami stavů a tečkami budov" />
          </div>

          <div className="grid gap-6 lg:grid-cols-2">
            <FeatureCard
              icon={Building2}
              eyebrow="Inspekční panel"
              title="Co se v budově děje, na jeden pohled"
              image={{ src: "/landing/inspekce.webp", alt: "Inspekční panel s telemetrií, místnostmi a zjištěními auditu" }}
            >
              Teplota za 48 hodin, spotřeba po dnech, provozní heatmapa a místnosti s aktuálním CO₂. Audit vysvětlí česky, co je špatně a proč.
            </FeatureCard>
            <FeatureCard
              icon={Radio}
              eyebrow="Skutečná čidla IQRF"
              title="Připojení čidla za pět minut"
              image={{ src: "/landing/senzory.webp", alt: "Obrazovka Senzory s živými hodnotami a přiřazením do místností" }}
            >
              IQRF gateway se přes MQTT připojí k aplikaci. Ta sama najde uzly a jejich senzory, správce jen vybere místnost. Skutečné čidlo
              nahradí v místnosti fyzikální model.
            </FeatureCard>
          </div>

          <div className="grid items-center gap-x-16 gap-y-10 lg:grid-cols-2">
            <div className="overflow-hidden rounded-2xl bg-slate-200">
              <Image src="/landing/pdf.webp" alt="Dvě strany PDF auditu objektu" {...SHOT} sizes="(min-width: 1024px) 560px, 100vw" className="h-auto w-full" />
            </div>
            <div className="max-w-110">
              <Eyebrow icon={FileText}>Reporty a AI</Eyebrow>
              <h2 className={FEATURE_TITLE}>Podklad pro rozhodnutí, ne další dashboard</h2>
              <p className="mt-4 text-[16px] leading-relaxed text-slate-600">
                PDF audit budovy s grafy a přehled portfolia s mapou okresů. Azure OpenAI navrhne opatření s návratností a dopis zřizovateli, s
                ohledem na památkovou ochranu.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Pro veřejnost */}
      {/* overflow-x-clip místo hidden: natočený telefon se ořízne jen do stran, našeptávač smí přesahovat pod sekci (z-10 = nad další sekcí) */}
      <section id="verejnost" className={cn(ANCHOR, "relative z-10 overflow-x-clip border-y border-amber-200 bg-amber-50")}>
        <div className="mx-auto grid max-w-300 items-center gap-14 px-6 py-22 lg:grid-cols-2">
          <div>
            <Eyebrow icon={Wind} className="text-amber-800">
              Pro veřejnost · bez přihlášení
            </Eyebrow>
            <h2 className="mt-3.5 text-[clamp(32px,4.4vw,52px)] leading-[1.05] font-bold tracking-[-0.035em]">Jak se dýchá ve škole mého dítěte?</h2>
            <p className="mt-4.5 max-w-[50ch] text-[17px] leading-relaxed text-amber-900">
              Rodiče, žáci, senioři a jejich blízcí najdou svou budovu bez přihlášení. Jaký je vzduch teď, v kterých místnostech, kdy větrat. QR kód
              na dveřích třídy vede rovnou na kartu.
            </p>

            {/* Obyčejný formulář — hledání funguje i bez JavaScriptu a vede na stránku objektů; s JavaScriptem našeptává */}
            <form action="/objekty" className="mt-7 flex max-w-130 flex-wrap gap-2">
              <div className="flex flex-[1_1_260px]">
                <SchoolSearch
                  items={toSearchItems(facilities)}
                  placeholder="Název školy, domova nebo obce"
                  fieldClassName="flex h-13.5 items-center gap-2.5 rounded-xl border border-amber-300 bg-white px-4 shadow-[0_1px_2px_rgba(11,25,44,0.06)] focus-within:outline-3 focus-within:outline-amber-500/60"
                  iconClassName="text-amber-800"
                  inputClassName="text-[16px]"
                />
              </div>
              <button
                type="submit"
                className="h-13.5 rounded-xl bg-hk-navy px-5.5 text-[16px] font-bold text-white transition-colors hover:bg-hk-blue focus-visible:outline-3 focus-visible:outline-offset-2 focus-visible:outline-amber-500"
              >
                Najít
              </button>
            </form>

            <div id="vzduch" className={cn(ANCHOR, "mt-9 grid grid-cols-3 gap-2.5")}>
              <AirLevelCard icon={Check} label="Čerstvý vzduch" range="do 1 000 ppm" className="border-emerald-200 text-emerald-800" />
              <AirLevelCard icon={Wind} label="Čas vyvětrat" range="do 1 500 ppm" className="border-amber-300 text-amber-800" />
              <AirLevelCard icon={AlertTriangle} label="Vydýchaný vzduch" range="nad 1 500 ppm" className="border-red-200 text-red-800" />
            </div>
          </div>

          <div className="flex justify-center">
            <div className="w-[min(100%,320px)] -rotate-2 rounded-[40px] bg-hk-navy p-2.5 shadow-[0_40px_80px_-30px_rgba(120,53,15,0.45)]">
              <div className="overflow-hidden rounded-[31px] bg-white">
                <Image src="/landing/karta-mobil.webp" alt="Veřejná karta školy v telefonu: Čas vyvětrat" {...PHONE} sizes="300px" className="h-auto w-full" />
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Otevřená data */}
      <section id="data" className={cn(ANCHOR, "bg-hk-navy text-white")}>
        <div className="mx-auto max-w-300 px-6 py-24">
          <div className="grid items-end gap-x-16 gap-y-10 lg:grid-cols-2">
            <div>
              <Eyebrow icon={Braces} className="text-red-300">
                Otevřená data zpět
              </Eyebrow>
              <h2 className="mt-3.5 text-[clamp(28px,3.8vw,48px)] leading-[1.08] font-bold tracking-[-0.03em]">Co SmartAudit zjistí, vrací jako otevřená data</h2>
            </div>
            <div>
              <p className="text-[16px] leading-relaxed text-white/75">
                Objekty, půdorysy budov a měření jsou k dispozici bez klíče jako JSON, GeoJSON a CSV pod licencí CC BY 4.0.
              </p>
              <div className="mt-4.5 flex flex-wrap gap-2 font-mono text-[12px]">
                {["JSON", "GeoJSON", "CSV", "OpenAPI 3.1"].map((format) => (
                  <span key={format} className="rounded-lg border border-white/20 px-2.5 py-1.5">
                    {format}
                  </span>
                ))}
                <span className="rounded-lg bg-white px-2.5 py-1.5 font-bold text-hk-navy">CC BY 4.0</span>
              </div>
              <a
                href="/api/open/docs/"
                className="mt-5 inline-flex items-center gap-1.5 pb-0.75 text-[15px] font-bold shadow-[inset_0_-2px_0_var(--color-hk-accent)] transition-colors hover:text-red-300"
              >
                Dokumentace otevřeného API
                <ArrowRight className="size-3.75" aria-hidden />
              </a>
            </div>
          </div>

          <div className="mt-12 overflow-hidden rounded-2xl border border-white/14">
            <Image src="/landing/otevrene-api.webp" alt="Stránka otevřeného API se seznamem endpointů" {...SHOT} sizes="(min-width: 1200px) 1152px, 100vw" className="h-auto w-full" />
          </div>

          <div className="mt-20 border-t border-white/14 pt-10">
            <div className="flex flex-wrap items-baseline justify-between gap-3">
              <h3 className="text-[24px] font-bold tracking-[-0.02em]">Z čeho SmartAudit vychází</h3>
              <span className="text-[14px] text-white/60">Zdrojový kód je otevřený na GitHubu.</span>
            </div>
            <dl className="mt-6 grid gap-px overflow-hidden rounded-2xl border border-white/12 bg-white/12 sm:grid-cols-2 lg:grid-cols-3">
              {SOURCES.map((source) => (
                <div key={source.title} className="bg-hk-navy p-5.5">
                  <dt className="text-[16px] font-bold">{source.title}</dt>
                  <dd className="mt-1.5 text-[14px] leading-normal text-white/65">{source.text}</dd>
                </div>
              ))}
            </dl>
          </div>
        </div>
      </section>

      {/* Pro koho */}
      <section className="bg-white">
        <div className="mx-auto max-w-300 px-6 py-24">
          <h2 className="text-[clamp(28px,3.4vw,40px)] font-bold tracking-[-0.03em]">Pro koho</h2>
          <div className="mt-10 grid gap-5 md:grid-cols-3">
            <Audience
              icon={Landmark}
              title="Kraj"
              subtitle="Radní, investiční a majetkový odbor"
              items={["Kde plýtvá a kolik to stojí", "Pořadí investic podle ztráty", "Přehled celého portfolia v PDF"]}
              tone={{ tile: "bg-red-50 text-hk-red", check: "text-hk-red" }}
              link={{ href: "/login", label: "Demo pro kraj" }}
            />
            <Audience
              icon={Building2}
              title="Správce budovy"
              subtitle="Ředitel školy nebo domova"
              items={["Co je v budově špatně a proč", "Připojení vlastních čidel IQRF", "Audit a návrh opatření s návratností"]}
              tone={{ tile: "bg-[#EFF4FA] text-hk-blue", check: "text-hk-blue" }}
              link={{ href: "/login", label: "Demo pro správce" }}
            />
            <Audience
              icon={Users}
              title="Veřejnost"
              subtitle="Rodiče, žáci, senioři a jejich blízcí"
              items={["Kvalita vzduchu ve škole nebo domově", "Kdy větrat a co kraj řeší", "QR kód na dveřích třídy"]}
              tone={{ tile: "bg-amber-50 text-amber-800", check: "text-amber-800" }}
              link={{ href: "#verejnost", label: "Najít svou školu" }}
            />
          </div>
        </div>
      </section>

      {/* Výzva */}
      <section className="bg-hk-navy text-white">
        <div className="mx-auto flex max-w-300 flex-wrap items-center justify-between gap-8 px-6 py-20">
          <div>
            <h2 className="text-[clamp(32px,4.4vw,52px)] leading-[1.05] font-bold tracking-[-0.035em]">Vyzkoušejte si to</h2>
            <p className="mt-3 text-[17px] text-white/72">Demo bez registrace pro kraj, správce budovy i veřejnost.</p>
          </div>
          <div className="flex flex-wrap gap-3">
            <PrimaryButton href="/login">Vyzkoušet demo</PrimaryButton>
            <a href="#verejnost" className={cn(OUTLINE_BUTTON, "h-13 px-5.5 text-[16px]")}>
              Najít svou školu
            </a>
          </div>
        </div>
      </section>

      <PublicGuide page="uvod" />
    </main>
  );
}

const SOURCES = [
  { title: "Data KHK", text: "25 sad: objekty kraje, organizace, dotace, památková ochrana" },
  { title: "ČÚZK", text: "Budovy a půdorysy z katastru, rok výstavby, zastavěná plocha" },
  { title: "Open-Meteo a MŠMT", text: "Venkovní teplota, školní kalendář a prázdniny, počty žáků" },
  { title: "IQRF · MICRORISC", text: "Skutečná čidla českého výrobce z Jičína, připojená přes MQTT" },
  { title: "Azure OpenAI", text: "Návrh opatření a dopisu zřizovateli z výsledků auditu" },
  { title: "Fyzikální model z otevřených dat", text: "Kde čidlo chybí, hodnoty počítá model z ČÚZK, Data KHK a Open-Meteo a aplikace to vždy uvádí" },
];

const FEATURE_TITLE = "mt-3.5 text-[clamp(28px,3.4vw,40px)] leading-[1.1] font-bold tracking-[-0.03em]";

const OUTLINE_BUTTON =
  "inline-flex items-center gap-2.5 rounded-xl border border-white/30 font-bold text-white transition-colors hover:bg-white/8 focus-visible:outline-3 focus-visible:outline-offset-2 focus-visible:outline-white";

// „ze 172“, „ze 17“, ale „z 46“ — předložka podle výslovnosti čísla (sto, sedm…)
function fromPreposition(value: number) {
  const digits = String(value);
  const startsWithSto = digits.length === 3 && digits[0] === "1";
  return startsWithSto || digits[0] === "7" ? "ze" : "z";
}

function buildings(count: number) {
  if (count === 1) return "budova";
  if (count >= 2 && count <= 4) return "budovy";
  return "budov";
}

function PrimaryButton({ href, size = "md", children }: { href: "/login"; size?: "md" | "lg"; children: React.ReactNode }) {
  return (
    <Link
      href={href}
      className={cn(
        "inline-flex items-center gap-2.5 rounded-xl bg-hk-accent font-bold text-white transition-colors hover:bg-hk-red focus-visible:outline-3 focus-visible:outline-offset-2 focus-visible:outline-white",
        size === "lg" ? "h-14.5 px-6 text-[17px]" : "h-13 px-5.5 text-[16px]",
      )}
    >
      {children}
      <ArrowRight className="size-4.5" aria-hidden />
    </Link>
  );
}

function TextLink({ href, className, children }: { href: "/login"; className?: string; children: React.ReactNode }) {
  return (
    <Link href={href} className={cn("inline-flex items-center gap-1.5 text-[15px] font-bold text-hk-red hover:text-hk-accent hover:underline", className)}>
      {children}
      <ArrowRight className="size-3.75" aria-hidden />
    </Link>
  );
}

function Eyebrow({ icon: Icon, className, children }: { icon: LucideIcon; className?: string; children: React.ReactNode }) {
  return (
    <p className={cn("flex items-center gap-2 text-[12px] font-bold tracking-[0.06em] text-hk-red uppercase", className)}>
      <Icon className="size-3.75" aria-hidden />
      {children}
    </p>
  );
}

function StatusCount({ icon: Icon, tone, value, label }: { icon: LucideIcon; tone: string; value: number; label: string }) {
  return (
    <span className="flex items-center gap-1.75">
      <Icon className={cn("size-3.75", tone)} aria-hidden />
      <span className="font-mono font-bold">{value}</span>
      {label}
    </span>
  );
}

function BigFigure({ value, label }: { value: number; label: string }) {
  return (
    <div className="flex flex-col-reverse bg-white p-6">
      <span className="mt-1.5 text-[14px] leading-[1.45] text-slate-600">{label}</span>
      <span className="font-mono text-[40px] font-bold tracking-[-0.03em] tabular-nums">{value.toLocaleString("cs-CZ")}</span>
    </div>
  );
}

function Step({ number, icon: Icon, title, accent = false, children }: { number: number; icon: LucideIcon; title: string; accent?: boolean; children: React.ReactNode }) {
  return (
    <li className={cn("border-t-2 pt-5.5", accent ? "border-hk-red" : "border-hk-navy")}>
      <div className="flex items-center justify-between">
        <span className="font-mono text-[13px] text-slate-500">0{number}</span>
        <Icon className={cn("size-5.5", accent ? "text-hk-red" : "text-hk-blue")} aria-hidden />
      </div>
      <h3 className="mt-4.5 text-[22px] font-bold tracking-[-0.02em]">{title}</h3>
      <p className="mt-2.5 text-[15px] leading-relaxed text-slate-600">{children}</p>
    </li>
  );
}

function Screenshot({ src, alt }: { src: string; alt: string }) {
  return (
    <div className="overflow-hidden rounded-2xl border border-slate-200 shadow-[0_24px_60px_-24px_rgba(11,25,44,0.28)]">
      <Image src={src} alt={alt} {...SHOT} sizes="(min-width: 1024px) 560px, 100vw" className="h-auto w-full" />
    </div>
  );
}

type FeatureCardProps = {
  icon: LucideIcon;
  eyebrow: string;
  title: string;
  image: { src: string; alt: string };
  children: React.ReactNode;
};

// Karta funkce se snímkem, který vyjíždí ze spodní hrany
function FeatureCard({ icon, eyebrow, title, image, children }: FeatureCardProps) {
  return (
    <div className="flex flex-col overflow-hidden rounded-3xl border border-slate-200 bg-slate-50 px-6 pt-8 sm:px-8">
      <Eyebrow icon={icon}>{eyebrow}</Eyebrow>
      <h3 className="mt-3.5 text-[28px] leading-[1.15] font-bold tracking-[-0.025em]">{title}</h3>
      <p className="mt-3 text-[15px] leading-relaxed text-slate-600">{children}</p>
      {/* Mezera se natáhne, aby snímky v obou kartách začínaly ve stejné výšce */}
      <div className="min-h-7 flex-1" aria-hidden />
      <div className="overflow-hidden rounded-t-[14px] border border-b-0 border-slate-200">
        <Image src={image.src} alt={image.alt} {...SHOT} sizes="(min-width: 1024px) 520px, 100vw" className="h-auto w-full" />
      </div>
    </div>
  );
}

function AirLevelCard({ icon: Icon, label, range, className }: { icon: LucideIcon; label: string; range: string; className: string }) {
  return (
    <div className={cn("rounded-xl border bg-white p-3.5", className)}>
      <p className="flex items-center gap-1.5 text-[13px] leading-tight font-bold">
        <Icon className="size-3.5 shrink-0" aria-hidden />
        {label}
      </p>
      <p className="mt-1.5 font-mono text-[12px] text-slate-600">{range}</p>
    </div>
  );
}

type AudienceProps = {
  icon: LucideIcon;
  title: string;
  subtitle: string;
  items: string[];
  tone: { tile: string; check: string };
  link: { href: "/login" | "#verejnost"; label: string };
};

function Audience({ icon: Icon, title, subtitle, items, tone, link }: AudienceProps) {
  return (
    <div className="flex flex-col rounded-[20px] border border-slate-200 p-7">
      <span className={cn("flex size-11 items-center justify-center rounded-xl", tone.tile)}>
        <Icon className="size-5" aria-hidden />
      </span>
      <h3 className="mt-4.5 text-[22px] font-bold tracking-[-0.02em]">{title}</h3>
      <p className="mt-1 text-[14px] text-slate-500">{subtitle}</p>
      <ul className="mt-4.5 flex flex-col gap-2.5 text-[15px] text-slate-700">
        {items.map((item) => (
          <li key={item} className="flex gap-2.5">
            <Check className={cn("mt-0.5 size-4 shrink-0", tone.check)} aria-hidden />
            {item}
          </li>
        ))}
      </ul>
      <a href={link.href} className="mt-auto inline-flex items-center gap-1.5 pt-6 text-[15px] font-bold text-hk-red hover:text-hk-accent hover:underline">
        {link.label}
        <ArrowRight className="size-3.75" aria-hidden />
      </a>
    </div>
  );
}
