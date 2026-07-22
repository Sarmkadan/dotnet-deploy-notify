#nullable enable

using System.Text.Json;
using FluentAssertions;
using DotNetDeployNotify.Infrastructure;
using Xunit;

namespace DotNetDeployNotify.Infrastructure.Tests;

/// <summary>
/// Unit tests for <see cref="CanaryServiceExtensionsJsonExtensions"/> JSON serialization/deserialization.
/// </summary>
public class CanaryServiceExtensionsJsonExtensionsTests
{
    private static readonly string _sampleType = "CanaryDeploymentService";
    private static readonly string _sampleNamespace = "DotNetDeployNotify.Infrastructure";
    private static readonly string _sampleAssembly = "DotNetDeployNotify";
    private static readonly string[] _sampleMethods = ["Deploy", "Rollback", "GetStatus"];

    private static readonly CanaryServiceExtensionsJsonExtensions.CanaryServiceExtensionsMetadata _sampleMetadata = new()
    {
        Type = _sampleType,
        Namespace = _sampleNamespace,
        Assembly = _sampleAssembly,
        Methods = _sampleMethods
    };

    private static readonly string _expectedJson =
        "{\"type\":\"CanaryDeploymentService\",\"namespace\":\"DotNetDeployNotify.Infrastructure\",\"assembly\":\"DotNetDeployNotify\",\"methods\":[\"Deploy\",\"Rollback\",\"GetStatus\"]}";

    [Fact]
    public void ToJson_WithValidMetadata_ReturnsValidJson()
    {
        // Act
        var result = CanaryServiceExtensionsJsonExtensions.ToJson(_sampleMetadata);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Be(_expectedJson);
    }

    [Fact]
    public void ToJson_WithValidMetadataAndIndented_ReturnsFormattedJson()
    {
        // Act
        var result = CanaryServiceExtensionsJsonExtensions.ToJson(_sampleMetadata, indented: true);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("CanaryDeploymentService");
        result.Should().Contain("Deploy");
    }

    [Fact]
    public void ToJson_WithNullMetadata_ThrowsArgumentNullException()
    {
        // Arrange
        CanaryServiceExtensionsJsonExtensions.CanaryServiceExtensionsMetadata? nullMetadata = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CanaryServiceExtensionsJsonExtensions.ToJson(nullMetadata!));
    }

    [Fact]
    public void FromJson_WithValidJson_ReturnsMetadata()
    {
        // Act
        var result = CanaryServiceExtensionsJsonExtensions.FromJson(_expectedJson);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(_sampleType);
        result.Namespace.Should().Be(_sampleNamespace);
        result.Assembly.Should().Be(_sampleAssembly);
        result.Methods.Should().BeEquivalentTo(_sampleMethods);
    }

    [Fact]
    public void FromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CanaryServiceExtensionsJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void FromJson_WithEmptyJson_ReturnsNull()
    {
        // Arrange
        var emptyJson = "";

        // Act
        var result = CanaryServiceExtensionsJsonExtensions.FromJson(emptyJson);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithInvalidJson_ReturnsNull()
    {
        // Arrange
        var invalidJson = "{invalid json}";

        // Act
        var result = CanaryServiceExtensionsJsonExtensions.FromJson(invalidJson);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndMetadata()
    {
        // Act
        var success = CanaryServiceExtensionsJsonExtensions.TryFromJson(_expectedJson, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result!.Type.Should().Be(_sampleType);
        result.Namespace.Should().Be(_sampleNamespace);
        result.Assembly.Should().Be(_sampleAssembly);
        result.Methods.Should().BeEquivalentTo(_sampleMethods);
    }

    [Fact]
    public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => CanaryServiceExtensionsJsonExtensions.TryFromJson(null!, out _));
    }

    [Fact]
    public void TryFromJson_WithEmptyJson_ReturnsFalseAndNull()
    {
        // Arrange
        var emptyJson = "";

        // Act
        var success = CanaryServiceExtensionsJsonExtensions.TryFromJson(emptyJson, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "{invalid json}";

        // Act
        var success = CanaryServiceExtensionsJsonExtensions.TryFromJson(invalidJson, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void CanaryServiceExtensionsMetadata_TypeProperty_RoundTripsCorrectly()
    {
        // Arrange
        var metadata = new CanaryServiceExtensionsJsonExtensions.CanaryServiceExtensionsMetadata
        {
            Type = "TestType",
            Namespace = _sampleNamespace,
            Assembly = _sampleAssembly,
            Methods = _sampleMethods
        };

        // Act
        var json = CanaryServiceExtensionsJsonExtensions.ToJson(metadata);
        var deserialized = CanaryServiceExtensionsJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Type.Should().Be("TestType");
    }

    [Fact]
    public void CanaryServiceExtensionsMetadata_NamespaceProperty_RoundTripsCorrectly()
    {
        // Arrange
        var metadata = new CanaryServiceExtensionsJsonExtensions.CanaryServiceExtensionsMetadata
        {
            Type = _sampleType,
            Namespace = "Custom.Namespace",
            Assembly = _sampleAssembly,
            Methods = _sampleMethods
        };

        // Act
        var json = CanaryServiceExtensionsJsonExtensions.ToJson(metadata);
        var deserialized = CanaryServiceExtensionsJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Namespace.Should().Be("Custom.Namespace");
    }

    [Fact]
    public void CanaryServiceExtensionsMetadata_AssemblyProperty_RoundTripsCorrectly()
    {
        // Arrange
        var metadata = new CanaryServiceExtensionsJsonExtensions.CanaryServiceExtensionsMetadata
        {
            Type = _sampleType,
            Namespace = _sampleNamespace,
            Assembly = "CustomAssembly",
            Methods = _sampleMethods
        };

        // Act
        var json = CanaryServiceExtensionsJsonExtensions.ToJson(metadata);
        var deserialized = CanaryServiceExtensionsJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Assembly.Should().Be("CustomAssembly");
    }

    [Fact]
    public void CanaryServiceExtensionsMetadata_MethodsProperty_RoundTripsCorrectly()
    {
        // Arrange
        var metadata = new CanaryServiceExtensionsJsonExtensions.CanaryServiceExtensionsMetadata
        {
            Type = _sampleType,
            Namespace = _sampleNamespace,
            Assembly = _sampleAssembly,
            Methods = ["Method1", "Method2"]
        };

        // Act
        var json = CanaryServiceExtensionsJsonExtensions.ToJson(metadata);
        var deserialized = CanaryServiceExtensionsJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Methods.Should().BeEquivalentTo(["Method1", "Method2"]);
    }

    [Fact]
    public void CanaryServiceExtensionsMetadata_EmptyMethodsArray_SerializesCorrectly()
    {
        // Arrange
        var metadata = new CanaryServiceExtensionsJsonExtensions.CanaryServiceExtensionsMetadata
        {
            Type = _sampleType,
            Namespace = _sampleNamespace,
            Assembly = _sampleAssembly,
            Methods = []
        };

        // Act
        var json = CanaryServiceExtensionsJsonExtensions.ToJson(metadata);

        // Assert
        json.Should().Contain("\"methods\":[]");
    }

    [Fact]
    public void CanaryServiceExtensionsMetadata_NullProperties_SerializesWithoutNulls()
    {
        // Arrange
        var metadata = new CanaryServiceExtensionsJsonExtensions.CanaryServiceExtensionsMetadata
        {
            Type = _sampleType,
            Namespace = null,
            Assembly = null,
            Methods = null
        };

        // Act
        var json = CanaryServiceExtensionsJsonExtensions.ToJson(metadata);

        // Assert
        json.Should().NotContain("\"namespace\"");
        json.Should().NotContain("\"assembly\"");
        json.Should().NotContain("\"methods\"");
    }
}
