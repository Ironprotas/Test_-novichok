using Microsoft.AspNetCore.Mvc.RazorPages;
using TestNewPeople.Services;

namespace TestNewPeople.Pages.Mentor;

public class IndexModel : PageModel
{
    private readonly NewcomerStore _store;

    public IndexModel(NewcomerStore store)
    {
        _store = store;
    }

    public List<Models.Newcomer> People { get; private set; } = [];

    public void OnGet()
    {
        People = _store.All()
            .OrderBy(p => p.Mentor.Name)
            .ThenBy(p => p.FullName)
            .ToList();
    }
}