"use client";

import { Printer } from "lucide-react";

export function PrintButton() {
  return (
    <button
      type="button"
      onClick={() => window.print()}
      className="inline-flex h-10 items-center gap-2 rounded-lg bg-hk-navy px-4 text-[13.5px] font-bold text-white transition-colors hover:bg-hk-blue print:hidden"
    >
      <Printer className="size-4" aria-hidden />
      Vytisknout
    </button>
  );
}
