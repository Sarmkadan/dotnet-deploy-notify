#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Caching;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the CacheEntryExtensionsJsonExtensions class.
/// </summary>
public class CacheEntryExtensionsJsonExtensionsTests
{
    [Fact]
    public void ToJson_ReturnsValidJson()
    {
        // Act
        var json = CacheEntryExtensionsJsonExtensions.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"type\":\"CacheEntryExtensions\"");
        json.Should().Contain("\"namespace\":\"DotNetDeployNotify.Caching\"");
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsMetadata()
    {
        // Arrange
        var json = "{\"type\":\"CacheEntryExtensions\",\"namespace\":\"DotNetDeployNotify.Caching\",\"assembly\":\"DotNetDeployNotify\",\"methods\":[\"GetTimeToLive\",\"IsValid\"]}";

        // Act
        var result = CacheEntryExtensionsJsonExtensions.FromJson(json);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be("CacheEntryExtensions");
        result.Methods.Should().Contain("GetTimeToLive");
        result.Methods.Should().Contain("IsValid");
    }

    [Fact]
    public void FromJson_InvalidJson_ReturnsNull()
    {
        // Act
        var result = CacheEntryExtensionsJsonExtensions.FromJson("invalid");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => CacheEntryExtensionsJsonExtensions.FromJson(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndMetadata()
    {
        // Arrange
        var json = "{\"type\":\"CacheEntryExtensions\",\"namespace\":\"DotNetDeployNotify.Caching\",\"assembly\":\"DotNetDeployNotify\",\"methods\":[\"GetTimeToLive\"]}";

        // Act
        var success = CacheEntryExtensionsJsonExtensions.TryFromJson(json, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result!.Type.Should().Be("CacheEntryExtensions");
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Act
        var success = CacheEntryExtensionsJsonExtensions.TryFromJson("invalid", out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }
}
