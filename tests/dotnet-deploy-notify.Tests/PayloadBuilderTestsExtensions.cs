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
/// Extension methods for <see cref="PayloadBuilderTests"/> providing additional test utilities.
/// All members are static for extension methods.
/// </summary>
public static class PayloadBuilderTestsExtensions
{
    /// <summary>
    /// Creates a test deployment notification with default values.
    /// </summary>
    /// <param name="_">The instance parameter for extension method (unused).</param>
    /// <returns>A new <see cref="DeploymentNotification"/> with test values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="_"/> is <see langword="null"/></exception>
    public static DeploymentNotification CreateTestNotification(this PayloadBuilderTests _) =>
        new DeploymentNotification
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

    /// <summary>
    /// Creates a Slack channel configuration with default settings
    /// </summary>
    /// <param name="_">The instance parameter for extension method (unused).</param>
    /// <returns>A new <see cref="ChannelConfiguration"/> configured for Slack.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="_"/> is <see langword="null"/></exception>
    public static ChannelConfiguration CreateSlackChannelConfig(this PayloadBuilderTests _) =>
        new ChannelConfiguration
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

    /// <summary>
    /// Creates a Discord channel configuration with default settings
    /// </summary>
    /// <param name="_">The instance parameter for extension method (unused).</param>
    /// <returns>A new <see cref="ChannelConfiguration"/> configured for Discord.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="_"/> is <see langword="null"/></exception>
    public static ChannelConfiguration CreateDiscordChannelConfig(this PayloadBuilderTests _) =>
        new ChannelConfiguration
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

    /// <summary>
    /// Creates a Telegram channel configuration with default settings
    /// </summary>
    /// <param name="_">The instance parameter for extension method (unused).</param>
    /// <returns>A new <see cref="ChannelConfiguration"/> configured for Telegram.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="_"/> is <see langword="null"/></exception>
    public static ChannelConfiguration CreateTelegramChannelConfig(this PayloadBuilderTests _) =>
        new ChannelConfiguration
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

    /// <summary>
    /// Verifies that a webhook payload contains expected custom properties
    /// </summary>
    /// <param name="payload">The payload to verify.</param>
    /// <param name="key">The key of the custom property to check.</param>
    /// <param name="expectedValue">The expected value of the custom property.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payload"/> or <paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is empty.</exception>
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
    /// <param name="payload">The payload to verify.</param>
    /// <param name="expectedEventType">The expected event type.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="payload"/> or <paramref name="expectedEventType"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="expectedEventType"/> is empty.</exception>
    public static void ShouldHaveEventType(this WebhookPayload payload, string expectedEventType)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentException.ThrowIfNullOrEmpty(expectedEventType);

        payload.EventType.Should().Be(expectedEventType);
    }

    /// <summary>
    /// Creates a deployment notification with a specific status
    /// </summary>
    /// <param name="notification">The source notification to copy properties from.</param>
    /// <param name="status">The status to set on the new notification.</param>
    /// <returns>A new <see cref="DeploymentNotification"/> with the specified status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="notification"/> is null.</exception>
    public static DeploymentNotification WithStatus(this DeploymentNotification notification, BuildStatus status) =>
        new DeploymentNotification
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

    /// <summary>
    /// Creates a deployment notification with a specific environment
    /// </summary>
    /// <param name="notification">The source notification to copy properties from.</param>
    /// <param name="environment">The environment to set on the new notification.</param>
    /// <returns>A new <see cref="DeploymentNotification"/> with the specified environment.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="notification"/> is null.</exception>
    public static DeploymentNotification WithEnvironment(this DeploymentNotification notification, global::DotNetDeployNotify.Core.Environment environment) =>
        new DeploymentNotification
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

    /// <summary>
    /// Creates a channel configuration with emojis enabled
    /// </summary>
    /// <param name="config">The source configuration to copy properties from.</param>
    /// <returns>A new <see cref="ChannelConfiguration"/> with emojis enabled.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public static ChannelConfiguration WithEmojisEnabled(this ChannelConfiguration config) =>
        new ChannelConfiguration
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

    /// <summary>
    /// Creates a channel configuration with emojis disabled
    /// </summary>
    /// <param name="config">The source configuration to copy properties from.</param>
    /// <returns>A new <see cref="ChannelConfiguration"/> with emojis disabled.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public static ChannelConfiguration WithEmojisDisabled(this ChannelConfiguration config) =>
        new ChannelConfiguration
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

    /// <summary>
    /// Creates a channel configuration with commit details included
    /// </summary>
    /// <param name="config">The source configuration to copy properties from.</param>
    /// <returns>A new <see cref="ChannelConfiguration"/> with commit details included.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public static ChannelConfiguration WithCommitDetails(this ChannelConfiguration config) =>
        new ChannelConfiguration
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

    /// <summary>
    /// Creates a channel configuration without commit details
    /// </summary>
    /// <param name="config">The source configuration to copy properties from.</param>
    /// <returns>A new <see cref="ChannelConfiguration"/> without commit details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public static ChannelConfiguration WithoutCommitDetails(this ChannelConfiguration config) =>
        new ChannelConfiguration
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

    /// <summary>
    /// Creates a channel configuration with build URL included
    /// </summary>
    /// <param name="config">The source configuration to copy properties from.</param>
    /// <returns>A new <see cref="ChannelConfiguration"/> with build URL included.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public static ChannelConfiguration WithBuildUrl(this ChannelConfiguration config) =>
        new ChannelConfiguration
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

    /// <summary>
    /// Creates a channel configuration without build URL
    /// </summary>
    /// <param name="config">The source configuration to copy properties from.</param>
    /// <returns>A new <see cref="ChannelConfiguration"/> without build URL.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public static ChannelConfiguration WithoutBuildUrl(this ChannelConfiguration config) =>
        new ChannelConfiguration
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

    /// <summary>
    /// Creates a channel configuration with Slack Block Kit enabled
    /// </summary>
    /// <param name="config">The source configuration to copy properties from.</param>
    /// <returns>A new <see cref="ChannelConfiguration"/> with Slack Block Kit enabled.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public static ChannelConfiguration WithSlackBlockKit(this ChannelConfiguration config) =>
        new ChannelConfiguration
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

    /// <summary>
    /// Creates a channel configuration with Slack Block Kit disabled
    /// </summary>
    /// <param name="config">The source configuration to copy properties from.</param>
    /// <returns>A new <see cref="ChannelConfiguration"/> with Slack Block Kit disabled.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is null.</exception>
    public static ChannelConfiguration WithoutSlackBlockKit(this ChannelConfiguration config) =>
        new ChannelConfiguration
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

    /// <summary>
    /// Asserts that a Telegram message contains the project name and version
    /// </summary>
    /// <param name="message">The message to check.</param>
    /// <param name="projectName">The expected project name.</param>
    /// <param name="version">The expected version.</param>
    /// <exception cref="ArgumentException">Thrown when any parameter is null or empty.</exception>
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
    /// <param name="message">The message to check.</param>
    /// <param name="commitHash">The expected commit hash.</param>
    /// <param name="commitAuthor">The expected commit author.</param>
    /// <exception cref="ArgumentException">Thrown when any parameter is null or empty.</exception>
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
    /// <param name="message">The message to check.</param>
    /// <param name="durationSeconds">The expected duration in seconds.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is null or empty.</exception>
    public static void ShouldContainDuration(this string message, int durationSeconds)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);

        message.Should().Contain(durationSeconds.ToString(CultureInfo.InvariantCulture));
        message.Should().Contain("Duration");
    }

    /// <summary>
    /// Asserts that a Telegram message contains the build URL
    /// </summary>
    /// <param name="message">The message to check.</param>
    /// <param name="buildUrl">The expected build URL.</param>
    /// <exception cref="ArgumentException">Thrown when any parameter is null or empty.</exception>
    public static void ShouldContainBuildUrl(this string message, string buildUrl)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentException.ThrowIfNullOrEmpty(buildUrl);

        message.Should().Contain(buildUrl);
    }

    /// <summary>
    /// Creates a deployment notification with duration
    /// </summary>
    /// <param name="notification">The source notification to copy properties from.</param>
    /// <param name="durationSeconds">The duration in seconds to set.</param>
    /// <returns>A new <see cref="DeploymentNotification"/> with the specified duration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="notification"/> is null.</exception>
    public static DeploymentNotification WithDuration(this DeploymentNotification notification, int durationSeconds) =>
        new DeploymentNotification
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
