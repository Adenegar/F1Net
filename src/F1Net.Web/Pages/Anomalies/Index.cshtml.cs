using F1Net.Application.Sessions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace F1Net.Web.Pages.Anomalies;

public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    public IndexModel(IMediator mediator) => _mediator = mediator;

    public IReadOnlyList<SessionListItem> Sessions { get; private set; } = Array.Empty<SessionListItem>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Sessions = await _mediator.Send(new GetRecentSessionsQuery(100), ct);
    }
}
