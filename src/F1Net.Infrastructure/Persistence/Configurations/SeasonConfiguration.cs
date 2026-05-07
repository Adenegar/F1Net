using F1Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace F1Net.Infrastructure.Persistence.Configurations;

public class SeasonConfiguration : IEntityTypeConfiguration<Season>
{
    public void Configure(EntityTypeBuilder<Season> b)
    {
        b.ToTable("Seasons");
        b.HasKey(x => x.Id);
        b.Property(x => x.Url).HasMaxLength(512);
        b.HasIndex(x => x.Year).IsUnique();
    }
}
