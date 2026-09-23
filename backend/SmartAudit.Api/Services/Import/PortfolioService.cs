using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SmartAudit.Core.Data;
using SmartAudit.Core.Data.Tables;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Services.Import
{
    public record PortfolioSummary(int Facilities, int Organizations, int Regions, int Contacts, int Metrics, int HeritageProtected);

    public interface IPortfolioService
    {
        Task<PortfolioSummary> BuildAsync(CancellationToken ct);
    }

    // Převod surových sad KHK na doménové tabulky: regiony, kontakty, organizace, objekty, obsazenost, památky
    public class PortfolioService : IPortfolioService
    {
        private const string UnknownYear = "neuvedeno";
        private const string VosYear = "2024/2025";

        private static readonly GeometryFactory Factory = new(new PrecisionModel(), 4326);

        // Druhy bez vlastní budovy — sdílejí objekt se školou
        private static readonly string[] ExcludedSchoolKinds = ["Školní jídelna", "Školní jídelna - výdejna", "Školní družina", "Školní klub"];

        // Pořadí, podle kterého se vybere hlavní druh a název objektu
        private static readonly string[] SchoolKindPriority =
        [
            "Střední škola", "Konzervatoř", "Základní škola", "Vyšší odborná škola", "Mateřská škola",
            "Základní umělecká škola", "Dětský domov se školou", "Dětský domov", "Výchovný ústav",
            "Domov mládeže", "Internát",
        ];

        private static readonly string[] AccommodationKinds = ["Domov mládeže", "Internát"];

        // Pobytová zařízení — budovy, kde lidé žijí a má smysl je měřit
        private static readonly string[] ResidentialSocialTypes =
        [
            "domovy pro seniory", "domovy se zvláštním režimem", "domovy pro osoby se zdravotním postižením",
            "odlehčovací služby", "týdenní stacionáře",
        ];

        private readonly SmartAuditDbContext _dbContext;
        private readonly IOpenDataImportService _importService;
        private readonly ILogger<PortfolioService> _logger;

        public PortfolioService(SmartAuditDbContext dbContext, IOpenDataImportService importService, ILogger<PortfolioService> logger)
        {
            _dbContext = dbContext;
            _importService = importService;
            _logger = logger;
        }

        public async Task<PortfolioSummary> BuildAsync(CancellationToken ct)
        {
            var regions = await BuildRegionsAsync(ct);
            var contacts = await BuildContactsAsync(ct);
            var organizations = await BuildOrganizationsAsync(ct);

            var candidates = new List<FacilityCandidate>();
            candidates.AddRange(await SchoolCandidatesAsync(ct));
            candidates.AddRange(await SocialCandidatesAsync(ct));
            candidates.AddRange(await HealthCandidatesAsync(ct));
            candidates.AddRange(await CultureCandidatesAsync(ct));

            var facilities = await UpsertFacilitiesAsync(candidates, organizations, ct);
            var heritage = await ApplyHeritageAsync(facilities, ct);
            var metrics = await BuildMetricsAsync(facilities, ct);

            await _dbContext.SaveChangesAsync(ct);

            var summary = new PortfolioSummary(facilities.Count, organizations.Count, regions, contacts, metrics, heritage);
            _logger.LogInformation("Portfolio sestaveno: {Summary}", summary);

            return summary;
        }

        #region Regiony a kontakty

        private async Task<int> BuildRegionsAsync(CancellationToken ct)
        {
            await _dbContext.Regions.ExecuteDeleteAsync(ct);

            var regions = new List<Region>();

            foreach (var f in await _importService.LoadAsync("hranice-kraje", ct))
                regions.Add(new Region(RegionType.Kraj, f.Text("kod_vusc") ?? "CZ052", f.Text("nazev_vusc") ?? "Královéhradecký kraj", f.Geometry));

            foreach (var f in await _importService.LoadAsync("hranice-okresu", ct))
                regions.Add(new Region(RegionType.Okres, f.Text("kod_okresu")!, f.Text("nazev_okresu")!, f.Geometry));

            var orps = new Dictionary<string, Region>();
            foreach (var f in await _importService.LoadAsync("hranice-orp", ct))
            {
                var orp = new Region(RegionType.Orp, f.Text("kod_orp")!, f.Text("nazev_orp")!, f.Geometry);
                orps[orp.Code] = orp;
                regions.Add(orp);
            }

            foreach (var f in await _importService.LoadAsync("hsoo", ct))
            {
                var municipality = new Region(RegionType.Obec, f.Text("kod_obce")!, f.Text("nazev_obce")!, f.Geometry);
                municipality.SetThreat(f.Text("kod_orp") ?? string.Empty, f.Is("hsoo", "ano"), f.Text("urov_ohr"));
                regions.Add(municipality);
            }

            foreach (var f in await _importService.LoadAsync("dotace-orp", ct))
                if (orps.TryGetValue(f.Text("kod_orp") ?? string.Empty, out var orp))
                    orp.SetGrants(f.Int("poc_dotaci") ?? 0, f.Decimal("finance") ?? 0);

            foreach (var f in await _importService.LoadAsync("dotace-orp-individualni", ct))
                if (orps.TryGetValue(f.Text("kod_orp") ?? string.Empty, out var orp))
                    orp.SetIndividualGrants(f.Int("poc_dotaci") ?? 0, f.Decimal("finance") ?? 0);

            foreach (var f in await _importService.LoadAsync("dotace-orp-programove", ct))
                if (orps.TryGetValue(f.Text("kod_orp") ?? string.Empty, out var orp))
                    orp.SetProgramGrants(f.Int("poc_dotaci") ?? 0, f.Decimal("finance") ?? 0);

            _dbContext.Regions.AddRange(regions);

            return regions.Count;
        }

        private async Task<int> BuildContactsAsync(CancellationToken ct)
        {
            await _dbContext.MunicipalityContacts.ExecuteDeleteAsync(ct);

            var contacts = (await _importService.LoadAsync("adresar-obci", ct))
                .Where(f => f.Text("kod_obce") != null)
                .GroupBy(f => f.Text("kod_obce")!)
                .Select(g => g.First())
                .Select(f => new MunicipalityContact(
                    f.Text("kod_obce")!,
                    f.Text("nazev_obce") ?? string.Empty,
                    f.Text("ds"),
                    f.Text("email_1"),
                    f.Text("telefon_1"),
                    f.Text("www"),
                    JoinAddress(f.Text("nazev_ulice"), HouseNumber(f), f.Text("psc"), f.Text("posta"))))
                .ToList();

            _dbContext.MunicipalityContacts.AddRange(contacts);

            return contacts.Count;
        }

        #endregion

        #region Organizace

        private async Task<Dictionary<string, Organization>> BuildOrganizationsAsync(CancellationToken ct)
        {
            var organizations = await _dbContext.Organizations.ToDictionaryAsync(o => o.Ico, ct);

            Organization Ensure(string ico, string name, string? legalForm)
            {
                if (organizations.TryGetValue(ico, out var existing))
                    return existing;

                var organization = new Organization(ico, name, legalForm);
                organizations[ico] = organization;
                _dbContext.Organizations.Add(organization);

                return organization;
            }

            foreach (var f in await _importService.LoadAsync("organizace-khk", ct))
            {
                var ico = KeyNormalizer.Ico(f.Text("ico"));
                var name = f.Text("nazev");
                if (ico == null || name == null)
                    continue;

                Ensure(ico, name, f.Text("pravni_forma"))
                    .ApplyRegionRegistry(name, f.Text("odbor"), f.Text("typ"), f.Text("www_1"), f.Text("fotografie_1"));
            }

            foreach (var f in await _importService.LoadAsync("pravni-subjekty-skol", ct))
            {
                var ico = KeyNormalizer.Ico(f.Text("ico"));
                var name = f.Text("nazev");
                if (ico == null || name == null || !IsRegionFounder(f.Text("zrizovatel")))
                    continue;

                Ensure(ico, name, f.Text("pravni_forma"));
            }

            foreach (var f in await _importService.LoadAsync("skoly", ct))
            {
                var ico = KeyNormalizer.Ico(f.Text("ico"));
                var name = f.Text("nazev");
                if (ico == null || name == null || !f.Is("zrizovatel", "kraj"))
                    continue;

                Ensure(ico, name, f.Text("pravni_forma")).ApplySchoolRegistry(KeyNormalizer.Redizo(f.Text("redizo")), f.Text("pravni_forma"));
            }

            foreach (var f in await _importService.LoadAsync("socialni-sluzby", ct))
            {
                var ico = KeyNormalizer.Ico(f.Text("ico"));
                var name = f.Text("nazev_poskytovatele");
                if (ico == null || name == null || !IsRegionFounder(f.Text("pravni_forma_zrizovatele")))
                    continue;

                Ensure(ico, name, f.Text("pravni_forma"));
            }

            return organizations;
        }

        #endregion

        #region Kandidáti na objekty

        private record FacilityCandidate(
            string SourceKey,
            string Name,
            FacilityCategory Category,
            string[] Kinds,
            string[] Izos,
            string? Redizo,
            string? Ico,
            string? Web,
            Point Location,
            RawFeature Address);

        private async Task<List<FacilityCandidate>> SchoolCandidatesAsync(CancellationToken ct)
        {
            var records = (await _importService.LoadAsync("skoly", ct))
                .Where(f => f.Is("zrizovatel", "kraj"))
                .Where(f => !ExcludedSchoolKinds.Contains(f.Text("zarizeni_druh")))
                .Select(f => (Feature: f, Point: f.Point(Factory)))
                .Where(r => r.Point != null)
                .ToList();

            // Jedna budova = stejná organizace na stejném místě (~10 m)
            return records
                .GroupBy(r => (Ico: KeyNormalizer.Ico(r.Feature.Text("ico")), LocationKey(r.Point!)))
                .Select(g =>
                {
                    var ordered = g.OrderBy(r => KindRank(r.Feature.Text("zarizeni_druh"))).ToList();
                    var main = ordered[0].Feature;
                    var mainKind = main.Text("zarizeni_druh") ?? "Školské zařízení";
                    var name = main.Text("nazev") ?? mainKind;

                    // Samostatná budova domova mládeže nese název školy — doplníme druh, ať jde odlišit
                    if (AccommodationKinds.Contains(mainKind))
                        name = $"{mainKind} — {name}";

                    return new FacilityCandidate(
                        $"skola:{g.Key.Ico}:{g.Key.Item2}",
                        name,
                        FacilityCategory.School,
                        ordered.Select(r => r.Feature.Text("zarizeni_druh")).OfType<string>().Distinct().ToArray(),
                        ordered.Select(r => KeyNormalizer.Izo(r.Feature.Text("izo"))).OfType<string>().Distinct().ToArray(),
                        KeyNormalizer.Redizo(main.Text("redizo")),
                        g.Key.Ico,
                        main.Text("www"),
                        ordered[0].Point!,
                        main);
                })
                .ToList();
        }

        private async Task<List<FacilityCandidate>> SocialCandidatesAsync(CancellationToken ct)
        {
            var records = (await _importService.LoadAsync("socialni-sluzby", ct))
                .Where(f => IsRegionFounder(f.Text("pravni_forma_zrizovatele")))
                .Select(f => (Feature: f, Point: f.Point(Factory)))
                .Where(r => r.Point != null)
                .ToList();

            return records
                .GroupBy(r => (Ico: KeyNormalizer.Ico(r.Feature.Text("ico")), LocationKey(r.Point!)))
                .Where(g => g.Any(r => ResidentialSocialTypes.Contains(r.Feature.Text("typ_soc_sluzby"))))
                .Select(g =>
                {
                    var main = g.OrderBy(r => ResidentialSocialTypes.Contains(r.Feature.Text("typ_soc_sluzby")) ? 0 : 1).First();

                    return new FacilityCandidate(
                        $"socialni:{g.Key.Ico}:{g.Key.Item2}",
                        main.Feature.Text("nazev_zarizeni") ?? main.Feature.Text("nazev_poskytovatele") ?? "Sociální zařízení",
                        FacilityCategory.Social,
                        g.Select(r => r.Feature.Text("typ_soc_sluzby")).OfType<string>().Distinct().ToArray(),
                        [],
                        null,
                        g.Key.Ico,
                        null,
                        main.Point!,
                        main.Feature);
                })
                .ToList();
        }

        private async Task<List<FacilityCandidate>> HealthCandidatesAsync(CancellationToken ct)
        {
            var candidates = new List<(RawFeature Feature, string Name, string Kind)>();

            foreach (var f in await _importService.LoadAsync("nemocnice", ct))
            {
                if (!IsRegionFounder(f.Text("zrizovatel")))
                    continue;

                var branch = f.Text("pobocka");
                var name = branch == null || branch == "bez pobočky" ? f.Text("nazev") : branch;
                candidates.Add((f, name ?? "Nemocnice", "Nemocnice"));
            }

            foreach (var f in await _importService.LoadAsync("nasledna-pece", ct))
                if (IsRegionFounder(f.Text("zrizovatel")))
                    candidates.Add((f, f.Text("nazev") ?? "Následná lůžková péče", "Následná lůžková péče"));

            foreach (var f in await _importService.LoadAsync("lecebne-ustavy", ct))
                if (IsRegionFounder(f.Text("zrizovatel")))
                    candidates.Add((f, f.Text("poskytoval") ?? f.Text("druh_zarizeni") ?? "Odborný léčebný ústav", f.Text("druh_zarizeni") ?? "Odborný léčebný ústav"));

            return candidates
                .Select(c => (c.Feature, c.Name, c.Kind, Point: c.Feature.Point(Factory)))
                .Where(c => c.Point != null)
                .GroupBy(c => LocationKey(c.Point!))
                .Select(g =>
                {
                    var main = g.First();

                    return new FacilityCandidate(
                        $"zdravotnictvi:{g.Key}",
                        main.Name,
                        FacilityCategory.Health,
                        g.Select(c => c.Kind).Distinct().ToArray(),
                        [],
                        null,
                        KeyNormalizer.Ico(main.Feature.Text("ico")),
                        main.Feature.Text("www"),
                        main.Point!,
                        main.Feature);
                })
                .ToList();
        }

        private async Task<List<FacilityCandidate>> CultureCandidatesAsync(CancellationToken ct)
        {
            // Jen zařízení, která kraj zřizuje — o.p.s., kde je kraj spoluzakladatel, nejsou jeho budovy
            return (await _importService.LoadAsync("organizace-khk", ct))
                .Where(f => f.Is("typ", "kulturní zařízení"))
                .Select(f => (Feature: f, Point: f.Point(Factory)))
                .Where(r => r.Point != null)
                .Select(r => new FacilityCandidate(
                    $"kultura:{KeyNormalizer.Ico(r.Feature.Text("ico"))}:{LocationKey(r.Point!)}",
                    r.Feature.Text("nazev") ?? "Kulturní zařízení",
                    FacilityCategory.Culture,
                    ["Kulturní zařízení"],
                    [],
                    null,
                    KeyNormalizer.Ico(r.Feature.Text("ico")),
                    r.Feature.Text("www_1"),
                    r.Point!,
                    r.Feature))
                .ToList();
        }

        #endregion

        #region Objekty

        private async Task<List<Facility>> UpsertFacilitiesAsync(List<FacilityCandidate> candidates, Dictionary<string, Organization> organizations, CancellationToken ct)
        {
            var existing = await _dbContext.Facilities.ToDictionaryAsync(f => f.SourceKey, ct);
            var keys = candidates.Select(c => c.SourceKey).ToHashSet();

            // Objekty, které ze zdrojových dat zmizely, se smažou i se senzory
            foreach (var removed in existing.Values.Where(f => !keys.Contains(f.SourceKey)))
                _dbContext.Facilities.Remove(removed);

            var facilities = new List<Facility>();

            foreach (var candidate in candidates.GroupBy(c => c.SourceKey).Select(g => g.First()))
            {
                if (!existing.TryGetValue(candidate.SourceKey, out var facility))
                {
                    facility = new Facility(candidate.SourceKey, candidate.Name, candidate.Category, candidate.Location);
                    facility.SetSimulationProfile(ProfileFor(candidate.SourceKey, candidate.Category));
                    _dbContext.Facilities.Add(facility);
                }
                else
                {
                    facility.Rename(candidate.Name);
                    facility.Relocate(candidate.Location);
                }

                var a = candidate.Address;
                facility.SetIdentifiers(candidate.Kinds, candidate.Izos, candidate.Redizo, candidate.Ico, candidate.Web);
                facility.SetAddress(
                    a.Text("nazev_ulice"),
                    HouseNumber(a),
                    a.Text("psc")?.Replace(" ", string.Empty),
                    a.Text("kod_obce") ?? string.Empty,
                    a.Text("nazev_obce") ?? string.Empty,
                    a.Text("kod_orp") ?? string.Empty,
                    a.Text("nazev_orp") ?? string.Empty,
                    a.Text("kod_okresu") ?? string.Empty,
                    a.Text("nazev_okresu") ?? string.Empty);

                facility.AssignOrganization(candidate.Ico != null && organizations.TryGetValue(candidate.Ico, out var organization) ? organization.Id : null);

                facilities.Add(facility);
            }

            // Čitelný kód: kategorie-ORP-pořadí, stabilní mezi importy
            foreach (var group in facilities.GroupBy(f => (f.Category, f.OrpCode)))
            {
                var order = 1;
                foreach (var facility in group.OrderBy(f => f.SourceKey, StringComparer.Ordinal))
                    facility.AssignCode($"{CategoryPrefix(facility.Category)}-{facility.OrpCode}-{order++:00}");
            }

            return facilities;
        }

        private async Task<int> ApplyHeritageAsync(List<Facility> facilities, CancellationToken ct)
        {
            var zones = await PolygonsAsync("pamatkove-zony", ct);
            var reserves = await PolygonsAsync("pamatkove-rezervace", ct);
            var monumentAreas = await PolygonsAsync("nkp-plosne", ct);
            var monumentPoints = (await _importService.LoadAsync("nkp-bodove", ct))
                .Select(f => (Name: f.Text("nazev") ?? f.Text("objekt"), Point: f.Point(Factory)))
                .Where(m => m.Point != null)
                .ToList();

            var protectedCount = 0;

            foreach (var facility in facilities)
            {
                var location = facility.Location;

                // ~20 m tolerance — adresní bod často leží u vchodu, ne uvnitř obrysu památky
                var monument = monumentAreas.FirstOrDefault(m => m.Geometry.IsWithinDistance(location, 0.00025)).Name
                    ?? monumentPoints.FirstOrDefault(m => m.Point!.IsWithinDistance(location, 0.0003)).Name;

                if (monument != null)
                {
                    facility.SetHeritage(HeritageStatus.NationalMonument, monument);
                }
                else if (reserves.FirstOrDefault(r => r.Geometry.Contains(location)) is { Name: not null } reserve)
                {
                    facility.SetHeritage(HeritageStatus.Reserve, reserve.Name);
                }
                else if (zones.FirstOrDefault(z => z.Geometry.Contains(location)) is { Name: not null } zone)
                {
                    facility.SetHeritage(HeritageStatus.Zone, zone.Name);
                }
                else
                {
                    facility.SetHeritage(HeritageStatus.None, null);
                }

                if (facility.HeritageStatus != HeritageStatus.None)
                    protectedCount++;
            }

            return protectedCount;
        }

        private async Task<List<(string? Name, Geometry Geometry)>> PolygonsAsync(string key, CancellationToken ct)
        {
            return (await _importService.LoadAsync(key, ct))
                .Where(f => f.Geometry is Polygon or MultiPolygon)
                .Select(f => (f.Text("nazev") ?? f.Text("objekt"), f.Geometry!))
                .ToList();
        }

        #endregion

        #region Obsazenost

        private sealed class MetricAccumulator
        {
            public int? Students;
            public int? Graduates;
            public int? Admitted;
            public int? BedCapacity;
            public int? Residents;
            public int? Applicants;
            public int? OfferedPlaces;
        }

        private async Task<int> BuildMetricsAsync(List<Facility> facilities, CancellationToken ct)
        {
            var facilityIds = facilities.Select(f => f.Id).ToList();
            await _dbContext.FacilityMetrics.Where(m => facilityIds.Contains(m.FacilityId)).ExecuteDeleteAsync(ct);

            var byIzo = facilities
                .SelectMany(f => f.Izos.Select(izo => (Izo: izo, Facility: f)))
                .GroupBy(x => x.Izo)
                .ToDictionary(g => g.Key, g => g.First().Facility);

            var byRedizo = facilities
                .Where(f => f.Redizo != null)
                .GroupBy(f => f.Redizo!)
                .ToDictionary(g => g.Key, g => g.OrderBy(f => KindRank(f.Kinds.FirstOrDefault())).ToList());

            Facility? Resolve(RawFeature f, params string[] izoFields)
            {
                foreach (var field in izoFields)
                    if (KeyNormalizer.Izo(f.Text(field)) is { } izo && byIzo.TryGetValue(izo, out var facility))
                        return facility;

                var redizo = KeyNormalizer.Redizo(f.Text("redizo") ?? f.Text("identifikator_reditelstvi"));

                return redizo != null && byRedizo.TryGetValue(redizo, out var candidates) ? candidates[0] : null;
            }

            var accumulators = new Dictionary<(Facility Facility, string Year), MetricAccumulator>();

            MetricAccumulator Get(Facility facility, string year)
            {
                if (!accumulators.TryGetValue((facility, year), out var accumulator))
                    accumulators[(facility, year)] = accumulator = new MetricAccumulator();

                return accumulator;
            }

            // Žáci SŠ: historie od 2021/22, aktuální rok ze sady „zřizovaných krajem“ má přednost
            var secondary = new Dictionary<(Facility, string), (int Students, int Graduates, int Admitted)>();

            void AddSecondary(IEnumerable<RawFeature> rows)
            {
                var sums = new Dictionary<(Facility, string), (int, int, int)>();

                foreach (var row in rows)
                {
                    var facility = Resolve(row, "izo", "izo_sidlo");
                    var year = row.Text("skolni_rok");
                    if (facility == null || year == null)
                        continue;

                    sums.TryGetValue((facility, year), out var sum);
                    sums[(facility, year)] = (sum.Item1 + (row.Int("pocet_zaku_celkem") ?? 0), sum.Item2 + (row.Int("pocet_absolventu") ?? 0), sum.Item3 + (row.Int("pocet_prijatych") ?? 0));
                }

                foreach (var (key, value) in sums)
                    secondary[key] = value;
            }

            AddSecondary((await _importService.LoadAsync("zaci-ss-historie", ct)).Where(f => f.Is("zrizovatel", "kraj")));
            AddSecondary(await _importService.LoadAsync("zaci-ss-kraj", ct));

            foreach (var ((facility, year), value) in secondary)
            {
                var accumulator = Get(facility, year);
                accumulator.Students = (accumulator.Students ?? 0) + value.Students;
                accumulator.Graduates = (accumulator.Graduates ?? 0) + value.Graduates;
                accumulator.Admitted = (accumulator.Admitted ?? 0) + value.Admitted;
            }

            // VOŠ — data za školní rok 2024/2025 (podle popisu sady)
            foreach (var row in await _importService.LoadAsync("studenti-vos-kraj", ct))
            {
                var facility = Resolve(row, "izo");
                if (facility == null)
                    continue;

                var accumulator = Get(facility, VosYear);
                accumulator.Students = (accumulator.Students ?? 0) + (row.Int("pocet_studentu_celkem") ?? 0);
                accumulator.Graduates = (accumulator.Graduates ?? 0) + (row.Int("pocet_absolventu_2023_2024") ?? 0);
                accumulator.Admitted = (accumulator.Admitted ?? 0) + (row.Int("pocet_prijatych_studentu") ?? 0);
            }

            // Domovy mládeže — rok sada neuvádí
            foreach (var row in (await _importService.LoadAsync("ubytovani-dm", ct)).Where(f => f.Is("zrizovatel", "kraj")))
            {
                var redizo = KeyNormalizer.Redizo(row.Text("identifikator_reditelstvi"));
                if (redizo == null || !byRedizo.TryGetValue(redizo, out var candidates))
                    continue;

                var point = row.Point(Factory);
                var facility = candidates
                    .OrderByDescending(f => f.Kinds.Any(k => AccommodationKinds.Contains(k)))
                    .ThenBy(f => point == null ? 0 : f.Location.Distance(point))
                    .First();

                var accumulator = Get(facility, UnknownYear);
                accumulator.BedCapacity = (accumulator.BedCapacity ?? 0) + (row.Int("pocet_luzkove_kapacity") ?? 0);
                accumulator.Residents = (accumulator.Residents ?? 0) + (row.Int("uhrnny_pocet_ubytovanych") ?? 0);
            }

            // Přijímací řízení SŠ a VOŠ — rok sady neuvádějí
            foreach (var row in (await _importService.LoadAsync("prijimacky-ss", ct)).Where(f => f.Is("zrizovatel", "kraj")))
                AddAdmissions(Resolve(row, "izo"), row.Int("pocet_prihl_1_kolo_prij_riz"), row.Int("pocet_nabizenych_mist"));

            foreach (var row in (await _importService.LoadAsync("prijimacky-vos", ct)).Where(f => f.Is("zrizovatel", "kraj")))
                AddAdmissions(Resolve(row, "izo"), row.Int("pocet_prihlasenych_do_1_kola"), row.Int("pocet_nabizenych_mist"));

            void AddAdmissions(Facility? facility, int? applicants, int? places)
            {
                if (facility == null)
                    return;

                var accumulator = Get(facility, UnknownYear);
                accumulator.Applicants = (accumulator.Applicants ?? 0) + (applicants ?? 0);
                accumulator.OfferedPlaces = (accumulator.OfferedPlaces ?? 0) + (places ?? 0);
            }

            foreach (var ((facility, year), a) in accumulators)
            {
                var metric = new FacilityMetric(facility.Id, year);

                if (a.Students != null)
                    metric.SetStudents(a.Students.Value, a.Graduates, a.Admitted);

                if (a.BedCapacity != null)
                    metric.SetAccommodation(a.BedCapacity.Value, a.Residents ?? 0);

                if (a.Applicants != null)
                    metric.SetAdmissions(a.Applicants.Value, a.OfferedPlaces ?? 0);

                _dbContext.FacilityMetrics.Add(metric);
            }

            return accumulators.Count;
        }

        #endregion

        #region Pomocné

        private static bool IsRegionFounder(string? founder)
        {
            return founder != null && (founder.Equals("kraj", StringComparison.OrdinalIgnoreCase) || founder.Contains("hradecký kraj", StringComparison.OrdinalIgnoreCase));
        }

        private static int KindRank(string? kind)
        {
            var index = Array.IndexOf(SchoolKindPriority, kind);

            return index < 0 ? SchoolKindPriority.Length : index;
        }

        private static string LocationKey(Point point)
        {
            return FormattableString.Invariant($"{point.Y:F4},{point.X:F4}");
        }

        private static string? HouseNumber(RawFeature feature)
        {
            var number = feature.Text("cislo_domovni");
            var orientation = feature.Text("cislo_orientacni");

            if (number == null)
                return null;

            return orientation == null ? number : $"{number}/{orientation}";
        }

        private static string? JoinAddress(string? street, string? number, string? psc, string? post)
        {
            var line = string.Join(' ', new[] { street, number }.Where(p => p != null));
            var city = string.Join(' ', new[] { psc, post }.Where(p => p != null));
            var address = string.Join(", ", new[] { line, city }.Where(p => !string.IsNullOrWhiteSpace(p)));

            return address.Length == 0 ? null : address;
        }

        private static string CategoryPrefix(FacilityCategory category)
        {
            return category switch
            {
                FacilityCategory.School => "SKO",
                FacilityCategory.Social => "SOC",
                FacilityCategory.Health => "ZDR",
                _ => "KUL",
            };
        }

        // Rozdělení profilů ~70 / 10 / 10 / 5 / 5 % — stabilní podle klíče objektu
        private static SimulationProfile ProfileFor(string sourceKey, FacilityCategory category)
        {
            var percent = StableHash.Percent(sourceKey);

            var profile = percent switch
            {
                < 70 => SimulationProfile.Nominal,
                < 80 => SimulationProfile.WeekendOverheating,
                < 90 => SimulationProfile.PoorVentilation,
                < 95 => SimulationProfile.EarlyHeatingStart,
                _ => SimulationProfile.NoNightSetback,
            };

            // Nemocnice jedou 24/7 — víkendový útlum ani ranní náběh u nich nedávají smysl
            if (category == FacilityCategory.Health && profile is SimulationProfile.WeekendOverheating or SimulationProfile.EarlyHeatingStart)
                return SimulationProfile.NoNightSetback;

            return profile;
        }

        #endregion
    }
}
