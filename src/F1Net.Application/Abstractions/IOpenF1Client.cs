using F1Net.Application.Ingestion.Models;

namespace F1Net.Application.Abstractions;

public interface IOpenF1Client
{
    Task<IReadOnlyList<OpenF1Session>> GetSessionsAsync(int year, CancellationToken ct);
    Task<IReadOnlyList<OpenF1Driver>> GetDriversAsync(int sessionKey, CancellationToken ct);
    Task<IReadOnlyList<OpenF1Lap>> GetLapsAsync(int sessionKey, CancellationToken ct);
}
