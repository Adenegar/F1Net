using F1Net.Domain.Common;

namespace F1Net.Domain.Entities;

public class Driver : BaseEntity, IAuditable
{
    public required string FullName { get; set; }
    public required string DriverRef { get; set; }
    public string? Code { get; set; }
    public int? PermanentNumber { get; set; }
    public string? Nationality { get; set; }
    public DateOnly? DateOfBirth { get; set; }

    public int? TeamId { get; set; }
    public Team? Team { get; set; }

    public ICollection<Lap> Laps { get; set; } = new List<Lap>();
    public ICollection<Standing> Standings { get; set; } = new List<Standing>();

    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}
