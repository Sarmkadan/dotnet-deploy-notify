using System.Text.Json;
using DotNetDeployNotify.Core.Models;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class WebhookPayloadJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsJsonString()
    {
        // Arrange
        var payload = new WebhookPayload();

        // Act
        var json = WebhookPayloadJsonExtensions.ToJson(payload);

        // Assert
        Assert.NotEmpty(json);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => WebhookPayloadJsonExtensions.ToJson(null));
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsWebhookPayload()
    {
        // Arrange
        var payload = new WebhookPayload();
        var json = WebhookPayloadJsonExtensions.ToJson(payload);

        // Act
        var deserializedPayload = WebhookPayloadJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserializedPayload);
    }

    [Fact]
    public void FromJson_InvalidJson_ReturnsNull()
    {
        // Act
        var deserializedPayload = WebhookPayloadJsonExtensions.FromJson("Invalid Json");

        // Assert
        Assert.Null(deserializedPayload);
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndWebhookPayload()
    {
        // Arrange
        var payload = new WebhookPayload();
        var json = WebhookPayloadJsonExtensions.ToJson(payload);

        // Act
        var success = WebhookPayloadJsonExtensions.TryFromJson(json, out var deserializedPayload);

        // Assert
        Assert.True(success);
        Assert.NotNull(deserializedPayload);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Act
        var success = WebhookPayloadJsonExtensions.TryFromJson("Invalid Json", out var deserializedPayload);

        // Assert
        Assert.False(success);
        Assert.Null(deserializedPayload);
    }
}
