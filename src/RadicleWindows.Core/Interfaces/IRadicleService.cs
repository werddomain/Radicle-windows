using RadicleWindows.Core.Models;

namespace RadicleWindows.Core.Interfaces;

/// <summary>
/// High-level interface for interacting with the Radicle CLI.
/// </summary>
public interface IRadicleService
{
    // Identity
    Task<RadProfile?> GetProfileAsync(CancellationToken cancellationToken = default);
    Task<RadCommandResult> AuthAsync(CancellationToken cancellationToken = default);

    // Repository
    Task<IReadOnlyList<RadRepository>> ListRepositoriesAsync(CancellationToken cancellationToken = default);
    Task<RadCommandResult> InitAsync(string name, string description, string defaultBranch, string? workingDirectory = null, CancellationToken cancellationToken = default);
    Task<RadCommandResult> CloneAsync(string rid, string? workingDirectory = null, CancellationToken cancellationToken = default);
    Task<RadCommandResult> SyncAsync(string? workingDirectory = null, CancellationToken cancellationToken = default);
    Task<RadCommandResult> InspectAsync(string? workingDirectory = null, CancellationToken cancellationToken = default);

    // Issues
    Task<IReadOnlyList<RadIssue>> ListIssuesAsync(string? workingDirectory = null, CancellationToken cancellationToken = default);
    Task<RadCommandResult> CreateIssueAsync(string title, string description, string? workingDirectory = null, CancellationToken cancellationToken = default);

    // Patches
    Task<IReadOnlyList<RadPatch>> ListPatchesAsync(string? workingDirectory = null, CancellationToken cancellationToken = default);
    Task<RadCommandResult> CreatePatchAsync(string message, string? workingDirectory = null, CancellationToken cancellationToken = default);

    // Node
    Task<RadNodeStatus> GetNodeStatusAsync(CancellationToken cancellationToken = default);
    Task<RadCommandResult> StartNodeAsync(CancellationToken cancellationToken = default);
    Task<RadCommandResult> StopNodeAsync(CancellationToken cancellationToken = default);

    // Generic command
    Task<RadCommandResult> ExecuteRadCommandAsync(string arguments, string? workingDirectory = null, CancellationToken cancellationToken = default);
}
