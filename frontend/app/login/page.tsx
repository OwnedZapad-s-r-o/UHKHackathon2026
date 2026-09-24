import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { redirect } from "next/navigation";
import { ArrowLeft, ArrowRight, Check, Search, Users } from "lucide-react";
import { getSession } from "@/lib/api";
import { DemoButtons } from "./_components/demo-buttons";
import { LoginForm } from "./_components/login-form";

export const metadata: Metadata = {
  title: "Přihlášení · SmartAudit KHK",
};

const PROMISES = ["Kde kraj zbytečně topí a kolik to stojí", "Kde se ve třídách špatně dýchá", "Co s tím udělat — PDF a návrh opatření"];

// Přihlášení = rozcestník: kraj a správce jdou do dema (bez registrace), veřejnost na hledání budov bez přihlášení.
// Kdo má účet, rozbalí přihlášení e-mailem. Po vstupu do aplikace provede nového uživatele průvodce.
export default async function LoginPage() {
  if (await getSession()) redirect("/mapa");

  return (
    <main className="flex min-h-0 flex-1 overflow-y-auto bg-white text-hk-navy">
      {/* Levá strana: co aplikace umí — jen na širokých obrazovkách */}
      <aside className="relative hidden w-[44%] max-w-160 shrink-0 flex-col overflow-hidden bg-hk-navy px-12 pt-10 text-white lg:flex">
        <Link href="/" className="inline-flex w-fit items-center gap-1.5 text-[13px] text-white/60 transition-colors hover:text-white">
          <ArrowLeft className="size-3.5" aria-hidden />
          Zpět na úvod
        </Link>
        <div className="mt-10 flex items-center gap-4">
          <Image src="/brand/hk-logo-white.png" alt="Hradec Králové" width={330} height={142} priority className="h-11 w-auto" />
          <span className="leading-tight">
            <span className="block text-[17px] font-bold">SmartAudit</span>
            <span className="block text-[12px] text-white/60">Královéhradecký kraj</span>
          </span>
        </div>
        <h1 className="mt-10 max-w-[18ch] text-[clamp(30px,2.6vw,40px)] leading-[1.1] font-bold tracking-[-0.03em]">
          Energetický audit budov kraje na jedné mapě
        </h1>
        <ul className="mt-6 space-y-2.5 text-[15px] text-white/80">
          {PROMISES.map((promise) => (
            <li key={promise} className="flex gap-2.5">
              <Check className="mt-0.5 size-4 shrink-0 text-hk-red" aria-hidden />
              {promise}
            </li>
          ))}
        </ul>
        <div className="relative mt-auto pt-8">
          {/* Ambientní záře za oknem pro vizuální odtržení */}
          <div className="absolute -inset-1 rounded-3xl bg-gradient-to-tr from-hk-accent/35 via-blue-500/25 to-sky-400/10 blur-xl" aria-hidden />

          {/* Rám okna aplikace jako ucelená karta s kompletními zaoblenými rohy a světlejší lištou proti splývání */}
          <div className="relative overflow-hidden rounded-2xl border border-white/30 bg-slate-900 shadow-[0_20px_60px_-15px_rgba(0,0,0,0.8)] ring-1 ring-white/20">
            {/* Horní lišta okna (světlejší styl jako macOS Safari pro jasné ohraničení tmavého panelu) */}
            <div className="flex items-center justify-between border-b border-slate-300/80 bg-slate-200/95 px-4 py-2.5 backdrop-blur-md">
              <div className="flex items-center gap-1.5">
                <span className="size-2.5 rounded-full bg-[#ff5f56]" />
                <span className="size-2.5 rounded-full bg-[#ffbd2e]" />
                <span className="size-2.5 rounded-full bg-[#27c93f]" />
              </div>
              <div className="flex items-center gap-1.5 rounded-full border border-slate-300 bg-white px-3 py-0.5 text-[11px] font-medium text-slate-700 shadow-xs">
                <span className="size-1.5 rounded-full bg-emerald-500" />
                <span>smartaudit.khk.cz/mapa</span>
              </div>
              <div className="w-10" />
            </div>

            {/* Obrázek orámovaný celistvě bez ořezu */}
            <div className="relative overflow-hidden bg-slate-950">
              <Image
                src="/landing/mapa-okresy.webp"
                alt="Ukázka aplikace SmartAudit KHK"
                width={1440}
                height={900}
                sizes="520px"
                priority
                className="h-auto w-full block"
              />
            </div>
          </div>
        </div>
      </aside>

      {/* Pravá strana: jak začít */}
      <div className="flex flex-1 items-start justify-center px-5 py-10 sm:px-8 lg:items-center">
        <div className="w-full max-w-md">
          <div className="flex items-center gap-3 lg:hidden">
            <Image src="/brand/hk-logo-white.png" alt="Hradec Králové" width={330} height={142} priority className="h-9 w-auto invert" />
            <span className="leading-tight">
              <span className="block text-[15px] font-bold">SmartAudit</span>
              <span className="block text-[11px] text-slate-500">Královéhradecký kraj</span>
            </span>
          </div>

          <h2 className="mt-8 text-[28px] leading-tight font-bold tracking-[-0.02em] lg:mt-0">Vyberte, kdo jste</h2>
          <p className="mt-1.5 text-[14px] leading-relaxed text-slate-600">
            Demo je bez registrace a ukáže, co je pro vás důležité. Po vstupu vás aplikací provede krátký průvodce.
          </p>

          <DemoButtons />

          {/* Veřejnost nepotřebuje účet — kvalita vzduchu je veřejná */}
          <div className="mt-3 rounded-2xl border border-amber-200 bg-amber-50 p-4">
            <div className="flex gap-3">
              <span className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-white text-amber-800">
                <Users className="size-5" aria-hidden />
              </span>
              <div className="min-w-0">
                <p className="text-[15px] font-bold">Jsem rodič, žák nebo senior</p>
                <p className="mt-0.5 text-[13px] leading-snug text-amber-900">Jak se dýchá ve škole nebo domově zjistíte bez přihlášení.</p>
              </div>
            </div>
            <Link
              href="/objekty"
              className="mt-3.5 flex h-12 items-center justify-center gap-2 rounded-xl bg-hk-navy text-[14px] font-bold text-white transition-colors hover:bg-hk-blue"
            >
              <Search className="size-4" aria-hidden />
              Najít svou školu nebo domov
            </Link>
            <DemoButtons publicOnly />
          </div>

          <details className="group mt-6 rounded-2xl border border-slate-200 open:bg-slate-50/60">
            <summary className="flex cursor-pointer list-none items-center justify-between gap-3 px-4 py-3.5 text-[14px] font-bold select-none [&::-webkit-details-marker]:hidden">
              Mám účet — přihlásit e-mailem
              <ArrowRight className="size-4 text-slate-400 transition-transform group-open:rotate-90" aria-hidden />
            </summary>
            <div className="border-t border-slate-200 px-4 pt-4 pb-4">
              <LoginForm />
            </div>
          </details>

          <p className="mt-6 text-center text-[12.5px] text-slate-500">
            Data jsou i jako otevřené API:{" "}
            <a href="/api/open/docs/" className="font-bold text-hk-navy underline underline-offset-2 hover:text-hk-red">
              dokumentace a stažení
            </a>
          </p>
        </div>
      </div>
    </main>
  );
}
