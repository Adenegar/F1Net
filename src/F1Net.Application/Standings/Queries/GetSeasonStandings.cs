using F1Net.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace F1Net.Application.Standings.Queries;

public record StandingDto(int Position, string DriverName, string? Code, decimal Points, int Wins, string? TeamName);

public record GetSeasonStandingsQuery(int Year) : IRequest<IReadOnlyList<StandingDto>>;

public class GetSeasonStandingsHandler : IRequestHandler<GetSeasonStandingsQuery, IReadOnlyList<StandingDto>>
{
    private readonly IF1NetDbContext _db;
    public GetSeasonStandingsHandler(IF1NetDbContext db) => _db = db;

    public async Task<IReadOnlyList<StandingDto>> Handle(GetSeasonStandingsQuery req, CancellationToken ct)
    {
        return await _db.Standings
            .Where(s => s.Season.Year == req.Year && s.AfterRound == null)
            .OrderBy(s => s.Position)
            .Select(s => new StandingDto(
                s.Position,
                s.Driver.FullName,
                s.Driver.Code,
                s.Points,
                s.Wins,
                s.Driver.Team!.Name))
            .ToListAsync(ct);
    }
}
