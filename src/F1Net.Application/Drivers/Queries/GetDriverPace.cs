using F1Net.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace F1Net.Application.Drivers.Queries;

public record DriverPacePoint(int LapNumber, double LapSeconds, string TyreCompound, bool IsAnomaly);
public record DriverPaceDto(string DriverName, IReadOnlyList<DriverPacePoint> Laps);

public record GetDriverPaceQuery(int SessionId, int DriverId) : IRequest<DriverPaceDto?>;

public class GetDriverPaceHandler : IRequestHandler<GetDriverPaceQuery, DriverPaceDto?>
{
    private readonly IF1NetDbContext _db;
    public GetDriverPaceHandler(IF1NetDbContext db) => _db = db;

    public async Task<DriverPaceDto?> Handle(GetDriverPaceQuery req, CancellationToken ct)
    {
        var driver = await _db.Drivers.FirstOrDefaultAsync(d => d.Id == req.DriverId, ct);
        if (driver is null) return null;

        var anomalyLapIds = await _db.AnomalyFlags
            .Where(f => f.SessionId == req.SessionId)
            .Select(f => f.LapId)
            .ToListAsync(ct);
        var set = anomalyLapIds.ToHashSet();

        var laps = await _db.Laps
            .Where(l => l.SessionId == req.SessionId && l.DriverId == req.DriverId && l.LapTime != null)
            .OrderBy(l => l.LapNumber)
            .Select(l => new { l.Id, l.LapNumber, l.LapTime, l.TyreCompound })
            .ToListAsync(ct);

        return new DriverPaceDto(
            driver.FullName,
            laps.Select(l => new DriverPacePoint(
                l.LapNumber,
                l.LapTime!.Value.TotalSeconds,
                l.TyreCompound.ToString(),
                set.Contains(l.Id))).ToList());
    }
}
