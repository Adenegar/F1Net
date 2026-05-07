using F1Net.Application.Anomalies.Commands;
using F1Net.Application.Ingestion.Commands;
using F1Net.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace F1Net.Infrastructure.BackgroundJobs;

public class TelemetryIngestionService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IOptionsMonitor<IngestionOptions> _options;
    private readonly ILogger<TelemetryIngestionService> _log;

    public TelemetryIngestionService(IServiceProvider services, IOptionsMonitor<IngestionOptions> options, ILogger<TelemetryIngestionService> log)
    {
        _services = services;
        _options = options;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // small delay so app finishes startup (migrations, seeders) before first run
        try { await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            var opt = _options.CurrentValue;
            if (opt.Enabled)
            {
                try { await RunOnceAsync(opt, stoppingToken); }
                catch (Exception ex) { _log.LogError(ex, "Ingestion cycle failed"); }
            }

            try { await Task.Delay(opt.Interval, stoppingToken); }
            catch (OperationCanceledException) { return; }
        }
    }

    public async Task RunOnceAsync(IngestionOptions opt, CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var db = scope.ServiceProvider.GetRequiredService<F1NetDbContext>();

        await mediator.Send(new IngestStandingsCommand(opt.CurrentYear), ct);
        await mediator.Send(new RegisterSessionsForYearCommand(opt.CurrentYear), ct);

        var staleSessions = await db.Sessions
            .Where(s => s.LastIngestedUtc == null
                || (s.EndUtc != null && s.EndUtc > s.LastIngestedUtc))
            .OrderByDescending(s => s.StartUtc)
            .Take(20)
            .Select(s => new { s.Id, s.OpenF1SessionKey })
            .ToListAsync(ct);

        foreach (var s in staleSessions)
        {
            await mediator.Send(new IngestSessionLapsCommand(s.OpenF1SessionKey), ct);
            if (opt.DetectAnomaliesAfterIngest)
                await mediator.Send(new DetectSessionAnomaliesCommand(s.Id), ct);
        }

        _log.LogInformation("Ingestion cycle complete: {Sessions} session(s) processed", staleSessions.Count);
    }
}
