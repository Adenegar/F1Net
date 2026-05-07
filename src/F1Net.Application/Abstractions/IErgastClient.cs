using F1Net.Application.Ingestion.Models;

namespace F1Net.Application.Abstractions;

public interface IErgastClient
{
    Task<ErgastSeason> GetSeasonAsync(int year, CancellationToken ct);
    Task<IReadOnlyList<ErgastStanding>> GetDriverStandingsAsync(int year, CancellationToken ct);
}
