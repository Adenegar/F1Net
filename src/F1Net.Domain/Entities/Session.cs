using F1Net.Domain.Common;
using F1Net.Domain.Enums;

namespace F1Net.Domain.Entities;

public class Session : BaseEntity, IAuditable
{
    public int RaceId { get; set; }
    public Race Race { get; set; } = null!;

    public required int OpenF1SessionKey { get; set; }
    public required SessionType Type { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset? StartUtc { get; set; }
    public DateTimeOffset? EndUtc { get; set; }

    public DateTimeOffset? LastIngestedUtc { get; set; }

    public ICollection<Lap> Laps { get; set; } = new List<Lap>();
    public ICollection<AnomalyFlag> AnomalyFlags { get; set; } = new List<AnomalyFlag>();

    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}
