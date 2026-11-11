using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TestNewPeople.Models;
using TestNewPeople.Services;

namespace TestNewPeople.Pages.Hr;

public class PersonModel : PageModel
{
    private readonly NewcomerStore _store;
    private readonly AttentionService _attention;

    public PersonModel(NewcomerStore store, AttentionService attention)
    {
        _store = store;
        _attention = attention;
    }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = "";

    [BindProperty]
    public string? NewTitle { get; set; }

    [BindProperty]
    public string? NewDescription { get; set; }

    [BindProperty]
    public int NewDayFrom { get; set; } = 1;

    [BindProperty]
    public int NewDayTo { get; set; } = 7;

    public Models.Newcomer? Person { get; private set; }
    public int Day { get; private set; }
    public IReadOnlyList<AttentionItem> Attention { get; private set; } = [];
    public string? TaskError { get; private set; }

    public IActionResult OnGet()
    {
        if (!Load()) return NotFound();
        return Page();
    }

    public IActionResult OnPostAdd()
    {
        if (string.IsNullOrWhiteSpace(Id))
            return RedirectToPage("/Hr/Index");

        if (string.IsNullOrWhiteSpace(NewTitle))
        {
            Load();
            TaskError = "Укажите название задачи.";
            return Page();
        }

        _store.AddTask(Id, NewTitle, NewDescription, NewDayFrom, NewDayTo);
        return RedirectToPage(new { id = Id });
    }

    public IActionResult OnPostRemove(string id, string taskId)
    {
        var personId = string.IsNullOrWhiteSpace(id) ? Id : id;
        if (!string.IsNullOrWhiteSpace(personId) && !string.IsNullOrWhiteSpace(taskId))
            _store.RemoveTask(personId, taskId);

        return RedirectToPage(new { id = personId });
    }

    private bool Load()
    {
        Person = _store.Get(Id);
        if (Person is null) return false;
        Day = Person.DayNumber(_store.Today);
        Attention = _attention.For(Person, _store.Today);
        return true;
    }
}