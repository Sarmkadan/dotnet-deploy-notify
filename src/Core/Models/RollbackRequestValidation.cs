#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Globalization;

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides validation helpers for <see cref="RollbackRequest"/> instances.
/// </summary>
public static class RollbackRequestValidation
{
    /// <summary>
    /// Validates the specified rollback request and returns a list of human-readable validation errors.
    /// </summary>
    /// <param name="value">The rollback request to validate</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this RollbackRequest? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("Id cannot be null or whitespace.");
        }
        else if (!IsValidGuid(value.Id))
        {
            errors.Add("Id must be a valid GUID.");
        }

        // Validate ProjectName
        if (string.IsNullOrWhiteSpace(value.ProjectName))
        {
            errors.Add("ProjectName cannot be null or whitespace.");
        }
        else if (value.ProjectName.Length > 256)
        {
            errors.Add("ProjectName cannot exceed 256 characters.");
        }

        // Validate TargetVersion
        if (string.IsNullOrWhiteSpace(value.TargetVersion))
        {
            errors.Add("TargetVersion cannot be null or whitespace.");
        }
        else if (value.TargetVersion.Length > 64)
        {
            errors.Add("TargetVersion cannot exceed 64 characters.");
        }

        // Validate CurrentVersion
        if (string.IsNullOrWhiteSpace(value.CurrentVersion))
        {
            errors.Add("CurrentVersion cannot be null or whitespace.");
        }
        else if (value.CurrentVersion.Length > 64)
        {
            errors.Add("CurrentVersion cannot exceed 64 characters.");
        }

        // Validate TargetEnvironment
        if (!Enum.IsDefined(typeof(Environment), value.TargetEnvironment))
        {
            errors.Add("TargetEnvironment must be a valid Environment value.");
        }

        // Validate RequestedBy
        if (string.IsNullOrWhiteSpace(value.RequestedBy))
        {
            errors.Add("RequestedBy cannot be null or whitespace.");
        }
        else if (value.RequestedBy.Length > 128)
        {
            errors.Add("RequestedBy cannot exceed 128 characters.");
        }

        // Validate Reason
        if (string.IsNullOrWhiteSpace(value.Reason))
        {
            errors.Add("Reason cannot be null or whitespace.");
        }
        else if (value.Reason.Length > 1024)
        {
            errors.Add("Reason cannot exceed 1024 characters.");
        }

        // Validate Channels
        if (value.Channels is null)
        {
            errors.Add("Channels collection cannot be null.");
        }
        else if (value.Channels.Count == 0)
        {
            errors.Add("At least one notification channel must be specified in Channels.");
        }
        else
        {
            foreach (var channel in value.Channels)
            {
                if (!Enum.IsDefined(typeof(NotificationChannel), channel))
                {
                    errors.Add($"Invalid notification channel value: {channel}.");
                }
            }
        }

        // Validate Priority
        if (!Enum.IsDefined(typeof(NotificationPriority), value.Priority))
        {
            errors.Add("Priority must be a valid NotificationPriority value.");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt cannot be the default DateTime value.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt cannot be in the future.");
        }

        // Validate Metadata
        if (value.Metadata is null)
        {
            errors.Add("Metadata dictionary cannot be null.");
        }
        else
        {
            foreach (var kvp in value.Metadata)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    errors.Add("Metadata keys cannot be null or whitespace.");
                    break;
                }

                if (kvp.Key.Length > 256)
                {
                    errors.Add("Metadata keys cannot exceed 256 characters.");
                    break;
                }
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified rollback request is valid.
    /// </summary>
    /// <param name="value">The rollback request to check</param>
    /// <returns>True if the request is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this RollbackRequest? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Validates the specified rollback request and throws an <see cref="ArgumentException"/> if invalid.
    /// </summary>
    /// <param name="value">The rollback request to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if the request is invalid with a detailed error message</exception>
    public static void EnsureValid(this RollbackRequest? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"RollbackRequest validation failed:{System.Environment.NewLine}- {string.Join($"{System.Environment.NewLine}- ", errors)}");
        }
    }

    /// <summary>
    /// Validates that a string is a valid GUID format.
    /// </summary>
    /// <param name="input">The string to validate</param>
    /// <returns>True if valid GUID; otherwise, false</returns>
    private static bool IsValidGuid(string input)
    {
        return Guid.TryParse(input, out _);
    }
}