#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Helper class for parallel batch dispatch with bounded concurrency and error handling
/// </summary>
public static class ParallelBatchDispatcher
{
    /// <summary>
    /// Sends notifications to multiple channels with bounded parallelism
    /// </summary>
    /// <param name="notificationId">The notification ID</param>
    /// <param name="channels">List of channels to send to</param>
    /// <param name="sendAction">Async function to send to a single channel</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of results for each channel attempt</returns>
    public static async Task<IEnumerable<NotificationResult>> SendToChannelsWithParallelismAsync(
        string notificationId,
        IReadOnlyList<ChannelConfiguration> channels,
        Func<ChannelConfiguration, Task<NotificationResult>> sendAction,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(channels);
        ArgumentNullException.ThrowIfNull(sendAction);
        ArgumentNullException.ThrowIfNull(logger);

        if (channels.Count == 0)
        {
            logger.LogWarning("No channels provided for notification {NotificationId}", notificationId);
            return Enumerable.Empty<NotificationResult>();
        }

        // Group configurations by channel type for better rate limiting
        var results = new ConcurrentBag<NotificationResult>();
        var groupedByChannel = channels
            .Where(c => c.ShouldSendNotification(null)) // Filter based on channel settings
            .GroupBy(c => c.ChannelType)
            .OrderBy(g => g.Key.ToString());

        logger.LogDebug("Sending notification {NotificationId} to {ChannelCount} channels with parallel dispatch",
            notificationId, channels.Count);

        // Use bounded parallelism per channel group
        foreach (var channelGroup in groupedByChannel)
        {
            var channelType = channelGroup.Key;
            var channelConfigs = channelGroup.ToList();

            logger.LogDebug("Processing {ChannelCount} configurations for channel {ChannelType}",
                channelConfigs.Count, channelType);

            // Determine max degree of parallelism for this channel type
            // Use the minimum from all configs for this channel, or default to 4
            var maxDegree = channelConfigs
                .Select(c => c.MaxDegreeOfParallelism)
                .DefaultIfEmpty(4)
                .Min();

            // Ensure at least 1, but not more than the number of configs
            maxDegree = Math.Max(1, Math.Min(maxDegree, channelConfigs.Count));

            logger.LogDebug("Using max parallelism of {MaxDegree} for channel {ChannelType}", maxDegree, channelType);

            // Create a semaphore to limit concurrency
            using var semaphore = new SemaphoreSlim(maxDegree, maxDegree);

            // Process each configuration in parallel with bounded concurrency
            var tasks = channelConfigs.Select(async config =>
            {
                try
                {
                    await semaphore.WaitAsync(cancellationToken);

                    try
                    {
                        var result = await sendAction(config);
                        results.Add(result);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Error sending to channel {ChannelType} for notification {NotificationId}",
                        channelType, notificationId);

                    // Create a failure result for this channel
                    var failureResult = new NotificationResult
                    {
                        NotificationId = notificationId,
                        Channel = config.ChannelType,
                        ConfigurationId = config.Id,
                        Status = DeliveryStatus.Failed,
                        ErrorMessage = ex.Message,
                        ExceptionType = ex.GetType().Name,
                        AttemptNumber = 1,
                        AttemptedAt = DateTime.UtcNow
                    };
                    results.Add(failureResult);
                }
            }).ToList();

            await Task.WhenAll(tasks);
        }

        return results;
    }

    /// <summary>
    /// Sends a batch of notifications with bounded parallelism and error isolation
    /// </summary>
    /// <param name="batchId">The batch ID</param>
    /// <param name="notifications">Notifications to send</param>
    /// <param name="sendAction">Async function to send a single notification</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Batch notification result with per-item results</returns>
    public static async Task<BatchNotificationResult> SendBatchWithParallelismAsync<TNotification>(
        string batchId,
        IReadOnlyList<TNotification> notifications,
        Func<TNotification, Task<IEnumerable<NotificationResult>>> sendAction,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(sendAction);
        ArgumentNullException.ThrowIfNull(logger);

        if (notifications.Count == 0)
        {
            logger.LogWarning("No notifications provided for batch {BatchId}", batchId);
            return new BatchNotificationResult
            {
                BatchId = batchId,
                Status = BatchStatus.Sent,
                NotificationCount = 0,
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };
        }

        logger.LogInformation("Sending batch {BatchId} with {NotificationCount} notifications using parallel dispatch",
            batchId, notifications.Count);

        var batchResult = new BatchNotificationResult
        {
            BatchId = batchId,
            Status = BatchStatus.Processing,
            NotificationCount = notifications.Count,
            StartedAt = DateTime.UtcNow
        };

        var globalSemaphore = new SemaphoreSlim(4, 4);

        // Process each notification in parallel
        var tasks = notifications.Select(async notification =>
        {
            try
            {
                await globalSemaphore.WaitAsync(cancellationToken);

                try
                {
                    var results = await sendAction(notification);
                    foreach (var result in results)
                    {
                        batchResult.AddNotificationResult(result);
                    }
                }
                finally
                {
                    globalSemaphore.Release();
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error processing notification in batch {BatchId}", batchId);

                // Create failure result with proper attribution
                var failureResult = new NotificationResult
                {
                    NotificationId = typeof(TNotification) == typeof(DeploymentNotification)
                        ? ((DeploymentNotification)(object)notification).Id
                        : Guid.NewGuid().ToString(),
                    Status = DeliveryStatus.Failed,
                    ErrorMessage = ex.Message,
                    ExceptionType = ex.GetType().Name,
                    AttemptNumber = 1,
                    AttemptedAt = DateTime.UtcNow
                };
                batchResult.AddNotificationResult(failureResult);
            }
        }).ToList();

        await Task.WhenAll(tasks);

        batchResult.MarkAsCompleted();

        logger.LogInformation("Batch {BatchId} completed: {SuccessCount} succeeded, {FailureCount} failed",
            batchId, batchResult.SuccessfulDeliveries, batchResult.FailedDeliveries);

        return batchResult;
    }
}