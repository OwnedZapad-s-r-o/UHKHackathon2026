import type { ReactNode } from "react";
import { cn } from "@/lib/utils";

type PanelCardProps = {
  title?: string;
  meta?: string;
  className?: string;
  children: ReactNode;
};

// Bílá karta inspekčního panelu (prototyp: radius 16, border slate-200, padding 18)
export function PanelCard({ title, meta, className, children }: PanelCardProps) {
  return (
    <section className={cn("mt-3.5 rounded-2xl border border-slate-200 bg-white p-4.5", className)}>
      {title && (
        <div className="mb-3 flex items-baseline justify-between gap-2.5">
          <h2 className="text-lg leading-snug font-bold tracking-tight">{title}</h2>
          {meta && <span className="font-mono text-[10.5px] text-slate-500">{meta}</span>}
        </div>
      )}
      {children}
    </section>
  );
}

type StatTileProps = {
  label: string;
  value: string;
  note?: string;
  tone?: "default" | "warning" | "critical";
};

export function StatTile({ label, value, note, tone = "default" }: StatTileProps) {
  return (
    <div className="rounded-xl border border-slate-200 bg-white p-3.5">
      <p className="text-[11px] text-slate-500">{label}</p>
      <p className={cn("mt-1 font-mono text-xl font-bold", tone === "critical" && "text-red-800", tone === "warning" && "text-amber-800")}>{value}</p>
      {note && <p className="mt-0.5 text-[11px] text-slate-400">{note}</p>}
    </div>
  );
}
