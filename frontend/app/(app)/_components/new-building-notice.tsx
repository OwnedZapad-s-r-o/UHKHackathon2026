"use client";

import { useEffect } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { CheckCircle2, Loader2 } from "lucide-react";
import { withParams } from "@/lib/search-params";

// Simulace doplní čidla a týden historie do ~10 s, audit hned potom — do té doby se mapa sama obnovuje
const REFRESH_MS = 6_000;
const MAX_REFRESHES = 15;

// Jak dlouho zůstane vidět potvrzení, když už je budova vyhodnocená
const DONE_VISIBLE_MS = 6_000;

type NewBuildingNoticeProps = {
  name: string;
  // Budova ještě nemá první vyhodnocení auditu (čidla se teprve zakládají)
  waiting: boolean;
};

// Potvrzení po přidání budovy (?nova=1). Server komponenta mapy je statická, proto obnova přes router.refresh().
export function NewBuildingNotice({ name, waiting }: NewBuildingNoticeProps) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  useEffect(() => {
    if (waiting) {
      let refreshes = 0;
      const timer = setInterval(() => {
        refreshes++;
        router.refresh();
        if (refreshes >= MAX_REFRESHES) clearInterval(timer);
      }, REFRESH_MS);

      return () => clearInterval(timer);
    }

    const timer = setTimeout(() => router.replace(withParams(pathname, searchParams, { nova: null }), { scroll: false }), DONE_VISIBLE_MS);
    return () => clearTimeout(timer);
  }, [waiting, router, pathname, searchParams]);

  return (
    <div role="status" className="absolute top-3 left-1/2 z-20 w-[min(26rem,calc(100%-1.5rem))] -translate-x-1/2 rounded-xl border border-emerald-200 bg-white px-4 py-3 shadow-[0_12px_32px_rgba(11,25,44,0.16)]">
      <p className="flex items-center gap-2 text-[13px] font-bold text-hk-navy">
        <CheckCircle2 className="size-4.5 shrink-0 text-emerald-500" aria-hidden />
        <span className="truncate">{name} je v mapě</span>
      </p>
      <p className="mt-1 flex items-center gap-2 pl-6.5 text-xs text-slate-600">
        {waiting ? (
          <>
            <Loader2 className="size-3.5 shrink-0 animate-spin text-slate-400" aria-hidden />
            Zakládám čidla a dopočítávám týden historie, první vyhodnocení auditu bude do minuty…
          </>
        ) : (
          "Audit budovu vyhodnotil. Skutečná čidla můžete připojit v Senzorech."
        )}
      </p>
    </div>
  );
}
