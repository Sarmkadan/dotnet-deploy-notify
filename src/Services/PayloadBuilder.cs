#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Formatting;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Services;

/// <summary>
/// Interface for building notification payloads for different channels
/// </summary>
public interface IPayloadBuilder
{
    /// <summary>Builds a webhook payload from notification and channel config</summary>
    WebhookPayload BuildPayload(DeploymentNotification notification, ChannelConfiguration config);

    /// <summary>Builds a Telegram-formatted message</summary>
    string BuildTelegramMessage(DeploymentNotification notification, ChannelConfiguration config);

    /// <summary>Builds a Slack-formatted message</summary>
    object BuildSlackPayload(DeploymentNotification notification, ChannelConfiguration config);

    /// <summary>Builds a Discord-formatted message</summary>
    object BuildDiscordPayload(DeploymentNotification notification, ChannelConfiguration config);
}

/// <summary>
/// Implementation of payload builder for various notification channels
/// </summary>
public class PayloadBuilder : IPayloadBuilder
{
    private readonly ILogger<PayloadBuilder> _logger;

    /// <summary>Initializes the payload builder</summary>
    public PayloadBuilder(ILogger<PayloadBuilder> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Builds a webhook payload with channel-specific formatting
    /// </summary>
    public WebhookPayload BuildPayload(DeploymentNotification notification, ChannelConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(config);
        var payload = new WebhookPayload
        {
            EventType = $"deployment.{notification.Status.ToString().ToLower()}",
            Data = WebhookData.FromNotification(notification)
        };

        // Add channel-specific formatting if needed
        if (config.ChannelType == NotificationChannel.Slack)
        {
            payload.Data.CustomProperties["slack_format"] = BuildSlackPayload(notification, config);
        }
        else if (config.ChannelType == NotificationChannel.Discord)
        {
            payload.Data.CustomProperties["discord_format"] = BuildDiscordPayload(notification, config);
        }
        else if (config.ChannelType == NotificationChannel.Telegram)
        {
            payload.Data.CustomProperties["telegram_text"] = BuildTelegramMessage(notification, config);
        }

        return payload;
    }

    /// <summary>
    /// Builds a Telegram-formatted text message
    /// </summary>
    public string BuildTelegramMessage(DeploymentNotification notification, ChannelConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(config);
        var emoji = StatusEmoji.Get(notification.Status, config.EnableEmojis);
        var titleEmoji = string.IsNullOrEmpty(emoji) ? "" : $"{emoji} ";
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"{titleEmoji}<b>{notification.ProjectName}</b> v{notification.Version}");
        sb.AppendLine($"<b>Status:</b> {StatusEmoji.Format(notification.Status, config.EnableEmojis)}");
        sb.AppendLine($"<b>Environment:</b> {notification.TargetEnvironment}");
        sb.AppendLine($"<b>Branch:</b> <code>{notification.BranchName}</code>");

        if (config.IncludeCommitDetails && !string.IsNullOrWhiteSpace(notification.CommitHash))
        {
            var shortHash = notification.CommitHash[..Math.Min(7, notification.CommitHash.Length)];
            sb.AppendLine($"<b>Commit:</b> <code>{shortHash}</code>");
            if (!string.IsNullOrWhiteSpace(notification.CommitAuthor))
                sb.AppendLine($"<b>Author:</b> {notification.CommitAuthor}");
        }

        sb.AppendLine($"\n<b>Message:</b>\n{notification.Message}");

        if (notification.DurationSeconds.HasValue)
            sb.AppendLine($"\n⏱️ <b>Duration:</b> {notification.DurationSeconds} seconds");

        if (config.IncludeBuildUrl && !string.IsNullOrWhiteSpace(notification.BuildUrl))
            sb.AppendLine($"\n<a href=\"{notification.BuildUrl}\">View Build</a>");

        return sb.ToString();
    }

    /// <summary>
    /// Builds a Slack-formatted message payload.
    /// Uses Block Kit rich layout when <see cref="ChannelConfiguration.UseSlackBlockKit"/> is true,
    /// otherwise falls back to the legacy attachments format.
    /// </summary>
    public object BuildSlackPayload(DeploymentNotification notification, ChannelConfiguration config)
    {
        if (config.UseSlackBlockKit)
            return BuildSlackBlockKitPayload(notification, config);

        var color = GetStatusColor(notification.Status);
        var emoji = StatusEmoji.Get(notification.Status, config.EnableEmojis);
        var titlePrefix = string.IsNullOrEmpty(emoji) ? "" : $"{emoji} ";

        var payload = new
        {
            attachments = new[]
            {
                new
                {
                    color = color,
                    title = $"{titlePrefix}{notification.ProjectName} v{notification.Version}",
                    title_link = config.IncludeBuildUrl ? notification.BuildUrl : null,
                    fields = BuildSlackFields(notification, config),
                    ts = ((DateTimeOffset)notification.CreatedAt).ToUnixTimeSeconds()
                }
            }
        };

        return payload;
    }

    /// <summary>
    /// Builds a Slack Block Kit payload with sections, dividers, and context blocks.
    /// </summary>
    private object BuildSlackBlockKitPayload(DeploymentNotification notification, ChannelConfiguration config)
    {
        var emoji = StatusEmoji.Get(notification.Status, config.EnableEmojis);
        var titlePrefix = string.IsNullOrEmpty(emoji) ? "" : $"{emoji} ";
        var blocks = new List<object>
        {
            new
            {
                type = "header",
                text = new { type = "plain_text", text = $"{titlePrefix}{notification.ProjectName} v{notification.Version}", emoji = true }
            },
            new { type = "divider" },
            new
            {
                type = "section",
                fields = new object[]
                {
                    new { type = "mrkdwn", text = $"*Status*\n{StatusEmoji.Format(notification.Status, config.EnableEmojis)}" },
                    new { type = "mrkdwn", text = $"*Environment*\n{notification.TargetEnvironment}" },
                    new { type = "mrkdwn", text = $"*Branch*\n`{notification.BranchName}`" },
                    new { type = "mrkdwn", text = $"*Priority*\n{notification.Priority}" }
                }
            }
        };

        if (config.IncludeCommitDetails && !string.IsNullOrWhiteSpace(notification.CommitHash))
        {
            var shortHash = notification.CommitHash[..Math.Min(7, notification.CommitHash.Length)];
            blocks.Add(new
            {
                type = "section",
                fields = new object[]
                {
                    new { type = "mrkdwn", text = $"*Commit*\n`{shortHash}`" },
                    new { type = "mrkdwn", text = $"*Author*\n{notification.CommitAuthor}" }
                }
            });
        }

        if (!string.IsNullOrWhiteSpace(notification.Message))
        {
            blocks.Add(new
            {
                type = "section",
                text = new { type = "mrkdwn", text = $"*Message*\n{notification.Message}" }
            });
        }

        if (notification.DurationSeconds.HasValue)
        {
            blocks.Add(new
            {
                type = "context",
                elements = new object[]
                {
                    new { type = "mrkdwn", text = $"⏱️ Duration: {notification.DurationSeconds}s" }
                }
            });
        }

        if (config.IncludeBuildUrl && !string.IsNullOrWhiteSpace(notification.BuildUrl))
        {
            blocks.Add(new
            {
                type = "actions",
                elements = new object[]
                {
                    new
                    {
                        type = "button",
                        text = new { type = "plain_text", text = "View Build", emoji = true },
                        url = notification.BuildUrl,
                        action_id = "view_build"
                    }
                }
            });
        }

        return new { blocks = blocks.ToArray() };
    }

    /// <summary>
    /// Builds a Discord-formatted message payload
    /// </summary>
    public object BuildDiscordPayload(DeploymentNotification notification, ChannelConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(config);
        // Discord embed description is limited to 4096 characters
        const int MaxEmbedDescriptionLength = 4096;

        var color = GetDiscordStatusColor(notification.Status);
        var emoji = StatusEmoji.Get(notification.Status, config.EnableEmojis);
        var titlePrefix = string.IsNullOrEmpty(emoji) ? "" : $"{emoji} ";

        var description = notification.Message;
        if (!string.IsNullOrEmpty(description) && description.Length > MaxEmbedDescriptionLength)
            description = description[..(MaxEmbedDescriptionLength - 3)] + "...";

        var embed = new
        {
            title = $"{titlePrefix}{notification.ProjectName} v{notification.Version}",
            description = description,
            color = color,
            fields = BuildDiscordFields(notification, config),
            url = config.IncludeBuildUrl ? notification.BuildUrl : null,
            timestamp = notification.CreatedAt.ToUniversalTime().ToString("O")
        };

        return new { embeds = new[] { embed } };
    }

    /// <summary>
    /// Builds field array for Slack messages
    /// </summary>
    private object[] BuildSlackFields(DeploymentNotification notification, ChannelConfiguration config)
    {
        var fields = new List<object>
        {
            new { title = "Status", value = StatusEmoji.Format(notification.Status, config.EnableEmojis), @short = true },
            new { title = "Environment", value = notification.TargetEnvironment.ToString(), @short = true },
            new { title = "Branch", value = notification.BranchName, @short = true },
            new { title = "Priority", value = notification.Priority.ToString(), @short = true }
        };

        if (config.IncludeCommitDetails && !string.IsNullOrWhiteSpace(notification.CommitHash))
        {
            var shortHash = notification.CommitHash[..Math.Min(7, notification.CommitHash.Length)];
            fields.Add(new { title = "Commit", value = shortHash, @short = true });
            if (!string.IsNullOrWhiteSpace(notification.CommitAuthor))
                fields.Add(new { title = "Author", value = notification.CommitAuthor, @short = true });
        }

        if (notification.DurationSeconds.HasValue)
            fields.Add(new { title = "Duration", value = $"{notification.DurationSeconds}s", @short = true });

        return fields.ToArray();
    }

    /// <summary>
    /// Builds field array for Discord messages
    /// </summary>
    private object[] BuildDiscordFields(DeploymentNotification notification, ChannelConfiguration config)
    {
        var fields = new List<object>
        {
            new { name = "Status", value = StatusEmoji.Format(notification.Status, config.EnableEmojis), inline = true },
            new { name = "Environment", value = notification.TargetEnvironment.ToString(), inline = true },
            new { name = "Branch", value = $"`{notification.BranchName}`", inline = true }
        };

        if (config.IncludeCommitDetails && !string.IsNullOrWhiteSpace(notification.CommitHash))
        {
            var shortHash = notification.CommitHash[..Math.Min(7, notification.CommitHash.Length)];
            fields.Add(new { name = "Commit", value = $"`{shortHash}`", inline = true });
            if (!string.IsNullOrWhiteSpace(notification.CommitAuthor))
                fields.Add(new { name = "Author", value = notification.CommitAuthor, inline = true });
        }

        if (notification.DurationSeconds.HasValue)
            fields.Add(new { name = "Duration", value = $"{notification.DurationSeconds}s", inline = true });

        return fields.ToArray();
    }

    /// <summary>
    /// Gets a Slack color code for status
    /// </summary>
    private static string GetStatusColor(BuildStatus status) => status switch
    {
        BuildStatus.Success => "#36a64f",
        BuildStatus.SuccessWithWarnings => "#ff9900",
        BuildStatus.Failed => "#ff0000",
        BuildStatus.DeploymentSuccess => "#00ff00",
        BuildStatus.DeploymentFailed => "#ff0000",
        BuildStatus.Deploying => "#0099ff",
        _ => "#808080"
    };

    /// <summary>
    /// Gets a Discord color code (hex to decimal) for status
    /// </summary>
    private static int GetDiscordStatusColor(BuildStatus status) => status switch
    {
        BuildStatus.Success => 3394575,      // #36a64f
        BuildStatus.SuccessWithWarnings => 16750848,  // #ff9900
        BuildStatus.Failed => 16711680,     // #ff0000
        BuildStatus.DeploymentSuccess => 65280,       // #00ff00
        BuildStatus.DeploymentFailed => 16711680,    // #ff0000
        BuildStatus.Deploying => 39423,     // #0099ff
        _ => 8421504                         // #808080
    };
}
