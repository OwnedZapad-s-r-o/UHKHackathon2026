"use client";

import { useState, type MouseEvent, type ReactNode } from "react";
import { Menu, X } from "lucide-react";

// Mobilní lišta: postranní panel se na úzké obrazovce vysune zleva přes obsah
export function MobileNav({ children }: { children: ReactNode }) {
  const [open, setOpen] = useState(false);

  // Klik na odkaz v menu = přechod na jinou obrazovku, menu se zavře
  function closeOnNavigation(event: MouseEvent) {
    if ((event.target as HTMLElement).closest("a")) setOpen(false);
  }

  return (
    <div className="lg:hidden">
      <div className="flex h-12 items-center gap-3 bg-hk-navy px-3 text-white">
        <button
          type="button"
          onClick={() => setOpen(true)}
          aria-label="Otevřít menu"
          aria-expanded={open}
          className="flex size-9 items-center justify-center rounded-lg hover:bg-white/8"
        >
          <Menu className="size-5" />
        </button>
        <span className="text-[15px] font-bold tracking-tight">SmartAudit</span>
        <span className="text-[11px] text-white/55">Královéhradecký kraj</span>
      </div>

      {open && (
        <div className="fixed inset-0 z-50 flex">
          <div className="relative flex h-full animate-in slide-in-from-left duration-200" onClick={closeOnNavigation}>
            {children}
            <button
              type="button"
              onClick={() => setOpen(false)}
              aria-label="Zavřít menu"
              className="absolute top-3 right-3 flex size-9 items-center justify-center rounded-lg text-white/70 hover:bg-white/8 hover:text-white"
            >
              <X className="size-5" />
            </button>
          </div>
          <button type="button" aria-label="Zavřít menu" onClick={() => setOpen(false)} className="flex-1 bg-black/40 backdrop-blur-[1px]" />
        </div>
      )}
    </div>
  );
}
