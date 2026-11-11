using Microsoft.AspNetCore.Mvc.RazorPages;
using TestNewPeople.Models;
using TestNewPeople.Services;

namespace TestNewPeople.Pages;

public class DemoModel : PageModel
{
    private readonly NewcomerStore _store;
    private readonly AttentionService _attention;

    public DemoModel(NewcomerStore store, AttentionService attention)
    {
        _store = store;
        _attention = attention;
    }

    public List<Pick> Picks { get; private set; } = [];

    public sealed class Pick
    {
        public required Models.Newcomer Person { get; init; }
        public required string Why { get; init; }
        public required string Badge { get; init; }
    }

    public void OnGet()
    {
        var today = _store.Today;
        var all = _store.All();

        Picks = all.Select(p =>
        {
            var open = p.Cases.FirstOrDefault(c => c.Status != CaseStatus.Resolved);
            var attention = _attention.For(p, today);
            var why = p.Id switch
            {
                "n01" => "Есть проблема: терминал → совместная приёмка → можно закрыть после проверки",
                "n02" => "Всё в порядке: задачи закрыты, опрос спокойный",
                "n03" => "Опрос не заполнен; наставник отметил: не обращается",
                _ => attention.FirstOrDefault()?.Title ?? "Демо-профиль"
            };
            var badge = open?.Status switch
            {
                CaseStatus.InProgress => "В работе",
                CaseStatus.NeedsHelp => "Нужна помощь",
                _ when attention.Count > 0 => "Требует внимания",
                _ => "В порядке"
            };
            return new Pick { Person = p, Why = why, Badge = badge };
        }).ToList();
    }
}