import type { AuditStatus } from "@/types/api";

// Povely průvodce pro mapu — průvodce a mapa jsou sourozenci, ne rodič a dítě, proto přes události okna
// (stejně jako MAP_HOME_EVENT). Mapa aplikace i veřejná mapa poslouchají stejné události.
export const GUIDE_HIGHLIGHT_EVENT = "smartaudit:guide-highlight";
export const GUIDE_HOVER_EVENT = "smartaudit:guide-hover";

export type GuideHighlight = {
  id: string;
  rank: number;
  latitude: number;
  longitude: number;
  status: AuditStatus;
  name: string;
  districtCode: string;
};

// Očíslované špendlíky v mapě (čísla = karty v průvodci); null je smaže
export function showGuideHighlights(items: GuideHighlight[] | null) {
  window.dispatchEvent(new CustomEvent<GuideHighlight[] | null>(GUIDE_HIGHLIGHT_EVENT, { detail: items }));
}

// Najetí na kartu v průvodci zvýrazní její špendlík
export function hoverGuideItem(id: string | null) {
  window.dispatchEvent(new CustomEvent<string | null>(GUIDE_HOVER_EVENT, { detail: id }));
}
