using F1Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace F1Net.Application.Abstractions;

public interface IF1NetDbContext
{
    DbSet<Team> Teams { get; }
    DbSet<Driver> Drivers { get; }
    DbSet<Season> Seasons { get; }
    DbSet<Race> Races { get; }
    DbSet<Session> Sessions { get; }
    DbSet<Lap> Laps { get; }
    DbSet<Sector> Sectors { get; }
    DbSet<CarTelemetry> CarTelemetry { get; }
    DbSet<Standing> Standings { get; }
    DbSet<AnomalyFlag> AnomalyFlags { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
