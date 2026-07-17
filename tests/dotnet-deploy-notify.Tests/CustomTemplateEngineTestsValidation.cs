#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Provides validation extension methods for <see cref="CustomTemplateEngine"/>, <see cref="CustomTemplate"/>, and <see cref="DeploymentNotification"/> instances.
/// </summary>
public static class CustomTemplateEngineTestsValidation
{
    /// <summary>
    /// Validates a <see cref="CustomTemplateEngine"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The engine instance to validate.</param>
    /// <returns>List of validation problems; empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this CustomTemplateEngine? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return [];
    }

    /// <summary>
    /// Checks if a <see cref="CustomTemplateEngine"/> instance is valid.
    /// </summary>
    /// <param name="value">The engine instance to check.</param>
    /// <returns>True if valid; false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this CustomTemplateEngine value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="CustomTemplateEngine"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The engine instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing the list of problems.</exception>
    public static void EnsureValid(this CustomTemplateEngine value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"CustomTemplateEngine validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates a <see cref="CustomTemplate"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The template to validate.</param>
    /// <returns>List of validation problems; empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this CustomTemplate? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Name: must not be null, empty, or whitespace
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add("Template Name is null, empty, or whitespace");
        }

        // Validate Content: can be empty but not null
        if (value.Content is null)
        {
            problems.Add("Template Content is null");
        }

        // Validate Description: can be empty or whitespace
        // (no validation needed beyond null check since it's already handled)

        // Validate CreatedAt: should not be default DateTime
        if (value.CreatedAt == default)
        {
            problems.Add("Template CreatedAt has default value (DateTime.MinValue)");
        }

        // Validate UpdatedAt: should not be default DateTime
        if (value.UpdatedAt == default)
        {
            problems.Add("Template UpdatedAt has default value (DateTime.MinValue)");
        }

        // Validate IsActive: should be a valid boolean (always valid)
        // No validation needed - any boolean is valid

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a <see cref="CustomTemplate"/> instance is valid.
    /// </summary>
    /// <param name="value">The template to check.</param>
    /// <returns>True if valid; false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this CustomTemplate value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="CustomTemplate"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The template to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing the list of problems.</exception>
    public static void EnsureValid(this CustomTemplate value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"CustomTemplate validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates a <see cref="DeploymentNotification"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The notification to validate.</param>
    /// <returns>List of validation problems; empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this DeploymentNotification? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.ProjectName))
        {
            problems.Add("DeploymentNotification ProjectName is null, empty, or whitespace");
        }

        if (string.IsNullOrWhiteSpace(value.Version))
        {
            problems.Add("DeploymentNotification Version is null, empty, or whitespace");
        }

        if (string.IsNullOrWhiteSpace(value.BranchName))
        {
            problems.Add("DeploymentNotification BranchName is null, empty, or whitespace");
        }

        if (string.IsNullOrWhiteSpace(value.CommitHash))
        {
            problems.Add("DeploymentNotification CommitHash is null, empty, or whitespace");
        }

        if (string.IsNullOrWhiteSpace(value.CommitAuthor))
        {
            problems.Add("DeploymentNotification CommitAuthor is null, empty, or whitespace");
        }

        // Validate optional string properties (can be null but not empty if provided)
        if (value.Message is not null && string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("DeploymentNotification Message is empty or whitespace");
        }

        if (value.BuildUrl is not null && string.IsNullOrWhiteSpace(value.BuildUrl))
        {
            problems.Add("DeploymentNotification BuildUrl is empty or whitespace");
        }

        if (value.RepositoryUrl is not null && string.IsNullOrWhiteSpace(value.RepositoryUrl))
        {
            problems.Add("DeploymentNotification RepositoryUrl is empty or whitespace");
        }

        // Validate DurationSeconds: should be non-negative if provided
        if (value.DurationSeconds.HasValue && value.DurationSeconds.Value < 0)
        {
            problems.Add("DeploymentNotification DurationSeconds is negative");
        }

        // Validate CreatedAt: should not be default DateTime
        if (value.CreatedAt == default)
        {
            problems.Add("DeploymentNotification CreatedAt has default value (DateTime.MinValue)");
        }

        // Validate TargetEnvironment: should not be default (0)
        if (value.TargetEnvironment == 0)
        {
            problems.Add("DeploymentNotification TargetEnvironment has default/uninitialized value");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a <see cref="DeploymentNotification"/> instance is valid.
    /// </summary>
    /// <param name="value">The notification to check.</param>
    /// <returns>True if valid; false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this DeploymentNotification value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="DeploymentNotification"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The notification to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing the list of problems.</exception>
    public static void EnsureValid(this DeploymentNotification value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DeploymentNotification validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}