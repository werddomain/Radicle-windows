using RadicleWindows.Core.Models;

namespace RadicleWindows.Core.Tests;

public class RadCommandResultTests
{
    [Fact]
    public void Success_WhenExitCodeIsZero_ReturnsTrue()
    {
        var result = new RadCommandResult { ExitCode = 0 };
        Assert.True(result.Success);
    }

    [Fact]
    public void Success_WhenExitCodeIsNonZero_ReturnsFalse()
    {
        var result = new RadCommandResult { ExitCode = 1 };
        Assert.False(result.Success);
    }

    [Fact]
    public void DefaultValues_AreEmpty()
    {
        var result = new RadCommandResult();
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Equal(0, result.ExitCode);
    }
}
