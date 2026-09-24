import type { FacilityListItem } from "@/types/api";

// Stejná škála jako mapa okresů v PDF (backend ReportCharts.DistrictMap):
// podíl objektů se zjištěním, 5 tříd sekvenční modré — světlá = málo problémů, tmavá = hodně
export const ISSUE_SHARE_BREAKS = [0.15, 0.22, 0.29, 0.36];
export const ISSUE_SHARE_COLORS = ["#EEF3F8", "#CBDAEA", "#9DB8D5", "#6A8FB8", "#3F6592"];
export const ISSUE_SHARE_LABELS = ["< 15 %", "15–22 %", "22–29 %", "29–36 %", "≥ 36 %"];

export type DistrictStats = {
  code: string;
  name: string;
  total: number;
  critical: number;
  warning: number;
  issueShare: number;
  // null = role bez přístupu k částkám
  lossCzk: number | null;
};

export function issueShareColor(share: number) {
  return ISSUE_SHARE_COLORS[ISSUE_SHARE_BREAKS.filter((b) => share >= b).length];
}

// Souhrn po okresech — vždy z celého kraje, ne z filtrovaného výběru, aby barvy okresů byly stálé
export function districtStats(facilities: FacilityListItem[]): Record<string, DistrictStats> {
  const stats: Record<string, DistrictStats> = {};

  for (const facility of facilities) {
    const entry = (stats[facility.districtCode] ??= {
      code: facility.districtCode,
      name: facility.districtName,
      total: 0,
      critical: 0,
      warning: 0,
      issueShare: 0,
      lossCzk: facility.annualLossCzk === null ? null : 0,
    });

    entry.total += 1;
    if (facility.status === "Critical") entry.critical += 1;
    if (facility.status === "Warning") entry.warning += 1;
    if (entry.lossCzk !== null && facility.annualLossCzk !== null) entry.lossCzk += facility.annualLossCzk;
  }

  for (const entry of Object.values(stats)) {
    entry.issueShare = entry.total === 0 ? 0 : (entry.critical + entry.warning) / entry.total;
  }

  return stats;
}
