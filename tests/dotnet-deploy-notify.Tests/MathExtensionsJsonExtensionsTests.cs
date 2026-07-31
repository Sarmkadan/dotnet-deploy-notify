#nullable enable

using System;
using System.Text.Json;
using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class MathExtensionsJsonExtensionsTests
{
    [Fact]
    public void ToJson_ReturnsJsonString()
    {
        // Act
        var json = MathExtensionsJsonExtensions.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FromJson_ReturnsMetadata()
    {
        // Arrange
        var json = MathExtensionsJsonExtensions.ToJson();

        // Act
        var metadata = MathExtensionsJsonExtensions.FromJson(json);

        // Assert
        metadata.Should().NotBeNull();
        metadata.Type.Should().Be("MathExtensions");
        metadata.Namespace.Should().NotBeNullOrEmpty();
        metadata.Assembly.Should().NotBeNullOrEmpty();
        metadata.Methods.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void TryFromJson_ReturnsTrueAndMetadata()
    {
        // Arrange
        var json = MathExtensionsJsonExtensions.ToJson();

        // Act
        var success = MathExtensionsJsonExtensions.TryFromJson(json, out var metadata);

        // Assert
        success.Should().BeTrue();
        metadata.Should().NotBeNull();
        metadata.Type.Should().Be("MathExtensions");
        metadata.Namespace.Should().NotBeNullOrEmpty();
        metadata.Assembly.Should().NotBeNullOrEmpty();
        metadata.Methods.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FromJson_ThrowsArgumentNullException_ForNullInput()
    {
        // Act / Assert
        Action act = () => MathExtensionsJsonExtensions.FromJson(null!);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("json");
    }

    [Fact]
    public void TryFromJson_ReturnsFalseAndNullMetadata_ForNullInput()
    {
        // Act
        var success = MathExtensionsJsonExtensions.TryFromJson(null!, out var metadata);

        // Assert
        success.Should().BeFalse();
        metadata.Should().BeNull();
    }
}
