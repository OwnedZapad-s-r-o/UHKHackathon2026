import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import {
  ArrowRight,
  Braces,
  Building2,
  ClipboardCheck,
  Database,
  FileDown,
  Landmark,
  Map as MapIcon,
  RadioTower,
  Search,
  Smartphone,
  Users,
} from "lucide-react";
import { getPublicDatasets, getPublicFacilities } from "@/lib/public-api";
import { Bullets, FeatureRow } from "./_components/feature-row";

export const metadata: Metadata = {
  title: "SmartAudit KHK · energetický audit budov kraje",
  description:
    "Kraj topí ve stovkách budov a neví, kde plýtvá. SmartAudit to zjistí z otevřených dat a čidel IQRF — pro kraj, správce budov i veřejnost.",
};

// Snímky obrazovek generuje tools/screenshots/capture.py (1440 × 900, telefon 390 × 844)
const SHOT = { width: 1440, height: 900 };
const PHONE = { width: 390, height: 844 };

export default async function LandingPage() {
  const [facilities, datasets] = await Promise.all([getPublicFacilities().catch(() => []), getPublicDatasets().catch(() => null)]);

  const stats = [
    { value: facilities.length, label: "budov kraje v evidenci" },
    { value: facilities.filter((f) => f.status !== "Nominal").length, label: "se zjištěním z telemetrie" },
    { value: datasets ? datasets.khkDatasetCount + datasets.externalSourceCount : 29, label: "otevřených datových sad" },
    { value: facilities.filter((f) => f.hasLiveSensors).length, label: "budov se živým čidlem IQRF" },
  ];

  return (
    <main>
      {/* Úvod */}
      <section className="relative overflow-hidden bg-hk-navy text-white">
        <div className="mx-auto grid max-w-6xl items-center gap-10 px-4 py-16 sm:px-6 lg:grid-cols-[1fr_1.1fr] lg:py-24">
          <div>
            <p className="text-[12px] font-bold tracking-wide text-white/60 uppercase">Hackathon Královéhradeckého kraje 2026 · otevřená data</p>
            <h1 className="mt-3 text-[38px] leading-[1.08] font-bold tracking-tight sm:text-[52px]">Kolik kraj protopí v budovách, které nikdo neměří?</h1>
            <p className="mt-5 max-w-xl text-[17px] leading-relaxed text-white/75">
              SmartAudit najde ve školách, domovech a nemocnicích kraje přetápění o víkendu, vydýchané třídy i chybějící noční útlum. Stojí na
              otevřených datech kraje a skutečných čidlech IQRF.
            </p>
            <div className="mt-8 flex flex-wrap gap-3">
              <Link href="/login" className="inline-flex h-12 items-center gap-2 rounded-xl bg-hk-accent px-5 text-[15px] font-bold transition-colors hover:bg-hk-red">
                Vyzkoušet demo
                <ArrowRight className="size-4" aria-hidden />
              </Link>
              <Link
                href="/objekty"
                className="inline-flex h-12 items-center gap-2 rounded-xl border border-white/25 px-5 text-[15px] font-bold transition-colors hover:bg-white/10"
              >
                <Search className="size-4" aria-hidden />
                Najít svou školu
              </Link>
            </div>
          </div>
          <div className="relative">
            <div className="overflow-hidden rounded-2xl border border-white/15 shadow-[0_30px_80px_rgba(0,0,0,0.45)]">
              <Image src="/landing/mapa-3d.webp" alt="Mapa objektů kraje s vybranou budovou ve 3D a inspekčním panelem" {...SHOT} priority sizes="(min-width: 1024px) 620px, 100vw" className="h-auto w-full" />
            </div>
            <div className="absolute -bottom-8 -left-6 hidden w-36 overflow-hidden rounded-[22px] border-4 border-white shadow-[0_20px_50px_rgba(0,0,0,0.4)] sm:block">
              <Image src="/landing/karta-mobil.webp" alt="Veřejná karta objektu v telefonu" {...PHONE} sizes="144px" className="h-auto w-full" />
            </div>
          </div>
        </div>
      </section>

      {/* Živá čísla z otevřeného API */}
      <section className="border-b border-slate-200 bg-white">
        <dl className="mx-auto grid max-w-6xl grid-cols-2 gap-6 px-4 py-10 sm:px-6 lg:grid-cols-4">
          {stats.map((stat) => (
            <div key={stat.label} className="flex flex-col-reverse">
              <dt className="mt-1 text-[13.5px] text-slate-500">{stat.label}</dt>
              <dd className="font-mono text-[36px] font-bold tracking-tight text-hk-navy tabular-nums">{stat.value.toLocaleString("cs-CZ")}</dd>
            </div>
          ))}
        </dl>
      </section>

      {/* Jak to funguje */}
      <section className="bg-slate-50">
        <div className="mx-auto max-w-6xl px-4 py-16 sm:px-6">
          <h2 className="text-[30px] font-bold tracking-tight">Jak to funguje</h2>
          <ol className="mt-8 grid gap-4 md:grid-cols-3">
            <Step number={1} icon={Database} title="Otevřená data">
              Kraj zveřejňuje, co zřizuje: školy, domovy, nemocnice. SmartAudit k nim připojí katastr ČÚZK (rok výstavby, plocha), počet žáků,
              počasí, prázdniny, památkovou ochranu i dotace.
            </Step>
            <Step number={2} icon={RadioTower} title="Čidla IQRF">
              Gateway v budově posílá teplotu, CO₂ i vlhkost. Aplikace sama najde uzly a senzory, správce jen přiřadí místnost. Kde čidla zatím
              nejsou, počítá model.
            </Step>
            <Step number={3} icon={ClipboardCheck} title="Audit a opatření">
              Pravidla najdou přetápění, špatné větrání a chybějící noční útlum. Kraj dostane pořadí budov podle ztráty, správce PDF audit a AI
              návrh opatření.
            </Step>
          </ol>
        </div>
      </section>

      {/* Funkce */}
      <section className="mx-auto max-w-6xl space-y-24 px-4 py-20 sm:px-6">
        <FeatureRow icon={MapIcon} eyebrow="Mapa kraje" title="Celý kraj na jedné mapě" image={{ src: "/landing/mapa-okresy.webp", alt: "Mapa okresů obarvených podle podílu budov se zjištěním", ...SHOT }}>
          <p>Nejdřív kraj po okresech, pak budovy v okrese, nakonec jedna budova ve 3D. Okresy mají barvu podle toho, kolik budov má problém.</p>
          <Bullets items={["Na kraji štítky okresů s počty a tečky budov", "Po výběru okresu špendlíky jen z něj — barva = stav, ikona = druh", "3D budova z katastru v barvě stavu"]} />
        </FeatureRow>

        <FeatureRow
          reversed
          icon={Building2}
          eyebrow="Inspekční panel"
          title="Co se v budově děje, na jeden pohled"
          image={{ src: "/landing/inspekce.webp", alt: "Inspekční panel s telemetrií, místnostmi a zjištěními auditu", ...SHOT }}
        >
          <p>Teplota za 48 hodin, spotřeba po dnech, provozní heatmapa a místnosti s aktuálním CO₂. Audit vysvětlí česky, co je špatně a proč.</p>
          <Bullets items={["Zjištění s důkazem z měření", "Karta budovy z ČÚZK a obsazenost z dat kraje", "Památková ochrana a dotace v okolí"]} />
        </FeatureRow>

        <FeatureRow
          icon={RadioTower}
          eyebrow="Skutečná čidla IQRF"
          title="Připojení čidla za pět minut"
          image={{ src: "/landing/senzory.webp", alt: "Obrazovka Senzory s kanály IQRF gatewaye a živými hodnotami", ...SHOT }}
        >
          <p>IQRF gateway se přes MQTT připojí k aplikaci. Ta sama najde spárované uzly a jejich senzory, správce jen vybere místnost.</p>
          <Bullets items={["Teplota, CO₂, vlhkost, osvětlení, hluk, pohyb", "Živé hodnoty a průběh v průvodci", "Skutečné čidlo nahradí v místnosti simulaci"]} />
        </FeatureRow>

        <FeatureRow
          reversed
          icon={FileDown}
          eyebrow="Reporty a AI"
          title="Podklad pro rozhodnutí, ne další dashboard"
          image={{ src: "/landing/pdf.webp", alt: "PDF audit objektu s grafy a mapou okresů", ...SHOT }}
        >
          <p>PDF audit budovy s grafy a přehled portfolia s mapou okresů. Azure OpenAI navrhne opatření s návratností a dopis zřizovateli.</p>
          <Bullets items={["Audit objektu pro správce budovy", "Portfolio kraje nebo okresu pro investiční odbor", "AI opatření respektují památkovou ochranu"]} />
        </FeatureRow>

        <FeatureRow
          icon={Smartphone}
          eyebrow="Pro veřejnost"
          title="Jak se dýchá ve škole mého dítěte?"
          image={{ src: "/landing/verejna-karta.webp", alt: "Veřejná karta objektu s kvalitou vzduchu a místnostmi", ...SHOT }}
        >
          <p>
            Rodiče, žáci, senioři a jejich blízcí najdou svou budovu bez přihlášení: jaký je vzduch teď, v kterých místnostech, kdy větrat a co s
            tím kraj dělá. QR kód na dveře třídy vede rovnou na kartu.
          </p>
          <Link href="/objekty" className="inline-flex items-center gap-1.5 font-bold text-hk-red hover:underline">
            Najít svou školu nebo domov
            <ArrowRight className="size-4" aria-hidden />
          </Link>
        </FeatureRow>

        <FeatureRow
          reversed
          icon={Braces}
          eyebrow="Otevřená data zpět"
          title="Co SmartAudit zjistí, vrací jako otevřená data"
          image={{ src: "/landing/otevrene-api.webp", alt: "Stránka otevřeného API s endpointy a ukázkami", ...SHOT }}
        >
          <p>Objekty, půdorysy budov a měření jsou k dispozici bez klíče jako JSON, GeoJSON a CSV pod licencí CC BY 4.0.</p>
          <a href="/api/open/docs/" className="inline-flex items-center gap-1.5 font-bold text-hk-red hover:underline">
            Dokumentace otevřeného API
            <ArrowRight className="size-4" aria-hidden />
          </a>
        </FeatureRow>
      </section>

      {/* Pro koho */}
      <section className="bg-slate-50">
        <div className="mx-auto max-w-6xl px-4 py-16 sm:px-6">
          <h2 className="text-[30px] font-bold tracking-tight">Pro koho</h2>
          <div className="mt-8 grid gap-4 md:grid-cols-3">
            <Audience icon={Landmark} title="Kraj" items={["Kde plýtvá a kolik to stojí", "Pořadí investic podle ztráty", "Přehled celého portfolia v PDF"]} />
            <Audience icon={Building2} title="Správce budovy" items={["Co je v budově špatně a proč", "Připojení vlastních čidel IQRF", "Audit a návrh opatření s návratností"]} />
            <Audience icon={Users} title="Veřejnost" items={["Kvalita vzduchu ve škole nebo domově", "Kdy větrat, co kraj řeší", "QR kód na dveřích třídy"]} />
          </div>
        </div>
      </section>

      {/* Výzva */}
      <section className="bg-hk-navy text-white">
        <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-6 px-4 py-14 sm:px-6">
          <div>
            <h2 className="text-[28px] font-bold tracking-tight">Vyzkoušejte si to</h2>
            <p className="mt-1 text-white/70">Demo bez registrace pro kraj, správce budovy i veřejnost.</p>
          </div>
          <div className="flex flex-wrap gap-3">
            <Link href="/login" className="inline-flex h-12 items-center gap-2 rounded-xl bg-hk-accent px-5 text-[15px] font-bold transition-colors hover:bg-hk-red">
              Vyzkoušet demo
              <ArrowRight className="size-4" aria-hidden />
            </Link>
            <Link href="/objekty" className="inline-flex h-12 items-center gap-2 rounded-xl border border-white/25 px-5 text-[15px] font-bold transition-colors hover:bg-white/10">
              Najít svou školu
            </Link>
          </div>
        </div>
      </section>
    </main>
  );
}

type StepProps = { number: number; icon: React.ComponentType<{ className?: string }>; title: string; children: React.ReactNode };

function Step({ number, icon: Icon, title, children }: StepProps) {
  return (
    <li className="rounded-2xl border border-slate-200 bg-white p-6">
      <div className="flex items-center gap-3">
        <span className="flex size-10 items-center justify-center rounded-xl bg-hk-navy text-white">
          <Icon className="size-5" />
        </span>
        <span className="font-mono text-[12px] text-slate-400">0{number}</span>
      </div>
      <h3 className="mt-4 text-[18px] font-bold">{title}</h3>
      <p className="mt-2 text-[14px] leading-relaxed text-slate-600">{children}</p>
    </li>
  );
}

function Audience({ icon: Icon, title, items }: { icon: React.ComponentType<{ className?: string }>; title: string; items: string[] }) {
  return (
    <div className="rounded-2xl border border-slate-200 bg-white p-6">
      <Icon className="size-6 text-hk-red" />
      <h3 className="mt-3 text-[18px] font-bold">{title}</h3>
      <div className="mt-3 text-[14px] text-slate-600">
        <Bullets items={items} />
      </div>
    </div>
  );
}
