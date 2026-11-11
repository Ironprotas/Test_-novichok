namespace TestNewPeople.Models;

public enum PlanTaskStatus
{
    Todo,
    Done
}

public enum CaseStatus
{
    NeedsHelp,
    InProgress,
    Resolved
}

public enum AttentionKind
{
    None,
    MentorOtherShift,
    TaskOverdue,
    SurveyMissing,
    OpenCase,
    MentorMark
}