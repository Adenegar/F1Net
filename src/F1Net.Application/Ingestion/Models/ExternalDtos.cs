namespace F1Net.Application.Ingestion.Models;

public record OpenF1Session(
    int SessionKey,
    int MeetingKey,
    string SessionName,
    string SessionType,
    DateTimeOffset? DateStart,
    DateTimeOffset? DateEnd,
    int Year,
    string CircuitShortName,
    string CountryName,
    string Location);

public record OpenF1Driver(
    int DriverNumber,
    string FullName,
    string NameAcronym,
    string TeamName,
    string TeamColour,
    string CountryCode);

public record OpenF1Lap(
    int SessionKey,
    int DriverNumber,
    int LapNumber,
    double? LapDuration,
    double? DurationSector1,
    double? DurationSector2,
    double? DurationSector3,
    bool? IsPitOutLap,
    string? Compound,
    int? TyreLifeLaps);

public record ErgastSeason(int Year, string? Url, IReadOnlyList<ErgastRace> Races);

public record ErgastRace(
    int Round,
    string RaceName,
    string CircuitId,
    string CircuitName,
    string Country,
    string Locality,
    DateTimeOffset? DateUtc);

public record ErgastStanding(
    int Position,
    decimal Points,
    int Wins,
    string DriverRef,
    string FullName,
    string DriverCode,
    int? PermanentNumber,
    string? Nationality,
    string ConstructorRef,
    string ConstructorName);
