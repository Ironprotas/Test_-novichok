using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TestNewPeople.Services;

namespace TestNewPeople.Pages.Novice;

public class SurveyModel : PageModel
{
    private readonly NewcomerStore _store;

    public SurveyModel(NewcomerStore store)
    {
        _store = store;
    }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = "";

    [BindProperty]
    public int Clarity { get; set; } = 3;

    [BindProperty]
    public int Contact { get; set; } = 3;

    [BindProperty]
    public string? Blocker { get; set; }

    public Models.Newcomer? Person { get; private set; }

    public IActionResult OnGet()
    {
        if (!Load()) return NotFound();
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!Load()) return NotFound();
        _store.SaveSurvey(Id, Clarity, Contact, Blocker);
        return RedirectToPage("/Newcomer/Index", new { id = Id });
    }

    private bool Load()
    {
        Person = _store.Get(Id);
        if (Person is null) return false;

        if (Person.Survey?.IsCompleted == true)
        {
            Clarity = Person.Survey.ClarityScore ?? 3;
            Contact = Person.Survey.ContactScore ?? 3;
            Blocker = Person.Survey.BlockerText;
        }

        return true;
    }
}