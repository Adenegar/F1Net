using F1Net.Application.Anomalies.Queries;
using F1Net.Application.Sessions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace F1Net.Web.Pages.Sessions;

public class DetailsModel : PageModel
{
    private readonly IMediator _mediator;
    public DetailsModel(IMediator mediator) => _mediator = mediator;

    public int SessionId { get; private set; }
    public string Header { get; private set; } = "";
    public IReadOnlyList<AnomalyDto> Anomalies { get; private set; } = Array.Empty<AnomalyDto>();

    public async Task OnGetAsync(int id, CancellationToken ct)
    {
        SessionId = id;
        var sessions = await _mediator.Send(new GetRecentSessionsQuery(200), ct);
        var s = sessions.FirstOrDefault(x => x.Id == id);
        Header = s is null ? $"Session {id}" : $"{s.RaceName} — {s.Name}";
        Anomalies = await _mediator.Send(new GetSessionAnomaliesQuery(id), ct);
    }
}
