namespace RadicleWindows.Core.Models;

/// <summary>
/// Represents the status of the Radicle node.
/// </summary>
public sealed class RadNodeStatus
{
    public bool IsRunning { get; init; }
    public string Version { get; init; } = string.Empty;
    public string NodeId { get; init; } = string.Empty;
    public string ListeningAddress { get; init; } = string.Empty;
    public string StatusMessage { get; init; } = string.Empty;
}
