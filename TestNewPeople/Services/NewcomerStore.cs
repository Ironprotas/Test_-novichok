using Microsoft.EntityFrameworkCore;
using TestNewPeople.Data;
using TestNewPeople.Models;

namespace TestNewPeople.Services;

public sealed class NewcomerStore(AppDbContext db)
{
    public DateOnly Today { get; } = DateOnly.FromDateTime(DateTime.Today);

    public IReadOnlyList<Newcomer> All() =>
        db.Newcomers
            .Include(p => p.Tasks)
            .Include(p => p.Survey)
            .Include(p => p.Cases)
            .Include(p => p.MentorMarks)
            .AsSplitQuery()
            .OrderBy(p => p.Id)
            .ToList();

    public Newcomer? Get(string id) =>
        db.Newcomers
            .Include(p => p.Tasks)
            .Include(p => p.Survey)
            .Include(p => p.Cases)
            .Include(p => p.MentorMarks)
            .AsSplitQuery()
            .FirstOrDefault(p => p.Id == id);

    public IReadOnlyList<ChangeLog> RecentChanges(int take = 12) =>
        db.ChangeLogs.OrderByDescending(x => x.AtUtc).Take(take).ToList();

    public bool ToggleTask(string personId, string taskId)
    {
        var person = Get(personId);
        var task = person?.Tasks.FirstOrDefault(t => t.Id == taskId);
        if (person is null || task is null) return false;

        var wasDone = task.Status == PlanTaskStatus.Done;
        task.Status = wasDone ? PlanTaskStatus.Todo : PlanTaskStatus.Done;

        Log(person, "task", wasDone
            ? $"Вернул в работу: «{task.Title}»"
            : $"Отметил выполненным: «{task.Title}»");

        db.SaveChanges();
        return true;
    }

    public bool AddTask(string personId, string title, string? description, int dayFrom, int dayTo)
    {
        var person = Get(personId);
        if (person is null || string.IsNullOrWhiteSpace(title)) return false;

        if (dayFrom < 1) dayFrom = 1;
        if (dayTo < dayFrom) dayTo = dayFrom;

        var next = person.Tasks
            .Select(t => int.TryParse(t.Id.TrimStart('t'), out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        var task = new OnboardingTask
        {
            NewcomerId = personId,
            Id = $"t{next}",
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description)
                ? "Задача от руководителя"
                : description.Trim(),
            DayFrom = dayFrom,
            DayTo = dayTo,
            Status = PlanTaskStatus.Todo
        };

        person.Tasks.Add(task);
        Log(person, "task", $"Поставлена задача: «{task.Title}» (дни {dayFrom}–{dayTo})");
        db.SaveChanges();
        return true;
    }

    public bool RemoveTask(string personId, string taskId)
    {
        var person = Get(personId);
        var task = person?.Tasks.FirstOrDefault(t => t.Id == taskId);
        if (person is null || task is null) return false;

        // Базовые три задачи недели оставляем — снимать можно дополнительные
        if (taskId is "t1" or "t2" or "t3")
            return false;

        person.Tasks.Remove(task);
        db.Tasks.Remove(task);
        Log(person, "task", $"Снята задача: «{task.Title}»");
        db.SaveChanges();
        return true;
    }

    public bool SaveSurvey(string personId, int clarity, int contact, string? blocker)
    {
        var person = Get(personId);
        if (person is null) return false;

        person.Survey ??= new WeekSurvey { NewcomerId = personId, Day = 7 };
        person.Survey.CompletedOn = Today;
        person.Survey.ClarityScore = Math.Clamp(clarity, 1, 5);
        person.Survey.ContactScore = Math.Clamp(contact, 1, 5);
        person.Survey.BlockerText = string.IsNullOrWhiteSpace(blocker) ? null : blocker.Trim();

        Log(person, "survey", "Прошёл опрос 7-го дня");

        var hasBlocker = !string.IsNullOrWhiteSpace(person.Survey.BlockerText)
                         && !person.Survey.BlockerText.Contains("не мешает", StringComparison.OrdinalIgnoreCase)
                         && (clarity <= 3 || person.Survey.BlockerText.Length > 20);

        if (hasBlocker)
        {
            var open = person.Cases.FirstOrDefault(c => c.Status != CaseStatus.Resolved);
            if (open is null)
            {
                open = new HelpCase
                {
                    NewcomerId = personId,
                    CreatedAtUtc = DateTime.UtcNow
                };
                person.Cases.Add(open);
            }

            open.Reason = "По результатам опроса нужна поддержка";
            open.NewcomerReply = person.Survey.BlockerText ?? "";
            open.Status = CaseStatus.NeedsHelp;
            open.UpdatedAtUtc = DateTime.UtcNow;
            Log(person, "case", "Сообщил о сложности в опросе 7-го дня");
        }

        db.SaveChanges();
        return true;
    }

    public Newcomer Create(
        string fullName,
        string location,
        string managerName,
        string shift,
        DateOnly startDate,
        Mentor mentor)
    {
        var id = NextId();
        var person = new Newcomer
        {
            Id = id,
            FullName = fullName.Trim(),
            Location = string.IsNullOrWhiteSpace(location) ? "Склад Север" : location.Trim(),
            ManagerName = managerName.Trim(),
            Shift = shift.Trim(),
            StartDate = startDate,
            Mentor = new Mentor
            {
                Name = mentor.Name.Trim(),
                RoleTitle = string.IsNullOrWhiteSpace(mentor.RoleTitle) ? "Наставник" : mentor.RoleTitle.Trim(),
                Phone = mentor.Phone.Trim(),
                Messenger = mentor.Messenger.Trim(),
                Shift = string.IsNullOrWhiteSpace(mentor.Shift) ? shift.Trim() : mentor.Shift.Trim()
            },
            Tasks = WarehouseWeekPlan.CreateTasks(id),
            Survey = new WeekSurvey { NewcomerId = id, Day = 7 }
        };

        db.Newcomers.Add(person);
        Log(person, "hire", "Добавлен новый новичок склада");
        db.SaveChanges();
        return person;
    }

    private string NextId()
    {
        var max = db.Newcomers
            .AsEnumerable()
            .Select(p => int.TryParse(p.Id.TrimStart('n'), out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"n{max + 1:00}";
    }

    public bool AddMentorMark(string personId, string label, string? comment, bool needsAttention)
    {
        var person = Get(personId);
        if (person is null || string.IsNullOrWhiteSpace(label)) return false;

        person.MentorMarks.Add(new MentorMark
        {
            NewcomerId = personId,
            Label = label.Trim(),
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            NeedsAttention = needsAttention,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        Log(person, "mentor", needsAttention
            ? $"Наставник отметил: «{label.Trim()}»"
            : $"Наставник отметил позитив: «{label.Trim()}»");
        db.SaveChanges();
        return true;
    }

    public bool DeactivateMentorMark(string personId, int markId)
    {
        var person = Get(personId);
        var mark = person?.MentorMarks.FirstOrDefault(m => m.Id == markId);
        if (person is null || mark is null) return false;

        mark.IsActive = false;
        Log(person, "mentor", $"Метка наставника снята: «{mark.Label}»");
        db.SaveChanges();
        return true;
    }

    public HelpCase? GetCase(int caseId) =>
        db.Cases.Include(c => c.Newcomer).FirstOrDefault(c => c.Id == caseId);

    public bool UpdateCase(int caseId, CaseStatus status, string? nextAction, DateOnly? checkDate)
    {
        var helpCase = db.Cases.Include(c => c.Newcomer).FirstOrDefault(c => c.Id == caseId);
        if (helpCase?.Newcomer is null) return false;

        helpCase.Status = status;
        helpCase.NextAction = string.IsNullOrWhiteSpace(nextAction) ? null : nextAction.Trim();
        helpCase.CheckDate = checkDate;
        helpCase.UpdatedAtUtc = DateTime.UtcNow;
        if (status == CaseStatus.Resolved)
            helpCase.ResolvedAtUtc = DateTime.UtcNow;

        var person = helpCase.Newcomer;
        Log(person, "case", status switch
        {
            CaseStatus.NeedsHelp => "Обращение: нужна помощь",
            CaseStatus.InProgress => $"В работе: {helpCase.NextAction ?? "действие назначено"}",
            CaseStatus.Resolved => "Обращение закрыто после проверки",
            _ => "Обновлено обращение"
        });

        db.SaveChanges();
        return true;
    }

    private void Log(Newcomer person, string kind, string message) =>
        db.ChangeLogs.Add(new ChangeLog
        {
            AtUtc = DateTime.UtcNow,
            NewcomerId = person.Id,
            NewcomerName = person.FullName,
            Kind = kind,
            Message = message
        });
}