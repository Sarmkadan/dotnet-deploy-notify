#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Extension methods for testing string extensions and providing fluent assertion helpers.
/// </summary>
public static class StringExtensionsTestsExtensions
{
    /// <summary>
    /// Validates that a string is properly truncated with the expected suffix.
    /// </summary>
    /// <param name="input">The input string to test.</param>
    /// <param name="expectedLength">The expected length of the truncated result.</param>
    /// <param name="expectedSuffix">The expected suffix appended to the truncated string.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="expectedSuffix"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="expectedLength"/> is negative.</exception>
    public static void ShouldBeTruncatedTo(this string input, int expectedLength, string expectedSuffix = "...")
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(expectedSuffix);
        ArgumentOutOfRangeException.ThrowIfNegative(expectedLength);

        // Act
        var result = input.Truncate(expectedLength, expectedSuffix);

        // Assert
        result.Should().HaveLength(expectedLength);
        result.Should().EndWith(expectedSuffix);
    }

    /// <summary>
    /// Validates that a slug is properly formatted (lowercase, hyphenated).
    /// </summary>
    /// <param name="input">The input string to convert to slug format.</param>
    /// <param name="expectedSlug">The expected slug result.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static void ShouldBeValidSlug(this string input, string expectedSlug)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Act
        var result = input.ToSlug();

        // Assert
        result.Should().Be(expectedSlug);
        result.Should().NotContain(" ");
        result.Should().BeLowerCased();
    }

    /// <summary>
    /// Validates that a string has the expected boolean conversion.
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <param name="expectedValue">The expected boolean result.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static void ShouldConvertToBoolean(this string input, bool expectedValue)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Act
        var result = input.ToBooleanSafe();

        // Assert
        result.Should().Be(expectedValue);
    }

    /// <summary>
    /// Validates that sensitive data is properly masked.
    /// </summary>
    /// <param name="input">The input string containing sensitive data.</param>
    /// <param name="visibleChars">Number of characters to leave visible at the start.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="visibleChars"/> is not positive.</exception>
    public static void ShouldBeProperlyMasked(this string input, int visibleChars = 4)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(visibleChars);

        // Act
        var result = input.MaskSensitive(visibleChars);

        // Assert
        result.Should().StartWith(input[..visibleChars]);
        result.Should().Contain("*");
        result.Should().HaveLength(input.Length);
    }
}
