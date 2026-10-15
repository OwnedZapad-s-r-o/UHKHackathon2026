"use client";

import { useCallback } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Clock, Info, School, Wind } from "lucide-react";
import { GuidePanel, type GuideStarter } from "@/components/guide/guide-panel";
import { showGuideHighlights } from "@/lib/guide-events";
import { withParams } from "@/lib/search-params";
import type { GuideAnswer, GuideFacilityCard, GuidePage } from "@/types/api";

const STARTERS: GuideStarter[] = [
  { icon: Wind, label: "Kde se teď dýchá nejhůř", question: "Ve kterých budovách je teď nejhorší vzduch?" },
  { icon: School, label: "Najdi mou školu", question: "Jak se dýchá ve školách v Hradci Králové?" },
  { icon: Info, label: "Co znamená CO₂", question: "Co znamená CO₂ a proč je limit 1 500 ppm?" },
  { icon: Clock, label: "Jak správně větrat", question: "Jak a kdy správně větrat ve třídě?" },
];

const STARTERS_HERE: GuideStarter[] = [
  { icon: Wind, label: "Jak se tu dýchá", question: "Jak se dýchá v této budově?" },
  { icon: Clock, label: "Kdy tu větrat", question: "Kdy a jak tady větrat?" },
  { icon: Info, label: "Co znamená CO₂", question: "Co znamená CO₂ a proč je limit 1 500 ppm?" },
  { icon: School, label: "Podobné budovy v okolí", question: "Jak se dýchá v dalších budovách v tomto okrese?" },
];

type PublicGuideProps = {
  page: GuidePage;
  // Karta objektu: na „tady“ se myslí tenhle objekt
  facility?: { id: string; name: string } | null;
};

// Průvodce pro veřejnost (bez přihlášení, nikdy bez Kč — to hlídá API podle role).
// Na /objekty řídí mapu nad seznamem (filtr okresu v URL, očíslované objekty), jinde plove v rohu a vede na karty objektů.
export function PublicGuide({ page, facility = null }: PublicGuideProps) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const withMap = page === "objekty";

  const showOnList = useCallback(
    (changes: Record<string, string | null>) => {
      const target = withMap ? withParams(pathname, searchParams, changes) : withParams("/objekty", new URLSearchParams(), changes);
      router.push(target as `/objekty${string}`, { scroll: false });
    },
    [pathname, router, searchParams, withMap],
  );

  const applyAnswer = useCallback(
    (answer: GuideAnswer) => {
      if (!withMap || !answer.mapAction) return;

      const action = answer.mapAction;
      if (action.type === "Facility" || action.type === "Facilities") {
        // Vybraný okres by očíslované objekty z jiného okresu schoval — mapa ukáže celý kraj
        const outside = answer.facilities.some((c) => searchParams.get("okres") && c.districtCode !== searchParams.get("okres"));
        if (outside) showOnList({ okres: null });
        showGuideHighlights(answer.facilities.map((c) => ({ id: c.id, rank: c.rank, latitude: c.latitude, longitude: c.longitude, status: c.status, name: c.name, districtCode: c.districtCode })));
      } else if (action.type === "District" && action.districtCode) {
        showGuideHighlights(null);
        showOnList({ okres: action.districtCode });
      } else if (action.type === "Region") {
        showGuideHighlights(null);
        showOnList({ okres: null, druh: null, q: null });
      }
    },
    [searchParams, showOnList, withMap],
  );

  function openFacility(card: GuideFacilityCard) {
    router.push(`/objekty/${card.code}`);
  }

  return (
    <GuidePanel
      context={{ page, facilityId: facility?.id ?? null, districtCode: searchParams.get("okres") }}
      contextLabel={facility?.name ?? null}
      starters={facility ? STARTERS_HERE : STARTERS}
      storageKey="smartaudit:pruvodce-ai:v2:verejnost"
      variant={withMap ? "map" : "floating"}
      onAnswer={applyAnswer}
      onOpenFacility={openFacility}
      onOpenDistrict={(code) => showOnList({ okres: code })}
      onReset={() => showGuideHighlights(null)}
    />
  );
}
