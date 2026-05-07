using F1Net.Domain.Common;

namespace F1Net.Domain.Entities;

public class CarTelemetry : BaseEntity, IAuditable
{
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public int DriverId { get; set; }
    public Driver Driver { get; set; } = null!;

    public required DateTimeOffset SampleUtc { get; set; }
    public int? SpeedKph { get; set; }
    public int? Rpm { get; set; }
    public int? Gear { get; set; }
    public decimal? Throttle { get; set; }
    public decimal? Brake { get; set; }
    public bool? Drs { get; set; }

    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}
