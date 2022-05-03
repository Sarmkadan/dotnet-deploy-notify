// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Represents a deployment notification event in the system
/// </summary>
public class DeploymentNotification
{
    /// <summary>Unique identifier for this notification</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Project or application name</summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>Version or build number</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Current status of the build/deployment</summary>
    public BuildStatus Status { get; set; }

    /// <summary>Detailed message about the deployment</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Target environment</summary>
    public Environment TargetEnvironment { get; set; }

    /// <summary>Branch name being deployed</summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>Git commit hash</summary>
    public string CommitHash { get; set; } = string.Empty;

    /// <summary>Git commit author name</summary>
    public string CommitAuthor { get; set; } = string.Empty;

    /// <summary>Repository URL</summary>
    public string RepositoryUrl { get; set; } = string.Empty;

    /// <summary>Build/Job URL for reference</summary>
    public string BuildUrl { get; set; } = string.Empty;

    /// <summary>Duration of the build in seconds</summary>
    public int? DurationSeconds { get; set; }

    /// <summary>Timestamp when notification was created</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Channels where this notification should be sent</summary>
    public List<NotificationChannel> Channels { get; set; } = new();

    /// <summary>Priority level of the notification</summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    /// <summary>Additional metadata key-value pairs</summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>Whether the notification has been processed</summary>
    public bool IsProcessed { get; set; }

    /// <summary>Number of delivery attempts</summary>
    public int DeliveryAttempts { get; set; }

    /// <summary>
    /// Validates the notification data for required fields
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ProjectName) &&
               !string.IsNullOrWhiteSpace(Version) &&
               !string.IsNullOrWhiteSpace(BranchName) &&
               Channels.Any();
    }

    /// <summary>
    /// Gets a formatted summary of the notification
    /// </summary>
    /// <returns>String representation of the notification</returns>
    public string GetSummary()
    {
        return $"[{Status}] {ProjectName} v{Version} - {TargetEnvironment} ({BranchName})";
    }

    /// <summary>
    /// Increments the delivery attempt counter
    /// </summary>
    public void IncrementDeliveryAttempt()
    {
        DeliveryAttempts++;
    }

    /// <summary>
    /// Marks the notification as processed
    /// </summary>
    public void MarkAsProcessed()
    {
        IsProcessed = true;
    }

    /// <summary>
    /// Gets metadata value by key with type conversion
    /// </summary>
    public T? GetMetadata<T>(string key)
    {
        if (Metadata.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    /// <summary>
    /// Sets metadata value by key
    /// </summary>
    public void SetMetadata(string key, object value)
    {
        Metadata[key] = value;
    }
}
