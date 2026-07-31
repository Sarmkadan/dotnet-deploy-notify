#nullable enable

using System;
using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class EnumExtensionsValidationTests
{
    private enum TestEnum
    {
        First = 1,
        Second = 2
    }

    [Fact]
    public void Validate_ValidEnumValue_ReturnsEmptyList()
    {
        // Arrange
        var value = TestEnum.First;

        // Act
        var problems = value.Validate();

        // Assert
        problems.Should().BeEmpty();
    }

    [Fact]
    public void Validate_InvalidEnumValue_ReturnsProblem()
    {
        // Arrange
        // Cast an undefined integer to the enum type
        var value = (TestEnum)99;

        // Act
        var problems = value.Validate();

        // Assert
        problems.Should().ContainSingle()
                .Which.Should().Be($"Value {value} is not defined in enum {typeof(TestEnum).Name}");
    }

    [Fact]
    public void IsValid_ValidEnumValue_ReturnsTrue()
    {
        // Arrange
        var value = TestEnum.Second;

        // Act
        var result = value.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_InvalidEnumValue_ReturnsFalse()
    {
        // Arrange
        var value = (TestEnum)0; // 0 is not defined in TestEnum

        // Act
        var result = value.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_ValidEnumValue_DoesNotThrow()
    {
        // Arrange
        var value = TestEnum.First;

        // Act
        Action act = () => value.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_InvalidEnumValue_ThrowsArgumentException()
    {
        // Arrange
        var value = (TestEnum)42;

        // Act
        Action act = () => value.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithMessage($"Enum value {typeof(TestEnum).Name}.{value} is not valid or not defined in the enum.");
    }
}
