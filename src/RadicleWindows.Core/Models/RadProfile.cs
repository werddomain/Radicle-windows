namespace RadicleWindows.Core.Models;

/// <summary>
/// Represents a Radicle identity (profile).
/// </summary>
public sealed record RadProfile
{
    public string NodeId { get; init; } = string.Empty;
    public string Did { get; init; } = string.Empty;
    public string Alias { get; init; } = string.Empty;
    public string Home { get; init; } = string.Empty;
    public string SshAgent { get; init; } = string.Empty;
}
