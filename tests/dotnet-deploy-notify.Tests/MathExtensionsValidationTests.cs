#nullable enable

using System;
using System.Collections.Generic;
using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class MathExtensionsValidationTests
{
    [Fact]
    public void Validate_ReturnsEmptyList()
    {
        // Act
        var problems = MathExtensionsValidation.Validate();

        // Assert
        problems.Should().BeEmpty();
    }

    [Fact]
    public void IsValid_ReturnsTrue()
    {
        // Act
        var result = MathExtensionsValidation.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EnsureValid_DoesNotThrow()
    {
        // Act / Assert
        var exception = Record.Exception(() => MathExtensionsValidation.EnsureValid());
        exception.Should().BeNull();
    }

    [Fact]
    public void ValidateClamp_ReturnsEmptyList_ForValidValues()
    {
        // Arrange
        int value = 5;
        int min = 0;
        int max = 10;

        // Act
        var problems = value.ValidateClamp(min, max);

        // Assert
        problems.Should().BeEmpty();
    }

    [Fact]
    public void ValidateClamp_ReturnsProblem_WhenMinGreaterThanMax()
    {
        // Arrange
        int value = 5;
        int min = 10;
        int max = 0;

        // Act
        var problems = value.ValidateClamp(min, max);

        // Assert
        problems.Should().ContainSingle()
                .Which.Should().Be("Minimum value cannot be greater than maximum value.");
    }

    [Fact]
    public void ValidateClamp_ThrowsArgumentNullException_WhenValueIsNull()
    {
        // Arrange
        string? value = null;
        string min = "a";
        string max = "z";

        // Act
        Action act = () => value!.ValidateClamp(min, max);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("value");
    }

    [Fact]
    public void ValidateToPercentage_ThrowsArgumentOutOfRangeException_WhenTotalNegative()
    {
        // Arrange
        int value = 5;
        int total = -1;

        // Act
        Action act = () => value.ValidateToPercentage(total);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithParameterName("total");
    }

    [Fact]
    public void ValidateAverage_ThrowsArgumentNullException_WhenValuesNull()
    {
        // Arrange
        IEnumerable<int>? values = null;

        // Act
        Action act = () => values!.ValidateAverage();

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("values");
    }
}
