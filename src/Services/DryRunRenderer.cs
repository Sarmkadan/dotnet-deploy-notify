#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Services;

/// <summary>
/// The rendered result of a single channel in dry-run mode. Contains the payload
/// that <em>would</em> have been dispatched, without any network call taking place.
/// </summary>
public sealed class DryRunRenderResult
{
    /// <summary>Channel this render targets</summary>
    public NotificationChannel Channel { get; init; }

    /// <summary>Id of the channel configuration used to render</summary>
    public string ConfigurationId { get; init; } = string.Empty;

    /// <summary>Human-readable name of the channel configuration</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Destination endpoint, with any embedded token masked for safe logging</summary>
    public string TargetUrl { get; init; } = string.Empty;

    /// <summary>The fully rendered payload (JSON for webhook channels, HTML text for Telegram)</summary>
    public string RenderedPayload { get; init; } = string.Empty;

    /// <summary>
    /// True when this notification would actually be delivered had dry-run been off.
    /// False when channel filters (priority, environment, status) would suppress it.
    /// </summary>
    public bool WouldSend { get; init; }

    /// <summary>Explains why <see cref="WouldSend"/> is false, when applicable</summary>
    public string? SkipReason { get; init; }
}

/// <summary>
/// Renders deployment notifications into their channel-specific payloads without
/// dispatching them. Backs the <c>--dry-run</c> CLI flag and lets callers preview
/// exactly what would be sent to Slack, Telegram, Discord, or a generic webhook.
/// </summary>
public interface IDryRunRenderer
{
    /// <summary>Renders a single notification for one channel configuration</summary>
    DryRunRenderResult Render(DeploymentNotification notification, ChannelConfiguration config);

    /// <summary>Renders a notification against every supplied channel configuration</summary>
    IReadOnlyList<DryRunRenderResult> RenderAll(
        DeploymentNotification notification,
        IEnumerable<ChannelConfiguration> configs);
}

/// <summary>
/// Default implementation of <see cref="IDryRunRenderer"/> that reuses the same
/// <see cref="IPayloadBuilder"/> the live dispatcher relies on, so the preview is
/// byte-for-byte identical to what a real send would produce.
/// </summary>
public sealed class DryRunRenderer : IDryRunRenderer
{
    private readonly IPayloadBuilder _payloadBuilder;
    private readonly ILogger<DryRunRenderer> _logger;

    /// <summary>Initialises the renderer with its payload builder dependency</summary>
    public DryRunRenderer(IPayloadBuilder payloadBuilder, ILogger<DryRunRenderer> logger)
    {
        _payloadBuilder = payloadBuilder;
        _logger = logger;
    }

    /// <inheritdoc />
    public DryRunRenderResult Render(DeploymentNotification notification, ChannelConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(config);

        var wouldSend = config.ShouldSendNotification(notification);
        string? skipReason = wouldSend ? null : DescribeSkipReason(notification, config);

        // Telegram sends raw HTML text rather than a JSON envelope, so render it as text.
        // Every other channel goes through the shared webhook payload builder.
        var rendered = config.ChannelType == NotificationChannel.Telegram
            ? _payloadBuilder.BuildTelegramMessage(notification, config)
            : _payloadBuilder.BuildPayload(notification, config).ToJson();

        _logger.LogInformation(
            "[DRY-RUN] {Channel}/{Config}: would {Action} ({Bytes} bytes)",
            config.ChannelType,
            config.DisplayName,
            wouldSend ? "send" : "SKIP",
            rendered.Length);

        return new DryRunRenderResult
        {
            Channel = config.ChannelType,
            ConfigurationId = config.Id,
            DisplayName = config.DisplayName,
            TargetUrl = MaskUrl(config.WebhookUrl),
            RenderedPayload = rendered,
            WouldSend = wouldSend,
            SkipReason = skipReason
        };
    }

    /// <inheritdoc />
    public IReadOnlyList<DryRunRenderResult> RenderAll(
        DeploymentNotification notification,
        IEnumerable<ChannelConfiguration> configs)
    {
        ArgumentNullException.ThrowIfNull(configs);
        return configs.Select(c => Render(notification, c)).ToList();
    }

    private static string DescribeSkipReason(DeploymentNotification notification, ChannelConfiguration config)
    {
        if (!config.IsEnabled)
            return "channel is disabled";

        if (notification.Priority < config.MinimumPriority)
            return $"priority {notification.Priority} below minimum {config.MinimumPriority}";

        if (config.AllowedEnvironments.Any() && !config.AllowedEnvironments.Contains(notification.TargetEnvironment))
            return $"environment {notification.TargetEnvironment} not in allow-list";

        if (config.AllowedStatuses.Any() && !config.AllowedStatuses.Contains(notification.Status))
            return $"status {notification.Status} not in allow-list";

        return "filtered out";
    }

    /// <summary>
    /// Masks the token segment commonly embedded in bot / webhook URLs so previews
    /// can be logged or printed without leaking credentials.
    /// </summary>
    private static string MaskUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        // Telegram bot URLs look like .../bot<token>/sendMessage
        var botIndex = url.IndexOf("/bot", StringComparison.OrdinalIgnoreCase);
        if (botIndex >= 0)
        {
            var start = botIndex + 4;
            var end = url.IndexOf('/', start);
            if (end < 0) end = url.Length;
            return url[..start] + "***" + url[end..];
        }

        // For generic webhooks, keep the host and mask the last path segment.
        var lastSlash = url.LastIndexOf('/');
        if (lastSlash > 0 && lastSlash < url.Length - 1)
            return url[..(lastSlash + 1)] + "***";

        return url;
    }
}
