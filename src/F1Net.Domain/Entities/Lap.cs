using F1Net.Domain.Common;
using F1Net.Domain.Enums;

namespace F1Net.Domain.Entities;

public class Lap : BaseEntity, IAuditable
{
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public int DriverId { get; set; }
    public Driver Driver { get; set; } = null!;

    public required int LapNumber { get; set; }
    public TimeSpan? LapTime { get; set; }

    public TyreCompound TyreCompound { get; set; } = TyreCompound.Unknown;
    public int? TyreAgeLaps { get; set; }
    public bool IsPitOutLap { get; set; }
    public bool IsPitInLap { get; set; }

    public ICollection<Sector> Sectors { get; set; } = new List<Sector>();
    public ICollection<AnomalyFlag> AnomalyFlags { get; set; } = new List<AnomalyFlag>();

    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}
