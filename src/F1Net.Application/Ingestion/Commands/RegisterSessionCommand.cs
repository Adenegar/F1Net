using F1Net.Application.Abstractions;
using F1Net.Domain.Entities;
using F1Net.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace F1Net.Application.Ingestion.Commands;

public record RegisterSessionsForYearCommand(int Year) : IRequest<int>;

public class RegisterSessionsForYearHandler : IRequestHandler<RegisterSessionsForYearCommand, int>
{
    private readonly IOpenF1Client _openF1;
    private readonly IF1NetDbContext _db;

    public RegisterSessionsForYearHandler(IOpenF1Client openF1, IF1NetDbContext db)
    {
        _openF1 = openF1;
        _db = db;
    }

    public async Task<int> Handle(RegisterSessionsForYearCommand req, CancellationToken ct)
    {
        var sessions = await _openF1.GetSessionsAsync(req.Year, ct);
        var season = await _db.Seasons.FirstOrDefaultAsync(s => s.Year == req.Year, ct);
        if (season is null)
        {
            season = new Season { Year = req.Year };
            _db.Seasons.Add(season);
            await _db.SaveChangesAsync(ct);
        }

        var added = 0;
        foreach (var s in sessions)
        {
            var race = await _db.Races.FirstOrDefaultAsync(r => r.SeasonId == season.Id && r.Name == s.CircuitShortName, ct);
            if (race is null)
            {
                var nextRound = await _db.Races.Where(r => r.SeasonId == season.Id).CountAsync(ct) + 1;
                race = new Race
                {
                    SeasonId = season.Id,
                    Round = nextRound,
                    Name = s.CircuitShortName,
                    CircuitName = s.CircuitShortName,
                    Country = s.CountryName,
                    Locality = s.Location,
                };
                _db.Races.Add(race);
                await _db.SaveChangesAsync(ct);
            }

            var existing = await _db.Sessions.FirstOrDefaultAsync(x => x.OpenF1SessionKey == s.SessionKey, ct);
            if (existing is null)
            {
                _db.Sessions.Add(new Session
                {
                    RaceId = race.Id,
                    OpenF1SessionKey = s.SessionKey,
                    Type = ParseType(s.SessionType, s.SessionName),
                    Name = s.SessionName,
                    StartUtc = s.DateStart,
                    EndUtc = s.DateEnd,
                });
                added++;
            }
        }
        await _db.SaveChangesAsync(ct);
        return added;
    }

    private static SessionType ParseType(string type, string name) =>
        (type.ToLowerInvariant(), name.ToLowerInvariant()) switch
        {
            ("race", "race") => SessionType.Race,
            ("race", "sprint") => SessionType.Sprint,
            ("qualifying", _) when name.Contains("sprint") => SessionType.SprintQualifying,
            ("qualifying", _) => SessionType.Qualifying,
            ("practice", var n) when n.Contains("1") => SessionType.Practice1,
            ("practice", var n) when n.Contains("2") => SessionType.Practice2,
            ("practice", var n) when n.Contains("3") => SessionType.Practice3,
            _ => SessionType.Unknown,
        };
}
