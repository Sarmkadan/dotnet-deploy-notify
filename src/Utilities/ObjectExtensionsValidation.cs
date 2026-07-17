#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Validation extension methods for objects using ObjectExtensions helpers
/// </summary>
public static class ObjectExtensionsValidation
{
    /// <summary>
    /// Validates an object and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The object to validate</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this object? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate string properties for null/empty/whitespace
        if (value is string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                problems.Add("String value is null, empty, or whitespace");
            }
        }

        // Validate numeric types for default values
        if (value.GetType().IsValueType && value.IsDefault())
        {
            problems.Add($"Value type {value.GetTypeName()} has default value");
        }

        // Validate DateTime/DateTimeOffset for default values
        if (value is DateTime dt && dt == default)
        {
            problems.Add("DateTime has default value (DateTime.MinValue)");
        }

        if (value is DateTimeOffset dto && dto == default)
        {
            problems.Add("DateTimeOffset has default value (DateTimeOffset.MinValue)");
        }

        // Validate collections for null/empty
        if (value is System.Collections.IEnumerable enumerable && value is not string)
        {
            var count = enumerable.Cast<object>().Count();
            if (count == 0)
            {
                problems.Add("Collection is empty");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a specific property of an object
    /// </summary>
    /// <param name="value">The object containing the property to validate</param>
    /// <param name="propertyName">The name of the property to validate</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> or <paramref name="propertyName"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="propertyName"/> is empty or whitespace</exception>
    public static IReadOnlyList<string> ValidateProperty(this object value, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        var problems = new List<string>();

        var propertyValue = value.GetPropertyValue(propertyName);
        if (propertyValue is not null)
        {
            var propertyProblems = propertyValue.Validate();
            problems.AddRange(propertyProblems);
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if an object is valid
    /// </summary>
    /// <param name="value">The object to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this object value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Checks if an object with a specific property is valid
    /// </summary>
    /// <param name="value">The object to check</param>
    /// <param name="propertyName">The name of the property to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> or <paramref name="propertyName"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="propertyName"/> is empty or whitespace</exception>
    public static bool IsValidProperty(this object value, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        return ValidateProperty(value, propertyName).Count == 0;
    }

    /// <summary>
    /// Ensures that an object is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The object to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing the list of problems</exception>
    public static void EnsureValid(this object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Object validation failed:{System.Environment.NewLine}{string.Join("\n", problems)}");
        }
    }

    /// <summary>
    /// Ensures that an object with a specific property is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The object to validate</param>
    /// <param name="propertyName">The name of the property to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> or <paramref name="propertyName"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="propertyName"/> is empty or whitespace</exception>
    /// <exception cref="ArgumentException">Thrown if the object or property is not valid, containing the list of problems</exception>
    public static void EnsureValidProperty(this object value, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        var problems = ValidateProperty(value, propertyName);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Property validation failed for '{propertyName}':{System.Environment.NewLine}{string.Join("\n", problems)}");
        }
    }
}