#nullable enable

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using FluentAssertions;

namespace DotNetDeployNotify.Tests;

public static class NotificationProcessorTestsExtensions
{
    /// <summary>
    /// Creates a processing result with the specified metrics for testing purposes.
    /// </summary>
    /// <param name="tests">The test instance</param>
    /// <param name="totalProcessed">Total number of notifications processed</param>
    /// <param name="successCount">Number of successful deliveries</param>
    /// <param name="failureCount">Number of failed deliveries</param>
    /// <param name="skippedCount">Number of skipped notifications</param>
    /// <param name="durationMs">Total processing duration in milliseconds</param>
    /// <returns>A new ProcessingResult instance</returns>
    public static ProcessingResult CreateProcessingResult(
        this NotificationProcessorTests tests,
        int totalProcessed = 10,
        int successCount = 8,
        int failureCount = 1,
        int skippedCount = 1,
        long durationMs = 1500)
    {
        return new ProcessingResult
        {
            TotalProcessed = totalProcessed,
            SuccessCount = successCount,
            FailureCount = failureCount,
            SkippedCount = skippedCount,
            DurationMs = durationMs
        };
    }

    /// <summary>
    /// Creates a processing statistics object with the specified metrics for testing purposes.
    /// </summary>
    /// <param name="tests">The test instance</param>
    /// <param name="totalNotifications">Total number of notifications</param>
    /// <param name="pendingCount">Number of pending notifications</param>
    /// <param name="processedCount">Number of processed notifications</param>
    /// <param name="successfulDeliveries">Number of successful deliveries</param>
    /// <param name="failedDeliveries">Number of failed deliveries</param>
    /// <param name="totalDeliveryAttempts">Total delivery attempts made</param>
    /// <param name="averageDeliveryTimeMs">Average delivery time in milliseconds</param>
    /// <returns>A new ProcessingStatistics instance</returns>
    public static ProcessingStatistics CreateProcessingStatistics(
        this NotificationProcessorTests tests,
        int totalNotifications = 25,
        int pendingCount = 5,
        int processedCount = 20,
        int successfulDeliveries = 18,
        int failedDeliveries = 2,
        int totalDeliveryAttempts = 25,
        long averageDeliveryTimeMs = 250)
    {
        return new ProcessingStatistics
        {
            TotalNotifications = totalNotifications,
            PendingCount = pendingCount,
            ProcessedCount = processedCount,
            SuccessfulDeliveries = successfulDeliveries,
            FailedDeliveries = failedDeliveries,
            TotalDeliveryAttempts = totalDeliveryAttempts,
            AverageDeliveryTimeMs = averageDeliveryTimeMs
        };
    }

    /// <summary>
    /// Creates a deployment notification with the specified properties for testing purposes.
    /// </summary>
    /// <param name="tests">The test instance</param>
    /// <param name="projectName">Project name</param>
    /// <param name="version">Version number</param>
    /// <param name="branchName">Branch name</param>
    /// <param name="isProcessed">Whether the notification is processed</param>
    /// <param name="channels">Notification channels</param>
    /// <returns>A new DeploymentNotification instance</returns>
    public static DeploymentNotification CreateDeploymentNotification(
        this NotificationProcessorTests tests,
        string projectName = "TestProject",
        string version = "1.0.0",
        string branchName = "main",
        bool isProcessed = false,
        List<NotificationChannel>? channels = null)
    {
        return new DeploymentNotification
        {
            Id = Guid.NewGuid().ToString(),
            ProjectName = projectName,
            Version = version,
            BranchName = branchName,
            Message = "Test deployment message",
            IsProcessed = isProcessed,
            Channels = channels ?? [NotificationChannel.Slack]
        };
    }

    /// <summary>
    /// Creates a notification result with the specified status and metrics for testing purposes.
    /// </summary>
    /// <param name="tests">The test instance</param>
    /// <param name="status">Delivery status</param>
    /// <param name="durationMs">Duration in milliseconds</param>
    /// <param name="httpStatusCode">HTTP status code</param>
    /// <param name="attemptNumber">Attempt number</param>
    /// <returns>A new NotificationResult instance</returns>
    public static NotificationResult CreateNotificationResult(
        this NotificationProcessorTests tests,
        DeliveryStatus status = DeliveryStatus.Delivered,
        long durationMs = 150,
        int httpStatusCode = 200,
        int attemptNumber = 1)
    {
        return new NotificationResult
        {
            Id = Guid.NewGuid().ToString(),
            NotificationId = Guid.NewGuid().ToString(),
            Status = status,
            DurationMs = durationMs,
            HttpStatusCode = httpStatusCode,
            AttemptNumber = attemptNumber,
            Channel = NotificationChannel.Slack
        };
    }
}
