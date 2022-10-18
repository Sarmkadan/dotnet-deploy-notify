#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotNetDeployNotify.BackgroundWorkers;

/// <summary>
/// Provides validation helpers for <see cref="NotificationProcessingWorker"/> instances.
/// </summary>
public static class NotificationProcessingWorkerValidation
{
    /// <summary>
    /// Validates the specified <see cref="NotificationProcessingWorker"/> instance.
    /// </summary>
    /// <param name="value">The worker instance to validate.</param>
    /// <returns>A list of validation errors; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this NotificationProcessingWorker? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate statistics from GetStatistics()
        try
        {
            var stats = value.GetStatistics();

            // Validate TotalProcessed (should be non-negative)
            if (stats.TotalProcessed < 0)
            {
                errors.Add(
                    $"TotalProcessed cannot be negative, but was {stats.TotalProcessed}.");
            }

            // Validate SuccessRate (should be between 0 and 1 inclusive)
            if (stats.SuccessRate < 0.0 || stats.SuccessRate > 1.0)
            {
                errors.Add(
                    $"SuccessRate must be between 0.0 and 1.0, but was {stats.SuccessRate.ToString(CultureInfo.InvariantCulture)}.");
            }

            // Validate Uptime (should be non-negative)
            if (stats.Uptime < TimeSpan.Zero)
            {
                errors.Add(
                    $"Uptime cannot be negative, but was {stats.Uptime.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds.");
            }
        }
        catch (Exception ex)
        {
            errors.Add(
                $"Failed to retrieve statistics from worker: {ex.Message}");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="NotificationProcessingWorker"/> is valid.
    /// </summary>
    /// <param name="value">The worker instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this NotificationProcessingWorker? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="NotificationProcessingWorker"/> is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The worker instance to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this NotificationProcessingWorker? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"NotificationProcessingWorker is not valid. Errors:\n\t- {
                string.Join("\n\t- ", errors)
                }".ReplaceLineEndings("\n"));
        }
    }
}