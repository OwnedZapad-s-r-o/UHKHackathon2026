"use client";

import { useState } from "react";
import { Check, Copy } from "lucide-react";

// Zkopíruje ukázku volání API do schránky; potvrzení se na chvíli ukáže místo ikony
export function CopyButton({ text, label = "Kopírovat" }: { text: string; label?: string }) {
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
      className="inline-flex h-7 items-center gap-1.5 rounded-md border border-white/15 px-2 text-[11px] text-white/70 transition-colors hover:bg-white/10 hover:text-white"
    >
      {copied ? <Check className="size-3.5" aria-hidden /> : <Copy className="size-3.5" aria-hidden />}
      <span aria-live="polite">{copied ? "Zkopírováno" : label}</span>
    </button>
  );
}
