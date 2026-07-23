#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Integration;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Notifications;

/// <summary>
/// Telegram notification channel implementation
/// </summary>
public sealed class TelegramChannel : INotificationChannel
{
    private readonly ILogger<TelegramChannel> _logger;
    private readonly IWebhookClient _webhookClient;

    /// <summary>
    /// Gets the name of the Telegram channel
    /// </summary>
    public string Name => "Telegram";

    /// <summary>
    /// Gets the channel type this instance handles
    /// </summary>
    public NotificationChannel ChannelType => NotificationChannel.Telegram;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelegramChannel"/> class
    /// </summary>
    /// <param name="webhookClient">Webhook client for HTTP requests</param>
    /// <param name="logger">Logger instance</param>
    public TelegramChannel(IWebhookClient webhookClient, ILogger<TelegramChannel> logger)
    {
        _webhookClient = webhookClient ?? throw new ArgumentNullException(nameof(webhookClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Determines if this channel can handle a specific notification target
    /// </summary>
    /// <param name="target">The notification target to check</param>
    /// <returns>True if this channel can handle the target, false otherwise</returns>
    public bool CanHandle(NotificationTarget target) =>
        target != null && !string.IsNullOrWhiteSpace(target.WebhookUrl);

    /// <summary>
    /// Sends a deployment notification to Telegram
    /// </summary>
    /// <param name="notification">The deployment notification to send</param>
    /// <param name="target">The Telegram webhook target configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Notification result</returns>
    public async Task<NotificationResult> SendAsync(
        DeploymentNotification notification,
        NotificationTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(target);

        try
        {
            _logger.LogDebug("Preparing Telegram notification for {ProjectName}", notification.ProjectName);

            // Build Telegram-specific text message
            var messageText = BuildTelegramMessage(notification, target);

            _logger.LogDebug("Sending Telegram notification to {WebhookUrl}", target.WebhookUrl);

            // Telegram uses sendMessage method in the URL path
            var telegramUrl = target.WebhookUrl.EndsWith("/sendMessage")
                ? target.WebhookUrl
                : $"{target.WebhookUrl.TrimEnd('/')}/sendMessage";

            // Send via webhook client
            var response = await _webhookClient.SendWebhookAsync(telegramUrl, messageText, target.CustomHeaders, cancellationToken);

            if (response.IsSuccessful)
            {
                _logger.LogInformation("Successfully sent Telegram notification for {ProjectName} v{Version}",
                    notification.ProjectName, notification.Version);
                return NotificationResult.Success((int)response.StatusCode);
            }

            var error = response.ErrorMessage ?? "Unknown error";
            _logger.LogError("Failed to send Telegram notification: {Error}", error);
            return NotificationResult.Failure(error, (int)response.StatusCode);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Telegram notification send operation was cancelled");
            return NotificationResult.Failure("Operation cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Telegram notification for {ProjectName}", notification.ProjectName);
            return NotificationResult.Failure(ex.Message);
        }
    }

    private string BuildTelegramMessage(DeploymentNotification notification, NotificationTarget target)
    {
        var message = new StringBuilder();

        // Add emoji based on status
        var statusEmoji = notification.Status switch
        {
            BuildStatus.DeploymentSuccess or BuildStatus.Success => "✅",
            BuildStatus.DeploymentFailed or BuildStatus.Failed => "❌",
            BuildStatus.Started or BuildStatus.InProgress => "🔄",
            BuildStatus.Cancelled => "⏹️",
            _ => "📝"
        };

        // Main header
        message.AppendLine($"<b>{statusEmoji} {notification.ProjectName} v{notification.Version}</b>");
        message.AppendLine();

        // Status and message
        var statusText = notification.Status switch
        {
            BuildStatus.DeploymentSuccess or BuildStatus.Success => "Successfully deployed",
            BuildStatus.DeploymentFailed or BuildStatus.Failed => "Deployment failed",
            BuildStatus.Started or BuildStatus.InProgress => "Deployment in progress",
            BuildStatus.Cancelled => "Deployment cancelled",
            _ => notification.Status.ToString()
        };

        message.AppendLine($"<b>Status:</b> {statusText}");
        message.AppendLine($"<b>Message:</b> {notification.Message}");
        message.AppendLine();

        // Environment
        message.AppendLine($"<b>Environment:</b> {notification.TargetEnvironment}");
        message.AppendLine($"<b>Branch:</b> {notification.BranchName}");

        // Commit information if enabled
        if (target.GetSetting("includeCommitDetails") != "false" && !string.IsNullOrWhiteSpace(notification.CommitHash))
        {
            message.AppendLine($"<b>Commit:</b> `{notification.CommitHash[..8]}`");
            message.AppendLine($"<b>Author:</b> {notification.CommitAuthor}");
        }

        // Duration if available
        if (notification.DurationSeconds.HasValue && notification.DurationSeconds > 0)
        {
            message.AppendLine($"<b>Duration:</b> {notification.DurationSeconds.Value}s");
        }

        // Repository URL if available
        if (!string.IsNullOrWhiteSpace(notification.RepositoryUrl))
        {
            message.AppendLine($"<b>Repository:</b> {notification.RepositoryUrl}");
        }

        // Build URL if available
        if (!string.IsNullOrWhiteSpace(notification.BuildUrl))
        {
            message.AppendLine();
            message.AppendLine($"<a href=\"{notification.BuildUrl}\">View Build Details</a>");
        }

        // Timestamp
        message.AppendLine();
        message.AppendLine($"<i>📅 {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</i>");

        return message.ToString();
    }
}