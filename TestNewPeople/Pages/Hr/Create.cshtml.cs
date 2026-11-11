using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TestNewPeople.Models;
using TestNewPeople.Services;

namespace TestNewPeople.Pages.Hr;

public class CreateModel : PageModel
{
    private readonly NewcomerStore _store;

    public CreateModel(NewcomerStore store)
    {
        _store = store;
    }

    [BindProperty]
    public string FullName { get; set; } = "";

    [BindProperty]
    public string Location { get; set; } = "Склад Север";

    [BindProperty]
    public string ManagerName { get; set; } = "";

    [BindProperty]
    public string Shift { get; set; } = "День";

    [BindProperty]
    public DateOnly StartDate { get; set; }

    [BindProperty]
    public string MentorName { get; set; } = "";

    [BindProperty]
    public string MentorRole { get; set; } = "Бригадир склада";

    [BindProperty]
    public string MentorPhone { get; set; } = "";

    [BindProperty]
    public string MentorMessenger { get; set; } = "";

    [BindProperty]
    public string MentorShift { get; set; } = "День";

    public string? Error { get; private set; }

    public void OnGet()
    {
        StartDate = _store.Today;
    }

    public IActionResult OnPost()
    {
        if (string.IsNullOrWhiteSpace(FullName) ||
            string.IsNullOrWhiteSpace(ManagerName) ||
            string.IsNullOrWhiteSpace(MentorName))
        {
            Error = "Заполните ФИО новичка, руководителя и наставника.";
            return Page();
        }

        if (StartDate == default)
            StartDate = _store.Today;

        var person = _store.Create(
            FullName,
            Location,
            ManagerName,
            string.IsNullOrWhiteSpace(Shift) ? "День" : Shift,
            StartDate,
            new Models.Mentor
            {
                Name = MentorName,
                RoleTitle = MentorRole,
                Phone = MentorPhone,
                Messenger = MentorMessenger,
                Shift = string.IsNullOrWhiteSpace(MentorShift) ? Shift : MentorShift
            });

        return RedirectToPage("/Newcomer/Index", new { id = person.Id });
    }
}