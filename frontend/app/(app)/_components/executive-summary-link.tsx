import { Landmark } from "lucide-react";

// Stažení jednostránkového souhrnu pro Radu kraje — odkaz na route handler se souborem, ne na stránku
const SUMMARY_URL = "/reporty/souhrn";

export function ExecutiveSummaryLink({ className }: { className?: string }) {
  return (
    <a
      href={SUMMARY_URL}
      className={
        className ??
        "inline-flex h-10 items-center gap-2 rounded-lg border border-slate-300 bg-white px-4 text-sm font-bold text-hk-navy transition-colors hover:bg-slate-50"
      }
    >
      <Landmark className="size-4" aria-hidden />
      Souhrn pro Radu (A4)
    </a>
  );
}
