#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Configuration settings for a specific notification channel
/// </summary>
public sealed class ChannelConfiguration
{
    /// <summary>Unique identifier for this channel configuration</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Channel type (Telegram, Slack, Discord, etc.)</summary>
    public NotificationChannel ChannelType { get; set; }

    /// <summary>Webhook URL or endpoint for this channel</summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>API token or authentication credential</summary>
    public string ApiToken { get; set; } = string.Empty;

    /// <summary>Chat ID or channel ID specific to the platform</summary>
    public string TargetId { get; set; } = string.Empty;

    /// <summary>Human-readable name for this configuration</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Whether this channel is currently enabled</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Whether to include commit details in notifications</summary>
    public bool IncludeCommitDetails { get; set; } = true;

    /// <summary>Whether to include build URL in notifications</summary>
    public bool IncludeBuildUrl { get; set; } = true;

    /// <summary>Minimum priority level to send notifications</summary>
    public NotificationPriority MinimumPriority { get; set; } = NotificationPriority.Low;

    /// <summary>Environment filter (send only for specific environments)</summary>
    public List<Environment> AllowedEnvironments { get; set; } = new();

    /// <summary>Build status filter (send only for specific statuses)</summary>
    public List<BuildStatus> AllowedStatuses { get; set; } = new();

    /// <summary>Maximum retries for delivery</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>Timeout in milliseconds for webhook calls</summary>
    public int TimeoutMs { get; set; } = 10000;

    /// <summary>Custom headers for HTTP requests</summary>
    public Dictionary<string, string> CustomHeaders { get; set; } = new();

    /// <summary>Additional formatting or template settings</summary>
    public Dictionary<string, string> Settings { get; set; } = new();

    /// <summary>When this configuration was created</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When this configuration was last updated</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Validates the channel configuration has required fields
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(WebhookUrl) &&
               !string.IsNullOrWhiteSpace(DisplayName) &&
               IsEnabled &&
               TimeoutMs > 0;
    }

    /// <summary>
    /// Determines if a notification should be sent to this channel based on filters
    /// </summary>
    public bool ShouldSendNotification(DeploymentNotification notification)
    {
        if (!IsEnabled)
            return false;

        // Check priority level
        if (notification.Priority < MinimumPriority)
            return false;

        // Check environment filter
        if (AllowedEnvironments.Any() && !AllowedEnvironments.Contains(notification.TargetEnvironment))
            return false;

        // Check status filter
        if (AllowedStatuses.Any() && !AllowedStatuses.Contains(notification.Status))
            return false;

        return true;
    }

    /// <summary>
    /// Gets a custom setting value
    /// </summary>
    public string? GetSetting(string key)
    {
        return Settings.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Sets a custom setting value
    /// </summary>
    public void SetSetting(string key, string value)
    {
        Settings[key] = value;
    }

    /// <summary>
    /// Marks this configuration as updated
    /// </summary>
    public void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Returns a masked version of sensitive data for logging
    /// </summary>
    public ChannelConfiguration GetMasked()
    {
        return new ChannelConfiguration
        {
            Id = this.Id,
            ChannelType = this.ChannelType,
            WebhookUrl = MaskUrl(this.WebhookUrl),
            ApiToken = "***MASKED***",
            TargetId = this.TargetId,
            DisplayName = this.DisplayName,
            IsEnabled = this.IsEnabled,
            MinimumPriority = this.MinimumPriority,
            AllowedEnvironments = new List<Environment>(this.AllowedEnvironments),
            AllowedStatuses = new List<BuildStatus>(this.AllowedStatuses),
            MaxRetries = this.MaxRetries,
            TimeoutMs = this.TimeoutMs
        };
    }

    private static string MaskUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || url.Length < 20)
            return "***MASKED***";
        return url[..10] + "***MASKED***" + url[^5..];
    }
}
