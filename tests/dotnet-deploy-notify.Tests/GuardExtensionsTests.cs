#nullable enable

using System;
using System.Collections.Generic;
using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class GuardExtensionsTests
{
    // ThrowIfNull -------------------------------------------------------------

    [Fact]
    public void ThrowIfNull_DoesNotThrow_WhenValueIsNotNull()
    {
        var obj = new object();

        Action act = () => obj.ThrowIfNull(nameof(obj));

        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfNull_ThrowsArgumentNullException_WhenValueIsNull()
    {
        object? obj = null;

        Action act = () => obj.ThrowIfNull(nameof(obj));

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName(nameof(obj))
            .WithMessage("*cannot be null*");
    }

    // ThrowIfNullOrEmpty (string) -------------------------------------------

    [Fact]
    public void ThrowIfNullOrEmpty_String_DoesNotThrow_WhenStringHasContent()
    {
        var str = "hello";

        Action act = () => str.ThrowIfNullOrEmpty(nameof(str));

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ThrowIfNullOrEmpty_String_ThrowsArgumentException_WhenInvalid(string? value)
    {
        Action act = () => value.ThrowIfNullOrEmpty(nameof(value));

        act.Should().Throw<ArgumentException>()
            .WithParameterName(nameof(value))
            .WithMessage("*cannot be null or empty*");
    }

    // ThrowIfNullOrEmpty (IEnumerable) ---------------------------------------

    [Fact]
    public void ThrowIfNullOrEmpty_Collection_DoesNotThrow_WhenCollectionHasItems()
    {
        var list = new List<int> { 1, 2, 3 };

        Action act = () => list.ThrowIfNullOrEmpty(nameof(list));

        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfNullOrEmpty_Collection_ThrowsArgumentException_WhenNull()
    {
        List<int>? list = null;

        Action act = () => list.ThrowIfNullOrEmpty(nameof(list));

        act.Should().Throw<ArgumentException>()
            .WithParameterName(nameof(list))
            .WithMessage("*cannot be null or empty*");
    }

    [Fact]
    public void ThrowIfNullOrEmpty_Collection_ThrowsArgumentException_WhenEmpty()
    {
        var empty = new List<string>();

        Action act = () => empty.ThrowIfNullOrEmpty(nameof(empty));

        act.Should().Throw<ArgumentException>()
            .WithParameterName(nameof(empty))
            .WithMessage("*cannot be null or empty*");
    }

    // ThrowIfFalse -----------------------------------------------------------

    [Fact]
    public void ThrowIfFalse_DoesNotThrow_WhenConditionIsTrue()
    {
        Action act = () => true.ThrowIfFalse(nameof(act), "should not happen");

        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfFalse_ThrowsArgumentException_WhenConditionIsFalse()
    {
        Action act = () => false.ThrowIfFalse(nameof(act), "condition failed");

        act.Should().Throw<ArgumentException>()
            .WithParameterName(nameof(act))
            .WithMessage("condition failed");
    }

    // ThrowIfLessThan --------------------------------------------------------

    [Fact]
    public void ThrowIfLessThan_DoesNotThrow_WhenValueMeetsMinimum()
    {
        Action act = () => 10.ThrowIfLessThan(5, nameof(act));

        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfLessThan_ThrowsArgumentException_WhenValueIsBelowMinimum()
    {
        Action act = () => 3.ThrowIfLessThan(5, nameof(act));

        act.Should().Throw<ArgumentException>()
            .WithParameterName(nameof(act))
            .WithMessage("*must be at least 5, but was 3*");
    }

    // ThrowIfLongerThan -------------------------------------------------------

    [Fact]
    public void ThrowIfLongerThan_DoesNotThrow_WhenLengthIsWithinLimit()
    {
        var str = "12345";

        Action act = () => str.ThrowIfLongerThan(10, nameof(str));

        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfLongerThan_ThrowsArgumentException_WhenStringIsTooLong()
    {
        var str = "this string is definitely longer than ten characters";

        Action act = () => str.ThrowIfLongerThan(10, nameof(str));

        act.Should().Throw<ArgumentException>()
            .WithParameterName(nameof(str))
            .WithMessage("*cannot be longer than 10 characters, but was*");
    }

    // ThrowIfInvalidUrl -------------------------------------------------------

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com/path?query=1")]
    public void ThrowIfInvalidUrl_DoesNotThrow_WhenUrlIsValid(string url)
    {
        Action act = () => url.ThrowIfInvalidUrl(nameof(url));

        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfInvalidUrl_ThrowsArgumentException_WhenUrlIsNullOrEmpty()
    {
        string? url = null;

        Action act = () => url.ThrowIfInvalidUrl(nameof(url));

        act.Should().Throw<ArgumentException>()
            .WithParameterName(nameof(url))
            .WithMessage("*cannot be null or empty*");
    }

    [Fact]
    public void ThrowIfInvalidUrl_ThrowsArgumentException_WhenSchemeIsNotHttpOrHttps()
    {
        var url = "ftp://example.com";

        Action act = () => url.ThrowIfInvalidUrl(nameof(url));

        act.Should().Throw<ArgumentException>()
            .WithParameterName(nameof(url))
            .WithMessage("*must be an HTTP or HTTPS URL*");
    }

    [Fact]
    public void ThrowIfInvalidUrl_ThrowsArgumentException_WhenUrlIsMalformed()
    {
        var url = "not a url";

        Action act = () => url.ThrowIfInvalidUrl(nameof(url));

        act.Should().Throw<ArgumentException>()
            .WithParameterName(nameof(url))
            .WithMessage("*is not a valid URL*");
    }

    // GetValueOrThrow ---------------------------------------------------------

    [Fact]
    public void GetValueOrThrow_ReturnsValue_WhenNotNull()
    {
        string? value = "hello";

        var result = value.GetValueOrThrow(nameof(value));

        result.Should().Be("hello");
    }

    [Fact]
    public void GetValueOrThrow_ThrowsArgumentNullException_WhenNull()
    {
        string? value = null;

        Action act = () => value.GetValueOrThrow(nameof(value));

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName(nameof(value));
    }

    // IsInRange --------------------------------------------------------------

    [Theory]
    [InlineData(5, 1, 10, true)]
    [InlineData(1, 1, 10, true)]
    [InlineData(10, 1, 10, true)]
    [InlineData(0, 1, 10, false)]
    [InlineData(11, 1, 10, false)]
    public void IsInRange_ReturnsExpectedResult(int value, int min, int max, bool expected)
    {
        var result = value.IsInRange(min, max);

        result.Should().Be(expected);
    }

    // MatchesPattern ----------------------------------------------------------

    [Fact]
    public void MatchesPattern_ReturnsTrue_WhenPatternMatches()
    {
        var value = "abc123";
        var pattern = @"^[a-z]{3}\d{3}$";

        var result = value.MatchesPattern(pattern);

        result.Should().BeTrue();
    }

    [Fact]
    public void MatchesPattern_ReturnsFalse_WhenPatternDoesNotMatch()
    {
        var value = "abc123";
        var pattern = @"^\d+$";

        var result = value.MatchesPattern(pattern);

        result.Should().BeFalse();
    }

    [Fact]
    public void MatchesPattern_ReturnsFalse_WhenValueIsNullOrWhiteSpace()
    {
        string? value = null;
        var pattern = @"\w+";

        var result = value.MatchesPattern(pattern);

        result.Should().BeFalse();
    }

    [Fact]
    public void MatchesPattern_ThrowsArgumentException_WhenPatternIsInvalid()
    {
        var value = "test";
        var invalidPattern = "["; // malformed regex

        Action act = () => value.MatchesPattern(invalidPattern);

        act.Should().Throw<ArgumentException>();
    }
}
