#nullable enable

using System.Globalization;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Extension methods for PayloadBuilderTests providing additional test utilities
/// </summary>
public static class PayloadBuilderTestsExtensions
{
    /// <summary>
    /// Creates a test deployment notification with default values
    /// </summary>
    public static DeploymentNotification CreateTestNotification(this PayloadBuilderTests _)
    {
        return new DeploymentNotification
        {
            ProjectName = "TestApp",
            Version = "1.0.0",
            BranchName = "main",
            Message = "Test deployment",
            Status = BuildStatus.Success,
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Production,
            CommitHash = "abc1234",
            CommitAuthor = "Test User",
            Channels = [NotificationChannel.Slack],
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a Slack channel configuration with default settings
    /// </summary>
    public static ChannelConfiguration CreateSlackChannelConfig(this PayloadBuilderTests _)
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

    /// <summary>
    /// Creates a Discord channel configuration with default settings
    /// </summary>
    public static ChannelConfiguration CreateDiscordChannelConfig(this PayloadBuilderTests _)
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

    /// <summary>
    /// Creates a Telegram channel configuration with default settings
    /// </summary>
    public static ChannelConfiguration CreateTelegramChannelConfig(this PayloadBuilderTests _)
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

    /// <summary>
    /// Verifies that a webhook payload contains expected custom properties
    /// </summary>
    public static void ShouldContainCustomProperty(this WebhookPayload payload, string key, object expectedValue)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentException.ThrowIfNullOrEmpty(key);

        payload.Data.Should().NotBeNull();
        payload.Data.CustomProperties.Should().ContainKey(key);
        payload.Data.CustomProperties[key].Should().BeEquivalentTo(expectedValue);
    }

    /// <summary>
    /// Verifies that a webhook payload has the expected event type
    /// </summary>
    public static void ShouldHaveEventType(this WebhookPayload payload, string expectedEventType)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentException.ThrowIfNullOrEmpty(expectedEventType);

        payload.EventType.Should().Be(expectedEventType);
    }

    /// <summary>
    /// Creates a deployment notification with a specific status
    /// </summary>
    public static DeploymentNotification WithStatus(this DeploymentNotification notification, BuildStatus status)
    {
        ArgumentNullException.ThrowIfNull(notification);
        return new DeploymentNotification
        {
            ProjectName = notification.ProjectName,
            Version = notification.Version,
            BranchName = notification.BranchName,
            Message = notification.Message,
            Status = status,
            TargetEnvironment = notification.TargetEnvironment,
            CommitHash = notification.CommitHash,
            CommitAuthor = notification.CommitAuthor,
            Channels = notification.Channels,
            CreatedAt = notification.CreatedAt,
            DurationSeconds = notification.DurationSeconds,
            BuildUrl = notification.BuildUrl,
            Priority = notification.Priority,
            Metadata = notification.Metadata,
            RepositoryUrl = notification.RepositoryUrl
        };
    }

    /// <summary>
    /// Creates a deployment notification with a specific environment
    /// </summary>
    public static DeploymentNotification WithEnvironment(this DeploymentNotification notification, global::DotNetDeployNotify.Core.Environment environment)
    {
        ArgumentNullException.ThrowIfNull(notification);
        return new DeploymentNotification
        {
            ProjectName = notification.ProjectName,
            Version = notification.Version,
            BranchName = notification.BranchName,
            Message = notification.Message,
            Status = notification.Status,
            TargetEnvironment = environment,
            CommitHash = notification.CommitHash,
            CommitAuthor = notification.CommitAuthor,
            Channels = notification.Channels,
            CreatedAt = notification.CreatedAt,
            DurationSeconds = notification.DurationSeconds,
            BuildUrl = notification.BuildUrl,
            Priority = notification.Priority,
            Metadata = notification.Metadata,
            RepositoryUrl = notification.RepositoryUrl
        };
    }

    /// <summary>
    /// Creates a channel configuration with emojis enabled
    /// </summary>
    public static ChannelConfiguration WithEmojisEnabled(this ChannelConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        return new ChannelConfiguration
        {
            ChannelType = config.ChannelType,
            DisplayName = config.DisplayName,
            WebhookUrl = config.WebhookUrl,
            TargetId = config.TargetId,
            TimeoutMs = config.TimeoutMs,
            CustomHeaders = new Dictionary<string, string>(config.CustomHeaders),
            EnableEmojis = true,
            UseSlackBlockKit = config.UseSlackBlockKit,
            IncludeCommitDetails = config.IncludeCommitDetails,
            IncludeBuildUrl = config.IncludeBuildUrl
        };
    }

    /// <summary>
    /// Creates a channel configuration with emojis disabled
    /// </summary>
    public static ChannelConfiguration WithEmojisDisabled(this ChannelConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        return new ChannelConfiguration
        {
            ChannelType = config.ChannelType,
            DisplayName = config.DisplayName,
            WebhookUrl = config.WebhookUrl,
            TargetId = config.TargetId,
            TimeoutMs = config.TimeoutMs,
            CustomHeaders = new Dictionary<string, string>(config.CustomHeaders),
            EnableEmojis = false,
            UseSlackBlockKit = config.UseSlackBlockKit,
            IncludeCommitDetails = config.IncludeCommitDetails,
            IncludeBuildUrl = config.IncludeBuildUrl
        };
    }

    /// <summary>
    /// Creates a channel configuration with commit details included
    /// </summary>
    public static ChannelConfiguration WithCommitDetails(this ChannelConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        return new ChannelConfiguration
        {
            ChannelType = config.ChannelType,
            DisplayName = config.DisplayName,
            WebhookUrl = config.WebhookUrl,
            TargetId = config.TargetId,
            TimeoutMs = config.TimeoutMs,
            CustomHeaders = new Dictionary<string, string>(config.CustomHeaders),
            EnableEmojis = config.EnableEmojis,
            UseSlackBlockKit = config.UseSlackBlockKit,
            IncludeCommitDetails = true,
            IncludeBuildUrl = config.IncludeBuildUrl
        };
    }

    /// <summary>
    /// Creates a channel configuration without commit details
    /// </summary>
    public static ChannelConfiguration WithoutCommitDetails(this ChannelConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        return new ChannelConfiguration
        {
            ChannelType = config.ChannelType,
            DisplayName = config.DisplayName,
            WebhookUrl = config.WebhookUrl,
            TargetId = config.TargetId,
            TimeoutMs = config.TimeoutMs,
            CustomHeaders = new Dictionary<string, string>(config.CustomHeaders),
            EnableEmojis = config.EnableEmojis,
            UseSlackBlockKit = config.UseSlackBlockKit,
            IncludeCommitDetails = false,
            IncludeBuildUrl = config.IncludeBuildUrl
        };
    }

    /// <summary>
    /// Creates a channel configuration with build URL included
    /// </summary>
    public static ChannelConfiguration WithBuildUrl(this ChannelConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        return new ChannelConfiguration
        {
            ChannelType = config.ChannelType,
            DisplayName = config.DisplayName,
            WebhookUrl = config.WebhookUrl,
            TargetId = config.TargetId,
            TimeoutMs = config.TimeoutMs,
            CustomHeaders = new Dictionary<string, string>(config.CustomHeaders),
            EnableEmojis = config.EnableEmojis,
            UseSlackBlockKit = config.UseSlackBlockKit,
            IncludeCommitDetails = config.IncludeCommitDetails,
            IncludeBuildUrl = true
        };
    }

    /// <summary>
    /// Creates a channel configuration without build URL
    /// </summary>
    public static ChannelConfiguration WithoutBuildUrl(this ChannelConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        return new ChannelConfiguration
        {
            ChannelType = config.ChannelType,
            DisplayName = config.DisplayName,
            WebhookUrl = config.WebhookUrl,
            TargetId = config.TargetId,
            TimeoutMs = config.TimeoutMs,
            CustomHeaders = new Dictionary<string, string>(config.CustomHeaders),
            EnableEmojis = config.EnableEmojis,
            UseSlackBlockKit = config.UseSlackBlockKit,
            IncludeCommitDetails = config.IncludeCommitDetails,
            IncludeBuildUrl = false
        };
    }

    /// <summary>
    /// Creates a channel configuration with Slack Block Kit enabled
    /// </summary>
    public static ChannelConfiguration WithSlackBlockKit(this ChannelConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        return new ChannelConfiguration
        {
            ChannelType = config.ChannelType,
            DisplayName = config.DisplayName,
            WebhookUrl = config.WebhookUrl,
            TargetId = config.TargetId,
            TimeoutMs = config.TimeoutMs,
            CustomHeaders = new Dictionary<string, string>(config.CustomHeaders),
            EnableEmojis = config.EnableEmojis,
            UseSlackBlockKit = true,
            IncludeCommitDetails = config.IncludeCommitDetails,
            IncludeBuildUrl = config.IncludeBuildUrl
        };
    }

    /// <summary>
    /// Creates a channel configuration with Slack Block Kit disabled
    /// </summary>
    public static ChannelConfiguration WithoutSlackBlockKit(this ChannelConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        return new ChannelConfiguration
        {
            ChannelType = config.ChannelType,
            DisplayName = config.DisplayName,
            WebhookUrl = config.WebhookUrl,
            TargetId = config.TargetId,
            TimeoutMs = config.TimeoutMs,
            CustomHeaders = new Dictionary<string, string>(config.CustomHeaders),
            EnableEmojis = config.EnableEmojis,
            UseSlackBlockKit = false,
            IncludeCommitDetails = config.IncludeCommitDetails,
            IncludeBuildUrl = config.IncludeBuildUrl
        };
    }

    /// <summary>
    /// Asserts that a Telegram message contains the project name and version
    /// </summary>
    public static void ShouldContainProjectAndVersion(this string message, string projectName, string version)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentException.ThrowIfNullOrEmpty(projectName);
        ArgumentException.ThrowIfNullOrEmpty(version);

        message.Should().Contain(projectName);
        message.Should().Contain(version);
    }

    /// <summary>
    /// Asserts that a Telegram message contains commit information
    /// </summary>
    public static void ShouldContainCommitInfo(this string message, string commitHash, string commitAuthor)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentException.ThrowIfNullOrEmpty(commitHash);
        ArgumentException.ThrowIfNullOrEmpty(commitAuthor);

        var shortHash = commitHash[..Math.Min(7, commitHash.Length)];
        message.Should().Contain(shortHash);
        message.Should().Contain(commitAuthor);
    }

    /// <summary>
    /// Asserts that a Telegram message contains duration information
    /// </summary>
    public static void ShouldContainDuration(this string message, int durationSeconds)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);

        message.Should().Contain(durationSeconds.ToString(CultureInfo.InvariantCulture));
        message.Should().Contain("Duration");
    }

    /// <summary>
    /// Asserts that a Telegram message contains the build URL
    /// </summary>
    public static void ShouldContainBuildUrl(this string message, string buildUrl)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentException.ThrowIfNullOrEmpty(buildUrl);

        message.Should().Contain(buildUrl);
    }

    /// <summary>
    /// Creates a deployment notification with duration
    /// </summary>
    public static DeploymentNotification WithDuration(this DeploymentNotification notification, int durationSeconds)
    {
        ArgumentNullException.ThrowIfNull(notification);
        return new DeploymentNotification
        {
            ProjectName = notification.ProjectName,
            Version = notification.Version,
            BranchName = notification.BranchName,
            Message = notification.Message,
            Status = notification.Status,
            TargetEnvironment = notification.TargetEnvironment,
            CommitHash = notification.CommitHash,
            CommitAuthor = notification.CommitAuthor,
            Channels = notification.Channels,
            CreatedAt = notification.CreatedAt,
            DurationSeconds = durationSeconds,
            BuildUrl = notification.BuildUrl,
            Priority = notification.Priority,
            Metadata = notification.Metadata,
            RepositoryUrl = notification.RepositoryUrl
        };
    }

    /// <summary>
    /// Creates a deployment notification with build URL
    /// </summary>
    public static DeploymentNotification WithBuildUrl(this DeploymentNotification notification, string buildUrl)
    {
        ArgumentNullException.ThrowIfNull(notification);
        return new DeploymentNotification
        {
            ProjectName = notification.ProjectName,
            Version = notification.Version,
            BranchName = notification.BranchName,
            Message = notification.Message,
            Status = notification.Status,
            TargetEnvironment = notification.TargetEnvironment,
            CommitHash = notification.CommitHash,
            CommitAuthor = notification.CommitAuthor,
            Channels = notification.Channels,
            CreatedAt = notification.CreatedAt,
            DurationSeconds = notification.DurationSeconds,
            BuildUrl = buildUrl,
            Priority = notification.Priority,
            Metadata = notification.Metadata,
            RepositoryUrl = notification.RepositoryUrl
        };
    }

    /// <summary>
    /// Creates a deployment notification with commit details
    /// </summary>
    public static DeploymentNotification WithCommitDetails(this DeploymentNotification notification, string commitHash, string commitAuthor)
    {
        ArgumentNullException.ThrowIfNull(notification);
        return new DeploymentNotification
        {
            ProjectName = notification.ProjectName,
            Version = notification.Version,
            BranchName = notification.BranchName,
            Message = notification.Message,
            Status = notification.Status,
            TargetEnvironment = notification.TargetEnvironment,
            CommitHash = commitHash,
            CommitAuthor = commitAuthor,
            Channels = notification.Channels,
            CreatedAt = notification.CreatedAt,
            DurationSeconds = notification.DurationSeconds,
            BuildUrl = notification.BuildUrl,
            Priority = notification.Priority,
            Metadata = notification.Metadata,
            RepositoryUrl = notification.RepositoryUrl
        };
    }
}
