#nullable enable

using System.Text.Json;
using DotNetDeployNotify.Caching;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class CacheEntryJsonExtensionsTests
{
    [Fact]
    public void ToJson_ValidEntry_ReturnsJsonString()
    {
        var entry = new CacheEntry<string> { Value = "test", ExpiresAt = DateTime.UtcNow.AddMinutes(1) };
        var json = entry.ToJson();
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("test");
    }

    [Fact]
    public void ToJson_NullEntry_ThrowsArgumentNullException()
    {
        CacheEntry<string>? entry = null;
        Action act = () => entry!.ToJson();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsCacheEntry()
    {
        var json = "{\"value\":\"test\",\"expiresAt\":\"2026-07-31T23:59:59Z\",\"createdAt\":\"2026-07-31T00:00:00Z\"}";
        var entry = CacheEntryJsonExtensions.FromJson<string>(json);
        entry.Should().NotBeNull();
        entry!.Value.Should().Be("test");
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        var json = "{ invalid json }";
        Action act = () => CacheEntryJsonExtensions.FromJson<string>(json);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrue()
    {
        var json = "{\"value\":\"test\",\"expiresAt\":\"2026-07-31T23:59:59Z\",\"createdAt\":\"2026-07-31T00:00:00Z\"}";
        var result = CacheEntryJsonExtensions.TryFromJson<string>(json, out var entry);
        result.Should().BeTrue();
        entry.Should().NotBeNull();
        entry!.Value.Should().Be("test");
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        var json = "{ invalid json }";
        var result = CacheEntryJsonExtensions.TryFromJson<string>(json, out var entry);
        result.Should().BeFalse();
        entry.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_NullJson_ThrowsArgumentNullException()
    {
        string? json = null;
        Action act = () => CacheEntryJsonExtensions.TryFromJson<string>(json!, out _);
        act.Should().Throw<ArgumentNullException>();
    }
}
