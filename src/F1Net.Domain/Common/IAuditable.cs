namespace F1Net.Domain.Common;

public interface IAuditable
{
    DateTimeOffset CreatedUtc { get; set; }
    DateTimeOffset UpdatedUtc { get; set; }
}
