namespace RadicleWindows.Core.Models;

/// <summary>
/// Represents the result of executing a Radicle CLI command.
/// </summary>
public sealed class RadCommandResult
{
    public int ExitCode { get; init; }
    public string StandardOutput { get; init; } = string.Empty;
    public string StandardError { get; init; } = string.Empty;
    public bool Success => ExitCode == 0;
}
