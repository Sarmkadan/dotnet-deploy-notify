#nullable enable

using DotNetDeployNotify.Core;
using System.Globalization;

namespace DotNetDeployNotify.Configuration;

/// <summary>
/// Extension methods for <see cref="DotnetDeployNotifyOptions"/> configuration.
/// </summary>
public static class DotnetDeployNotifyOptionsExtensions
{
    /// <summary>
    /// Gets the effective webhook timeout for the given environment channel.
    /// Returns the channel-specific timeout if configured, otherwise falls back to the default <see cref="NotificationConfig.WebhookTimeoutMs"/>.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <param name="environmentName">The environment name to look up.</param>
    /// <returns>The timeout in milliseconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static int GetWebhookTimeoutMs(this DotnetDeployNotifyOptions options, string environmentName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        return options.Notification.WebhookTimeoutMs;
    }

    /// <summary>
    /// Gets the effective maximum retry count for the given environment channel.
    /// Returns the channel-specific max retries if configured, otherwise falls back to the default <see cref="NotificationConfig.MaxRetries"/>.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <param name="environmentName">The environment name to look up.</param>
    /// <returns>The maximum retry count.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static int GetMaxRetries(this DotnetDeployNotifyOptions options, string environmentName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        return options.Notification.MaxRetries;
    }

    /// <summary>
    /// Determines whether notifications should be automatically processed for the specified environment.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <param name="environmentName">The environment name to check.</param>
    /// <returns>True if auto-processing is enabled; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static bool IsAutoProcessingEnabled(this DotnetDeployNotifyOptions options, string environmentName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        return options.Notification.AutoProcessNotifications;
    }

    /// <summary>
    /// Gets the notification priority for the specified environment channel.
    /// Returns the channel-specific priority if configured, otherwise falls back to the default <see cref="NotificationConfig.DefaultPriority"/>.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <param name="environmentName">The environment name to look up.</param>
    /// <returns>The notification priority level.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static NotificationPriority GetPriority(this DotnetDeployNotifyOptions options, string environmentName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        if (Enum.TryParse<NotificationPriority>(options.Notification.DefaultPriority, ignoreCase: true, out var priority))
        {
            return priority;
        }

        return NotificationPriority.Normal;
    }

    /// <summary>
    /// Checks if audit logging is enabled for the specified environment.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <param name="environmentName">The environment name to check.</param>
    /// <returns>True if audit logging is enabled; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static bool IsAuditLoggingEnabled(this DotnetDeployNotifyOptions options, string environmentName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        return options.Notification.EnableAuditLogging;
    }

    /// <summary>
    /// Gets the display name for the specified environment channel.
    /// Returns the channel-specific display name if configured, otherwise returns a formatted default.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <param name="environmentName">The environment name to look up.</param>
    /// <returns>The display name for the channel.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static string GetDisplayName(this DotnetDeployNotifyOptions options, string environmentName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        if (options.Notification.EnvironmentChannels.TryGetValue(environmentName, out var channelConfig) &&
            !string.IsNullOrWhiteSpace(channelConfig.DisplayName))
        {
            return channelConfig.DisplayName;
        }

        return $"{environmentName}-{channelConfig?.ChannelType ?? "Slack"}";
    }

    /// <summary>
    /// Gets the effective storage path for the given environment.
    /// Returns the channel-specific storage path if configured, otherwise falls back to the default <see cref="NotificationConfig.StoragePath"/>.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <param name="environmentName">The environment name to look up.</param>
    /// <returns>The storage path, or null if not configured.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static string? GetStoragePath(this DotnetDeployNotifyOptions options, string environmentName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        return options.Notification.StoragePath;
    }

    /// <summary>
    /// Gets the effective log level for the given environment.
    /// Returns the channel-specific log level if configured, otherwise falls back to the default <see cref="NotificationConfig.LogLevel"/>.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <param name="environmentName">The environment name to look up.</param>
    /// <returns>The log level string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static string GetLogLevel(this DotnetDeployNotifyOptions options, string environmentName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        return options.Notification.LogLevel ?? "Information";
    }

    /// <summary>
    /// Checks if the canary deployment feature is enabled.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <returns>True if canary deployments are enabled; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static bool IsCanaryEnabled(this DotnetDeployNotifyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.Canary.Enabled;
    }

    /// <summary>
    /// Gets the effective canary auto-rollback setting.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <returns>True if auto-rollback is enabled; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static bool IsCanaryAutoRollbackEnabled(this DotnetDeployNotifyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.Canary.AutoRollbackOnFailure;
    }

    /// <summary>
    /// Gets the effective canary auto-advance setting.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <returns>True if auto-advance is enabled; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static bool IsCanaryAutoAdvanceEnabled(this DotnetDeployNotifyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.Canary.AutoAdvanceOnSuccess;
    }

    /// <summary>
    /// Gets the effective canary alert priority.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <returns>The notification priority for canary alerts.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static NotificationPriority GetCanaryAlertPriority(this DotnetDeployNotifyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.Canary.AlertPriority;
    }

    /// <summary>
    /// Gets the effective canary maximum deployment duration.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <returns>The maximum deployment duration as a TimeSpan.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static TimeSpan GetCanaryMaxDeploymentDuration(this DotnetDeployNotifyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.Canary.MaxDeploymentDuration;
    }

    /// <summary>
    /// Gets the effective canary step soak duration.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <returns>The step soak duration as a TimeSpan.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static TimeSpan GetCanaryStepSoakDuration(this DotnetDeployNotifyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.Canary.StepSoakDuration;
    }

    /// <summary>
    /// Gets the effective canary linear step count.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <returns>The number of linear steps.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static int GetCanaryLinearStepCount(this DotnetDeployNotifyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.Canary.LinearStepCount;
    }

    /// <summary>
    /// Gets the effective canary thresholds for error rate and latency.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <returns>A tuple containing (MaxErrorRatePercent, MaxP95LatencyMs, MaxP99LatencyMs).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static (double MaxErrorRatePercent, double MaxP95LatencyMs, double MaxP99LatencyMs) GetCanaryThresholds(this DotnetDeployNotifyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return (
            options.Canary.Thresholds.MaxErrorRatePercent,
            options.Canary.Thresholds.MaxP95LatencyMs,
            options.Canary.Thresholds.MaxP99LatencyMs
        );
    }

    /// <summary>
    /// Gets all environment names configured in the notification channels.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <returns>An enumerable of environment names.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static IEnumerable<string> GetConfiguredEnvironments(this DotnetDeployNotifyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.Notification.EnvironmentChannels.Keys;
    }

    /// <summary>
    /// Gets the webhook URL for the specified environment channel.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <param name="environmentName">The environment name to look up.</param>
    /// <returns>The webhook URL, or null if not configured.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static string? GetWebhookUrl(this DotnetDeployNotifyOptions options, string environmentName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        if (options.Notification.EnvironmentChannels.TryGetValue(environmentName, out var channelConfig))
        {
            return channelConfig.WebhookUrl;
        }

        return null;
    }

    /// <summary>
    /// Gets the channel type for the specified environment channel.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <param name="environmentName">The environment name to look up.</param>
    /// <returns>The channel type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static string GetChannelType(this DotnetDeployNotifyOptions options, string environmentName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        if (options.Notification.EnvironmentChannels.TryGetValue(environmentName, out var channelConfig))
        {
            return channelConfig.ChannelType;
        }

        return "Slack";
    }

    /// <summary>
    /// Gets the target ID for the specified environment channel.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <param name="environmentName">The environment name to look up.</param>
    /// <returns>The target ID, or null if not configured.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static string? GetTargetId(this DotnetDeployNotifyOptions options, string environmentName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        if (options.Notification.EnvironmentChannels.TryGetValue(environmentName, out var channelConfig))
        {
            return channelConfig.TargetId;
        }

        return null;
    }

    /// <summary>
    /// Checks if commit details should be included in notifications for the specified environment.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <param name="environmentName">The environment name to check.</param>
    /// <returns>True if commit details should be included; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static bool IncludeCommitDetails(this DotnetDeployNotifyOptions options, string environmentName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        return options.Notification.IncludeCommitDetails;
    }

    /// <summary>
    /// Checks if build URL should be included in notifications for the specified environment.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <param name="environmentName">The environment name to check.</param>
    /// <returns>True if build URL should be included; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static bool IncludeBuildUrl(this DotnetDeployNotifyOptions options, string environmentName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        return options.Notification.IncludeBuildUrl;
    }

    /// <summary>
    /// Gets the effective retention days for the specified environment.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <param name="environmentName">The environment name to check.</param>
    /// <returns>The retention days.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static int GetRetentionDays(this DotnetDeployNotifyOptions options, string environmentName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        return options.Notification.RetentionDays;
    }

    /// <summary>
    /// Gets the effective processing interval in seconds.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <returns>The processing interval in seconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static int GetProcessingIntervalSeconds(this DotnetDeployNotifyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.Notification.ProcessingIntervalSeconds;
    }
}
