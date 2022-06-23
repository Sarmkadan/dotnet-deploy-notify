// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Canary;

/// <summary>
/// Orchestrates canary deployments: initiating rollouts, advancing traffic steps,
/// evaluating health signals, and driving promote or abort decisions.
/// </summary>
public interface ICanaryDeploymentService
{
    /// <summary>
    /// Creates and activates a new canary deployment from the supplied request.
    /// Generates a strategy-appropriate rollout plan, starts the first step,
    /// and dispatches a start notification to all configured channels.
    /// </summary>
    /// <param name="request">Deployment parameters including versions, environment, and strategy.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The created <see cref="CanaryDeployment"/> with its initial state populated.</returns>
    Task<CanaryDeployment> StartCanaryAsync(
        CanaryDeploymentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes the current rollout step and activates the next one, updating the live
    /// traffic split and dispatching a step-progress notification.
    /// When the final step completes and <c>AutoAdvanceOnSuccess</c> is enabled,
    /// automatically calls <see cref="PromoteAsync"/>.
    /// </summary>
    /// <param name="deploymentId">Identifier of the active deployment to advance.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated <see cref="CanaryDeployment"/>.</returns>
    Task<CanaryDeployment> AdvanceRolloutAsync(
        string deploymentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Promotes the canary to receive 100% of production traffic, marks the deployment
    /// as <see cref="CanaryStatus.Promoted"/>, and dispatches a success notification.
    /// Any remaining pending steps are skipped.
    /// </summary>
    /// <param name="deploymentId">Identifier of the deployment to promote.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The promoted <see cref="CanaryDeployment"/>.</returns>
    Task<CanaryDeployment> PromoteAsync(
        string deploymentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aborts the deployment, resets traffic to 100% stable, marks the deployment
    /// as <see cref="CanaryStatus.Aborted"/>, and — when
    /// <c>AutoRollbackOnFailure</c> is enabled — triggers a rollback notification
    /// via the rollback service.
    /// </summary>
    /// <param name="deploymentId">Identifier of the deployment to abort.</param>
    /// <param name="reason">Human-readable explanation for the abort.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The aborted <see cref="CanaryDeployment"/>.</returns>
    Task<CanaryDeployment> AbortAsync(
        string deploymentId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Collects live health metrics, compares them against configured thresholds,
    /// updates the deployment's metric snapshots, and — when violations are detected
    /// and <c>AutoRollbackOnFailure</c> is enabled — automatically calls
    /// <see cref="AbortAsync"/>.
    /// </summary>
    /// <param name="deploymentId">Identifier of the deployment to evaluate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="CanaryEvaluationResult"/> describing the health outcome.</returns>
    Task<CanaryEvaluationResult> EvaluateHealthAsync(
        string deploymentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the deployment with the given identifier, or <see langword="null"/> if not found.
    /// </summary>
    Task<CanaryDeployment?> GetDeploymentAsync(
        string deploymentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all deployments that are not yet in a terminal state
    /// (<see cref="CanaryStatus.Promoted"/> or <see cref="CanaryStatus.Aborted"/>),
    /// ordered by creation time ascending.
    /// </summary>
    Task<List<CanaryDeployment>> GetActiveDeploymentsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the deployment history for a project, ordered most-recent first.
    /// </summary>
    /// <param name="projectName">Project name to filter by.</param>
    /// <param name="limit">Maximum number of records to return.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<List<CanaryDeployment>> GetDeploymentHistoryAsync(
        string projectName,
        int limit = 50,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Computes per-request routing decisions and generates per-strategy rollout plans.
/// </summary>
public interface ITrafficSplitter
{
    /// <summary>
    /// Derives the <see cref="TrafficSplit"/> that should be applied given
    /// the deployment's currently active rollout step.
    /// </summary>
    /// <param name="deployment">The canary deployment whose active step should be consulted.</param>
    /// <returns>The target <see cref="TrafficSplit"/>, or the current split if no step is active.</returns>
    TrafficSplit ComputeNextSplit(CanaryDeployment deployment);

    /// <summary>
    /// Returns <see langword="true"/> when a single inbound request should be routed
    /// to the canary version, using the supplied split percentages for a probabilistic decision.
    /// </summary>
    /// <param name="split">Current traffic distribution.</param>
    bool ShouldRouteToCanary(TrafficSplit split);

    /// <summary>
    /// Generates a complete, ordered list of <see cref="CanaryRolloutStep"/> objects
    /// appropriate for the given strategy using the current options configuration.
    /// </summary>
    /// <param name="strategy">The traffic-splitting algorithm to generate steps for.</param>
    List<CanaryRolloutStep> GenerateRolloutPlan(CanaryStrategy strategy);
}

/// <summary>
/// Collects live health metrics and evaluates whether a canary version is safe
/// to continue receiving traffic.
/// </summary>
public interface ICanaryHealthEvaluator
{
    /// <summary>
    /// Runs a full health evaluation, collecting live metrics for both the stable and
    /// canary versions and comparing them against all configured thresholds.
    /// </summary>
    /// <param name="deployment">The deployment whose health should be assessed.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="CanaryEvaluationResult"/> with violation details and rollback recommendation.</returns>
    Task<CanaryEvaluationResult> EvaluateAsync(
        CanaryDeployment deployment,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Collects a <see cref="CanaryMetrics"/> snapshot for a specific version and environment.
    /// Implementations may query Prometheus, Datadog, CloudWatch, Application Insights, or any
    /// other observability backend.
    /// </summary>
    /// <param name="version">The deployment version to collect metrics for.</param>
    /// <param name="environment">The environment from which to pull metrics.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<CanaryMetrics> CollectMetricsAsync(
        string version,
        Environment environment,
        CancellationToken cancellationToken = default);
}
