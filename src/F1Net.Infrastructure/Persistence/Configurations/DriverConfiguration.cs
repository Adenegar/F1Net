using F1Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace F1Net.Infrastructure.Persistence.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> b)
    {
        b.ToTable("Drivers");
        b.HasKey(x => x.Id);
        b.Property(x => x.FullName).HasMaxLength(128).IsRequired();
        b.Property(x => x.DriverRef).HasMaxLength(64).IsRequired();
        b.Property(x => x.Code).HasMaxLength(8);
        b.Property(x => x.Nationality).HasMaxLength(64);

        b.HasIndex(x => x.DriverRef).IsUnique();
        b.HasIndex(x => x.PermanentNumber);

        b.HasOne(x => x.Team)
            .WithMany(t => t.Drivers)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
