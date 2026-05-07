using Microsoft.AspNetCore.Mvc.RazorPages;

namespace F1Net.Web.Pages;

public class IndexModel : PageModel
{
    public int Year { get; private set; }

    public void OnGet()
    {
        Year = DateTime.UtcNow.Year;
    }
}
