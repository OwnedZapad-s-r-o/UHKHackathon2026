import { FileDown } from "lucide-react";

// Tlačítko „Report portfolia (PDF)“ do horního pruhu — červené jako „Generovat AI Audit“ v prototypu
export function PortfolioReportLink({ onlyIssues }: { onlyIssues: boolean }) {
  return (
    <a
      href={onlyIssues ? "/reporty/portfolio?zjisteni=1" : "/reporty/portfolio"}
      className="inline-flex h-10 items-center gap-2 rounded-lg bg-hk-red px-4 text-sm font-bold text-white transition-colors hover:bg-hk-red-dark"
    >
      <FileDown className="size-4" aria-hidden />
      Report portfolia (PDF)
    </a>
  );
}
