#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Middleware;

/// <summary>
/// Executes a pipeline of notification processors to validate and transform notifications
/// </summary>
public class NotificationPipeline
{
    private readonly List<INotificationProcessor> _processors = new();
    private readonly ILogger<NotificationPipeline> _logger;

    public NotificationPipeline(ILogger<NotificationPipeline> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Adds a processor to the pipeline
    /// </summary>
    public NotificationPipeline Use(INotificationProcessor processor)
    {
        ArgumentNullException.ThrowIfNull(processor);
        _processors.Add(processor);
        _logger.LogDebug("Added processor: {ProcessorType}", processor.GetType().Name);
        return this;
    }

    /// <summary>
    /// Executes all processors in the pipeline for the given notification
    /// </summary>
    public async Task<PipelineResult> ExecuteAsync(DeploymentNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        var result = new PipelineResult { Notification = notification };
        var context = new PipelineContext { Notification = notification };

        foreach (var processor in _processors)
        {
            try
            {
                _logger.LogDebug("Executing processor: {ProcessorType}", processor.GetType().Name);
                await processor.ProcessAsync(context);

                if (!context.IsValid)
                {
                    result.Success = false;
                    result.Errors.Add($"Processor {processor.GetType().Name}: {string.Join(", ", context.Errors)}");
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Processor {ProcessorType} failed", processor.GetType().Name);
                result.Success = false;
                result.Errors.Add($"Processor error: {ex.Message}");
                break;
            }
        }

        result.ProcessedNotification = context.Notification;
        result.Success = context.IsValid;

        return result;
    }
}

/// <summary>
/// Represents the state of a notification as it flows through the pipeline
/// </summary>
public class PipelineContext
{
    public DeploymentNotification Notification { get; set; } = new();
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Result of pipeline execution
/// </summary>
public class PipelineResult
{
    public bool Success { get; set; }
    public DeploymentNotification? Notification { get; set; }
    public DeploymentNotification? ProcessedNotification { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Interface for notification processors in the pipeline
/// </summary>
public interface INotificationProcessor
{
    Task ProcessAsync(PipelineContext context);
}

/// <summary>
/// Validates notification data integrity and required fields
/// </summary>
public class ValidationProcessor : INotificationProcessor
{
    private readonly ILogger<ValidationProcessor> _logger;

    public ValidationProcessor(ILogger<ValidationProcessor> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public Task ProcessAsync(PipelineContext context)
    {
        if (context.Notification is null)
        {
            context.IsValid = false;
            context.Errors.Add("Notification is null");
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(context.Notification.ProjectName))
        {
            context.IsValid = false;
            context.Errors.Add("ProjectName is required");
        }

        if (string.IsNullOrWhiteSpace(context.Notification.Version))
        {
            context.IsValid = false;
            context.Errors.Add("Version is required");
        }

        if (!context.Notification.Channels.Any())
        {
            context.IsValid = false;
            context.Errors.Add("At least one channel is required");
        }

        if (context.IsValid)
        {
            _logger.LogDebug("Notification validation passed for {ProjectName}",
                context.Notification.ProjectName);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Enriches notification with additional data like timestamps and IDs
/// </summary>
public class EnrichmentProcessor : INotificationProcessor
{
    public Task ProcessAsync(PipelineContext context)
    {
        var notification = context.Notification;

        // Ensure notification has ID and timestamp
        if (string.IsNullOrWhiteSpace(notification.Id))
            notification.Id = Guid.NewGuid().ToString();

        if (notification.CreatedAt == default)
            notification.CreatedAt = DateTime.UtcNow;

        // Add correlation ID
        if (!context.Data.ContainsKey("CorrelationId"))
            context.Data["CorrelationId"] = Guid.NewGuid().ToString();

        // Track processing start time
        context.Data["ProcessingStartTime"] = DateTime.UtcNow;

        return Task.CompletedTask;
    }
}

/// <summary>
/// Filters notifications based on priority and channel eligibility
/// </summary>
public class FilterProcessor : INotificationProcessor
{
    private readonly IChannelConfigRepository _configRepository;
    private readonly ILogger<FilterProcessor> _logger;

    public FilterProcessor(IChannelConfigRepository configRepository, ILogger<FilterProcessor> logger)
    {
        ArgumentNullException.ThrowIfNull(configRepository);
        ArgumentNullException.ThrowIfNull(logger);
        _configRepository = configRepository;
        _logger = logger;
    }

    public async Task ProcessAsync(PipelineContext context)
    {
        var notification = context.Notification;
        var configs = await _configRepository.GetAllAsync();

        // Filter channels based on configuration and notification priority
        var eligibleChannels = new List<NotificationChannel>();

        foreach (var channel in notification.Channels)
        {
            var config = configs.FirstOrDefault(c => c.ChannelType == channel);
            if (config is null)
            {
                _logger.LogWarning("No configuration found for channel: {Channel}", channel);
                continue;
            }

            // Check minimum priority requirement
            if (notification.Priority < config.MinimumPriority)
            {
                _logger.LogDebug("Notification priority {Priority} below minimum for {Channel}",
                    notification.Priority, channel);
                continue;
            }

            // Check if status is allowed
            if (config.AllowedStatuses.Any() && !config.AllowedStatuses.Contains(notification.Status))
            {
                _logger.LogDebug("Status {Status} not in allowed list for {Channel}",
                    notification.Status, channel);
                continue;
            }

            // Check if environment is allowed
            if (config.AllowedEnvironments.Any() &&
                !config.AllowedEnvironments.Contains(notification.TargetEnvironment))
            {
                _logger.LogDebug("Environment {Environment} not in allowed list for {Channel}",
                    notification.TargetEnvironment, channel);
                continue;
            }

            eligibleChannels.Add(channel);
        }

        if (!eligibleChannels.Any())
        {
            context.IsValid = false;
            context.Errors.Add("Notification filtered out: No eligible channels");
            return;
        }

        // Update notification with filtered channels
        notification.Channels = eligibleChannels;
        context.Data["FilteredChannels"] = eligibleChannels;
    }
}

/// <summary>
/// Sanitizes and truncates notification content for channel-specific limits
/// </summary>
public class SanitizationProcessor : INotificationProcessor
{
    private const int MaxMessageLength = 1024;

    public Task ProcessAsync(PipelineContext context)
    {
        var notification = context.Notification;

        // Sanitize message content
        notification.Message = SanitizeString(notification.Message, MaxMessageLength);
        notification.CommitAuthor = SanitizeString(notification.CommitAuthor, 100);
        notification.BranchName = SanitizeString(notification.BranchName, 100);

        return Task.CompletedTask;
    }

    private string SanitizeString(string? input, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove control characters
        var cleaned = System.Text.RegularExpressions.Regex.Replace(input, @"[\x00-\x1F\x7F]", "");

        // Trim to max length
        return cleaned.Length > maxLength ? cleaned.Substring(0, maxLength) + "..." : cleaned;
    }
}
