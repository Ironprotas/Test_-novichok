using TestNewPeople.Models;
using TestNewPeople.Services;

namespace TestNewPeople.Data;

public static class DbSeed
{
    public static async Task EnsureSeededAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        if (db.Newcomers.Any()) return;

        var today = DateOnly.FromDateTime(DateTime.Today);

        // 1) Проблема с терминалом — полный путь уже частично пройден, можно довести до «Решено»
        var problem = MakePerson(
            "n01",
            "Алексей Морозов",
            "Игорь К.",
            "День",
            today.AddDays(-8),
            new Mentor
            {
                Name = "Сергей Наставник",
                RoleTitle = "Бригадир склада",
                Phone = "+7 900 111-22-01",
                Messenger = "Telegram @sklad_sergey",
                Shift = "День"
            },
            taskDone: [true, false, false]);

        problem.Survey = new WeekSurvey
        {
            NewcomerId = "n01",
            Day = 7,
            CompletedOn = today.AddDays(-1),
            ClarityScore = 2,
            ContactScore = 4,
            BlockerText = "Не получается уверенно работать с терминалом на приёмке — сканер часто сбрасывает позицию."
        };

        problem.Cases.Add(new HelpCase
        {
            NewcomerId = "n01",
            Reason = "Проблема с терминалом на приёмке",
            NewcomerReply = "Не получается уверенно работать с терминалом на приёмке — сканер часто сбрасывает позицию.",
            Status = CaseStatus.InProgress,
            NextAction = "Совместная приёмка с наставником: 15 позиций на терминале",
            CheckDate = today,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            UpdatedAtUtc = DateTime.UtcNow.AddHours(-5)
        });

        // 2) Всё в порядке
        var ok = MakePerson(
            "n02",
            "Мария Соколова",
            "Ольга М.",
            "День",
            today.AddDays(-9),
            new Mentor
            {
                Name = "Ирина Кладовщица",
                RoleTitle = "Старший кладовщик",
                Phone = "+7 900 111-22-02",
                Messenger = "WhatsApp",
                Shift = "День"
            },
            taskDone: [true, true, true]);

        ok.Survey = new WeekSurvey
        {
            NewcomerId = "n02",
            Day = 7,
            CompletedOn = today.AddDays(-2),
            ClarityScore = 5,
            ContactScore = 5,
            BlockerText = "Пока ничего существенного не мешает — вопросы закрываем со смены."
        };

        // 3) Ещё не ответил + наставник в другой смене
        var silent = MakePerson(
            "n03",
            "Кирилл Белов",
            "Игорь К.",
            "День",
            today.AddDays(-7),
            new Mentor
            {
                Name = "Сергей Наставник",
                RoleTitle = "Бригадир склада",
                Phone = "+7 900 111-22-01",
                Messenger = "Telegram @sklad_sergey",
                Shift = "Ночь"
            },
            taskDone: [true, true, false]);

        silent.Survey = new WeekSurvey
        {
            NewcomerId = "n03",
            Day = 7
        };

        silent.MentorMarks.Add(new MentorMark
        {
            NewcomerId = "n03",
            Label = "Не обращается ко мне",
            Comment = "Работает молча, вопросы не задаёт — непонятно, всё ли ясно.",
            NeedsAttention = true,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow.AddHours(-6)
        });

        db.Newcomers.AddRange(problem, ok, silent);
        db.ChangeLogs.AddRange(
            new ChangeLog
            {
                AtUtc = DateTime.UtcNow.AddDays(-1),
                NewcomerId = "n01",
                NewcomerName = problem.FullName,
                Kind = "case",
                Message = "Сообщил о проблеме с терминалом на приёмке"
            },
            new ChangeLog
            {
                AtUtc = DateTime.UtcNow.AddHours(-5),
                NewcomerId = "n01",
                NewcomerName = problem.FullName,
                Kind = "case",
                Message = "Руководитель назначил совместную приёмку с наставником"
            },
            new ChangeLog
            {
                AtUtc = DateTime.UtcNow.AddHours(-6),
                NewcomerId = "n03",
                NewcomerName = silent.FullName,
                Kind = "mentor",
                Message = "Наставник отметил: «Не обращается ко мне»"
            },
            new ChangeLog
            {
                AtUtc = DateTime.UtcNow,
                NewcomerId = "system",
                NewcomerName = "Система",
                Kind = "seed",
                Message = "Демо: склад, первая неделя, опрос 7-го дня, метки наставника"
            });

        await db.SaveChangesAsync();
    }

    private static Newcomer MakePerson(
        string id,
        string name,
        string manager,
        string shift,
        DateOnly start,
        Mentor mentor,
        bool[] taskDone)
    {
        return new Newcomer
        {
            Id = id,
            FullName = name,
            Location = "Склад Север",
            ManagerName = manager,
            Shift = shift,
            StartDate = start,
            Mentor = mentor,
            Tasks = WarehouseWeekPlan.CreateTasks(id, taskDone),
            Survey = new WeekSurvey { NewcomerId = id, Day = 7 }
        };
    }
}