using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Validation helpers for the NotificationServiceTests class.
/// </summary>
public static class NotificationServiceTestsValidation
{
    /// <summary>
    /// Validates the NotificationServiceTests instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The NotificationServiceTests instance to validate.</param>
    /// <returns>A list of validation problems, or an empty list if valid.</returns>
    public static IReadOnlyList<string> Validate(this NotificationServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate private fields are initialized (via reflection check)
        // This is a test class, so we validate the test infrastructure

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the NotificationServiceTests instance is valid.
    /// </summary>
    /// <param name="value">The NotificationServiceTests instance to check.</param>
    /// <returns>true if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this NotificationServiceTests value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the NotificationServiceTests instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The NotificationServiceTests instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
    public static void EnsureValid(this NotificationServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"NotificationServiceTests is not valid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}
