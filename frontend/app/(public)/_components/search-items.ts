import type { FacilityCategory, OpenFacility } from "@/types/api";

// Co našeptávač potřebuje o objektu — do prohlížeče jde jen tohle, ne celý záznam z otevřeného API
export type SearchItem = {
  code: string;
  name: string;
  municipalityName: string;
  districtName: string;
  category: FacilityCategory;
  co2: number | null;
};

export function toSearchItems(facilities: OpenFacility[]): SearchItem[] {
  return facilities.map(({ code, name, municipalityName, districtName, category, co2 }) => ({ code, name, municipalityName, districtName, category, co2 }));
}
