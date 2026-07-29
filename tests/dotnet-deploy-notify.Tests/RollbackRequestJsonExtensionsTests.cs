using System;
using System.Text.Json;
using DotNetDeployNotify.Core.Models;
using Xunit;

namespace DotNetDeployNotify.Tests;

public sealed class RollbackRequestJsonExtensionsTests
{
    private static RollbackRequest CreateSampleRequest()
    {
        // Create a minimal instance via deserialization; this works even if the type
        // does not expose a public parameterless constructor.
        return JsonSerializer.Deserialize<RollbackRequest>("{}")!;
    }

    [Fact]
    public void ToJson_WithValidRequest_ReturnsNonEmptyJson()
    {
        // Arrange
        var request = CreateSampleRequest();

        // Act
        var json = request.ToJson();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    [Fact]
    public void ToJson_NullArgument_ThrowsArgumentNullException()
    {
        // Arrange
        RollbackRequest? request = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => request!.ToJson());
    }

    [Fact]
    public void FromJson_NullArgument_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => RollbackRequestJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void FromJson_EmptyOrWhiteSpace_ReturnsNull()
    {
        // Act
        var resultEmpty = RollbackRequestJsonExtensions.FromJson(string.Empty);
        var resultWhiteSpace = RollbackRequestJsonExtensions.FromJson("   ");

        // Assert
        Assert.Null(resultEmpty);
        Assert.Null(resultWhiteSpace);
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndDeserializedValue()
    {
        // Arrange
        var request = CreateSampleRequest();
        var json = request.ToJson();

        // Act
        var success = RollbackRequestJsonExtensions.TryFromJson(json, out var deserialized);

        // Assert
        Assert.True(success);
        Assert.NotNull(deserialized);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNullValue()
    {
        // Arrange
        const string invalidJson = "{ this is not valid json }";

        // Act
        var success = RollbackRequestJsonExtensions.TryFromJson(invalidJson, out var deserialized);

        // Assert
        Assert.False(success);
        Assert.Null(deserialized);
    }

    [Fact]
    public void TryFromJson_NullArgument_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => RollbackRequestJsonExtensions.TryFromJson(null!, out _));
    }
}
