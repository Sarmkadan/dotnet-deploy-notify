#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Extension methods for <see cref="ChannelConfiguration"/> that provide common operations
/// and validations for notification channel configurations.
/// </summary>
public static class ChannelConfigurationExtensions
{
    /// <summary>
    /// Creates a deep copy of this <see cref="ChannelConfiguration"/> instance.
    /// </summary>
    /// <param name="configuration">The channel configuration to copy.</param>
    /// <returns>A new instance with all properties copied.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    public static ChannelConfiguration DeepCopy(this ChannelConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return new ChannelConfiguration
        {
            Id = configuration.Id,
            ChannelType = configuration.ChannelType,
            WebhookUrl = configuration.WebhookUrl,
            ApiToken = configuration.ApiToken,
            TargetId = configuration.TargetId,
            DisplayName = configuration.DisplayName,
            IsEnabled = configuration.IsEnabled,
            IncludeCommitDetails = configuration.IncludeCommitDetails,
            IncludeBuildUrl = configuration.IncludeBuildUrl,
            MinimumPriority = configuration.MinimumPriority,
            AllowedEnvironments = new List<Environment>(configuration.AllowedEnvironments),
            AllowedStatuses = new List<BuildStatus>(configuration.AllowedStatuses),
            MaxRetries = configuration.MaxRetries,
            TimeoutMs = configuration.TimeoutMs,
            CustomHeaders = new Dictionary<string, string>(configuration.CustomHeaders),
            Settings = new Dictionary<string, string>(configuration.Settings),
            UseSlackBlockKit = configuration.UseSlackBlockKit,
            EnableEmojis = configuration.EnableEmojis,
            CreatedAt = configuration.CreatedAt,
            UpdatedAt = configuration.UpdatedAt
        };
    }

    /// <summary>
    /// Determines if this channel configuration is configured for the specified environment.
    /// </summary>
    /// <param name="configuration">The channel configuration.</param>
    /// <param name="environment">The environment to check.</param>
    /// <returns>True if the environment is allowed or if no restrictions exist; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> or <paramref name="environment"/> is null.</exception>
    public static bool IsEnvironmentAllowed(this ChannelConfiguration configuration, Environment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        return !configuration.AllowedEnvironments.Any() ||
               configuration.AllowedEnvironments.Contains(environment);
    }

    /// <summary>
    /// Determines if this channel configuration is configured for the specified build status.
    /// </summary>
    /// <param name="configuration">The channel configuration.</param>
    /// <param name="status">The build status to check.</param>
    /// <returns>True if the status is allowed or if no restrictions exist; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> or <paramref name="status"/> is null.</exception>
    public static bool IsStatusAllowed(this ChannelConfiguration configuration, BuildStatus status)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(status);

        return !configuration.AllowedStatuses.Any() ||
               configuration.AllowedStatuses.Contains(status);
    }

    /// <summary>
    /// Gets the effective timeout value for this channel configuration, ensuring it's within valid bounds.
    /// </summary>
    /// <param name="configuration">The channel configuration.</param>
    /// <param name="defaultTimeoutMs">The default timeout to use if configuration timeout is invalid.</param>
    /// <returns>The effective timeout in milliseconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    public static int GetEffectiveTimeout(this ChannelConfiguration configuration, int defaultTimeoutMs = 10000)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return Math.Max(1000, Math.Min(configuration.TimeoutMs, 60000));
    }

    /// <summary>
    /// Gets the effective retry count for this channel configuration, ensuring it's within valid bounds.
    /// </summary>
    /// <param name="configuration">The channel configuration.</param>
    /// <param name="defaultMaxRetries">The default retry count to use if configuration value is invalid.</param>
    /// <returns>The effective retry count.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    public static int GetEffectiveRetryCount(this ChannelConfiguration configuration, int defaultMaxRetries = 3)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return Math.Max(0, Math.Min(configuration.MaxRetries, 10));
    }

    /// <summary>
    /// Gets the priority threshold as a string representation for display purposes.
    /// </summary>
    /// <param name="configuration">The channel configuration.</param>
    /// <returns>Human-readable priority threshold.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    public static string GetPriorityThresholdDisplay(this ChannelConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.MinimumPriority switch
        {
            NotificationPriority.Critical => "Critical",
            NotificationPriority.High => "High",
            NotificationPriority.Normal => "Normal",
            NotificationPriority.Low => "Low",
            _ => configuration.MinimumPriority.ToString()
        };
    }

    /// <summary>
    /// Gets the channel type as a string representation for display purposes.
    /// </summary>
    /// <param name="configuration">The channel configuration.</param>
    /// <returns>Human-readable channel type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    public static string GetChannelTypeDisplay(this ChannelConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.ChannelType switch
        {
            NotificationChannel.Telegram => "Telegram",
            NotificationChannel.Slack => "Slack",
            NotificationChannel.Discord => "Discord",
            NotificationChannel.Webhook => "Webhook",
            NotificationChannel.Email => "Email",
            _ => configuration.ChannelType.ToString()
        };
    }

    /// <summary>
    /// Determines if this channel should include commit details based on configuration.
    /// </summary>
    /// <param name="configuration">The channel configuration.</param>
    /// <returns>True if commit details should be included; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    public static bool ShouldIncludeCommitDetails(this ChannelConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.IncludeCommitDetails && configuration.IsEnabled;
    }

    /// <summary>
    /// Determines if this channel should include build URL based on configuration.
    /// </summary>
    /// <param name="configuration">The channel configuration.</param>
    /// <returns>True if build URL should be included; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    public static bool ShouldIncludeBuildUrl(this ChannelConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.IncludeBuildUrl && configuration.IsEnabled;
    }

    /// <summary>
    /// Gets all custom headers as a read-only dictionary for safe access.
    /// </summary>
    /// <param name="configuration">The channel configuration.</param>
    /// <returns>Read-only dictionary of custom headers.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    public static IReadOnlyDictionary<string, string> GetCustomHeaders(this ChannelConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.CustomHeaders.AsReadOnly();
    }

    /// <summary>
    /// Gets all settings as a read-only dictionary for safe access.
    /// </summary>
    /// <param name="configuration">The channel configuration.</param>
    /// <returns>Read-only dictionary of settings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    public static IReadOnlyDictionary<string, string> GetSettings(this ChannelConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.Settings.AsReadOnly();
    }

    /// <summary>
    /// Gets a setting value with a fallback to a default value if not found.
    /// </summary>
    /// <param name="configuration">The channel configuration.</param>
    /// <param name="key">The setting key.</param>
    /// <param name="defaultValue">The default value to return if key is not found.</param>
    /// <returns>The setting value or default.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
    public static string GetSettingOrDefault(this ChannelConfiguration configuration, string key, string defaultValue = "")
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return configuration.Settings.TryGetValue(key, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Determines if this channel configuration uses Slack Block Kit format.
    /// </summary>
    /// <param name="configuration">The channel configuration.</param>
    /// <returns>True if using Slack Block Kit; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    public static bool UsesSlackBlockKit(this ChannelConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.UseSlackBlockKit && configuration.ChannelType == NotificationChannel.Slack;
    }

    /// <summary>
    /// Determines if emojis are enabled for this channel configuration.
    /// </summary>
    /// <param name="configuration">The channel configuration.</param>
    /// <returns>True if emojis are enabled; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    public static bool EmojisEnabled(this ChannelConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.EnableEmojis && configuration.IsEnabled;
    }

    /// <summary>
    /// Gets a masked version of the configuration for logging purposes.
    /// </summary>
    /// <param name="configuration">The channel configuration.</param>
    /// <returns>A masked copy with sensitive data obscured.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    public static ChannelConfiguration GetMaskedForLogging(this ChannelConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var masked = configuration.GetMasked();
        masked.CustomHeaders = new Dictionary<string, string>();
        masked.Settings = new Dictionary<string, string>();
        return masked;
    }
}