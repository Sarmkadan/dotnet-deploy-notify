#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Utilities;
using FluentAssertions;

namespace DotNetDeployNotify.Tests;

public class StringExtensionsTests
{
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
        result.Should().BeLowerCased();
    }

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
