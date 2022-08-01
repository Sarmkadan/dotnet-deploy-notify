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
/// Extension methods for testing string extensions and providing additional utility methods
/// </summary>
public static class StringExtensionsTestsExtensions
{
    /// <summary>
    /// Validates that a string is properly truncated with the expected suffix
    /// </summary>
    public static void ShouldBeTruncatedTo(this string input, int expectedLength, string expectedSuffix = "...")
    {
        // Act
        var result = input.Truncate(expectedLength, expectedSuffix);

        // Assert
        result.Should().HaveLength(expectedLength);
        result.Should().EndWith(expectedSuffix);
    }

    /// <summary>
    /// Validates that a slug is properly formatted (lowercase, hyphenated)
    /// </summary>
    public static void ShouldBeValidSlug(this string input, string expectedSlug)
    {
        // Act
        var result = input.ToSlug();

        // Assert
        result.Should().Be(expectedSlug);
        result.Should().NotContain(" ");
        result.Should().BeLowerCased();
    }

    /// <summary>
    /// Validates that a string has the expected boolean conversion
    /// </summary>
    public static void ShouldConvertToBoolean(this string input, bool expectedValue)
    {
        // Act
        var result = input.ToBooleanSafe();

        // Assert
        result.Should().Be(expectedValue);
    }

    /// <summary>
    /// Validates that sensitive data is properly masked
    /// </summary>
    public static void ShouldBeProperlyMasked(this string input, int visibleChars = 4)
    {
        // Act
        var result = input.MaskSensitive(visibleChars);

        // Assert
        result.Should().StartWith(input.Substring(0, visibleChars));
        result.Should().Contain("*");
        result.Should().HaveLength(input.Length);
    }
}
