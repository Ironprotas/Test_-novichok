using Microsoft.AspNetCore.Mvc.RazorPages;
using TestNewPeople.Models;
using TestNewPeople.Services;

namespace TestNewPeople.Pages;

public class IndexModel : PageModel
{
    private readonly NewcomerStore _store;
    private readonly AttentionService _attention;

    public IndexModel(NewcomerStore store, AttentionService attention)
    {
        _store = store;
        _attention = attention;
    }

    public int Total { get; private set; }
    public int NeedsAttention { get; private set; }
    public int OpenCases { get; private set; }

    public void OnGet()
    {
        var people = _store.All();
        Total = people.Count;
        NeedsAttention = people.Count(p => _attention.For(p, _store.Today).Count > 0);
        OpenCases = people.SelectMany(p => p.Cases)
            .Count(c => c.Status is CaseStatus.NeedsHelp or CaseStatus.InProgress);
    }
}