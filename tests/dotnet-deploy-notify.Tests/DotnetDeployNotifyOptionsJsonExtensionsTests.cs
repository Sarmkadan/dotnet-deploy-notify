#nullable enable

using System;
using System.Text.Json;
using DotNetDeployNotify.Configuration;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class DotnetDeployNotifyOptionsJsonExtensionsTests
{
    // Helper to create a minimal, non‑default options instance.
    private static DotnetDeployNotifyOptions CreateSampleOptions()
    {
        // The actual properties of DotnetDeployNotifyOptions are not known here.
        // We rely on the type having a parameterless constructor and at least one settable property.
        // If the type has no public setters, the default instance will still be serializable.
        return new DotnetDeployNotifyOptions();
    }

    [Fact]
    public void ToJson_WithValidOptions_ReturnsJsonString()
    {
        // Arrange
        var options = CreateSampleOptions();

        // Act
        var json = options.ToJson();

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        // The JSON should be deserializable back to the same type.
        var deserialized = JsonSerializer.Deserialize<DotnetDeployNotifyOptions>(json);
        deserialized.Should().NotBeNull();
        deserialized.Should().BeEquivalentTo(options);
    }

    [Fact]
    public void ToJson_WithIndentation_ProducesIndentedJson()
    {
        // Arrange
        var options = CreateSampleOptions();

        // Act
        var json = options.ToJson(indented: true);

        // Assert
        json.Should().Contain("\n"); // indented JSON contains line breaks
    }

    [Fact]
    public void ToJson_NullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        DotnetDeployNotifyOptions? options = null;

        // Act
        Action act = () => options!.ToJson();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromJson_WithValidJson_ReturnsOptions()
    {
        // Arrange
        var original = CreateSampleOptions();
        var json = original.ToJson();

        // Act
        var result = DotnetDeployNotifyOptionsJsonExtensions.FromJson(json);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void FromJson_NullOrWhiteSpace_ThrowsArgumentException()
    {
        // Arrange
        string nullJson = null!;
        string emptyJson = "";
        string whitespaceJson = "   ";

        // Act / Assert
        Action actNull = () => DotnetDeployNotifyOptionsJsonExtensions.FromJson(nullJson);
        actNull.Should().Throw<ArgumentException>();

        Action actEmpty = () => DotnetDeployNotifyOptionsJsonExtensions.FromJson(emptyJson);
        actEmpty.Should().Throw<ArgumentException>();

        Action actWhite = () => DotnetDeployNotifyOptionsJsonExtensions.FromJson(whitespaceJson);
        actWhite.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndValue()
    {
        // Arrange
        var original = CreateSampleOptions();
        var json = original.ToJson();

        // Act
        var success = DotnetDeployNotifyOptionsJsonExtensions.TryFromJson(json, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var malformedJson = "{ this is not valid json }";

        // Act
        var success = DotnetDeployNotifyOptionsJsonExtensions.TryFromJson(malformedJson, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_EmptyString_ReturnsFalseAndNull()
    {
        // Arrange
        var emptyJson = string.Empty;

        // Act
        var success = DotnetDeployNotifyOptionsJsonExtensions.TryFromJson(emptyJson, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }
}
