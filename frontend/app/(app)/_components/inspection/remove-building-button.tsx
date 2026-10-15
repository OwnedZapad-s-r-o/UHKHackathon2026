"use client";

import { useState, useTransition } from "react";
import { Loader2, Trash2 } from "lucide-react";
import { deleteBuilding } from "../../budovy/actions";

// Jen u budovy, kterou správce přidal sám — kdyby se spletl v místě nebo ji přidal dvakrát.
// Potvrzení přímo v panelu: smaže se i s čidly a měřeními, vrátit to nejde.
export function RemoveBuildingButton({ facilityId, name }: { facilityId: string; name: string }) {
  const [confirming, setConfirming] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  function remove() {
    setError(null);
    startTransition(async () => {
      const result = await deleteBuilding(facilityId);
      if (result?.error) setError(result.error);
    });
  }

  if (!confirming) {
    return (
      <button
        type="button"
        onClick={() => setConfirming(true)}
        className="mt-3.5 inline-flex h-9 items-center gap-1.5 rounded-lg px-2.5 text-[12.5px] font-bold text-slate-500 transition-colors hover:bg-red-50 hover:text-red-700"
      >
        <Trash2 className="size-4" aria-hidden />
        Odebrat budovu z mapy
      </button>
    );
  }

  return (
    <div role="alertdialog" aria-label="Odebrat budovu" className="mt-3.5 rounded-2xl border border-red-200 bg-red-50 p-4">
      <p className="text-[13px] font-bold text-red-900">Odebrat „{name}“?</p>
      <p className="mt-1 text-xs text-red-800">Smažou se i čidla, naměřená data a vyhodnocení auditu. Vrátit to nejde.</p>
      {error && <p className="mt-2 text-xs font-bold text-red-800">{error}</p>}
      <div className="mt-3 flex gap-2">
        <button
          type="button"
          onClick={remove}
          disabled={isPending}
          className="inline-flex h-9 items-center gap-1.5 rounded-lg bg-red-600 px-3.5 text-[13px] font-bold text-white transition-colors hover:bg-red-700 disabled:opacity-60"
        >
          {isPending && <Loader2 className="size-4 animate-spin" aria-hidden />}
          Odebrat
        </button>
        <button type="button" onClick={() => setConfirming(false)} disabled={isPending} className="h-9 rounded-lg px-3.5 text-[13px] font-bold text-slate-600 hover:bg-white">
          Ponechat
        </button>
      </div>
    </div>
  );
}
