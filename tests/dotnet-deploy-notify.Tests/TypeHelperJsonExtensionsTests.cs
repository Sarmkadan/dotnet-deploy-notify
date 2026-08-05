#nullable enable

using System;
using DotNetDeployNotify.Utilities;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class TypeHelperJsonExtensionsTests
{
    [Fact]
    public void ToJson_ReturnsValidMetadata_ForTypeHelper()
    {
        // Act
        var json = TypeHelperJsonExtensions.ToJson();

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        var metadata = TypeHelperJsonExtensions.FromJson(json);
        metadata.Should().NotBeNull();
        metadata!.Type.Should().Be(nameof(TypeHelper));
        metadata.Namespace.Should().Be(typeof(TypeHelper).Namespace);
        metadata.Assembly.Should().Be(typeof(TypeHelper).Assembly.GetName().Name);
        metadata.Methods.Should().NotBeNull();
    }

    [Fact]
    public void ToJson_Indented_ProducesMultiLineOutput()
    {
        // Act
        var compact = TypeHelperJsonExtensions.ToJson(indented: false);
        var indented = TypeHelperJsonExtensions.ToJson(indented: true);

        // Assert
        compact.Should().NotContain("\n");
        indented.Should().Contain("\n");
    }

    [Fact]
    public void FromJson_RoundTrips_ToJsonOutput()
    {
        // Arrange
        var json = TypeHelperJsonExtensions.ToJson();

        // Act
        var metadata = TypeHelperJsonExtensions.FromJson(json);

        // Assert
        metadata.Should().NotBeNull();
        metadata!.Methods.Should().NotBeEmpty();
    }

    [Fact]
    public void FromJson_ReturnsNull_ForInvalidJson()
    {
        // Act
        var metadata = TypeHelperJsonExtensions.FromJson("{ this is not valid json ");

        // Assert
        metadata.Should().BeNull();
    }

    [Fact]
    public void FromJson_ThrowsArgumentNullException_ForNullInput()
    {
        // Act
        Action act = () => TypeHelperJsonExtensions.FromJson(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryFromJson_ReturnsTrueAndValue_ForValidJson()
    {
        // Arrange
        var json = TypeHelperJsonExtensions.ToJson();

        // Act
        var result = TypeHelperJsonExtensions.TryFromJson(json, out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().NotBeNull();
        value!.Type.Should().Be(nameof(TypeHelper));
    }

    [Fact]
    public void TryFromJson_ReturnsFalse_ForMalformedJson()
    {
        // Act
        var result = TypeHelperJsonExtensions.TryFromJson("not json at all", out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_ThrowsArgumentNullException_ForNullInput()
    {
        // Act
        Action act = () => TypeHelperJsonExtensions.TryFromJson(null!, out _);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TypeHelperMetadata_PropertiesAreSettableAndReadable()
    {
        // Arrange
        var metadata = new TypeHelperJsonExtensions.TypeHelperMetadata
        {
            Type = "SomeType",
            Namespace = "Some.Namespace",
            Assembly = "SomeAssembly",
            Methods = new[] { "MethodA", "MethodB" }
        };

        // Assert
        metadata.Type.Should().Be("SomeType");
        metadata.Namespace.Should().Be("Some.Namespace");
        metadata.Assembly.Should().Be("SomeAssembly");
        metadata.Methods.Should().BeEquivalentTo(new[] { "MethodA", "MethodB" });
    }
}
