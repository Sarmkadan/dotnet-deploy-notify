#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides validation helpers for <see cref="WebhookPayload"/> instances
/// </summary>
public static class WebhookPayloadValidation
{
    /// <summary>
    /// Validates a webhook payload and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The payload to validate</param>
    /// <returns>An enumerable of validation error messages, or empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this WebhookPayload value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate EventId
        if (string.IsNullOrWhiteSpace(value.EventId))
        {
            errors.Add("EventId must not be null or whitespace");
        }
        else if (!IsValidGuidFormat(value.EventId))
        {
            errors.Add("EventId must be a valid GUID format");
        }

        // Validate EventType
        if (string.IsNullOrWhiteSpace(value.EventType))
        {
            errors.Add("EventType must not be null or whitespace");
        }

        // Validate Timestamp
        if (value.Timestamp == default)
        {
            errors.Add("Timestamp must be set to a non-default DateTime value");
        }
        else if (value.Timestamp.Kind != DateTimeKind.Utc)
        {
            errors.Add("Timestamp must be in UTC format");
        }
        else if (value.Timestamp > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("Timestamp cannot be in the future");
        }
        else if (value.Timestamp < DateTime.UtcNow.AddYears(-1))
        {
            errors.Add("Timestamp cannot be more than one year in the past");
        }

        // Validate Source
        if (string.IsNullOrWhiteSpace(value.Source))
        {
            errors.Add("Source must not be null or whitespace");
        }

        // Validate SchemaVersion
        if (string.IsNullOrWhiteSpace(value.SchemaVersion))
        {
            errors.Add("SchemaVersion must not be null or whitespace");
        }
        else if (!IsValidSemanticVersion(value.SchemaVersion))
        {
            errors.Add("SchemaVersion must be a valid semantic version (e.g., 1.0.0)");
        }

        // Validate Data
        if (value.Data is null)
        {
            errors.Add("Data must not be null");
        }
        else
        {
            errors.AddRange(value.Data.Validate());
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the webhook payload is valid
    /// </summary>
    /// <param name="value">The payload to check</param>
    /// <returns>True if the payload is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static bool IsValid(this WebhookPayload value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the webhook payload is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The payload to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when the payload is invalid, containing the validation errors</exception>
    public static void EnsureValid(this WebhookPayload value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"WebhookPayload is invalid. Errors: {string.Join("; ", errors)}",
                nameof(value));
        }
    }

    /// <summary>
    /// Validates that a string is a valid GUID format
    /// </summary>
    private static bool IsValidGuidFormat(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length != 36)
        {
            return false;
        }

        try
        {
            _ = Guid.Parse(input);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates that a string is a valid semantic version format
    /// </summary>
    private static bool IsValidSemanticVersion(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        // Basic semantic version pattern: major.minor.patch[-prerelease][+buildmetadata]
        var pattern = @"^(\d+\.){2}\d+(-[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*)?(\+[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*)?$";
        return System.Text.RegularExpressions.Regex.IsMatch(input, pattern);
    }
}

/// <summary>
/// Provides validation helpers for <see cref="WebhookData"/> instances
/// </summary>
public static class WebhookDataValidation
{
    /// <summary>
    /// Validates webhook data and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The data to validate</param>
    /// <returns>An enumerable of validation error messages, or empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this WebhookData value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate ProjectName
        if (string.IsNullOrWhiteSpace(value.ProjectName))
        {
            errors.Add("ProjectName must not be null or whitespace");
        }

        // Validate Version
        if (string.IsNullOrWhiteSpace(value.Version))
        {
            errors.Add("Version must not be null or whitespace");
        }

        // Validate Status
        if (string.IsNullOrWhiteSpace(value.Status))
        {
            errors.Add("Status must not be null or whitespace");
        }
        else if (!IsValidStatus(value.Status))
        {
            errors.Add("Status must be a valid deployment status (e.g., success, failed, queued)");
        }

        // Validate Environment
        if (!string.IsNullOrWhiteSpace(value.Environment) && !IsValidEnvironment(value.Environment))
        {
            errors.Add("Environment must be a valid environment name (e.g., dev, staging, production)");
        }

        // Validate Branch
        if (!string.IsNullOrWhiteSpace(value.Branch) && !IsValidBranchName(value.Branch))
        {
            errors.Add("Branch must be a valid branch name");
        }

        // Validate CommitHash
        if (!string.IsNullOrWhiteSpace(value.CommitHash) && !IsValidCommitHash(value.CommitHash))
        {
            errors.Add("CommitHash must be a valid commit hash (7+ hex characters)");
        }

        // Validate CommitAuthor
        if (!string.IsNullOrWhiteSpace(value.CommitAuthor) && value.CommitAuthor.Length > 256)
        {
            errors.Add("CommitAuthor must be 256 characters or less");
        }

        // Validate RepositoryUrl
        if (!string.IsNullOrWhiteSpace(value.RepositoryUrl) && !Uri.IsWellFormedUriString(value.RepositoryUrl, UriKind.Absolute))
        {
            errors.Add("RepositoryUrl must be a valid absolute URI");
        }

        // Validate BuildUrl
        if (!string.IsNullOrWhiteSpace(value.BuildUrl) && !Uri.IsWellFormedUriString(value.BuildUrl, UriKind.Absolute))
        {
            errors.Add("BuildUrl must be a valid absolute URI");
        }

        // Validate DurationSeconds
        if (value.DurationSeconds.HasValue)
        {
            if (value.DurationSeconds < 0)
            {
                errors.Add("DurationSeconds must be a non-negative value");
            }
            else if (value.DurationSeconds > 86400) // 24 hours in seconds
            {
                errors.Add("DurationSeconds must be 24 hours or less");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the webhook data is valid
    /// </summary>
    /// <param name="value">The data to check</param>
    /// <returns>True if the data is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static bool IsValid(this WebhookData value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the webhook data is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The data to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when the data is invalid, containing the validation errors</exception>
    public static void EnsureValid(this WebhookData value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"WebhookData is invalid. Errors: {string.Join("; ", errors)}",
                nameof(value));
        }
    }

    /// <summary>
    /// Validates that a status string is valid
    /// </summary>
    private static bool IsValidStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return false;
        }

        var validStatuses = new[] { "success", "failed", "queued", "in_progress", "cancelled", "error" };
        return validStatuses.Contains(status.ToLowerInvariant());
    }

    /// <summary>
    /// Validates that an environment name is valid
    /// </summary>
    private static bool IsValidEnvironment(string environment)
    {
        if (string.IsNullOrWhiteSpace(environment))
        {
            return true; // Optional field
        }

        var validEnvironments = new[] { "dev", "development", "staging", "test", "qa", "production", "prod" };
        return validEnvironments.Contains(environment.ToLowerInvariant());
    }

    /// <summary>
    /// Validates that a branch name is valid
    /// </summary>
    private static bool IsValidBranchName(string branch)
    {
        if (string.IsNullOrWhiteSpace(branch))
        {
            return true; // Optional field
        }

        // Basic branch name validation - alphanumeric, dashes, underscores, slashes, dots
        var pattern = @"^[a-zA-Z0-9_\-/\.]+$";
        return System.Text.RegularExpressions.Regex.IsMatch(branch, pattern);
    }

    /// <summary>
    /// Validates that a commit hash is valid (7+ hex characters)
    /// </summary>
    private static bool IsValidCommitHash(string commitHash)
    {
        if (string.IsNullOrWhiteSpace(commitHash))
        {
            return true; // Optional field
        }

        if (commitHash.Length < 7)
        {
            return false;
        }

        // Check if it's a valid hex string
        return System.Text.RegularExpressions.Regex.IsMatch(commitHash, @"^[0-9a-fA-F]+");
    }
}