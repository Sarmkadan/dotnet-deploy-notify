// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Represents the payload structure sent to webhooks
/// </summary>
public class WebhookPayload
{
    /// <summary>Unique event identifier</summary>
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Event type (deployment, build_completed, etc.)</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Timestamp when the event occurred</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Source system that generated the event</summary>
    public string Source { get; set; } = "dotnet-deploy-notify";

    /// <summary>Version of the payload schema</summary>
    public string SchemaVersion { get; set; } = "1.0.0";

    /// <summary>The deployment notification data</summary>
    public WebhookData Data { get; set; } = new();

    /// <summary>Validation errors if any</summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Returns true if the payload is complete and valid
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(EventId) &&
               !string.IsNullOrWhiteSpace(EventType) &&
               Data != null &&
               Data.IsValid();
    }

    /// <summary>
    /// Serializes the payload to JSON string
    /// </summary>
    public string ToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(this,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = false });
    }
}

/// <summary>
/// Core deployment data within a webhook payload
/// </summary>
public class WebhookData
{
    /// <summary>Project name</summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>Version or build number</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Build status</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Detailed message</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Target environment</summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>Branch name</summary>
    public string Branch { get; set; } = string.Empty;

    /// <summary>Commit hash (shortened)</summary>
    public string CommitHash { get; set; } = string.Empty;

    /// <summary>Commit author</summary>
    public string CommitAuthor { get; set; } = string.Empty;

    /// <summary>Repository URL</summary>
    public string RepositoryUrl { get; set; } = string.Empty;

    /// <summary>Build/Job URL</summary>
    public string BuildUrl { get; set; } = string.Empty;

    /// <summary>Build duration in seconds</summary>
    public int? DurationSeconds { get; set; }

    /// <summary>Priority level</summary>
    public string Priority { get; set; } = "normal";

    /// <summary>Additional custom properties</summary>
    public Dictionary<string, object> CustomProperties { get; set; } = new();

    /// <summary>
    /// Validates the webhook data
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ProjectName) &&
               !string.IsNullOrWhiteSpace(Version) &&
               !string.IsNullOrWhiteSpace(Status);
    }

    /// <summary>
    /// Creates webhook data from a deployment notification
    /// </summary>
    public static WebhookData FromNotification(DeploymentNotification notification)
    {
        return new WebhookData
        {
            ProjectName = notification.ProjectName,
            Version = notification.Version,
            Status = notification.Status.ToString(),
            Message = notification.Message,
            Environment = notification.TargetEnvironment.ToString(),
            Branch = notification.BranchName,
            CommitHash = notification.CommitHash[..Math.Min(7, notification.CommitHash.Length)],
            CommitAuthor = notification.CommitAuthor,
            RepositoryUrl = notification.RepositoryUrl,
            BuildUrl = notification.BuildUrl,
            DurationSeconds = notification.DurationSeconds,
            Priority = notification.Priority.ToString(),
            CustomProperties = new Dictionary<string, object>(notification.Metadata)
        };
    }
}
