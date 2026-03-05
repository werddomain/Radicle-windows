namespace RadicleWindows.Core.Models;

/// <summary>
/// Represents a Radicle patch (similar to a pull request).
/// </summary>
public sealed class RadPatch
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public List<string> Labels { get; init; } = [];
    public int Revisions { get; init; }
    public DateTime? Opened { get; init; }
}
