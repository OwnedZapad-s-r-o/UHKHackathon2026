using System.Net;
using System.Text.Json;
using SmartAudit.Core.Domain.Helpers;

namespace SmartAudit.Api.Services.Contracts
{
    public interface IContractRegisterService
    {
        Task<IReadOnlyList<RegistrySearchRow>> SearchAsync(string ico, string subject, CancellationToken ct);
        Task<RegistryContract?> GetContractAsync(string versionId, CancellationToken ct);
        Task<Stream?> DownloadAttachmentAsync(string attachmentUrl, CancellationToken ct);
    }

    // Registr smluv (DIA) — podrobné vyhledávání podle IČO publikujícího subjektu a podřetězce předmětu smlouvy.
    // Vyhledávání je formulář Nette: výsledky vrací jen v rámci session (cookie PHPSESSID), jinak přesměruje na prázdný formulář.
    // Cookie si služba drží sama (handler cookies nepoužívá): PHP zamyká session na dobu požadavku, takže jeden zaseklý
    // dotaz zablokuje všechny další ve stejné session — po chybě se proto začíná s novou.
    public class ContractRegisterService : IContractRegisterService
    {
        private const string SearchPath = "/vyhledavani";
        private const string AttachmentPrefix = ContractRegistryParser.BaseUrl + "/smlouva/soubor/";

        // Jeden dotaz registr běžně vyřídí do pár sekund — déle znamená zaseklou session
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(20);
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

        // Registr omezuje počet dotazů (429) — počkat podle Retry-After, jinak minutu; pak to vzdát
        private const int MaxRateLimitWaits = 3;
        private static readonly TimeSpan DefaultRateLimitWait = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan MaxRateLimitWait = TimeSpan.FromMinutes(3);

        // Všechna pole formuláře musí v dotazu být, i prázdná — jinak registr filtr nepoužije
        private static readonly string[] FormFields =
        [
            "contract_id", "version_id", "publication_date[from]", "publication_date[to]", "subject_box", "subject_name", "subject_idnum",
            "subject_address", "party_box", "party_name", "party_idnum", "party_address", "contr_num", "contract_reference_number",
            "sign_date[from]", "sign_date[to]", "contract_descr", "sign_person_name", "value_no_vat[from]", "value_no_vat[to]",
            "value_vat[from]", "value_vat[to]", "value_foreign[from]", "value_foreign[to]", "foreign_currency", "file_text",
        ];

        private readonly HttpClient _httpClient;
        private string? _sessionCookie;

        public ContractRegisterService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<RegistrySearchRow>> SearchAsync(string ico, string subject, CancellationToken ct)
        {
            var query = new List<KeyValuePair<string, string>>
            {
                new("searchResultList-limit", "500"),
                new("searchResultList-offset", "0"),
                new("do", "searchResultList-setLimit"),
            };
            query.AddRange(FormFields.Select(field => new KeyValuePair<string, string>(field, field switch
            {
                "subject_idnum" => ico,
                "contract_descr" => subject,
                _ => string.Empty,
            })));
            query.Add(new("search_type", "0"));

            var url = $"{SearchPath}?{await new FormUrlEncodedContent(query).ReadAsStringAsync(ct)}";

            // Bez session registr odpoví {"redirect": …}; při chybě 500 nebo zaseklém dotazu nová session a ještě jeden pokus
            for (var attempt = 0; attempt < 2; attempt++)
            {
                string body;
                try
                {
                    _sessionCookie ??= await StartSessionAsync(ct);

                    using var response = await GetAsync(url, ajax: true, _sessionCookie, ct);
                    response.EnsureSuccessStatusCode();
                    body = await response.Content.ReadAsStringAsync(ct);
                }
                catch (Exception ex) when (attempt == 0 && IsTransient(ex, ct))
                {
                    _sessionCookie = null;
                    await Task.Delay(RetryDelay, ct);
                    continue;
                }

                using var document = JsonDocument.Parse(body);
                if (document.RootElement.TryGetProperty("snippets", out var snippets))
                {
                    var html = string.Concat(snippets.EnumerateObject().Select(s => s.Value.GetString()));
                    return ContractRegistryParser.ParseSearchResults(html);
                }

                _sessionCookie = null;
            }

            throw new HttpRequestException("Registr smluv nevrátil výsledky vyhledávání (session se nepodařilo založit).");
        }

        // Detail smlouvy session nepotřebuje
        public async Task<RegistryContract?> GetContractAsync(string versionId, CancellationToken ct)
        {
            using var response = await GetAsync($"/smlouva/{Uri.EscapeDataString(versionId)}", ajax: false, cookie: null, ct);
            response.EnsureSuccessStatusCode();

            return ContractRegistryParser.ParseDetail(await response.Content.ReadAsStringAsync(ct));
        }

        // Jen soubory z registru — adresa pochází z naší DB, ale ať proxy nikdy nestahuje odjinud.
        // Bez čekání na 429 — volá se i z požadavku uživatele, který nemá minutu čekat.
        public async Task<Stream?> DownloadAttachmentAsync(string attachmentUrl, CancellationToken ct)
        {
            if (!attachmentUrl.StartsWith(AttachmentPrefix, StringComparison.Ordinal))
                return null;

            var response = await _httpClient.GetAsync(attachmentUrl, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
            {
                response.Dispose();
                return null;
            }

            return await response.Content.ReadAsStreamAsync(ct);
        }

        // Nová session = formulář bez cookie; registr pošle PHPSESSID (a další cookies) v Set-Cookie
        private async Task<string> StartSessionAsync(CancellationToken ct)
        {
            using var response = await GetAsync(SearchPath, ajax: false, cookie: null, ct);
            response.EnsureSuccessStatusCode();

            var cookies = response.Headers.TryGetValues("Set-Cookie", out var values)
                ? values.Select(v => v.Split(';', 2)[0].Trim()).Where(v => v.Contains('='))
                : [];

            return string.Join("; ", cookies);
        }

        // Zaseklé spojení (Docker NAT potichu zahodí nečinné spojení a čtení pak visí) se po RequestTimeout zkusí jednou znovu
        private async Task<HttpResponseMessage> GetAsync(string url, bool ajax, string? cookie, CancellationToken ct)
        {
            var timedOut = false;

            for (var wait = 0; ; )
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (ajax)
                    request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                if (!string.IsNullOrEmpty(cookie))
                    request.Headers.Add("Cookie", cookie);

                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeout.CancelAfter(RequestTimeout);

                HttpResponseMessage response;
                try
                {
                    response = await _httpClient.SendAsync(request, timeout.Token);
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested && !timedOut)
                {
                    timedOut = true;
                    continue;
                }

                if (response.StatusCode != HttpStatusCode.TooManyRequests || wait++ >= MaxRateLimitWaits)
                    return response;

                var delay = response.Headers.RetryAfter?.Delta ?? DefaultRateLimitWait;
                response.Dispose();

                await Task.Delay(delay > MaxRateLimitWait ? MaxRateLimitWait : delay, ct);
            }
        }

        // Chyba serveru nebo vypršelý čas dotazu (ne zrušení celé synchronizace)
        private static bool IsTransient(Exception ex, CancellationToken ct)
        {
            return ex switch
            {
                HttpRequestException { StatusCode: >= HttpStatusCode.InternalServerError } => true,
                OperationCanceledException => !ct.IsCancellationRequested,
                _ => false,
            };
        }
    }
}
