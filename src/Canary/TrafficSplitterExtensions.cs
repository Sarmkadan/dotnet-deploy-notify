#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core.Models;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Canary;

/// <summary>
/// Provides useful extension methods for <see cref="TrafficSplitter"/> to simplify common canary deployment scenarios.
/// </summary>
public static class TrafficSplitterExtensions
{
    /// <summary>
    /// Creates a canary deployment with a linear rollout strategy.
    /// </summary>
    /// <param name="splitter">The traffic splitter instance.</param>
    /// <param name="projectName">Name of the project being deployed.</param>
    /// <param name="canaryVersion">Version identifier for the canary.</param>
    /// <param name="stableVersion">Version identifier for the stable baseline.</param>
    /// <param name="targetEnvironment">Environment where deployment occurs.</param>
    /// <param name="stepCount">Number of steps in the linear rollout (default: 5).</param>
    /// <returns>A configured canary deployment ready for evaluation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="splitter"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="projectName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="canaryVersion"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="stableVersion"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="stepCount"/> is less than 1.</exception>
    public static CanaryDeployment CreateLinearCanaryDeployment(
        this TrafficSplitter splitter,
        string projectName,
        string canaryVersion,
        string stableVersion,
        Environment targetEnvironment,
        int stepCount = 5)
    {
        ArgumentNullException.ThrowIfNull(splitter);
        ArgumentNullException.ThrowIfNull(projectName);
        ArgumentNullException.ThrowIfNull(canaryVersion);
        ArgumentNullException.ThrowIfNull(stableVersion);

        if (stepCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(stepCount), stepCount, "Step count must be at least 1.");
        }

        var rolloutPlan = splitter.GenerateRolloutPlan(CanaryStrategy.Linear);

        return new CanaryDeployment
        {
            ProjectName = projectName,
            CanaryVersion = canaryVersion,
            StableVersion = stableVersion,
            TargetEnvironment = targetEnvironment,
            Strategy = CanaryStrategy.Linear,
            RolloutPlan = rolloutPlan,
            CurrentSplit = TrafficSplit.FromCanaryPercent(0)
        };
    }

    /// <summary>
    /// Creates a canary deployment with an exponential rollout strategy.
    /// </summary>
    /// <param name="splitter">The traffic splitter instance.</param>
    /// <param name="projectName">Name of the project being deployed.</param>
    /// <param name="canaryVersion">Version identifier for the canary.</param>
    /// <param name="stableVersion">Version identifier for the stable baseline.</param>
    /// <param name="targetEnvironment">Environment where deployment occurs.</param>
    /// <returns>A configured canary deployment with exponential rollout plan.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="splitter"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="projectName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="canaryVersion"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="stableVersion"/> is <see langword="null"/>.</exception>
    public static CanaryDeployment CreateExponentialCanaryDeployment(
        this TrafficSplitter splitter,
        string projectName,
        string canaryVersion,
        string stableVersion,
        Environment targetEnvironment)
    {
        ArgumentNullException.ThrowIfNull(splitter);
        ArgumentNullException.ThrowIfNull(projectName);
        ArgumentNullException.ThrowIfNull(canaryVersion);
        ArgumentNullException.ThrowIfNull(stableVersion);

        var rolloutPlan = splitter.GenerateRolloutPlan(CanaryStrategy.Exponential);

        return new CanaryDeployment
        {
            ProjectName = projectName,
            CanaryVersion = canaryVersion,
            StableVersion = stableVersion,
            TargetEnvironment = targetEnvironment,
            Strategy = CanaryStrategy.Exponential,
            RolloutPlan = rolloutPlan,
            CurrentSplit = TrafficSplit.FromCanaryPercent(0)
        };
    }

    /// <summary>
    /// Determines if the current canary deployment should proceed to the next rollout step.
    /// </summary>
    /// <param name="splitter">The traffic splitter instance.</param>
    /// <param name="deployment">The canary deployment to evaluate.</param>
    /// <param name="healthEvaluator">Health evaluator for canary metrics.</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <returns>True if deployment should proceed to next step; false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="splitter"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="deployment"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="healthEvaluator"/> is <see langword="null"/>.</exception>
    public static async Task<bool> ShouldProceedToNextStepAsync(
        this TrafficSplitter splitter,
        CanaryDeployment deployment,
        CanaryHealthEvaluator healthEvaluator,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(splitter);
        ArgumentNullException.ThrowIfNull(deployment);
        ArgumentNullException.ThrowIfNull(healthEvaluator);

        if (deployment.RolloutPlan.Count == 0)
        {
            logger?.LogWarning("No rollout plan available for {Project}", deployment.ProjectName);
            return false;
        }

        var currentStep = deployment.ActiveStep;
        if (currentStep is null)
        {
            logger?.LogDebug("No active step found for {Project}", deployment.ProjectName);
            return false;
        }

        // Collect metrics for both versions
        var evaluation = await healthEvaluator.EvaluateAsync(deployment);

        logger?.LogInformation(
            "Evaluating deployment for {Project} step {Step}: healthy={IsHealthy}",
            deployment.ProjectName,
            currentStep.StepNumber,
            evaluation.IsHealthy);

        if (!evaluation.IsHealthy)
        {
            logger?.LogWarning(
                "Canary deployment for {Project} step {Step} is unhealthy: {Reason}",
                deployment.ProjectName,
                currentStep.StepNumber,
                evaluation.Reason);
            return false;
        }

        // Check if we've completed the soak duration for this step
        if (currentStep.SoakDuration > TimeSpan.Zero &&
            currentStep.StartedAt.HasValue &&
            DateTime.UtcNow - currentStep.StartedAt.Value >= currentStep.SoakDuration)
        {
            logger?.LogInformation(
                "Step {Step} for {Project} has completed soak duration of {Soak}",
                currentStep.StepNumber,
                deployment.ProjectName,
                currentStep.SoakDuration);
            return true;
        }

        logger?.LogDebug(
            "Step {Step} for {Project} still within soak period",
            currentStep.StepNumber,
            deployment.ProjectName);
        return false;
    }

    /// <summary>
    /// Gets the current canary percentage as a normalized value between 0.0 and 1.0.
    /// </summary>
    /// <param name="splitter">The traffic splitter instance.</param>
    /// <param name="split">The traffic split to normalize.</param>
    /// <returns>A normalized value between 0.0 and 1.0 representing the canary percentage.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="splitter"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="split"/> is <see langword="null"/>.</exception>
    public static double GetCanaryPercentageNormalized(this TrafficSplitter splitter, TrafficSplit split)
    {
        ArgumentNullException.ThrowIfNull(splitter);
        ArgumentNullException.ThrowIfNull(split);

        return split.CanaryPercent / 100.0;
    }

    /// <summary>
    /// Creates a blue-green deployment strategy with two steps (50% then 100%).
    /// </summary>
    /// <param name="splitter">The traffic splitter instance.</param>
    /// <param name="projectName">Name of the project being deployed.</param>
    /// <param name="canaryVersion">Version identifier for the canary.</param>
    /// <param name="stableVersion">Version identifier for the stable baseline.</param>
    /// <param name="targetEnvironment">Environment where deployment occurs.</param>
    /// <returns>A configured canary deployment with blue-green rollout plan.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="splitter"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="projectName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="canaryVersion"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="stableVersion"/> is <see langword="null"/>.</exception>
    public static CanaryDeployment CreateBlueGreenCanaryDeployment(
        this TrafficSplitter splitter,
        string projectName,
        string canaryVersion,
        string stableVersion,
        Environment targetEnvironment)
    {
        ArgumentNullException.ThrowIfNull(splitter);
        ArgumentNullException.ThrowIfNull(projectName);
        ArgumentNullException.ThrowIfNull(canaryVersion);
        ArgumentNullException.ThrowIfNull(stableVersion);

        var rolloutPlan = splitter.GenerateRolloutPlan(CanaryStrategy.BlueGreen);

        return new CanaryDeployment
        {
            ProjectName = projectName,
            CanaryVersion = canaryVersion,
            StableVersion = stableVersion,
            TargetEnvironment = targetEnvironment,
            Strategy = CanaryStrategy.BlueGreen,
            RolloutPlan = rolloutPlan,
            CurrentSplit = TrafficSplit.FromCanaryPercent(0)
        };
    }
}