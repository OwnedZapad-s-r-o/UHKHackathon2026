"use client";

import { useState } from "react";
import { Check, Copy } from "lucide-react";
import { cn } from "@/lib/utils";

// Zkopíruje text do schránky; potvrzení se na chvíli ukáže místo ikony. tone = podklad: tmavý blok kódu nebo světlá karta
export function CopyButton({ text, label = "Kopírovat", tone = "dark" }: { text: string; label?: string; tone?: "dark" | "light" }) {
  const [copied, setCopied] = useState(false);

  async function copy() {
    try {
      await navigator.clipboard.writeText(text);
      setCopied(true);
      setTimeout(() => setCopied(false), 1600);
    } catch {
      setCopied(false);
    }
  }

  return (
    <button
      type="button"
      onClick={copy}
      className={cn(
        "inline-flex h-7 items-center gap-1.5 rounded-md border px-2 text-[11px] transition-colors",
        tone === "dark" ? "border-white/15 text-white/70 hover:bg-white/10 hover:text-white" : "border-slate-300 bg-white text-slate-600 hover:bg-slate-100 hover:text-hk-navy",
      )}
    >
      {copied ? <Check className="size-3.5" aria-hidden /> : <Copy className="size-3.5" aria-hidden />}
      <span aria-live="polite">{copied ? "Zkopírováno" : label}</span>
    </button>
  );
}
