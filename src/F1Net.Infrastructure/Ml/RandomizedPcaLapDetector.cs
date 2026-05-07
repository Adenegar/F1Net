using F1Net.Application.Abstractions;
using F1Net.Application.Anomalies.Models;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace F1Net.Infrastructure.Ml;

public class RandomizedPcaLapDetector : ILapAnomalyDetector
{
    private readonly ILogger<RandomizedPcaLapDetector> _log;
    private readonly MLContext _ml = new(seed: 42);
    private const double AnomalyThreshold = 0.65;

    public RandomizedPcaLapDetector(ILogger<RandomizedPcaLapDetector> log) => _log = log;

    public IReadOnlyList<LapAnomalyResult> Detect(IReadOnlyList<LapFeature> features)
    {
        if (features.Count < 10) return Array.Empty<LapAnomalyResult>();

        var rows = features.Select(f => new LapInput
        {
            LapId = f.LapId,
            Features = new[]
            {
                (float)f.LapTimeSeconds,
                (float)f.Sector1Seconds,
                (float)f.Sector2Seconds,
                (float)f.Sector3Seconds,
                (float)f.TyreAgeLaps,
            }
        }).ToList();

        var data = _ml.Data.LoadFromEnumerable(rows);
        var rank = Math.Min(2, rows[0].Features.Length - 1);
        var pipeline = _ml.Transforms.NormalizeMeanVariance(nameof(LapInput.Features))
            .Append(_ml.AnomalyDetection.Trainers.RandomizedPca(
                featureColumnName: nameof(LapInput.Features),
                rank: rank,
                ensureZeroMean: true));

        ITransformer model;
        try { model = pipeline.Fit(data); }
        catch (Exception ex) { _log.LogWarning(ex, "PCA training failed"); return Array.Empty<LapAnomalyResult>(); }

        var transformed = model.Transform(data);
        var preds = _ml.Data.CreateEnumerable<LapPrediction>(transformed, reuseRowObject: false).ToList();

        var results = new List<LapAnomalyResult>(preds.Count);
        for (var i = 0; i < preds.Count; i++)
        {
            var p = preds[i];
            var score = double.IsFinite(p.Score) ? p.Score : 0;
            var isAnomaly = p.PredictedLabel || score > AnomalyThreshold;
            results.Add(new LapAnomalyResult(rows[i].LapId, score, isAnomaly));
        }
        return results;
    }

    private sealed class LapInput
    {
        public int LapId { get; set; }
        [VectorType(5)]
        public float[] Features { get; set; } = Array.Empty<float>();
    }

    private sealed class LapPrediction
    {
        [ColumnName("PredictedLabel")] public bool PredictedLabel { get; set; }
        public float Score { get; set; }
    }
}
