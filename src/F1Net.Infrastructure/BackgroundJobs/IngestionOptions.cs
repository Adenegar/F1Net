namespace F1Net.Infrastructure.BackgroundJobs;

public class IngestionOptions
{
    public const string SectionName = "Ingestion";
    public bool Enabled { get; set; } = true;
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(15);
    public int CurrentYear { get; set; } = DateTime.UtcNow.Year;
    public bool DetectAnomaliesAfterIngest { get; set; } = true;
}
