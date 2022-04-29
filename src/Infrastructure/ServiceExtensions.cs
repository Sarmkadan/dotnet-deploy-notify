// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;

namespace DotNetDeployNotify.Infrastructure;

/// <summary>
/// Extension methods for common operations with notification objects
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Determines if a notification is critical and requires immediate attention
    /// </summary>
    public static bool IsCritical(this DeploymentNotification notification)
    {
        return notification.Priority >= NotificationPriority.Critical ||
               notification.Status == BuildStatus.DeploymentFailed ||
               notification.Status == BuildStatus.Failed;
    }

    /// <summary>
    /// Determines if a notification is for a production environment
    /// </summary>
    public static bool IsProduction(this DeploymentNotification notification)
    {
        return notification.TargetEnvironment == Environment.Production ||
               notification.TargetEnvironment == Environment.PreProduction;
    }

    /// <summary>
    /// Checks if a channel configuration supports the given status
    /// </summary>
    public static bool SupportsStatus(this ChannelConfiguration config, BuildStatus status)
    {
        if (!config.AllowedStatuses.Any())
            return true;

        return config.AllowedStatuses.Contains(status);
    }

    /// <summary>
    /// Checks if a channel configuration supports the given environment
    /// </summary>
    public static bool SupportsEnvironment(this ChannelConfiguration config, Environment env)
    {
        if (!config.AllowedEnvironments.Any())
            return true;

        return config.AllowedEnvironments.Contains(env);
    }

    /// <summary>
    /// Gets a readable description of a notification status
    /// </summary>
    public static string GetDescription(this BuildStatus status)
    {
        return status switch
        {
            BuildStatus.Started => "Build has started",
            BuildStatus.InProgress => "Build is in progress",
            BuildStatus.Success => "Build completed successfully",
            BuildStatus.Failed => "Build failed with errors",
            BuildStatus.Cancelled => "Build was cancelled",
            BuildStatus.SuccessWithWarnings => "Build succeeded with warnings",
            BuildStatus.Deploying => "Deployment in progress",
            BuildStatus.DeploymentSuccess => "Deployment completed successfully",
            BuildStatus.DeploymentFailed => "Deployment failed",
            _ => "Unknown status"
        };
    }

    /// <summary>
    /// Gets a readable description of a channel type
    /// </summary>
    public static string GetDescription(this NotificationChannel channel)
    {
        return channel switch
        {
            NotificationChannel.Telegram => "Telegram",
            NotificationChannel.Slack => "Slack",
            NotificationChannel.Discord => "Discord",
            NotificationChannel.Webhook => "Generic Webhook",
            NotificationChannel.Email => "Email",
            _ => "Unknown Channel"
        };
    }

    /// <summary>
    /// Gets a readable description of delivery status
    /// </summary>
    public static string GetDescription(this DeliveryStatus status)
    {
        return status switch
        {
            DeliveryStatus.Pending => "Pending delivery",
            DeliveryStatus.Delivered => "Successfully delivered",
            DeliveryStatus.Failed => "Delivery failed",
            DeliveryStatus.Retried => "Retry scheduled",
            DeliveryStatus.Skipped => "Delivery skipped",
            DeliveryStatus.Timeout => "Delivery timed out",
            _ => "Unknown status"
        };
    }

    /// <summary>
    /// Gets a readable description of environment
    /// </summary>
    public static string GetDescription(this Environment env)
    {
        return env switch
        {
            Environment.Development => "Development",
            Environment.Staging => "Staging / QA",
            Environment.Production => "Production",
            Environment.Testing => "Testing",
            Environment.PreProduction => "Pre-Production",
            _ => "Unknown Environment"
        };
    }

    /// <summary>
    /// Merges two notifications by combining their metadata
    /// </summary>
    public static void MergeMetadata(this DeploymentNotification target, DeploymentNotification source)
    {
        foreach (var kvp in source.Metadata)
        {
            target.Metadata[kvp.Key] = kvp.Value;
        }
    }

    /// <summary>
    /// Creates a copy of a notification with new ID
    /// </summary>
    public static DeploymentNotification Clone(this DeploymentNotification notification)
    {
        return new DeploymentNotification
        {
            Id = Guid.NewGuid().ToString(),
            ProjectName = notification.ProjectName,
            Version = notification.Version,
            Status = notification.Status,
            Message = notification.Message,
            TargetEnvironment = notification.TargetEnvironment,
            BranchName = notification.BranchName,
            CommitHash = notification.CommitHash,
            CommitAuthor = notification.CommitAuthor,
            RepositoryUrl = notification.RepositoryUrl,
            BuildUrl = notification.BuildUrl,
            DurationSeconds = notification.DurationSeconds,
            CreatedAt = DateTime.UtcNow,
            Channels = new List<NotificationChannel>(notification.Channels),
            Priority = notification.Priority,
            Metadata = new Dictionary<string, object>(notification.Metadata),
            IsProcessed = false,
            DeliveryAttempts = 0
        };
    }

    /// <summary>
    /// Formats a notification as a compact string for logging
    /// </summary>
    public static string ToCompactString(this DeploymentNotification notification)
    {
        return $"[{notification.Status}] {notification.ProjectName}@{notification.Version} " +
               $"({notification.TargetEnvironment}/{notification.BranchName})";
    }

    /// <summary>
    /// Formats a result as a compact string for logging
    /// </summary>
    public static string ToCompactString(this NotificationResult result)
    {
        return $"[{result.Status}] {result.Channel} ({result.HttpStatusCode ?? 0}) {result.DurationMs}ms";
    }

    /// <summary>
    /// Gets the severity level of a build status
    /// </summary>
    public static int GetSeverityLevel(this BuildStatus status)
    {
        return status switch
        {
            BuildStatus.DeploymentFailed => 5,
            BuildStatus.Failed => 4,
            BuildStatus.SuccessWithWarnings => 2,
            BuildStatus.Success => 1,
            BuildStatus.DeploymentSuccess => 1,
            BuildStatus.Cancelled => 3,
            BuildStatus.Deploying => 1,
            BuildStatus.InProgress => 1,
            BuildStatus.Started => 0,
            _ => 0
        };
    }

    /// <summary>
    /// Determines if a delivery should be retried based on status
    /// </summary>
    public static bool ShouldRetry(this NotificationResult result)
    {
        return result.Status == DeliveryStatus.Failed ||
               result.Status == DeliveryStatus.Timeout;
    }

    /// <summary>
    /// Gets the next suggested retry delay based on attempt number
    /// </summary>
    public static TimeSpan GetRetryDelay(this NotificationResult result)
    {
        var baseDelayMs = 5000;
        var exponentialBackoff = Math.Min(Math.Pow(2, result.AttemptNumber), 10);
        var totalMs = (long)(baseDelayMs * exponentialBackoff);
        return TimeSpan.FromMilliseconds(totalMs);
    }
}
