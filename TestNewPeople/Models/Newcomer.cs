namespace TestNewPeople.Models;

public class Mentor
{
    public string Name { get; set; } = "";
    public string RoleTitle { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Messenger { get; set; } = "";
    public string Shift { get; set; } = "";
}

public class OnboardingTask
{
    public string NewcomerId { get; set; } = "";
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int DayFrom { get; set; }
    public int DayTo { get; set; }
    public PlanTaskStatus Status { get; set; } = PlanTaskStatus.Todo;
    public Newcomer? Newcomer { get; set; }
}

public class WeekSurvey
{
    public string NewcomerId { get; set; } = "";
    public int Day { get; set; } = 7;
    public DateOnly? CompletedOn { get; set; }
    /// <summary>1–5: понятно ли, что делать в смене</summary>
    public int? ClarityScore { get; set; }
    /// <summary>1–5: знаешь ли, к кому обратиться</summary>
    public int? ContactScore { get; set; }
    /// <summary>Что сейчас мешает работать самостоятельно</summary>
    public string? BlockerText { get; set; }

    public Newcomer? Newcomer { get; set; }
    public bool IsCompleted => CompletedOn.HasValue;
}

public class HelpCase
{
    public int Id { get; set; }
    public string NewcomerId { get; set; } = "";
    public string Reason { get; set; } = "";
    public string NewcomerReply { get; set; } = "";
    public CaseStatus Status { get; set; } = CaseStatus.NeedsHelp;
    public string? NextAction { get; set; }
    public DateOnly? CheckDate { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }

    public Newcomer? Newcomer { get; set; }
}

/// <summary>Метка наставника о том, как идёт адаптация.</summary>
public class MentorMark
{
    public int Id { get; set; }
    public string NewcomerId { get; set; } = "";
    public string Label { get; set; } = "";
    public string? Comment { get; set; }
    public bool NeedsAttention { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public Newcomer? Newcomer { get; set; }
}

public class ChangeLog
{
    public int Id { get; set; }
    public DateTime AtUtc { get; set; }
    public string NewcomerId { get; set; } = "";
    public string NewcomerName { get; set; } = "";
    public string Kind { get; set; } = "";
    public string Message { get; set; } = "";
}

public class Newcomer
{
    public string Id { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Location { get; set; } = "Склад Север";
    public string ManagerName { get; set; } = "";
    public string Shift { get; set; } = "";
    public DateOnly StartDate { get; set; }
    public Mentor Mentor { get; set; } = new();
    public List<OnboardingTask> Tasks { get; set; } = [];
    public WeekSurvey? Survey { get; set; }
    public List<HelpCase> Cases { get; set; } = [];
    public List<MentorMark> MentorMarks { get; set; } = [];
    public bool IsActive { get; set; } = true;

    public int DayNumber(DateOnly today) =>
        Math.Max(1, today.DayNumber - StartDate.DayNumber + 1);
}