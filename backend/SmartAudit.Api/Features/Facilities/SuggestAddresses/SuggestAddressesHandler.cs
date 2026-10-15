using MediatR;
using SmartAudit.Api.Services;
using SmartAudit.Core.Domain;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Features.Facilities.SuggestAddresses
{
    public record SuggestAddressesCommand(string? Query) : IRequest<Response<IReadOnlyCollection<AddressSuggestionDto>>>;

    // Key = kód adresního místa RÚIAN, podle něj se dohledá přesná poloha
    public record AddressSuggestionDto(string Key, string Label, string? Street, string? HouseNumber, string? Psc, string? Post);

    // Našeptávač adres pro přidání budovy — oficiální registr adres (RÚIAN), jen adresy s PSČ Královéhradeckého kraje
    public class SuggestAddressesHandler : IRequestHandler<SuggestAddressesCommand, Response<IReadOnlyCollection<AddressSuggestionDto>>>
    {
        private const int MinQueryLength = 3;
        private const int MaxSuggestions = 8;

        private readonly IRuianGeocodingService _geocoding;

        public SuggestAddressesHandler(IRuianGeocodingService geocoding)
        {
            _geocoding = geocoding;
        }

        public async Task<Response<IReadOnlyCollection<AddressSuggestionDto>>> Handle(SuggestAddressesCommand request, CancellationToken ct)
        {
            var query = request.Query?.Trim() ?? string.Empty;

            if (query.Length < MinQueryLength)
                return Response.Success<IReadOnlyCollection<AddressSuggestionDto>>([]);

            if (query.Length > 120)
                return Response.ValidationFailed<IReadOnlyCollection<AddressSuggestionDto>>("Adresa je příliš dlouhá");

            IReadOnlyList<RuianSuggestion> suggestions;
            try
            {
                suggestions = await _geocoding.SuggestAsync(query, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return Response.Unavailable<IReadOnlyCollection<AddressSuggestionDto>>("Registr adres ČÚZK teď neodpovídá. Budovu můžete najít kliknutím do mapy.");
            }

            var addresses = suggestions
                .Select(s => (Suggestion: s, Address: RuianAddress.Parse(s.Text)))
                .Where(s => RuianAddress.IsRegionalPostalCode(s.Address.Psc))
                .Take(MaxSuggestions)
                .Select(s => new AddressSuggestionDto(s.Suggestion.Key, s.Suggestion.Text, s.Address.Street, s.Address.HouseNumber, s.Address.Psc, s.Address.Post))
                .ToList();

            return Response.Success<IReadOnlyCollection<AddressSuggestionDto>>(addresses);
        }
    }
}
