#nullable enable

using System;
using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class StringExtensionsValidationTests
{
    [Fact]
    public void Validate_WithValidString_ReturnsEmptyList()
    {
        // Arrange
        var value = "valid string";

        // Act
        var problems = value.Validate();

        // Assert
        problems.Should().BeEmpty();
    }

    [Fact]
    public void IsValid_WithValidString_ReturnsTrue()
    {
        // Arrange
        var value = "another valid string";

        // Act
        var result = value.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EnsureValid_WithValidString_DoesNotThrow()
    {
        // Arrange
        var value = "yet another valid string";

        // Act
        Action act = () => value.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_NullString_ThrowsArgumentNullException()
    {
        // Arrange
        string? value = null;

        // Act
        Action act = () => value!.Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("value");
    }

    [Fact]
    public void IsValid_NullString_ThrowsArgumentNullException()
    {
        // Arrange
        string? value = null;

        // Act
        Action act = () => value!.IsValid();

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("value");
    }

    [Fact]
    public void EnsureValid_NullString_ThrowsArgumentNullException()
    {
        // Arrange
        string? value = null;

        // Act
        Action act = () => value!.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("value");
    }
}
