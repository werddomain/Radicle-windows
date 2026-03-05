using System.Text.RegularExpressions;
using RadicleWindows.Core.Interfaces;
using RadicleWindows.Core.Models;

namespace RadicleWindows.Core.Services;

/// <summary>
/// Service that wraps the Radicle CLI (rad) and provides a .NET-friendly API.
/// </summary>
public sealed partial class RadicleService : IRadicleService
{
    private readonly IProcessExecutor _executor;
    private readonly string _radPath;

    public RadicleService(IProcessExecutor executor, string radPath = "rad")
    {
        _executor = executor;
        _radPath = radPath;
    }

    // ── Identity ──────────────────────────────────────────────

    public async Task<RadProfile?> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        var result = await ExecuteRadCommandAsync("self", cancellationToken: cancellationToken);
        if (!result.Success) return null;
        return ParseProfile(result.StandardOutput);
    }

    public Task<RadCommandResult> AuthAsync(CancellationToken cancellationToken = default)
        => ExecuteRadCommandAsync("auth", cancellationToken: cancellationToken);

    // ── Repository ────────────────────────────────────────────

    public async Task<IReadOnlyList<RadRepository>> ListRepositoriesAsync(CancellationToken cancellationToken = default)
    {
        var result = await ExecuteRadCommandAsync("ls", cancellationToken: cancellationToken);
        if (!result.Success) return [];
        return ParseRepositories(result.StandardOutput);
    }

    public Task<RadCommandResult> InitAsync(string name, string description, string defaultBranch, string? workingDirectory = null, CancellationToken cancellationToken = default)
        => ExecuteRadCommandAsync($"init --name \"{name}\" --description \"{description}\" --default-branch \"{defaultBranch}\"", workingDirectory, cancellationToken);

    public Task<RadCommandResult> CloneAsync(string rid, string? workingDirectory = null, CancellationToken cancellationToken = default)
        => ExecuteRadCommandAsync($"clone {rid}", workingDirectory, cancellationToken);

    public Task<RadCommandResult> SyncAsync(string? workingDirectory = null, CancellationToken cancellationToken = default)
        => ExecuteRadCommandAsync("sync", workingDirectory, cancellationToken);

    public Task<RadCommandResult> InspectAsync(string? workingDirectory = null, CancellationToken cancellationToken = default)
        => ExecuteRadCommandAsync("inspect", workingDirectory, cancellationToken);

    // ── Issues ────────────────────────────────────────────────

    public async Task<IReadOnlyList<RadIssue>> ListIssuesAsync(string? workingDirectory = null, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteRadCommandAsync("issue list", workingDirectory, cancellationToken);
        if (!result.Success) return [];
        return ParseIssues(result.StandardOutput);
    }

    public Task<RadCommandResult> CreateIssueAsync(string title, string description, string? workingDirectory = null, CancellationToken cancellationToken = default)
        => ExecuteRadCommandAsync($"issue open --title \"{title}\" --description \"{description}\"", workingDirectory, cancellationToken);

    // ── Patches ───────────────────────────────────────────────

    public async Task<IReadOnlyList<RadPatch>> ListPatchesAsync(string? workingDirectory = null, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteRadCommandAsync("patch list", workingDirectory, cancellationToken);
        if (!result.Success) return [];
        return ParsePatches(result.StandardOutput);
    }

    public Task<RadCommandResult> CreatePatchAsync(string message, string? workingDirectory = null, CancellationToken cancellationToken = default)
        => ExecuteRadCommandAsync($"patch open --message \"{message}\"", workingDirectory, cancellationToken);

    // ── Node ──────────────────────────────────────────────────

    public async Task<RadNodeStatus> GetNodeStatusAsync(CancellationToken cancellationToken = default)
    {
        var result = await ExecuteRadCommandAsync("node status", cancellationToken: cancellationToken);
        return ParseNodeStatus(result);
    }

    public Task<RadCommandResult> StartNodeAsync(CancellationToken cancellationToken = default)
        => ExecuteRadCommandAsync("node start", cancellationToken: cancellationToken);

    public Task<RadCommandResult> StopNodeAsync(CancellationToken cancellationToken = default)
        => ExecuteRadCommandAsync("node stop", cancellationToken: cancellationToken);

    // ── Generic ───────────────────────────────────────────────

    public Task<RadCommandResult> ExecuteRadCommandAsync(string arguments, string? workingDirectory = null, CancellationToken cancellationToken = default)
        => _executor.ExecuteAsync(_radPath, arguments, workingDirectory, cancellationToken);

    // ── Parsers ───────────────────────────────────────────────

    internal static RadProfile ParseProfile(string output)
    {
        var profile = new RadProfile();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var parts = line.Split(':', 2, StringSplitOptions.TrimEntries);
            if (parts.Length < 2) continue;
            var key = parts[0].Trim().ToLowerInvariant();
            var value = parts[1].Trim();
            profile = key switch
            {
                "node id" or "nid" => profile with { NodeId = value },
                "did" => profile with { Did = value },
                "alias" => profile with { Alias = value },
                "home" => profile with { Home = value },
                "ssh-agent" or "ssh agent" => profile with { SshAgent = value },
                _ => profile,
            };
        }
        return profile;
    }

    internal static IReadOnlyList<RadRepository> ParseRepositories(string output)
    {
        var repos = new List<RadRepository>();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var match = RepoLineRegex().Match(line);
            if (match.Success)
            {
                repos.Add(new RadRepository
                {
                    Rid = match.Groups["rid"].Value,
                    Name = match.Groups["name"].Value,
                    Description = match.Groups["desc"].Value,
                    Head = match.Groups["head"].Value,
                });
            }
        }
        return repos;
    }

    internal static IReadOnlyList<RadIssue> ParseIssues(string output)
    {
        var issues = new List<RadIssue>();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var match = IssueLineRegex().Match(line);
            if (match.Success)
            {
                issues.Add(new RadIssue
                {
                    Id = match.Groups["id"].Value,
                    Title = match.Groups["title"].Value.Trim('"'),
                    State = match.Groups["state"].Value,
                    Author = match.Groups["author"].Value,
                });
            }
        }
        return issues;
    }

    internal static IReadOnlyList<RadPatch> ParsePatches(string output)
    {
        var patches = new List<RadPatch>();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var match = PatchLineRegex().Match(line);
            if (match.Success)
            {
                patches.Add(new RadPatch
                {
                    Id = match.Groups["id"].Value,
                    Title = match.Groups["title"].Value.Trim('"'),
                    State = match.Groups["state"].Value,
                    Author = match.Groups["author"].Value,
                });
            }
        }
        return patches;
    }

    internal static RadNodeStatus ParseNodeStatus(RadCommandResult result)
    {
        if (!result.Success)
        {
            return new RadNodeStatus
            {
                IsRunning = false,
                StatusMessage = result.StandardError.Length > 0 ? result.StandardError.Trim() : "Node is not running",
            };
        }

        var output = result.StandardOutput;
        var isRunning = output.Contains("running", StringComparison.OrdinalIgnoreCase);
        return new RadNodeStatus
        {
            IsRunning = isRunning,
            StatusMessage = output.Trim(),
        };
    }

    // Regex patterns for parsing CLI output.
    // These match common formats from `rad ls`, `rad issue list`, `rad patch list`.
    [GeneratedRegex(@"(?<rid>rad:[a-zA-Z0-9]+)\s+(?<name>\S+)\s+(?<desc>.+?)\s+(?<head>[a-f0-9]+)\s*$")]
    private static partial Regex RepoLineRegex();

    [GeneratedRegex(@"(?<id>[a-f0-9]+)\s+(?<state>\S+)\s+(?<title>""[^""]*""|\S+)\s+(?<author>\S+)")]
    private static partial Regex IssueLineRegex();

    [GeneratedRegex(@"(?<id>[a-f0-9]+)\s+(?<state>\S+)\s+(?<title>""[^""]*""|\S+)\s+(?<author>\S+)")]
    private static partial Regex PatchLineRegex();
}
