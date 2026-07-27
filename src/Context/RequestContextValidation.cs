#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetDeployNotify.Context;

/// <summary>
/// Provides validation helpers for <see cref="RequestContext"/> instances
/// </summary>
public static class RequestContextValidation
{
    /// <summary>
    /// Validates a <see cref="RequestContext"/> instance
    /// </summary>
    /// <param name="value">The request context to validate</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this RequestContext? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate CorrelationId
        if (string.IsNullOrWhiteSpace(value.CorrelationId))
        {
            errors.Add("CorrelationId cannot be null or whitespace");
        }
        else if (!Guid.TryParse(value.CorrelationId, out _))
        {
            errors.Add("CorrelationId must be a valid GUID");
        }

        // Validate RequestId
        if (string.IsNullOrWhiteSpace(value.RequestId))
        {
            errors.Add("RequestId cannot be null or whitespace");
        }
        else if (!Guid.TryParse(value.RequestId, out _))
        {
            errors.Add("RequestId must be a valid GUID");
        }

        // Validate RequestTime
        if (value.RequestTime == default)
        {
            errors.Add("RequestTime cannot be default(DateTime)");
        }
        else if (value.RequestTime.Kind != DateTimeKind.Utc)
    {
        errors.Add("RequestTime must be in UTC kind");
    }
    else if (value.RequestTime > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("RequestTime cannot be in the future");
        }
        else if (value.RequestTime < DateTime.UtcNow.AddYears(-1))
        {
            errors.Add("RequestTime cannot be more than one year in the past");
        }

        // Validate UserId (if set)
        if (value.UserId is not null && string.IsNullOrWhiteSpace(value.UserId))
        {
            errors.Add("UserId cannot be empty string when set");
        }

        // Validate ClientId (if set)
        if (value.ClientId is not null && string.IsNullOrWhiteSpace(value.ClientId))
        {
            errors.Add("ClientId cannot be empty string when set");
        }

        // Validate Metadata
        if (value.Metadata is null)
        {
            errors.Add("Metadata dictionary cannot be null");
        }

        // Validate ExecutionTimeMs
        if (value.ExecutionTimeMs < 0)
        {
            errors.Add("ExecutionTimeMs cannot be negative");
        }
        else if (value.ExecutionTimeMs > 86400000) // 24 hours in milliseconds
        {
            errors.Add("ExecutionTimeMs cannot exceed 24 hours (86400000 ms)");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="RequestContext"/> instance is valid
    /// </summary>
    /// <param name="value">The request context to check</param>
    /// <returns>True if the context is valid; otherwise, false</returns>
    public static bool IsValid(this RequestContext? value) => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures that a <see cref="RequestContext"/> instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The request context to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
/// <exception cref="ArgumentException">Thrown if the context is invalid, containing all validation errors</exception>
    public static void EnsureValid(this RequestContext? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            var errorMessage = string.Join("\n- ", errors);
            throw new ArgumentException($"RequestContext is invalid:\n- {errorMessage}", nameof(value));
        }
    }
}
