#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using DotNetDeployNotify.Configuration;
using DotNetDeployNotify.Core.Exceptions;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using Microsoft.Extensions.Options;

namespace DotNetDeployNotify.Canary;

/// <summary>
/// Orchestrates canary deployments: traffic splitting, step advancement, health evaluation,
/// automatic rollback, and lifecycle notifications dispatched via the notification pipeline.
/// </summary>
public sealed class CanaryDeploymentEngine : ICanaryDeploymentService
{
    private readonly INotificationService _notificationService;
    private readonly IRollbackService _rollbackService;
    private readonly ITrafficSplitter _trafficSplitter;
    private readonly ICanaryHealthEvaluator _healthEvaluator;
    private readonly CanaryOptions _options;
    private readonly ILogger<CanaryDeploymentEngine> _logger;
    private readonly ConcurrentDictionary<string, CanaryDeployment> _deployments = new();

    /// <summary>
    /// Initialises the canary engine with all required collaborating services
    /// </summary>
    public CanaryDeploymentEngine(
        INotificationService notificationService,
        IRollbackService rollbackService,
        ITrafficSplitter trafficSplitter,
        ICanaryHealthEvaluator healthEvaluator,
        IOptions<CanaryOptions> options,
        ILogger<CanaryDeploymentEngine> logger)
    {
        _notificationService = notificationService;
        _rollbackService     = rollbackService;
        _trafficSplitter     = trafficSplitter;
        _healthEvaluator     = healthEvaluator;
        _options             = options.Value;
        _logger              = logger;
    }

    /// <inheritdoc />
    public async Task<CanaryDeployment> StartCanaryAsync(
        CanaryDeploymentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request);

        var plan = _trafficSplitter.GenerateRolloutPlan(request.Strategy);

        var deployment = new CanaryDeployment
        {
            ProjectName          = request.ProjectName,
            StableVersion        = request.StableVersion,
            CanaryVersion        = request.CanaryVersion,
            TargetEnvironment    = request.TargetEnvironment,
            Strategy             = request.Strategy,
            RolloutPlan          = plan,
            NotificationChannels = request.NotificationChannels,
            Priority             = request.Priority,
            InitiatedBy          = request.InitiatedBy,
            BranchName           = request.BranchName,
            CommitHash           = request.CommitHash,
            BuildUrl             = request.BuildUrl,
            Metadata             = new Dictionary<string, object>(request.Metadata)
            {
                ["CanaryEngine"]     = "v2",
                ["AutoRollback"]     = _options.AutoRollbackOnFailure,
                ["AutoAdvance"]      = _options.AutoAdvanceOnSuccess
            }
        };

        // Activate the first rollout step immediately
        if (plan.Count > 0)
        {
            plan[0].Status    = RolloutStepStatus.InProgress;
            plan[0].StartedAt = DateTime.UtcNow;
            deployment.CurrentSplit = TrafficSplit.FromCanaryPercent(plan[0].CanaryPercent);
        }

        deployment.Status = CanaryStatus.Active;
        _deployments[deployment.Id] = deployment;

        _logger.LogInformation(
            "Canary deployment started: {Project} v{Canary} → {Env} " +
            "({Strategy} strategy, {Steps} steps, initial split: {Split})",
            deployment.ProjectName,
            deployment.CanaryVersion,
            deployment.TargetEnvironment,
            deployment.Strategy,
            plan.Count,
            deployment.CurrentSplit);

        await DispatchNotificationAsync(
            deployment,
            BuildStatus.Deploying,
            $"Canary deployment started: {request.ProjectName} v{request.CanaryVersion} " +
            $"on {request.TargetEnvironment} — step 1/{plan.Count}, {deployment.CurrentSplit}",
            cancellationToken);

        return deployment;
    }

    /// <inheritdoc />
    public async Task<CanaryDeployment> AdvanceRolloutAsync(
        string deploymentId,
        CancellationToken cancellationToken = default)
    {
        var deployment = GetOrThrow(deploymentId);

        if (deployment.Status != CanaryStatus.Active)
            throw new NotificationValidationException(
                $"Cannot advance deployment '{deploymentId}': status is {deployment.Status}, expected Active.",
                [$"Deployment must be in Active state to advance; current state: {deployment.Status}"]);

        // Complete the currently soaking step
        if (deployment.ActiveStep is { } active)
        {
            active.Status      = RolloutStepStatus.Completed;
            active.CompletedAt = DateTime.UtcNow;
        }

        var nextStep = deployment.NextStep;

        if (nextStep is null)
        {
            if (_options.AutoAdvanceOnSuccess)
                return await PromoteAsync(deploymentId, cancellationToken);

            _logger.LogInformation(
                "All rollout steps complete for {Project} v{Canary} — awaiting manual promotion",
                deployment.ProjectName,
                deployment.CanaryVersion);

            return deployment;
        }

        nextStep.Status    = RolloutStepStatus.InProgress;
        nextStep.StartedAt = DateTime.UtcNow;
        deployment.CurrentSplit = TrafficSplit.FromCanaryPercent(nextStep.CanaryPercent);

        _logger.LogInformation(
            "Rollout advanced: {Project} v{Canary} → step {Step}/{Total} ({Split})",
            deployment.ProjectName,
            deployment.CanaryVersion,
            nextStep.StepNumber,
            deployment.RolloutPlan.Count,
            deployment.CurrentSplit);

        await DispatchNotificationAsync(
            deployment,
            BuildStatus.Deploying,
            $"Canary rollout step {nextStep.StepNumber}/{deployment.RolloutPlan.Count}: " +
            $"{deployment.ProjectName} v{deployment.CanaryVersion} — {deployment.CurrentSplit}",
            cancellationToken);

        return deployment;
    }

    /// <inheritdoc />
    public async Task<CanaryDeployment> PromoteAsync(
        string deploymentId,
        CancellationToken cancellationToken = default)
    {
        var deployment = GetOrThrow(deploymentId);

        if (deployment.IsTerminal)
            throw new NotificationValidationException(
                $"Cannot promote deployment '{deploymentId}': already in terminal state {deployment.Status}.",
                [$"Terminal deployments cannot be promoted; current state: {deployment.Status}"]);

        // Finalise any active step and skip all pending steps
        if (deployment.ActiveStep is { } active)
        {
            active.Status      = RolloutStepStatus.Completed;
            active.CompletedAt = DateTime.UtcNow;
        }

        foreach (var step in deployment.RolloutPlan.Where(s => s.Status == RolloutStepStatus.Pending))
            step.Status = RolloutStepStatus.Skipped;

        deployment.CurrentSplit = TrafficSplit.FullCanary;
        deployment.Status       = CanaryStatus.Promoted;
        deployment.PromotedAt   = DateTime.UtcNow;

        _logger.LogInformation(
            "Canary promoted: {Project} v{Canary} is now serving 100% of {Env} traffic",
            deployment.ProjectName,
            deployment.CanaryVersion,
            deployment.TargetEnvironment);

        await DispatchNotificationAsync(
            deployment,
            BuildStatus.DeploymentSuccess,
            $"Canary promoted: {deployment.ProjectName} v{deployment.CanaryVersion} " +
            $"is live on {deployment.TargetEnvironment} — " +
            $"stable v{deployment.StableVersion} retired",
            cancellationToken);

        return deployment;
    }

    /// <inheritdoc />
    public async Task<CanaryDeployment> AbortAsync(
        string deploymentId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var deployment = GetOrThrow(deploymentId);

        if (deployment.IsTerminal)
            throw new NotificationValidationException(
                $"Cannot abort deployment '{deploymentId}': already in terminal state {deployment.Status}.",
                [$"Terminal deployments cannot be aborted; current state: {deployment.Status}"]);

        // Fail the active step and skip all remaining steps
        if (deployment.ActiveStep is { } active)
        {
            active.Status      = RolloutStepStatus.Failed;
            active.CompletedAt = DateTime.UtcNow;
        }

        foreach (var step in deployment.RolloutPlan.Where(s => s.Status == RolloutStepStatus.Pending))
            step.Status = RolloutStepStatus.Skipped;

        deployment.CurrentSplit = TrafficSplit.Initial;
        deployment.Status       = CanaryStatus.Aborted;
        deployment.AbortReason  = reason;
        deployment.AbortedAt    = DateTime.UtcNow;

        _logger.LogWarning(
            "Canary aborted: {Project} v{Canary} on {Env} — Reason: {Reason}",
            deployment.ProjectName,
            deployment.CanaryVersion,
            deployment.TargetEnvironment,
            reason);

        await DispatchNotificationAsync(
            deployment,
            BuildStatus.DeploymentFailed,
            $"Canary deployment aborted: {deployment.ProjectName} v{deployment.CanaryVersion} " +
            $"on {deployment.TargetEnvironment} — traffic returned to v{deployment.StableVersion}. " +
            $"Reason: {reason}",
            cancellationToken);

        if (_options.AutoRollbackOnFailure)
            await TriggerRollbackAsync(deployment, reason, cancellationToken);

        return deployment;
    }

    /// <inheritdoc />
    public async Task<CanaryEvaluationResult> EvaluateHealthAsync(
        string deploymentId,
        CancellationToken cancellationToken = default)
    {
        var deployment = GetOrThrow(deploymentId);
        var result = await _healthEvaluator.EvaluateAsync(deployment, cancellationToken);

        // Persist the latest metrics snapshots on the deployment object
        deployment.StableMetrics = result.StableMetrics;
        deployment.CanaryMetrics = result.CanaryMetrics;

        if (result.ShouldAutoRollback)
        {
            _logger.LogWarning(
                "Auto-rollback triggered for {Project} v{Canary}: {Reason}",
                deployment.ProjectName,
                deployment.CanaryVersion,
                result.Reason);

            await AbortAsync(deploymentId, $"Automatic rollback: {result.Reason}", cancellationToken);
        }
        else if (!result.IsHealthy)
        {
            _logger.LogWarning(
                "Health warnings for {Project} v{Canary} (no auto-rollback): {Reason}",
                deployment.ProjectName,
                deployment.CanaryVersion,
                result.Reason);
        }

        return result;
    }

    /// <inheritdoc />
    public Task<CanaryDeployment?> GetDeploymentAsync(
        string deploymentId,
        CancellationToken cancellationToken = default)
    {
        _deployments.TryGetValue(deploymentId, out var deployment);
        return Task.FromResult(deployment);
    }

    /// <inheritdoc />
    public Task<List<CanaryDeployment>> GetActiveDeploymentsAsync(
        CancellationToken cancellationToken = default)
    {
        var active = _deployments.Values
            .Where(d => !d.IsTerminal)
            .OrderBy(d => d.CreatedAt)
            .ToList();

        return Task.FromResult(active);
    }

    /// <inheritdoc />
    public Task<List<CanaryDeployment>> GetDeploymentHistoryAsync(
        string projectName,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var history = _deployments.Values
            .Where(d => string.Equals(d.ProjectName, projectName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(d => d.CreatedAt)
            .Take(limit)
            .ToList();

        return Task.FromResult(history);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static void ValidateRequest(CanaryDeploymentRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.ProjectName))
            errors.Add("ProjectName is required.");
        if (string.IsNullOrWhiteSpace(request.StableVersion))
            errors.Add("StableVersion is required.");
        if (string.IsNullOrWhiteSpace(request.CanaryVersion))
            errors.Add("CanaryVersion is required.");
        if (request.StableVersion == request.CanaryVersion)
            errors.Add("CanaryVersion must differ from StableVersion.");

        if (errors.Count > 0)
            throw new NotificationValidationException(
                $"Invalid canary deployment request for '{request.ProjectName}'.",
                errors);
    }

    private CanaryDeployment GetOrThrow(string deploymentId)
    {
        if (!_deployments.TryGetValue(deploymentId, out var deployment))
            throw new RepositoryException(
                $"Canary deployment '{deploymentId}' not found.",
                "GetDeployment",
                deploymentId);

        return deployment;
    }

    private async Task DispatchNotificationAsync(
        CanaryDeployment deployment,
        BuildStatus status,
        string message,
        CancellationToken cancellationToken)
    {
        if (deployment.NotificationChannels.Count == 0) return;

        try
        {
            var notification = new DeploymentNotification
            {
                ProjectName       = deployment.ProjectName,
                Version           = deployment.CanaryVersion,
                Status            = status,
                Message           = message,
                TargetEnvironment = deployment.TargetEnvironment,
                BranchName        = deployment.BranchName,
                CommitHash        = deployment.CommitHash,
                CommitAuthor      = deployment.InitiatedBy,
                BuildUrl          = deployment.BuildUrl,
                Channels          = deployment.NotificationChannels,
                Priority          = deployment.Priority,
                Metadata          = new Dictionary<string, object>(deployment.Metadata)
                {
                    ["CanaryDeploymentId"] = deployment.Id,
                    ["StableVersion"]      = deployment.StableVersion,
                    ["CanaryVersion"]      = deployment.CanaryVersion,
                    ["TrafficSplit"]       = deployment.CurrentSplit.ToString(),
                    ["CanaryStatus"]       = deployment.Status.ToString(),
                    ["ProgressPercent"]    = deployment.ProgressPercent,
                    ["ActiveStep"]         = deployment.ActiveStep?.StepNumber ?? 0,
                    ["TotalSteps"]         = deployment.RolloutPlan.Count
                }
            };

            var notificationId = await _notificationService.CreateNotificationAsync(notification);
            await _notificationService.SendNotificationAsync(notificationId, deployment.NotificationChannels);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex,
                "Failed to dispatch canary lifecycle notification for {Project} v{Canary} ({Status})",
                deployment.ProjectName,
                deployment.CanaryVersion,
                status);
        }
    }

    private async Task TriggerRollbackAsync(
        CanaryDeployment deployment,
        string reason,
        CancellationToken cancellationToken)
    {
        try
        {
            var rollbackRequest = new RollbackRequest
            {
                ProjectName       = deployment.ProjectName,
                TargetVersion     = deployment.StableVersion,
                CurrentVersion    = deployment.CanaryVersion,
                TargetEnvironment = deployment.TargetEnvironment,
                RequestedBy       = nameof(CanaryDeploymentEngine),
                Reason            = reason,
                Channels          = deployment.NotificationChannels,
                Priority          = NotificationPriority.Critical,
                Metadata          = new Dictionary<string, object>(deployment.Metadata)
                {
                    ["CanaryDeploymentId"] = deployment.Id,
                    ["AutoRollback"]       = true,
                    ["AbortReason"]        = reason
                }
            };

            await _rollbackService.InitiateRollbackAsync(rollbackRequest, cancellationToken);

            _logger.LogInformation(
                "Auto-rollback initiated for {Project}: v{Canary} → v{Stable}",
                deployment.ProjectName,
                deployment.CanaryVersion,
                deployment.StableVersion);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex,
                "Auto-rollback failed for {Project} v{Canary} → v{Stable}",
                deployment.ProjectName,
                deployment.CanaryVersion,
                deployment.StableVersion);
        }
    }
}
