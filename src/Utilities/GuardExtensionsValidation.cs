#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Validation helpers for common types validated by GuardExtensions methods.
/// Provides comprehensive validation with detailed error reporting capabilities.
/// </summary>
public static class GuardExtensionsValidation
{
    /// <summary>
    /// Validates an object reference, returning human-readable problems
    /// </summary>
    /// <param name="value">The object reference to validate</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <returns>An enumerable of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> ValidateObject(this object? value, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));

        var problems = new List<string>();

        if (value is null)
        {
            problems.Add($"{paramName} cannot be null");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a string, returning human-readable problems
    /// </summary>
    /// <param name="value">The string to validate</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <returns>An enumerable of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> ValidateString(this string? value, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));

        var problems = new List<string>();

        if (value is null)
        {
            problems.Add($"{paramName} cannot be null");
        }
        else if (string.IsNullOrWhiteSpace(value))
        {
            problems.Add($"{paramName} cannot be null or empty");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a collection, returning human-readable problems
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection</typeparam>
    /// <param name="value">The collection to validate</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <returns>An enumerable of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> ValidateCollection<T>(this IEnumerable<T>? value, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));

        var problems = new List<string>();

        if (value is null)
        {
            problems.Add($"{paramName} cannot be null");
        }
        else if (!value.Any())
        {
            problems.Add($"{paramName} cannot be empty");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a boolean condition, returning human-readable problems
    /// </summary>
    /// <param name="condition">The condition to validate</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <param name="message">The error message to use if condition is false</param>
    /// <returns>An enumerable of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> ValidateCondition(this bool condition, string paramName, string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));

        var problems = new List<string>();

        if (!condition)
        {
            problems.Add(message);
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates an integer value against a minimum threshold, returning human-readable problems
    /// </summary>
    /// <param name="value">The integer value to validate</param>
    /// <param name="minimum">The minimum allowed value (inclusive)</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <returns>An enumerable of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> ValidateMinimum(this int value, int minimum, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));

        var problems = new List<string>();

        if (value < minimum)
        {
            problems.Add($"{paramName} must be at least {minimum}, but was {value}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a string length against a maximum, returning human-readable problems
    /// </summary>
    /// <param name="value">The string to validate</param>
    /// <param name="maxLength">The maximum allowed length</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <returns>An enumerable of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> ValidateMaxLength(this string? value, int maxLength, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));

        var problems = new List<string>();

        if (value?.Length > maxLength)
        {
            problems.Add($"{paramName} cannot be longer than {maxLength} characters, but was {value.Length}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a URL string, returning human-readable problems
    /// </summary>
    /// <param name="value">The URL string to validate</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <returns>An enumerable of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> ValidateUrl(this string? value, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));

        var problems = new List<string>();

        if (value is null)
        {
            problems.Add($"{paramName} cannot be null");
        }
        else if (string.IsNullOrWhiteSpace(value))
        {
            problems.Add($"{paramName} cannot be null or empty");
        }
        else if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            problems.Add($"{paramName} is not a valid URL");
        }
        else if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            problems.Add($"{paramName} must be an HTTP or HTTPS URL");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a nullable value, returning human-readable problems
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="value">The nullable value to check</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <returns>An enumerable of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> ValidateNotNull<T>(this T? value, string paramName) where T : class
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));

        var problems = new List<string>();

        if (value is null)
        {
            problems.Add($"{paramName} cannot be null");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates an integer is within a range, returning human-readable problems
    /// </summary>
    /// <param name="value">The integer value to validate</param>
    /// <param name="min">The minimum value (inclusive)</param>
    /// <param name="max">The maximum value (inclusive)</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <returns>An enumerable of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> ValidateRange(this int value, int min, int max, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));

        var problems = new List<string>();

        if (value < min || value > max)
        {
            problems.Add($"{paramName} must be between {min} and {max} (inclusive), but was {value}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a string against a regular expression pattern, returning human-readable problems
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="pattern">The regular expression pattern</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <returns>An enumerable of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> ValidatePattern(this string? value, string pattern, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));
        ArgumentException.ThrowIfNullOrEmpty(pattern, nameof(pattern));

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value))
        {
            problems.Add($"{paramName} cannot be null or empty");
        }
        else
        {
            try
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(value, pattern))
                {
                    problems.Add($"{paramName} does not match the required pattern");
                }
            }
            catch (ArgumentException)
            {
                problems.Add($"{paramName}: Invalid regular expression pattern");
            }
            catch (Exception)
            {
                problems.Add($"{paramName}: Pattern matching failed");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if an object reference is valid
    /// </summary>
    /// <param name="value">The object reference to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this object? value)
    {
        return value is not null;
    }

    /// <summary>
    /// Checks if a string is valid
    /// </summary>
    /// <param name="value">The string to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Checks if a collection is valid
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection</typeparam>
    /// <param name="value">The collection to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid<T>(this IEnumerable<T>? value)
    {
        return value is not null && value.Any();
    }

    /// <summary>
    /// Checks if a boolean condition is valid
    /// </summary>
    /// <param name="condition">The condition to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this bool condition)
    {
        return condition;
    }

    /// <summary>
    /// Checks if an integer is valid (meets minimum threshold)
    /// </summary>
    /// <param name="value">The integer value to check</param>
    /// <param name="minimum">The minimum allowed value (inclusive)</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidMinimum(this int value, int minimum)
    {
        return value >= minimum;
    }

    /// <summary>
    /// Checks if a string length is valid (doesn't exceed maximum)
    /// </summary>
    /// <param name="value">The string to check</param>
    /// <param name="maxLength">The maximum allowed length</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidMaxLength(this string? value, int maxLength)
    {
        return value?.Length <= maxLength;
    }

    /// <summary>
    /// Checks if a URL string is valid
    /// </summary>
    /// <param name="value">The URL string to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidUrl(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return false;

        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }

    /// <summary>
    /// Checks if a nullable value is valid
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="value">The nullable value to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidNotNull<T>(this T? value) where T : class
    {
        return value is not null;
    }

    /// <summary>
    /// Checks if an integer is within a range
    /// </summary>
    /// <param name="value">The integer value to check</param>
    /// <param name="min">The minimum value (inclusive)</param>
    /// <param name="max">The maximum value (inclusive)</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidRange(this int value, int min, int max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Checks if a string matches a regular expression pattern
    /// </summary>
    /// <param name="value">The string value to check</param>
    /// <param name="pattern">The regular expression pattern</param>
    /// <returns>True if the string matches the pattern, false otherwise</returns>
    public static bool IsValidPattern(this string? value, string pattern)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            return System.Text.RegularExpressions.Regex.IsMatch(value, pattern);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures an object reference is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The object reference to validate</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static void EnsureValid(this object? value, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));

        if (value is null)
        {
            throw new ArgumentNullException(paramName, $"{paramName} cannot be null");
        }
    }

    /// <summary>
    /// Ensures a string is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The string to validate</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or empty</exception>
    public static void EnsureValid(this string? value, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
        }
    }

    /// <summary>
    /// Ensures a collection is valid, throwing an exception if not
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection</typeparam>
    /// <param name="value">The collection to validate</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or empty</exception>
    public static void EnsureValid<T>(this IEnumerable<T>? value, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));

        if (value is null || !value.Any())
        {
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
        }
    }

    /// <summary>
    /// Ensures a boolean condition is valid, throwing an exception if not
    /// </summary>
    /// <param name="condition">The condition to evaluate</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <param name="message">The exception message</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="condition"/> is false</exception>
    public static void EnsureValid(this bool condition, string paramName, string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));
        ArgumentException.ThrowIfNullOrEmpty(message, nameof(message));

        if (!condition)
        {
            throw new ArgumentException(message, paramName);
        }
    }

    /// <summary>
    /// Ensures an integer is valid (meets minimum threshold), throwing an exception if not
    /// </summary>
    /// <param name="value">The integer value to validate</param>
    /// <param name="minimum">The minimum allowed value (inclusive)</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is less than <paramref name="minimum"/></exception>
    public static void EnsureValidMinimum(this int value, int minimum, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));

        if (value < minimum)
        {
            throw new ArgumentException($"{paramName} must be at least {minimum}, but was {value}", paramName);
        }
    }

    /// <summary>
    /// Ensures a string length is valid (doesn't exceed maximum), throwing an exception if not
    /// </summary>
    /// <param name="value">The string to validate</param>
    /// <param name="maxLength">The maximum allowed length</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> length exceeds <paramref name="maxLength"/></exception>
    public static void EnsureValidMaxLength(this string? value, int maxLength, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));

        if (value?.Length > maxLength)
        {
            throw new ArgumentException(
                $"{paramName} cannot be longer than {maxLength} characters, but was {value.Length}",
                paramName);
        }
    }

    /// <summary>
    /// Ensures a URL string is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The URL string to validate</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not a valid HTTP/HTTPS URL</exception>
    public static void EnsureValidUrl(this string? value, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"{paramName} is not a valid URL", paramName);
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException($"{paramName} must be an HTTP or HTTPS URL", paramName);
        }
    }

    /// <summary>
    /// Ensures a nullable value is valid, throwing an exception if not
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="value">The nullable value to check</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static void EnsureValidNotNull<T>(this T? value, string paramName) where T : class
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));

        if (value is null)
        {
            throw new ArgumentNullException(paramName, $"{paramName} cannot be null");
        }
    }

    /// <summary>
    /// Ensures an integer is within a range, throwing an exception if not
    /// </summary>
    /// <param name="value">The integer value to validate</param>
    /// <param name="min">The minimum value (inclusive)</param>
    /// <param name="max">The maximum value (inclusive)</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is outside the range</exception>
    public static void EnsureValidRange(this int value, int min, int max, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));

        if (value < min || value > max)
        {
            throw new ArgumentException(
                $"{paramName} must be between {min} and {max} (inclusive), but was {value}",
                paramName);
        }
    }

    /// <summary>
    /// Ensures a string matches a regular expression pattern, throwing an exception if not
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="pattern">The regular expression pattern</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> does not match the pattern</exception>
    public static void EnsureValidPattern(this string? value, string pattern, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName, nameof(paramName));
        ArgumentException.ThrowIfNullOrEmpty(pattern, nameof(pattern));

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
        }

        try
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, pattern))
            {
                throw new ArgumentException($"{paramName} does not match the required pattern", paramName);
            }
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"{paramName}: Pattern matching failed", paramName, ex);
        }
    }
}