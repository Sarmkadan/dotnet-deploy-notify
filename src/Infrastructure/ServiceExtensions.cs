#nullable enable
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
    /// <param name="notification">The deployment notification to check.</param>
    /// <returns><see langword="true"/> if the notification is critical; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="notification"/> is <see langword="null"/>.</exception>
    public static bool IsCritical(this DeploymentNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        return notification.Priority >= NotificationPriority.Critical ||
               notification.Status == BuildStatus.DeploymentFailed ||
               notification.Status == BuildStatus.Failed;
    }

    /// <summary>
    /// Determines if a notification is for a production environment
    /// </summary>
    /// <param name="notification">The deployment notification to check.</param>
    /// <returns><see langword="true"/> if the notification targets a production environment; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="notification"/> is <see langword="null"/>.</exception>
    public static bool IsProduction(this DeploymentNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        return notification.TargetEnvironment == Environment.Production ||
               notification.TargetEnvironment == Environment.PreProduction;
    }

    /// <summary>
    /// Checks if a channel configuration supports the given status
    /// </summary>
    /// <param name="config">The channel configuration to check.</param>
    /// <param name="status">The build status to verify.</param>
    /// <returns><see langword="true"/> if the channel supports the status; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
    public static bool SupportsStatus(this ChannelConfiguration config, BuildStatus status)
    {
        ArgumentNullException.ThrowIfNull(config);
        if (!config.AllowedStatuses.Any())
            return true;

        return config.AllowedStatuses.Contains(status);
    }

    /// <summary>
    /// Checks if a channel configuration supports the given environment
    /// </summary>
    /// <param name="config">The channel configuration to check.</param>
    /// <param name="env">The environment to verify.</param>
    /// <returns><see langword="true"/> if the channel supports the environment; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
    public static bool SupportsEnvironment(this ChannelConfiguration config, Environment env)
    {
        ArgumentNullException.ThrowIfNull(config);
        if (!config.AllowedEnvironments.Any())
            return true;

        return config.AllowedEnvironments.Contains(env);
    }

    /// <summary>
    /// Gets a readable description of a notification status
    /// </summary>
    /// <param name="status">The build status to describe.</param>
    /// <returns>A human-readable description of the status.</returns>
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
    /// <param name="channel">The notification channel to describe.</param>
    /// <returns>A human-readable description of the channel.</returns>
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
    /// <param name="status">The delivery status to describe.</param>
    /// <returns>A human-readable description of the delivery status.</returns>
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
    /// <param name="env">The environment to describe.</param>
    /// <returns>A human-readable description of the environment.</returns>
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
    /// <param name="target">The target notification to merge into.</param>
    /// <param name="source">The source notification to merge from.</param>
    /// <exception cref="ArgumentNullException"><paramref name="target"/> or <paramref name="source"/> is <see langword="null"/>.</exception>
    public static void MergeMetadata(this DeploymentNotification target, DeploymentNotification source)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(source);

        foreach (var kvp in source.Metadata)
        {
            target.Metadata[kvp.Key] = kvp.Value;
        }
    }

    /// <summary>
    /// Creates a copy of a notification with new ID
    /// </summary>
    /// <param name="notification">The notification to clone.</param>
    /// <returns>A new notification instance with copied properties and a new ID.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="notification"/> is <see langword="null"/>.</exception>
    public static DeploymentNotification Clone(this DeploymentNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
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
    /// <param name="notification">The notification to format.</param>
    /// <returns>A compact string representation of the notification.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="notification"/> is <see langword="null"/>.</exception>
    public static string ToCompactString(this DeploymentNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        return $"[{notification.Status}] {notification.ProjectName}@{notification.Version} " +
               $"({notification.TargetEnvironment}/{notification.BranchName})";
    }

    /// <summary>
    /// Formats a result as a compact string for logging
    /// </summary>
    /// <param name="result">The notification result to format.</param>
    /// <returns>A compact string representation of the notification result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static string ToCompactString(this NotificationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return $"[{result.Status}] {result.Channel} ({result.HttpStatusCode ?? 0}) {result.DurationMs}ms";
    }

    /// <summary>
    /// Gets the severity level of a build status
    /// </summary>
    /// <param name="status">The build status to evaluate.</param>
    /// <returns>A numeric severity level where higher values indicate more severe statuses.</returns>
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
    /// <param name="result">The notification result to evaluate.</param>
    /// <returns><see langword="true"/> if the delivery should be retried; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static bool ShouldRetry(this NotificationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.Status == DeliveryStatus.Failed ||
               result.Status == DeliveryStatus.Timeout;
    }

    /// <summary>
    /// Gets the next suggested retry delay based on attempt number
    /// </summary>
    /// <param name="result">The notification result containing attempt information.</param>
    /// <returns>The suggested retry delay time span.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static TimeSpan GetRetryDelay(this NotificationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var baseDelayMs = 5000;
        var exponentialBackoff = Math.Min(Math.Pow(2, result.AttemptNumber), 10);
        var totalMs = (long)(baseDelayMs * exponentialBackoff);
        return TimeSpan.FromMilliseconds(totalMs);
    }
}