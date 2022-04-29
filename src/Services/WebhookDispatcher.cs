// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Exceptions;
using DotNetDeployNotify.Core.Models;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Services;

/// <summary>
/// Interface for dispatching notifications to webhook endpoints
/// </summary>
public interface IWebhookDispatcher
{
    /// <summary>Sends a notification to a webhook endpoint</summary>
    Task<NotificationResult> SendToWebhookAsync(ChannelConfiguration config, DeploymentNotification notification);

    /// <summary>Sends raw payload to a webhook URL</summary>
    Task<NotificationResult> SendPayloadAsync(string webhookUrl, WebhookPayload payload, Dictionary<string, string> headers, int timeoutMs);

    /// <summary>Validates webhook connectivity</summary>
    Task<bool> ValidateWebhookAsync(string webhookUrl, int timeoutMs);
}

/// <summary>
/// Implementation of webhook dispatcher using HttpClient
/// </summary>
public class WebhookDispatcher : IWebhookDispatcher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookDispatcher> _logger;
    private readonly IPayloadBuilder _payloadBuilder;

    /// <summary>Initializes the webhook dispatcher with dependencies</summary>
    public WebhookDispatcher(
        HttpClient httpClient,
        ILogger<WebhookDispatcher> logger,
        IPayloadBuilder payloadBuilder)
    {
        _httpClient = httpClient;
        _logger = logger;
        _payloadBuilder = payloadBuilder;
    }

    /// <summary>
    /// Sends a notification to the configured webhook endpoint
    /// </summary>
    public async Task<NotificationResult> SendToWebhookAsync(
        ChannelConfiguration config,
        DeploymentNotification notification)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Validate configuration
            if (!config.IsValid())
            {
                throw new ChannelConfigurationException(
                    "Channel configuration is invalid",
                    config.ChannelType,
                    config.Id);
            }

            // Build the appropriate payload based on channel type
            var payload = _payloadBuilder.BuildPayload(notification, config);

            if (!payload.IsValid())
            {
                throw new NotificationException("Payload validation failed");
            }

            // Send the webhook
            var result = await SendPayloadAsync(
                config.WebhookUrl,
                payload,
                config.CustomHeaders,
                config.TimeoutMs);

            stopwatch.Stop();
            result.DurationMs = stopwatch.ElapsedMilliseconds;
            result.ConfigurationId = config.Id;
            result.Channel = config.ChannelType;

            _logger.LogInformation(
                "Webhook sent to {Channel} for {Project} v{Version}: {Status}",
                config.ChannelType,
                notification.ProjectName,
                notification.Version,
                result.Status);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Failed to send webhook to {Channel} for {Project}: {Message}",
                config.ChannelType,
                notification.ProjectName,
                ex.Message);

            return NotificationResult.CreateFailure(
                notification.Id,
                config.ChannelType,
                config.Id,
                ex.Message,
                ex.GetType().Name);
        }
    }

    /// <summary>
    /// Sends a raw JSON payload to a webhook URL with retry logic
    /// </summary>
    public async Task<NotificationResult> SendPayloadAsync(
        string webhookUrl,
        WebhookPayload payload,
        Dictionary<string, string> headers,
        int timeoutMs)
    {
        var result = new NotificationResult
        {
            NotificationId = payload.EventId,
            Status = DeliveryStatus.Pending
        };

        try
        {
            var content = new StringContent(
                payload.ToJson(),
                System.Text.Encoding.UTF8,
                "application/json");

            // Add custom headers
            foreach (var header in headers ?? new Dictionary<string, string>())
            {
                if (!content.Headers.Contains(header.Key))
                {
                    content.Headers.Add(header.Key, header.Value);
                }
            }

            // Create a timeout token
            using var cts = new CancellationTokenSource(timeoutMs);

            // Send the POST request
            var response = await _httpClient.PostAsync(webhookUrl, content, cts.Token);

            result.HttpStatusCode = (int)response.StatusCode;
            result.ResponseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                result.MarkAsSuccessful((int)response.StatusCode, result.ResponseBody);
            }
            else
            {
                result.MarkAsFailed(
                    $"HTTP {response.StatusCode}: {result.ResponseBody}",
                    null,
                    (int)response.StatusCode);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Webhook request to {Url} timed out after {TimeoutMs}ms", webhookUrl, timeoutMs);
            result.MarkAsTimeout();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending webhook to {Url}", webhookUrl);
            result.MarkAsFailed(ex.Message, ex.GetType().Name);
            return result;
        }
    }

    /// <summary>
    /// Tests webhook connectivity with a simple health check
    /// </summary>
    public async Task<bool> ValidateWebhookAsync(string webhookUrl, int timeoutMs)
    {
        try
        {
            var testPayload = new WebhookPayload
            {
                EventType = "health_check",
                Data = new WebhookData
                {
                    ProjectName = "health-check",
                    Version = "1.0.0",
                    Status = "test"
                }
            };

            var result = await SendPayloadAsync(
                webhookUrl,
                testPayload,
                new Dictionary<string, string>(),
                timeoutMs);

            return result.IsSuccessful;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Webhook validation failed for {Url}", webhookUrl);
            return false;
        }
    }
}
