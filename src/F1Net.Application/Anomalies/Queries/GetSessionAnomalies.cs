using F1Net.Application.Abstractions;
using F1Net.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace F1Net.Application.Anomalies.Queries;

public record AnomalyDto(
    int LapId,
    int LapNumber,
    string DriverName,
    FlagSeverity Severity,
    double Score,
    TimeSpan? LapTime,
    TimeSpan? DriverMeanLapTime);

public record GetSessionAnomaliesQuery(int SessionId) : IRequest<IReadOnlyList<AnomalyDto>>;

public class GetSessionAnomaliesHandler : IRequestHandler<GetSessionAnomaliesQuery, IReadOnlyList<AnomalyDto>>
{
    private readonly IF1NetDbContext _db;
    public GetSessionAnomaliesHandler(IF1NetDbContext db) => _db = db;

    public async Task<IReadOnlyList<AnomalyDto>> Handle(GetSessionAnomaliesQuery req, CancellationToken ct)
    {
        return await _db.AnomalyFlags
            .Where(f => f.SessionId == req.SessionId)
            .OrderByDescending(f => f.Score)
            .Select(f => new AnomalyDto(
                f.LapId,
                f.Lap.LapNumber,
                f.Lap.Driver.FullName,
                f.Severity,
                f.Score,
                f.Lap.LapTime,
                f.DriverMeanLapTime))
            .ToListAsync(ct);
    }
}
