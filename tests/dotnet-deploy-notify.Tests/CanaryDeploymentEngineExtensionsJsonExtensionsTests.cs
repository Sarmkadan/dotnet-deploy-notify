#nullable enable
using System;
using System.Text.Json;
using DotNetDeployNotify.Canary;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class CanaryDeploymentEngineExtensionsJsonExtensionsTests
{
    [Fact]
    public void ToJson_ReturnsValidJson_WithExpectedValues()
    {
        // Act
        var json = CanaryDeploymentEngineExtensionsJsonExtensions.ToJson();

        // Assert
        json.Should().NotBeNullOrWhiteSpace();

        var metadata = JsonSerializer.Deserialize<CanaryDeploymentEngineExtensionsJsonExtensions.CanaryDeploymentEngineExtensionsMetadata>(json);
        metadata.Should().NotBeNull();
        metadata!.Type.Should().Be("CanaryDeploymentEngineExtensions");
        metadata.Namespace.Should().Be(typeof(CanaryDeploymentEngineExtensions).Namespace);
        metadata.Assembly.Should().Be(typeof(CanaryDeploymentEngineExtensions).Assembly.GetName().Name);
        metadata.Methods.Should().BeEquivalentTo(
            "TryAdvanceRolloutAsync",
            "TryPromoteAsync",
            "TryAbortAsync",
            "GetCanaryPercentageNormalizedAsync");
    }

    [Fact]
    public void ToJson_WithIndentation_ContainsNewLine()
    {
        // Act
        var json = CanaryDeploymentEngineExtensionsJsonExtensions.ToJson(indented: true);

        // Assert
        json.Should().Contain("\n");
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsMetadata()
    {
        // Arrange
        var originalJson = CanaryDeploymentEngineExtensionsJsonExtensions.ToJson();

        // Act
        var metadata = CanaryDeploymentEngineExtensionsJsonExtensions.FromJson(originalJson);

        // Assert
        metadata.Should().NotBeNull();
        metadata!.Type.Should().Be("CanaryDeploymentEngineExtensions");
    }

    [Fact]
    public void FromJson_NullArgument_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => CanaryDeploymentEngineExtensionsJsonExtensions.FromJson(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromJson_MalformedJson_ReturnsNull()
    {
        // Arrange
        var malformed = "{ this is not valid json }";

        // Act
        var result = CanaryDeploymentEngineExtensionsJsonExtensions.FromJson(malformed);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndMetadata()
    {
        // Arrange
        var json = CanaryDeploymentEngineExtensionsJsonExtensions.ToJson();

        // Act
        var success = CanaryDeploymentEngineExtensionsJsonExtensions.TryFromJson(json, out var metadata);

        // Assert
        success.Should().BeTrue();
        metadata.Should().NotBeNull();
        metadata!.Type.Should().Be("CanaryDeploymentEngineExtensions");
    }

    [Fact]
    public void TryFromJson_MalformedJson_ReturnsFalseAndNull()
    {
        // Arrange
        var malformed = "not a json";

        // Act
        var success = CanaryDeploymentEngineExtensionsJsonExtensions.TryFromJson(malformed, out var metadata);

        // Assert
        success.Should().BeFalse();
        metadata.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_NullArgument_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => CanaryDeploymentEngineExtensionsJsonExtensions.TryFromJson(null!, out _);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
