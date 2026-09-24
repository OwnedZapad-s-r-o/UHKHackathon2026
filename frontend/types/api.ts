import type { Feature, FeatureCollection, MultiPolygon, Polygon } from "geojson";

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

// Hranice okresů z GET /api/map/regions?type=Okres (GeoJSON, zjednodušené)
export type RegionFeature = Feature<Polygon | MultiPolygon, { code: string; name: string; regionType: string }>;

export type RegionCollection = FeatureCollection<Polygon | MultiPolygon, RegionFeature["properties"]>;

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

export type SensorMetric = "Temperature" | "Co2" | "Power" | "Humidity" | "Voc" | "Illuminance" | "Pressure" | "Pm25" | "Noise" | "Motion" | "Other";

export type TelemetryPoint = { timestamp: string; value: number };

export type Telemetry = {
  metric: SensorMetric;
  unit: string;
  // live = zónu měří skutečné čidlo (IQRF), ne simulace
  series: { zoneId: string | null; name: string; floor: number | null; isHotspot: boolean; live: boolean; points: TelemetryPoint[] }[];
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
  zones: {
    id: string;
    name: string;
    type: string;
    floor: number;
    wing: string;
    isHotspot: boolean;
    temperature: number | null;
    co2: number | null;
    humidity: number | null;
    live: boolean;
  }[];
  updatedAt: string | null;
};

export type Footprint = {
  type: "Feature";
  geometry: { type: string; coordinates: unknown } | null;
  properties: {
    facilityId: string;
    heightMeters: number;
    floors: number;
    heightEstimated: boolean;
    builtUpArea: number | null;
    constructionYear: number | null;
    ruianCode: string | null;
    source: string;
  };
};

export type AiRecommendation = {
  title: string;
  detail: string;
  cost: string;
  saving: string;
  payback: string;
  heritageSafe: boolean;
};

export type AiAnalysis = {
  id: string;
  model: string;
  summary: string;
  rootCause: string;
  recommendations: AiRecommendation[];
  letterDraft: string | null;
  createdAt: string;
};

export type AiAuditState = {
  configured: boolean;
  analysis: AiAnalysis | null;
};

// Senzory IQRF — GET /api/sensors/gateways(/{id})
export type Broker = {
  connected: boolean;
  error: string | null;
  publicHost: string | null;
  publicPort: number;
  // Gatewaye se připojují šifrovaně (TLS); certifikát CA brokeru jde stáhnout
  publicTls: boolean;
  caAvailable: boolean;
  responseTopicFilter: string;
  requestTopicTemplate: string;
};

export type GatewayListItem = {
  id: string;
  name: string;
  gatewayId: string;
  facilityId: string;
  facilityCode: string;
  facilityName: string;
  online: boolean;
  lastSeenAt: string | null;
  nodeCount: number;
  channelCount: number;
  assignedCount: number;
};

export type GatewaysResponse = {
  broker: Broker;
  gateways: GatewayListItem[];
  unknown: { gatewayId: string; lastSeenAt: string; messageCount: number }[];
};

export type GatewayChannel = {
  id: string;
  channelKey: string;
  nodeAddress: number;
  iqrfType: number;
  name: string;
  unit: string;
  metric: SensorMetric;
  lastValue: number | null;
  lastSeenAt: string | null;
  sensorId: string | null;
  zoneId: string | null;
  zoneName: string | null;
  samples: { timestamp: string; value: number }[];
};

export type GatewayDetail = {
  id: string;
  name: string;
  gatewayId: string;
  pollSeconds: number;
  nodes: number[];
  online: boolean;
  lastSeenAt: string | null;
  facility: { id: string; code: string; name: string };
  broker: Broker;
  // Přihlášení gatewaye k brokeru (jméno = id gatewaye); null = broker bez účtů
  credentials: { username: string; password: string } | null;
  channels: GatewayChannel[];
  zones: { id: string; name: string; floor: number }[];
  messages: { timestamp: string; direction: "Request" | "Response"; messageType: string; payload: string }[];
};

// Otevřené API — GET /api/open/v1/facilities (veřejný pohled, bez Kč)
export type OpenFacility = {
  id: string;
  code: string;
  name: string;
  category: FacilityCategory;
  kind: string;
  street: string | null;
  houseNumber: string | null;
  municipalityName: string;
  districtCode: string;
  districtName: string;
  latitude: number;
  longitude: number;
  status: AuditStatus;
  headline: string;
  temperature: number | null;
  co2: number | null;
  co2HoursOverLimit: number | null;
  hasLiveSensors: boolean;
  evaluatedAt: string | null;
};
