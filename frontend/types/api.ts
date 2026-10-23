import type { Feature, FeatureCollection, MultiPolygon, Polygon } from "geojson";

// Tvary odpovědí ASP.NET API (SmartAudit.Api) — enumy chodí jako řetězce

export type AuditStatus = "Nominal" | "Warning" | "Critical";

// Reference = referenční objekt mimo portfolio kraje se skutečnými měřeními IQRF (MICRORISC, Jičín)
export type FacilityCategory = "School" | "Social" | "Health" | "Culture" | "Reference";

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
  // Mění se s doplněnými přilehlými budovami — mapa ji přidá k adrese obrysu (ten se jinak cachuje 2 dny)
  footprintVersion: string;
  // Pre-audit z otevřených dat (0–100) a jeho hlavní příčina
  preAuditScore: number | null;
  preAuditDriver: string | null;
  // Sada čidel MICRORISC IQRF pro budovu (Kč, ceník) a návratnost v měsících (jen kraj a správce)
  iqrfHardwareCostCzk: number | null;
  iqrfPaybackMonths: number | null;
  // Teplotu měří skutečné čidlo IQRF (jinak fyzikální model z otevřených dat)
  liveMeasurement: boolean;
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
  // Čl. 7.3 — záznam v katalogu poskytovatele a živý endpoint, ze kterého aplikace čte
  category: string;
  catalogUrl: string;
  liveApiUrl: string;
  dcatRecordUrl: string | null;
  // Poslední živá kontrola (POST /api/opendata/sync)
  lastSyncedAt: string | null;
  liveAvailable: boolean | null;
  liveRecordCount: number | null;
};

// GET /api/opendata/status a POST /api/opendata/sync
export type SourceSync = {
  key: string;
  title: string;
  provider: string;
  category: string;
  group: string;
  catalogUrl: string;
  liveApiUrl: string;
  dcatRecordUrl: string | null;
  syncMode: string;
  status: string;
  liveAvailable: boolean | null;
  liveRecordCount: number | null;
  localRecordCount: number | null;
  version: string | null;
  responseMs: number | null;
  error: string | null;
  changed: boolean;
  lastSyncedAt: string | null;
  lastImportedAt: string | null;
  lastImportStatus: ImportStatus | null;
};

export type OpenDataSync = {
  syncedAt: string | null;
  durationMs: number | null;
  syncMode: string;
  catalog: { url: string; available: boolean | null; datasetCount: number | null; eTag: string | null; lastModified: string | null; responseMs: number | null; error: string | null };
  summary: { total: number; live: number; cache: number; changed: number };
  reimportRunning: boolean;
  reimportedKeys: string[];
  sources: SourceSync[];
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
  // Odkud zjištění pochází (citace zdrojů dat)
  source: string | null;
};

// Odkaz na zdrojovou sadu (Čl. 7.3) — katalog poskytovatele, živé API, záznam DCAT-AP-CZ
export type DatasetLink = {
  key: string;
  title: string;
  provider: string;
  category: string;
  catalogUrl: string;
  liveApiUrl: string;
  dcatRecordUrl: string | null;
};

export type PreAuditParameter = {
  key: "A" | "B" | "C" | "D" | "E";
  name: string;
  score: number;
  maxScore: number;
  value: string;
  explanation: string;
  sources: DatasetLink[];
};

export type PreAudit = {
  score: number;
  status: AuditStatus;
  dominantDriver: string;
  parameters: PreAuditParameter[];
  specificHeatLoss: number;
  heatLossWattsPerKelvin: number;
  degreeDays: number;
  annualHeatDemandKwh: number;
  annualHeatCostCzk: number | null;
  heatPriceCzkPerMwh: number | null;
  heatPriceFromContract: boolean;
  regulationSavingPercent: number;
  computedAt: string;
};

export type IqrfRoi = {
  gateways: number;
  sensors: number;
  gatewayUnitCzk: number;
  sensorUnitCzk: number;
  estimatedHardwareCostCzk: number;
  estimatedAnnualSavingsCzk: number | null;
  paybackMonths: number | null;
  savingsBasis: string;
};

export type DataMode = "LiveIqrf" | "OpenDataModel";

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
    // Opravy od správce — mají přednost ve výpočtu ztráty; hodnoty výše zůstávají z ČÚZK a odhadu
    areaOverride: number | null;
    floorsOverride: number | null;
    yearOverride: number | null;
    editedEnergyCategory: string | null;
    editedAt: string | null;
    // Výpis z katastru (LV) a technické údaje stavebního objektu z RÚIAN — jen u ověřených budov
    cadastralCard: CadastralCard | null;
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
    // Jak skutečná čidla opravila odhad ztráty (jen pro kraj a správce) — null, dokud budovu neměří žádné čidlo teploty
    sensorCorrection: SensorCorrection | null;
  } | null;
  zoneCount: number;
  sensorCount: number;
  // Budovu přidal správce (není v otevřených datech kraje) — jen takovou může odebrat
  addedByManager: boolean;
  // Přilehlé budovy, které katastr k objektu nevede a doplnil je správce — jejich plocha vstupuje do výpočtu ztráty
  addedBuildingParts: BuildingPart[];
  // Živé měření IQRF, nebo fyzikální model z otevřených dat
  dataMode: DataMode;
  preAudit: PreAudit | null;
  iqrfRoi: IqrfRoi | null;
  // Sady otevřených dat propojené s objektem a přímé odkazy na záznam objektu ve zdrojích
  linkedDatasets: DatasetLink[];
  objectLinks: { label: string; url: string; provider: string }[];
};

export type CadastralCard = {
  cadastralTerritory: string | null;
  ownershipSheet: string | null;
  parcelNumbers: string | null;
  parcelArea: number | null;
  municipalityPart: string | null;
  owner: string | null;
  structureUse: string | null;
  floorArea: number | null;
  enclosedVolume: number | null;
  heatingType: string | null;
  cadastreUrl: string | null;
};

export type BuildingPart = { id: string; areaSquareMeters: number; source: string; addedAt: string };

// virtualHistory = aspoň jedno čidlo je virtuální a dodalo týden historie; skutečné čidlo ztrátu upřesňuje postupně 7 dní
export type SensorCorrection = {
  lossBeforeCzk: number;
  measuredZones: number;
  zoneCount: number;
  virtualHistory: boolean;
  since: string;
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
    // Kolik přilehlých budov doplnil správce — jsou v geometrii jako další polygony MultiPolygonu
    addedParts: number;
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
  // Stav budovy se od poslední analýzy nezměnil — API vrátilo uloženou analýzu bez volání modelu
  fromCache?: boolean;
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
  // Virtuální gateway (testovací síť, demo) — odpovídá za ni aplikace, ne hardware
  isVirtual: boolean;
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
  isVirtual: boolean;
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

// Nové gatewaye online, které čekají na spárování heslem z nálepky — GET /api/sensors/nearby
export type NearbyDevice = {
  serial: string;
  model: string;
  nodes: { address: number; metrics: SensorMetric[] }[];
  rssi: number | null;
  emulated: boolean;
  lastSeenAt: string;
  // Nálepka emulovaného zařízení (demo) — skutečné zařízení ji v aplikaci nemá
  label: { serial: string; model: string; password: string } | null;
};

// Čidla v dosahu gatewaye mimo její síť — GET /api/sensors/gateways/{id}/nearby
// supported = false: gateway seznam neumí (skutečný IQRF daemon) nebo neodpověděla — čidlo jde přidat jen kódem
export type NearbyNodes = {
  supported: boolean;
  nodes: { mid: string; metrics: SensorMetric[]; rssi: number | null; label: { mid: string; code: string } | null }[];
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
  dataMode: DataMode;
  preAuditScore: number | null;
  preAuditStatus: AuditStatus | null;
  preAuditDriver: string | null;
};

// Smlouvy organizace z Registru smluv — GET /api/facilities/{id}/contracts (jen kraj a správce)
export type ContractCategory = "EnergySupply" | "FacilityManagement" | "Reconstruction" | "Other";

export type EnergyCommodity = "Heat" | "Gas" | "Electricity";

export type FacilityContract = {
  id: string;
  contractId: string;
  contractNumber: string | null;
  title: string;
  category: ContractCategory;
  partnerName: string;
  partnerIco: string | null;
  amountWithoutVat: number | null;
  amountWithVat: number | null;
  dateSigned: string | null;
  publishedOn: string;
  registryUrl: string;
  commodity: EnergyCommodity | null;
  unitPriceCzkPerMwh: number | null;
  priceSource: string | null;
  attachments: { id: string; name: string }[];
};

// Cena tepla, se kterou počítá audit — ze smlouvy (fromContract), jinak paušál
export type HeatPrice = {
  czkPerKwh: number;
  defaultCzkPerKwh: number;
  fromContract: boolean;
  contractId: string | null;
  annualLossCzk: number | null;
};

// syncedAt = null: smlouvy se z registru ještě nestáhly (běží na pozadí po startu)
export type FacilityContracts = {
  organizationName: string | null;
  ico: string | null;
  syncedAt: string | null;
  heatPrice: HeatPrice;
  contracts: FacilityContract[];
};

// Demo správa: vyrobená zařízení, jejich stav v párování a budovy, kterým čidla opravila odhad ztráty
export type DemoBehavior = "Setback" | "Overheating";

export type DemoStage = "Starting" | "Announcing" | "InRange" | "Paired" | "Reporting" | "Assigned";

export type DemoDevice = {
  id: string;
  kind: "Gateway" | "Node";
  serial: string;
  model: string;
  code: string;
  nodes: string[];
  metrics: SensorMetric[];
  behavior: DemoBehavior;
  stage: DemoStage;
  parentName: string | null;
  gatewayId: string | null;
  facilityId: string | null;
  facilityName: string | null;
  channelCount: number;
  assignedCount: number;
  createdAt: string;
};

export type LossCorrection = {
  facilityId: string;
  facilityCode: string;
  facilityName: string;
  lossBeforeCzk: number | null;
  lossNowCzk: number;
  measuredZones: number;
  zoneCount: number;
  virtualHistory: boolean;
  since: string;
  evaluatedAt: string;
};

export type DemoOverview = {
  templates: { id: string; label: string; metrics: SensorMetric[] }[];
  gateways: { id: string; gatewayId: string; name: string; facilityName: string }[];
  devices: DemoDevice[];
  corrections: LossCorrection[];
};

// Přidání budovy správcem — našeptávač adres a dohledání místa (RÚIAN, hranice KHK, katastr, památky)
export type AddressSuggestion = {
  key: string;
  label: string;
  street: string | null;
  houseNumber: string | null;
  psc: string | null;
  post: string | null;
};

export type EnergyCategory = "Unknown" | "Before1945" | "From1946To1975" | "From1976To1990" | "From1991To2005" | "After2006";

export type Place = {
  latitude: number;
  longitude: number;
  address: { label: string; street: string | null; houseNumber: string | null; psc: string | null } | null;
  territory: { municipalityCode: string; municipalityName: string; orpName: string; districtCode: string; districtName: string };
  building: {
    footprint: Polygon | MultiPolygon;
    builtUpArea: number | null;
    constructionYear: number | null;
    currentUse: string | null;
    ruianCode: string | null;
    energyCategory: EnergyCategory;
    estimatedFloors: Record<FacilityCategory, number>;
  } | null;
  cadastreAvailable: boolean;
  heritageStatus: HeritageStatus;
  heritageName: string | null;
  neighbors: { id: string; code: string; name: string; distanceMeters: number; sameBuilding: boolean; own: boolean }[];
};

// Průvodce SmartAuditu — POST /api/guide/ask (přes /pruvodce na serveru Next, kvůli tokenu z httpOnly cookie)
export type GuidePage = "mapa" | "objekty" | "karta" | "uvod";

export type GuideContext = {
  page: GuidePage;
  facilityId: string | null;
  districtCode: string | null;
};

export type GuideMode = "Ai" | "Offline" | "Blocked";

export type GuideFocus = "None" | "Facility" | "Facilities" | "District" | "Region";

export type GuideFacilityCard = {
  rank: number;
  id: string;
  code: string;
  name: string;
  category: FacilityCategory;
  kind: string;
  municipalityName: string;
  districtCode: string;
  districtName: string;
  latitude: number;
  longitude: number;
  status: AuditStatus;
  headline: string;
  co2: number | null;
  co2HoursOverLimit: number | null;
  temperature: number | null;
  annualLossCzk: number | null;
  hasLiveSensors: boolean;
};

export type GuideDistrictCard = {
  code: string;
  name: string;
  facilities: number;
  withIssues: number;
  critical: number;
  annualLossCzk: number | null;
  averageCo2: number | null;
};

export type GuideSource = { title: string; snippet: string; kind: string; url: string | null };

export type GuideMapAction = {
  type: GuideFocus;
  facilityIds: string[];
  districtCode: string | null;
  panel: string | null;
};

export type GuideLink = { label: string; href: string };

export type GuideChartBar = { rank: number; facilityId: string; label: string; value: number; status: AuditStatus };

export type GuideChartLine = { label: string; primary: boolean; points: { time: string; value: number }[] };

// Graf v odpovědi průvodce — čísla doplňuje API z dat, model jen vybírá druh grafu
export type GuideChart = {
  kind: "Ranking" | "Series";
  title: string;
  unit: string;
  bars: GuideChartBar[];
  lines: GuideChartLine[];
  thresholds: { value: number; label: string; tone: "warning" | "critical" | "neutral" }[];
  note: string | null;
};

export type GuideAnswer = {
  answer: string;
  mode: GuideMode;
  facilities: GuideFacilityCard[];
  districts: GuideDistrictCard[];
  sources: GuideSource[];
  mapAction: GuideMapAction | null;
  links: GuideLink[];
  suggestions: string[];
  charts: GuideChart[];
};
