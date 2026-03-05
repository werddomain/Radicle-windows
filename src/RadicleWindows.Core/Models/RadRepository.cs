namespace RadicleWindows.Core.Models;

/// <summary>
/// Represents a Radicle repository.
/// </summary>
public sealed class RadRepository
{
    public string Rid { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string DefaultBranch { get; init; } = string.Empty;
    public string Head { get; init; } = string.Empty;
}
