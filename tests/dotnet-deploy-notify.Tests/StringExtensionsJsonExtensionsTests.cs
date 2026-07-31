#nullable enable

using System;
using System.Text.Json;
using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class StringExtensionsJsonExtensionsTests
{
    [Fact]
    public void ToJson_Default_ReturnsValidJson()
    {
        // Act
        var json = StringExtensionsJsonExtensions.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();

        var metadata = StringExtensionsJsonExtensions.FromJson(json);
        metadata.Should().NotBeNull();

        metadata!.Type.Should().Be(nameof(StringExtensions));
        metadata.Namespace.Should().NotBeNullOrEmpty();
        metadata.Assembly.Should().NotBeNullOrEmpty();
        metadata.Methods.Should().NotBeNull();
        metadata.Methods!.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ToJson_Indented_ContainsNewLine()
    {
        // Act
        var json = StringExtensionsJsonExtensions.ToJson(indented: true);

        // Assert
        json.Should().Contain(Environment.NewLine);
    }

    [Fact]
    public void FromJson_InvalidJson_ReturnsNull()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act
        var result = StringExtensionsJsonExtensions.FromJson(invalidJson);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => StringExtensionsJsonExtensions.FromJson(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndMetadata()
    {
        // Arrange
        var json = StringExtensionsJsonExtensions.ToJson();

        // Act
        var success = StringExtensionsJsonExtensions.TryFromJson(json, out var metadata);

        // Assert
        success.Should().BeTrue();
        metadata.Should().NotBeNull();
        metadata!.Type.Should().Be(nameof(StringExtensions));
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act
        var success = StringExtensionsJsonExtensions.TryFromJson(invalidJson, out var metadata);

        // Assert
        success.Should().BeFalse();
        metadata.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_NullInput_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => StringExtensionsJsonExtensions.TryFromJson(null!, out _);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
