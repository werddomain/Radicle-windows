using RadicleWindows.Core.Models;

namespace RadicleWindows.Core.Interfaces;

/// <summary>
/// Abstraction for executing processes.
/// </summary>
public interface IProcessExecutor
{
    Task<RadCommandResult> ExecuteAsync(string fileName, string arguments, string? workingDirectory = null, CancellationToken cancellationToken = default);
}
