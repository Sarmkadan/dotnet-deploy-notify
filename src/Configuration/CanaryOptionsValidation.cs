#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetDeployNotify.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="CanaryOptions"/> configuration.
/// </summary>
public static class CanaryOptionsValidation
{
    /// <summary>
    /// Validates the provided <see cref="CanaryOptions"/> instance.
    /// </summary>
    /// <param name="value">The configuration to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CanaryOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate boolean flags - these don't have specific constraints beyond being boolean
        // Enabled: no validation needed beyond null check
        // AutoRollbackOnFailure: no validation needed beyond null check
        // AutoAdvanceOnSuccess: no validation needed beyond null check

        // Validate LinearStepCount: must be in range [2, 20]
        if (value.LinearStepCount < 2 || value.LinearStepCount > 20)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "CanaryOptions.LinearStepCount must be between 2 and 20, but was {0}.",
                value.LinearStepCount));
        }

        // Validate StepSoakDuration: must be positive
        if (value.StepSoakDuration <= TimeSpan.Zero)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "CanaryOptions.StepSoakDuration must be positive, but was {0} seconds.",
                value.StepSoakDuration.TotalSeconds));
        }

        // Validate MaxDeploymentDuration: must be positive
        if (value.MaxDeploymentDuration <= TimeSpan.Zero)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "CanaryOptions.MaxDeploymentDuration must be positive, but was {0} hours.",
                value.MaxDeploymentDuration.TotalHours));
        }

        // Validate Thresholds: must not be null
        if (value.Thresholds is null)
        {
            problems.Add("CanaryOptions.Thresholds is required and cannot be null.");
        }
        else
        {
            problems.AddRange(value.Thresholds.Validate());
        }

        // Validate AlertPriority: must be a valid enum value
        if (!Enum.IsDefined(typeof(NotificationPriority), value.AlertPriority))
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "CanaryOptions.AlertPriority must be a valid NotificationPriority value, but was {0}.",
                value.AlertPriority));
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the provided <see cref="CanaryOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The configuration to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this CanaryOptions value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the provided <see cref="CanaryOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The configuration to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the configuration contains validation problems.</exception>
    public static void EnsureValid(this CanaryOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "CanaryOptions validation failed:{0}{1}",
                    "\n",
                    string.Join("\n", problems)));
        }
    }
}
