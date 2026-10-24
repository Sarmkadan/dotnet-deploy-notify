#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using DotNetDeployNotify.Validation;

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
        ValidationGuard.CheckRequired(problems, value.NotificationId, nameof(value.NotificationId));
        ValidationGuard.CheckRequired(problems, value.ConfigurationId, nameof(value.ConfigurationId));
        ValidationGuard.CheckRequired(problems, value.ResponseBody, nameof(value.ResponseBody));

        // Validate HttpStatusCode if set (a null code is not reported)
        if (value.HttpStatusCode is < 100 or > 599)
        {
            problems.Add("HttpStatusCode must be a valid HTTP status code (100-599)");
        }

        // Validate AttemptNumber
        ValidationGuard.CheckPositive(problems, value.AttemptNumber, nameof(value.AttemptNumber));

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

        // Validate enum properties against their defined members
        ValidationGuard.CheckDefinedEnum(problems, value.Channel, nameof(value.Channel));
        ValidationGuard.CheckDefinedEnum(problems, value.Status, nameof(value.Status));

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
    /// <exception cref="ValidationFailedException">Thrown if <paramref name="value"/> is invalid, listing all problems</exception>
    public static void EnsureValid(this NotificationResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        ValidationGuard.ThrowIfAny(nameof(value), nameof(NotificationResult), value.Validate());
    }
}
