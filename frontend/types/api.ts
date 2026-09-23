// Tvary odpovědí ASP.NET API (SmartAudit.Api) — enumy chodí jako řetězce

export type AuditStatus = "Nominal" | "Warning" | "Critical";

export type FacilityCategory = "School" | "Social" | "Health" | "Culture";

export type HeritageStatus = "None" | "Zone" | "Reserve" | "NationalMonument";

export type Role = "Kraj" | "Spravce" | "Verejnost";

export type FacilityListItem = {
  id: string;
  code: string;
  name: string;
  category: FacilityCategory;
  kind: string;
  municipalityName: string;
  orpCode: string;
  orpName: string;
  districtCode: string;
  districtName: string;
  latitude: number;
  longitude: number;
  status: AuditStatus;
  headline: string;
  temperature: number | null;
  co2: number | null;
  power: number | null;
  annualLossCzk: number | null;
  heritageStatus: HeritageStatus;
};

export type Session = {
  token: string;
  displayName: string;
  role: Role;
  organizationName: string | null;
};

export type AuthResponse = {
  token: string;
  userId: string;
  displayName: string;
  role: Role;
  organizationId: string | null;
  organizationName?: string | null;
};

export type ImportStatus = "Running" | "Live" | "Snapshot" | "Failed";

export type DataSource = {
  key: string;
  title: string;
  provider: string;
  group: string;
  purpose: string;
  datasetId: string | null;
  portalUrl: string | null;
  serviceUrl: string;
  lastImportedAt: string | null;
  lastRecordCount: number | null;
  lastStatus: ImportStatus | null;
};

export type DataSourcesResponse = {
  khkDatasetCount: number;
  externalSourceCount: number;
  totalRecords: number;
  groups: { group: string; sources: DataSource[] }[];
};

export type ApiStatus = {
  status: string;
  phase: string;
  ready: boolean;
  facilities: number;
  sensors: number;
};

export type Finding = {
  title: string;
  detail: string;
  tag: string;
  severity: AuditStatus;
};

export type FacilityDetail = {
  id: string;
  code: string;
  name: string;
  category: FacilityCategory;
  kinds: string[];
  address: {
    street: string | null;
    houseNumber: string | null;
    psc: string | null;
    municipality: string;
    orp: string;
    district: string;
    districtCode: string;
    label: string;
  };
  latitude: number;
  longitude: number;
  web: string | null;
  organization: { id: string; ico: string; name: string; legalForm: string | null; web: string | null; photoUrl: string | null } | null;
  department: string;
  building: {
    found: boolean;
    builtUpArea: number | null;
    constructionYear: number | null;
    energyCategory: string;
    estimatedFloors: number;
    currentUse: string | null;
    ruianCode: string | null;
    cadastreUrl: string | null;
    parcelRef: string | null;
  } | null;
  occupancy: {
    schoolYear: string;
    students: number | null;
    graduates: number | null;
    admitted: number | null;
    bedCapacity: number | null;
    residents: number | null;
    occupancyPercent: number | null;
    applicants: number | null;
    offeredPlaces: number | null;
    demandRatio: number | null;
  }[];
  territory: {
    isThreatenedArea: boolean;
    threatLevel: string | null;
    orpGrants: { orpName: string; count: number | null; total: number | null; individualTotal: number | null; programTotal: number | null } | null;
  };
  heritage: { status: HeritageStatus; name: string | null; constraint: string | null };
  municipalityContact: { municipality: string; dataBox: string | null; email: string | null; phone: string | null; web: string | null } | null;
  assessment: {
    status: AuditStatus;
    headline: string;
    summary: string;
    findings: Finding[];
    temperature: number | null;
    co2: number | null;
    power: number | null;
    co2HoursOverLimit: number;
    freeDayMeanTemperature: number | null;
    nightMeanTemperature: number | null;
    weeklyEnergyKwh: number;
    annualLossCzk: number | null;
    savingPercent: number | null;
    evaluatedAt: string;
  } | null;
  zoneCount: number;
  sensorCount: number;
};

export type SensorMetric = "Temperature" | "Co2" | "Power";

export type TelemetryPoint = { timestamp: string; value: number };

export type Telemetry = {
  metric: SensorMetric;
  unit: string;
  series: { zoneId: string | null; name: string; floor: number | null; isHotspot: boolean; points: TelemetryPoint[] }[];
};

export type Heatmap = {
  metric: SensorMetric;
  unit: string;
  min: number | null;
  max: number | null;
  days: { date: string; label: string; isFreeDay: boolean; hours: (number | null)[] }[];
};

export type BuildingSection = {
  floors: number;
  co2Limit: number;
  wings: string[];
  zones: { id: string; name: string; type: string; floor: number; wing: string; isHotspot: boolean; temperature: number | null; co2: number | null }[];
  updatedAt: string | null;
};

export type Footprint = {
  type: "Feature";
  geometry: { type: string; coordinates: unknown } | null;
  properties: { facilityId: string; status: AuditStatus; heightMeters: number; floors: number; heightEstimated: boolean };
};
