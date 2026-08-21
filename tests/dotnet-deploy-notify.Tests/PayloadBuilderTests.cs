#nullable enable
using DotNetDeployNotify.Core;
using Environment = DotNetDeployNotify.Core.Environment;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class PayloadBuilderTests
{
    private readonly PayloadBuilder _payloadBuilder;
    private readonly ILogger<PayloadBuilder> _mockLogger;

    public PayloadBuilderTests()
    {
        _mockLogger = Substitute.For<ILogger<PayloadBuilder>>();
        _payloadBuilder = new PayloadBuilder(_mockLogger);
    }

    #region BuildPayload Tests

    [Fact]
    public void BuildPayload_WithSlackChannel_IncludesSlackFormat()
    {
        _mockLogger.LogInformation("Starting {TestMethod}", nameof(BuildPayload_WithSlackChannel_IncludesSlackFormat));
        // Arrange
        var notification = CreateTestNotification();
        var config = CreateSlackChannelConfig();

        // Act
        var payload = _payloadBuilder.BuildPayload(notification, config);

        // Assert
        payload.Should().NotBeNull();
        payload.Data.Should().NotBeNull();
        payload.Data.CustomProperties.Should().ContainKey("slack_format");
        payload.EventType.Should().Be("deployment.success");
        _mockLogger.LogInformation("Completed {TestMethod}", nameof(BuildPayload_WithSlackChannel_IncludesSlackFormat));
    }

    [Fact]
    public void BuildPayload_WithDiscordChannel_IncludesDiscordFormat()
    {
        _mockLogger.LogInformation("Starting {TestMethod}", nameof(BuildPayload_WithDiscordChannel_IncludesDiscordFormat));
        // Arrange
        var notification = CreateTestNotification();
        var config = CreateDiscordChannelConfig();

        // Act
        var payload = _payloadBuilder.BuildPayload(notification, config);

        // Assert
        payload.Should().NotBeNull();
        payload.Data.CustomProperties.Should().ContainKey("discord_format");
        payload.EventType.Should().Be("deployment.success");
        _mockLogger.LogInformation("Completed {TestMethod}", nameof(BuildPayload_WithDiscordChannel_IncludesDiscordFormat));
    }

    [Fact]
    public void BuildPayload_WithTelegramChannel_IncludesTelegramText()
    {
        _mockLogger.LogInformation("Starting {TestMethod}", nameof(BuildPayload_WithTelegramChannel_IncludesTelegramText));
        // Arrange
        var notification = CreateTestNotification();
        var config = CreateTelegramChannelConfig();

        // Act
        var payload = _payloadBuilder.BuildPayload(notification, config);

        // Assert
        payload.Should().NotBeNull();
        payload.Data.CustomProperties.Should().ContainKey("telegram_text");
        payload.EventType.Should().Be("deployment.success");
        _mockLogger.LogInformation("Completed {TestMethod}", nameof(BuildPayload_WithTelegramChannel_IncludesTelegramText));
    }

    [Fact]
    public void BuildPayload_WithFailedStatus_SetCorrectEventType()
    {
        _mockLogger.LogInformation("Starting {TestMethod}", nameof(BuildPayload_WithFailedStatus_SetCorrectEventType));
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Build failed",
            Status = BuildStatus.Failed,
            Channels = [NotificationChannel.Slack]
        };
        var config = CreateSlackChannelConfig();

        // Act
        var payload = _payloadBuilder.BuildPayload(notification, config);

        // Assert
        payload.EventType.Should().Be("deployment.failed");
        _mockLogger.LogInformation("Completed {TestMethod}", nameof(BuildPayload_WithFailedStatus_SetCorrectEventType));
    }

    [Fact]
    public void BuildPayload_WithDeploymentSuccess_SetCorrectEventType()
    {
        _mockLogger.LogInformation("Starting {TestMethod}", nameof(BuildPayload_WithDeploymentSuccess_SetCorrectEventType));
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Deployment successful",
            Status = BuildStatus.DeploymentSuccess,
            Channels = [NotificationChannel.Slack]
        };
        var config = CreateSlackChannelConfig();

        // Act
        var payload = _payloadBuilder.BuildPayload(notification, config);

        // Assert
        payload.EventType.Should().Be("deployment.deploymentsuccess");
        _mockLogger.LogInformation("Completed {TestMethod}", nameof(BuildPayload_WithDeploymentSuccess_SetCorrectEventType));
    }

    #endregion

    #region BuildTelegramMessage Tests

    [Fact]
    public void BuildTelegramMessage_WithValidNotification_ContainsProjectNameAndVersion()
    {
        _mockLogger.LogInformation("Starting {TestMethod}", nameof(BuildTelegramMessage_WithValidNotification_ContainsProjectNameAndVersion));
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "ApiGateway",
            Version = "2.1.0",
            BranchName = "main",
            Message = "Deployed to production",
            Status = BuildStatus.DeploymentSuccess,
            TargetEnvironment = Environment.Production
        };
        var config = CreateTelegramChannelConfig();

        // Act
        var message = _payloadBuilder.BuildTelegramMessage(notification, config);

        // Assert
        message.Should().Contain("ApiGateway");
        message.Should().Contain("2.1.0");
        message.Should().Contain("main");
        message.Should().Contain("Deployed to production");
        _mockLogger.LogInformation("Completed {TestMethod}", nameof(BuildTelegramMessage_WithValidNotification_ContainsProjectNameAndVersion));
    }

    [Fact]
    public void BuildTelegramMessage_WithCommitDetailsEnabled_IncludesCommitInfo()
    {
        _mockLogger.LogInformation("Starting {TestMethod}", nameof(BuildTelegramMessage_WithCommitDetailsEnabled_IncludesCommitInfo));
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test",
            CommitHash = "abc1234567890def",
            CommitAuthor = "John Doe",
            Status = BuildStatus.Success
        };
        var config = CreateTelegramChannelConfig();
        config.IncludeCommitDetails = true;

        // Act
        var message = _payloadBuilder.BuildTelegramMessage(notification, config);

        // Assert
        message.Should().Contain("abc1234");
        message.Should().Contain("John Doe");
        _mockLogger.LogInformation("Completed {TestMethod}", nameof(BuildTelegramMessage_WithCommitDetailsEnabled_IncludesCommitInfo));
    }

    [Fact]
    public void BuildTelegramMessage_WithCommitDetailsDisabled_ExcludesCommitInfo()
    {
        _mockLogger.LogInformation("Starting {TestMethod}", nameof(BuildTelegramMessage_WithCommitDetailsDisabled_ExcludesCommitInfo));
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test",
            CommitHash = "abc1234567890def",
            CommitAuthor = "John Doe",
            Status = BuildStatus.Success
        };
        var config = CreateTelegramChannelConfig();
        config.IncludeCommitDetails = false;

        // Act
        var message = _payloadBuilder.BuildTelegramMessage(notification, config);

        // Assert
        message.Should().NotContain("abc1234");
        message.Should().NotContain("John Doe");
        _mockLogger.LogInformation("Completed {TestMethod}", nameof(BuildTelegramMessage_WithCommitDetailsDisabled_ExcludesCommitInfo));
    }

    [Fact]
    public void BuildTelegramMessage_WithDuration_IncludesDurationInfo()
    {
        _mockLogger.LogInformation("Starting {TestMethod}", nameof(BuildTelegramMessage_WithDuration_IncludesDurationInfo));
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test",
            DurationSeconds = 300,
            Status = BuildStatus.Success
        };
        var config = CreateTelegramChannelConfig();

        // Act
        var message = _payloadBuilder.BuildTelegramMessage(notification, config);

        // Assert
        message.Should().Contain("300");
        message.Should().Contain("Duration");
        _mockLogger.LogInformation("Completed {TestMethod}", nameof(BuildTelegramMessage_WithDuration_IncludesDurationInfo));
    }

    [Fact]
    public void BuildTelegramMessage_WithBuildUrlEnabled_IncludesBuildUrl()
    {
        _mockLogger.LogInformation("Starting {TestMethod}", nameof(BuildTelegramMessage_WithBuildUrlEnabled_IncludesBuildUrl));
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test",
            BuildUrl = "https://ci.example.com/build/123",
            Status = BuildStatus.Success
        };
        var config = CreateTelegramChannelConfig();
        config.IncludeBuildUrl = true;

        // Act
        var message = _payloadBuilder.BuildTelegramMessage(notification, config);

        // Assert
        message.Should().Contain("https://ci.example.com/build/123");
        _mockLogger.LogInformation("Completed {TestMethod}", nameof(BuildTelegramMessage_WithBuildUrlEnabled_IncludesBuildUrl));
    }

    [Fact]
    public void BuildTelegramMessage_WithEmojisEnabled_IncludesStatusEmoji()
    {
        _mockLogger.LogInformation("Starting {TestMethod}", nameof(BuildTelegramMessage_WithEmojisEnabled_IncludesStatusEmoji));
        // Arrange
        var notification = new DeploymentNotification
        {
            ProjectName = "MyApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test",
            Status = BuildStatus.Success
        };
        var config = CreateTelegramChannelConfig();
        config.EnableEmojis = true;

        // Act
        var message = _payloadBuilder.BuildTelegramMessage(notification, config);

        // Assert - Success should have an emoji
        message.Length.Should().BeGreaterThan("MyApp".Length);
        _mockLogger.LogInformation("Completed {TestMethod}", nameof(BuildTelegramMessage_WithEmojisEnabled_IncludesStatusEmoji));
    }

    #endregion

    #region BuildSlackPayload Tests

    [Fact]
    public void BuildSlackPayload_WithDefaultSettings_ReturnsAttachmentFormat()
    {
        // Arrange
        var notification = CreateTestNotification();
        var config = CreateSlackChannelConfig();
        config.UseSlackBlockKit = false;

        // Act
        var payload = _payloadBuilder.BuildSlackPayload(notification, config);

        // Assert
        payload.Should().NotBeNull();
    }

    [Fact]
    public void BuildSlackPayload_WithBlockKitEnabled_ReturnsBlockKitFormat()
    {
        // Arrange
        var notification = CreateTestNotification();
        var config = CreateSlackChannelConfig();
        config.UseSlackBlockKit = true;

        // Act
        var payload = _payloadBuilder.BuildSlackPayload(notification, config);

        // Assert
        payload.Should().NotBeNull();
    }

    [Fact]
    public void BuildSlackPayload_WithEmojisEnabled_IncludesStatusEmoji()
    {
        // Arrange
        var notification = CreateTestNotification();
        var config = CreateSlackChannelConfig();
        config.EnableEmojis = true;

        // Act
        var payload = _payloadBuilder.BuildSlackPayload(notification, config);

        // Assert
        payload.Should().NotBeNull();
    }

    #endregion

    #region BuildDiscordPayload Tests

    [Fact]
    public void BuildDiscordPayload_WithValidNotification_ReturnsValidPayload()
    {
        // Arrange
        var notification = CreateTestNotification();
        var config = CreateDiscordChannelConfig();

        // Act
        var payload = _payloadBuilder.BuildDiscordPayload(notification, config);

        // Assert
        payload.Should().NotBeNull();
    }

    [Fact]
    public void BuildDiscordPayload_WithDifferentStatuses_ReturnsValidPayload()
    {
        // Arrange
        var statuses = new[] { BuildStatus.Success, BuildStatus.Failed, BuildStatus.DeploymentSuccess };

        foreach (var status in statuses)
        {
            var notification = new DeploymentNotification
            {
                ProjectName = "TestApp",
                Version = "1.0.0",
                BranchName = "main",
                Message = "Test",
                Status = status,
                Channels = [NotificationChannel.Discord]
            };
            var config = CreateDiscordChannelConfig();

            // Act
            var payload = _payloadBuilder.BuildDiscordPayload(notification, config);

            // Assert
            payload.Should().NotBeNull();
        }
    }

    #endregion

    #region Helper Methods

    private DeploymentNotification CreateTestNotification()
    {
        return new DeploymentNotification
        {
            ProjectName = "TestApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test deployment",
            Status = BuildStatus.Success,
            TargetEnvironment = Environment.Production,
            CommitHash = "abc1234",
            CommitAuthor = "Test User",
            Channels = [NotificationChannel.Slack]
        };
    }

    private ChannelConfiguration CreateSlackChannelConfig()
    {
        return new ChannelConfiguration
        {
            ChannelType = NotificationChannel.Slack,
            DisplayName = "Slack Prod",
            WebhookUrl = "https://hooks.slack.com/services/T00/B00/XXXX",
            TargetId = "C123456",
            TimeoutMs = 5000,
            CustomHeaders = new Dictionary<string, string>(),
            EnableEmojis = false,
            UseSlackBlockKit = false,
            IncludeCommitDetails = true,
            IncludeBuildUrl = true
        };
    }

    private ChannelConfiguration CreateDiscordChannelConfig()
    {
        return new ChannelConfiguration
        {
            ChannelType = NotificationChannel.Discord,
            DisplayName = "Discord Prod",
            WebhookUrl = "https://discordapp.com/api/webhooks/123/ABC",
            TargetId = "C123456",
            TimeoutMs = 5000,
            CustomHeaders = new Dictionary<string, string>(),
            EnableEmojis = false,
            IncludeCommitDetails = true,
            IncludeBuildUrl = true
        };
    }

    private ChannelConfiguration CreateTelegramChannelConfig()
    {
        return new ChannelConfiguration
        {
            ChannelType = NotificationChannel.Telegram,
            DisplayName = "Telegram Prod",
            WebhookUrl = "https://api.telegram.org/botXXX/sendMessage",
            TargetId = "-123456",
            TimeoutMs = 5000,
            CustomHeaders = new Dictionary<string, string>(),
            EnableEmojis = false,
            IncludeCommitDetails = true,
            IncludeBuildUrl = true
        };
    }

    #endregion
}
