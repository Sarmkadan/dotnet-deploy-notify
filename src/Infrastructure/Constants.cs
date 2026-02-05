// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Infrastructure;

/// <summary>
/// Application constants and default values
/// </summary>
public static class AppConstants
{
    /// <summary>Application name</summary>
    public const string AppName = "DotNetDeployNotify";

    /// <summary>Application version</summary>
    public const string Version = "1.0.0";

    /// <summary>Default notification timeout in milliseconds</summary>
    public const int DefaultTimeoutMs = 10000;

    /// <summary>Minimum notification timeout in milliseconds</summary>
    public const int MinTimeoutMs = 1000;

    /// <summary>Maximum notification timeout in milliseconds</summary>
    public const int MaxTimeoutMs = 60000;

    /// <summary>Default maximum retry attempts</summary>
    public const int DefaultMaxRetries = 3;

    /// <summary>Maximum allowed retry attempts</summary>
    public const int MaxRetryAttempts = 10;

    /// <summary>Default delay between retries in milliseconds</summary>
    public const int DefaultRetryDelayMs = 5000;

    /// <summary>HTTP header for request ID tracking</summary>
    public const string RequestIdHeader = "X-Request-ID";

    /// <summary>HTTP header for correlation ID</summary>
    public const string CorrelationIdHeader = "X-Correlation-ID";

    /// <summary>Content type for JSON payloads</summary>
    public const string JsonContentType = "application/json";

    /// <summary>User agent string for HTTP requests</summary>
    public const string UserAgent = $"{AppName}/{Version}";

    /// <summary>Default notification channel name</summary>
    public const string DefaultChannelName = "default";

    /// <summary>Batch processing size for notifications</summary>
    public const int BatchSize = 50;

    /// <summary>Maximum concurrent webhook requests</summary>
    public const int MaxConcurrentRequests = 10;
}

/// <summary>
/// HTTP status code constants for webhook responses
/// </summary>
public static class HttpStatusCodes
{
    /// <summary>Success response range (200-299)</summary>
    public const int SuccessMin = 200;
    public const int SuccessMax = 299;

    /// <summary>Client error range (400-499)</summary>
    public const int ClientErrorMin = 400;
    public const int ClientErrorMax = 499;

    /// <summary>Server error range (500-599)</summary>
    public const int ServerErrorMin = 500;
    public const int ServerErrorMax = 599;

    /// <summary>OK (200)</summary>
    public const int Ok = 200;

    /// <summary>Created (201)</summary>
    public const int Created = 201;

    /// <summary>Accepted (202)</summary>
    public const int Accepted = 202;

    /// <summary>Bad Request (400)</summary>
    public const int BadRequest = 400;

    /// <summary>Unauthorized (401)</summary>
    public const int Unauthorized = 401;

    /// <summary>Forbidden (403)</summary>
    public const int Forbidden = 403;

    /// <summary>Not Found (404)</summary>
    public const int NotFound = 404;

    /// <summary>Internal Server Error (500)</summary>
    public const int InternalServerError = 500;

    /// <summary>Service Unavailable (503)</summary>
    public const int ServiceUnavailable = 503;

    /// <summary>Gateway Timeout (504)</summary>
    public const int GatewayTimeout = 504;
}

/// <summary>
/// Logging message templates
/// </summary>
public static class LogMessages
{
    public const string NotificationCreated = "Notification created: {NotificationId} for {ProjectName}";
    public const string NotificationSent = "Notification {NotificationId} sent to {Channel}";
    public const string NotificationFailed = "Notification {NotificationId} failed to send to {Channel}: {Reason}";
    public const string WebhookAttempt = "Webhook attempt #{Attempt} to {Url}";
    public const string WebhookSuccess = "Webhook succeeded: {Url} (Status: {StatusCode})";
    public const string WebhookTimeout = "Webhook timeout: {Url}";
    public const string ConfigurationCreated = "Configuration created: {ConfigId} ({DisplayName})";
    public const string ConfigurationUpdated = "Configuration updated: {ConfigId} ({DisplayName})";
    public const string ConfigurationDeleted = "Configuration deleted: {ConfigId} ({DisplayName})";
    public const string ValidationFailed = "Validation failed: {Errors}";
    public const string RetryScheduled = "Retry scheduled for {NotificationId} at {ScheduledTime}";
    public const string ProcessingStarted = "Started processing {Count} pending notifications";
    public const string ProcessingCompleted = "Completed processing notifications: {SuccessCount} succeeded, {FailureCount} failed";
}

/// <summary>
/// Template messages for notifications
/// </summary>
public static class MessageTemplates
{
    public const string DeploymentSuccess = "✅ Deployment succeeded for {ProjectName} v{Version}";
    public const string DeploymentFailed = "❌ Deployment failed for {ProjectName} v{Version}";
    public const string BuildStarted = "⏳ Build started for {ProjectName} ({Branch})";
    public const string BuildCompleted = "Build completed for {ProjectName} in {DurationSeconds}s";
    public const string BranchInfo = "Branch: {BranchName}";
    public const string EnvironmentInfo = "Environment: {Environment}";
    public const string CommitInfo = "Commit: {CommitHash} by {Author}";
}

/// <summary>
/// Error messages
/// </summary>
public static class ErrorMessages
{
    public const string InvalidNotification = "Notification is invalid: missing required fields";
    public const string InvalidConfiguration = "Configuration is invalid: {Reason}";
    public const string WebhookUrlRequired = "Webhook URL is required";
    public const string ChannelNotConfigured = "No configuration found for channel: {Channel}";
    public const string NotificationNotFound = "Notification not found: {NotificationId}";
    public const string ConfigurationNotFound = "Configuration not found: {ConfigurationId}";
    public const string DeliveryFailed = "Delivery failed after {Attempts} attempts";
    public const string TimeoutExceeded = "Operation timed out after {TimeoutMs}ms";
    public const string RetryLimitExceeded = "Maximum retry limit ({MaxRetries}) exceeded";
}
