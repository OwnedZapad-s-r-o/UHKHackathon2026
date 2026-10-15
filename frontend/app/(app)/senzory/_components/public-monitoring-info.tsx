import Link from "next/link";
import { ArrowRight, Map as MapIcon, Radio, ShieldCheck, Wind } from "lucide-react";

const HOW = [
  { icon: Radio, title: "Čidla v místnostech", text: "Měří teplotu, CO₂ a vlhkost ve třídách, jídelnách a společných prostorách." },
  { icon: ShieldCheck, title: "Šifrované spojení", text: "Gateway v budově posílá hodnoty kraji šifrovaně a smí jen za svou budovu." },
  { icon: Wind, title: "Co z toho máte vy", text: "U každé budovy vidíte, jaký je teď vzduch a kdy je čas vyvětrat." },
];

// Veřejnost připojovat čidla nemůže — místo slepé uličky vysvětlení, jak monitoring funguje, a cesta k výsledkům
export function PublicMonitoringInfo() {
  return (
    <div className="max-w-3xl">
      <h1 className="text-[28px] leading-tight font-bold tracking-tight">Jak kraj měří vnitřní prostředí škol</h1>
      <p className="mt-1.5 text-sm leading-relaxed text-slate-600">
        Čidla připojuje kraj a správci budov. Vy vidíte jejich výsledky — bez přihlášení i na veřejné kartě každé budovy.
      </p>

      <ol className="mt-6 grid gap-3 md:grid-cols-3">
        {HOW.map(({ icon: Icon, title, text }, index) => (
          <li key={title} className="rounded-2xl border border-slate-200 bg-white p-4">
            <span className="flex items-center justify-between">
              <span className="flex size-10 items-center justify-center rounded-xl bg-[#EFF4FA] text-hk-blue">
                <Icon className="size-5" aria-hidden />
              </span>
              <span className="font-mono text-[12px] text-slate-400">0{index + 1}</span>
            </span>
            <p className="mt-3 text-[15px] font-bold">{title}</p>
            <p className="mt-1 text-[13px] leading-snug text-slate-600">{text}</p>
          </li>
        ))}
      </ol>

      <div className="mt-6 flex flex-wrap gap-3">
        <Link
          href="/mapa"
          className="inline-flex h-12 items-center gap-2 rounded-lg bg-hk-red px-5 text-sm font-bold text-white transition-colors hover:bg-hk-red-dark"
        >
          <MapIcon className="size-4" aria-hidden />
          Najít budovu v mapě
        </Link>
        <Link
          href="/objekty"
          className="inline-flex h-12 items-center gap-2 rounded-lg border border-slate-300 bg-white px-5 text-sm font-bold text-hk-navy transition-colors hover:bg-slate-50"
        >
          Kvalita vzduchu ve školách
          <ArrowRight className="size-4" aria-hidden />
        </Link>
      </div>
    </div>
  );
}
