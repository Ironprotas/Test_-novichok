using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TestNewPeople.Models;
using TestNewPeople.Services;

namespace TestNewPeople.Pages.Hr;

public class CaseModel : PageModel
{
    private readonly NewcomerStore _store;

    public CaseModel(NewcomerStore store)
    {
        _store = store;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public CaseStatus Status { get; set; }

    [BindProperty]
    public string? NextAction { get; set; }

    [BindProperty]
    public DateOnly? CheckDate { get; set; }

    public HelpCase? Case { get; private set; }
    public Models.Newcomer? Person { get; private set; }

    public IActionResult OnGet()
    {
        if (!Load()) return NotFound();
        Status = Case!.Status;
        NextAction = Case.NextAction;
        CheckDate = Case.CheckDate;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!Load()) return NotFound();
        _store.UpdateCase(Id, Status, NextAction, CheckDate);
        return RedirectToPage("/Hr/Index");
    }

    private bool Load()
    {
        Case = _store.GetCase(Id);
        if (Case is null) return false;
        Person = _store.Get(Case.NewcomerId);
        return Person is not null;
    }
}