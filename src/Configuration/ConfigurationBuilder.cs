#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Configuration;

/// <summary>
/// Fluent builder for channel configurations
/// </summary>
public sealed class ChannelConfigurationBuilder
{
    private readonly ChannelConfiguration _config;

    public ChannelConfigurationBuilder(NotificationChannel channelType)
    {
        _config = new ChannelConfiguration
        {
            ChannelType = channelType,
            DisplayName = channelType.ToString(),
            MaxRetries = 3,
            TimeoutMs = 10000
        };
    }

    public ChannelConfigurationBuilder WithName(string displayName)
    {
        _config.DisplayName = displayName;
        return this;
    }

    public ChannelConfigurationBuilder WithWebhook(string url)
    {
        _config.WebhookUrl = url;
        return this;
    }

    public ChannelConfigurationBuilder WithTargetId(string targetId)
    {
        _config.TargetId = targetId;
        return this;
    }

    public ChannelConfigurationBuilder WithTimeout(int milliseconds)
    {
        _config.TimeoutMs = milliseconds;
        return this;
    }

    public ChannelConfigurationBuilder WithRetries(int maxRetries)
    {
        _config.MaxRetries = maxRetries;
        return this;
    }

    public ChannelConfigurationBuilder WithMinimumPriority(NotificationPriority priority)
    {
        _config.MinimumPriority = priority;
        return this;
    }

    public ChannelConfigurationBuilder IncludeCommitDetails(bool include = true)
    {
        _config.IncludeCommitDetails = include;
        return this;
    }

    public ChannelConfigurationBuilder IncludeBuildUrl(bool include = true)
    {
        _config.IncludeBuildUrl = include;
        return this;
    }

    public ChannelConfigurationBuilder AllowEnvironments(params Environment[] environments)
    {
        _config.AllowedEnvironments = environments.ToList();
        return this;
    }

    public ChannelConfigurationBuilder AllowStatuses(params BuildStatus[] statuses)
    {
        _config.AllowedStatuses = statuses.ToList();
        return this;
    }

    public ChannelConfigurationBuilder OnlyProduction()
    {
        _config.AllowedEnvironments = new List<Environment> { Environment.Production };
        return this;
    }

    public ChannelConfigurationBuilder OnlyOnFailure()
    {
        _config.AllowedStatuses = new List<BuildStatus>
        {
            BuildStatus.Failed,
            BuildStatus.DeploymentFailed
        };
        return this;
    }

    public ChannelConfigurationBuilder OnlyOnSuccess()
    {
        _config.AllowedStatuses = new List<BuildStatus>
        {
            BuildStatus.Success,
            BuildStatus.DeploymentSuccess
        };
        return this;
    }

    public ChannelConfiguration Build()
    {
        if (string.IsNullOrWhiteSpace(_config.WebhookUrl))
            throw new InvalidOperationException("Webhook URL is required");

        return _config;
    }

    public static ChannelConfigurationBuilder ForSlack() => new(NotificationChannel.Slack);
    public static ChannelConfigurationBuilder ForDiscord() => new(NotificationChannel.Discord);
    public static ChannelConfigurationBuilder ForTelegram() => new(NotificationChannel.Telegram);
}

/// <summary>
/// Configuration for the notification system
/// </summary>
public sealed class NotificationSystemConfiguration
{
    public List<ChannelConfiguration> Channels { get; set; } = new();
    public int BatchSize { get; set; } = 10;
    public int MaxConcurrentRequests { get; set; } = 5;
    public int ProcessingIntervalSeconds { get; set; } = 30;
    public bool EnableMetrics { get; set; } = true;
    public bool EnableCaching { get; set; } = true;
    public int CacheDurationMinutes { get; set; } = 5;

    /// <summary>
    /// Validates the configuration
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (!Channels.Any())
            errors.Add("At least one channel must be configured");

        foreach (var channel in Channels)
        {
            if (string.IsNullOrWhiteSpace(channel.WebhookUrl))
                errors.Add($"Webhook URL is required for {channel.DisplayName}");

            if (string.IsNullOrWhiteSpace(channel.TargetId))
                errors.Add($"Target ID is required for {channel.DisplayName}");
        }

        return errors;
    }

    public bool IsValid() => !Validate().Any();
}

/// <summary>
/// Builder for notification system configuration
/// </summary>
public sealed class NotificationSystemConfigurationBuilder
{
    private readonly NotificationSystemConfiguration _config = new();
    private readonly List<ChannelConfiguration> _channels = new();

    public NotificationSystemConfigurationBuilder WithBatchSize(int size)
    {
        _config.BatchSize = size;
        return this;
    }

    public NotificationSystemConfigurationBuilder WithMaxConcurrency(int maxRequests)
    {
        _config.MaxConcurrentRequests = maxRequests;
        return this;
    }

    public NotificationSystemConfigurationBuilder WithProcessingInterval(int seconds)
    {
        _config.ProcessingIntervalSeconds = seconds;
        return this;
    }

    public NotificationSystemConfigurationBuilder EnableMetrics()
    {
        _config.EnableMetrics = true;
        return this;
    }

    public NotificationSystemConfigurationBuilder DisableMetrics()
    {
        _config.EnableMetrics = false;
        return this;
    }

    public NotificationSystemConfigurationBuilder EnableCaching()
    {
        _config.EnableCaching = true;
        return this;
    }

    public NotificationSystemConfigurationBuilder DisableCaching()
    {
        _config.EnableCaching = false;
        return this;
    }

    public NotificationSystemConfigurationBuilder WithCacheDuration(int minutes)
    {
        _config.CacheDurationMinutes = minutes;
        return this;
    }

    public NotificationSystemConfigurationBuilder AddChannel(ChannelConfiguration channel)
    {
        _channels.Add(channel);
        return this;
    }

    public NotificationSystemConfigurationBuilder AddSlackChannel(string webhookUrl, string targetId)
    {
        var channel = ChannelConfigurationBuilder.ForSlack()
            .WithWebhook(webhookUrl)
            .WithTargetId(targetId)
            .Build();

        _channels.Add(channel);
        return this;
    }

    public NotificationSystemConfigurationBuilder AddDiscordChannel(string webhookUrl, string targetId)
    {
        var channel = ChannelConfigurationBuilder.ForDiscord()
            .WithWebhook(webhookUrl)
            .WithTargetId(targetId)
            .Build();

        _channels.Add(channel);
        return this;
    }

    public NotificationSystemConfiguration Build()
    {
        _config.Channels = _channels;

        var errors = _config.Validate();
        if (errors.Any())
        {
            throw new InvalidOperationException($"Configuration is invalid:\n{string.Join("\n", errors)}");
        }

        return _config;
    }
}
