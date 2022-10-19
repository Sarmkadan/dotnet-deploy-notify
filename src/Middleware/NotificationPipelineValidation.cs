using System;
using System.Collections.Generic;

namespace DotNetDeployNotify.Middleware;

/// <summary>
/// Provides validation helpers for <see cref="PipelineResult"/> instances.
/// </summary>
public static class NotificationPipelineValidation
{
    /// <summary>
    /// Validates the specified <see cref="PipelineResult"/> instance.
    /// </summary>
    /// <param name="value">The pipeline result to validate.</param>
    /// <returns>A read-only list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PipelineResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Notification
        if (value.Notification is null)
        {
            errors.Add("Notification is required.");
        }

        // Validate ProcessedNotification
        if (value.ProcessedNotification is null)
        {
            errors.Add("ProcessedNotification is required.");
        }

        // Validate Errors collection
        if (value.Errors is null)
        {
            errors.Add("Errors collection is required.");
        }
        else if (value.Errors.Count > 100) // Reasonable upper bound
        {
            errors.Add("Errors collection exceeds maximum size (100).");
        }

        // Validate Success flag consistency
        if (value.Errors.Count > 0 && value.Success)
        {
            errors.Add("Pipeline cannot be marked as successful when errors exist.");
        }

        if (value.Errors.Count == 0 && !value.Success)
        {
            errors.Add("Pipeline should be marked as successful when no errors exist.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="PipelineResult"/> is valid.
    /// </summary>
    /// <param name="value">The pipeline result to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this PipelineResult value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="PipelineResult"/> is valid.
    /// </summary>
    /// <param name="value">The pipeline result to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the pipeline result is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this PipelineResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "PipelineResult validation failed:\n" + string.Join("\n", errors),
                nameof(value));
        }
    }
}