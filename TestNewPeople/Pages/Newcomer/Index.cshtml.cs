using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TestNewPeople.Models;
using TestNewPeople.Services;

namespace TestNewPeople.Pages.Novice;

public class IndexModel : PageModel
{
    private readonly NewcomerStore _store;
    private readonly AttentionService _attention;

    public IndexModel(NewcomerStore store, AttentionService attention)
    {
        _store = store;
        _attention = attention;
    }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = "";

    public Models.Newcomer? Person { get; private set; }
    public int Day { get; private set; }
    public int ProgressPct { get; private set; }
    public bool SurveyDue { get; private set; }
    public bool SurveyDone { get; private set; }
    public HelpCase? OpenCase { get; private set; }

    public IActionResult OnGet()
    {
        if (!Load()) return NotFound();
        return Page();
    }

    public IActionResult OnPostToggle(string id, string taskId)
    {
        var personId = string.IsNullOrWhiteSpace(id) ? Id : id;
        if (string.IsNullOrWhiteSpace(personId) || string.IsNullOrWhiteSpace(taskId))
            return RedirectToPage("/Demo");

        _store.ToggleTask(personId, taskId);
        return RedirectToPage(new { id = personId });
    }

    private bool Load()
    {
        Person = _store.Get(Id);
        if (Person is null) return false;

        Day = Person.DayNumber(_store.Today);
        var done = Person.Tasks.Count(t => t.Status == PlanTaskStatus.Done);
        ProgressPct = (int)Math.Round(100.0 * done / Math.Max(1, Person.Tasks.Count));
        SurveyDone = Person.Survey?.IsCompleted == true;
        SurveyDue = Day >= 7 && !SurveyDone;
        OpenCase = Person.Cases
            .Where(c => c.Status != CaseStatus.Resolved)
            .OrderByDescending(c => c.UpdatedAtUtc ?? c.CreatedAtUtc)
            .FirstOrDefault();
        return true;
    }
}