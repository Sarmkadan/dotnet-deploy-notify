#nullable enable

using System;
using System.Collections.Generic;
using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class TypeHelperValidationTests
{
    [Fact]
    public void Validate_ReturnsEmpty_ForNumericValueType()
    {
        // Arrange
        Type type = typeof(int);

        // Act
        IReadOnlyList<string> problems = type.Validate();

        // Assert
        problems.Should().BeEmpty();
    }

    [Fact]
    public void IsValid_ReturnsTrue_ForNumericValueType()
    {
        // Arrange
        Type type = typeof(double);

        // Act
        bool result = type.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_ForNumericValueType()
    {
        // Arrange
        Type type = typeof(long);

        // Act
        Action act = () => type.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WhenTypeIsNull()
    {
        // Arrange
        Type? type = null;

        // Act
        Action act = () => type!.Validate();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsValid_ThrowsArgumentNullException_WhenTypeIsNull()
    {
        // Arrange
        Type? type = null;

        // Act
        Action act = () => type!.IsValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentNullException_WhenTypeIsNull()
    {
        // Arrange
        Type? type = null;

        // Act
        Action act = () => type!.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validate_ReturnsExpectedMessages_ForReferenceTypeWithoutParameterlessCtor()
    {
        // Arrange
        Type type = typeof(Uri); // Uri is a reference type with no public parameterless ctor

        // Act
        IReadOnlyList<string> problems = type.Validate();

        // Assert
        problems.Should().Contain("Type is not numeric.");
        problems.Should().Contain("Reference type does not have a parameterless constructor.");
        problems.Should().HaveCount(2);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_WithAggregatedMessages_ForInvalidType()
    {
        // Arrange
        Type type = typeof(Uri);

        // Act
        Action act = () => type.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Type validation failed*")
            .Where(ex => ex.Message.Contains("Type is not numeric.") && ex.Message.Contains("Reference type does not have a parameterless constructor."));
    }

    [Fact]
    public void Validate_HandlesNullableNumericType()
    {
        // Arrange
        Type type = typeof(int?); // nullable numeric

        // Act
        IReadOnlyList<string> problems = type.Validate();

        // Assert
        // Nullable numeric should be considered valid (no problems)
        problems.Should().BeEmpty();
    }

    [Fact]
    public void Validate_HandlesNonGenericCollection()
    {
        // Arrange
        Type type = typeof(System.Collections.ArrayList); // non‑generic collection

        // Act
        IReadOnlyList<string> problems = type.Validate();

        // Assert
        problems.Should().Contain("Type is not numeric.");
        problems.Should().Contain("Collection type is not generic.");
        // Parameterless ctor exists for ArrayList, so no ctor problem
        problems.Should().HaveCount(2);
    }
}
