#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Validation helpers for StringExtensions extension methods
/// </summary>
public static class StringExtensionsValidation
{
    /// <summary>
    /// Validates extension methods from StringExtensions class by testing edge cases and parameter validation.
    /// Returns a list of problems found during validation testing.
    /// </summary>
    /// <param name="value">The string instance to validate extension methods against.</param>
    /// <returns>List of validation problems found, empty if all tests pass.</returns>
    public static IReadOnlyList<string> Validate(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Truncate validation - checks parameter validation
        try
        {
            "test".Truncate(-1);
            problems.Add("StringExtensions.Truncate: maxLength parameter should be validated to reject negative values");
        }
        catch (ArgumentOutOfRangeException) { /* Expected to throw */ }
        catch
        {
            problems.Add("StringExtensions.Truncate: throws unexpected exception type for negative maxLength");
        }

        try
        {
            "test".Truncate(10, null!);
            problems.Add("StringExtensions.Truncate: suffix parameter should be validated to reject null values");
        }
        catch (ArgumentNullException) { /* Expected to throw */ }
        catch
        {
            problems.Add("StringExtensions.Truncate: throws unexpected exception type for null suffix");
        }

        // ToSlug validation - checks null handling
        try
        {
            "test".ToSlug();
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            problems.Add("StringExtensions.ToSlug: throws unexpected exception type for valid input");
        }

        // ToPascalCase validation
        try
        {
            "test".ToPascalCase();
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            problems.Add("StringExtensions.ToPascalCase: throws unexpected exception type for valid input");
        }

        // ToCamelCase validation
        try
        {
            "test".ToCamelCase();
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            problems.Add("StringExtensions.ToCamelCase: throws unexpected exception type for valid input");
        }

        // MaskSensitive validation - checks parameter validation
        try
        {
            "test".MaskSensitive(-1);
            problems.Add("StringExtensions.MaskSensitive: visibleChars parameter should be validated to reject negative values");
        }
        catch (ArgumentOutOfRangeException) { /* Expected to throw */ }
        catch
        {
            problems.Add("StringExtensions.MaskSensitive: throws unexpected exception type for negative visibleChars");
        }

        try
        {
            "test".MaskSensitive(0);
            problems.Add("StringExtensions.MaskSensitive: visibleChars parameter should be validated to reject zero values");
        }
        catch (ArgumentOutOfRangeException) { /* Expected to throw */ }
        catch
        {
            problems.Add("StringExtensions.MaskSensitive: throws unexpected exception type for zero visibleChars");
        }

        // ContainsAny validation - checks parameter validation
        try
        {
            "test".ContainsAny(null!);
            problems.Add("StringExtensions.ContainsAny: should throw ArgumentNullException for null input");
        }
        catch (ArgumentNullException) { /* Expected to throw */ }
        catch
        {
            problems.Add("StringExtensions.ContainsAny: throws unexpected exception type for null input");
        }

        try
        {
            "test".ContainsAny("test", null!);
            problems.Add("StringExtensions.ContainsAny: should throw ArgumentNullException for null substrings element");
        }
        catch (ArgumentNullException) { /* Expected to throw */ }
        catch
        {
            problems.Add("StringExtensions.ContainsAny: throws unexpected exception type for null substrings element");
        }

        // NormalizeLineEndings validation
        try
        {
            "test".NormalizeLineEndings();
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            problems.Add("StringExtensions.NormalizeLineEndings: throws unexpected exception type for valid input");
        }

        // CountOccurrences validation - checks parameter validation
        try
        {
            "test".CountOccurrences(null!);
            problems.Add("StringExtensions.CountOccurrences: should throw ArgumentNullException for null substring");
        }
        catch (ArgumentNullException) { /* Expected to throw */ }
        catch
        {
            problems.Add("StringExtensions.CountOccurrences: throws unexpected exception type for null substring");
        }

        // RemoveDuplicateCharacters validation
        try
        {
            "test".RemoveDuplicateCharacters();
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            problems.Add("StringExtensions.RemoveDuplicateCharacters: throws unexpected exception type for valid input");
        }

        // TakeWords validation - checks parameter validation
        try
        {
            "test".TakeWords(-1);
            problems.Add("StringExtensions.TakeWords: wordCount parameter should be validated to reject negative values");
        }
        catch (ArgumentOutOfRangeException) { /* Expected to throw */ }
        catch
        {
            problems.Add("StringExtensions.TakeWords: throws unexpected exception type for negative wordCount");
        }

        // WrapText validation - checks parameter validation
        try
        {
            "test".WrapText(-1);
            problems.Add("StringExtensions.WrapText: lineLength parameter should be validated to reject negative values");
        }
        catch (ArgumentOutOfRangeException) { /* Expected to throw */ }
        catch
        {
            problems.Add("StringExtensions.WrapText: throws unexpected exception type for negative lineLength");
        }

        try
        {
            "test".WrapText(0);
            problems.Add("StringExtensions.WrapText: lineLength parameter should be validated to reject zero values");
        }
        catch (ArgumentOutOfRangeException) { /* Expected to throw */ }
        catch
        {
            problems.Add("StringExtensions.WrapText: throws unexpected exception type for zero lineLength");
        }

        // ToBooleanSafe validation
        try
        {
            "test".ToBooleanSafe(default);
            problems.Add("StringExtensions.ToBooleanSafe: should handle default bool value correctly");
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            problems.Add("StringExtensions.ToBooleanSafe: throws unexpected exception type for default bool value");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the string instance passes all StringExtensions validation checks.
    /// </summary>
    /// <param name="value">The string instance to validate.</param>
    /// <returns>True if validation passes (no problems found); otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static bool IsValid(this string value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures the string instance passes all StringExtensions validation checks, throwing if not.
    /// </summary>
    /// <param name="value">The string instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails with details of all problems found.</exception>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static void EnsureValid(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException($"StringExtensions validation failed:{System.Environment.NewLine}{string.Join(System.Environment.NewLine, problems)}");
        }
    }
}
