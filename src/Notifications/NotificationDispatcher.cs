#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core.Models;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Notifications;

/// <summary>
/// Dispatcher that resolves and uses appropriate notification channels for sending notifications
/// </summary>
public sealed class NotificationDispatcher
{
    private readonly ILogger<NotificationDispatcher> _logger;
    private readonly IEnumerable<INotificationChannel> _channels;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationDispatcher"/> class
    /// </summary>
    /// <param name="channels">Available notification channels</param>
    /// <param name="logger">Logger instance</param>
    public NotificationDispatcher(
        IEnumerable<INotificationChannel> channels,
        ILogger<NotificationDispatcher> logger)
    {
        ArgumentNullException.ThrowIfNull(channels);
        ArgumentNullException.ThrowIfNull(logger);

        _channels = channels;
        _logger = logger;
    }

    /// <summary>
    /// Sends a notification to all configured channels
    /// </summary>
    /// <param name="notification">The deployment notification to send</param>
    /// <param name="targets">Collection of notification targets (one per channel)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of notification results</returns>
    public async Task<IEnumerable<NotificationResult>> SendToChannelsAsync(
        DeploymentNotification notification,
        IEnumerable<NotificationTarget> targets,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(targets);

        var results = new List<NotificationResult>();
        var targetsList = targets.ToList();

        if (!targetsList.Any())
        {
            _logger.LogWarning("No notification targets provided");
            return results;
        }

        _logger.LogDebug("Sending notification to {Count} channels", targetsList.Count);

        foreach (var target in targetsList)
        {
            try
            {
                var result = await SendToChannelAsync(notification, target, cancellationToken);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to channel");
                results.Add(NotificationResult.Failure(ex.Message));
            }
        }

        return results;
    }

    /// <summary>
    /// Sends a notification to a specific channel
    /// </summary>
    /// <param name="notification">The deployment notification to send</param>
    /// <param name="target">The notification target configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Notification result</returns>
    public async Task<NotificationResult> SendToChannelAsync(
        DeploymentNotification notification,
        NotificationTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentNullException.ThrowIfNull(target);

        _logger.LogDebug("Finding appropriate channel for target type: {ChannelType}", target.GetType().Name);

        // Find all channels that can handle this target
        var suitableChannels = _channels
            .Where(c => c.CanHandle(target))
            .ToList();

        if (!suitableChannels.Any())
        {
            _logger.LogWarning("No suitable notification channel found for target");
            return NotificationResult.Failure("No suitable notification channel found");
        }

        _logger.LogDebug("Found {Count} suitable channels", suitableChannels.Count);

        // Try each suitable channel until one succeeds
        foreach (var channel in suitableChannels)
        {
            _logger.LogDebug("Attempting to send via {ChannelName} ({ChannelType})",
                channel.Name, channel.ChannelType);

            try
            {
                var result = await channel.SendAsync(notification, target, cancellationToken);

                if (result.IsSuccessful)
                {
                    _logger.LogInformation("Successfully sent notification via {ChannelName}", channel.Name);
                    return result;
                }

                _logger.LogWarning("Channel {ChannelName} failed to send: {Error}",
                    channel.Name, result.ErrorMessage ?? "Unknown error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in channel {ChannelName}", channel.Name);
            }
        }

        _logger.LogError("All suitable channels failed to send notification");
        return NotificationResult.Failure("All channels failed to send notification");
    }

    /// <summary>
    /// Gets all available notification channel types
    /// </summary>
    /// <returns>List of channel types</returns>
    public List<NotificationChannel> GetAvailableChannelTypes()
    {
        return _channels
            .Select(c => c.ChannelType)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    /// <summary>
    /// Gets all registered notification channels
    /// </summary>
    /// <returns>List of channel names</returns>
    public List<string> GetAvailableChannels()
    {
        return _channels
            .Select(c => c.Name)
            .Distinct()
            .OrderBy(n => n)
            .ToList();
    }
}
