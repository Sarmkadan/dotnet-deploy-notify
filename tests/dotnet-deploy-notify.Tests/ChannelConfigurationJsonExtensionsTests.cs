using System.Text.Json;
using DotNetDeployNotify.Core.Models;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class ChannelConfigurationJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithValidObject_ReturnsNonEmptyJson()
    {
        // Arrange
        var config = new ChannelConfiguration();

        // Act
        var json = config.ToJson();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));

        // Verify that the JSON can be deserialized back to an object
        var deserialized = ChannelConfigurationJsonExtensions.FromJson(json);
        Assert.NotNull(deserialized);
    }

    [Fact]
    public void ToJson_WithIndentedOption_ProducesIndentedJson()
    {
        // Arrange
        var config = new ChannelConfiguration();

        // Act
        var json = config.ToJson(indented: true);

        // Assert
        // Indented JSON contains line breaks
        Assert.Contains('\n', json);
    }

    [Fact]
    public void FromJson_NullOrEmpty_ReturnsNull()
    {
        // Null input should be treated as empty and return null
        Assert.Null(ChannelConfigurationJsonExtensions.FromJson(null!));

        // Empty string should also return null
        Assert.Null(ChannelConfigurationJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        const string invalidJson = "{ this is not valid json }";

        // Act / Assert
        Assert.Throws<JsonException>(() => ChannelConfigurationJsonExtensions.FromJson(invalidJson));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndObject()
    {
        // Arrange
        var config = new ChannelConfiguration();
        var json = config.ToJson();

        // Act
        var success = ChannelConfigurationJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        const string invalidJson = "{ invalid json }";

        // Act
        var success = ChannelConfigurationJsonExtensions.TryFromJson(invalidJson, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_NullOrEmpty_ReturnsFalseAndNull()
    {
        // Null input
        var successNull = ChannelConfigurationJsonExtensions.TryFromJson(null!, out var resultNull);
        Assert.False(successNull);
        Assert.Null(resultNull);

        // Empty string input
        var successEmpty = ChannelConfigurationJsonExtensions.TryFromJson(string.Empty, out var resultEmpty);
        Assert.False(successEmpty);
        Assert.Null(resultEmpty);
    }
}
