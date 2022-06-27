#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Status of a deployment rollback operation
/// </summary>
public enum RollbackStatus
{
    /// <summary>Rollback has been requested and is queued</summary>
    Pending = 0,

    /// <summary>Rollback is actively being processed</summary>
    InProgress = 1,

    /// <summary>Rollback completed successfully</summary>
    Completed = 2,

    /// <summary>Rollback failed and could not be completed</summary>
    Failed = 3,

    /// <summary>Rollback was cancelled before completion</summary>
    Cancelled = 4
}

/// <summary>
/// Represents a one-click rollback request for a deployment
/// </summary>
public sealed class RollbackRequest
{
    /// <summary>Unique identifier for this rollback request</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Project or application to roll back</summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>Version to revert to</summary>
    public string TargetVersion { get; set; } = string.Empty;

    /// <summary>Version currently deployed that is being reverted</summary>
    public string CurrentVersion { get; set; } = string.Empty;

    /// <summary>Environment where the rollback should occur</summary>
    public Environment TargetEnvironment { get; set; }

    /// <summary>User or system that initiated the rollback</summary>
    public string RequestedBy { get; set; } = string.Empty;

    /// <summary>Reason for initiating the rollback</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Channels where rollback notifications should be sent</summary>
    public List<NotificationChannel> Channels { get; set; } = new();

    /// <summary>Priority level for the rollback notification</summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.High;

    /// <summary>Timestamp when the request was created</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Additional metadata for rollback context</summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Validates that all required fields are present
    /// </summary>
    /// <returns>True if the request is valid, false otherwise</returns>
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(ProjectName) &&
        !string.IsNullOrWhiteSpace(TargetVersion) &&
        !string.IsNullOrWhiteSpace(CurrentVersion) &&
        Channels.Any();

    /// <summary>
    /// Returns a concise summary of the rollback request
    /// </summary>
    public string GetSummary() =>
        $"Rollback {ProjectName}: v{CurrentVersion} → v{TargetVersion} [{TargetEnvironment}]";
}

/// <summary>
/// Represents the outcome of a deployment rollback operation
/// </summary>
public sealed class RollbackResult
{
    /// <summary>Unique identifier for this rollback result</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>ID of the originating rollback request</summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>Project that was rolled back</summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>Version the deployment was reverted from</summary>
    public string RolledBackFromVersion { get; set; } = string.Empty;

    /// <summary>Version the deployment was reverted to</summary>
    public string RolledBackToVersion { get; set; } = string.Empty;

    /// <summary>Current status of this rollback operation</summary>
    public RollbackStatus Status { get; set; } = RollbackStatus.Pending;

    /// <summary>Error message if the rollback failed</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>When the rollback process started</summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the rollback process finished</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Notification results dispatched as part of this rollback</summary>
    public List<NotificationResult> NotificationResults { get; set; } = new();

    /// <summary>Whether the rollback completed successfully</summary>
    public bool IsSuccessful => Status == RollbackStatus.Completed;

    /// <summary>
    /// Marks the rollback as successfully completed
    /// </summary>
    public void MarkAsCompleted()
    {
        Status = RollbackStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the rollback as failed with an error description
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        Status = RollbackStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the rollback as cancelled
    /// </summary>
    public void MarkAsCancelled()
    {
        Status = RollbackStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Returns a human-readable summary of the rollback outcome
    /// </summary>
    public string GetSummary() => Status switch
    {
        RollbackStatus.Completed => $"Rollback of {ProjectName} from v{RolledBackFromVersion} to v{RolledBackToVersion} succeeded",
        RollbackStatus.Failed    => $"Rollback of {ProjectName} failed: {ErrorMessage}",
        RollbackStatus.Cancelled => $"Rollback of {ProjectName} was cancelled",
        RollbackStatus.InProgress => $"Rollback of {ProjectName} is in progress",
        _                        => $"Rollback of {ProjectName} is pending"
    };
}
