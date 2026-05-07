using F1Net.Application.Anomalies.Models;

namespace F1Net.Application.Abstractions;

public interface ILapAnomalyDetector
{
    IReadOnlyList<LapAnomalyResult> Detect(IReadOnlyList<LapFeature> features);
}
