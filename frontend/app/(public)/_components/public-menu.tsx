"use client";

import { useState, type MouseEvent } from "react";
import { Menu, X } from "lucide-react";
import { PUBLIC_LINKS } from "./public-links";

// Úzká obrazovka: navigace z hlavičky se schová pod tlačítko a rozbalí se pod hlavičkou
export function PublicMenu() {
  const [open, setOpen] = useState(false);

  function closeOnNavigation(event: MouseEvent) {
    if ((event.target as HTMLElement).closest("a")) setOpen(false);
  }

  return (
    <div className="md:hidden">
      <button
        type="button"
        onClick={() => setOpen((value) => !value)}
        aria-label={open ? "Zavřít menu" : "Otevřít menu"}
        aria-expanded={open}
        className="flex size-10 items-center justify-center rounded-[10px] border border-slate-200 bg-white text-hk-navy"
      >
        {open ? <X className="size-4.5" /> : <Menu className="size-4.5" />}
      </button>

      {open && (
        <nav
          onClick={closeOnNavigation}
          aria-label="Veřejná navigace"
          className="absolute inset-x-0 top-full flex flex-col border-t border-slate-200 bg-white px-4 pt-2 pb-4 shadow-[0_16px_32px_-16px_rgba(11,25,44,0.25)]"
        >
          {PUBLIC_LINKS.map((link) => (
            <a key={link.label} href={link.href} className="border-b border-slate-100 px-2 py-3.5 text-[16px] text-hk-navy last:border-b-0">
              {link.label}
            </a>
          ))}
        </nav>
      )}
    </div>
  );
}
