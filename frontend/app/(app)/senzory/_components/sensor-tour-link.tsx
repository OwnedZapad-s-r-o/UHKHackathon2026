import Link from "next/link";
import { Compass } from "lucide-react";

// Spustí průvodce nastavením čidel na aktuální stránce (Senzory nebo detail gatewaye)
export function SensorTourLink({ href }: { href: `/senzory?pruvodce=1` | `/senzory/${string}?pruvodce=1` }) {
  return (
    <Link
      href={href}
      scroll={false}
      className="inline-flex h-9 items-center gap-2 rounded-lg border border-slate-300 bg-white px-3 text-[13px] font-bold text-hk-navy transition-colors hover:bg-slate-50"
    >
      <Compass className="size-4 text-hk-red" aria-hidden />
      Průvodce nastavením čidel
    </Link>
  );
}
