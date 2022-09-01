#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core.Models;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Canary;

/// <summary>
/// Provides useful extension methods for <see cref="CanaryDeploymentEngine"/> to simplify common canary deployment scenarios.
/// </summary>
public static class CanaryDeploymentEngineExtensions
{
    /// <summary>
    /// Attempts to advance the canary deployment to the next rollout step if the current step's soak duration has elapsed.
    /// This method combines health evaluation and step advancement into a single operation for convenience.
    /// </summary>
    /// <param name="engine">The canary deployment engine instance.</param>
    /// <param name="deploymentId">Identifier of the deployment to advance.</param>
    /// <param name="healthEvaluator">Health evaluator for canary metrics.</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if the deployment was advanced; false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="healthEvaluator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="deploymentId"/> is <see langword="null"/> or whitespace.</exception>
    public static async Task<bool> TryAdvanceRolloutAsync(
        this CanaryDeploymentEngine engine,
        string deploymentId,
        ICanaryHealthEvaluator healthEvaluator,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(healthEvaluator);
        ArgumentException.ThrowIfNullOrWhiteSpace(deploymentId);

        var deployment = await engine.GetDeploymentAsync(deploymentId, cancellationToken);

        if (deployment is null)
        {
            logger?.LogWarning("Deployment {DeploymentId} not found", deploymentId);
            return false;
        }

        if (deployment.Status != CanaryStatus.Active)
        {
            logger?.LogDebug("Deployment {DeploymentId} is not in Active state (current: {Status})", deploymentId, deployment.Status);
            return false;
        }

        var activeStep = deployment.ActiveStep;
        if (activeStep is null)
        {
            logger?.LogDebug("No active step found for deployment {DeploymentId}", deploymentId);
            return false;
        }

        // Check if soak duration has elapsed
        if (activeStep.IsSoakComplete)
        {
            logger?.LogInformation(
                "Step {StepNumber} for deployment {DeploymentId} has completed soak duration of {SoakDuration}",
                activeStep.StepNumber,
                deploymentId,
                activeStep.SoakDuration);

            await engine.AdvanceRolloutAsync(deploymentId, cancellationToken);
            return true;
        }

        // Evaluate health to check if we should proceed
        var evaluation = await engine.EvaluateHealthAsync(deploymentId, cancellationToken);

        if (!evaluation.IsHealthy)
        {
            logger?.LogWarning(
                "Cannot advance deployment {DeploymentId} - health check failed: {Reason}",
                deploymentId,
                evaluation.Reason);
            return false;
        }

        logger?.LogDebug(
            "Step {StepNumber} for deployment {DeploymentId} still within soak period. Progress: {ProgressPercent}%",
            activeStep.StepNumber,
            deploymentId,
            deployment.ProgressPercent);

        return false;
    }

    /// <summary>
    /// Promotes the canary deployment if all rollout steps have completed successfully.
    /// This method checks the deployment state and only promotes if the rollout is complete.
    /// </summary>
    /// <param name="engine">The canary deployment engine instance.</param>
    /// <param name="deploymentId">Identifier of the deployment to promote.</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if the deployment was promoted; false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="deploymentId"/> is <see langword="null"/> or whitespace.</exception>
    public static async Task<bool> TryPromoteAsync(
        this CanaryDeploymentEngine engine,
        string deploymentId,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentException.ThrowIfNullOrWhiteSpace(deploymentId);

        var deployment = await engine.GetDeploymentAsync(deploymentId, cancellationToken);

        if (deployment is null)
        {
            logger?.LogWarning("Deployment {DeploymentId} not found", deploymentId);
            return false;
        }

        if (deployment.IsTerminal)
        {
            logger?.LogDebug("Deployment {DeploymentId} is already in terminal state: {Status}", deploymentId, deployment.Status);
            return false;
        }

        if (!deployment.IsRolloutComplete)
        {
            logger?.LogInformation(
                "Cannot promote deployment {DeploymentId} - rollout is not complete. Progress: {ProgressPercent}%",
                deploymentId,
                deployment.ProgressPercent);
            return false;
        }

        logger?.LogInformation(
            "Promoting canary deployment {DeploymentId} for {ProjectName} v{CanaryVersion}",
            deploymentId,
            deployment.ProjectName,
            deployment.CanaryVersion);

        await engine.PromoteAsync(deploymentId, cancellationToken);
        return true;
    }

    /// <summary>
    /// Aborts the canary deployment if it is still active and returns the aborted deployment.
    /// This method provides a convenience wrapper around AbortAsync with a default reason.
    /// </summary>
    /// <param name="engine">The canary deployment engine instance.</param>
    /// <param name="deploymentId">Identifier of the deployment to abort.</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The aborted deployment if successful; null otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="deploymentId"/> is <see langword="null"/> or whitespace.</exception>
    public static async Task<CanaryDeployment?> TryAbortAsync(
        this CanaryDeploymentEngine engine,
        string deploymentId,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentException.ThrowIfNullOrWhiteSpace(deploymentId);

        var deployment = await engine.GetDeploymentAsync(deploymentId, cancellationToken);

        if (deployment is null)
        {
            logger?.LogWarning("Deployment {DeploymentId} not found", deploymentId);
            return null;
        }

        if (deployment.IsTerminal)
        {
            logger?.LogDebug("Deployment {DeploymentId} is already in terminal state: {Status}", deploymentId, deployment.Status);
            return deployment;
        }

        logger?.LogWarning("Aborting canary deployment {DeploymentId} for {ProjectName}", deploymentId, deployment.ProjectName);
        return await engine.AbortAsync(deploymentId, "Manual abort via extension method", cancellationToken);
    }

    /// <summary>
    /// Gets the current canary percentage as a normalized value between 0.0 and 1.0.
    /// </summary>
    /// <param name="engine">The canary deployment engine instance.</param>
    /// <param name="deploymentId">Identifier of the deployment to query.</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <returns>A normalized value between 0.0 and 1.0 representing the canary percentage, or null if deployment not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="deploymentId"/> is <see langword="null"/> or whitespace.</exception>
    public static async Task<double?> GetCanaryPercentageNormalizedAsync(
        this CanaryDeploymentEngine engine,
        string deploymentId,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentException.ThrowIfNullOrWhiteSpace(deploymentId);

        var deployment = await engine.GetDeploymentAsync(deploymentId);

        return deployment is null
            ? null
            : deployment.CurrentSplit.CanaryPercent is >= 0 and <= 100
                ? deployment.CurrentSplit.CanaryPercent / 100.0
                : null;
    }
}