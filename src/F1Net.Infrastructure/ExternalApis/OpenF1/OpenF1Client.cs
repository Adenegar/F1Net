using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using F1Net.Application.Abstractions;
using F1Net.Application.Ingestion.Models;

namespace F1Net.Infrastructure.ExternalApis.OpenF1;

public class OpenF1Client : IOpenF1Client
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public OpenF1Client(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<OpenF1Session>> GetSessionsAsync(int year, CancellationToken ct)
    {
        var raw = await _http.GetFromJsonAsync<List<SessionRow>>($"sessions?year={year}", Json, ct) ?? new();
        return raw.Select(r => new OpenF1Session(
            r.SessionKey, r.MeetingKey, r.SessionName ?? "", r.SessionType ?? "",
            r.DateStart, r.DateEnd, r.Year ?? year,
            r.CircuitShortName ?? "", r.CountryName ?? "", r.Location ?? "")).ToList();
    }

    public async Task<IReadOnlyList<OpenF1Driver>> GetDriversAsync(int sessionKey, CancellationToken ct)
    {
        var raw = await _http.GetFromJsonAsync<List<DriverRow>>($"drivers?session_key={sessionKey}", Json, ct) ?? new();
        return raw.Select(r => new OpenF1Driver(
            r.DriverNumber, r.FullName ?? "", r.NameAcronym ?? "",
            r.TeamName ?? "", r.TeamColour ?? "", r.CountryCode ?? "")).ToList();
    }

    public async Task<IReadOnlyList<OpenF1Lap>> GetLapsAsync(int sessionKey, CancellationToken ct)
    {
        var raw = await _http.GetFromJsonAsync<List<LapRow>>($"laps?session_key={sessionKey}", Json, ct) ?? new();
        return raw.Select(r => new OpenF1Lap(
            r.SessionKey, r.DriverNumber, r.LapNumber,
            r.LapDuration, r.DurationSector1, r.DurationSector2, r.DurationSector3,
            r.IsPitOutLap, r.Compound, r.TyreLifeLaps)).ToList();
    }

    private sealed record SessionRow(int SessionKey, int MeetingKey, string? SessionName, string? SessionType,
        DateTimeOffset? DateStart, DateTimeOffset? DateEnd, int? Year,
        string? CircuitShortName, string? CountryName, string? Location);

    private sealed record DriverRow(int DriverNumber, string? FullName, string? NameAcronym,
        string? TeamName, string? TeamColour, string? CountryCode);

    private sealed record LapRow(int SessionKey, int DriverNumber, int LapNumber,
        double? LapDuration, double? DurationSector1, double? DurationSector2, double? DurationSector3,
        bool? IsPitOutLap, string? Compound, int? TyreLifeLaps);
}
