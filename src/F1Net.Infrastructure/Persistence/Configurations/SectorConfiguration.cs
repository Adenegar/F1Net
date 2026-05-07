using F1Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace F1Net.Infrastructure.Persistence.Configurations;

public class SectorConfiguration : IEntityTypeConfiguration<Sector>
{
    public void Configure(EntityTypeBuilder<Sector> b)
    {
        b.ToTable("Sectors");
        b.HasKey(x => x.Id);
        b.Property(x => x.SpeedTrapKph).HasPrecision(6, 2);

        b.HasIndex(x => new { x.LapId, x.SectorNumber }).IsUnique();

        b.HasOne(x => x.Lap)
            .WithMany(l => l.Sectors)
            .HasForeignKey(x => x.LapId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
