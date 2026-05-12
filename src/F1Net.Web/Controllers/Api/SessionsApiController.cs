using F1Net.Application.Anomalies.Queries;
using F1Net.Application.Drivers.Queries;
using F1Net.Application.Sessions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace F1Net.Web.Controllers.Api;

[ApiController]
[Route("api/sessions")]
public class SessionsApiController : ControllerBase
{
    private readonly IMediator _mediator;
    public SessionsApiController(IMediator mediator) => _mediator = mediator;

    [HttpGet("recent")]
    public async Task<IReadOnlyList<SessionListItem>> Recent([FromQuery] int take = 10, CancellationToken ct = default) =>
        await _mediator.Send(new GetRecentSessionsQuery(take), ct);

    [HttpGet("{sessionId:int}/anomalies")]
    public async Task<IReadOnlyList<AnomalyDto>> Anomalies(int sessionId, CancellationToken ct) =>
        await _mediator.Send(new GetSessionAnomaliesQuery(sessionId), ct);

    [HttpGet("{sessionId:int}/drivers/{driverId:int}/pace")]
    public async Task<ActionResult<DriverPaceDto>> Pace(int sessionId, int driverId, CancellationToken ct)
    {
        var dto = await _mediator.Send(new GetDriverPaceQuery(sessionId, driverId), ct);
        return dto is null ? NotFound() : Ok(dto);
    }
}
