using F1Net.Domain.Common;
using F1Net.Domain.Enums;

namespace F1Net.Domain.Entities;

public class AnomalyFlag : BaseEntity, IAuditable
{
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public int LapId { get; set; }
    public Lap Lap { get; set; } = null!;

    public required string DetectorName { get; set; }
    public required FlagSeverity Severity { get; set; }
    public required double Score { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset DetectedUtc { get; set; }

    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}
