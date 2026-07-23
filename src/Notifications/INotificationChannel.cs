#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core.Models;
using System.Threading;

namespace DotNetDeployNotify.Notifications;

/// <summary>
/// Represents a notification channel that can send deployment notifications
/// </summary>
public interface INotificationChannel
{
    /// <summary>
    /// Gets the name of the notification channel
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the type of notification channel this instance handles
    /// </summary>
    NotificationChannel ChannelType { get; }

    /// <summary>
    /// Determines if this channel can handle a specific notification target
    /// </summary>
    /// <param name="target">The notification target to check</param>
    /// <returns>True if this channel can handle the target, false otherwise</returns>
    bool CanHandle(NotificationTarget target);

    /// <summary>
    /// Sends a deployment notification asynchronously
    /// </summary>
    /// <param name="notification">The deployment notification to send</param>
    /// <param name="target">The notification target configuration</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Notification result indicating success or failure</returns>
    Task<NotificationResult> SendAsync(
        DeploymentNotification notification,
        NotificationTarget target,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a notification delivery attempt
/// </summary>
public sealed class NotificationResult
{
    /// <summary>
    /// Gets whether the notification was successfully delivered
    /// </summary>
    public bool IsSuccessful { get; init; }

    /// <summary>
    /// Gets the HTTP status code from the webhook response (if applicable)
    /// </summary>
    public int? HttpStatusCode { get; init; }

    /// <summary>
    /// Gets the error message if the delivery failed
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the timestamp when this result was created
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful notification result
    /// </summary>
    /// <param name="httpStatusCode">The HTTP status code from the webhook</param>
    /// <returns>Notification result</returns>
    public static NotificationResult Success(int? httpStatusCode = null) =>
        new NotificationResult { IsSuccessful = true, HttpStatusCode = httpStatusCode };

    /// <summary>
    /// Creates a failed notification result
    /// </summary>
    /// <param name="errorMessage">The error message describing the failure</param>
    /// <param name="httpStatusCode">The HTTP status code from the webhook</param>
    /// <returns>Notification result</returns>
    public static NotificationResult Failure(string errorMessage, int? httpStatusCode = null) =>
        new NotificationResult { IsSuccessful = false, ErrorMessage = errorMessage, HttpStatusCode = httpStatusCode };
}

/// <summary>
/// Notification target configuration containing webhook URL and channel-specific settings
/// </summary>
public sealed class NotificationTarget
{
    /// <summary>
    /// Gets the webhook URL for this notification target
    /// </summary>
    public string WebhookUrl { get; init; }

    /// <summary>
    /// Gets the API token or authentication credential
    /// </summary>
    public string ApiToken { get; init; }

    /// <summary>
    /// Gets the chat ID or channel identifier
    /// </summary>
    public string TargetId { get; init; }

    /// <summary>
    /// Gets custom headers to include in the HTTP request
    /// </summary>
    public Dictionary<string, string> CustomHeaders { get; } = new();

    /// <summary>
    /// Gets additional settings specific to this notification target
    /// </summary>
    public Dictionary<string, string> Settings { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationTarget"/> class
    /// </summary>
    /// <param name="webhookUrl">The webhook URL</param>
    /// <param name="apiToken">The API token</param>
    /// <param name="targetId">The target identifier</param>
    public NotificationTarget(string webhookUrl, string apiToken = "", string targetId = "")
    {
        WebhookUrl = webhookUrl ?? throw new ArgumentNullException(nameof(webhookUrl));
        ApiToken = apiToken ?? string.Empty;
        TargetId = targetId ?? string.Empty;
    }

    /// <summary>
    /// Gets a custom setting value
    /// </summary>
    /// <param name="key">The setting key</param>
    /// <returns>The setting value if found, null otherwise</returns>
    public string? GetSetting(string key) => Settings.TryGetValue(key, out var value) ? value : null;

    /// <summary>
    /// Sets a custom setting value
    /// </summary>
    /// <param name="key">The setting key</param>
    /// <param name="value">The setting value</param>
    public void SetSetting(string key, string value) => Settings[key] = value;
}