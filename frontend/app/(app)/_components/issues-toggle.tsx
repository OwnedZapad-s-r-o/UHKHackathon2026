"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Filter } from "lucide-react";
import { cn } from "@/lib/utils";

// Přepínač „jen objekty se zjištěním“ — stav žije v URL (?zjisteni=1), platí pro mapu, portfolio i anomálie
export function IssuesToggle() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const active = searchParams.get("zjisteni") === "1";

  function toggle() {
    const params = new URLSearchParams(searchParams);
    if (active) params.delete("zjisteni");
    else params.set("zjisteni", "1");

    const query = params.toString();
    router.push(query ? `${pathname}?${query}` : pathname);
  }

  return (
    <button
      type="button"
      role="switch"
      aria-checked={active}
      onClick={toggle}
      className={cn(
        "flex h-10.5 items-center gap-3 rounded-lg px-3 text-left text-sm transition-colors hover:text-white",
        active ? "bg-neutral-900 text-white" : "text-white/55",
      )}
    >
      <Filter className="size-4.5" strokeWidth={1.75} aria-hidden />
      <span className="flex-1">Jen se zjištěním</span>
      <span className={cn("flex h-4 w-7 items-center rounded-full p-0.5 transition-colors", active ? "justify-end bg-hk-red" : "justify-start bg-neutral-800")}>
        <span className="size-3 rounded-full bg-white" />
      </span>
    </button>
  );
}
