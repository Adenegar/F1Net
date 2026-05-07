using F1Net.Application.Abstractions;
using F1Net.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace F1Net.Application.Ingestion.Commands;

public record IngestStandingsCommand(int Year) : IRequest<int>;

public class IngestStandingsValidator : AbstractValidator<IngestStandingsCommand>
{
    public IngestStandingsValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(1950, 2100);
    }
}

public class IngestStandingsHandler : IRequestHandler<IngestStandingsCommand, int>
{
    private readonly IErgastClient _ergast;
    private readonly IF1NetDbContext _db;
    private readonly ILogger<IngestStandingsHandler> _log;

    public IngestStandingsHandler(IErgastClient ergast, IF1NetDbContext db, ILogger<IngestStandingsHandler> log)
    {
        _ergast = ergast;
        _db = db;
        _log = log;
    }

    public async Task<int> Handle(IngestStandingsCommand req, CancellationToken ct)
    {
        var ergastSeason = await _ergast.GetSeasonAsync(req.Year, ct);
        var standings = await _ergast.GetDriverStandingsAsync(req.Year, ct);

        var season = await _db.Seasons.FirstOrDefaultAsync(s => s.Year == req.Year, ct);
        if (season is null)
        {
            season = new Season { Year = req.Year, Url = ergastSeason.Url };
            _db.Seasons.Add(season);
            await _db.SaveChangesAsync(ct);
        }

        var written = 0;
        foreach (var s in standings)
        {
            var driver = await _db.Drivers.FirstOrDefaultAsync(d => d.DriverRef == s.DriverRef, ct);
            if (driver is null)
            {
                driver = new Driver
                {
                    DriverRef = s.DriverRef,
                    FullName = s.FullName,
                    Code = s.DriverCode,
                    PermanentNumber = s.PermanentNumber,
                    Nationality = s.Nationality,
                };
                _db.Drivers.Add(driver);
                await _db.SaveChangesAsync(ct);
            }

            var existing = await _db.Standings.FirstOrDefaultAsync(
                x => x.SeasonId == season.Id && x.DriverId == driver.Id && x.AfterRound == null, ct);

            if (existing is null)
            {
                _db.Standings.Add(new Standing
                {
                    SeasonId = season.Id,
                    DriverId = driver.Id,
                    Position = s.Position,
                    Points = s.Points,
                    Wins = s.Wins,
                    AfterRound = null,
                });
            }
            else
            {
                existing.Position = s.Position;
                existing.Points = s.Points;
                existing.Wins = s.Wins;
            }
            written++;
        }

        await _db.SaveChangesAsync(ct);
        _log.LogInformation("Ingested {Count} standings for {Year}", written, req.Year);
        return written;
    }
}
