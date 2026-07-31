#nullable enable

using System;
using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class DateTimeExtensionsTests
{
    [Fact]
    public void ToRelativeTimeString_ReturnsJustNow_ForLessThanOneMinute()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var date = now.AddSeconds(-30);

        // Act
        var result = date.ToRelativeTimeString();

        // Assert
        result.Should().Be("just now");
    }

    [Fact]
    public void ToRelativeTimeString_ReturnsCorrectPluralization()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var oneMinute = now.AddMinutes(-1);
        var twoMinutes = now.AddMinutes(-2);
        var oneHour = now.AddHours(-1);
        var twoHours = now.AddHours(-2);
        var oneDay = now.AddDays(-1);
        var twoDays = now.AddDays(-2);
        var oneWeek = now.AddDays(-7);
        var twoWeeks = now.AddDays(-14);
        var oneMonth = now.AddDays(-30);
        var twoMonths = now.AddDays(-60);
        var oneYear = now.AddDays(-365);
        var twoYears = now.AddDays(-730);

        // Act & Assert
        oneMinute.ToRelativeTimeString().Should().Be("1 minute ago");
        twoMinutes.ToRelativeTimeString().Should().Be("2 minutes ago");
        oneHour.ToRelativeTimeString().Should().Be("1 hour ago");
        twoHours.ToRelativeTimeString().Should().Be("2 hours ago");
        oneDay.ToRelativeTimeString().Should().Be("1 day ago");
        twoDays.ToRelativeTimeString().Should().Be("2 days ago");
        oneWeek.ToRelativeTimeString().Should().Be("1 week ago");
        twoWeeks.ToRelativeTimeString().Should().Be("2 weeks ago");
        oneMonth.ToRelativeTimeString().Should().Be("1 month ago");
        twoMonths.ToRelativeTimeString().Should().Be("2 months ago");
        oneYear.ToRelativeTimeString().Should().Be("1 year ago");
        twoYears.ToRelativeTimeString().Should().Be("2 years ago");
    }

    [Fact]
    public void ToIsoString_ProducesExpectedFormat()
    {
        // Arrange
        var date = new DateTime(2023, 3, 15, 12, 34, 56, 789, DateTimeKind.Utc);

        // Act
        var iso = date.ToIsoString();

        // Assert
        iso.Should().Be("2023-03-15T12:34:56.789Z");
    }

    [Fact]
    public void ToFormattedString_ProducesExpectedFormat()
    {
        // Arrange
        var date = new DateTime(2023, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        // Act
        var formatted = date.ToFormattedString();

        // Assert
        formatted.Should().Be("2023-12-31 23:59:59 UTC");
    }

    [Fact]
    public void IsPast_And_IsFuture_WorkCorrectly()
    {
        // Arrange
        var past = DateTime.UtcNow.AddHours(-1);
        var future = DateTime.UtcNow.AddHours(1);
        var now = DateTime.UtcNow;

        // Act & Assert
        past.IsPast().Should().BeTrue();
        past.IsFuture().Should().BeFalse();

        future.IsPast().Should().BeFalse();
        future.IsFuture().Should().BeTrue();

        // 'now' is considered not past and not future (depends on exact tick)
        now.IsPast().Should().BeFalse();
        now.IsFuture().Should().BeFalse();
    }

    [Fact]
    public void GetMinutesAndSecondsElapsed_ReturnCorrectValues()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var fiveMinutesAgo = now.AddMinutes(-5).AddSeconds(-30); // 5 min 30 sec ago

        // Act
        var minutes = fiveMinutesAgo.GetMinutesElapsed();
        var seconds = fiveMinutesAgo.GetSecondsElapsed();

        // Assert
        minutes.Should().Be(5);
        seconds.Should().BeGreaterOrEqualTo(330).And.BeLessThan(340); // 5*60 + 30 = 330
    }

    [Fact]
    public void RoundToNearestMinute_ZeroesSeconds()
    {
        // Arrange
        var date = new DateTime(2024, 1, 2, 15, 45, 27, DateTimeKind.Utc);

        // Act
        var rounded = date.RoundToNearestMinute();

        // Assert
        rounded.Should().Be(new DateTime(2024, 1, 2, 15, 45, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void RoundToNearestHour_ZeroesMinutesAndSeconds()
    {
        // Arrange
        var date = new DateTime(2024, 1, 2, 15, 45, 27, DateTimeKind.Utc);

        // Act
        var rounded = date.RoundToNearestHour();

        // Assert
        rounded.Should().Be(new DateTime(2024, 1, 2, 15, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void GetStartOfDay_ReturnsMidnightUtc()
    {
        // Arrange
        var date = new DateTime(2024, 5, 10, 13, 22, 44, DateTimeKind.Utc);

        // Act
        var start = date.GetStartOfDay();

        // Assert
        start.Should().Be(new DateTime(2024, 5, 10, 0, 0, 0, DateTimeKind.Utc));
    }
}
