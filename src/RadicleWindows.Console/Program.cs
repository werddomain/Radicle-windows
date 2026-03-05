using RadicleWindows.Core.Interfaces;
using RadicleWindows.Core.Services;

namespace RadicleWindows.Console;

/// <summary>
/// Console entry point – forwards commands to the Radicle CLI and optionally
/// launches the WPF GUI when invoked with the "gui" command.
/// Usage:
///   radicle-windows &lt;command&gt; [arguments]
///   radicle-windows gui              → launches the WPF GUI
///   radicle-windows self             → shows your Radicle profile
///   radicle-windows ls               → lists local repositories
///   radicle-windows issue list       → lists issues
///   radicle-windows patch list       → lists patches
///   radicle-windows node status      → shows node status
///   radicle-windows -- &lt;any rad args&gt; → pass-through to rad CLI
/// </summary>
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 0;
        }

        IProcessExecutor executor = new ProcessExecutor();
        IRadicleService radicle = new RadicleService(executor);

        var command = args[0].ToLowerInvariant();

        switch (command)
        {
            case "gui":
                System.Console.WriteLine("Launching the Radicle Windows GUI…");
                System.Console.WriteLine("(The WPF application should be started separately via RadicleWindows.Wpf)");
                return 0;

            case "self":
                return await HandleSelfAsync(radicle);

            case "ls":
                return await HandleListReposAsync(radicle);

            case "issue":
                return await HandleIssueAsync(radicle, args);

            case "patch":
                return await HandlePatchAsync(radicle, args);

            case "node":
                return await HandleNodeAsync(radicle, args);

            case "init":
                return await HandleInitAsync(radicle, args);

            case "clone":
                return await HandleCloneAsync(radicle, args);

            case "sync":
                return await HandleSyncAsync(radicle);

            case "auth":
                return await HandleAuthAsync(radicle);

            case "--":
                // Pass-through to rad
                var passArgs = string.Join(' ', args.Skip(1));
                var result = await radicle.ExecuteRadCommandAsync(passArgs);
                System.Console.Write(result.StandardOutput);
                if (!string.IsNullOrEmpty(result.StandardError))
                    System.Console.Error.Write(result.StandardError);
                return result.ExitCode;

            default:
                // Forward unknown commands directly
                var fwdResult = await radicle.ExecuteRadCommandAsync(string.Join(' ', args));
                System.Console.Write(fwdResult.StandardOutput);
                if (!string.IsNullOrEmpty(fwdResult.StandardError))
                    System.Console.Error.Write(fwdResult.StandardError);
                return fwdResult.ExitCode;
        }
    }

    private static async Task<int> HandleSelfAsync(IRadicleService radicle)
    {
        var profile = await radicle.GetProfileAsync();
        if (profile is null)
        {
            System.Console.Error.WriteLine("Failed to retrieve profile. Is `rad` installed and authenticated?");
            return 1;
        }

        System.Console.WriteLine($"Alias    : {profile.Alias}");
        System.Console.WriteLine($"DID      : {profile.Did}");
        System.Console.WriteLine($"Node ID  : {profile.NodeId}");
        System.Console.WriteLine($"Home     : {profile.Home}");
        System.Console.WriteLine($"SSH Agent: {profile.SshAgent}");
        return 0;
    }

    private static async Task<int> HandleListReposAsync(IRadicleService radicle)
    {
        var repos = await radicle.ListRepositoriesAsync();
        if (repos.Count == 0)
        {
            System.Console.WriteLine("No repositories found.");
            return 0;
        }

        foreach (var repo in repos)
        {
            System.Console.WriteLine($"{repo.Rid}  {repo.Name}  {repo.Description}");
        }
        return 0;
    }

    private static async Task<int> HandleIssueAsync(IRadicleService radicle, string[] args)
    {
        var sub = args.Length > 1 ? args[1].ToLowerInvariant() : "list";
        switch (sub)
        {
            case "list":
                var issues = await radicle.ListIssuesAsync();
                foreach (var issue in issues)
                    System.Console.WriteLine($"[{issue.State}] {issue.Id}  {issue.Title}  (by {issue.Author})");
                return 0;

            case "open" or "create":
                var title = GetArgValue(args, "--title") ?? "Untitled";
                var desc = GetArgValue(args, "--description") ?? "";
                var result = await radicle.CreateIssueAsync(title, desc);
                System.Console.Write(result.StandardOutput);
                return result.ExitCode;

            default:
                var fwd = await radicle.ExecuteRadCommandAsync(string.Join(' ', args));
                System.Console.Write(fwd.StandardOutput);
                return fwd.ExitCode;
        }
    }

    private static async Task<int> HandlePatchAsync(IRadicleService radicle, string[] args)
    {
        var sub = args.Length > 1 ? args[1].ToLowerInvariant() : "list";
        switch (sub)
        {
            case "list":
                var patches = await radicle.ListPatchesAsync();
                foreach (var patch in patches)
                    System.Console.WriteLine($"[{patch.State}] {patch.Id}  {patch.Title}  (by {patch.Author})");
                return 0;

            case "open" or "create":
                var message = GetArgValue(args, "--message") ?? "Untitled patch";
                var result = await radicle.CreatePatchAsync(message);
                System.Console.Write(result.StandardOutput);
                return result.ExitCode;

            default:
                var fwd = await radicle.ExecuteRadCommandAsync(string.Join(' ', args));
                System.Console.Write(fwd.StandardOutput);
                return fwd.ExitCode;
        }
    }

    private static async Task<int> HandleNodeAsync(IRadicleService radicle, string[] args)
    {
        var sub = args.Length > 1 ? args[1].ToLowerInvariant() : "status";
        switch (sub)
        {
            case "status":
                var status = await radicle.GetNodeStatusAsync();
                System.Console.WriteLine($"Running  : {status.IsRunning}");
                System.Console.WriteLine($"Status   : {status.StatusMessage}");
                return 0;

            case "start":
                var startResult = await radicle.StartNodeAsync();
                System.Console.Write(startResult.StandardOutput);
                return startResult.ExitCode;

            case "stop":
                var stopResult = await radicle.StopNodeAsync();
                System.Console.Write(stopResult.StandardOutput);
                return stopResult.ExitCode;

            default:
                var fwd = await radicle.ExecuteRadCommandAsync(string.Join(' ', args));
                System.Console.Write(fwd.StandardOutput);
                return fwd.ExitCode;
        }
    }

    private static async Task<int> HandleInitAsync(IRadicleService radicle, string[] args)
    {
        var name = GetArgValue(args, "--name") ?? "my-project";
        var desc = GetArgValue(args, "--description") ?? "";
        var branch = GetArgValue(args, "--default-branch") ?? "main";
        var result = await radicle.InitAsync(name, desc, branch);
        System.Console.Write(result.StandardOutput);
        if (!string.IsNullOrEmpty(result.StandardError))
            System.Console.Error.Write(result.StandardError);
        return result.ExitCode;
    }

    private static async Task<int> HandleCloneAsync(IRadicleService radicle, string[] args)
    {
        if (args.Length < 2)
        {
            System.Console.Error.WriteLine("Usage: radicle-windows clone <RID>");
            return 1;
        }
        var result = await radicle.CloneAsync(args[1]);
        System.Console.Write(result.StandardOutput);
        if (!string.IsNullOrEmpty(result.StandardError))
            System.Console.Error.Write(result.StandardError);
        return result.ExitCode;
    }

    private static async Task<int> HandleSyncAsync(IRadicleService radicle)
    {
        var result = await radicle.SyncAsync();
        System.Console.Write(result.StandardOutput);
        if (!string.IsNullOrEmpty(result.StandardError))
            System.Console.Error.Write(result.StandardError);
        return result.ExitCode;
    }

    private static async Task<int> HandleAuthAsync(IRadicleService radicle)
    {
        var result = await radicle.AuthAsync();
        System.Console.Write(result.StandardOutput);
        if (!string.IsNullOrEmpty(result.StandardError))
            System.Console.Error.Write(result.StandardError);
        return result.ExitCode;
    }

    private static string? GetArgValue(string[] args, string flag)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals(flag, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }
        return null;
    }

    private static void PrintUsage()
    {
        System.Console.WriteLine("""
            Radicle Windows – CLI & GUI for the Radicle peer-to-peer code collaboration stack

            Usage:
              radicle-windows <command> [options]

            Commands:
              gui               Launch the WPF graphical interface
              self              Show your Radicle identity
              auth              Authenticate / create identity
              ls                List local repositories
              init              Initialize a new Radicle repository
              clone <RID>       Clone a repository by Radicle ID
              sync              Synchronize with the network
              issue list        List issues
              issue open        Create a new issue
              patch list        List patches
              patch open        Submit a new patch
              node status       Show node status
              node start        Start the Radicle node
              node stop         Stop the Radicle node
              -- <args>         Pass-through to the rad CLI

            For more information visit https://radicle.xyz/
            """);
    }
}
