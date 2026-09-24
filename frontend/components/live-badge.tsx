import { cn } from "@/lib/utils";

// Hodnota ze skutečného čidla IQRF, ne ze simulace
export function LiveBadge({ className }: { className?: string }) {
  return (
    <span className={cn("inline-flex shrink-0 items-center gap-1 rounded-full bg-emerald-600 px-1.5 py-px text-[9px] font-bold tracking-wide text-white uppercase", className)}>
      <span className="size-1 animate-pulse rounded-full bg-white" aria-hidden />
      živě
    </span>
  );
}
