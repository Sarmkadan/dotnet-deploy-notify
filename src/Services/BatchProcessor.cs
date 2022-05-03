// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Services;

/// <summary>
/// Processes notifications in batches for efficient delivery
/// </summary>
public interface IBatchProcessor<T>
{
    Task<List<T>> ProcessBatchAsync(List<T> items, int batchSize);
}

/// <summary>
/// Batch processor for notifications with concurrent execution
/// </summary>
public class NotificationBatchProcessor : IBatchProcessor<DeploymentNotification>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationBatchProcessor> _logger;

    public NotificationBatchProcessor(
        INotificationService notificationService,
        ILogger<NotificationBatchProcessor> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Processes notifications in batches with specified size
    /// </summary>
    public async Task<List<DeploymentNotification>> ProcessBatchAsync(
        List<DeploymentNotification> items,
        int batchSize = 10)
    {
        if (items == null || items.Count == 0)
            return new List<DeploymentNotification>();

        var processed = new List<DeploymentNotification>();
        var batches = ChunkItems(items, batchSize);

        _logger.LogInformation("Processing {Count} notifications in {BatchCount} batches of size {BatchSize}",
            items.Count, batches.Count, batchSize);

        foreach (var batch in batches)
        {
            try
            {
                var batchTasks = batch.Select(item => ProcessSingleAsync(item));
                var results = await Task.WhenAll(batchTasks);
                processed.AddRange(results);

                _logger.LogDebug("Completed batch processing: {ProcessedCount}/{TotalCount}",
                    processed.Count, items.Count);

                // Small delay between batches to avoid overwhelming the system
                if (batches.IndexOf(batch) < batches.Count - 1)
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch");
            }
        }

        return processed;
    }

    private async Task<DeploymentNotification> ProcessSingleAsync(DeploymentNotification notification)
    {
        try
        {
            await _notificationService.SendNotificationAsync(notification);
            notification.MarkAsProcessed();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process notification: {NotificationId}", notification.Id);
            notification.IncrementDeliveryAttempt();
        }

        return notification;
    }

    private List<List<T>> ChunkItems<T>(List<T> items, int chunkSize)
    {
        var chunks = new List<List<T>>();

        for (int i = 0; i < items.Count; i += chunkSize)
        {
            chunks.Add(items.Skip(i).Take(chunkSize).ToList());
        }

        return chunks;
    }
}

/// <summary>
/// Options for batch processing configuration
/// </summary>
public class BatchProcessingOptions
{
    public int DefaultBatchSize { get; set; } = 10;
    public int MaxConcurrentBatches { get; set; } = 5;
    public TimeSpan DelayBetweenBatches { get; set; } = TimeSpan.FromMilliseconds(500);
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// Async batch processor with retry logic
/// </summary>
public class ResilientBatchProcessor<T>
{
    private readonly Func<T, Task> _processor;
    private readonly ILogger<ResilientBatchProcessor<T>> _logger;
    private readonly BatchProcessingOptions _options;

    public ResilientBatchProcessor(
        Func<T, Task> processor,
        ILogger<ResilientBatchProcessor<T>> logger,
        BatchProcessingOptions? options = null)
    {
        _processor = processor;
        _logger = logger;
        _options = options ?? new BatchProcessingOptions();
    }

    /// <summary>
    /// Processes items with automatic retry and error recovery
    /// </summary>
    public async Task<ProcessingResult> ProcessWithRetryAsync(IEnumerable<T> items)
    {
        var result = new ProcessingResult();
        var itemList = items.ToList();

        _logger.LogInformation("Starting resilient batch processing for {Count} items", itemList.Count);

        for (int retryCount = 0; retryCount < _options.MaxRetries; retryCount++)
        {
            var failedItems = new List<T>();

            foreach (var item in itemList)
            {
                try
                {
                    await _processor(item);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Processing failed for item (attempt {Attempt}/{MaxRetries})",
                        retryCount + 1, _options.MaxRetries);
                    failedItems.Add(item);
                    result.FailureCount++;
                }
            }

            if (!failedItems.Any())
            {
                _logger.LogInformation("All items processed successfully");
                break;
            }

            itemList = failedItems;

            if (retryCount < _options.MaxRetries - 1)
            {
                await Task.Delay(_options.DelayBetweenBatches);
            }
        }

        result.FinalFailureCount = itemList.Count;
        return result;
    }
}

/// <summary>
/// Result of batch processing
/// </summary>
public class ProcessingResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public int FinalFailureCount { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    public int TotalProcessed => SuccessCount + FinalFailureCount;
    public double SuccessRate => TotalProcessed > 0 ? (double)SuccessCount / TotalProcessed * 100 : 0;

    public override string ToString() =>
        $"Success: {SuccessCount}, Failures: {FinalFailureCount}, Rate: {SuccessRate:F1}%";
}
