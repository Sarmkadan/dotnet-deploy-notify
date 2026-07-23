#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Nodes;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Integration;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Notifications;

/// <summary>
/// Discord notification channel implementation
/// </summary>
public sealed class DiscordChannel : INotificationChannel
{
    private readonly ILogger<DiscordChannel> _logger;
    private readonly IWebhookClient _webhookClient;

    /// <summary>
    /// Gets the name of the Discord channel
    /// </summary>
    public string Name => "Discord";

    /// <summary>
    /// Gets the channel type this instance handles
    /// </summary>
    public NotificationChannel ChannelType => NotificationChannel.Discord;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscordChannel"/> class
    /// </summary>
    /// <param name="webhookClient">Webhook client for HTTP requests</param>
    /// <param name="logger">Logger instance</param>
    public DiscordChannel(IWebhookClient webhookClient, ILogger<DiscordChannel> logger)
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
    /// Sends a deployment notification to Discord
    /// </summary>
    /// <param name="notification">The deployment notification to send</param>
    /// <param name="target">The Discord webhook target configuration</param>
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
            _logger.LogDebug("Preparing Discord notification for {ProjectName}", notification.ProjectName);

            // Build Discord-specific payload
            var payload = BuildDiscordPayload(notification, target);
            var payloadJson = payload.ToJsonString(new JsonSerializerOptions { WriteIndented = false });

            _logger.LogDebug("Sending Discord notification to {WebhookUrl}", target.WebhookUrl);

            // Send via webhook client
            var response = await _webhookClient.SendWebhookAsync(target.WebhookUrl, payloadJson, target.CustomHeaders, cancellationToken);

            if (response.IsSuccessful)
            {
                _logger.LogInformation("Successfully sent Discord notification for {ProjectName} v{Version}",
                    notification.ProjectName, notification.Version);
                return NotificationResult.Success((int)response.StatusCode);
            }

            var error = response.ErrorMessage ?? "Unknown error";
            _logger.LogError("Failed to send Discord notification: {Error}", error);
            return NotificationResult.Failure(error, (int)response.StatusCode);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Discord notification send operation was cancelled");
            return NotificationResult.Failure("Operation cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Discord notification for {ProjectName}", notification.ProjectName);
            return NotificationResult.Failure(ex.Message);
        }
    }

    private JsonObject BuildDiscordPayload(DeploymentNotification notification, NotificationTarget target)
    {
        // Determine color based on deployment status
        var statusColor = notification.Status switch
        {
            BuildStatus.DeploymentSuccess or BuildStatus.Success => 0x00ff00, // Green
            BuildStatus.DeploymentFailed or BuildStatus.Failed => 0xff0000, // Red
            BuildStatus.Started or BuildStatus.InProgress => 0xffff00, // Yellow
            BuildStatus.Cancelled => 0xff9900, // Orange
            _ => 0x0000ff // Blue for unknown status
        };

        // Build embed object
        var embed = new JsonObject
        {
            ["title"] = $"🚀 Deployment: {notification.ProjectName} v{notification.Version}",
            ["description"] = notification.Message,
            ["url"] = notification.BuildUrl,
            ["color"] = statusColor,
            ["timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            ["fields"] = new JsonArray()
        };

        var fields = (JsonArray)embed["fields"];

        // Add environment field
        fields.Add(new JsonObject
        {
            ["name"] = "Environment",
            ["value"] = notification.TargetEnvironment.ToString(),
            ["inline"] = true
        });

        // Add branch field
        fields.Add(new JsonObject
        {
            ["name"] = "Branch",
            ["value"] = $"`{notification.BranchName}`",
            ["inline"] = true
        });

        // Add status field
        var statusText = notification.Status switch
        {
            BuildStatus.DeploymentSuccess or BuildStatus.Success => "✅ Success",
            BuildStatus.DeploymentFailed or BuildStatus.Failed => "❌ Failed",
            BuildStatus.Started or BuildStatus.InProgress => "🔄 In Progress",
            BuildStatus.Cancelled => "⏹️ Cancelled",
            _ => notification.Status.ToString()
        };

        fields.Add(new JsonObject
        {
            ["name"] = "Status",
            ["value"] = statusText,
            ["inline"] = true
        });

        // Add commit information if available
        if (target.GetSetting("includeCommitDetails") != "false" && !string.IsNullOrWhiteSpace(notification.CommitHash))
        {
            fields.Add(new JsonObject
            {
                ["name"] = "Commit",
                ["value"] = $"`{notification.CommitHash[..8]}` - {notification.CommitAuthor}",
                ["inline"] = false
            });
        }

        // Add duration if available
        if (notification.DurationSeconds.HasValue && notification.DurationSeconds > 0)
        {
            fields.Add(new JsonObject
            {
                ["name"] = "Duration",
                ["value"] = $"{notification.DurationSeconds.Value}s",
                ["inline"] = true
            });
        }

        // Add repository URL if available
        if (!string.IsNullOrWhiteSpace(notification.RepositoryUrl))
        {
            fields.Add(new JsonObject
            {
                ["name"] = "Repository",
                ["value"] = notification.RepositoryUrl,
                ["inline"] = false
            });
        }

        // Build main payload
        var payload = new JsonObject
        {
            ["content"] = $"**Deployment Notification**\n{notification.GetSummary()}",
            ["embeds"] = new JsonArray { embed }
        };

        return payload;
    }
}