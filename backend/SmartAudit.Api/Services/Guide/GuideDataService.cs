using MediatR;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.Facilities.GetFacilities;
using SmartAudit.Api.Features.Facilities.GetFacilityDetail;
using SmartAudit.Api.Features.OpenData.GetOpenFacilities;
using SmartAudit.Core.Common;
using SmartAudit.Core.Domain.Enums;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Services.Guide
{
    // Objekt, jak ho vidí průvodce — spojení veřejného záznamu (CO₂ za týden, živá čidla) a interního (Kč jen pro kraj a správce)
    public record GuideFacility(
        Guid Id,
        string Code,
        string Name,
        FacilityCategory Category,
        string Kind,
        string MunicipalityName,
        string DistrictCode,
        string DistrictName,
        double Latitude,
        double Longitude,
        AuditStatus Status,
        string Headline,
        double? Temperature,
        double? Co2,
        double? Co2HoursOverLimit,
        decimal? AnnualLossCzk,
        bool HasLiveSensors,
        HeritageStatus HeritageStatus);

    public record GuideDistrict(string Code, string Name, int Facilities, int WithIssues, int Critical, decimal? AnnualLossCzk, double? AverageCo2, double? Co2HoursOverLimit);

    public enum GuideSort
    {
        Severity,
        Loss,
        Co2,
        Co2Hours,
        // Nejlepší vzduch: nejméně hodin nad limitem, pak nejnižší CO₂
        BestAir,
        Temperature,
        Name,
    }

    // Filtr vyhledávání objektů — stejný pro nástroj modelu i záložní režim bez modelu
    public record GuideSearch(
        string? Text = null,
        string? District = null,
        FacilityCategory? Category = null,
        AuditStatus? Status = null,
        bool OnlyIssues = false,
        bool OnlyLive = false,
        GuideSort Sort = GuideSort.Severity,
        int Limit = 5);

    public record GuideSearchResult(int Total, IReadOnlyList<GuideFacility> Facilities, GuideSearch AppliedSearch);

    public interface IGuideDataService
    {
        bool CanSeeMoney { get; }
        void UseViewer(ViewerContext viewer);
        Task<IReadOnlyList<GuideFacility>> FacilitiesAsync(CancellationToken ct);
        Task<GuideSearchResult> SearchAsync(GuideSearch search, CancellationToken ct);
        Task<IReadOnlyList<GuideDistrict>> DistrictsAsync(CancellationToken ct);
        Task<GuideFacility?> FindAsync(string codeOrName, CancellationToken ct);
        Task<GetFacilityDetailDto?> DetailAsync(Guid facilityId, CancellationToken ct);
        string? ResolveDistrictCode(string? districtCodeOrName, IReadOnlyList<GuideFacility> facilities);
    }

    // Data pro průvodce — jen čtení přes existující handlery, takže platí stejná oprávnění jako v aplikaci:
    // správce vidí jen své budovy, veřejnost nikdy Kč
    public class GuideDataService : IGuideDataService
    {
        public const int MaxResults = 8;

        private readonly IMediator _mediator;
        private ViewerContext _viewer = new(AppRoles.Public, null);
        private IReadOnlyList<GuideFacility>? _facilities;

        public GuideDataService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public bool CanSeeMoney => _viewer.CanSeeMoney;

        // Kdo se ptá — nastaví handler z tokenu před prvním dotazem
        public void UseViewer(ViewerContext viewer)
        {
            _viewer = viewer;
            _facilities = null;
        }

        public async Task<IReadOnlyList<GuideFacility>> FacilitiesAsync(CancellationToken ct)
        {
            if (_facilities != null)
                return _facilities;

            var open = await _mediator.Send(new GetOpenFacilitiesCommand(), ct);
            var openFacilities = open.Data ?? [];

            if (_viewer.Role == AppRoles.Public)
            {
                _facilities = openFacilities.Select(f => ToGuide(f, null)).ToList();
                return _facilities;
            }

            // Kraj a správce: seznam podle oprávnění (správce jen svoje budovy) a k němu ztráta v Kč
            var own = await _mediator.Send(new GetFacilitiesCommand(_viewer), ct);
            var losses = (own.Data ?? []).ToDictionary(f => f.Id, f => f.AnnualLossCzk);

            _facilities = openFacilities
                .Where(f => losses.ContainsKey(f.Id))
                .Select(f => ToGuide(f, losses[f.Id]))
                .ToList();

            return _facilities;
        }

        public async Task<GuideSearchResult> SearchAsync(GuideSearch search, CancellationToken ct)
        {
            var facilities = await FacilitiesAsync(ct);

            // Veřejnost řadit podle Kč nemůže — částky nevidí
            var applied = search with
            {
                District = ResolveDistrictCode(search.District, facilities),
                Sort = search.Sort == GuideSort.Loss && !CanSeeMoney ? GuideSort.Severity : search.Sort,
                Limit = Math.Clamp(search.Limit, 1, MaxResults),
            };

            var words = string.IsNullOrWhiteSpace(applied.Text) ? [] : GuideText.Normalize(applied.Text).Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var matches = facilities
                .Where(f => applied.District == null || f.DistrictCode == applied.District)
                .Where(f => applied.Category == null || f.Category == applied.Category)
                .Where(f => applied.Status == null || f.Status == applied.Status)
                .Where(f => !applied.OnlyIssues || f.Status != AuditStatus.Nominal)
                .Where(f => !applied.OnlyLive || f.HasLiveSensors)
                .Where(f => words.Length == 0 || MatchesAll(f, words))
                .ToList();

            return new GuideSearchResult(matches.Count, Sort(matches, applied.Sort).Take(applied.Limit).ToList(), applied);
        }

        public async Task<IReadOnlyList<GuideDistrict>> DistrictsAsync(CancellationToken ct)
        {
            var facilities = await FacilitiesAsync(ct);

            return facilities
                .GroupBy(f => (f.DistrictCode, f.DistrictName))
                .Select(g => new GuideDistrict(
                    g.Key.DistrictCode,
                    g.Key.DistrictName,
                    g.Count(),
                    g.Count(f => f.Status != AuditStatus.Nominal),
                    g.Count(f => f.Status == AuditStatus.Critical),
                    CanSeeMoney ? g.Sum(f => f.AnnualLossCzk ?? 0) : null,
                    Average(g.Select(f => f.Co2)),
                    Average(g.Select(f => f.Co2HoursOverLimit))))
                .OrderByDescending(d => (double)d.WithIssues / Math.Max(1, d.Facilities))
                .ToList();
        }

        // Kód objektu přesně, jinak nejlepší shoda názvu (všechna slova dotazu v názvu nebo obci)
        public async Task<GuideFacility?> FindAsync(string codeOrName, CancellationToken ct)
        {
            var facilities = await FacilitiesAsync(ct);
            var needle = codeOrName.Trim();

            var byCode = facilities.FirstOrDefault(f => string.Equals(f.Code, needle, StringComparison.OrdinalIgnoreCase));
            if (byCode != null)
                return byCode;

            var words = GuideText.Normalize(needle).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
                return null;

            return facilities
                .Where(f => MatchesAll(f, words))
                .OrderBy(f => f.Name.Length)
                .FirstOrDefault();
        }

        public async Task<GetFacilityDetailDto?> DetailAsync(Guid facilityId, CancellationToken ct)
        {
            var detail = await _mediator.Send(new GetFacilityDetailCommand(facilityId, _viewer), ct);

            return detail.IsSuccess ? detail.Data : null;
        }

        // Okres podle kódu (CZ0523) nebo názvu bez ohledu na diakritiku a koncovku („Náchodě“, „rychnov“)
        public string? ResolveDistrictCode(string? districtCodeOrName, IReadOnlyList<GuideFacility> facilities)
        {
            if (string.IsNullOrWhiteSpace(districtCodeOrName))
                return null;

            var districts = facilities.Select(f => (f.DistrictCode, f.DistrictName)).Distinct().ToList();
            var needle = GuideText.Normalize(districtCodeOrName);

            var byCode = districts.FirstOrDefault(d => string.Equals(d.DistrictCode, districtCodeOrName.Trim(), StringComparison.OrdinalIgnoreCase));
            if (byCode.DistrictCode != null)
                return byCode.DistrictCode;

            var stem = GuideText.Stem(needle.Split(' ')[0]);
            var byName = districts.FirstOrDefault(d => GuideText.Normalize(d.DistrictName).StartsWith(stem.Length >= 4 ? stem[..4] : stem, StringComparison.Ordinal));

            return byName.DistrictCode;
        }

        private static bool MatchesAll(GuideFacility facility, string[] words)
        {
            var haystack = GuideText.Normalize($"{facility.Code} {facility.Name} {facility.MunicipalityName} {facility.Kind}");

            // Kmen slova, aby „Jičíně“ našlo „Jičín“ a „gymnáziu“ „gymnázium“
            return words.All(w => haystack.Contains(w.Length > 5 ? w[..^2] : w, StringComparison.Ordinal));
        }

        private static IEnumerable<GuideFacility> Sort(IEnumerable<GuideFacility> facilities, GuideSort sort) => sort switch
        {
            GuideSort.Loss => facilities.OrderByDescending(f => f.AnnualLossCzk ?? 0),
            GuideSort.Co2 => facilities.OrderByDescending(f => f.Co2 ?? 0),
            GuideSort.Co2Hours => facilities.OrderByDescending(f => f.Co2HoursOverLimit ?? 0).ThenByDescending(f => f.Co2 ?? 0),
            GuideSort.BestAir => facilities.Where(f => f.Co2 != null).OrderBy(f => f.Co2HoursOverLimit ?? 0).ThenBy(f => f.Co2),
            GuideSort.Temperature => facilities.OrderByDescending(f => f.Temperature ?? 0),
            GuideSort.Name => facilities.OrderBy(f => f.Name, StringComparer.Create(System.Globalization.CultureInfo.GetCultureInfo("cs-CZ"), false)),
            _ => facilities.OrderByDescending(f => f.Status).ThenByDescending(f => f.AnnualLossCzk ?? 0).ThenByDescending(f => f.Co2HoursOverLimit ?? 0),
        };

        private static double? Average(IEnumerable<double?> values)
        {
            var present = values.Where(v => v != null).Select(v => v!.Value).ToList();
            return present.Count == 0 ? null : Math.Round(present.Average(), 1);
        }

        private static GuideFacility ToGuide(OpenFacilityDto f, decimal? annualLossCzk) => new(
            f.Id, f.Code, f.Name, f.Category, f.Kind, f.MunicipalityName, f.DistrictCode, f.DistrictName, f.Latitude, f.Longitude,
            f.Status, f.Headline, f.Temperature, f.Co2, f.Co2HoursOverLimit, annualLossCzk, f.HasLiveSensors, f.HeritageStatus);
    }
}
