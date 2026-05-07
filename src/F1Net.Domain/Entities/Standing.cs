using F1Net.Domain.Common;

namespace F1Net.Domain.Entities;

public class Standing : BaseEntity, IAuditable
{
    public int SeasonId { get; set; }
    public Season Season { get; set; } = null!;

    public int DriverId { get; set; }
    public Driver Driver { get; set; } = null!;

    public required int Position { get; set; }
    public required decimal Points { get; set; }
    public int Wins { get; set; }
    public int? AfterRound { get; set; }

    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}
