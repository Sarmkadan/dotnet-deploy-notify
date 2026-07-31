#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class DateTimeExtensionsValidationTests
{
    [Fact]
    public void Validate_ReturnsEmptyList_ForValidDateTime()
    {
        // Arrange
        var dateTime = new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var problems = dateTime.Validate();

        // Assert
        problems.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ReturnsList_ForInvalidDateTime()
    {
        // Arrange
        var dateTime = DateTime.MinValue;

        // Act
        var problems = dateTime.Validate();

        // Assert
        problems.Should().Contain("DateTime cannot be DateTime.MinValue.");
    }

    [Fact]
    public void Validate_ReturnsList_ForFutureDateTime()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.AddYears(100);

        // Act
        var problems = dateTime.Validate();

        // Assert
        problems.Should().Contain("DateTime is unreasonably far in the future (more than 100 years).");
    }

    [Fact]
    public void Validate_ReturnsList_ForPastDateTime()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.AddYears(-100);

        // Act
        var problems = dateTime.Validate();

        // Assert
        problems.Should().Contain("DateTime is unreasonably far in the past (more than 100 years).");
    }

    [Fact]
    public void IsValid_ReturnsTrue_ForValidDateTime()
    {
        // Arrange
        var dateTime = new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var isValid = dateTime.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForInvalidDateTime()
    {
        // Arrange
        var dateTime = DateTime.MinValue;

        // Act
        var isValid = dateTime.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_ForInvalidDateTime()
    {
        // Arrange
        var dateTime = DateTime.MinValue;

        // Act and Assert
        Assert.Throws<ArgumentException>(() => dateTime.EnsureValid());
    }

    [Fact]
    public void EnsureValid_ReturnsDateTime_ForValidDateTime()
    {
        // Arrange
        var dateTime = new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var validatedDateTime = dateTime.EnsureValid();

        // Assert
        validatedDateTime.Should().Be(dateTime);
    }
}
