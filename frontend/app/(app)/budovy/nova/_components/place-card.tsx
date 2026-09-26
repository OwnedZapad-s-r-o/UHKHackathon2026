import Link from "next/link";
import { AlertTriangle, Building2, CircleAlert, Info, Landmark, Loader2, MapPinned } from "lucide-react";
import { formatNumber } from "@/lib/facility-meta";
import type { Place } from "@/types/api";
import { HERITAGE_LABEL } from "./building-options";

type PlaceCardProps = {
  place: Place | null;
  loading: boolean;
  error: string | null;
};

// Co aplikace o vybraném místě zjistila sama — správce nic z toho nemusí vyplňovat, jen zkontroluje
export function PlaceCard({ place, loading, error }: PlaceCardProps) {
  if (loading) {
    return (
      <p className="flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-3.5 py-3 text-[13px] text-slate-600" aria-live="polite">
        <Loader2 className="size-4 animate-spin text-hk-red" aria-hidden />
        Dohledávám adresu, obec a budovu v katastru…
      </p>
    );
  }

  if (error) {
    return (
      <p role="alert" className="flex items-start gap-2 rounded-xl border border-red-200 bg-red-50 px-3.5 py-3 text-[13px] text-red-800">
        <CircleAlert className="mt-0.5 size-4 shrink-0" aria-hidden />
        {error}
      </p>
    );
  }

  if (!place) return null;

  const building = place.building;
  const duplicate = place.neighbors.find((n) => n.sameBuilding && n.own);
  const shared = place.neighbors.find((n) => n.sameBuilding && !n.own);
  const nearby = place.neighbors.filter((n) => !n.sameBuilding);

  return (
    <div className="space-y-2.5" aria-live="polite">
      {duplicate && (
        <div role="alert" className="rounded-xl border border-red-200 bg-red-50 px-3.5 py-3 text-[13px] text-red-800">
          <p className="flex items-start gap-2 font-bold">
            <AlertTriangle className="mt-0.5 size-4 shrink-0" aria-hidden />
            Tuhle budovu už v mapě máte
          </p>
          <p className="mt-1 pl-6">
            {duplicate.name} ({duplicate.code}).{" "}
            <Link href={`/mapa?objekt=${duplicate.id}`} className="font-bold underline underline-offset-2">
              Otevřít v mapě
            </Link>
          </p>
        </div>
      )}

      <div className="rounded-xl border border-slate-200 bg-white">
        <div className="flex items-start gap-3 px-3.5 py-3">
          <MapPinned className="mt-0.5 size-4.5 shrink-0 text-hk-red" aria-hidden />
          <div className="min-w-0">
            <p className="text-[14px] leading-snug font-bold text-hk-navy">{place.address?.label ?? "Místo bez adresy"}</p>
            <p className="mt-0.5 text-xs text-slate-500">
              obec {place.territory.municipalityName} · ORP {place.territory.orpName} · okres {place.territory.districtName}
            </p>
          </div>
        </div>

        <div className="flex items-start gap-3 border-t border-slate-100 px-3.5 py-3">
          <Building2 className="mt-0.5 size-4.5 shrink-0 text-slate-400" aria-hidden />
          <p className="min-w-0 text-[13px] text-slate-600">
            {building ? (
              <>
                <span className="font-bold text-hk-navy">Budova nalezena v katastru ČÚZK</span>
                <span className="block text-xs text-slate-500">
                  {[
                    building.builtUpArea !== null ? `zastavěná plocha ${formatNumber(building.builtUpArea, "m²")}` : null,
                    building.constructionYear !== null ? `postavena ${building.constructionYear}` : "rok výstavby neuveden",
                  ]
                    .filter(Boolean)
                    .join(" · ")}{" "}
                  — v mapě ji zvýrazňuje červený obrys
                </span>
              </>
            ) : place.cadastreAvailable ? (
              "V katastru jsme tu budovu nenašli. Klikněte přímo na střechu budovy — nebo pokračujte, audit použije odhad."
            ) : (
              "Katastr ČÚZK teď neodpovídá. Budovu můžete přidat i tak, půdorys se doplní později."
            )}
          </p>
        </div>

        {place.heritageStatus !== "None" && (
          <div className="flex items-start gap-3 border-t border-slate-100 px-3.5 py-3">
            <Landmark className="mt-0.5 size-4.5 shrink-0 text-amber-600" aria-hidden />
            <p className="min-w-0 text-[13px] text-slate-600">
              <span className="font-bold text-hk-navy">{HERITAGE_LABEL[place.heritageStatus]}</span>
              {place.heritageName && <span className="block text-xs text-slate-500">{place.heritageName} — audit s tím počítá u navrhovaných úprav</span>}
            </p>
          </div>
        )}
      </div>

      {shared && !duplicate && (
        <p className="flex items-start gap-2 rounded-xl border border-amber-200 bg-amber-50 px-3.5 py-3 text-[13px] text-amber-900">
          <Info className="mt-0.5 size-4 shrink-0" aria-hidden />
          <span>
            V téže budově už kraj eviduje <strong>{shared.name}</strong>. Pokud budovu sdílíte s jinou organizací, klidně pokračujte.
          </span>
        </p>
      )}

      {nearby.length > 0 && !duplicate && (
        <p className="px-1 text-xs text-slate-500">
          V okolí: {nearby.map((n) => `${n.name} (${formatNumber(n.distanceMeters, "m")})`).join(", ")}
        </p>
      )}
    </div>
  );
}
