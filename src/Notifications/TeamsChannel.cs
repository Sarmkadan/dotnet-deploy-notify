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
/// Microsoft Teams notification channel implementation
/// </summary>
public sealed class TeamsChannel : INotificationChannel
{
    private readonly ILogger<TeamsChannel> _logger;
    private readonly IWebhookClient _webhookClient;

    /// <summary>
    /// Gets the name of the Teams channel
    /// </summary>
    public string Name => "Teams";

    /// <summary>
    /// Gets the channel type this instance handles
    /// </summary>
    public NotificationChannel ChannelType => NotificationChannel.Teams;

    /// <summary>
    /// Initializes a new instance of the <see cref="TeamsChannel"/> class
    /// </summary>
    /// <param name="webhookClient">Webhook client for HTTP requests</param>
    /// <param name="logger">Logger instance</param>
    public TeamsChannel(IWebhookClient webhookClient, ILogger<TeamsChannel> logger)
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
    /// Sends a deployment notification to Microsoft Teams
    /// </summary>
    /// <param name="notification">The deployment notification to send</param>
    /// <param name="target">The Teams webhook target configuration</param>
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
            _logger.LogDebug("Preparing Teams notification for {ProjectName}", notification.ProjectName);

            // Build Teams-specific payload
            var payload = BuildTeamsPayload(notification, target);
            var payloadJson = payload.ToJsonString(new JsonSerializerOptions { WriteIndented = false });

            _logger.LogDebug("Sending Teams notification to {WebhookUrl}", target.WebhookUrl);

            // Send via webhook client
            var response = await _webhookClient.SendWebhookAsync(target.WebhookUrl, payloadJson, target.CustomHeaders, cancellationToken);

            if (response.IsSuccessful)
            {
                _logger.LogInformation("Successfully sent Teams notification for {ProjectName} v{Version}",
                    notification.ProjectName, notification.Version);
                return NotificationResult.Success((int)response.StatusCode);
            }

            var error = response.ErrorMessage ?? "Unknown error";
            _logger.LogError("Failed to send Teams notification: {Error}", error);
            return NotificationResult.Failure(error, (int)response.StatusCode);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Teams notification send operation was cancelled");
            return NotificationResult.Failure("Operation cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Teams notification for {ProjectName}", notification.ProjectName);
            return NotificationResult.Failure(ex.Message);
        }
    }

    private JsonObject BuildTeamsPayload(DeploymentNotification notification, NotificationTarget target)
    {
        // Determine color based on deployment status
        var statusColor = notification.Status switch
        {
            BuildStatus.DeploymentSuccess or BuildStatus.Success => "#00ff00", // Green
            BuildStatus.DeploymentFailed or BuildStatus.Failed => "#ff0000", // Red
            BuildStatus.Started or BuildStatus.InProgress => "#ffff00", // Yellow
            BuildStatus.Cancelled => "#ff9900", // Orange
            _ => "#0000ff" // Blue for unknown status
        };

        var statusEmoji = notification.Status switch
        {
            BuildStatus.DeploymentSuccess or BuildStatus.Success => "✅",
            BuildStatus.DeploymentFailed or BuildStatus.Failed => "❌",
            BuildStatus.Started or BuildStatus.InProgress => "🔄",
            BuildStatus.Cancelled => "⏹️",
            _ => "📝"
        };

        var statusText = notification.Status switch
        {
            BuildStatus.DeploymentSuccess or BuildStatus.Success => "Successfully deployed",
            BuildStatus.DeploymentFailed or BuildStatus.Failed => "Deployment failed",
            BuildStatus.Started or BuildStatus.InProgress => "Deployment in progress",
            BuildStatus.Cancelled => "Deployment cancelled",
            _ => notification.Status.ToString()
        };

        // Build facts array
        var facts = new JsonArray();

        facts.Add(new JsonObject
        {
            ["name"] = "Environment",
            ["value"] = notification.TargetEnvironment.ToString()
        });

        facts.Add(new JsonObject
        {
            ["name"] = "Branch",
            ["value"] = $"`{notification.BranchName}`"
        });

        facts.Add(new JsonObject
        {
            ["name"] = "Status",
            ["value"] = statusText
        });

        // Add commit information if enabled
        if (target.GetSetting("includeCommitDetails") != "false" && !string.IsNullOrWhiteSpace(notification.CommitHash))
        {
            facts.Add(new JsonObject
            {
                ["name"] = "Commit",
                ["value"] = $"`{notification.CommitHash[..8]}`"
            });
        }

        // Add duration if available
        if (notification.DurationSeconds.HasValue && notification.DurationSeconds > 0)
        {
            facts.Add(new JsonObject
            {
                ["name"] = "Duration",
                ["value"] = $"{notification.DurationSeconds.Value}s"
            });
        }

        // Build sections array
        var sections = new JsonArray
        {
            new JsonObject
            {
                ["activityTitle"] = $"{statusEmoji} {statusText}",
                ["activitySubtitle"] = notification.TargetEnvironment.ToString(),
                ["facts"] = facts,
                ["markdown"] = true
            }
        };

        // Build potential actions array
        var actions = new JsonArray();
        if (!string.IsNullOrWhiteSpace(notification.BuildUrl))
        {
            actions.Add(new JsonObject
            {
                ["@type"] = "OpenUri",
                ["name"] = "View Build Details",
                ["targets"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["os"] = "default",
                        ["uri"] = notification.BuildUrl
                    }
                }
            });
        }

        // Build main card payload
        var card = new JsonObject
        {
            ["@type"] = "MessageCard",
            ["@context"] = "https://schema.org/extensions",
            ["themeColor"] = statusColor,
            ["summary"] = $"Deployment: {notification.ProjectName} v{notification.Version}",
            ["title"] = $"🚀 {notification.ProjectName} v{notification.Version}",
            ["text"] = notification.Message,
            ["sections"] = sections
        };

        if (actions.Count > 0)
        {
            card["potentialAction"] = actions;
        }

        return new JsonObject { ["card"] = card };
    }
}