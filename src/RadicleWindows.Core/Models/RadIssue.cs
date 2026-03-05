namespace RadicleWindows.Core.Models;

/// <summary>
/// Represents a Radicle issue.
/// </summary>
public sealed class RadIssue
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public List<string> Labels { get; init; } = [];
    public List<string> Assignees { get; init; } = [];
    public DateTime? Opened { get; init; }
}
