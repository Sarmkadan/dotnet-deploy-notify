#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Exceptions;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Data;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Services;

/// <summary>
/// Manages one-click deployment rollbacks with channel notifications
/// </summary>
public interface IRollbackService
{
    /// <summary>Initiates a rollback to a previous deployment version and notifies all configured channels</summary>
    Task<RollbackResult> InitiateRollbackAsync(RollbackRequest request, CancellationToken cancellationToken = default);

    /// <summary>Retrieves rollback history for a project, ordered most recent first</summary>
    Task<List<RollbackResult>> GetRollbackHistoryAsync(string projectName, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>Retrieves status and details of a specific rollback operation</summary>
    Task<RollbackResult?> GetRollbackStatusAsync(string rollbackId, CancellationToken cancellationToken = default);

    /// <summary>Cancels a rollback that is still in the Pending state</summary>
    Task<bool> CancelRollbackAsync(string rollbackId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of the rollback service
/// </summary>
public class RollbackService : IRollbackService
{
    private readonly INotificationService _notificationService;
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<RollbackService> _logger;
    private readonly ConcurrentDictionary<string, RollbackResult> _rollbackStore = new();

    /// <summary>Initializes the rollback service with its dependencies</summary>
    public RollbackService(
        INotificationService notificationService,
        INotificationRepository notificationRepository,
        ILogger<RollbackService> logger)
    {
        _notificationService = notificationService;
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    /// <summary>
    /// Initiates a one-click rollback and dispatches notifications to all configured channels
    /// </summary>
    public async Task<RollbackResult> InitiateRollbackAsync(RollbackRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.IsValid())
            throw new NotificationValidationException(
                $"Invalid rollback request for '{request.ProjectName}': missing required fields",
                new List<string> { "ProjectName, TargetVersion, CurrentVersion, and at least one Channel are required" });

        var result = new RollbackResult
        {
            RequestId = request.Id,
            ProjectName = request.ProjectName,
            RolledBackFromVersion = request.CurrentVersion,
            RolledBackToVersion = request.TargetVersion,
            Status = RollbackStatus.InProgress
        };

        _rollbackStore[result.Id] = result;

        _logger.LogInformation(
            "Rollback initiated for {Project}: v{From} → v{To} on {Environment} by {RequestedBy}",
            request.ProjectName,
            request.CurrentVersion,
            request.TargetVersion,
            request.TargetEnvironment,
            request.RequestedBy);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var priorDeployment = await FindPriorDeploymentAsync(request, cancellationToken);
            var notification = BuildRollbackNotification(request, priorDeployment);

            var notificationId = await _notificationService.CreateNotificationAsync(notification);
            var notificationResults = await _notificationService.SendNotificationAsync(notificationId, request.Channels);

            result.NotificationResults.AddRange(notificationResults);
            result.MarkAsCompleted();

            _logger.LogInformation(
                "Rollback completed for {Project} v{To}: {Sent} notification(s) dispatched",
                request.ProjectName,
                request.TargetVersion,
                notificationResults.Count);
        }
        catch (OperationCanceledException)
        {
            result.MarkAsCancelled();
            _logger.LogWarning("Rollback for {Project} was cancelled", request.ProjectName);
        }
        catch (Exception ex)
        {
            result.MarkAsFailed(ex.Message);
            _logger.LogError(ex, "Rollback failed for {Project} v{To}", request.ProjectName, request.TargetVersion);
            throw;
        }

        return result;
    }

    /// <summary>
    /// Retrieves rollback history for a given project
    /// </summary>
    public Task<List<RollbackResult>> GetRollbackHistoryAsync(string projectName, int limit = 50, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var history = _rollbackStore.Values
            .Where(r => r.ProjectName == projectName)
            .OrderByDescending(r => r.StartedAt)
            .Take(limit)
            .ToList();

        _logger.LogDebug("Retrieved {Count} rollback record(s) for {Project}", history.Count, projectName);
        return Task.FromResult(history);
    }

    /// <summary>
    /// Retrieves a specific rollback operation by its ID
    /// </summary>
    public Task<RollbackResult?> GetRollbackStatusAsync(string rollbackId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _rollbackStore.TryGetValue(rollbackId, out var result);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Cancels a rollback that has not yet started processing
    /// </summary>
    public Task<bool> CancelRollbackAsync(string rollbackId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_rollbackStore.TryGetValue(rollbackId, out var result))
        {
            _logger.LogWarning("Rollback {Id} not found for cancellation", rollbackId);
            return Task.FromResult(false);
        }

        if (result.Status != RollbackStatus.Pending)
        {
            _logger.LogWarning(
                "Rollback {Id} cannot be cancelled — current status: {Status}",
                rollbackId,
                result.Status);
            return Task.FromResult(false);
        }

        result.MarkAsCancelled();
        _logger.LogInformation("Rollback {Id} for {Project} cancelled", rollbackId, result.ProjectName);
        return Task.FromResult(true);
    }

    private async Task<DeploymentNotification?> FindPriorDeploymentAsync(RollbackRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var history = await _notificationRepository.GetByProjectAsync(request.ProjectName, 100);
        return history.FirstOrDefault(n =>
            n.Version == request.TargetVersion &&
            n.TargetEnvironment == request.TargetEnvironment &&
            n.Status is BuildStatus.DeploymentSuccess or BuildStatus.Success);
    }

    private static DeploymentNotification BuildRollbackNotification(RollbackRequest request, DeploymentNotification? priorDeployment)
    {
        var message = string.IsNullOrWhiteSpace(request.Reason)
            ? $"Rolling back {request.ProjectName} from v{request.CurrentVersion} to v{request.TargetVersion}"
            : $"Rolling back {request.ProjectName} from v{request.CurrentVersion} to v{request.TargetVersion}: {request.Reason}";

        return new DeploymentNotification
        {
            ProjectName = request.ProjectName,
            Version = request.TargetVersion,
            Status = BuildStatus.Deploying,
            Message = message,
            TargetEnvironment = request.TargetEnvironment,
            BranchName = priorDeployment?.BranchName ?? string.Empty,
            CommitHash = priorDeployment?.CommitHash ?? string.Empty,
            CommitAuthor = request.RequestedBy,
            RepositoryUrl = priorDeployment?.RepositoryUrl ?? string.Empty,
            BuildUrl = priorDeployment?.BuildUrl ?? string.Empty,
            Channels = request.Channels,
            Priority = request.Priority,
            Metadata = new Dictionary<string, object>(request.Metadata)
            {
                ["RollbackFromVersion"] = request.CurrentVersion,
                ["RollbackReason"] = request.Reason,
                ["RollbackRequestId"] = request.Id
            }
        };
    }
}
