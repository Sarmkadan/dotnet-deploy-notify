#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetDeployNotify.Notifications;

/// <summary>
/// Sends deployment notifications to Slack via incoming webhooks.
/// </summary>
public sealed class SlackNotifier : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SlackNotifier> _logger;
    private readonly SlackNotifierOptions _options;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlackNotifier"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client for webhook calls.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="options">Slack configuration options.</param>
    public SlackNotifier(
        HttpClient httpClient,
        ILogger<SlackNotifier> logger,
        IOptions<SlackNotifierOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Sends a plain text message to the configured Slack channel.
    /// </summary>
    /// <param name="message">Message text (supports Slack mrkdwn).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var payload = new { text = message, channel = _options.Channel };
        await PostPayloadAsync(payload, cancellationToken);
    }

    /// <summary>
    /// Sends a deployment status notification with structured formatting.
    /// </summary>
    /// <param name="deploymentId">Deployment identifier.</param>
    /// <param name="environment">Target environment.</param>
    /// <param name="status">Current status text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SendDeploymentNotificationAsync(
        string deploymentId,
        string environment,
        string status,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            channel = _options.Channel,
            blocks = new object[]
            {
                new { type = "header", text = new { type = "plain_text", text = $"Deployment {status}" } },
                new
                {
                    type = "section",
                    fields = new[]
                    {
                        new { type = "mrkdwn", text = $"*Deployment:*\n`{deploymentId}`" },
                        new { type = "mrkdwn", text = $"*Environment:*\n{environment}" },
                        new { type = "mrkdwn", text = $"*Status:*\n{status}" },
                        new { type = "mrkdwn", text = $"*Time:*\n{DateTimeOffset.UtcNow:u}" }
                    }
                }
            }
        };

        await PostPayloadAsync(payload, cancellationToken);
        _logger.LogInformation("Sent Slack notification for deployment {DeploymentId} ({Status})",
            deploymentId, status);
    }

    /// <summary>
    /// Sends an error alert to the configured alert channel.
    /// </summary>
    /// <param name="title">Alert title.</param>
    /// <param name="errorDetails">Error details text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SendErrorAlertAsync(string title, string errorDetails, CancellationToken cancellationToken = default)
    {
        var webhookUrl = _options.AlertWebhookUrl ?? _options.WebhookUrl;
        var payload = new { text = $":rotating_light: *{title}*\n```{errorDetails}```" };

        var response = await _httpClient.PostAsJsonAsync(webhookUrl, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        _logger.LogWarning("Sent error alert to Slack: {Title}", title);
    }

    private async Task PostPayloadAsync(object payload, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(_options.WebhookUrl, payload, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Slack webhook returned {StatusCode}: {Body}", response.StatusCode, body);
            response.EnsureSuccessStatusCode();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Configuration options for <see cref="SlackNotifier"/>.
/// </summary>
public sealed class SlackNotifierOptions
{
    /// <summary>Slack incoming webhook URL.</summary>
    public required string WebhookUrl { get; set; }

    /// <summary>Optional separate webhook URL for error alerts.</summary>
    public string? AlertWebhookUrl { get; set; }

    /// <summary>Default channel override (if webhook supports it).</summary>
    public string? Channel { get; set; }
}
