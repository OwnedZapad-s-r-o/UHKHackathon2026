using SmartAudit.Core.Domain.Settings;

namespace SmartAudit.Api.Services.Import
{
    // Soubory v seed/opendata — poslední úspěšně stažená data pro start bez sítě
    public class SnapshotStore
    {
        private readonly string _directory;

        public SnapshotStore(OpenDataSettings settings, IWebHostEnvironment environment)
        {
            _directory = Path.GetFullPath(Path.Combine(environment.ContentRootPath, settings.SnapshotPath));
        }

        public async Task WriteAsync(string fileName, string content, CancellationToken ct)
        {
            Directory.CreateDirectory(_directory);

            await File.WriteAllTextAsync(Path.Combine(_directory, fileName), content, ct);
        }

        public async Task<string?> ReadAsync(string fileName, CancellationToken ct)
        {
            var path = Path.Combine(_directory, fileName);

            return File.Exists(path) ? await File.ReadAllTextAsync(path, ct) : null;
        }
    }
}
