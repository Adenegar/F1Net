namespace F1Net.Application.Anomalies.Models;

public record LapFeature(
    int LapId,
    int DriverId,
    int LapNumber,
    double LapTimeSeconds,
    double Sector1Seconds,
    double Sector2Seconds,
    double Sector3Seconds,
    double TyreAgeLaps);

public record LapAnomalyResult(
    int LapId,
    double Score,
    bool IsAnomaly);
