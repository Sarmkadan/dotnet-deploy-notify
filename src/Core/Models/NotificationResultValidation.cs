#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides validation helpers for <see cref="NotificationResult"/> instances
/// </summary>
public static class NotificationResultValidation
{
    /// <summary>
    /// Validates the notification result and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The notification result to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this NotificationResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.NotificationId))
        {
            problems.Add("NotificationId is required and cannot be null, empty, or whitespace");
        }

        if (string.IsNullOrWhiteSpace(value.ConfigurationId))
        {
            problems.Add("ConfigurationId is required and cannot be null, empty, or whitespace");
        }

        if (string.IsNullOrWhiteSpace(value.ResponseBody))
        {
            problems.Add("ResponseBody is required and cannot be null, empty, or whitespace");
        }

        // Validate HttpStatusCode if set
        if (value.HttpStatusCode.HasValue)
        {
            if (value.HttpStatusCode < 100 || value.HttpStatusCode > 599)
            {
                problems.Add("HttpStatusCode must be a valid HTTP status code (100-599)");
            }
        }

        // Validate AttemptNumber
        if (value.AttemptNumber < 1)
        {
            problems.Add("AttemptNumber must be at least 1");
        }

        // Validate DurationMs
        if (value.DurationMs < 0)
        {
            problems.Add("DurationMs cannot be negative");
        }

        // Validate AttemptedAt is not default
        if (value.AttemptedAt == default)
        {
            problems.Add("AttemptedAt must be set to a valid DateTime");
        }

        // Validate LastRetryAt if set
        if (value.LastRetryAt.HasValue)
        {
            if (value.LastRetryAt.Value == default)
            {
                problems.Add("LastRetryAt must be a valid DateTime if set");
            }
            else if (value.LastRetryAt.Value > DateTime.UtcNow)
            {
                problems.Add("LastRetryAt cannot be in the future");
            }
        }

        // Validate NextRetryAt if set
        if (value.NextRetryAt.HasValue)
        {
            if (value.NextRetryAt.Value == default)
            {
                problems.Add("NextRetryAt must be a valid DateTime if set");
            }
            else if (value.NextRetryAt.Value < DateTime.UtcNow)
            {
                problems.Add("NextRetryAt cannot be in the past");
            }
            else if (value.LastRetryAt.HasValue && value.NextRetryAt.Value <= value.LastRetryAt.Value)
            {
                problems.Add("NextRetryAt must be after LastRetryAt");
            }
        }

        // Validate Channel is not default
        if (value.Channel == default)
        {
            problems.Add("Channel must be set to a valid NotificationChannel value");
        }

        // Validate Status is not default
        if (value.Status == default)
        {
            problems.Add("Status must be set to a valid DeliveryStatus value");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the notification result is valid
    /// </summary>
    /// <param name="value">The notification result to check</param>
    /// <returns>True if valid; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this NotificationResult value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures the notification result is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The notification result to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, with details</exception>
    public static void EnsureValid(this NotificationResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        ArgumentNullException.ThrowIfNull(problems);

        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"NotificationResult is invalid. Problems:\n{string.Join("\n", problems)}");
    }
}