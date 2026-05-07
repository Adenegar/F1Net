using F1Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace F1Net.Infrastructure.Persistence.Configurations;

public class AnomalyFlagConfiguration : IEntityTypeConfiguration<AnomalyFlag>
{
    public void Configure(EntityTypeBuilder<AnomalyFlag> b)
    {
        b.ToTable("AnomalyFlags");
        b.HasKey(x => x.Id);
        b.Property(x => x.DetectorName).HasMaxLength(64).IsRequired();
        b.Property(x => x.Severity).HasConversion<string>().HasMaxLength(16);
        b.Property(x => x.Reason).HasMaxLength(512);

        b.HasIndex(x => new { x.SessionId, x.LapId });
        b.HasIndex(x => new { x.LapId, x.DetectorName }).IsUnique();

        b.HasOne(x => x.Session)
            .WithMany(s => s.AnomalyFlags)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Lap)
            .WithMany(l => l.AnomalyFlags)
            .HasForeignKey(x => x.LapId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
