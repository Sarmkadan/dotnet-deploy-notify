#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Tests for the <see cref="StringExtensions"/> utility methods.
/// </summary>
public class StringExtensionsTests
{
    /// <summary>
    /// Verifies that <c>Truncate</c> correctly truncates a string that exceeds the maximum length
    /// and appends the ellipsis suffix.
    /// </summary>
    [Fact]
    public void Truncate_StringExceedsMaxLength_TruncatesAndAppendsSuffix()
    {
        // Arrange
        var input = "This is a very long deployment message that exceeds the limit";

        // Act
        var result = input.Truncate(20);

        // Assert
        result.Should().HaveLength(20);
        result.Should().EndWith("...");
        result.Should().Be("This is a very lo...");
    }

    /// <summary>
    /// Ensures that <c>Truncate</c> returns an empty string when the input is null, empty, or whitespace.
    /// </summary>
    /// <param name="input">The input string to truncate.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Truncate_NullOrWhitespace_ReturnsEmptyString(string? input)
    {
        // Act
        var result = input!.Truncate(10);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <c>ToSlug</c> converts a string with spaces and uppercase letters
    /// into a lowercase, hyphen‑separated slug.
    /// </summary>
    [Fact]
    public void ToSlug_StringWithSpacesAndUppercase_ReturnsLowercaseHyphenated()
    {
        // Arrange
        var input = "My Deploy Project";

        // Act
        var result = input.ToSlug();

        // Assert
        result.Should().Be("my-deploy-project");
        result.Should().NotContain(" ");
        result.Should().MatchRegex("^[a-z0-9-]+$");
    }

    /// <summary>
    /// Validates that <c>ToBooleanSafe</c> correctly interprets common string representations
    /// of boolean values.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="expected">The expected boolean result.</param>
    [Theory]
    [InlineData("1", true)]
    [InlineData("true", true)]
    [InlineData("yes", true)]
    [InlineData("on", true)]
    [InlineData("0", false)]
    [InlineData("false", false)]
    [InlineData("no", false)]
    [InlineData("off", false)]
    public void ToBooleanSafe_KnownStringValues_ReturnsExpectedBoolean(string input, bool expected)
    {
        // Act
        var result = input.ToBooleanSafe();

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Checks that <c>CountOccurrences</c> accurately counts how many times a substring appears
    /// within a larger string.
    /// </summary>
    [Fact]
    public void CountOccurrences_SubstringAppearsMultipleTimes_ReturnsCorrectCount()
    {
        // Arrange
        var input = "deploy success deploy failed deploy retry";

        // Act
        var count = input.CountOccurrences("deploy");

        // Assert
        count.Should().Be(3);
    }

    /// <summary>
    /// Ensures that <c>MaskSensitive</c> returns a string of asterisks when the input
    /// is shorter than the visible character threshold.
    /// </summary>
    [Fact]
    public void MaskSensitive_StringShorterThanVisibleCharThreshold_ReturnsAllStars()
    {
        // Arrange
        var shortToken = "abc";

        // Act
        var result = shortToken.MaskSensitive(visibleChars: 4);

        // Assert
        result.Should().Be("****");
    }

    /// <summary>
    /// Verifies that <c>MaskSensitive</c> shows only the first four characters of a long token
    /// and masks the remainder with asterisks, preserving the original length.
    /// </summary>
    [Fact]
    public void MaskSensitive_LongApiToken_ShowsOnlyFirstFourCharsAndMasksRest()
    {
        // Arrange
        var token = "secret-api-token-12345";

        // Act
        var result = token.MaskSensitive(visibleChars: 4);

        // Assert
        result.Should().StartWith("secr");
        result.Should().Contain("*");
        result.Should().HaveLength(token.Length);
    }
}
