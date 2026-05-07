using F1Net.Infrastructure.BackgroundJobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace F1Net.Web.Controllers.Api;

[ApiController]
[Route("api/sync")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class SyncApiController : ControllerBase
{
    private readonly TelemetryIngestionService _ingestion;
    private readonly IOptionsMonitor<IngestionOptions> _options;

    public SyncApiController(IEnumerable<IHostedService> hosted, IOptionsMonitor<IngestionOptions> options)
    {
        _ingestion = hosted.OfType<TelemetryIngestionService>().Single();
        _options = options;
    }

    [HttpPost]
    public async Task<IActionResult> Run(CancellationToken ct)
    {
        await _ingestion.RunOnceAsync(_options.CurrentValue, ct);
        return Accepted(new { status = "ok", at = DateTimeOffset.UtcNow });
    }
}
