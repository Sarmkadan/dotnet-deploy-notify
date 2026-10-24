#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace DotNetDeployNotify.Validation;

/// <summary>
/// Shared checks for model validators. Each <c>Check*</c> method appends a human-readable problem
/// to the supplied list when its condition fails, so validators keep collecting every problem.
/// <see cref="ThrowIfAny"/> turns the collected problems into a <see cref="ValidationFailedException"/>.
/// </summary>
public static class ValidationGuard
{
    /// <summary>
    /// Adds a problem when <paramref name="value"/> is <see langword="null"/>, empty, or whitespace.
    /// </summary>
    /// <param name="problems">The list that receives the problem</param>
    /// <param name="value">The value to check</param>
    /// <param name="name">The display name of the value, used as the problem prefix</param>
    /// <exception cref="ArgumentNullException"><paramref name="problems"/> or <paramref name="name"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty</exception>
    public static void CheckRequired(ICollection<string> problems, string? value, string name)
    {
        ArgumentNullException.ThrowIfNull(problems);
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (string.IsNullOrWhiteSpace(value))
        {
            problems.Add($"{name} is required.");
        }
    }

    /// <summary>
    /// Adds a problem when <paramref name="value"/> is <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The reference type of the value</typeparam>
    /// <param name="problems">The list that receives the problem</param>
    /// <param name="value">The value to check</param>
    /// <param name="name">The display name of the value, used as the problem prefix</param>
    /// <exception cref="ArgumentNullException"><paramref name="problems"/> or <paramref name="name"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty</exception>
    public static void CheckNotNull<T>(ICollection<string> problems, T? value, string name) where T : class
    {
        ArgumentNullException.ThrowIfNull(problems);
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (value is null)
        {
            problems.Add($"{name} must not be null.");
        }
    }

    /// <summary>
    /// Adds a problem when <paramref name="value"/> is longer than <paramref name="maxLength"/> characters.
    /// </summary>
    /// <param name="problems">The list that receives the problem</param>
    /// <param name="value">The string to check; <see langword="null"/> is not reported here</param>
    /// <param name="maxLength">The maximum allowed length</param>
    /// <param name="name">The display name of the value, used as the problem prefix</param>
    /// <exception cref="ArgumentNullException"><paramref name="problems"/> or <paramref name="name"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty</exception>
    public static void CheckMaxLength(ICollection<string> problems, string? value, int maxLength, string name)
    {
        ArgumentNullException.ThrowIfNull(problems);
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (value is not null && value.Length > maxLength)
        {
            problems.Add($"{name} cannot exceed {maxLength} characters, but was {value.Length}.");
        }
    }

    /// <summary>
    /// Adds a problem when <paramref name="value"/> lies outside the inclusive range [<paramref name="min"/>, <paramref name="max"/>].
    /// </summary>
    /// <typeparam name="T">The comparable numeric type of the value</typeparam>
    /// <param name="problems">The list that receives the problem</param>
    /// <param name="value">The value to check</param>
    /// <param name="min">The minimum allowed value (inclusive)</param>
    /// <param name="max">The maximum allowed value (inclusive)</param>
    /// <param name="name">The display name of the value, used as the problem prefix</param>
    /// <exception cref="ArgumentNullException"><paramref name="problems"/> or <paramref name="name"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty</exception>
    public static void CheckInRange<T>(ICollection<string> problems, T value, T min, T max, string name) where T : IComparable<T>
    {
        ArgumentNullException.ThrowIfNull(problems);
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            problems.Add($"{name} must be between {min} and {max}.");
        }
    }

    /// <summary>
    /// Adds a problem when <paramref name="value"/> is not strictly positive.
    /// </summary>
    /// <param name="problems">The list that receives the problem</param>
    /// <param name="value">The value to check</param>
    /// <param name="name">The display name of the value, used as the problem prefix</param>
    /// <exception cref="ArgumentNullException"><paramref name="problems"/> or <paramref name="name"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty</exception>
    public static void CheckPositive(ICollection<string> problems, int value, string name)
    {
        ArgumentNullException.ThrowIfNull(problems);
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (value <= 0)
        {
            problems.Add($"{name} must be positive.");
        }
    }

    /// <summary>
    /// Adds a problem when <paramref name="value"/> is not strictly positive.
    /// </summary>
    /// <param name="problems">The list that receives the problem</param>
    /// <param name="value">The duration to check</param>
    /// <param name="name">The display name of the value, used as the problem prefix</param>
    /// <exception cref="ArgumentNullException"><paramref name="problems"/> or <paramref name="name"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty</exception>
    public static void CheckPositive(ICollection<string> problems, TimeSpan value, string name)
    {
        ArgumentNullException.ThrowIfNull(problems);
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (value <= TimeSpan.Zero)
        {
            problems.Add($"{name} must be positive.");
        }
    }

    /// <summary>
    /// Adds a problem when <paramref name="value"/> is not a defined member of <typeparamref name="TEnum"/>.
    /// Zero-valued members are accepted when they are defined, so this does not reject a legitimate default.
    /// </summary>
    /// <typeparam name="TEnum">The enum type of the value</typeparam>
    /// <param name="problems">The list that receives the problem</param>
    /// <param name="value">The enum value to check</param>
    /// <param name="name">The display name of the value, used as the problem prefix</param>
    /// <exception cref="ArgumentNullException"><paramref name="problems"/> or <paramref name="name"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty</exception>
    public static void CheckDefinedEnum<TEnum>(ICollection<string> problems, TEnum value, string name) where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(problems);
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (!Enum.IsDefined(value))
        {
            problems.Add($"{name} must be a valid {typeof(TEnum).Name} value.");
        }
    }

    /// <summary>
    /// Throws a <see cref="ValidationFailedException"/> listing every problem, or returns when there are none.
    /// </summary>
    /// <param name="paramName">The name of the parameter that was validated</param>
    /// <param name="subject">The name of the validated type, used at the start of the message</param>
    /// <param name="problems">The problems collected by the validator</param>
    /// <exception cref="ArgumentNullException"><paramref name="problems"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="paramName"/> or <paramref name="subject"/> is <see langword="null"/> or empty</exception>
    /// <exception cref="ValidationFailedException"><paramref name="problems"/> contains at least one problem</exception>
    public static void ThrowIfAny(string paramName, string subject, IReadOnlyList<string> problems)
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName);
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentNullException.ThrowIfNull(problems);

        if (problems.Count == 0)
        {
            return;
        }

        var newLine = System.Environment.NewLine;
        throw new ValidationFailedException(
            $"{subject} is invalid. Problems:{newLine}- {string.Join($"{newLine}- ", problems)}",
            paramName,
            problems);
    }
}
