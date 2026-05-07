using F1Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace F1Net.Infrastructure.Persistence.Configurations;

public class LapConfiguration : IEntityTypeConfiguration<Lap>
{
    public void Configure(EntityTypeBuilder<Lap> b)
    {
        b.ToTable("Laps");
        b.HasKey(x => x.Id);
        b.Property(x => x.TyreCompound).HasConversion<string>().HasMaxLength(16);

        b.HasIndex(x => new { x.SessionId, x.DriverId, x.LapNumber }).IsUnique();
        b.HasIndex(x => new { x.SessionId, x.DriverId });

        b.HasOne(x => x.Session)
            .WithMany(s => s.Laps)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Driver)
            .WithMany(d => d.Laps)
            .HasForeignKey(x => x.DriverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
