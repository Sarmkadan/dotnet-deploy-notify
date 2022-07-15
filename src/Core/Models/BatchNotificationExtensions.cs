#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides extension methods for <see cref="BatchNotification"/> to enhance batch processing capabilities
/// </summary>
public static class BatchNotificationExtensions
{
    /// <summary>
    /// Filters notifications in the batch by project name
    /// </summary>
    /// <param name="batch">The batch notification</param>
    /// <param name="projectName">Project name to filter by</param>
    /// <returns>Filtered list of notifications matching the project name</returns>
    public static List<DeploymentNotification> FilterByProject(this BatchNotification batch, string projectName)
    {
        ArgumentNullException.ThrowIfNull(batch);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectName);

        return batch.Notifications
            .Where(n => n.ProjectName.Equals(projectName, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Filters notifications in the batch by target environment
    /// </summary>
    /// <param name="batch">The batch notification</param>
    /// <param name="environment">Target environment to filter by</param>
    /// <returns>Filtered list of notifications matching the target environment</returns>
    public static List<DeploymentNotification> FilterByEnvironment(this BatchNotification batch, Environment environment)
    {
        ArgumentNullException.ThrowIfNull(batch);

        return batch.Notifications
            .Where(n => n.TargetEnvironment == environment)
            .ToList();
    }

    /// <summary>
    /// Gets a summary of delivery statistics for the batch
    /// </summary>
    /// <param name="batch">The batch notification</param>
    /// <returns>Formatted string with delivery statistics</returns>
    public static string GetDeliveryStatistics(this BatchNotification batch)
    {
        ArgumentNullException.ThrowIfNull(batch);

        var sb = new StringBuilder();
        sb.AppendLine($"Delivery Statistics for Batch: {batch.Name}");
        sb.AppendLine($"  Total Attempts: {batch.TotalDeliveryAttempts}");
        sb.AppendLine($"  Successful: {batch.SuccessfulDeliveries}");
        sb.AppendLine($"  Failed: {batch.FailedDeliveries}");
        sb.AppendLine($"  Success Rate: {batch.GetSuccessRate():F1}%");
        sb.AppendLine($"  Total Delivery Targets: {batch.GetTotalDeliveryTargets()}");

        if (batch.Metadata.TryGetValue("LastError", out var lastError) && lastError is string errorMessage)
        {
            sb.AppendLine($"  Last Error: {errorMessage}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Checks if the batch has any pending notifications that haven't been processed
    /// </summary>
    /// <param name="batch">The batch notification</param>
    /// <returns>True if any notification is not processed, false otherwise</returns>
    public static bool HasPendingNotifications(this BatchNotification batch)
    {
        ArgumentNullException.ThrowIfNull(batch);

        return batch.Notifications.Any(n => !n.IsProcessed);
    }

    /// <summary>
    /// Gets the count of pending notifications in the batch
    /// </summary>
    /// <param name="batch">The batch notification</param>
    /// <returns>Number of pending notifications</returns>
    public static int GetPendingNotificationCount(this BatchNotification batch)
    {
        ArgumentNullException.ThrowIfNull(batch);

        return batch.Notifications.Count(n => !n.IsProcessed);
    }

    /// <summary>
    /// Gets the count of processed notifications in the batch
    /// </summary>
    /// <param name="batch">The batch notification</param>
    /// <returns>Number of processed notifications</returns>
    public static int GetProcessedNotificationCount(this BatchNotification batch)
    {
        ArgumentNullException.ThrowIfNull(batch);

        return batch.Notifications.Count(n => n.IsProcessed);
    }

    /// <summary>
    /// Gets a formatted string representation of the batch with detailed information
    /// </summary>
    /// <param name="batch">The batch notification</param>
    /// <returns>Formatted string with batch details</returns>
    public static string GetDetailedSummary(this BatchNotification batch)
    {
        ArgumentNullException.ThrowIfNull(batch);

        var sb = new StringBuilder();
        sb.AppendLine("=== Batch Notification Details ===");
        sb.AppendLine($"Id: {batch.Id}");
        sb.AppendLine($"Name: {batch.Name}");
        sb.AppendLine($"Description: {batch.Description}");
        sb.AppendLine($"Status: {batch.Status}");
        sb.AppendLine($"Created: {batch.CreatedAt:yyyy-MM-dd HH:mm:ss}");

        if (batch.ScheduledAt.HasValue)
        {
            sb.AppendLine($"Scheduled: {batch.ScheduledAt.Value:yyyy-MM-dd HH:mm:ss}");
        }

        if (batch.SentAt.HasValue)
        {
            sb.AppendLine($"Sent: {batch.SentAt.Value:yyyy-MM-dd HH:mm:ss}");
        }

        sb.AppendLine($"\nNotifications: {batch.GetNotificationCount()}");
        sb.AppendLine($"Pending: {batch.GetPendingNotificationCount()}");
        sb.AppendLine($"Processed: {batch.GetProcessedNotificationCount()}");
        sb.AppendLine($"Channels: {batch.Channels.Count}");
        sb.AppendLine($"\nDelivery Stats:");
        sb.AppendLine($"  Total Attempts: {batch.TotalDeliveryAttempts}");
        sb.AppendLine($"  Successful: {batch.SuccessfulDeliveries}");
        sb.AppendLine($"  Failed: {batch.FailedDeliveries}");
        sb.AppendLine($"  Success Rate: {batch.GetSuccessRate():F1}%");

        if (batch.Metadata.Count > 0)
        {
            sb.AppendLine("\nMetadata:");
            foreach (var kvp in batch.Metadata)
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Determines if the batch is in a terminal state (Sent, Failed, Cancelled, or PartiallyCompleted)
    /// </summary>
    /// <param name="batch">The batch notification</param>
    /// <returns>True if batch is in terminal state, false otherwise</returns>
    public static bool IsTerminalState(this BatchNotification batch)
    {
        ArgumentNullException.ThrowIfNull(batch);

        return batch.Status is BatchStatus.Sent or BatchStatus.Failed
            or BatchStatus.Cancelled or BatchStatus.PartiallyCompleted;
    }

    /// <summary>
    /// Gets the count of unique channels in the batch
    /// </summary>
    /// <param name="batch">The batch notification</param>
    /// <returns>Number of unique channels</returns>
    public static int GetUniqueChannelCount(this BatchNotification batch)
    {
        ArgumentNullException.ThrowIfNull(batch);

        return batch.Channels
            .Distinct()
            .Count();
    }

    /// <summary>
    /// Gets the count of notifications by project name
    /// </summary>
    /// <param name="batch">The batch notification</param>
    /// <param name="projectName">Project name to count</param>
    /// <returns>Count of notifications for the specified project</returns>
    public static int GetNotificationCountByProject(this BatchNotification batch, string projectName)
    {
        ArgumentNullException.ThrowIfNull(batch);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectName);

        return batch.Notifications.Count(n => n.ProjectName.Equals(projectName, StringComparison.OrdinalIgnoreCase));
    }
}
