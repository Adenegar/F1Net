using F1Net.Application.Abstractions;
using F1Net.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace F1Net.Application.Sessions.Queries;

public record SessionListItem(int Id, string Name, SessionType Type, string RaceName, DateTimeOffset? StartUtc, int LapCount, int AnomalyCount);

public record GetRecentSessionsQuery(int Take = 10) : IRequest<IReadOnlyList<SessionListItem>>;

public class GetRecentSessionsHandler : IRequestHandler<GetRecentSessionsQuery, IReadOnlyList<SessionListItem>>
{
    private readonly IF1NetDbContext _db;
    public GetRecentSessionsHandler(IF1NetDbContext db) => _db = db;

    public async Task<IReadOnlyList<SessionListItem>> Handle(GetRecentSessionsQuery req, CancellationToken ct)
    {
        return await _db.Sessions
            .OrderByDescending(s => s.StartUtc)
            .Take(req.Take)
            .Select(s => new SessionListItem(
                s.Id,
                s.Name,
                s.Type,
                s.Race.Name,
                s.StartUtc,
                s.Laps.Count,
                s.AnomalyFlags.Count))
            .ToListAsync(ct);
    }
}
