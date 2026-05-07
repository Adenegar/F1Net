using F1Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace F1Net.Infrastructure.Persistence.Configurations;

public class CarTelemetryConfiguration : IEntityTypeConfiguration<CarTelemetry>
{
    public void Configure(EntityTypeBuilder<CarTelemetry> b)
    {
        b.ToTable("CarTelemetry");
        b.HasKey(x => x.Id);
        b.Property(x => x.Throttle).HasPrecision(5, 2);
        b.Property(x => x.Brake).HasPrecision(5, 2);

        b.HasIndex(x => new { x.SessionId, x.DriverId, x.SampleUtc });

        b.HasOne(x => x.Session)
            .WithMany()
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Driver)
            .WithMany()
            .HasForeignKey(x => x.DriverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
