namespace F1Net.Infrastructure.ExternalApis.OpenF1;

public class OpenF1Options
{
    public const string SectionName = "OpenF1";
    public string BaseUrl { get; set; } = "https://api.openf1.org/v1/";
    public string? ApiKey { get; set; }
}
