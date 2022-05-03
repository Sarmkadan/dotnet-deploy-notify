// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Exceptions;
using DotNetDeployNotify.Core.Models;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Data;

/// <summary>
/// Interface for notification data access
/// </summary>
public interface INotificationRepository
{
    /// <summary>Creates a new notification record</summary>
    Task CreateAsync(DeploymentNotification notification);

    /// <summary>Retrieves a notification by ID</summary>
    Task<DeploymentNotification?> GetByIdAsync(string id);

    /// <summary>Retrieves all notifications for a project</summary>
    Task<List<DeploymentNotification>> GetByProjectAsync(string projectName, int limit);

    /// <summary>Retrieves unprocessed notifications</summary>
    Task<List<DeploymentNotification>> GetPendingAsync();

    /// <summary>Updates an existing notification</summary>
    Task UpdateAsync(DeploymentNotification notification);

    /// <summary>Deletes a notification</summary>
    Task DeleteAsync(string id);

    /// <summary>Retrieves notifications by environment</summary>
    Task<List<DeploymentNotification>> GetByEnvironmentAsync(Environment environment);

    /// <summary>Retrieves notifications by status</summary>
    Task<List<DeploymentNotification>> GetByStatusAsync(BuildStatus status, int limit);
}

/// <summary>
/// Interface for channel configuration data access
/// </summary>
public interface IChannelConfigRepository
{
    /// <summary>Creates a new channel configuration</summary>
    Task CreateAsync(ChannelConfiguration config);

    /// <summary>Retrieves a configuration by ID</summary>
    Task<ChannelConfiguration?> GetByIdAsync(string id);

    /// <summary>Retrieves all configurations for a channel type</summary>
    Task<List<ChannelConfiguration>> GetByChannelAsync(NotificationChannel channel);

    /// <summary>Retrieves all enabled configurations</summary>
    Task<List<ChannelConfiguration>> GetEnabledAsync();

    /// <summary>Updates a configuration</summary>
    Task UpdateAsync(ChannelConfiguration config);

    /// <summary>Deletes a configuration</summary>
    Task DeleteAsync(string id);

    /// <summary>Retrieves all configurations with pagination</summary>
    Task<List<ChannelConfiguration>> GetAllAsync(int skip = 0, int take = 100);
}

/// <summary>
/// Interface for notification result data access
/// </summary>
public interface INotificationResultRepository
{
    /// <summary>Creates a new result record</summary>
    Task CreateAsync(NotificationResult result);

    /// <summary>Retrieves a result by ID</summary>
    Task<NotificationResult?> GetByIdAsync(string id);

    /// <summary>Retrieves all results for a notification</summary>
    Task<List<NotificationResult>> GetByNotificationIdAsync(string notificationId);

    /// <summary>Retrieves failed results for a notification</summary>
    Task<List<NotificationResult>> GetFailedByNotificationIdAsync(string notificationId);

    /// <summary>Retrieves all results by channel</summary>
    Task<List<NotificationResult>> GetByChannelAsync(NotificationChannel channel, int limit);

    /// <summary>Retrieves all results with pagination</summary>
    Task<List<NotificationResult>> GetAllAsync(int skip = 0, int take = 100);

    /// <summary>Updates a result</summary>
    Task UpdateAsync(NotificationResult result);

    /// <summary>Deletes old result records</summary>
    Task DeleteOlderThanAsync(DateTime date);
}

/// <summary>
/// In-memory implementation of notification repository
/// </summary>
public class NotificationRepository : INotificationRepository
{
    private readonly List<DeploymentNotification> _notifications = new();
    private readonly ILogger<NotificationRepository> _logger;
    private readonly object _lockObject = new();

    /// <summary>Initializes the repository</summary>
    public NotificationRepository(ILogger<NotificationRepository> logger)
    {
        _logger = logger;
    }

    public Task CreateAsync(DeploymentNotification notification)
    {
        lock (_lockObject)
        {
            _notifications.Add(notification);
            _logger.LogDebug("Notification {Id} created", notification.Id);
        }
        return Task.CompletedTask;
    }

    public Task<DeploymentNotification?> GetByIdAsync(string id)
    {
        lock (_lockObject)
        {
            return Task.FromResult(_notifications.FirstOrDefault(n => n.Id == id));
        }
    }

    public Task<List<DeploymentNotification>> GetByProjectAsync(string projectName, int limit)
    {
        lock (_lockObject)
        {
            var results = _notifications
                .Where(n => n.ProjectName == projectName)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToList();
            return Task.FromResult(results);
        }
    }

    public Task<List<DeploymentNotification>> GetPendingAsync()
    {
        lock (_lockObject)
        {
            var results = _notifications
                .Where(n => !n.IsProcessed)
                .OrderBy(n => n.CreatedAt)
                .ToList();
            return Task.FromResult(results);
        }
    }

    public Task UpdateAsync(DeploymentNotification notification)
    {
        lock (_lockObject)
        {
            var existing = _notifications.FirstOrDefault(n => n.Id == notification.Id);
            if (existing == null)
                throw new RepositoryException("Notification not found", "Update", notification.Id);

            var index = _notifications.IndexOf(existing);
            _notifications[index] = notification;
            _logger.LogDebug("Notification {Id} updated", notification.Id);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        lock (_lockObject)
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == id);
            if (notification == null)
                throw new RepositoryException("Notification not found", "Delete", id);

            _notifications.Remove(notification);
            _logger.LogDebug("Notification {Id} deleted", id);
        }
        return Task.CompletedTask;
    }

    public Task<List<DeploymentNotification>> GetByEnvironmentAsync(Environment environment)
    {
        lock (_lockObject)
        {
            var results = _notifications
                .Where(n => n.TargetEnvironment == environment)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
            return Task.FromResult(results);
        }
    }

    public Task<List<DeploymentNotification>> GetByStatusAsync(BuildStatus status, int limit)
    {
        lock (_lockObject)
        {
            var results = _notifications
                .Where(n => n.Status == status)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToList();
            return Task.FromResult(results);
        }
    }
}

/// <summary>
/// In-memory implementation of channel config repository
/// </summary>
public class ChannelConfigRepository : IChannelConfigRepository
{
    private readonly List<ChannelConfiguration> _configurations = new();
    private readonly ILogger<ChannelConfigRepository> _logger;
    private readonly object _lockObject = new();

    /// <summary>Initializes the repository</summary>
    public ChannelConfigRepository(ILogger<ChannelConfigRepository> logger)
    {
        _logger = logger;
    }

    public Task CreateAsync(ChannelConfiguration config)
    {
        lock (_lockObject)
        {
            _configurations.Add(config);
            _logger.LogDebug("Channel configuration {DisplayName} created", config.DisplayName);
        }
        return Task.CompletedTask;
    }

    public Task<ChannelConfiguration?> GetByIdAsync(string id)
    {
        lock (_lockObject)
        {
            return Task.FromResult(_configurations.FirstOrDefault(c => c.Id == id));
        }
    }

    public Task<List<ChannelConfiguration>> GetByChannelAsync(NotificationChannel channel)
    {
        lock (_lockObject)
        {
            var results = _configurations
                .Where(c => c.ChannelType == channel && c.IsEnabled)
                .ToList();
            return Task.FromResult(results);
        }
    }

    public Task<List<ChannelConfiguration>> GetEnabledAsync()
    {
        lock (_lockObject)
        {
            var results = _configurations.Where(c => c.IsEnabled).ToList();
            return Task.FromResult(results);
        }
    }

    public Task UpdateAsync(ChannelConfiguration config)
    {
        lock (_lockObject)
        {
            var existing = _configurations.FirstOrDefault(c => c.Id == config.Id);
            if (existing == null)
                throw new RepositoryException("Configuration not found", "Update", config.Id);

            var index = _configurations.IndexOf(existing);
            _configurations[index] = config;
            config.MarkAsUpdated();
            _logger.LogDebug("Configuration {DisplayName} updated", config.DisplayName);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        lock (_lockObject)
        {
            var config = _configurations.FirstOrDefault(c => c.Id == id);
            if (config == null)
                throw new RepositoryException("Configuration not found", "Delete", id);

            _configurations.Remove(config);
            _logger.LogDebug("Configuration {DisplayName} deleted", config.DisplayName);
        }
        return Task.CompletedTask;
    }

    public Task<List<ChannelConfiguration>> GetAllAsync(int skip = 0, int take = 100)
    {
        lock (_lockObject)
        {
            var results = _configurations
                .OrderBy(c => c.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToList();
            return Task.FromResult(results);
        }
    }
}

/// <summary>
/// In-memory implementation of notification result repository
/// </summary>
public class NotificationResultRepository : INotificationResultRepository
{
    private readonly List<NotificationResult> _results = new();
    private readonly ILogger<NotificationResultRepository> _logger;
    private readonly object _lockObject = new();

    /// <summary>Initializes the repository</summary>
    public NotificationResultRepository(ILogger<NotificationResultRepository> logger)
    {
        _logger = logger;
    }

    public Task CreateAsync(NotificationResult result)
    {
        lock (_lockObject)
        {
            _results.Add(result);
            _logger.LogDebug("Result {Id} created for notification {NotificationId}", result.Id, result.NotificationId);
        }
        return Task.CompletedTask;
    }

    public Task<NotificationResult?> GetByIdAsync(string id)
    {
        lock (_lockObject)
        {
            return Task.FromResult(_results.FirstOrDefault(r => r.Id == id));
        }
    }

    public Task<List<NotificationResult>> GetByNotificationIdAsync(string notificationId)
    {
        lock (_lockObject)
        {
            var results = _results
                .Where(r => r.NotificationId == notificationId)
                .OrderByDescending(r => r.AttemptedAt)
                .ToList();
            return Task.FromResult(results);
        }
    }

    public Task<List<NotificationResult>> GetFailedByNotificationIdAsync(string notificationId)
    {
        lock (_lockObject)
        {
            var results = _results
                .Where(r => r.NotificationId == notificationId && r.Status == DeliveryStatus.Failed)
                .OrderByDescending(r => r.AttemptedAt)
                .ToList();
            return Task.FromResult(results);
        }
    }

    public Task<List<NotificationResult>> GetByChannelAsync(NotificationChannel channel, int limit)
    {
        lock (_lockObject)
        {
            var results = _results
                .Where(r => r.Channel == channel)
                .OrderByDescending(r => r.AttemptedAt)
                .Take(limit)
                .ToList();
            return Task.FromResult(results);
        }
    }

    public Task<List<NotificationResult>> GetAllAsync(int skip = 0, int take = 100)
    {
        lock (_lockObject)
        {
            var results = _results
                .OrderByDescending(r => r.AttemptedAt)
                .Skip(skip)
                .Take(take)
                .ToList();
            return Task.FromResult(results);
        }
    }

    public Task UpdateAsync(NotificationResult result)
    {
        lock (_lockObject)
        {
            var existing = _results.FirstOrDefault(r => r.Id == result.Id);
            if (existing == null)
                throw new RepositoryException("Result not found", "Update", result.Id);

            var index = _results.IndexOf(existing);
            _results[index] = result;
            _logger.LogDebug("Result {Id} updated", result.Id);
        }
        return Task.CompletedTask;
    }

    public Task DeleteOlderThanAsync(DateTime date)
    {
        lock (_lockObject)
        {
            var oldResults = _results.Where(r => r.AttemptedAt < date).ToList();
            foreach (var result in oldResults)
            {
                _results.Remove(result);
            }
            _logger.LogDebug("Deleted {Count} results older than {Date}", oldResults.Count, date);
        }
        return Task.CompletedTask;
    }
}
