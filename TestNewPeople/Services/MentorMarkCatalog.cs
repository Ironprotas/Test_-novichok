namespace TestNewPeople.Services;

public static class MentorMarkCatalog
{
    public static IReadOnlyList<(string Label, bool NeedsAttention)> Presets { get; } =
    [
        ("Не обращается ко мне", true),
        ("Теряется без чёткого задания", true),
        ("Нужна помощь с терминалом / оборудованием", true),
        ("Не пересекаемся по сменам", true),
        ("Хорошо справляется, вопросов почти нет", false),
        ("Другое", true)
    ];
}