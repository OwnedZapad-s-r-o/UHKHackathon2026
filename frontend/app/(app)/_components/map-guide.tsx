"use client";

import { useCallback } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Banknote, Flame, Map as MapIcon, Radio, Wind, Wrench } from "lucide-react";
import { GuidePanel, type GuideStarter } from "@/components/guide/guide-panel";
import { showGuideHighlights } from "@/lib/guide-events";
import { requestMapHome } from "@/lib/map-events";
import { withParams } from "@/lib/search-params";
import type { GuideAnswer, GuideFacilityCard, Role } from "@/types/api";

const STARTERS: Record<"Kraj" | "Spravce", GuideStarter[]> = {
  Kraj: [
    { icon: Banknote, label: "Kde kraj ztrácí nejvíc peněz", question: "Které budovy mají nejvyšší odhad roční ztráty?" },
    { icon: MapIcon, label: "Srovnej okresy", question: "Který okres má nejvyšší podíl budov se zjištěním?" },
    { icon: Flame, label: "Budovy, které se přetápí", question: "Které budovy se přetápí mimo provoz?" },
    { icon: Wind, label: "Kde se nejhůř dýchá", question: "Ve kterých školách je nejvíc hodin nad limitem CO₂?" },
  ],
  Spravce: [
    { icon: Flame, label: "Co řešit v mých budovách", question: "Které moje budovy mají zjištění a proč?" },
    { icon: Banknote, label: "Kolik mě to stojí", question: "Jaký je odhad roční ztráty mých budov?" },
    { icon: Radio, label: "Jak připojit čidla", question: "Jak připojím IQRF gateway a čidla?" },
    { icon: Wrench, label: "Co s tím udělat", question: "Jaká opatření bez investice pomůžou nejvíc?" },
  ],
};

// Prodleva před ukázáním očíslovaných objektů, když se zároveň mění URL — mapa nejdřív dokončí přechod z okresu nebo objektu
const AFTER_NAVIGATION_MS = 450;

type MapGuideProps = {
  role: Role;
  // Název vybraného objektu — průvodce ho nabídne jako kontext („tady“)
  selectedName: string | null;
  // Vlevo nahoře je drobečková navigace okresů, panel průvodce začne pod ní
  belowBreadcrumb: boolean;
};

// Průvodce na mapě aplikace: odpovědi rovnou řídí mapu přes URL (okres, objekt, záložka panelu)
// a očíslované špendlíky (GuideHighlights uvnitř FacilityMap)
export function MapGuide({ role, selectedName, belowBreadcrumb }: MapGuideProps) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  const navigate = useCallback(
    (changes: Record<string, string | null>) => {
      const changed = Object.entries(changes).some(([key, value]) => searchParams.get(key) !== value);
      if (changed) router.push(withParams(pathname, searchParams, changes), { scroll: false });
      return changed;
    },
    [pathname, router, searchParams],
  );

  const applyAnswer = useCallback(
    (answer: GuideAnswer) => {
      const action = answer.mapAction;
      if (!action) return;

      const cards = answer.facilities;

      switch (action.type) {
        case "Facility": {
          const card = cards[0];
          if (!card) return;
          showGuideHighlights(null);
          navigate({ okres: role === "Spravce" ? null : card.districtCode, objekt: card.id, panel: action.panel ?? searchParams.get("panel") });
          break;
        }
        case "Facilities": {
          // Všechny v jednom okrese → vybrat ho (mapa ukáže jeho špendlíky), jinak pohled na kraj
          const districts = new Set(cards.map((c) => c.districtCode));
          const district = role !== "Spravce" && districts.size === 1 ? cards[0].districtCode : null;
          const moved = navigate({ okres: district, objekt: null, panel: null });
          const highlights = cards.map((c) => ({ id: c.id, rank: c.rank, latitude: c.latitude, longitude: c.longitude, status: c.status, name: c.name, districtCode: c.districtCode }));
          if (moved) setTimeout(() => showGuideHighlights(highlights), AFTER_NAVIGATION_MS);
          else showGuideHighlights(highlights);
          break;
        }
        case "District":
          showGuideHighlights(null);
          if (action.districtCode && role !== "Spravce") navigate({ okres: action.districtCode, objekt: null, panel: null });
          break;
        case "Region":
          showGuideHighlights(null);
          requestMapHome();
          navigate({ okres: null, objekt: null, panel: null });
          break;
      }
    },
    [navigate, role, searchParams],
  );

  function openFacility(card: GuideFacilityCard) {
    navigate({ okres: role === "Spravce" ? null : card.districtCode, objekt: card.id, panel: card.status === "Nominal" ? "telemetrie" : "audit" });
  }

  return (
    <GuidePanel
      context={{ page: "mapa", facilityId: searchParams.get("objekt"), districtCode: searchParams.get("okres") }}
      contextLabel={selectedName}
      starters={role === "Spravce" ? STARTERS.Spravce : STARTERS.Kraj}
      storageKey={`smartaudit:pruvodce-ai:v2:${role}`}
      variant="map"
      panelClassName={belowBreadcrumb ? "sm:top-16" : undefined}
      onAnswer={applyAnswer}
      onOpenFacility={openFacility}
      onOpenDistrict={(code) => {
        showGuideHighlights(null);
        if (role !== "Spravce") navigate({ okres: code, objekt: null, panel: null });
      }}
      onReset={() => showGuideHighlights(null)}
    />
  );
}
