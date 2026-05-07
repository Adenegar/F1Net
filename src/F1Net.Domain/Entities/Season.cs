using F1Net.Domain.Common;

namespace F1Net.Domain.Entities;

public class Season : BaseEntity, IAuditable
{
    public required int Year { get; set; }
    public string? Url { get; set; }

    public ICollection<Race> Races { get; set; } = new List<Race>();
    public ICollection<Standing> Standings { get; set; } = new List<Standing>();

    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}
