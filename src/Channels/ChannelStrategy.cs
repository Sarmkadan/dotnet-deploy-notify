#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Integration;

namespace DotNetDeployNotify.Channels;

/// <summary>
/// Strategy pattern for handling different notification channels
/// </summary>
public interface IChannelStrategy
{
    NotificationChannel Channel { get; }
    Task<bool> SendAsync(DeploymentNotification notification, ChannelConfiguration config, string payload);
    bool CanHandle(NotificationChannel channel);
}

/// <summary>
/// Base class for channel strategies
/// </summary>
public abstract class BaseChannelStrategy : IChannelStrategy
{
    protected readonly WebhookClient WebhookClient;
    protected readonly ILogger Logger;

    public abstract NotificationChannel Channel { get; }

    protected BaseChannelStrategy(WebhookClient webhookClient, ILogger logger)
    {
        WebhookClient = webhookClient;
        Logger = logger;
    }

    public virtual bool CanHandle(NotificationChannel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);
        return channel == Channel;
    }

    public abstract Task<bool> SendAsync(DeploymentNotification notification, ChannelConfiguration config, string payload);

    protected async Task<bool> SendPayloadAsync(string webhookUrl, string payload)
    {
        try
        {
            var result = await WebhookClient.SendWebhookAsync(webhookUrl, payload);
            return result.IsSuccessful;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error sending to webhook: {Url}", webhookUrl);
            return false;
        }
    }
}

/// <summary>
/// Slack channel strategy
/// </summary>
public class SlackChannelStrategy : BaseChannelStrategy
{
    public override NotificationChannel Channel => NotificationChannel.Slack;

    public SlackChannelStrategy(WebhookClient webhookClient, ILogger<SlackChannelStrategy> logger)
        : base(webhookClient, logger)
    {
    }

    public override async Task<bool> SendAsync(
        DeploymentNotification notification,
        ChannelConfiguration config,
        string payload)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(config);
        ArgumentException.ThrowIfNullOrEmpty(payload);
        Logger.LogDebug("Sending Slack notification for {ProjectName}", notification.ProjectName);
        return await SendPayloadAsync(config.WebhookUrl, payload);
    }
}

/// <summary>
/// Discord channel strategy
/// </summary>
public class DiscordChannelStrategy : BaseChannelStrategy
{
    public override NotificationChannel Channel => NotificationChannel.Discord;

    public DiscordChannelStrategy(WebhookClient webhookClient, ILogger<DiscordChannelStrategy> logger)
        : base(webhookClient, logger)
    {
    }

    public override async Task<bool> SendAsync(
        DeploymentNotification notification,
        ChannelConfiguration config,
        string payload)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(config);
        ArgumentException.ThrowIfNullOrEmpty(payload);
        Logger.LogDebug("Sending Discord notification for {ProjectName}", notification.ProjectName);
        return await SendPayloadAsync(config.WebhookUrl, payload);
    }
}

/// <summary>
/// Telegram channel strategy
/// </summary>
public class TelegramChannelStrategy : BaseChannelStrategy
{
    public override NotificationChannel Channel => NotificationChannel.Telegram;

    public TelegramChannelStrategy(WebhookClient webhookClient, ILogger<TelegramChannelStrategy> logger)
        : base(webhookClient, logger)
    {
    }

    public override async Task<bool> SendAsync(
        DeploymentNotification notification,
        ChannelConfiguration config,
        string payload)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(config);
        ArgumentException.ThrowIfNullOrEmpty(payload);
        Logger.LogDebug("Sending Telegram notification for {ProjectName}", notification.ProjectName);
        return await SendPayloadAsync(config.WebhookUrl, payload);
    }
}

/// <summary>
/// Channel strategy resolver
/// </summary>
public sealed class ChannelStrategyResolver
{
    private readonly Dictionary<NotificationChannel, IChannelStrategy> _strategies = new();
    private readonly ILogger<ChannelStrategyResolver> _logger;

    public ChannelStrategyResolver(ILogger<ChannelStrategyResolver> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers a channel strategy
    /// </summary>
    public void RegisterStrategy(IChannelStrategy strategy)
    {
        _strategies[strategy.Channel] = strategy;
        _logger.LogDebug("Registered channel strategy: {Channel}", strategy.Channel);
    }

    /// <summary>
    /// Gets strategy for a channel
    /// </summary>
    public IChannelStrategy? GetStrategy(NotificationChannel channel)
    {
        if (_strategies.TryGetValue(channel, out var strategy))
            return strategy;

        _logger.LogWarning("No strategy found for channel: {Channel}", channel);
        return null;
    }

    /// <summary>
    /// Gets all registered strategies
    /// </summary>
    public List<IChannelStrategy> GetAllStrategies() => _strategies.Values.ToList();

    /// <summary>
    /// Checks if a channel is supported
    /// </summary>
    public bool IsSupported(NotificationChannel channel) => _strategies.ContainsKey(channel);
}

/// <summary>
/// Channel adapter for backward compatibility
/// </summary>
public sealed class ChannelAdapter
{
    private readonly ChannelStrategyResolver _resolver;
    private readonly WebhookPayloadBuilderFactory _payloadBuilderFactory;
    private readonly ILogger<ChannelAdapter> _logger;

    public ChannelAdapter(
        ChannelStrategyResolver resolver,
        WebhookPayloadBuilderFactory payloadBuilderFactory,
        ILogger<ChannelAdapter> logger)
    {
        _resolver = resolver;
        _payloadBuilderFactory = payloadBuilderFactory;
        _logger = logger;
    }

    /// <summary>
    /// Sends a notification through a channel
    /// </summary>
    public async Task<bool> SendAsync(
        DeploymentNotification notification,
        ChannelConfiguration config)
    {
        var strategy = _resolver.GetStrategy(config.ChannelType);
        if (strategy is null)
        {
            _logger.LogError("No strategy available for channel: {Channel}", config.ChannelType);
            return false;
        }

        try
        {
            var payloadBuilder = WebhookPayloadBuilderFactory.CreateBuilder(config.ChannelType);
            var payload = payloadBuilder.BuildPayload(notification);

            _logger.LogDebug("Sending notification via {Channel}", config.ChannelType);
            return await strategy.SendAsync(notification, config, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification via {Channel}", config.ChannelType);
            return false;
        }
    }

    /// <summary>
    /// Gets supported channels
    /// </summary>
    public List<NotificationChannel> GetSupportedChannels()
    {
        return _resolver.GetAllStrategies()
            .Select(s => s.Channel)
            .ToList();
    }
}
