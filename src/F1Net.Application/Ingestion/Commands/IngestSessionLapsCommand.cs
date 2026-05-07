using F1Net.Application.Abstractions;
using F1Net.Domain.Entities;
using F1Net.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace F1Net.Application.Ingestion.Commands;

public record IngestSessionLapsCommand(int OpenF1SessionKey) : IRequest<int>;

public class IngestSessionLapsValidator : AbstractValidator<IngestSessionLapsCommand>
{
    public IngestSessionLapsValidator()
    {
        RuleFor(x => x.OpenF1SessionKey).GreaterThan(0);
    }
}

public class IngestSessionLapsHandler : IRequestHandler<IngestSessionLapsCommand, int>
{
    private readonly IOpenF1Client _openF1;
    private readonly IF1NetDbContext _db;
    private readonly ILogger<IngestSessionLapsHandler> _log;

    public IngestSessionLapsHandler(IOpenF1Client openF1, IF1NetDbContext db, ILogger<IngestSessionLapsHandler> log)
    {
        _openF1 = openF1;
        _db = db;
        _log = log;
    }

    public async Task<int> Handle(IngestSessionLapsCommand req, CancellationToken ct)
    {
        var session = await _db.Sessions.FirstOrDefaultAsync(s => s.OpenF1SessionKey == req.OpenF1SessionKey, ct)
            ?? throw new InvalidOperationException($"Session {req.OpenF1SessionKey} not registered.");

        var drivers = await _openF1.GetDriversAsync(req.OpenF1SessionKey, ct);
        var driverByNumber = new Dictionary<int, Driver>();
        foreach (var d in drivers)
        {
            var driverRef = d.NameAcronym.ToLowerInvariant();
            var driver = await _db.Drivers.FirstOrDefaultAsync(x => x.DriverRef == driverRef, ct);
            if (driver is null)
            {
                driver = new Driver
                {
                    DriverRef = driverRef,
                    FullName = d.FullName,
                    Code = d.NameAcronym,
                    PermanentNumber = d.DriverNumber,
                    Nationality = d.CountryCode,
                };
                _db.Drivers.Add(driver);
                await _db.SaveChangesAsync(ct);
            }
            driverByNumber[d.DriverNumber] = driver;
        }

        var laps = await _openF1.GetLapsAsync(req.OpenF1SessionKey, ct);
        var written = 0;

        foreach (var lap in laps)
        {
            if (!driverByNumber.TryGetValue(lap.DriverNumber, out var driver)) continue;

            var existing = await _db.Laps.Include(l => l.Sectors)
                .FirstOrDefaultAsync(l => l.SessionId == session.Id
                    && l.DriverId == driver.Id
                    && l.LapNumber == lap.LapNumber, ct);

            if (existing is null)
            {
                var entity = new Lap
                {
                    SessionId = session.Id,
                    DriverId = driver.Id,
                    LapNumber = lap.LapNumber,
                    LapTime = lap.LapDuration is { } d ? TimeSpan.FromSeconds(d) : null,
                    TyreCompound = ParseCompound(lap.Compound),
                    TyreAgeLaps = lap.TyreLifeLaps,
                    IsPitOutLap = lap.IsPitOutLap ?? false,
                };
                AddSector(entity, 1, lap.DurationSector1);
                AddSector(entity, 2, lap.DurationSector2);
                AddSector(entity, 3, lap.DurationSector3);
                _db.Laps.Add(entity);
                written++;
            }
        }

        session.LastIngestedUtc = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        _log.LogInformation("Ingested {Count} laps for session {Key}", written, req.OpenF1SessionKey);
        return written;
    }

    private static void AddSector(Lap lap, int n, double? seconds)
    {
        if (seconds is null) return;
        lap.Sectors.Add(new Sector { SectorNumber = n, Duration = TimeSpan.FromSeconds(seconds.Value) });
    }

    private static TyreCompound ParseCompound(string? c) => c?.ToUpperInvariant() switch
    {
        "SOFT" => TyreCompound.Soft,
        "MEDIUM" => TyreCompound.Medium,
        "HARD" => TyreCompound.Hard,
        "INTERMEDIATE" => TyreCompound.Intermediate,
        "WET" => TyreCompound.Wet,
        _ => TyreCompound.Unknown,
    };
}
