using System.Text.RegularExpressions;
using SmartAudit.Core.Domain.Helpers;
using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services.Guide
{
    // Jeden úsek znalostí — kapitola dokumentu z Resources/Guide nebo popis datové sady.
    // Internal = jen pro přihlášený kraj a správce (Kč, AI analýza, párování čidel), veřejnost ho nedostane.
    // Links = odkazy na stránky aplikace, které k úseku patří (řádky „→ [Popisek](/adresa)“ v dokumentu)
    public record KnowledgeChunk(string Id, string Title, string Text, string Source, string? Url, bool Internal, IReadOnlyList<GuideLink> Links);

    public record KnowledgeHit(KnowledgeChunk Chunk, double Score);

    public interface IGuideKnowledgeBase
    {
        Task<IReadOnlyList<KnowledgeHit>> SearchAsync(string query, bool includeInternal, int take, CancellationToken ct);
    }

    // Znalostní báze průvodce (RAG): hybridní vyhledávání — BM25 nad slovy (funguje vždy, i bez internetu)
    // a kosinová podobnost embeddingů z OpenAI, když je model nastavený. Úseků je pár desítek, index žije v paměti.
    public partial class GuideKnowledgeBase : IGuideKnowledgeBase
    {
        public const string KnowledgeSource = "Znalosti SmartAuditu";
        public const string DatasetSource = "Katalog datových sad";

        // Váhy hybridního skóre — embeddingy chápou parafráze („vydýchaný vzduch“ ~ „CO₂“), slova přesné termíny
        private const double LexicalWeight = 0.45;
        private const double SemanticWeight = 0.55;

        // Pod touto podobností úsek s dotazem nesouvisí
        private const double MinSemantic = 0.28;
        private const double MinLexical = 1.2;

        // BM25 parametry (obvyklé výchozí hodnoty)
        private const double K1 = 1.4;
        private const double B = 0.75;

        // Po chybě embeddingové služby chvíli hledat jen podle slov, ať každý dotaz nečeká na timeout
        private static readonly TimeSpan EmbeddingRetryAfter = TimeSpan.FromMinutes(5);

        private readonly IReadOnlyList<KnowledgeChunk> _chunks;
        private readonly IReadOnlyList<IReadOnlyList<string>> _chunkTerms;
        private readonly Dictionary<string, int> _documentFrequency = new();
        private readonly double _averageLength;

        private readonly IAiClientFactory _clients;
        private readonly ILogger<GuideKnowledgeBase> _logger;
        private readonly SemaphoreSlim _embeddingLock = new(1, 1);
        private float[][]? _chunkEmbeddings;
        private DateTimeOffset _embeddingsFailedAt = DateTimeOffset.MinValue;

        public GuideKnowledgeBase(IWebHostEnvironment environment, OpenDataSettings openData, IAiClientFactory clients, ILogger<GuideKnowledgeBase> logger)
        {
            _clients = clients;
            _logger = logger;

            _chunks = LoadDocuments(Path.Combine(environment.ContentRootPath, "Resources", "Guide"))
                .Concat(DatasetChunks(openData))
                .ToList();

            // Nadpis se počítá dvakrát — krátký a výstižný, má větší váhu než slovo v textu
            _chunkTerms = _chunks.Select(c => GuideText.Terms($"{c.Title} {c.Title} {c.Text}")).ToList();
            _averageLength = _chunkTerms.Count == 0 ? 1 : _chunkTerms.Average(t => t.Count);

            foreach (var term in _chunkTerms.SelectMany(t => t.Distinct()))
                _documentFrequency[term] = _documentFrequency.GetValueOrDefault(term) + 1;

            _logger.LogInformation("Znalosti průvodce: {Count} úseků", _chunks.Count);
        }

        public async Task<IReadOnlyList<KnowledgeHit>> SearchAsync(string query, bool includeInternal, int take, CancellationToken ct)
        {
            var queryTerms = GuideText.Terms(query);
            var lexical = _chunkTerms.Select(terms => Bm25(queryTerms, terms)).ToArray();
            var semantic = await SemanticScoresAsync(query, ct);

            var maxLexical = lexical.DefaultIfEmpty(0).Max();

            return _chunks
                .Select((chunk, i) =>
                {
                    var lexicalNorm = maxLexical > 0 ? lexical[i] / maxLexical : 0;
                    var relevant = semantic != null ? semantic[i] >= MinSemantic || lexical[i] >= MinLexical : lexical[i] >= MinLexical;
                    var score = semantic != null ? LexicalWeight * lexicalNorm + SemanticWeight * semantic[i] : lexicalNorm;

                    return (Hit: new KnowledgeHit(chunk, score), Relevant: relevant);
                })
                .Where(x => x.Relevant && (includeInternal || !x.Hit.Chunk.Internal))
                .OrderByDescending(x => x.Hit.Score)
                .Take(take)
                .Select(x => x.Hit)
                .ToList();
        }

        private double Bm25(IReadOnlyList<string> queryTerms, IReadOnlyList<string> documentTerms)
        {
            if (queryTerms.Count == 0 || documentTerms.Count == 0)
                return 0;

            var score = 0.0;
            var lengthNorm = 1 - B + B * documentTerms.Count / _averageLength;

            foreach (var term in queryTerms.Distinct())
            {
                var frequency = documentTerms.Count(t => t == term);
                if (frequency == 0)
                    continue;

                var df = _documentFrequency.GetValueOrDefault(term);
                var idf = Math.Log(1 + (_chunks.Count - df + 0.5) / (df + 0.5));
                score += idf * frequency * (K1 + 1) / (frequency + K1 * lengthNorm);
            }

            return score;
        }

        // Kosinová podobnost dotazu ke každému úseku, nebo null, když embeddingy nejsou k dispozici
        private async Task<double[]?> SemanticScoresAsync(string query, CancellationToken ct)
        {
            if (!_clients.IsEmbeddingConfigured || DateTimeOffset.UtcNow - _embeddingsFailedAt < EmbeddingRetryAfter)
                return null;

            try
            {
                var chunkEmbeddings = await ChunkEmbeddingsAsync(ct);
                var result = await _clients.CreateEmbeddingClient().GenerateEmbeddingAsync(query, cancellationToken: ct);
                var queryVector = result.Value.ToFloats().ToArray();

                return chunkEmbeddings.Select(vector => Cosine(queryVector, vector)).ToArray();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _embeddingsFailedAt = DateTimeOffset.UtcNow;
                _logger.LogWarning(ex, "Embeddingy průvodce nejsou dostupné — hledám jen podle slov");
                return null;
            }
        }

        // Embeddingy úseků se spočítají jednou při prvním dotazu (jedno dávkové volání) a drží se v paměti
        private async Task<float[][]> ChunkEmbeddingsAsync(CancellationToken ct)
        {
            if (_chunkEmbeddings != null)
                return _chunkEmbeddings;

            await _embeddingLock.WaitAsync(ct);
            try
            {
                if (_chunkEmbeddings != null)
                    return _chunkEmbeddings;

                var inputs = _chunks.Select(c => $"{c.Title}\n{c.Text}").ToList();
                var result = await _clients.CreateEmbeddingClient().GenerateEmbeddingsAsync(inputs, cancellationToken: ct);

                _chunkEmbeddings = result.Value.OrderBy(e => e.Index).Select(e => e.ToFloats().ToArray()).ToArray();
                _logger.LogInformation("Znalosti průvodce: spočítáno {Count} embeddingů", _chunkEmbeddings.Length);

                return _chunkEmbeddings;
            }
            finally
            {
                _embeddingLock.Release();
            }
        }

        private static double Cosine(float[] a, float[] b)
        {
            double dot = 0, normA = 0, normB = 0;
            for (var i = 0; i < Math.Min(a.Length, b.Length); i++)
            {
                dot += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }

            return normA == 0 || normB == 0 ? 0 : dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }

        // Kapitola = nadpis „## …“ a text pod ním. Soubory interni-*.md jsou jen pro přihlášené
        private static IEnumerable<KnowledgeChunk> LoadDocuments(string directory)
        {
            if (!Directory.Exists(directory))
                yield break;

            foreach (var path in Directory.EnumerateFiles(directory, "*.md").Order())
            {
                var file = Path.GetFileNameWithoutExtension(path);
                var isInternal = file.StartsWith("interni-", StringComparison.OrdinalIgnoreCase);
                var sections = File.ReadAllText(path).Split("\n## ", StringSplitOptions.RemoveEmptyEntries);

                for (var i = 0; i < sections.Length; i++)
                {
                    var lines = sections[i].TrimStart('#', ' ').Split('\n').Select(l => l.Trim()).ToList();
                    var title = lines[0];
                    var links = lines.Skip(1).Select(ParseLink).OfType<GuideLink>().ToList();
                    var text = string.Join("\n", lines.Skip(1).Where(l => l.Length > 0 && ParseLink(l) == null));

                    if (title.Length > 0 && text.Length > 0)
                        yield return new KnowledgeChunk($"{file}#{i}", title, text, KnowledgeSource, null, isInternal, links);
                }
            }
        }

        // Datové sady z appsettings — průvodce tak umí odpovědět „odkud jsou data o …“ s odkazem na zdroj
        private static IEnumerable<KnowledgeChunk> DatasetChunks(OpenDataSettings openData)
        {
            return openData.Datasets.Select(d => new KnowledgeChunk(
                $"dataset#{d.Key}",
                $"Datová sada: {d.Title}",
                $"{d.Title} ({d.Provider}, skupina {d.Group}). K čemu ji SmartAudit používá: {d.Purpose}",
                DatasetSource,
                d.PortalUrl,
                false,
                []));
        }

        // „→ [Popisek](/adresa)“ — jen adresy uvnitř aplikace
        private static GuideLink? ParseLink(string line)
        {
            var match = LinkLine().Match(line);
            return match.Success ? new GuideLink(match.Groups[1].Value, match.Groups[2].Value) : null;
        }

        [GeneratedRegex(@"^→\s*\[([^\]]+)\]\((/[^)\s]*)\)$")]
        private static partial Regex LinkLine();
    }
}
