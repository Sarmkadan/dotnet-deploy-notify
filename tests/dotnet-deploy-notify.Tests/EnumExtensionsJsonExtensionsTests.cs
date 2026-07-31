#nullable enable

using System;
using System.Text.Json;
using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class EnumExtensionsJsonExtensionsTests
{
    private enum SampleEnum
    {
        FirstValue,
        SecondValue
    }

    [Fact]
    public void ToJson_ReturnsJsonString_ForEnumValue()
    {
        // Act
        var json = SampleEnum.FirstValue.ToJson();

        // Assert
        json.Should().Be("\"FirstValue\"");
    }

    [Fact]
    public void ToJson_WithIndentation_ReturnsJsonString_ForEnumValue()
    {
        // Act
        var json = SampleEnum.FirstValue.ToJson(indented: true);

        // Assert
        json.Should().Be("\"FirstValue\"");
    }

    [Fact]
    public void FromJson_ReturnsEnum_ForValidJson()
    {
        // Arrange
        var json = "\"SecondValue\"";

        // Act
        var result = EnumExtensionsJsonExtensions.FromJson<SampleEnum>(json);

        // Assert
        result.Should().Be(SampleEnum.SecondValue);
    }

    [Fact]
    public void FromJson_ThrowsArgumentException_WhenNullOrEmpty()
    {
        // Act
        Action actNull = () => EnumExtensionsJsonExtensions.FromJson<SampleEnum>(null!);
        Action actEmpty = () => EnumExtensionsJsonExtensions.FromJson<SampleEnum>(string.Empty);

        // Assert
        actNull.Should().Throw<ArgumentException>();
        actEmpty.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromJson_ThrowsJsonException_WhenInvalidJson()
    {
        // Arrange
        var json = "\"InvalidValue\"";

        // Act
        Action act = () => EnumExtensionsJsonExtensions.FromJson<SampleEnum>(json);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void TryFromJson_ReturnsTrueAndValue_ForValidJson()
    {
        // Arrange
        var json = "\"FirstValue\"";

        // Act
        var success = EnumExtensionsJsonExtensions.TryFromJson<SampleEnum>(json, out var value);

        // Assert
        success.Should().BeTrue();
        value.Should().Be(SampleEnum.FirstValue);
    }

    [Fact]
    public void TryFromJson_ReturnsFalse_ForNullOrWhitespace()
    {
        // Act
        var successNull = EnumExtensionsJsonExtensions.TryFromJson<SampleEnum>(null, out var valueNull);
        var successWhite = EnumExtensionsJsonExtensions.TryFromJson<SampleEnum>("   ", out var valueWhite);

        // Assert
        successNull.Should().BeFalse();
        successWhite.Should().BeFalse();
    }

    [Fact]
    public void TryFromJson_ReturnsFalse_WhenInvalidJson()
    {
        // Arrange
        var json = "\"Invalid\"";

        // Act
        var success = EnumExtensionsJsonExtensions.TryFromJson<SampleEnum>(json, out var value);

        // Assert
        success.Should().BeFalse();
    }
}
