using F1Net.Application.Abstractions;
using F1Net.Domain.Common;
using F1Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace F1Net.Infrastructure.Persistence;

public class F1NetDbContext : DbContext, IF1NetDbContext
{
    public F1NetDbContext(DbContextOptions<F1NetDbContext> options) : base(options) { }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Race> Races => Set<Race>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Lap> Laps => Set<Lap>();
    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<CarTelemetry> CarTelemetry => Set<CarTelemetry>();
    public DbSet<Standing> Standings => Set<Standing>();
    public DbSet<AnomalyFlag> AnomalyFlags => Set<AnomalyFlag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(F1NetDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedUtc = now;
                entry.Entity.UpdatedUtc = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedUtc = now;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
