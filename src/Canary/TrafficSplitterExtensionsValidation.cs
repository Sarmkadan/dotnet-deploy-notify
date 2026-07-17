#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Canary;

/// <summary>
/// Provides validation helpers for <see cref="TrafficSplitterExtensions"/> extension methods.
/// Validates parameters passed to extension methods like CreateLinearCanaryDeployment,
/// CreateExponentialCanaryDeployment, etc.
/// </summary>
public sealed class TrafficSplitterExtensionsValidation
{
    /// <summary>
    /// Validates parameters for <see cref="TrafficSplitterExtensions.CreateLinearCanaryDeployment"/> extension method.
    /// </summary>
    /// <param name="projectName">Project name to validate.</param>
    /// <param name="canaryVersion">Canary version to validate.</param>
    /// <param name="stableVersion">Stable version to validate.</param>
    /// <param name="stepCount">Number of steps in the linear rollout (default: 5).</param>
    /// <returns>An empty list if all parameters are valid; otherwise, a list of validation problems.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="projectName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="canaryVersion"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="stableVersion"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> ValidateCreateLinearCanaryDeployment(
        string projectName,
        string canaryVersion,
        string stableVersion,
        int stepCount = 5)
    {
        ArgumentNullException.ThrowIfNull(projectName);
        ArgumentNullException.ThrowIfNull(canaryVersion);
        ArgumentNullException.ThrowIfNull(stableVersion);

    ArgumentOutOfRangeException.ThrowIfLessThan(stepCount, 1);
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(projectName))
        {
            problems.Add("ProjectName cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(canaryVersion))
        {
            problems.Add("CanaryVersion cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(stableVersion))
        {
            problems.Add("StableVersion cannot be null or whitespace.");
        }


        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="TrafficSplitterExtensions.CreateExponentialCanaryDeployment"/> extension method.
    /// </summary>
    /// <param name="projectName">Project name to validate.</param>
    /// <param name="canaryVersion">Canary version to validate.</param>
    /// <param name="stableVersion">Stable version to validate.</param>
    /// <returns>An empty list if all parameters are valid; otherwise, a list of validation problems.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="projectName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="canaryVersion"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="stableVersion"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> ValidateCreateExponentialCanaryDeployment(
        string projectName,
        string canaryVersion,
        string stableVersion)
    {
        ArgumentNullException.ThrowIfNull(projectName);
        ArgumentNullException.ThrowIfNull(canaryVersion);
        ArgumentNullException.ThrowIfNull(stableVersion);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(projectName))
        {
            problems.Add("ProjectName cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(canaryVersion))
        {
            problems.Add("CanaryVersion cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(stableVersion))
        {
            problems.Add("StableVersion cannot be null or whitespace.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="TrafficSplitterExtensions.ShouldProceedToNextStepAsync"/> extension method.
    /// </summary>
    /// <param name="deployment">Canary deployment to validate.</param>
    /// <param name="healthEvaluator">Health evaluator to validate.</param>
    /// <returns>An empty list if all parameters are valid; otherwise, a list of validation problems.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="deployment"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="healthEvaluator"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> ValidateShouldProceedToNextStepAsync(
        CanaryDeployment deployment,
        CanaryHealthEvaluator healthEvaluator)
    {
        ArgumentNullException.ThrowIfNull(deployment);
        ArgumentNullException.ThrowIfNull(healthEvaluator);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(deployment.ProjectName))
        {
            problems.Add("Deployment.ProjectName cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(deployment.StableVersion))
        {
            problems.Add("Deployment.StableVersion cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(deployment.CanaryVersion))
        {
            problems.Add("Deployment.CanaryVersion cannot be null or whitespace.");
        }

        if (deployment.RolloutPlan is null)
        {
            problems.Add("Deployment.RolloutPlan cannot be null.");
        }
        else if (deployment.RolloutPlan.Count == 0)
        {
            problems.Add("Deployment.RolloutPlan must contain at least one step.");
        }

        if (!Enum.IsDefined(deployment.TargetEnvironment))
        {
            problems.Add("Deployment.TargetEnvironment must be a valid Environment value.");
        }

        if (!Enum.IsDefined(deployment.Status))
        {
            problems.Add("Deployment.Status must be a valid CanaryStatus value.");
        }

        if (!Enum.IsDefined(deployment.Strategy))
        {
            problems.Add("Deployment.Strategy must be a valid CanaryStrategy value.");
        }

        if (deployment.CurrentSplit.StablePercent is < 0 or > 100)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "Deployment.CurrentSplit.StablePercent must be between 0 and 100 (was {0:F2}).",
                deployment.CurrentSplit.StablePercent));
        }

        if (deployment.CurrentSplit.CanaryPercent is < 0 or > 100)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "Deployment.CurrentSplit.CanaryPercent must be between 0 and 100 (was {0:F2}).",
                deployment.CurrentSplit.CanaryPercent));
        }

        const double tolerance = 0.01;
        if (Math.Abs(deployment.CurrentSplit.StablePercent + deployment.CurrentSplit.CanaryPercent - 100) > tolerance)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "Deployment.CurrentSplit percentages must sum to 100 (Stable: {0:F2}%, Canary: {1:F2}% vs expected 100%).",
                deployment.CurrentSplit.StablePercent,
                deployment.CurrentSplit.CanaryPercent));
        }

        if (deployment.CreatedAt == default)
        {
            problems.Add("Deployment.CreatedAt must be set.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="TrafficSplitterExtensions.GetCanaryPercentageNormalized"/> extension method.
    /// </summary>
    /// <param name="split">Traffic split to validate.</param>
    /// <returns>An empty list if the traffic split is valid; otherwise, a list of validation problems.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="split"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> ValidateGetCanaryPercentageNormalized(TrafficSplit split)
    {
        ArgumentNullException.ThrowIfNull(split);

        var problems = new List<string>();

        if (split.CanaryPercent < 0 || split.CanaryPercent > 100)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "Split.CanaryPercent must be between 0 and 100 (was {0:F2}).",
                split.CanaryPercent));
        }

        if (split.StablePercent < 0 || split.StablePercent > 100)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "Split.StablePercent must be between 0 and 100 (was {0:F2}).",
                split.StablePercent));
        }

        const double tolerance = 0.01;
        if (Math.Abs(split.StablePercent + split.CanaryPercent - 100) > tolerance)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "Split percentages must sum to 100 (Stable: {0:F2}%, Canary: {1:F2}% vs expected 100%).",
                split.StablePercent,
                split.CanaryPercent));
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="TrafficSplitterExtensions.CreateBlueGreenCanaryDeployment"/> extension method.
    /// </summary>
    /// <param name="projectName">Project name to validate.</param>
    /// <param name="canaryVersion">Canary version to validate.</param>
    /// <param name="stableVersion">Stable version to validate.</param>
    /// <returns>An empty list if all parameters are valid; otherwise, a list of validation problems.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="projectName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="canaryVersion"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="stableVersion"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> ValidateCreateBlueGreenCanaryDeployment(
        string projectName,
        string canaryVersion,
        string stableVersion)
    {
        ArgumentNullException.ThrowIfNull(projectName);
        ArgumentNullException.ThrowIfNull(canaryVersion);
        ArgumentNullException.ThrowIfNull(stableVersion);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(projectName))
        {
            problems.Add("ProjectName cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(canaryVersion))
        {
            problems.Add("CanaryVersion cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(stableVersion))
        {
            problems.Add("StableVersion cannot be null or whitespace.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines if the provided parameters for CreateLinearCanaryDeployment are valid.
    /// </summary>
    /// <param name="projectName">Project name to check.</param>
    /// <param name="canaryVersion">Canary version to check.</param>
    /// <param name="stableVersion">Stable version to check.</param>
    /// <param name="stepCount">Number of steps in the linear rollout.</param>
    /// <returns>True if all parameters are valid; otherwise, false.</returns>
    public static bool IsValidCreateLinearCanaryDeployment(
        string projectName,
        string canaryVersion,
        string stableVersion,
        int stepCount = 5)
        => ValidateCreateLinearCanaryDeployment(projectName, canaryVersion, stableVersion, stepCount).Count == 0;

    /// <summary>
    /// Determines if the provided parameters for CreateExponentialCanaryDeployment are valid.
    /// </summary>
    /// <param name="projectName">Project name to check.</param>
    /// <param name="canaryVersion">Canary version to check.</param>
    /// <param name="stableVersion">Stable version to check.</param>
    /// <returns>True if all parameters are valid; otherwise, false.</returns>
    public static bool IsValidCreateExponentialCanaryDeployment(
        string projectName,
        string canaryVersion,
        string stableVersion)
        => ValidateCreateExponentialCanaryDeployment(projectName, canaryVersion, stableVersion).Count == 0;

    /// <summary>
    /// Determines if the provided parameters for ShouldProceedToNextStepAsync are valid.
    /// </summary>
    /// <param name="deployment">Canary deployment to check.</param>
    /// <param name="healthEvaluator">Health evaluator to check.</param>
    /// <returns>True if all parameters are valid; otherwise, false.</returns>
    public static bool IsValidShouldProceedToNextStepAsync(
        CanaryDeployment deployment,
        CanaryHealthEvaluator healthEvaluator)
        => ValidateShouldProceedToNextStepAsync(deployment, healthEvaluator).Count == 0;

    /// <summary>
    /// Determines if the provided traffic split is valid.
    /// </summary>
    /// <param name="split">Traffic split to check.</param>
    /// <returns>True if the traffic split is valid; otherwise, false.</returns>
    public static bool IsValidGetCanaryPercentageNormalized(TrafficSplit split)
        => ValidateGetCanaryPercentageNormalized(split).Count == 0;

    /// <summary>
    /// Determines if the provided parameters for CreateBlueGreenCanaryDeployment are valid.
    /// </summary>
    /// <param name="projectName">Project name to check.</param>
    /// <param name="canaryVersion">Canary version to check.</param>
    /// <param name="stableVersion">Stable version to check.</param>
    /// <returns>True if all parameters are valid; otherwise, false.</returns>
    public static bool IsValidCreateBlueGreenCanaryDeployment(
        string projectName,
        string canaryVersion,
        string stableVersion)
        => ValidateCreateBlueGreenCanaryDeployment(projectName, canaryVersion, stableVersion).Count == 0;

    /// <summary>
    /// Ensures that the provided parameters for CreateLinearCanaryDeployment are valid,
    /// throwing an <see cref="ArgumentException"/> if any validation fails.
    /// </summary>
    /// <param name="projectName">Project name to validate.</param>
    /// <param name="canaryVersion">Canary version to validate.</param>
    /// <param name="stableVersion">Stable version to validate.</param>
    /// <param name="stepCount">Number of steps in the linear rollout (default: 5).</param>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of all problems.</exception>
    public static void EnsureValidCreateLinearCanaryDeployment(
        string projectName,
        string canaryVersion,
        string stableVersion,
        int stepCount = 5)
    {
        var problems = ValidateCreateLinearCanaryDeployment(projectName, canaryVersion, stableVersion, stepCount);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"CreateLinearCanaryDeployment validation failed:{System.Environment.NewLine}{string.Join(System.Environment.NewLine, problems)}",
                nameof(stepCount));
        }
    }

    /// <summary>
    /// Ensures that the provided parameters for CreateExponentialCanaryDeployment are valid,
    /// throwing an <see cref="ArgumentException"/> if any validation fails.
    /// </summary>
    /// <param name="projectName">Project name to validate.</param>
    /// <param name="canaryVersion">Canary version to validate.</param>
    /// <param name="stableVersion">Stable version to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of all problems.</exception>
    public static void EnsureValidCreateExponentialCanaryDeployment(
        string projectName,
        string canaryVersion,
        string stableVersion)
    {
        var problems = ValidateCreateExponentialCanaryDeployment(projectName, canaryVersion, stableVersion);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"CreateExponentialCanaryDeployment validation failed:{System.Environment.NewLine}{string.Join(System.Environment.NewLine, problems)}",
                nameof(stableVersion));
        }
    }

    /// <summary>
    /// Ensures that the provided parameters for ShouldProceedToNextStepAsync are valid,
    /// throwing an <see cref="ArgumentException"/> if any validation fails.
    /// </summary>
    /// <param name="deployment">Canary deployment to validate.</param>
    /// <param name="healthEvaluator">Health evaluator to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of all problems.</exception>
    public static void EnsureValidShouldProceedToNextStepAsync(
        CanaryDeployment deployment,
        CanaryHealthEvaluator healthEvaluator)
    {
        var problems = ValidateShouldProceedToNextStepAsync(deployment, healthEvaluator);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ShouldProceedToNextStepAsync validation failed:{System.Environment.NewLine}{string.Join(System.Environment.NewLine, problems)}",
                nameof(healthEvaluator));
        }
    }

    /// <summary>
    /// Ensures that the provided traffic split is valid, throwing an <see cref="ArgumentException"/> if invalid.
    /// </summary>
    /// <param name="split">Traffic split to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of all problems.</exception>
    public static void EnsureValidGetCanaryPercentageNormalized(TrafficSplit split)
    {
        var problems = ValidateGetCanaryPercentageNormalized(split);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"TrafficSplit validation failed:{System.Environment.NewLine}{string.Join(System.Environment.NewLine, problems)}",
                nameof(split));
        }
    }

    /// <summary>
    /// Ensures that the provided parameters for CreateBlueGreenCanaryDeployment are valid,
    /// throwing an <see cref="ArgumentException"/> if any validation fails.
    /// </summary>
    /// <param name="projectName">Project name to validate.</param>
    /// <param name="canaryVersion">Canary version to validate.</param>
    /// <param name="stableVersion">Stable version to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of all problems.</exception>
    public static void EnsureValidCreateBlueGreenCanaryDeployment(
        string projectName,
        string canaryVersion,
        string stableVersion)
    {
        var problems = ValidateCreateBlueGreenCanaryDeployment(projectName, canaryVersion, stableVersion);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"CreateBlueGreenCanaryDeployment validation failed:{System.Environment.NewLine}{string.Join(System.Environment.NewLine, problems)}",
                nameof(stableVersion));
        }
    }
}
