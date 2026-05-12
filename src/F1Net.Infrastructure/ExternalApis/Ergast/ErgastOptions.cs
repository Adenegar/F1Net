namespace F1Net.Infrastructure.ExternalApis.Ergast;

public class ErgastOptions
{
    public const string SectionName = "Ergast";
    public string BaseUrl { get; set; } = "https://api.jolpi.ca/ergast/f1/";
}
