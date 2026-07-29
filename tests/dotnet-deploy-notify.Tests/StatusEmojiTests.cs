using Xunit;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Formatting;

namespace DotNetDeployNotify.Tests;

public class StatusEmojiTests
{
    [Theory]
    [InlineData(BuildStatus.Success, "✅")]
    [InlineData(BuildStatus.SuccessWithWarnings, "⚠️")]
    [InlineData(BuildStatus.Failed, "❌")]
    [InlineData(BuildStatus.DeploymentSuccess, "🚀")]
    [InlineData(BuildStatus.DeploymentFailed, "💥")]
    [InlineData(BuildStatus.Deploying, "🔄")]
    [InlineData(BuildStatus.InProgress, "⏳")]
    [InlineData(BuildStatus.Cancelled, "🛑")]
    [InlineData(BuildStatus.Started, "▶️")]
    // An undefined enum value should fall back to the default emoji
    [InlineData((BuildStatus)999, "ℹ️")]
    public void Get_ReturnsExpectedEmoji(BuildStatus status, string expectedEmoji)
    {
        // Act
        var actual = StatusEmoji.Get(status);

        // Assert
        Assert.Equal(expectedEmoji, actual);
    }

    [Theory]
    [InlineData(BuildStatus.Success, false, "")]
    [InlineData(BuildStatus.Failed, false, "")]
    [InlineData(BuildStatus.Started, true, "▶️")]
    public void Get_WithEnableEmojis_ReturnsConditionalResult(BuildStatus status, bool enableEmojis, string expected)
    {
        // Act
        var actual = StatusEmoji.Get(status, enableEmojis);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(BuildStatus.Success, true, "✅ Success")]
    [InlineData(BuildStatus.Failed, true, "❌ Failed")]
    [InlineData(BuildStatus.Started, false, "Started")]
    public void Format_ReturnsCorrectString(BuildStatus status, bool enableEmojis, string expected)
    {
        // Act
        var actual = StatusEmoji.Format(status, enableEmojis);

        // Assert
        Assert.Equal(expected, actual);
    }
}
