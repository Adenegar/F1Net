using F1Net.Domain.Common;

namespace F1Net.Domain.Entities;

public class Team : BaseEntity, IAuditable
{
    public required string Name { get; set; }
    public string? ConstructorRef { get; set; }
    public string? Nationality { get; set; }
    public string? ColorHex { get; set; }

    public ICollection<Driver> Drivers { get; set; } = new List<Driver>();

    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}
