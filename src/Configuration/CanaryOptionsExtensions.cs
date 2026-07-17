using System;
using DotNetDeployNotify.Core;

namespace DotNetDeployNotify.Configuration;

/// <summary>
/// Extensions for <see cref="CanaryOptions"/>.
/// </summary>
public static class CanaryOptionsExtensions
{
    /// <summary>
    /// Validates the canary deployment configuration.
    /// </summary>
    /// <param name="options">The options to validate.</param>
    /// <returns><see langword="true"/> if the configuration is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this CanaryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.LinearStepCount is >= 2 and <= 20
            && options.StepSoakDuration >= TimeSpan.Zero
            && options.MaxDeploymentDuration > TimeSpan.Zero
            && options.StepSoakDuration != TimeSpan.Zero
            && options.MaxDeploymentDuration != TimeSpan.Zero;
    }

    /// <summary>
    /// Calculates the total potential soak time for the deployment based on the linear step configuration.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <returns>A <see cref="TimeSpan"/> representing the total soak duration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>. </exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="options"/>.StepSoakDuration is zero or negative.</exception>
    public static TimeSpan GetTotalSoakTime(this CanaryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.StepSoakDuration.Ticks > 0
            ? TimeSpan.FromTicks(options.StepSoakDuration.Ticks * options.LinearStepCount)
            : throw new ArgumentException(
                "StepSoakDuration must be a positive time span.",
                nameof(options));
    }
}