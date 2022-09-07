#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text;

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides extension methods for <see cref="CanaryDeployment"/> to simplify common operations
/// </summary>
public static class CanaryDeploymentExtensions
{
    /// <summary>
    /// Determines whether the canary deployment is currently active and receiving traffic
    /// </summary>
    /// <param name="deployment">The canary deployment instance</param>
    /// <returns><see langword="true"/> if the deployment status is Active; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="deployment"/> is <see langword="null"/></exception>
    public static bool IsActive(this CanaryDeployment deployment)
    {
        ArgumentNullException.ThrowIfNull(deployment);
        return deployment.Status == CanaryStatus.Active;
    }

    /// <summary>
    /// Determines whether the canary deployment has been successfully promoted to full production traffic
    /// </summary>
    /// <param name="deployment">The canary deployment instance</param>
    /// <returns><see langword="true"/> if the deployment status is Promoted; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="deployment"/> is <see langword="null"/></exception>
    public static bool IsPromoted(this CanaryDeployment deployment)
    {
        ArgumentNullException.ThrowIfNull(deployment);
        return deployment.Status == CanaryStatus.Promoted;
    }

    /// <summary>
    /// Determines whether the canary deployment has been aborted or failed
    /// </summary>
    /// <param name="deployment">The canary deployment instance</param>
    /// <returns><see langword="true"/> if the deployment status is Aborted or RollingBack; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="deployment"/> is <see langword="null"/></exception>
    public static bool IsFailedOrAborted(this CanaryDeployment deployment)
    {
        ArgumentNullException.ThrowIfNull(deployment);
        return deployment.Status is CanaryStatus.Aborted or CanaryStatus.RollingBack;
    }

    /// <summary>
    /// Gets the current traffic split as a formatted string for display purposes
    /// </summary>
    /// <param name="deployment">The canary deployment instance</param>
    /// <returns>A formatted string representing the current traffic split</returns>
    /// <exception cref="ArgumentNullException"><paramref name="deployment"/> is <see langword="null"/></exception>
    public static string GetTrafficSplitDisplay(this CanaryDeployment deployment)
    {
        ArgumentNullException.ThrowIfNull(deployment);
        return $"Stable: {deployment.CurrentSplit.StablePercent}% | Canary: {deployment.CurrentSplit.CanaryPercent}%";
    }

    /// <summary>
    /// Calculates the overall health score of the canary deployment based on error rates and latency
    /// </summary>
    /// <param name="deployment">The canary deployment instance</param>
    /// <returns>A health score between 0 (unhealthy) and 100 (healthy)</returns>
    /// <exception cref="ArgumentNullException"><paramref name="deployment"/> is <see langword="null"/></exception>
    public static double CalculateHealthScore(this CanaryDeployment deployment)
    {
        ArgumentNullException.ThrowIfNull(deployment);

        // Weight error rate more heavily than latency
        const double errorRateWeight = 0.6;
        const double latencyWeight = 0.4;

        // Calculate error rate score (100 when error rate is 0%, 0 when error rate is 100%)
        double errorRateScore = Math.Max(0, 100 - deployment.CanaryMetrics.ErrorRatePercent * 2);

        // Calculate latency score (100 when latency is 0ms, 0 when latency is 500ms or more)
        double maxLatency = 500;
        double latencyScore = Math.Max(0, 100 - (deployment.CanaryMetrics.P95LatencyMs / maxLatency * 100));

        // Weighted average
        double healthScore = (errorRateScore * errorRateWeight) + (latencyScore * latencyWeight);
        return Math.Round(healthScore, 2);
    }

    /// <summary>
    /// Gets a summary of the deployment status suitable for logging or notifications
    /// </summary>
    /// <param name="deployment">The canary deployment instance</param>
    /// <returns>A formatted status summary string</returns>
    /// <exception cref="ArgumentNullException"><paramref name="deployment"/> is <see langword="null"/></exception>
    public static string GetStatusSummary(this CanaryDeployment deployment)
    {
        ArgumentNullException.ThrowIfNull(deployment);

        var sb = new StringBuilder();
        sb.AppendLine($"Canary Deployment: {deployment.ProjectName}");
        sb.AppendLine($"ID: {deployment.Id}");
        sb.AppendLine($"Status: {deployment.Status}");
        sb.AppendLine($"Environment: {deployment.TargetEnvironment}");
        sb.AppendLine($"Traffic: {deployment.GetTrafficSplitDisplay()}");
        sb.AppendLine($"Progress: {deployment.ProgressPercent:F1}%");
        sb.AppendLine($"Health Score: {deployment.CalculateHealthScore():F1}/100");

        if (!string.IsNullOrEmpty(deployment.AbortReason))
        {
            sb.AppendLine($"Abort Reason: {deployment.AbortReason}");
        }

        sb.AppendLine($"Created: {deployment.CreatedAt:yyyy-MM-dd HH:mm:ss}");

        if (deployment.PromotedAt.HasValue)
        {
            sb.AppendLine($"Promoted: {deployment.PromotedAt.Value:yyyy-MM-dd HH:mm:ss}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Determines whether the canary deployment can be safely promoted to full production traffic
    /// </summary>
    /// <param name="deployment">The canary deployment instance</param>
    /// <param name="errorRateThreshold">Maximum acceptable error rate percentage (default: 1.0)</param>
    /// <param name="p95LatencyThresholdMs">Maximum acceptable 95th percentile latency in milliseconds (default: 500)</param>
    /// <returns><see langword="true"/> if the canary metrics meet promotion criteria; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="deployment"/> is <see langword="null"/></exception>
    public static bool CanPromote(this CanaryDeployment deployment,
        double errorRateThreshold = 1.0,
        double p95LatencyThresholdMs = 500)
    {
        ArgumentNullException.ThrowIfNull(deployment);

        // Can only promote if rollout is complete and deployment is active
        if (deployment.Status != CanaryStatus.Active || !deployment.IsRolloutComplete)
        {
            return false;
        }

        // Check error rate threshold
        if (deployment.CanaryMetrics.ErrorRatePercent > errorRateThreshold)
        {
            return false;
        }

        // Check latency threshold
        if (deployment.CanaryMetrics.P95LatencyMs > p95LatencyThresholdMs)
        {
            return false;
        }

        // Check that canary metrics are actually available
        if (deployment.CanaryMetrics.RequestCount == 0)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the next traffic percentage step based on the current rollout plan and strategy
    /// </summary>
    /// <param name="deployment">The canary deployment instance</param>
    /// <returns>The next target canary traffic percentage, or null if no more steps are available</returns>
    /// <exception cref="ArgumentNullException"><paramref name="deployment"/> is <see langword="null"/></exception>
    public static double? GetNextTrafficPercentage(this CanaryDeployment deployment)
    {
        ArgumentNullException.ThrowIfNull(deployment);

        if (deployment.RolloutPlan.Count == 0)
        {
            return null;
        }

        // Find the next pending step
        var nextStep = deployment.NextStep;
        return nextStep?.CanaryPercent;
    }

    /// <summary>
    /// Gets the duration remaining for the current active soak step
    /// </summary>
    /// <param name="deployment">The canary deployment instance</param>
    /// <returns>TimeSpan representing remaining duration, or null if no active step or soak not started</returns>
    /// <exception cref="ArgumentNullException"><paramref name="deployment"/> is <see langword="null"/></exception>
    public static TimeSpan? GetCurrentSoakRemaining(this CanaryDeployment deployment)
    {
        ArgumentNullException.ThrowIfNull(deployment);

        var activeStep = deployment.ActiveStep;
        if (activeStep?.StartedAt == null || activeStep.Status != RolloutStepStatus.InProgress)
        {
            return null;
        }

        var elapsed = DateTime.UtcNow - activeStep.StartedAt.Value;
        var remaining = activeStep.SoakDuration - elapsed;

        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Determines whether the current soak duration has been completed for the active step
    /// </summary>
    /// <param name="deployment">The canary deployment instance</param>
    /// <returns><see langword="true"/> if the current soak is complete; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="deployment"/> is <see langword="null"/></exception>
    public static bool IsCurrentSoakComplete(this CanaryDeployment deployment)
    {
        ArgumentNullException.ThrowIfNull(deployment);
        return deployment.ActiveStep?.IsSoakComplete ?? false;
    }
}