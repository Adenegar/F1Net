using F1Net.Application.Standings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace F1Net.Web.Controllers.Api;

[ApiController]
[Route("api/standings")]
[Authorize]
public class StandingsApiController : ControllerBase
{
    private readonly IMediator _mediator;
    public StandingsApiController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{year:int}")]
    public async Task<IReadOnlyList<StandingDto>> Get(int year, CancellationToken ct) =>
        await _mediator.Send(new GetSeasonStandingsQuery(year), ct);
}
