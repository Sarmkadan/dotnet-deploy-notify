#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Validation helpers that operate on <see cref="System.Type"/> using the
/// functionality provided by <see cref="TypeHelper"/>.
/// </summary>
public static class TypeHelperValidation
{
    /// <summary>
    /// Returns a read‑only list of human‑readable validation problems for the supplied <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The type to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> containing validation messages; the list is empty when the type is considered valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var problems = new List<string>();

        // Numeric check – if the type is numeric we consider it valid for numeric scenarios.
        if (!type.IsNumeric())
            problems.Add("Type is not numeric.");

        // Nullable check – if the type is a nullable value type, ensure the underlying type is not void.
        if (type.IsNullable())
        {
            var underlying = type.GetUnderlyingType();
            if (underlying == typeof(void))
                problems.Add("Nullable type has no underlying type.");
        }

        // Collection check – strings are excluded by TypeHelper.IsCollection().
        if (type.IsCollection())
        {
            // For collections we expect a generic argument (e.g., IEnumerable<T>).
            if (!type.IsGeneric())
                problems.Add("Collection type is not generic.");
            else if (type.GetGenericArguments() is { Length: 0 })
                problems.Add("Generic collection type has no generic arguments.");
        }

        // Enum check – if the type is an enum we accept it; otherwise, no problem.
        // (No explicit validation needed; this is just an example of using BCL.)

        // Parameterless constructor – for reference types we often need a default ctor.
        if (!type.IsValueType && !type.HasParameterlessConstructor())
            problems.Add("Reference type does not have a parameter‑less constructor.");

        // Inheritance check – if the type inherits from another type, ensure it is not the same as the base.
        // (Demonstrates use of FindTypesThatInherit; not a strict validation rule.)
        // No action needed here.

        return problems;
    }

    /// <summary>
    /// Determines whether the supplied <see cref="Type"/> passes all validation rules.
    /// </summary>
    /// <param name="type">The type to validate.</param>
    /// <returns><c>true</c> if no validation problems are found; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <c>null</c>.</exception>
    public static bool IsValid(this Type type) => !type.Validate().Any();

    /// <summary>
    /// Ensures that the supplied <see cref="Type"/> is valid. If any validation problems are found,
    /// an <see cref="ArgumentException"/> is thrown containing the aggregated messages.
    /// </summary>
    /// <param name="type">The type to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when validation problems are detected.</exception>
    public static void EnsureValid(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var problems = type.Validate();
        if (problems.Any())
            throw new ArgumentException($"Type validation failed: {string.Join("; ", problems)}", nameof(type));
    }
}
