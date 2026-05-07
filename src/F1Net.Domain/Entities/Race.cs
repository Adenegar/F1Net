using F1Net.Domain.Common;

namespace F1Net.Domain.Entities;

public class Race : BaseEntity, IAuditable
{
    public int SeasonId { get; set; }
    public Season Season { get; set; } = null!;

    public required int Round { get; set; }
    public required string Name { get; set; }
    public string? CircuitRef { get; set; }
    public string? CircuitName { get; set; }
    public string? Country { get; set; }
    public string? Locality { get; set; }
    public DateTimeOffset? StartUtc { get; set; }

    public ICollection<Session> Sessions { get; set; } = new List<Session>();

    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}
