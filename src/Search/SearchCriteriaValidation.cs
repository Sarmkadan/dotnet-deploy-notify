#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

using System.Diagnostics.CodeAnalysis;
using DotNetDeployNotify.Core;

namespace DotNetDeployNotify.Search;

/// <summary>
/// Provides validation helpers for <see cref="SearchCriteria"/> instances to ensure search parameters are valid
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Validation logic is straightforward and covered by integration tests")]
public static class SearchCriteriaValidation
{
    /// <summary>
    /// Validates a <see cref="SearchCriteria"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The search criteria to validate</param>
    /// <returns>An empty list if valid, otherwise a list of human-readable error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SearchCriteria value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>(capacity: 16);

        // Validate ProjectName (optional, but if set must be non-empty)
        if (value.ProjectName is not null && string.IsNullOrWhiteSpace(value.ProjectName))
        {
            errors.Add("ProjectName cannot be empty or whitespace when set.");
        }

        // Validate Version (optional, but if set must be non-empty)
        if (value.Version is not null && string.IsNullOrWhiteSpace(value.Version))
        {
            errors.Add("Version cannot be empty or whitespace when set.");
        }

        // Validate Status (optional enum)
        // No validation needed for nullable enum

        // Validate TargetEnvironment (optional enum)
        // No validation needed for nullable enum

        // Validate BranchName (optional, but if set must be non-empty)
        if (value.BranchName is not null && string.IsNullOrWhiteSpace(value.BranchName))
        {
            errors.Add("BranchName cannot be empty or whitespace when set.");
        }

        // Validate CommitAuthor (optional, but if set must be non-empty)
        if (value.CommitAuthor is not null && string.IsNullOrWhiteSpace(value.CommitAuthor))
        {
            errors.Add("CommitAuthor cannot be empty or whitespace when set.");
        }

        // Validate CreatedAfter (optional date)
        if (value.CreatedAfter.HasValue)
        {
            if (value.CreatedAfter.Value == default)
            {
                errors.Add("CreatedAfter cannot be the default(DateTime) value.");
            }
            else if (value.CreatedAfter.Value > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("CreatedAfter cannot be in the future (more than 5 minutes ahead).");
            }
            else if (value.CreatedAfter.Value < DateTime.UtcNow.AddYears(-1))
            {
                errors.Add("CreatedAfter cannot be more than one year in the past.");
            }
        }

        // Validate CreatedBefore (optional date)
        if (value.CreatedBefore.HasValue)
        {
            if (value.CreatedBefore.Value == default)
            {
                errors.Add("CreatedBefore cannot be the default(DateTime) value.");
            }
            else if (value.CreatedBefore.Value > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("CreatedBefore cannot be in the future (more than 5 minutes ahead).");
            }
            else if (value.CreatedBefore.Value < DateTime.UtcNow.AddYears(-1))
            {
                errors.Add("CreatedBefore cannot be more than one year in the past.");
            }
        }

        // Validate that CreatedAfter is not after CreatedBefore
        if (value.CreatedAfter.HasValue && value.CreatedBefore.HasValue)
        {
            if (value.CreatedAfter.Value > value.CreatedBefore.Value)
            {
                errors.Add("CreatedAfter cannot be after CreatedBefore.");
            }
        }

        // Validate MinimumPriority (optional enum)
        // No validation needed for nullable enum

        // Validate Channels (optional list)
        if (value.Channels is not null)
        {
            if (value.Channels.Count == 0)
            {
                errors.Add("Channels cannot be empty when set.");
            }
            else
            {
                for (var i = 0; i < value.Channels.Count; i++)
                {
                    var channel = value.Channels[i];
                    if (channel == default)
                    {
                        errors.Add($"Channels[{i}] cannot be the default(NotificationChannel) value.");
                    }
                }
            }
        }

        // Validate MessageContains (optional, but if set must be non-empty)
        if (value.MessageContains is not null && string.IsNullOrWhiteSpace(value.MessageContains))
        {
            errors.Add("MessageContains cannot be empty or whitespace when set.");
        }

        // Validate Limit (must be positive and reasonable)
        if (value.Limit <= 0)
        {
            errors.Add("Limit must be a positive integer.");
        }
        else if (value.Limit > 10000)
        {
            errors.Add("Limit cannot exceed 10000 (maximum allowed value).");
        }

        // Validate Offset (must be non-negative)
        if (value.Offset < 0)
        {
            errors.Add("Offset cannot be negative.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="SearchCriteria"/> instance is valid.
    /// </summary>
    /// <param name="value">The search criteria to check</param>
    /// <returns>True if the search criteria is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SearchCriteria value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="SearchCriteria"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The search criteria to validate</param>
    /// <exception cref="ArgumentException">Thrown if the search criteria is invalid, containing all validation errors</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this SearchCriteria value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            var errorMessage = string.Join("\n- ", errors);
            throw new ArgumentException($"SearchCriteria is invalid:\n- {errorMessage}");
        }
    }
}