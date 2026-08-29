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
/// Slack notification channel implementation
/// </summary>
public sealed class SlackChannel : INotificationChannel
{
    private static class Constants
    {
        public const string ChannelName = "Slack";
        public const string SuccessColor = "#00ff00";
        public const string FailureColor = "#ff0000";
        public const string InProgressColor = "#ffff00";
        public const string CancelledColor = "#ff9900";
        public const string UnknownStatusColor = "#0000ff";
        public const string SuccessEmoji = "✅";
        public const string FailureEmoji = "❌";
        public const string InProgressEmoji = "🔄";
        public const string CancelledEmoji = "⏹️";
        public const string UnknownStatusEmoji = "📝";
        public const string CommitTitle = "Commit";
        public const string RepositoryTitle = "Repository";
        public const string DurationTitle = "Duration";
        public const string ViewBuildTitle = "View Build";
        public const string TypeField = "type";
        public const string TextField = "text";
        public const string TitleField = "title";
        public const string ValueField = "value";
        public const string ShortField = "short";
        public const string FieldsField = "fields";
        public const string EmojiField = "emoji";
        public const string ElementsField = "elements";
        public const string MarkdownType = "mrkdwn";
        public const string PlainTextType = "plain_text";
        public const string SectionType = "section";
        public const string IncludeCommitDetailsSetting = "includeCommitDetails";
        public const string UseSlackBlockKitSetting = "useSlackBlockKit";
        public const string TrueValue = "true";
        public const string FalseValue = "false";
    }

    private readonly ILogger<SlackChannel> _logger;
    private readonly IWebhookClient _webhookClient;

    /// <summary>
    /// Gets the name of the Slack channel
    /// </summary>
    public string Name => Constants.ChannelName;

    /// <summary>
    /// Gets the channel type this instance handles
    /// </summary>
    public NotificationChannel ChannelType => NotificationChannel.Slack;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlackChannel"/> class
    /// </summary>
    /// <param name="webhookClient">Webhook client for HTTP requests</param>
    /// <param name="logger">Logger instance</param>
    public SlackChannel(IWebhookClient webhookClient, ILogger<SlackChannel> logger)
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
    /// Sends a deployment notification to Slack
    /// </summary>
    /// <param name="notification">The deployment notification to send</param>
    /// <param name="target">The Slack webhook target configuration</param>
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
            _logger.LogDebug("Preparing Slack notification for {ProjectName}", notification.ProjectName);

            // Build Slack-specific payload
            var payload = BuildSlackPayload(notification, target);
            var payloadJson = payload.ToJsonString(new JsonSerializerOptions { WriteIndented = false });

            _logger.LogDebug("Sending Slack notification to {WebhookUrl}", target.WebhookUrl);

            // Send via webhook client
            var response = await _webhookClient.SendWebhookAsync(target.WebhookUrl, payloadJson, target.CustomHeaders, cancellationToken);

            if (response.IsSuccessful)
            {
                _logger.LogInformation("Successfully sent Slack notification for {ProjectName} v{Version}",
                    notification.ProjectName, notification.Version);
                return NotificationResult.Success((int)response.StatusCode);
            }

            var error = response.ErrorMessage ?? "Unknown error";
            _logger.LogError("Failed to send Slack notification: {Error}", error);
            return NotificationResult.Failure(error, (int)response.StatusCode);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Slack notification send operation was cancelled");
            return NotificationResult.Failure("Operation cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Slack notification for {ProjectName}", notification.ProjectName);
            return NotificationResult.Failure(ex.Message);
        }
    }

    private JsonObject BuildSlackPayload(DeploymentNotification notification, NotificationTarget target)
    {
        // Determine color based on deployment status
        var statusColor = notification.Status switch
        {
            BuildStatus.DeploymentSuccess or BuildStatus.Success => Constants.SuccessColor, // Green
            BuildStatus.DeploymentFailed or BuildStatus.Failed => Constants.FailureColor, // Red
            BuildStatus.Started or BuildStatus.InProgress => Constants.InProgressColor, // Yellow
            BuildStatus.Cancelled => Constants.CancelledColor, // Orange
            _ => Constants.UnknownStatusColor // Blue for unknown status
        };

        // Build main attachment
        var attachment = new JsonObject
        {
            ["color"] = statusColor,
            [Constants.TitleField] = $"🚀 Deployment: {notification.ProjectName} v{notification.Version}",
            ["title_link"] = notification.BuildUrl,
            [Constants.TextField] = notification.Message,
            ["ts"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        // Add fields if commit details are enabled
        var fields = new JsonArray();

        if (target.GetSetting(Constants.IncludeCommitDetailsSetting) != Constants.FalseValue && !string.IsNullOrWhiteSpace(notification.CommitHash))
        {
            fields.Add(new JsonObject
            {
                [Constants.TitleField] = Constants.CommitTitle,
                [Constants.ValueField] = $"`{notification.CommitHash[..8]}` - {notification.CommitAuthor}",
                [Constants.ShortField] = true
            });
        }

        if (!string.IsNullOrWhiteSpace(notification.RepositoryUrl))
        {
            fields.Add(new JsonObject
            {
                [Constants.TitleField] = Constants.RepositoryTitle,
                [Constants.ValueField] = notification.RepositoryUrl,
                [Constants.ShortField] = true
            });
        }

        if (notification.DurationSeconds.HasValue && notification.DurationSeconds > 0)
        {
            fields.Add(new JsonObject
            {
                [Constants.TitleField] = Constants.DurationTitle,
                [Constants.ValueField] = $"{notification.DurationSeconds.Value}s",
                [Constants.ShortField] = true
            });
        }

        if (fields.Count > 0)
        {
            attachment[Constants.FieldsField] = fields;
        }

        // Add footer with environment
        attachment["footer"] = $"Environment: {notification.TargetEnvironment}";

        // Build main payload
        var payload = new JsonObject
        {
            [Constants.TextField] = $"*Deployment Notification*\n{notification.GetSummary()}",
            ["attachments"] = new JsonArray { attachment }
        };

        // Add Slack Block Kit format if enabled in settings
        if (target.GetSetting(Constants.UseSlackBlockKitSetting) == Constants.TrueValue)
        {
            payload = BuildSlackBlockKitPayload(notification, target, statusColor);
        }

        return payload;
    }

    private JsonObject BuildSlackBlockKitPayload(
        DeploymentNotification notification,
        NotificationTarget target,
        string statusColor)
    {
        var blocks = new JsonArray();

        // Header block
        blocks.Add(new JsonObject
        {
            [Constants.TypeField] = "header",
            [Constants.TextField] = new JsonObject
            {
                [Constants.TypeField] = Constants.PlainTextType,
                [Constants.TextField] = $"🚀 {notification.ProjectName} v{notification.Version}",
                [Constants.EmojiField] = true
            }
        });

        // Section block with status
        var statusEmoji = notification.Status switch
        {
            BuildStatus.DeploymentSuccess or BuildStatus.Success => Constants.SuccessEmoji,
            BuildStatus.DeploymentFailed or BuildStatus.Failed => Constants.FailureEmoji,
            BuildStatus.Started or BuildStatus.InProgress => Constants.InProgressEmoji,
            BuildStatus.Cancelled => Constants.CancelledEmoji,
            _ => Constants.UnknownStatusEmoji
        };

        var statusText = notification.Status switch
        {
            BuildStatus.DeploymentSuccess or BuildStatus.Success => "Successfully deployed",
            BuildStatus.DeploymentFailed or BuildStatus.Failed => "Deployment failed",
            BuildStatus.Started or BuildStatus.InProgress => "Deployment in progress",
            BuildStatus.Cancelled => "Deployment cancelled",
            _ => notification.Status.ToString()
        };

        blocks.Add(new JsonObject
        {
            [Constants.TypeField] = Constants.SectionType,
            [Constants.TextField] = new JsonObject
            {
                [Constants.TypeField] = Constants.MarkdownType,
                [Constants.TextField] = $"*{statusEmoji} {statusText}*\n{notification.Message}"
            }
        });

        // Fields block
        var fieldsBlock = new JsonObject
        {
            [Constants.TypeField] = Constants.SectionType,
            [Constants.FieldsField] = new JsonArray()
        };

        var fields = (JsonArray)fieldsBlock[Constants.FieldsField];

        fields.Add(new JsonObject
        {
            [Constants.TypeField] = Constants.MarkdownType,
            [Constants.TextField] = $"*Environment:*\n{notification.TargetEnvironment}"
        });

        fields.Add(new JsonObject
        {
            [Constants.TypeField] = Constants.MarkdownType,
            [Constants.TextField] = $"*Branch:*\n`{notification.BranchName}`"
        });

        if (!string.IsNullOrWhiteSpace(notification.CommitHash))
        {
            fields.Add(new JsonObject
            {
                [Constants.TypeField] = Constants.MarkdownType,
                [Constants.TextField] = $"*Commit:*\n`{notification.CommitHash[..8]}`"
            });

            fields.Add(new JsonObject
            {
                [Constants.TypeField] = Constants.MarkdownType,
                [Constants.TextField] = $"*Author:*\n{notification.CommitAuthor}"
            });
        }

        blocks.Add(fieldsBlock);

        // Actions block with link to build
        if (!string.IsNullOrWhiteSpace(notification.BuildUrl))
        {
            blocks.Add(new JsonObject
            {
                [Constants.TypeField] = "actions",
                [Constants.ElementsField] = new JsonArray
                {
                    new JsonObject
                    {
                        [Constants.TypeField] = "button",
                        [Constants.TextField] = new JsonObject
                        {
                            [Constants.TypeField] = Constants.PlainTextType,
                            [Constants.TextField] = Constants.ViewBuildTitle,
                            [Constants.EmojiField] = true
                        },
                        ["url"] = notification.BuildUrl,
                        ["style"] = "primary"
                    }
                }
            });
        }

        // Context block with timestamp
        blocks.Add(new JsonObject
        {
            [Constants.TypeField] = "context",
            [Constants.ElementsField] = new JsonArray
            {
                new JsonObject
                {
                    [Constants.TypeField] = Constants.MarkdownType,
                    [Constants.TextField] = $"📅 {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"
                }
            }
        });

        return new JsonObject
        {
            ["blocks"] = blocks
        };
    }
}
