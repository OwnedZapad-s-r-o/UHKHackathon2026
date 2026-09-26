"use client";

import { useState, useTransition } from "react";
import Link from "next/link";
import { Loader2, PenLine, Replace, Trash2 } from "lucide-react";
import { deleteBuildingPart } from "../building-actions";

export type BuildingPartItem = {
  id: string;
  areaSquareMeters: number;
  source: string;
  replaceHref: string;
  outlineHref: string;
};

// Doplněné přilehlé budovy: vyměnit za jinou z mapy, upravit obrys, odebrat (plocha přestane vstupovat do ztráty)
export function BuildingPartsList({ facilityId, parts }: { facilityId: string; parts: BuildingPartItem[] }) {
  return (
    <ul className="mt-3 divide-y divide-slate-100 rounded-xl border border-slate-200">
      {parts.map((part, index) => (
        <PartRow key={part.id} facilityId={facilityId} part={part} index={index} />
      ))}
    </ul>
  );
}

function PartRow({ facilityId, part, index }: { facilityId: string; part: BuildingPartItem; index: number }) {
  const [confirming, setConfirming] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  function remove() {
    setError(null);
    startTransition(async () => {
      const result = await deleteBuildingPart(facilityId, part.id);
      if (result?.error) setError(result.error);
    });
  }

  return (
    <li className="px-3.5 py-2.5">
      <div className="flex flex-wrap items-center gap-x-3 gap-y-1.5">
        <div className="min-w-0 flex-1">
          <p className="text-[13px] font-bold">
            Přilehlá budova {index + 1} · {part.areaSquareMeters.toLocaleString("cs-CZ")} m²
          </p>
          <p className="text-[11px] text-slate-500">{part.source}</p>
        </div>
        {!confirming && (
          <div className="flex items-center gap-1">
            <Link href={part.replaceHref} scroll={false} title="Vybrat v mapě jinou budovu" className={ACTION}>
              <Replace className="size-3.5" aria-hidden />
              Vyměnit
            </Link>
            <Link href={part.outlineHref} scroll={false} title="Upravit obrys tažením bodů" className={ACTION}>
              <PenLine className="size-3.5" aria-hidden />
              Obrys
            </Link>
            <button type="button" onClick={() => setConfirming(true)} aria-label="Odebrat doplněnou budovu" className={`${ACTION} hover:bg-red-50 hover:text-red-700`}>
              <Trash2 className="size-3.5" aria-hidden />
            </button>
          </div>
        )}
      </div>

      {confirming && (
        <div role="alertdialog" aria-label="Odebrat doplněnou budovu" className="mt-2 flex flex-wrap items-center gap-2 rounded-lg bg-red-50 px-3 py-2">
          <p className="flex-1 text-xs text-red-900">Odebrat? Její plocha přestane vstupovat do výpočtu ztráty.</p>
          <button
            type="button"
            onClick={remove}
            disabled={isPending}
            className="inline-flex h-8 items-center gap-1.5 rounded-lg bg-red-600 px-3 text-[12.5px] font-bold text-white hover:bg-red-700 disabled:opacity-60"
          >
            {isPending && <Loader2 className="size-3.5 animate-spin" aria-hidden />}
            Odebrat
          </button>
          <button type="button" onClick={() => setConfirming(false)} disabled={isPending} className="h-8 rounded-lg px-2.5 text-[12.5px] font-bold text-slate-600 hover:bg-white">
            Ponechat
          </button>
        </div>
      )}

      {error && (
        <p role="alert" className="mt-1.5 text-xs font-bold text-red-700">
          {error}
        </p>
      )}
    </li>
  );
}

const ACTION = "inline-flex h-8 items-center gap-1 rounded-lg px-2 text-[12px] font-bold text-slate-600 transition-colors hover:bg-slate-100 hover:text-hk-navy";
