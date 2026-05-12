using F1Net.Application.Abstractions;
using F1Net.Application.Anomalies.Models;
using F1Net.Domain.Entities;
using F1Net.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace F1Net.Application.Anomalies.Commands;

public record DetectSessionAnomaliesCommand(int SessionId) : IRequest<int>;

public class DetectSessionAnomaliesHandler : IRequestHandler<DetectSessionAnomaliesCommand, int>
{
    private readonly IF1NetDbContext _db;
    private readonly ILapAnomalyDetector _detector;
    private readonly ILogger<DetectSessionAnomaliesHandler> _log;
    private const string DetectorName = "ZScore-v1";

    public DetectSessionAnomaliesHandler(IF1NetDbContext db, ILapAnomalyDetector detector, ILogger<DetectSessionAnomaliesHandler> log)
    {
        _db = db;
        _detector = detector;
        _log = log;
    }

    public async Task<int> Handle(DetectSessionAnomaliesCommand req, CancellationToken ct)
    {
        var laps = await _db.Laps
            .Include(l => l.Sectors)
            .Where(l => l.SessionId == req.SessionId && l.LapTime != null && !l.IsPitInLap && !l.IsPitOutLap)
            .ToListAsync(ct);

        if (laps.Count < 10) return 0;

        var features = laps.Select(l => new LapFeature(
            LapId: l.Id,
            DriverId: l.DriverId,
            LapNumber: l.LapNumber,
            LapTimeSeconds: l.LapTime!.Value.TotalSeconds,
            Sector1Seconds: l.Sectors.FirstOrDefault(s => s.SectorNumber == 1)?.Duration?.TotalSeconds ?? 0,
            Sector2Seconds: l.Sectors.FirstOrDefault(s => s.SectorNumber == 2)?.Duration?.TotalSeconds ?? 0,
            Sector3Seconds: l.Sectors.FirstOrDefault(s => s.SectorNumber == 3)?.Duration?.TotalSeconds ?? 0,
            TyreAgeLaps: l.TyreAgeLaps ?? 0
        )).ToList();

        var results = _detector.Detect(features);

        var stale = await _db.AnomalyFlags
            .Where(f => f.SessionId == req.SessionId && f.DetectorName == DetectorName)
            .ToListAsync(ct);
        if (stale.Count > 0) _db.AnomalyFlags.RemoveRange(stale);

        var driverMeans = features
            .GroupBy(f => f.DriverId)
            .ToDictionary(g => g.Key, g => g.Average(f => f.LapTimeSeconds));
        var lapById = laps.ToDictionary(l => l.Id);

        var written = 0;
        foreach (var r in results.Where(x => x.IsAnomaly))
        {
            var lap = lapById[r.LapId];
            _db.AnomalyFlags.Add(new AnomalyFlag
            {
                SessionId = req.SessionId,
                LapId = r.LapId,
                DetectorName = DetectorName,
                Severity = ScoreToSeverity(r.Score),
                Score = r.Score,
                DriverMeanLapTime = TimeSpan.FromSeconds(driverMeans[lap.DriverId]),
                DetectedUtc = DateTimeOffset.UtcNow,
            });
            written++;
        }
        await _db.SaveChangesAsync(ct);
        _log.LogInformation("Flagged {Count} anomalous laps in session {Sid}", written, req.SessionId);
        return written;
    }

    private static FlagSeverity ScoreToSeverity(double s) => s switch
    {
        > 5 => FlagSeverity.Critical,
        > 3 => FlagSeverity.Major,
        > 1.5 => FlagSeverity.Minor,
        _ => FlagSeverity.Info,
    };
}
