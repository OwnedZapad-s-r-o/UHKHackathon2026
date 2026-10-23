import type { EnergyCategory, FacilityCategory, HeritageStatus, Place } from "@/types/api";

// K čemu budova slouží — určuje simulaci provozu (výuka, nepřetržitý provoz…) a schéma místností
export const CATEGORY_OPTIONS: { value: FacilityCategory; label: string; hint: string }[] = [
  { value: "School", label: "Škola", hint: "výuka, internát, tělocvična" },
  { value: "Social", label: "Sociální služba", hint: "domov, stacionář, bydlení" },
  { value: "Health", label: "Zdravotnictví", hint: "nemocnice, léčebna, 24/7" },
  { value: "Culture", label: "Kultura", hint: "muzeum, knihovna, galerie" },
];

// Nejčastější druhy budov — první položka se uloží jako druh objektu (u škol podle ní vznikají místnosti řezu)
export const KIND_OPTIONS: Record<FacilityCategory, string[]> = {
  School: ["Střední škola", "Základní škola", "Mateřská škola", "Tělocvična", "Domov mládeže", "Dílny odborného výcviku", "Školní jídelna"],
  Social: ["Domov pro seniory", "Domov se zvláštním režimem", "Domov pro osoby se zdravotním postižením", "Denní stacionář", "Chráněné bydlení"],
  Health: ["Nemocnice", "Léčebna dlouhodobě nemocných", "Poliklinika", "Zdravotnická záchranná služba"],
  Culture: ["Muzeum", "Galerie", "Knihovna", "Divadlo", "Hvězdárna", "Depozitář"],
  Reference: [],
};

export const HERITAGE_LABEL: Record<Exclude<HeritageStatus, "None">, string> = {
  NationalMonument: "Národní kulturní památka",
  Reserve: "Památková rezervace",
  Zone: "Památková zóna",
};

// Stáří budovy → typická tepelná ztráta, se kterou audit počítá
export const ENERGY_LABEL: Record<EnergyCategory, string> = {
  Unknown: "stáří neznámé — audit počítá s průměrem",
  Before1945: "stavba do 1945",
  From1946To1975: "panel a cihla 1946–1975, nejvyšší ztráty",
  From1976To1990: "stavba 1976–1990",
  From1991To2005: "stavba 1991–2005",
  After2006: "moderní stavba po 2006",
};

export function energyCategoryOf(year: number | null): EnergyCategory {
  if (year === null) return "Unknown";
  if (year <= 1945) return "Before1945";
  if (year <= 1975) return "From1946To1975";
  if (year <= 1990) return "From1976To1990";
  if (year <= 2005) return "From1991To2005";
  return "After2006";
}

// Návrhy názvu z toho, co už víme — organizace, druh a ulice. Budova bez názvu se v mapě špatně hledá.
export function nameSuggestions(organizationName: string | null, kind: string | null, place: Place | null, hasBuildings: boolean) {
  const street = place?.address ? [place.address.street ?? place.territory.municipalityName, place.address.houseNumber].filter(Boolean).join(" ") : null;
  const town = place?.territory.municipalityName ?? null;

  const suggestions = [
    // První budova organizace je obvykle ta hlavní — pojmenuje se jako organizace
    !hasBuildings ? organizationName : null,
    kind && organizationName ? `${kind} — ${organizationName}` : null,
    kind && street ? `${kind}, ${street}` : null,
    kind && town && !street ? `${kind} ${town}` : null,
    // Bez druhu aspoň podle adresy — ať správce nezačíná s prázdným polem
    !kind && street ? `Budova ${street}` : null,
  ];

  return [...new Set(suggestions.filter((s): s is string => Boolean(s)))].slice(0, 3);
}
