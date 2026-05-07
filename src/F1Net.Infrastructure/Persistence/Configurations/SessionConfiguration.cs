using F1Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace F1Net.Infrastructure.Persistence.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> b)
    {
        b.ToTable("Sessions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(64).IsRequired();
        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);

        b.HasIndex(x => x.OpenF1SessionKey).IsUnique();
        b.HasIndex(x => new { x.RaceId, x.Type });

        b.HasOne(x => x.Race)
            .WithMany(r => r.Sessions)
            .HasForeignKey(x => x.RaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
