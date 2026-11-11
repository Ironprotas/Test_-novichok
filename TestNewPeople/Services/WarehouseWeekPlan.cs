using TestNewPeople.Models;

namespace TestNewPeople.Services;

public static class WarehouseWeekPlan
{
    public static List<OnboardingTask> CreateTasks(string newcomerId, bool[]? done = null)
    {
        done ??= [false, false, false];
        var titles = new[]
        {
            ("t1", "Пропуск, форма и инструктаж по ТБ", "Получить пропуск, спецодежду и пройти инструктаж.", 1, 1),
            ("t2", "Знакомство со складом и наставником", "Пройти зоны приёмки, хранения и отгрузки вместе с наставником.", 1, 2),
            ("t3", "Терминал: первая приёмка под присмотром", "Принять 10–15 позиций на терминале рядом с наставником.", 2, 5)
        };

        return titles.Select((t, i) => new OnboardingTask
        {
            NewcomerId = newcomerId,
            Id = t.Item1,
            Title = t.Item2,
            Description = t.Item3,
            DayFrom = t.Item4,
            DayTo = t.Item5,
            Status = i < done.Length && done[i] ? PlanTaskStatus.Done : PlanTaskStatus.Todo
        }).ToList();
    }
}