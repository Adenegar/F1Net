using F1Net.Application.Abstractions;
using F1Net.Application.Anomalies.Models;
using Microsoft.Extensions.Logging;

namespace F1Net.Infrastructure.Ml;

public class RandomizedPcaLapDetector : ILapAnomalyDetector
{
    private readonly ILogger<RandomizedPcaLapDetector> _log;
    private const double ZScoreThreshold = 2.5;
    private const int MinLapsPerDriver = 5;

    public RandomizedPcaLapDetector(ILogger<RandomizedPcaLapDetector> log) => _log = log;

    public IReadOnlyList<LapAnomalyResult> Detect(IReadOnlyList<LapFeature> features)
    {
        if (features.Count == 0) return Array.Empty<LapAnomalyResult>();

        var results = new List<LapAnomalyResult>(features.Count);

        foreach (var driverGroup in features.GroupBy(f => f.DriverId))
        {
            var laps = driverGroup.ToList();
            if (laps.Count < MinLapsPerDriver)
            {
                foreach (var l in laps) results.Add(new LapAnomalyResult(l.LapId, 0, false));
                continue;
            }

            var times = laps.Select(l => l.LapTimeSeconds).ToArray();
            var mean = times.Average();
            var variance = times.Sum(t => (t - mean) * (t - mean)) / times.Length;
            var stdDev = Math.Sqrt(variance);

            if (stdDev < 1e-6)
            {
                foreach (var l in laps) results.Add(new LapAnomalyResult(l.LapId, 0, false));
                continue;
            }

            foreach (var l in laps)
            {
                var z = Math.Abs(l.LapTimeSeconds - mean) / stdDev;
                results.Add(new LapAnomalyResult(l.LapId, z, z > ZScoreThreshold));
            }
        }

        var flagged = results.Count(r => r.IsAnomaly);
        if (results.Count > 0)
        {
            var sorted = results.Select(r => r.Score).OrderBy(s => s).ToArray();
            _log.LogInformation(
                "Z-score over {N} laps: median={Med:F2} p90={P90:F2} max={Max:F2} flagged={Flagged}",
                sorted.Length, sorted[sorted.Length / 2], sorted[(int)(sorted.Length * 0.9)], sorted[^1], flagged);
        }

        return results;
    }
}
