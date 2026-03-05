using RadicleWindows.Core.Models;
using RadicleWindows.Core.Services;

namespace RadicleWindows.Core.Tests;

public class RadicleServiceParserTests
{
    [Fact]
    public void ParseProfile_ValidOutput_ReturnsProfile()
    {
        var output = """
            Node ID : z6Mkg2stTwq6MxKj84iWniBW2pRkGKFa7mPFJQTnDMtz5PN1
            DID     : did:key:z6Mkg2stTwq6MxKj84iWniBW2pRkGKFa7mPFJQTnDMtz5PN1
            Alias   : alice
            Home    : /home/alice/.radicle
            SSH Agent: running
            """;

        var profile = RadicleService.ParseProfile(output);

        Assert.Equal("z6Mkg2stTwq6MxKj84iWniBW2pRkGKFa7mPFJQTnDMtz5PN1", profile.NodeId);
        Assert.Equal("did:key:z6Mkg2stTwq6MxKj84iWniBW2pRkGKFa7mPFJQTnDMtz5PN1", profile.Did);
        Assert.Equal("alice", profile.Alias);
        Assert.Equal("/home/alice/.radicle", profile.Home);
        Assert.Equal("running", profile.SshAgent);
    }

    [Fact]
    public void ParseProfile_EmptyOutput_ReturnsEmptyProfile()
    {
        var profile = RadicleService.ParseProfile("");

        Assert.Equal(string.Empty, profile.NodeId);
        Assert.Equal(string.Empty, profile.Alias);
    }

    [Fact]
    public void ParseRepositories_ValidOutput_ReturnsList()
    {
        var output = """
            rad:z3gqcJUoA1n9HaHKufZs5FCSGaNv4 heartwood A Radicle heart abc1234
            rad:z4nqcJUoB2m8HbHLvgAt6GDThBOv5 my-project Another project def5678
            """;

        var repos = RadicleService.ParseRepositories(output);

        Assert.Equal(2, repos.Count);
        Assert.Equal("rad:z3gqcJUoA1n9HaHKufZs5FCSGaNv4", repos[0].Rid);
        Assert.Equal("heartwood", repos[0].Name);
        Assert.Equal("rad:z4nqcJUoB2m8HbHLvgAt6GDThBOv5", repos[1].Rid);
    }

    [Fact]
    public void ParseRepositories_EmptyOutput_ReturnsEmptyList()
    {
        var repos = RadicleService.ParseRepositories("");

        Assert.Empty(repos);
    }

    [Fact]
    public void ParseIssues_ValidOutput_ReturnsList()
    {
        var output = """
            abc1234 open "Fix the build" alice
            def5678 closed "Update README" bob
            """;

        var issues = RadicleService.ParseIssues(output);

        Assert.Equal(2, issues.Count);
        Assert.Equal("abc1234", issues[0].Id);
        Assert.Equal("Fix the build", issues[0].Title);
        Assert.Equal("open", issues[0].State);
        Assert.Equal("alice", issues[0].Author);
        Assert.Equal("closed", issues[1].State);
    }

    [Fact]
    public void ParseIssues_EmptyOutput_ReturnsEmptyList()
    {
        var issues = RadicleService.ParseIssues("");

        Assert.Empty(issues);
    }

    [Fact]
    public void ParsePatches_ValidOutput_ReturnsList()
    {
        var output = """
            aaa1111 open "Add feature X" alice
            bbb2222 merged "Fix bug Y" bob
            """;

        var patches = RadicleService.ParsePatches(output);

        Assert.Equal(2, patches.Count);
        Assert.Equal("aaa1111", patches[0].Id);
        Assert.Equal("Add feature X", patches[0].Title);
        Assert.Equal("open", patches[0].State);
        Assert.Equal("merged", patches[1].State);
    }

    [Fact]
    public void ParsePatches_EmptyOutput_ReturnsEmptyList()
    {
        var patches = RadicleService.ParsePatches("");

        Assert.Empty(patches);
    }

    [Fact]
    public void ParseNodeStatus_SuccessRunning_ReturnsRunning()
    {
        var result = new RadCommandResult
        {
            ExitCode = 0,
            StandardOutput = "Node is running on z6Mkg2...",
            StandardError = "",
        };

        var status = RadicleService.ParseNodeStatus(result);

        Assert.True(status.IsRunning);
    }

    [Fact]
    public void ParseNodeStatus_FailedResult_ReturnsNotRunning()
    {
        var result = new RadCommandResult
        {
            ExitCode = 1,
            StandardOutput = "",
            StandardError = "Node is not running",
        };

        var status = RadicleService.ParseNodeStatus(result);

        Assert.False(status.IsRunning);
        Assert.Equal("Node is not running", status.StatusMessage);
    }
}
