#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Represents a batch of notifications to be sent together
/// </summary>
public sealed class BatchNotification
{
    /// <summary>Unique identifier for this batch</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Name or identifier for the batch</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description of the batch</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Notifications in this batch</summary>
    public List<DeploymentNotification> Notifications { get; set; } = new();

    /// <summary>Channels to send all notifications to</summary>
    public List<NotificationChannel> Channels { get; set; } = new();

    /// <summary>When this batch was created</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When this batch is scheduled to be sent</summary>
    public DateTime? ScheduledAt { get; set; }

    /// <summary>When this batch was actually sent</summary>
    public DateTime? SentAt { get; set; }

    /// <summary>Current state of the batch</summary>
    public BatchStatus Status { get; set; } = BatchStatus.Pending;

    /// <summary>Total number of delivery attempts</summary>
    public int TotalDeliveryAttempts { get; set; }

    /// <summary>Number of successful deliveries</summary>
    public int SuccessfulDeliveries { get; set; }

    /// <summary>Number of failed deliveries</summary>
    public int FailedDeliveries { get; set; }

    /// <summary>Optional metadata</summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets the total number of notifications in the batch
    /// </summary>
    public int GetNotificationCount() => Notifications.Count;

    /// <summary>
    /// Gets the total number of delivery targets (notifications × channels)
    /// </summary>
    public int GetTotalDeliveryTargets() => GetNotificationCount() * Channels.Count;

    /// <summary>
    /// Calculates the success rate
    /// </summary>
    public double GetSuccessRate()
    {
        if (TotalDeliveryAttempts == 0)
            return 0;
        return (SuccessfulDeliveries * 100.0) / TotalDeliveryAttempts;
    }

    /// <summary>
    /// Validates the batch for required fields
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name) &&
               Notifications.Any() &&
               Channels.Any() &&
               Notifications.All(n => n.IsValid());
    }

    /// <summary>
    /// Checks if the batch is ready to be sent
    /// </summary>
    public bool IsReadyToSend()
    {
        return IsValid() &&
               Status == BatchStatus.Pending &&
               (!ScheduledAt.HasValue || ScheduledAt <= DateTime.UtcNow);
    }

    /// <summary>
    /// Marks the batch as sent
    /// </summary>
    public void MarkAsSent()
    {
        Status = BatchStatus.Sent;
        SentAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the batch as failed
    /// </summary>
    public void MarkAsFailed()
    {
        Status = BatchStatus.Failed;
    }

    /// <summary>
    /// Marks the batch as cancelled
    /// </summary>
    public void MarkAsCancelled()
    {
        Status = BatchStatus.Cancelled;
    }

    /// <summary>
    /// Gets a summary of the batch
    /// </summary>
    public string GetSummary()
    {
        return $"Batch: {Name} | Notifications: {GetNotificationCount()} | " +
               $"Channels: {Channels.Count} | Status: {Status} | Success: {GetSuccessRate():F1}%";
    }

    /// <summary>
    /// Adds a notification to the batch
    /// </summary>
    public void AddNotification(DeploymentNotification notification)
    {
        Notifications.Add(notification);
    }

    /// <summary>
    /// Removes a notification from the batch
    /// </summary>
    public bool RemoveNotification(string notificationId)
    {
        var notification = Notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification is not null)
        {
            Notifications.Remove(notification);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Clears all notifications in the batch
    /// </summary>
    public void ClearNotifications()
    {
        Notifications.Clear();
    }

    /// <summary>
    /// Sets metadata value
    /// </summary>
    public void SetMetadata(string key, object value)
    {
        Metadata[key] = value;
    }

    /// <summary>
    /// Gets metadata value
    /// </summary>
    public T? GetMetadata<T>(string key)
    {
        if (Metadata.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;
        return default;
    }
}

/// <summary>
/// Represents the status of a batch
/// </summary>
public enum BatchStatus
{
    /// <summary>Batch is pending and awaiting processing</summary>
    Pending = 0,

    /// <summary>Batch is currently being processed</summary>
    Processing = 1,

    /// <summary>Batch has been sent successfully</summary>
    Sent = 2,

    /// <summary>Batch failed to send</summary>
    Failed = 3,

    /// <summary>Batch was cancelled by user</summary>
    Cancelled = 4,

    /// <summary>Batch is scheduled for future delivery</summary>
    Scheduled = 5,

    /// <summary>Batch is partially completed (some succeeded, some failed)</summary>
    PartiallyCompleted = 6
}
