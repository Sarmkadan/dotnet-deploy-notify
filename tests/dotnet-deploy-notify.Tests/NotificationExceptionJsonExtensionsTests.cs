using Xunit;
using DotNetDeployNotify.Core.Exceptions;
using System;

namespace DotNetDeployNotify.Tests;

public class NotificationExceptionJsonExtensionsTests
{
    [Fact]
    public void ToJson_ValidObject_ReturnsJsonString()
    {
        // Arrange
        var exception = new NotificationException("Test error");

        // Act
        var json = exception.ToJson();

        // Assert
        Assert.NotNull(json);
        Assert.Contains("message", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Test error", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToJson_Indented_ReturnsFormattedJson()
    {
        // Arrange
        var exception = new NotificationException("Test error");

        // Act
        var json = exception.ToJson(indented: true);

        // Assert
        Assert.NotNull(json);
        Assert.Contains(Environment.NewLine, json);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        NotificationException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.ToJson());
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsObject()
    {
        // Arrange
        var json = "{\"message\":\"Deserialized error\"}";

        // Act
        var result = NotificationExceptionJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Deserialized error", result.Message);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => NotificationExceptionJsonExtensions.FromJson(json!));
    }

    [Fact]
    public void FromJson_EmptyInput_ThrowsArgumentException()
    {
        // Arrange
        var json = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => NotificationExceptionJsonExtensions.FromJson(json));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndObject()
    {
        // Arrange
        var json = "{\"message\":\"Try parse success\"}";

        // Act
        var success = NotificationExceptionJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal("Try parse success", result.Message);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var json = "this is not valid json";

        // Act
        var success = NotificationExceptionJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_NullInput_ThrowsArgumentException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => NotificationExceptionJsonExtensions.TryFromJson(json!, out _));
    }
}
