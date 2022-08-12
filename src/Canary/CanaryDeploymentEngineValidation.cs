#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Globalization;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Canary;

/// <summary>
/// Provides validation helpers for <see cref="CanaryDeploymentEngine"/> instances.
/// Validates required fields, ranges, and business rules for canary deployments.
/// </summary>
public static class CanaryDeploymentEngineValidation
{
    /// <summary>
    /// Validates the specified <see cref="CanaryDeploymentEngine"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The canary deployment engine instance to validate</param>
    /// <returns>An immutable list of validation error messages; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> Validate(this CanaryDeploymentEngine value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Get active deployments to validate
        var activeDeployments = value.GetActiveDeploymentsAsync(CancellationToken.None).Result;
        var deploymentHistory = value.GetDeploymentHistoryAsync(string.Empty, 100, CancellationToken.None).Result;

        // Validate active deployments
        foreach (var deployment in activeDeployments)
        {
            ValidateDeployment(deployment, errors);
        }

        // Validate deployment history for any obvious issues
        foreach (var deployment in deploymentHistory)
        {
            ValidateDeployment(deployment, errors);
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a single canary deployment instance.
    /// </summary>
    private static void ValidateDeployment(CanaryDeployment deployment, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(deployment.ProjectName))
            errors.Add($"Deployment {deployment.Id}: ProjectName is required.");

        if (string.IsNullOrWhiteSpace(deployment.StableVersion))
            errors.Add($"Deployment {deployment.Id}: StableVersion is required.");

        if (string.IsNullOrWhiteSpace(deployment.CanaryVersion))
            errors.Add($"Deployment {deployment.Id}: CanaryVersion is required.");

        if (deployment.StableVersion == deployment.CanaryVersion)
            errors.Add($"Deployment {deployment.Id}: CanaryVersion must differ from StableVersion.");

        if (!Enum.IsDefined(typeof(Environment), deployment.TargetEnvironment))
            errors.Add($"Deployment {deployment.Id}: TargetEnvironment must be a valid Environment value.");

        if (!Enum.IsDefined(typeof(CanaryStatus), deployment.Status))
            errors.Add($"Deployment {deployment.Id}: Status must be a valid CanaryStatus value.");

        if (!Enum.IsDefined(typeof(CanaryStrategy), deployment.Strategy))
            errors.Add($"Deployment {deployment.Id}: Strategy must be a valid CanaryStrategy value.");

        // Validate rollout plan
        if (deployment.RolloutPlan is null)
            errors.Add($"Deployment {deployment.Id}: RolloutPlan cannot be null.");
        else if (deployment.RolloutPlan.Count == 0)
            errors.Add($"Deployment {deployment.Id}: RolloutPlan must contain at least one step.");
        else
        {
            foreach (var step in deployment.RolloutPlan)
            {
                if (step.StepNumber <= 0)
                    errors.Add($"Deployment {deployment.Id}, Step {step.StepNumber}: StepNumber must be positive.");

                if (step.CanaryPercent < 0 || step.CanaryPercent > 100)
                    errors.Add($"Deployment {deployment.Id}, Step {step.StepNumber}: CanaryPercent must be between 0 and 100.");

                if (step.SoakDuration.TotalSeconds <= 0)
                    errors.Add($"Deployment {deployment.Id}, Step {step.StepNumber}: SoakDuration must be positive.");

                if (!Enum.IsDefined(typeof(RolloutStepStatus), step.Status))
                    errors.Add($"Deployment {deployment.Id}, Step {step.StepNumber}: Status must be a valid RolloutStepStatus value.");
            }
        }

        // Validate traffic split
        if (deployment.CurrentSplit.StablePercent < 0 || deployment.CurrentSplit.StablePercent > 100)
            errors.Add($"Deployment {deployment.Id}: CurrentSplit.StablePercent must be between 0 and 100.");

        if (deployment.CurrentSplit.CanaryPercent < 0 || deployment.CurrentSplit.CanaryPercent > 100)
            errors.Add($"Deployment {deployment.Id}: CurrentSplit.CanaryPercent must be between 0 and 100.");

        if (Math.Abs(deployment.CurrentSplit.StablePercent + deployment.CurrentSplit.CanaryPercent - 100) > 0.01)
            errors.Add($"Deployment {deployment.Id}: CurrentSplit percentages must sum to 100 (Stable: {deployment.CurrentSplit.StablePercent:F2}%, Canary: {deployment.CurrentSplit.CanaryPercent:F2}% vs expected 100%).");

        // Validate timestamps
        if (deployment.CreatedAt == default)
            errors.Add($"Deployment {deployment.Id}: CreatedAt must be set.");

        if (deployment.Status == CanaryStatus.Promoted && deployment.PromotedAt == default)
            errors.Add($"Deployment {deployment.Id}: PromotedAt must be set when status is Promoted.");

        if (deployment.Status == CanaryStatus.Aborted && deployment.AbortedAt == default)
            errors.Add($"Deployment {deployment.Id}: AbortedAt must be set when status is Aborted.");

        if (deployment.ActiveStep?.StartedAt > DateTime.UtcNow)
            errors.Add($"Deployment {deployment.Id}: ActiveStep.StartedAt cannot be in the future.");

        if (deployment.ActiveStep?.CompletedAt > DateTime.UtcNow)
            errors.Add($"Deployment {deployment.Id}: ActiveStep.CompletedAt cannot be in the future.");

        if (deployment.ActiveStep?.CompletedAt < deployment.ActiveStep?.StartedAt)
            errors.Add($"Deployment {deployment.Id}: ActiveStep.CompletedAt cannot be before StartedAt.");
    }

    /// <summary>
    /// Determines whether the specified <see cref="CanaryDeploymentEngine"/> instance is valid.
    /// </summary>
    /// <param name="value">The canary deployment engine instance to check</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/></exception>
    public static bool IsValid(this CanaryDeploymentEngine value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="CanaryDeploymentEngine"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed error message if validation fails.
    /// </summary>
    /// <param name="value">The canary deployment engine instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of all problems</exception>
    public static void EnsureValid(this CanaryDeploymentEngine value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"CanaryDeploymentEngine validation failed:{System.Environment.NewLine}{string.Join($"{System.Environment.NewLine}", errors)}",
                nameof(value));
        }
    }
}