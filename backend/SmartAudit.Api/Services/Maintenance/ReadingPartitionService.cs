using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SmartAudit.Api.Services.Simulation;
using SmartAudit.Core.Data;

namespace SmartAudit.Api.Services.Maintenance
{
    // Tabulka SensorReadings je rozdělená na měsíční oddíly (migrace PartitionSensorReadings).
    // Služba drží založené oddíly na aktuální a dva další měsíce, aby měření nikdy nepadala do výchozího oddílu —
    // do něj by pak nešel vložit řádný oddíl pro stejný měsíc.
    public class ReadingPartitionService : BackgroundService
    {
        private const int MonthsAhead = 2;
        private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(12);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPipelineStateService _pipeline;
        private readonly ILogger<ReadingPartitionService> _logger;

        public ReadingPartitionService(IServiceScopeFactory scopeFactory, IPipelineStateService pipeline, ILogger<ReadingPartitionService> logger)
        {
            _scopeFactory = scopeFactory;
            _pipeline = pipeline;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            try
            {
                // Až po migracích — do té doby tabulka nemusí být rozdělená
                await _pipeline.WaitUntilReadyAsync(ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await EnsurePartitionsAsync(DateTimeOffset.UtcNow, ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Oddíly tabulky měření se nepodařilo založit");
                }

                await Task.Delay(CheckInterval, ct);
            }
        }

        private async Task EnsurePartitionsAsync(DateTimeOffset now, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SmartAuditDbContext>();

            var month = new DateOnly(now.Year, now.Month, 1);
            for (var i = 0; i <= MonthsAhead; i++, month = month.AddMonths(1))
            {
                var name = $"SensorReadings_{month:yyyy_MM}";
                var from = month.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var to = month.AddMonths(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                // Název oddílu i hranice skládá aplikace z data, ne z uživatelského vstupu
#pragma warning disable EF1002
                await dbContext.Database.ExecuteSqlRawAsync(
                    $"""CREATE TABLE IF NOT EXISTS "{name}" PARTITION OF "SensorReadings" FOR VALUES FROM ('{from}') TO ('{to}')""", ct);
#pragma warning restore EF1002
            }
        }
    }
}
