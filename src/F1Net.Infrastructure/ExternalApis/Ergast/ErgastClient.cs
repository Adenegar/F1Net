using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using F1Net.Application.Abstractions;
using F1Net.Application.Ingestion.Models;

namespace F1Net.Infrastructure.ExternalApis.Ergast;

public class ErgastClient : IErgastClient
{
    private readonly HttpClient _http;
    public ErgastClient(HttpClient http) => _http = http;

    public async Task<ErgastSeason> GetSeasonAsync(int year, CancellationToken ct)
    {
        var resp = await _http.GetFromJsonAsync<MRWrap<RaceTable>>($"{year}.json", ct);
        var rt = resp?.MRData?.RaceTable;
        var races = (rt?.Races ?? new()).Select(r => new ErgastRace(
            int.Parse(r.Round, CultureInfo.InvariantCulture),
            r.RaceName ?? "",
            r.Circuit?.CircuitId ?? "",
            r.Circuit?.CircuitName ?? "",
            r.Circuit?.Location?.Country ?? "",
            r.Circuit?.Location?.Locality ?? "",
            ParseDate(r.Date, r.Time))).ToList();
        return new ErgastSeason(year, new Uri(_http.BaseAddress!, $"{year}").ToString(), races);
    }

    public async Task<IReadOnlyList<ErgastStanding>> GetDriverStandingsAsync(int year, CancellationToken ct)
    {
        var resp = await _http.GetFromJsonAsync<MRWrap<StandingsTable>>($"{year}/driverStandings.json", ct);
        var lists = resp?.MRData?.StandingsTable?.StandingsLists;
        var first = lists?.FirstOrDefault();
        var rows = first?.DriverStandings ?? new();
        return rows.Select(s => new ErgastStanding(
            int.Parse(s.Position ?? "0", CultureInfo.InvariantCulture),
            decimal.Parse(s.Points ?? "0", CultureInfo.InvariantCulture),
            int.Parse(s.Wins ?? "0", CultureInfo.InvariantCulture),
            s.Driver?.DriverId ?? "",
            $"{s.Driver?.GivenName} {s.Driver?.FamilyName}".Trim(),
            s.Driver?.Code ?? "",
            int.TryParse(s.Driver?.PermanentNumber, out var n) ? n : null,
            s.Driver?.Nationality,
            s.Constructors?.FirstOrDefault()?.ConstructorId ?? "",
            s.Constructors?.FirstOrDefault()?.Name ?? "")).ToList();
    }

    private static DateTimeOffset? ParseDate(string? date, string? time)
    {
        if (string.IsNullOrWhiteSpace(date)) return null;
        var combined = string.IsNullOrWhiteSpace(time) ? date : $"{date}T{time}";
        return DateTimeOffset.TryParse(combined, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dto) ? dto : null;
    }

    private sealed class MRWrap<T> { [JsonPropertyName("MRData")] public MRDataT<T>? MRData { get; set; } }
    private sealed class MRDataT<T>
    {
        [JsonPropertyName("RaceTable")] public T? RaceTable { get; set; }
        [JsonPropertyName("StandingsTable")] public T? StandingsTable { get; set; }
    }
    private sealed class RaceTable { [JsonPropertyName("Races")] public List<RaceRow>? Races { get; set; } }
    private sealed class RaceRow
    {
        public string Round { get; set; } = "";
        public string? RaceName { get; set; }
        public CircuitRow? Circuit { get; set; }
        public string? Date { get; set; }
        public string? Time { get; set; }
    }
    private sealed class CircuitRow { public string? CircuitId { get; set; } public string? CircuitName { get; set; } public LocationRow? Location { get; set; } }
    private sealed class LocationRow { public string? Country { get; set; } public string? Locality { get; set; } }
    private sealed class StandingsTable { public List<StandingsList>? StandingsLists { get; set; } }
    private sealed class StandingsList { public List<DriverStanding>? DriverStandings { get; set; } }
    private sealed class DriverStanding
    {
        public string? Position { get; set; }
        public string? Points { get; set; }
        public string? Wins { get; set; }
        public DriverRow? Driver { get; set; }
        public List<ConstructorRow>? Constructors { get; set; }
    }
    private sealed class DriverRow
    {
        public string? DriverId { get; set; }
        public string? GivenName { get; set; }
        public string? FamilyName { get; set; }
        public string? Code { get; set; }
        public string? PermanentNumber { get; set; }
        public string? Nationality { get; set; }
    }
    private sealed class ConstructorRow { public string? ConstructorId { get; set; } public string? Name { get; set; } }
}
