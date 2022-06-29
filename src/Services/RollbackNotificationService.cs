#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Services;

/// <summary>
/// Builds and dispatches rich, channel-specific notifications for rollback events
/// </summary>
public interface IRollbackNotificationService
{
    /// <summary>Sends a notification that a rollback has been initiated</summary>
    Task<List<NotificationResult>> NotifyRollbackInitiatedAsync(RollbackRequest request);

    /// <summary>Sends a notification that a rollback completed successfully</summary>
    Task<List<NotificationResult>> NotifyRollbackCompletedAsync(RollbackRequest request, RollbackResult result);

    /// <summary>Sends a notification that a rollback failed</summary>
    Task<List<NotificationResult>> NotifyRollbackFailedAsync(RollbackRequest request, string reason);

    /// <summary>Formats a rollback status message tailored to the specified channel</summary>
    string FormatRollbackMessage(RollbackRequest request, RollbackStatus status, NotificationChannel channel, string? additionalDetails = null);

    /// <summary>Returns the notification history for rollback events on a given project</summary>
    Task<List<RollbackNotificationRecord>> GetRollbackNotificationHistoryAsync(string projectName, int limit = 50);
}

/// <summary>
/// A record of a rollback notification that was dispatched
/// </summary>
public sealed class RollbackNotificationRecord
{
    /// <summary>Unique record identifier</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>ID of the originating rollback request</summary>
    public string RollbackRequestId { get; set; } = string.Empty;

    /// <summary>Project this record belongs to</summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>Rollback status that triggered this notification</summary>
    public RollbackStatus TriggerStatus { get; set; }

    /// <summary>Channels the notification was dispatched to</summary>
    public List<NotificationChannel> Channels { get; set; } = new();

    /// <summary>UTC timestamp of the dispatch</summary>
    public DateTime DispatchedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Delivery results per channel</summary>
    public List<NotificationResult> DeliveryResults { get; set; } = new();
}

/// <summary>
/// Implementation of <see cref="IRollbackNotificationService"/>
/// </summary>
public sealed class RollbackNotificationService : IRollbackNotificationService
{
    private readonly INotificationService _notificationService;
    private readonly ConcurrentBag<RollbackNotificationRecord> _history = new();
    private readonly ILogger<RollbackNotificationService> _logger;

    /// <summary>Initialises the service with required dependencies</summary>
    public RollbackNotificationService(
        INotificationService notificationService,
        ILogger<RollbackNotificationService> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Dispatches a notification that a rollback has been initiated
    /// </summary>
    public async Task<List<NotificationResult>> NotifyRollbackInitiatedAsync(RollbackRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Dispatching rollback-initiated notification for {Project} v{From}→v{To}",
            request.ProjectName, request.CurrentVersion, request.TargetVersion);

        var notification = BuildNotification(request, RollbackStatus.InProgress);
        return await DispatchAndRecordAsync(request, RollbackStatus.InProgress, notification);
    }

    /// <summary>
    /// Dispatches a notification that a rollback completed successfully
    /// </summary>
    public async Task<List<NotificationResult>> NotifyRollbackCompletedAsync(RollbackRequest request, RollbackResult result)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(result);

        _logger.LogInformation(
            "Dispatching rollback-completed notification for {Project}", request.ProjectName);

        var notification = BuildNotification(request, RollbackStatus.Completed);
        notification.Status = BuildStatus.DeploymentSuccess;
        notification.Message = FormatRollbackMessage(request, RollbackStatus.Completed, NotificationChannel.Slack);
        return await DispatchAndRecordAsync(request, RollbackStatus.Completed, notification);
    }

    /// <summary>
    /// Dispatches a notification that a rollback failed
    /// </summary>
    public async Task<List<NotificationResult>> NotifyRollbackFailedAsync(RollbackRequest request, string reason)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogWarning(
            "Dispatching rollback-failed notification for {Project}: {Reason}",
            request.ProjectName, reason);

        var notification = BuildNotification(request, RollbackStatus.Failed);
        notification.Status = BuildStatus.DeploymentFailed;
        notification.Message = FormatRollbackMessage(request, RollbackStatus.Failed, NotificationChannel.Slack, reason);
        notification.Priority = NotificationPriority.Critical;
        return await DispatchAndRecordAsync(request, RollbackStatus.Failed, notification);
    }

    /// <summary>
    /// Formats a rollback message with channel-specific conventions
    /// </summary>
    public string FormatRollbackMessage(
        RollbackRequest request,
        RollbackStatus status,
        NotificationChannel channel,
        string? additionalDetails = null)
    {
        var emoji = status switch
        {
            RollbackStatus.InProgress => "🔄",
            RollbackStatus.Completed  => "✅",
            RollbackStatus.Failed     => "❌",
            RollbackStatus.Cancelled  => "🚫",
            _                         => "⏳"
        };

        var statusLabel = status switch
        {
            RollbackStatus.InProgress => "initiated",
            RollbackStatus.Completed  => "completed successfully",
            RollbackStatus.Failed     => "failed",
            RollbackStatus.Cancelled  => "cancelled",
            _                         => "pending"
        };

        var core = channel switch
        {
            NotificationChannel.Slack =>
                $"{emoji} *Rollback {statusLabel}*\n" +
                $"*Project:* {request.ProjectName}\n" +
                $"*From:* `v{request.CurrentVersion}` → *To:* `v{request.TargetVersion}`\n" +
                $"*Environment:* `{request.TargetEnvironment}`\n" +
                $"*Initiated by:* {request.RequestedBy}",

            NotificationChannel.Discord =>
                $"{emoji} **Rollback {statusLabel}**\n" +
                $"**Project:** {request.ProjectName}\n" +
                $"**From:** `v{request.CurrentVersion}` → **To:** `v{request.TargetVersion}`\n" +
                $"**Environment:** `{request.TargetEnvironment}`\n" +
                $"**Initiated by:** {request.RequestedBy}",

            NotificationChannel.Telegram =>
                $"{emoji} <b>Rollback {statusLabel}</b>\n" +
                $"<b>Project:</b> {request.ProjectName}\n" +
                $"<b>From:</b> <code>v{request.CurrentVersion}</code> → <b>To:</b> <code>v{request.TargetVersion}</code>\n" +
                $"<b>Environment:</b> <code>{request.TargetEnvironment}</code>\n" +
                $"<b>Initiated by:</b> {request.RequestedBy}",

            _ =>
                $"{emoji} Rollback {statusLabel}: {request.ProjectName} " +
                $"v{request.CurrentVersion}→v{request.TargetVersion} [{request.TargetEnvironment}] " +
                $"by {request.RequestedBy}"
        };

        if (!string.IsNullOrWhiteSpace(request.Reason))
            core += $"\n*Reason:* {request.Reason}";

        if (!string.IsNullOrWhiteSpace(additionalDetails))
            core += $"\n*Details:* {additionalDetails}";

        return core;
    }

    /// <summary>
    /// Returns rollback notification history for a project
    /// </summary>
    public Task<List<RollbackNotificationRecord>> GetRollbackNotificationHistoryAsync(string projectName, int limit = 50)
    {
        var results = _history
            .Where(r => string.Equals(r.ProjectName, projectName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.DispatchedAt)
            .Take(limit)
            .ToList();

        return Task.FromResult(results);
    }

    // ─── Private helpers ───────────────────────────────────────────────────────

    private DeploymentNotification BuildNotification(RollbackRequest request, RollbackStatus status)
    {
        return new DeploymentNotification
        {
            ProjectName = request.ProjectName,
            Version = request.TargetVersion,
            Status = status == RollbackStatus.InProgress ? BuildStatus.Deploying : BuildStatus.DeploymentSuccess,
            TargetEnvironment = request.TargetEnvironment,
            BranchName = string.Empty,
            CommitAuthor = request.RequestedBy,
            Channels = request.Channels,
            Priority = request.Priority,
            Message = FormatRollbackMessage(request, status, NotificationChannel.Slack),
            Metadata = new Dictionary<string, object>(request.Metadata)
            {
                ["RollbackFromVersion"] = request.CurrentVersion,
                ["RollbackReason"] = request.Reason,
                ["RollbackRequestId"] = request.Id,
                ["RollbackStatus"] = status.ToString()
            }
        };
    }

    private async Task<List<NotificationResult>> DispatchAndRecordAsync(
        RollbackRequest request,
        RollbackStatus status,
        DeploymentNotification notification)
    {
        var notificationId = await _notificationService.CreateNotificationAsync(notification);
        var results = await _notificationService.SendNotificationAsync(notificationId, request.Channels);

        var record = new RollbackNotificationRecord
        {
            RollbackRequestId = request.Id,
            ProjectName = request.ProjectName,
            TriggerStatus = status,
            Channels = request.Channels.ToList(),
            DeliveryResults = results
        };

        _history.Add(record);
        return results;
    }
}
