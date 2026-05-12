using F1Net.Infrastructure.BackgroundJobs;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace F1Net.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IOptionsMonitor<IngestionOptions> _options;

    public IndexModel(IOptionsMonitor<IngestionOptions> options) => _options = options;

    public int Year { get; private set; }

    public void OnGet()
    {
        Year = _options.CurrentValue.CurrentYear;
    }
}
