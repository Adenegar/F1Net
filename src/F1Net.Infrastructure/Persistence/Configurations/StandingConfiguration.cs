using F1Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace F1Net.Infrastructure.Persistence.Configurations;

public class StandingConfiguration : IEntityTypeConfiguration<Standing>
{
    public void Configure(EntityTypeBuilder<Standing> b)
    {
        b.ToTable("Standings");
        b.HasKey(x => x.Id);
        b.Property(x => x.Points).HasPrecision(8, 2);

        b.HasIndex(x => new { x.SeasonId, x.DriverId, x.AfterRound }).IsUnique();

        b.HasOne(x => x.Season)
            .WithMany(s => s.Standings)
            .HasForeignKey(x => x.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Driver)
            .WithMany(d => d.Standings)
            .HasForeignKey(x => x.DriverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
