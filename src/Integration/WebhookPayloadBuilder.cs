#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Integration;

/// <summary>
/// Builds channel-specific webhook payloads from notifications
/// </summary>
public interface IWebhookPayloadBuilder
{
    string BuildPayload(DeploymentNotification notification);
}

/// <summary>
/// Builds Slack webhook payloads with rich formatting
/// </summary>
public class SlackWebhookPayloadBuilder : IWebhookPayloadBuilder
{
    public string BuildPayload(DeploymentNotification notification)
    {
        var color = notification.Status switch
        {
            BuildStatus.Success => "good",
            BuildStatus.Failed => "danger",
            BuildStatus.DeploymentSuccess => "good",
            BuildStatus.DeploymentFailed => "danger",
            BuildStatus.Cancelled => "warning",
            _ => "#808080"
        };

        var payload = new
        {
            text = notification.GetSummary(),
            attachments = new object[]
            {
                new
                {
                    color,
                    fields = BuildSlackFields(notification),
                    timestamp = notification.CreatedAt.ToUnixTimestamp(),
                    footer = "dotnet-deploy-notify",
                    footer_icon = "https://raw.githubusercontent.com/dotnet/docs/main/docs/core/media/logo.png"
                }
            }
        };

        return JsonSerializer.Serialize(payload);
    }

    private object[] BuildSlackFields(DeploymentNotification notification)
    {
        var fields = new List<object>
        {
            new { title = "Status", value = notification.Status.ToString(), @short = true },
            new { title = "Environment", value = notification.TargetEnvironment.ToString(), @short = true },
            new { title = "Version", value = notification.Version, @short = true },
            new { title = "Branch", value = notification.BranchName, @short = true },
            new { title = "Author", value = notification.CommitAuthor, @short = true },
            new { title = "Priority", value = notification.Priority.ToString(), @short = true }
        };

        if (!string.IsNullOrWhiteSpace(notification.Message))
            fields.Add(new { title = "Message", value = notification.Message });

        if (!string.IsNullOrWhiteSpace(notification.BuildUrl))
            fields.Add(new { title = "Build URL", value = notification.BuildUrl });

        return fields.ToArray();
    }
}

/// <summary>
/// Builds Discord webhook payloads with embeds
/// </summary>
public class DiscordWebhookPayloadBuilder : IWebhookPayloadBuilder
{
    public string BuildPayload(DeploymentNotification notification)
    {
        var color = notification.Status switch
        {
            BuildStatus.Success => 3066993, // Green
            BuildStatus.Failed => 15158332, // Red
            BuildStatus.DeploymentSuccess => 3066993,
            BuildStatus.DeploymentFailed => 15158332,
            BuildStatus.Cancelled => 16776960, // Yellow
            _ => 9807270 // Gray
        };

        var payload = new
        {
            username = "Deploy Notify",
            avatar_url = "https://raw.githubusercontent.com/dotnet/docs/main/docs/core/media/logo.png",
            embeds = new object[]
            {
                new
                {
                    title = notification.GetSummary(),
                    description = notification.Message,
                    color,
                    fields = BuildDiscordFields(notification),
                    timestamp = notification.CreatedAt.ToString("O"),
                    footer = new { text = "dotnet-deploy-notify" }
                }
            }
        };

        return JsonSerializer.Serialize(payload);
    }

    private object[] BuildDiscordFields(DeploymentNotification notification)
    {
        var fields = new List<object>
        {
            new { name = "Status", value = notification.Status.ToString(), inline = true },
            new { name = "Environment", value = notification.TargetEnvironment.ToString(), inline = true },
            new { name = "Version", value = notification.Version, inline = true },
            new { name = "Branch", value = notification.BranchName, inline = true },
            new { name = "Author", value = notification.CommitAuthor, inline = true },
            new { name = "Priority", value = notification.Priority.ToString(), inline = true }
        };

        if (!string.IsNullOrWhiteSpace(notification.BuildUrl))
            fields.Add(new { name = "Build URL", value = $"[View Build]({notification.BuildUrl})", inline = false });

        return fields.ToArray();
    }
}

/// <summary>
/// Builds Telegram webhook payloads with markdown formatting
/// </summary>
public class TelegramWebhookPayloadBuilder : IWebhookPayloadBuilder
{
    public string BuildPayload(DeploymentNotification notification)
    {
        var statusEmoji = notification.Status switch
        {
            BuildStatus.Success => "✅",
            BuildStatus.Failed => "❌",
            BuildStatus.DeploymentSuccess => "✅",
            BuildStatus.DeploymentFailed => "❌",
            BuildStatus.Cancelled => "⚠️",
            _ => "ℹ️"
        };

        var message = new StringBuilder();
        message.AppendLine($"{statusEmoji} *{notification.GetSummary()}*");
        message.AppendLine();
        message.AppendLine("*Details:*");
        message.AppendLine($"• Status: `{notification.Status}`");
        message.AppendLine($"• Environment: `{notification.TargetEnvironment}`");
        message.AppendLine($"• Version: `{notification.Version}`");
        message.AppendLine($"• Branch: `{notification.BranchName}`");
        message.AppendLine($"• Author: `{notification.CommitAuthor}`");
        message.AppendLine($"• Priority: `{notification.Priority}`");

        if (!string.IsNullOrWhiteSpace(notification.Message))
        {
            message.AppendLine();
            message.AppendLine("*Message:*");
            message.AppendLine(notification.Message);
        }

        if (!string.IsNullOrWhiteSpace(notification.BuildUrl))
        {
            message.AppendLine();
            message.AppendLine($"[View Build]({notification.BuildUrl})");
        }

        var payload = new
        {
            chat_id = notification.Metadata?.GetValueOrDefault("chatId") ?? "-1",
            text = message.ToString(),
            parse_mode = "Markdown"
        };

        return JsonSerializer.Serialize(payload);
    }
}

/// <summary>
/// Factory for creating webhook payload builders
/// </summary>
public class WebhookPayloadBuilderFactory
{
    public static IWebhookPayloadBuilder CreateBuilder(NotificationChannel channel)
    {
        return channel switch
        {
            NotificationChannel.Slack => new SlackWebhookPayloadBuilder(),
            NotificationChannel.Discord => new DiscordWebhookPayloadBuilder(),
            NotificationChannel.Telegram => new TelegramWebhookPayloadBuilder(),
            _ => throw new ArgumentException($"Unknown channel: {channel}")
        };
    }
}

/// <summary>
/// HTTP webhook client for sending notifications to external services
/// </summary>
public class WebhookClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookClient> _logger;
    private readonly int _maxRetries;

    public WebhookClient(HttpClient httpClient, ILogger<WebhookClient> logger, int maxRetries = 3)
    {
        _httpClient = httpClient;
        _logger = logger;
        _maxRetries = maxRetries;
    }

    /// <summary>
    /// Sends a webhook payload to the specified URL
    /// </summary>
    public async Task<WebhookResult> SendWebhookAsync(string webhookUrl, string payload)
    {
        var result = new WebhookResult { WebhookUrl = webhookUrl };

        try
        {
            var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(webhookUrl, content).ConfigureAwait(false);

            result.IsSuccessful = response.IsSuccessStatusCode;
            result.StatusCode = (int)response.StatusCode;
            result.Duration = DateTime.UtcNow;

            if (!response.IsSuccessStatusCode)
            {
                result.ErrorMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _logger.LogWarning("Webhook failed: {Url} returned {Status}", webhookUrl, response.StatusCode);
            }
            else
            {
                _logger.LogDebug("Webhook sent successfully: {Url}", webhookUrl);
            }
        }
        catch (Exception ex)
        {
            result.IsSuccessful = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Webhook error: {Url}", webhookUrl);
        }

        return result;
    }
}

/// <summary>
/// Result of webhook delivery attempt
/// </summary>
public class WebhookResult
{
    public string WebhookUrl { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public int StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Duration { get; set; }
}
