#nullable enable
using System.ComponentModel.DataAnnotations;

namespace DotNetDeployNotify.Configuration;

/// <summary>
/// Root configuration options for DotnetDeployNotify.
/// </summary>
public sealed class DotnetDeployNotifyOptions
{
    public const string SectionName = "DotnetDeployNotify";

    [Required]
    public NotificationConfig Notification { get; set; } = new();

    public CanaryOptions Canary { get; set; } = new();
}

public sealed class NotificationConfig
{
    [Range(0, 100)]
    public int MaxRetries { get; set; } = 3;

    [Range(100, 60000)]
    public int WebhookTimeoutMs { get; set; } = 10000;

    [Range(100, 60000)]
    public int RetryDelayMs { get; set; } = 5000;

    public bool AutoProcessNotifications { get; set; } = true;

    [Range(1, 3600)]
    public int ProcessingIntervalSeconds { get; set; } = 30;

    [Required]
    public string StorageType { get; set; } = "InMemory";

    public string LogLevel { get; set; } = "Information";

    public string? StoragePath { get; set; }

    public bool IncludeCommitDetails { get; set; } = true;

    public bool IncludeBuildUrl { get; set; } = true;

    [Required]
    public string DefaultPriority { get; set; } = "Normal";

    public bool EnableAuditLogging { get; set; } = true;

    [Range(1, 365)]
    public int RetentionDays { get; set; } = 30;

    public Dictionary<string, EnvironmentChannelConfig> EnvironmentChannels { get; set; } = new();
}

public sealed class EnvironmentChannelConfig
{
    [Required]
    public string WebhookUrl { get; set; } = string.Empty;

    public string ChannelType { get; set; } = "Slack";

    public string DisplayName { get; set; } = string.Empty;

    public string TargetId { get; set; } = string.Empty;
}
