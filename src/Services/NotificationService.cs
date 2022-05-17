#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Exceptions;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Data;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Services;

/// <summary>
/// Main service for managing and sending deployment notifications
/// </summary>
public interface INotificationService
{
    /// <summary>Creates and queues a new deployment notification</summary>
    Task<string> CreateNotificationAsync(DeploymentNotification notification);

    /// <summary>Sends pending notifications to configured channels</summary>
    Task<List<NotificationResult>> SendPendingNotificationsAsync();

    /// <summary>Sends notification to specific channels</summary>
    Task<List<NotificationResult>> SendNotificationAsync(string notificationId, List<NotificationChannel>? channels = null);

    /// <summary>Gets notification history by project</summary>
    Task<List<DeploymentNotification>> GetNotificationHistoryAsync(string projectName, int limit = 50);

    /// <summary>Gets delivery results for a notification</summary>
    Task<List<NotificationResult>> GetDeliveryResultsAsync(string notificationId);

    /// <summary>Retries failed deliveries</summary>
    Task<List<NotificationResult>> RetryFailedDeliveriesAsync(string notificationId);
}

/// <summary>
/// Implementation of the notification service
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IChannelConfigRepository _configRepository;
    private readonly INotificationResultRepository _resultRepository;
    private readonly IWebhookDispatcher _dispatcher;
    private readonly IValidationService _validationService;
    private readonly ILogger<NotificationService> _logger;

    /// <summary>Initializes the notification service with dependencies</summary>
    public NotificationService(
        INotificationRepository notificationRepository,
        IChannelConfigRepository configRepository,
        INotificationResultRepository resultRepository,
        IWebhookDispatcher dispatcher,
        IValidationService validationService,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _configRepository = configRepository;
        _resultRepository = resultRepository;
        _dispatcher = dispatcher;
        _validationService = validationService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new deployment notification and stores it
    /// </summary>
    public async Task<string> CreateNotificationAsync(DeploymentNotification notification)
    {
        try
        {
            // Validate the notification
            var validation = _validationService.ValidateNotification(notification);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Count > 0
                    ? string.Join(", ", validation.Errors)
                    : "unknown validation error";
                throw new NotificationValidationException(
                    $"Notification validation failed: {errors}",
                    validation.Errors);
            }

            // Store the notification
            await _notificationRepository.CreateAsync(notification);

            _logger.LogInformation(
                "Notification created for {Project} v{Version}: {Id}",
                notification.ProjectName,
                notification.Version,
                notification.Id);

            return notification.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create notification for {Project}", notification?.ProjectName);
            throw;
        }
    }

    /// <summary>
    /// Sends all pending notifications to their configured channels
    /// </summary>
    public async Task<List<NotificationResult>> SendPendingNotificationsAsync()
    {
        _logger.LogInformation("Processing pending notifications");

        var pendingNotifications = await _notificationRepository.GetPendingAsync();
        var allResults = new List<NotificationResult>();

        foreach (var notification in pendingNotifications)
        {
            var results = await SendNotificationAsync(notification.Id);
            allResults.AddRange(results);
        }

        _logger.LogInformation("Sent {Count} notifications", allResults.Count);
        return allResults;
    }

    /// <summary>
    /// Sends a specific notification to configured channels
    /// </summary>
    public async Task<List<NotificationResult>> SendNotificationAsync(string notificationId, List<NotificationChannel>? channels = null)
    {
        var results = new List<NotificationResult>();

        try
        {
            // Retrieve the notification
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification is null)
            {
                throw new NotificationException($"Notification {notificationId} not found");
            }

            // Get channels to send to
            var channelsToSend = channels ?? notification.Channels;
            if (!channelsToSend.Any())
            {
                _logger.LogWarning("No channels specified for notification {Id}", notificationId);
                return results;
            }

            // Send to each channel
            foreach (var channel in channelsToSend)
            {
                var configs = await _configRepository.GetByChannelAsync(channel);
                if (!configs.Any())
                {
                    _logger.LogWarning("No configuration found for channel {Channel}", channel);
                    continue;
                }

                foreach (var config in configs)
                {
                    if (!config.ShouldSendNotification(notification))
                    {
                        _logger.LogDebug(
                            "Skipping notification {Id} to {Channel}/{Config} - filters not matched",
                            notificationId,
                            channel,
                            config.DisplayName);
                        continue;
                    }

                    // Send the webhook
                    var result = await _dispatcher.SendToWebhookAsync(config, notification);

                    // Store the result
                    await _resultRepository.CreateAsync(result);
                    results.Add(result);

                    notification.IncrementDeliveryAttempt();
                }
            }

            // Mark as processed
            notification.MarkAsProcessed();
            await _notificationRepository.UpdateAsync(notification);

            _logger.LogInformation(
                "Sent notification {Id} to {Channels}: {SuccessCount} succeeded, {FailureCount} failed",
                notificationId,
                string.Join(", ", channelsToSend),
                results.Count(r => r.IsSuccessful),
                results.Count(r => !r.IsSuccessful));

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification {Id}", notificationId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves notification history for a specific project
    /// </summary>
    public async Task<List<DeploymentNotification>> GetNotificationHistoryAsync(string projectName, int limit = 50)
    {
        try
        {
            return await _notificationRepository.GetByProjectAsync(projectName, limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve notification history for {Project}", projectName);
            throw;
        }
    }

    /// <summary>
    /// Gets all delivery results for a specific notification
    /// </summary>
    public async Task<List<NotificationResult>> GetDeliveryResultsAsync(string notificationId)
    {
        try
        {
            return await _resultRepository.GetByNotificationIdAsync(notificationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve delivery results for {Id}", notificationId);
            throw;
        }
    }

    /// <summary>
    /// Retries failed deliveries for a notification
    /// </summary>
    public async Task<List<NotificationResult>> RetryFailedDeliveriesAsync(string notificationId)
    {
        _logger.LogInformation("Retrying failed deliveries for notification {Id}", notificationId);

        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification is null)
        {
            throw new NotificationException($"Notification {notificationId} not found");
        }

        var failedResults = await _resultRepository.GetFailedByNotificationIdAsync(notificationId);
        var retryResults = new List<NotificationResult>();

        foreach (var failedResult in failedResults)
        {
            // Get the configuration
            var config = await _configRepository.GetByIdAsync(failedResult.ConfigurationId);
            if (config is null)
            {
                _logger.LogWarning("Configuration {Id} not found for retry", failedResult.ConfigurationId);
                continue;
            }

            // Check max retries
            if (failedResult.AttemptNumber >= config.MaxRetries)
            {
                _logger.LogWarning(
                    "Max retries ({MaxRetries}) reached for {Id}",
                    config.MaxRetries,
                    failedResult.Id);
                continue;
            }

            // Retry the delivery
            var newResult = await _dispatcher.SendToWebhookAsync(config, notification);
            newResult.AttemptNumber = failedResult.AttemptNumber + 1;

            await _resultRepository.CreateAsync(newResult);
            retryResults.Add(newResult);

            _logger.LogInformation(
                "Retried delivery {Id} for notification {NotificationId}: {Status}",
                newResult.Id,
                notificationId,
                newResult.Status);
        }

        return retryResults;
    }
}
