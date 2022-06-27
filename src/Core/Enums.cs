#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Core;

/// <summary>
/// Represents the status of a build or deployment operation
/// </summary>
public enum BuildStatus
{
    /// <summary>Build has started</summary>
    Started = 0,

    /// <summary>Build is in progress</summary>
    InProgress = 1,

    /// <summary>Build completed successfully</summary>
    Success = 2,

    /// <summary>Build failed with errors</summary>
    Failed = 3,

    /// <summary>Build was cancelled</summary>
    Cancelled = 4,

    /// <summary>Build completed with warnings</summary>
    SuccessWithWarnings = 5,

    /// <summary>Build deployment initiated</summary>
    Deploying = 6,

    /// <summary>Deployment completed</summary>
    DeploymentSuccess = 7,

    /// <summary>Deployment failed</summary>
    DeploymentFailed = 8
}

/// <summary>
/// Enum representing supported notification channels
/// </summary>
public enum NotificationChannel
{
    /// <summary>Telegram messaging service</summary>
    Telegram = 0,

    /// <summary>Slack workspace messaging</summary>
    Slack = 1,

    /// <summary>Discord server messaging</summary>
    Discord = 2,

    /// <summary>Generic webhook</summary>
    Webhook = 3,

    /// <summary>Email notifications</summary>
    Email = 4
}

/// <summary>
/// Enum for notification priority levels
/// </summary>
public enum NotificationPriority
{
    /// <summary>Low priority notification</summary>
    Low = 0,

    /// <summary>Normal priority notification</summary>
    Normal = 1,

    /// <summary>High priority notification</summary>
    High = 2,

    /// <summary>Critical priority - requires immediate attention</summary>
    Critical = 3
}

/// <summary>
/// Enum representing the result of a notification delivery attempt
/// </summary>
public enum DeliveryStatus
{
    /// <summary>Notification pending delivery</summary>
    Pending = 0,

    /// <summary>Notification successfully delivered</summary>
    Delivered = 1,

    /// <summary>Delivery attempt failed</summary>
    Failed = 2,

    /// <summary>Delivery was retried</summary>
    Retried = 3,

    /// <summary>Notification skipped due to policy</summary>
    Skipped = 4,

    /// <summary>Delivery attempt timed out</summary>
    Timeout = 5
}

/// <summary>
/// Environment classification for deployments
/// </summary>
public enum Environment
{
    /// <summary>Development environment</summary>
    Development = 0,

    /// <summary>Staging/QA environment</summary>
    Staging = 1,

    /// <summary>Production environment</summary>
    Production = 2,

    /// <summary>Test environment</summary>
    Testing = 3,

    /// <summary>Pre-production environment</summary>
    PreProduction = 4
}
