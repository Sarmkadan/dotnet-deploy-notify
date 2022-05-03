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
/// Interface for audit logging of notification operations
/// </summary>
public interface IAuditService
{
    /// <summary>Logs a notification creation event</summary>
    Task LogNotificationCreatedAsync(DeploymentNotification notification);

    /// <summary>Logs a notification delivery attempt</summary>
    Task LogDeliveryAttemptAsync(NotificationResult result);

    /// <summary>Logs a configuration change</summary>
    Task LogConfigurationChangeAsync(string configId, string action, ChannelConfiguration? beforeState = null, ChannelConfiguration? afterState = null);

    /// <summary>Logs a validation failure</summary>
    Task LogValidationFailureAsync(string entityType, string entityId, List<string> errors);

    /// <summary>Gets audit log entries</summary>
    Task<List<AuditLogEntry>> GetAuditLogsAsync(int limit = 100);

    /// <summary>Gets audit logs for a specific notification</summary>
    Task<List<AuditLogEntry>> GetNotificationAuditLogsAsync(string notificationId);

    /// <summary>Clears old audit logs</summary>
    Task ClearOldLogsAsync(DateTime olderThan);
}

/// <summary>
/// Represents an audit log entry
/// </summary>
public class AuditLogEntry
{
    /// <summary>Unique identifier for this log entry</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Timestamp of the event</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Type of operation (Create, Update, Delete, Send, etc.)</summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>Entity type (Notification, Configuration, Result, etc.)</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Entity identifier</summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>User or system that performed the action</summary>
    public string Actor { get; set; } = "System";

    /// <summary>Details about the action</summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>Status of the operation (Success, Failure)</summary>
    public string Status { get; set; } = "Success";

    /// <summary>Additional metadata</summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Implementation of audit service using in-memory storage
/// </summary>
public class AuditService : IAuditService
{
    private readonly List<AuditLogEntry> _auditLogs = new();
    private readonly ILogger<AuditService> _logger;
    private readonly object _lockObject = new();

    /// <summary>Initializes the audit service</summary>
    public AuditService(ILogger<AuditService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Logs notification creation events
    /// </summary>
    public Task LogNotificationCreatedAsync(DeploymentNotification notification)
    {
        var entry = new AuditLogEntry
        {
            Operation = "Create",
            EntityType = "Notification",
            EntityId = notification.Id,
            Details = $"Notification created for {notification.ProjectName} v{notification.Version}",
            Metadata = new Dictionary<string, object>
            {
                { "Project", notification.ProjectName },
                { "Version", notification.Version },
                { "Status", notification.Status.ToString() },
                { "Channels", notification.Channels.Count }
            }
        };

        return AddAuditLogAsync(entry);
    }

    /// <summary>
    /// Logs delivery attempts
    /// </summary>
    public Task LogDeliveryAttemptAsync(NotificationResult result)
    {
        var status = result.IsSuccessful ? "Success" : "Failure";
        var entry = new AuditLogEntry
        {
            Operation = "Deliver",
            EntityType = "NotificationResult",
            EntityId = result.Id,
            Status = status,
            Details = $"Delivery attempt to {result.Channel}: {result.GetSummary()}",
            Metadata = new Dictionary<string, object>
            {
                { "Channel", result.Channel.ToString() },
                { "Status", result.Status.ToString() },
                { "HttpStatus", result.HttpStatusCode ?? 0 },
                { "DurationMs", result.DurationMs },
                { "Attempt", result.AttemptNumber }
            }
        };

        return AddAuditLogAsync(entry);
    }

    /// <summary>
    /// Logs configuration changes
    /// </summary>
    public Task LogConfigurationChangeAsync(
        string configId,
        string action,
        ChannelConfiguration? beforeState = null,
        ChannelConfiguration? afterState = null)
    {
        var entry = new AuditLogEntry
        {
            Operation = action,
            EntityType = "ChannelConfiguration",
            EntityId = configId,
            Details = $"Configuration {action.ToLower()}: {afterState?.DisplayName ?? beforeState?.DisplayName ?? "Unknown"}",
            Metadata = new Dictionary<string, object>()
        };

        if (beforeState is not null)
        {
            entry.Metadata["BeforeChannel"] = beforeState.ChannelType.ToString();
            entry.Metadata["BeforeEnabled"] = beforeState.IsEnabled;
        }

        if (afterState is not null)
        {
            entry.Metadata["AfterChannel"] = afterState.ChannelType.ToString();
            entry.Metadata["AfterEnabled"] = afterState.IsEnabled;
        }

        return AddAuditLogAsync(entry);
    }

    /// <summary>
    /// Logs validation failures
    /// </summary>
    public Task LogValidationFailureAsync(string entityType, string entityId, List<string> errors)
    {
        var entry = new AuditLogEntry
        {
            Operation = "Validate",
            EntityType = entityType,
            EntityId = entityId,
            Status = "Failure",
            Details = $"Validation failed: {string.Join("; ", errors)}",
            Metadata = new Dictionary<string, object>
            {
                { "ErrorCount", errors.Count },
                { "Errors", errors }
            }
        };

        return AddAuditLogAsync(entry);
    }

    /// <summary>
    /// Retrieves audit logs with a size limit
    /// </summary>
    public Task<List<AuditLogEntry>> GetAuditLogsAsync(int limit = 100)
    {
        lock (_lockObject)
        {
            var logs = _auditLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToList();
            return Task.FromResult(logs);
        }
    }

    /// <summary>
    /// Retrieves audit logs for a specific notification
    /// </summary>
    public Task<List<AuditLogEntry>> GetNotificationAuditLogsAsync(string notificationId)
    {
        lock (_lockObject)
        {
            var logs = _auditLogs
                .Where(l => l.EntityId == notificationId)
                .OrderByDescending(l => l.Timestamp)
                .ToList();
            return Task.FromResult(logs);
        }
    }

    /// <summary>
    /// Deletes audit logs older than a specific date
    /// </summary>
    public Task ClearOldLogsAsync(DateTime olderThan)
    {
        lock (_lockObject)
        {
            var oldEntries = _auditLogs.Where(l => l.Timestamp < olderThan).ToList();
            foreach (var entry in oldEntries)
            {
                _auditLogs.Remove(entry);
            }
            _logger.LogDebug("Cleared {Count} audit logs older than {Date}", oldEntries.Count, olderThan);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds an audit log entry to the collection
    /// </summary>
    private Task AddAuditLogAsync(AuditLogEntry entry)
    {
        lock (_lockObject)
        {
            _auditLogs.Add(entry);
            _logger.LogDebug(
                "Audit logged: {Operation} on {EntityType} {EntityId}",
                entry.Operation,
                entry.EntityType,
                entry.EntityId);
        }
        return Task.CompletedTask;
    }
}
