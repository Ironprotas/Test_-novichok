using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TestNewPeople.Models;
using TestNewPeople.Services;

namespace TestNewPeople.Pages.Hr;

public class IndexModel : PageModel
{
    private readonly NewcomerStore _store;
    private readonly AttentionService _attention;

    public IndexModel(NewcomerStore store, AttentionService attention)
    {
        _store = store;
        _attention = attention;
    }

    [BindProperty(SupportsGet = true, Name = "only")]
    public string? OnlyAttention { get; set; }

    public DateOnly Today { get; private set; }
    public List<Row> Rows { get; private set; } = [];
    public List<ChangeLog> RecentChanges { get; private set; } = [];

    public sealed class Row
    {
        public required Models.Newcomer Person { get; init; }
        public required int Day { get; init; }
        public required IReadOnlyList<AttentionItem> Attention { get; init; }
        public required IReadOnlyList<MentorMark> MentorMarks { get; init; }
        public HelpCase? OpenCase { get; init; }
        public int ProgressPct { get; init; }
    }

    public void OnGet()
    {
        Today = _store.Today;
        var rows = _store.All().Select(p =>
        {
            var day = p.DayNumber(Today);
            var done = p.Tasks.Count(t => t.Status == PlanTaskStatus.Done);
            return new Row
            {
                Person = p,
                Day = day,
                Attention = _attention.For(p, Today),
                MentorMarks = p.MentorMarks
                    .Where(m => m.IsActive)
                    .OrderByDescending(m => m.CreatedAtUtc)
                    .ToList(),
                OpenCase = p.Cases
                    .Where(c => c.Status != CaseStatus.Resolved)
                    .OrderByDescending(c => c.UpdatedAtUtc ?? c.CreatedAtUtc)
                    .FirstOrDefault(),
                ProgressPct = (int)Math.Round(100.0 * done / Math.Max(1, p.Tasks.Count))
            };
        }).ToList();

        if (string.Equals(OnlyAttention, "1", StringComparison.Ordinal))
            rows = rows.Where(r => r.Attention.Count > 0).ToList();

        Rows = rows
            .OrderByDescending(r => r.Attention.Count)
            .ThenBy(r => r.Day)
            .ToList();

        RecentChanges = _store.RecentChanges().ToList();
    }
}