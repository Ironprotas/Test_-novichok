using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TestNewPeople.Services;

namespace TestNewPeople.Pages.Mentor;

public class PersonModel : PageModel
{
    private readonly NewcomerStore _store;

    public PersonModel(NewcomerStore store)
    {
        _store = store;
    }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = "";

    [BindProperty]
    public string Label { get; set; } = MentorMarkCatalog.Presets[0].Label;

    [BindProperty]
    public string? Comment { get; set; }

    public Models.Newcomer? Person { get; private set; }
    public string? Error { get; private set; }

    public IActionResult OnGet()
    {
        if (!Load()) return NotFound();
        return Page();
    }

    public IActionResult OnPostAdd()
    {
        if (!Load()) return NotFound();

        if (string.IsNullOrWhiteSpace(Label))
        {
            Error = "Выберите метку.";
            return Page();
        }

        var preset = MentorMarkCatalog.Presets.FirstOrDefault(p => p.Label == Label);
        var needsAttention = preset == default ? true : preset.NeedsAttention;

        if (Label == "Другое" && string.IsNullOrWhiteSpace(Comment))
        {
            Error = "Для «Другое» напишите комментарий.";
            return Page();
        }

        _store.AddMentorMark(Id, Label, Comment, needsAttention);
        return RedirectToPage(new { id = Id });
    }

    public IActionResult OnPostClear(int markId)
    {
        if (string.IsNullOrWhiteSpace(Id))
            return RedirectToPage("/Mentor/Index");

        _store.DeactivateMentorMark(Id, markId);
        return RedirectToPage(new { id = Id });
    }

    private bool Load()
    {
        Person = _store.Get(Id);
        return Person is not null;
    }
}