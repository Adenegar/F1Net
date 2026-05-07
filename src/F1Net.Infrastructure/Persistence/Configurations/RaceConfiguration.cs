using F1Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace F1Net.Infrastructure.Persistence.Configurations;

public class RaceConfiguration : IEntityTypeConfiguration<Race>
{
    public void Configure(EntityTypeBuilder<Race> b)
    {
        b.ToTable("Races");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(128).IsRequired();
        b.Property(x => x.CircuitRef).HasMaxLength(64);
        b.Property(x => x.CircuitName).HasMaxLength(128);
        b.Property(x => x.Country).HasMaxLength(64);
        b.Property(x => x.Locality).HasMaxLength(128);

        b.HasIndex(x => new { x.SeasonId, x.Round }).IsUnique();

        b.HasOne(x => x.Season)
            .WithMany(s => s.Races)
            .HasForeignKey(x => x.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
