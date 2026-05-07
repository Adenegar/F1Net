using F1Net.Domain.Common;

namespace F1Net.Domain.Entities;

public class Sector : BaseEntity, IAuditable
{
    public int LapId { get; set; }
    public Lap Lap { get; set; } = null!;

    public required int SectorNumber { get; set; }
    public TimeSpan? Duration { get; set; }
    public decimal? SpeedTrapKph { get; set; }

    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}
