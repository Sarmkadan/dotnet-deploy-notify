#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using System.Globalization;
using System;
using SystemEnvironment = System.Environment;

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides validation helpers for <see cref="DeploymentHistoryEntry"/> instances
/// </summary>
public static class DeploymentHistoryEntryValidation
{
    /// <summary>
    /// Validates a <see cref="DeploymentHistoryEntry"/> and returns a list of validation errors
    /// </summary>
    /// <param name="value">The deployment history entry to validate</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this DeploymentHistoryEntry? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("Id cannot be null or whitespace.");
        }
        else if (!Guid.TryParse(value.Id, out _))
        {
            errors.Add("Id must be a valid GUID.");
        }

        // Validate ProjectName
        if (string.IsNullOrWhiteSpace(value.ProjectName))
        {
            errors.Add("ProjectName cannot be null or whitespace.");
        }

        // Validate Version
        if (string.IsNullOrWhiteSpace(value.Version))
        {
            errors.Add("Version cannot be null or whitespace.");
        }
        else if (!IsValidSemanticVersion(value.Version))
        {
            errors.Add("Version must be a valid semantic version (e.g., 1.0.0, 2.3.4-beta).");
        }

        // Validate FinalStatus
        if (!Enum.IsDefined(typeof(BuildStatus), value.FinalStatus))
        {
            errors.Add("FinalStatus must be a defined BuildStatus value.");
        }

        // Validate TargetEnvironment
        if (!Enum.IsDefined(typeof(Environment), value.TargetEnvironment))
        {
            errors.Add("TargetEnvironment must be a defined Environment value.");
        }

        // Validate BranchName
        if (string.IsNullOrWhiteSpace(value.BranchName))
        {
            errors.Add("BranchName cannot be null or whitespace.");
        }
        else if (value.BranchName.Contains(' '))
        {
            errors.Add("BranchName cannot contain whitespace.");
        }

        // Validate CommitHash
        if (string.IsNullOrWhiteSpace(value.CommitHash))
        {
            errors.Add("CommitHash cannot be null or whitespace.");
        }
        else if (value.CommitHash.Length < 7)
        {
            errors.Add("CommitHash must be at least 7 characters long.");
        }
        else if (!IsValidCommitHash(value.CommitHash))
        {
            errors.Add("CommitHash must contain only hexadecimal characters.");
        }

        // Validate CommitAuthor
        if (string.IsNullOrWhiteSpace(value.CommitAuthor))
        {
            errors.Add("CommitAuthor cannot be null or whitespace.");
        }

        // Validate DeployedAt
        if (value.DeployedAt == default)
        {
            errors.Add("DeployedAt cannot be default(DateTime).");
        }
        else if (value.DeployedAt.Kind != DateTimeKind.Utc)
        {
            errors.Add("DeployedAt must be in UTC.");
        }
        else if (value.DeployedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("DeployedAt cannot be in the future.");
        }

        // Validate DurationSeconds
        if (value.DurationSeconds is { } duration)
        {
            if (duration <= 0)
            {
                errors.Add("DurationSeconds must be positive when specified.");
            }
            else if (duration > 86400) // 24 hours in seconds
            {
                errors.Add("DurationSeconds cannot exceed 86400 seconds (24 hours).");
            }
        }

        // Validate ErrorDetails (only if FinalStatus indicates failure)
        if (IsFailureStatus(value.FinalStatus) && string.IsNullOrWhiteSpace(value.ErrorDetails))
        {
            errors.Add("ErrorDetails must be provided when FinalStatus indicates a failure.");
        }

        // Validate IsRollback
        // No specific validation needed for boolean

        // Validate RolledBackFromVersion
        if (value.IsRollback && string.IsNullOrWhiteSpace(value.RolledBackFromVersion))
        {
            errors.Add("RolledBackFromVersion must be specified when IsRollback is true.");
        }
        else if (!string.IsNullOrWhiteSpace(value.RolledBackFromVersion) && !IsValidSemanticVersion(value.RolledBackFromVersion))
        {
            errors.Add("RolledBackFromVersion must be a valid semantic version when specified.");
        }

        // Validate Tags
        if (value.Tags == null)
        {
            errors.Add("Tags dictionary cannot be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="DeploymentHistoryEntry"/> is valid
    /// </summary>
    /// <param name="value">The deployment history entry to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static bool IsValid(this DeploymentHistoryEntry? value)
	=> value is not null && !Validate(value).Any();

    /// <summary>
    /// Ensures that a <see cref="DeploymentHistoryEntry"/> is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The deployment history entry to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid</exception>
    public static void EnsureValid(this DeploymentHistoryEntry? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"DeploymentHistoryEntry is invalid. Validation errors:{SystemEnvironment.NewLine}{string.Join(SystemEnvironment.NewLine, errors)}");
        }
    }

    private static bool IsFailureStatus(BuildStatus status)
        => status is BuildStatus.Failed
            or BuildStatus.Cancelled
            or BuildStatus.DeploymentFailed;

    private static bool IsValidSemanticVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return false;
        }

        // Basic semantic version pattern: major.minor.patch[-prerelease][+buildmetadata]
        // Allow common formats like: 1.0.0, 2.3.4-beta, 1.2.3+build.1234
        var pattern = @"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(\-(?<prerelease>[0-9A-Za-z\-\.]+))?(\+(?<build>[0-9A-Za-z\-\.]+))?$";
        return System.Text.RegularExpressions.Regex.IsMatch(version, pattern);
    }

    private static bool IsValidCommitHash(string hash)
    {
        // Git commit hashes are hexadecimal strings (40 characters for full SHA-1)
        // We accept both full hashes and short hashes
        return System.Text.RegularExpressions.Regex.IsMatch(hash, "^[0-9a-fA-F]+$");
    }
}