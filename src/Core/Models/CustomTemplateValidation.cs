#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides validation helpers for <see cref="CustomTemplate"/> instances
/// </summary>
public static class CustomTemplateValidation
{
    /// <summary>
    /// Validates a <see cref="CustomTemplate"/> instance and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The template to validate</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this CustomTemplate? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("Id cannot be null or whitespace.");
        }
        else if (value.Id.Length > 100)
        {
            errors.Add("Id cannot exceed 100 characters.");
        }

        // Validate Name
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add("Name cannot be null or whitespace.");
        }
        else if (value.Name.Length > 200)
        {
            errors.Add("Name cannot exceed 200 characters.");
        }

        // Validate Description
        if (string.IsNullOrEmpty(value.Description))
        {
            errors.Add("Description cannot be null or empty.");
        }
        else if (value.Description.Length > 2000)
        {
            errors.Add("Description cannot exceed 2000 characters.");
        }

        // Validate Content
        if (string.IsNullOrWhiteSpace(value.Content))
        {
            errors.Add("Content cannot be null or whitespace.");
        }
        else if (value.Content.Length > 100000)
        {
            errors.Add("Content cannot exceed 100000 characters.");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt cannot be the default DateTime value.");
        }
        else if (value.CreatedAt.Kind != DateTimeKind.Utc)
        {
            errors.Add("CreatedAt must be in UTC.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt cannot be in the future.");
        }

        // Validate UpdatedAt
        if (value.UpdatedAt == default)
        {
            errors.Add("UpdatedAt cannot be the default DateTime value.");
        }
        else if (value.UpdatedAt.Kind != DateTimeKind.Utc)
        {
            errors.Add("UpdatedAt must be in UTC.");
        }
        else if (value.UpdatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("UpdatedAt cannot be in the future.");
        }

        // Validate Category
        if (!string.IsNullOrEmpty(value.Category) && value.Category.Length > 200)
        {
            errors.Add("Category cannot exceed 200 characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="CustomTemplate"/> instance is valid
    /// </summary>
    /// <param name="value">The template to check</param>
    /// <returns>True if valid, otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static bool IsValid(this CustomTemplate? value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that a <see cref="CustomTemplate"/> instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The template to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when the template is invalid, with a list of problems</exception>
    public static void EnsureValid(this CustomTemplate? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "The CustomTemplate is invalid. " +
                string.Join(" ", errors),
                nameof(value));
        }
    }
}
