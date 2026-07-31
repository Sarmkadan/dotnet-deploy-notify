#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Caching;
using FluentAssertions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class CacheEntryValidationTests
{
    [Fact]
    public void Validate_Null_ThrowsArgumentNullException()
    {
        Action act = () => ((CacheStatistics)null!).Validate();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validate_Valid_ReturnsEmptyList()
    {
        var stats = new CacheStatistics { TotalItems = 1, Hits = 1, Misses = 0, LastCleanup = DateTime.UtcNow };
        var result = stats.Validate();
        result.Should().BeEmpty();
    }

    [Fact]
    public void Validate_InvalidFields_ReturnsErrors()
    {
        var stats = new CacheStatistics { TotalItems = -1, Hits = -1, Misses = -1, LastCleanup = default };
        var result = stats.Validate();
        result.Should().NotBeEmpty();
        result.Should().HaveCount(4);
    }

    [Fact]
    public void IsValid_Valid_ReturnsTrue()
    {
        var stats = new CacheStatistics { TotalItems = 1, Hits = 1, Misses = 0, LastCleanup = DateTime.UtcNow };
        stats.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_Invalid_ReturnsFalse()
    {
        var stats = new CacheStatistics { TotalItems = -1 };
        stats.IsValid().Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_Valid_DoesNotThrow()
    {
        var stats = new CacheStatistics { TotalItems = 1, Hits = 1, Misses = 0, LastCleanup = DateTime.UtcNow };
        stats.Invoking(s => s.EnsureValid()).Should().NotThrow();
    }

    [Fact]
    public void EnsureValid_Invalid_ThrowsArgumentException()
    {
        var stats = new CacheStatistics { TotalItems = -1 };
        stats.Invoking(s => s.EnsureValid()).Should().Throw<ArgumentException>();
    }
}
