using F1Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace F1Net.Infrastructure.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> b)
    {
        b.ToTable("Teams");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(128).IsRequired();
        b.Property(x => x.ConstructorRef).HasMaxLength(64);
        b.Property(x => x.Nationality).HasMaxLength(64);
        b.Property(x => x.ColorHex).HasMaxLength(9);
        b.HasIndex(x => x.ConstructorRef).IsUnique().HasFilter("[ConstructorRef] IS NOT NULL");
    }
}
