#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Configuration;

/// <summary>
/// Extension methods for <see cref="ChannelConfigurationBuilder"/> to provide additional
/// convenience methods for building channel configurations.
/// </summary>
public static class ChannelConfigurationBuilderExtensions
{
    /// <summary>
    /// Sets the API token for authentication with the notification channel.
    /// </summary>
    /// <param name="builder">The channel configuration builder</param>
    /// <param name="apiToken">The API token or authentication credential</param>
    /// <returns>The configured builder for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static ChannelConfigurationBuilder WithApiToken(this ChannelConfigurationBuilder builder, string apiToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(apiToken);

        builder.WithTargetId(apiToken);
        return builder;
    }

    /// <summary>
    /// Adds a custom header to the webhook requests for this channel.
    /// </summary>
    /// <param name="builder">The channel configuration builder</param>
    /// <param name="key">The header name</param>
    /// <param name="value">The header value</param>
    /// <returns>The configured builder for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder or key is null</exception>
    public static ChannelConfigurationBuilder WithCustomHeader(this ChannelConfigurationBuilder builder, string key, string value)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(value);

        if (builder.Build().CustomHeaders is null)
        {
            builder.Build(); // Ensure _config is initialized
        }

        builder.Build().CustomHeaders[key] = value;
        return builder;
    }

    /// <summary>
    /// Sets a custom setting value for the channel configuration.
    /// </summary>
    /// <param name="builder">The channel configuration builder</param>
    /// <param name="key">The setting key</param>
    /// <param name="value">The setting value</param>
    /// <returns>The configured builder for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder, key, or value is null</exception>
    public static ChannelConfigurationBuilder WithSetting(this ChannelConfigurationBuilder builder, string key, string value)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(value);

        if (builder.Build().Settings is null)
        {
            builder.Build(); // Ensure _config is initialized
        }

        builder.Build().Settings[key] = value;
        return builder;
    }

    /// <summary>
    /// Sets the channel to be enabled or disabled.
    /// </summary>
    /// <param name="builder">The channel configuration builder</param>
    /// <param name="isEnabled">Whether the channel should be enabled</param>
    /// <returns>The configured builder for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static ChannelConfigurationBuilder WithIsEnabled(this ChannelConfigurationBuilder builder, bool isEnabled)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Build().IsEnabled = isEnabled;
        return builder;
    }

    /// <summary>
    /// Sets the minimum priority level to send notifications to this channel.
    /// </summary>
    /// <param name="builder">The channel configuration builder</param>
    /// <param name="priority">The minimum priority level</param>
    /// <returns>The configured builder for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static ChannelConfigurationBuilder WithMinimumPriority(this ChannelConfigurationBuilder builder, NotificationPriority priority)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _ = builder.Build(); // Ensure _config is initialized
        builder.Build().MinimumPriority = priority;
        return builder;
    }

    /// <summary>
    /// Allows notifications only for the specified environments.
    /// </summary>
    /// <param name="builder">The channel configuration builder</param>
    /// <param name="environments">The environments to allow</param>
    /// <returns>The configured builder for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static ChannelConfigurationBuilder AllowEnvironments(this ChannelConfigurationBuilder builder, params Environment[] environments)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(environments);

        _ = builder.Build(); // Ensure _config is initialized
        builder.Build().AllowedEnvironments = new List<Environment>(environments);
        return builder;
    }

    /// <summary>
    /// Allows notifications only for the specified build statuses.
    /// </summary>
    /// <param name="builder">The channel configuration builder</param>
    /// <param name="statuses">The build statuses to allow</param>
    /// <returns>The configured builder for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static ChannelConfigurationBuilder AllowStatuses(this ChannelConfigurationBuilder builder, params BuildStatus[] statuses)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(statuses);

        _ = builder.Build(); // Ensure _config is initialized
        builder.Build().AllowedStatuses = new List<BuildStatus>(statuses);
        return builder;
    }

    /// <summary>
    /// Sets the timeout in seconds instead of milliseconds for convenience.
    /// </summary>
    /// <param name="builder">The channel configuration builder</param>
    /// <param name="seconds">Timeout in seconds</param>
    /// <returns>The configured builder for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when seconds is negative</exception>
    public static ChannelConfigurationBuilder WithTimeoutSeconds(this ChannelConfigurationBuilder builder, int seconds)
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (seconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds), "Timeout seconds cannot be negative");
        }

        _ = builder.Build(); // Ensure _config is initialized
        builder.Build().TimeoutMs = seconds * 1000;
        return builder;
    }

    /// <summary>
    /// Sets the timeout in minutes instead of milliseconds for convenience.
    /// </summary>
    /// <param name="builder">The channel configuration builder</param>
    /// <param name="minutes">Timeout in minutes</param>
    /// <returns>The configured builder for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when minutes is negative</exception>
    public static ChannelConfigurationBuilder WithTimeoutMinutes(this ChannelConfigurationBuilder builder, int minutes)
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (minutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minutes), "Timeout minutes cannot be negative");
        }

        _ = builder.Build(); // Ensure _config is initialized
        builder.Build().TimeoutMs = minutes * 60 * 1000;
        return builder;
    }

    /// <summary>
    /// Configures the channel to use Slack Block Kit rich formatting.
    /// </summary>
    /// <param name="builder">The channel configuration builder</param>
    /// <param name="enable">Whether to enable Block Kit formatting</param>
    /// <returns>The configured builder for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static ChannelConfigurationBuilder UseSlackBlockKit(this ChannelConfigurationBuilder builder, bool enable = true)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _ = builder.Build(); // Ensure _config is initialized
        builder.Build().UseSlackBlockKit = enable;
        return builder;
    }

    /// <summary>
    /// Configures the channel to enable or disable emoji status indicators.
    /// </summary>
    /// <param name="builder">The channel configuration builder</param>
    /// <param name="enable">Whether to enable emojis</param>
    /// <returns>The configured builder for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static ChannelConfigurationBuilder EnableEmojis(this ChannelConfigurationBuilder builder, bool enable = true)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _ = builder.Build(); // Ensure _config is initialized
        builder.Build().EnableEmojis = enable;
        return builder;
    }

    /// <summary>
    /// Sets the display name for the channel configuration.
    /// </summary>
    /// <param name="builder">The channel configuration builder</param>
    /// <param name="displayName">The human-readable name</param>
    /// <returns>The configured builder for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder or displayName is null</exception>
    public static ChannelConfigurationBuilder WithDisplayName(this ChannelConfigurationBuilder builder, string displayName)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(displayName);

        _ = builder.Build(); // Ensure _config is initialized
        builder.Build().DisplayName = displayName;
        return builder;
    }

    /// <summary>
    /// Creates a channel configuration for a generic webhook endpoint.
    /// </summary>
    /// <param name="builderAction">Action to configure the webhook channel</param>
    /// <returns>A configured channel configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when builderAction is null</exception>
    public static ChannelConfiguration ForWebhook(Action<ChannelConfigurationBuilder> builderAction)
    {
        ArgumentNullException.ThrowIfNull(builderAction);

        var builder = ChannelConfigurationBuilder.ForSlack(); // Start with Slack builder
        builderAction(builder);
        return builder.Build();
    }

    /// <summary>
    /// Creates a channel configuration for email notifications.
    /// </summary>
    /// <param name="builderAction">Action to configure the email channel</param>
    /// <returns>A configured channel configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when builderAction is null</exception>
    public static ChannelConfiguration ForEmail(Action<ChannelConfigurationBuilder> builderAction)
    {
        ArgumentNullException.ThrowIfNull(builderAction);

        var builder = new ChannelConfigurationBuilder(NotificationChannel.Email);
        builderAction(builder);
        return builder.Build();
    }

    /// <summary>
    /// Validates that the channel configuration is valid.
    /// </summary>
    /// <param name="builder">The channel configuration builder</param>
    /// <returns>True if the configuration is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static bool IsValid(this ChannelConfigurationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Build().IsValid();
    }

    /// <summary>
    /// Gets the current configuration from the builder.
    /// </summary>
    /// <param name="builder">The channel configuration builder</param>
    /// <returns>The built channel configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static ChannelConfiguration GetConfiguration(this ChannelConfigurationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Build();
    }

    /// <summary>
    /// Creates a masked version of the configuration for safe logging.
    /// </summary>
    /// <param name="builder">The channel configuration builder</param>
    /// <returns>A masked channel configuration</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static ChannelConfiguration GetMaskedConfiguration(this ChannelConfigurationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Build().GetMasked();
    }
}