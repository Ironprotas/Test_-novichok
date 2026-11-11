namespace TestNewPeople.Models;

public sealed class AttentionItem
{
    public required AttentionKind Kind { get; init; }
    public required string Title { get; init; }
    public required string Detail { get; init; }
    public required string SuggestedAction { get; init; }
}