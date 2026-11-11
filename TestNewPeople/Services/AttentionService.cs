using TestNewPeople.Models;

namespace TestNewPeople.Services;

public sealed class AttentionService
{
    public IReadOnlyList<AttentionItem> For(Newcomer person, DateOnly today)
    {
        var day = person.DayNumber(today);
        var items = new List<AttentionItem>();

        if (!string.Equals(person.Shift, person.Mentor.Shift, StringComparison.OrdinalIgnoreCase))
        {
            items.Add(new AttentionItem
            {
                Kind = AttentionKind.MentorOtherShift,
                Title = "Наставник в другой смене",
                Detail = $"{person.FullName}: смена «{person.Shift}», наставник {person.Mentor.Name} — «{person.Mentor.Shift}».",
                SuggestedAction = "Согласовать пересечение смен или временно закрепить доступного наставника на смене новичка."
            });
        }

        var overdue = person.Tasks
            .Where(t => t.Status == PlanTaskStatus.Todo && day > t.DayTo)
            .ToList();
        foreach (var task in overdue)
        {
            items.Add(new AttentionItem
            {
                Kind = AttentionKind.TaskOverdue,
                Title = "Задача просрочена",
                Detail = $"«{task.Title}» (окно дней {task.DayFrom}–{task.DayTo}).",
                SuggestedAction = "Помочь с задачей: пройти вместе или снять блокер."
            });
        }

        if (day >= 7 && (person.Survey is null || !person.Survey.IsCompleted))
        {
            items.Add(new AttentionItem
            {
                Kind = AttentionKind.SurveyMissing,
                Title = "Опрос ещё не заполнен",
                Detail = "Короткий опрос 7-го дня пока без ответа.",
                SuggestedAction = "Мягко напомнить новичку пройти опрос или уточнить причину задержки."
            });
        }

        var open = person.Cases
            .Where(c => c.Status is CaseStatus.NeedsHelp or CaseStatus.InProgress)
            .OrderByDescending(c => c.CreatedAtUtc)
            .FirstOrDefault();
        if (open is not null)
        {
            items.Add(new AttentionItem
            {
                Kind = AttentionKind.OpenCase,
                Title = open.Status == CaseStatus.NeedsHelp ? "Требует внимания" : "Обращение в работе",
                Detail = open.Reason,
                SuggestedAction = open.Status == CaseStatus.NeedsHelp
                    ? "Назначить следующее действие и дату проверки."
                    : "Проверить результат и закрыть обращение, если вопрос снят."
            });
        }

        foreach (var mark in person.MentorMarks.Where(m => m.IsActive && m.NeedsAttention))
        {
            items.Add(new AttentionItem
            {
                Kind = AttentionKind.MentorMark,
                Title = "Метка наставника",
                Detail = string.IsNullOrWhiteSpace(mark.Comment)
                    ? mark.Label
                    : $"{mark.Label}: {mark.Comment}",
                SuggestedAction = "Поговорить с наставником и новичком: уточнить, чем помочь."
            });
        }

        return items;
    }

    public static string CaseStatusLabel(CaseStatus status) => status switch
    {
        CaseStatus.NeedsHelp => "Нужна помощь",
        CaseStatus.InProgress => "В работе",
        CaseStatus.Resolved => "Решено",
        _ => status.ToString()
    };
}