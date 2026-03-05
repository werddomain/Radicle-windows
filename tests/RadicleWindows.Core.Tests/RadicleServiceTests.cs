using Moq;
using RadicleWindows.Core.Interfaces;
using RadicleWindows.Core.Models;
using RadicleWindows.Core.Services;

namespace RadicleWindows.Core.Tests;

public class RadicleServiceTests
{
    private readonly Mock<IProcessExecutor> _mockExecutor;
    private readonly RadicleService _service;

    public RadicleServiceTests()
    {
        _mockExecutor = new Mock<IProcessExecutor>();
        _service = new RadicleService(_mockExecutor.Object, "rad");
    }

    [Fact]
    public async Task GetProfileAsync_WhenRadSucceeds_ReturnsProfile()
    {
        _mockExecutor
            .Setup(x => x.ExecuteAsync("rad", "self", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RadCommandResult
            {
                ExitCode = 0,
                StandardOutput = "Alias : alice\nDID : did:key:z6Mkg\nNode ID : z6Mkg\n",
                StandardError = "",
            });

        var profile = await _service.GetProfileAsync();

        Assert.NotNull(profile);
        Assert.Equal("alice", profile.Alias);
    }

    [Fact]
    public async Task GetProfileAsync_WhenRadFails_ReturnsNull()
    {
        _mockExecutor
            .Setup(x => x.ExecuteAsync("rad", "self", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RadCommandResult
            {
                ExitCode = 1,
                StandardOutput = "",
                StandardError = "rad: not authenticated",
            });

        var profile = await _service.GetProfileAsync();

        Assert.Null(profile);
    }

    [Fact]
    public async Task ListRepositoriesAsync_ReturnsRepos()
    {
        _mockExecutor
            .Setup(x => x.ExecuteAsync("rad", "ls", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RadCommandResult
            {
                ExitCode = 0,
                StandardOutput = "rad:z3gqcJUoA1n9HaHKufZs5FCSGaNv4 heartwood Radicle abc1234\n",
                StandardError = "",
            });

        var repos = await _service.ListRepositoriesAsync();

        Assert.Single(repos);
        Assert.Equal("heartwood", repos[0].Name);
    }

    [Fact]
    public async Task GetNodeStatusAsync_WhenRunning_ReturnsRunning()
    {
        _mockExecutor
            .Setup(x => x.ExecuteAsync("rad", "node status", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RadCommandResult
            {
                ExitCode = 0,
                StandardOutput = "Node is running on z6Mkg2...",
                StandardError = "",
            });

        var status = await _service.GetNodeStatusAsync();

        Assert.True(status.IsRunning);
    }

    [Fact]
    public async Task ExecuteRadCommandAsync_PassesThroughToExecutor()
    {
        _mockExecutor
            .Setup(x => x.ExecuteAsync("rad", "help", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RadCommandResult
            {
                ExitCode = 0,
                StandardOutput = "Radicle help output",
                StandardError = "",
            });

        var result = await _service.ExecuteRadCommandAsync("help");

        Assert.True(result.Success);
        Assert.Equal("Radicle help output", result.StandardOutput);
    }

    [Fact]
    public async Task InitAsync_CallsRadInit()
    {
        _mockExecutor
            .Setup(x => x.ExecuteAsync("rad", It.Is<string>(s => s.Contains("init")), "/tmp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RadCommandResult
            {
                ExitCode = 0,
                StandardOutput = "Repository initialized",
                StandardError = "",
            });

        var result = await _service.InitAsync("test", "A test project", "main", "/tmp");

        Assert.True(result.Success);
        _mockExecutor.Verify(x => x.ExecuteAsync("rad", It.Is<string>(s => s.Contains("--name \"test\"")), "/tmp", It.IsAny<CancellationToken>()), Times.Once);
    }
}
