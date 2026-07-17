#nullable enable

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Extension methods for <see cref="NotificationTests"/> that provide additional test utilities
/// for working with Notification-related types in test scenarios.
/// </summary>
public static class NotificationTestsExtensions
{
    /// <summary>
    /// Creates a new <see cref="NotificationBuilder"/> instance.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    /// <returns>A new <see cref="NotificationBuilder"/> instance.</returns>
    public static NotificationBuilder CreateBuilder(this NotificationTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        return new NotificationBuilder();
    }

    /// <summary>
    /// Creates a valid <see cref="DeploymentNotification"/> for testing purposes.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="projectName">The project name.</param>
    /// <param name="version">The version string.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="projectName"/> or <paramref name="version"/> is <see langword="null"/> or whitespace.</exception>
    /// <returns>A new <see cref="DeploymentNotification"/> instance.</returns>
    public static DeploymentNotification CreateValidNotification(this NotificationTests tests, string projectName = "TestProject", string version = "1.0.0")
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectName);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        return new NotificationBuilder()
            .WithProject(projectName, version)
            .WithBranch("main")
            .WithChannels(NotificationChannel.Slack)
            .Build();
    }

    /// <summary>
    /// Creates a <see cref="ChannelConfiguration"/> instance for testing purposes.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="isEnabled">Whether the channel is enabled.</param>
    /// <param name="minPriority">The minimum priority for the channel.</param>
    /// <param name="channelType">The notification channel type.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    /// <returns>A new <see cref="ChannelConfiguration"/> instance.</returns>
    public static ChannelConfiguration CreateChannelConfig(
        this NotificationTests tests,
        bool isEnabled = true,
        NotificationPriority minPriority = NotificationPriority.Low,
        NotificationChannel channelType = NotificationChannel.Slack)
    {
        ArgumentNullException.ThrowIfNull(tests);

        return new ChannelConfiguration
        {
            IsEnabled = isEnabled,
            MinimumPriority = minPriority,
            DisplayName = "TestChannel",
            ChannelType = channelType,
            WebhookUrl = "https://example.com/webhook",
            TargetId = "test-channel-id",
            ApiToken = "test-token",
            TimeoutMs = 5000,
            MaxRetries = 2
        };
    }
}
