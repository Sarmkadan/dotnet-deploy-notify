// Copyright (c) 2024
// SPDX-License-Identifier: MIT

using System;
using System.Threading.Tasks;
using DotNetDeployNotify.Caching;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class CacheEntryTests
{
    [Fact]
    public void CacheEntry_IsExpired_ReturnsFalseWhenNotPastExpiration()
    {
        // Arrange
        var entry = new CacheEntry<int>
        {
            Value = 42,
            ExpiresAt = DateTime.UtcNow.AddSeconds(5) // future
        };

        // Act
        var isExpired = entry.IsExpired;

        // Assert
        Assert.False(isExpired);
    }

    [Fact]
    public void CacheEntry_IsExpired_ReturnsTrueWhenPastExpiration()
    {
        // Arrange
        var entry = new CacheEntry<string>
        {
            Value = "test",
            ExpiresAt = DateTime.UtcNow.AddSeconds(-1) // past
        };

        // Act
        var isExpired = entry.IsExpired;

        // Assert
        Assert.True(isExpired);
    }

    [Fact]
    public void MemoryCacheService_SetAndGet_ReturnsStoredValue()
    {
        // Arrange
        var logger = NullLogger<MemoryCacheService>.Instance;
        var cache = new MemoryCacheService(logger);
        const string key = "my-key";
        const string expected = "hello";

        // Act
        cache.Set(key, expected, TimeSpan.FromMinutes(1));
        var actual = cache.Get<string>(key);

        // Assert
        Assert.Equal(expected, actual);
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void MemoryCacheService_Get_ReturnsDefaultWhenKeyMissing()
    {
        // Arrange
        var logger = NullLogger<MemoryCacheService>.Instance;
        var cache = new MemoryCacheService(logger);

        // Act
        var result = cache.Get<int>("non‑existent");

        // Assert
        Assert.Equal(default, result);
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void MemoryCacheService_Get_ReturnsDefaultWhenExpired()
    {
        // Arrange
        var logger = NullLogger<MemoryCacheService>.Instance;
        var cache = new MemoryCacheService(logger);
        const string key = "temp";
        cache.Set(key, 123, TimeSpan.FromMilliseconds(10));

        // Give the entry a chance to expire
        Task.Delay(50).Wait();

        // Act
        var result = cache.Get<int>(key);

        // Assert
        Assert.Equal(default, result);
        Assert.Equal(0, cache.Count); // expired entry should be removed
    }

    [Fact]
    public void MemoryCacheService_Statistics_TracksHitsAndMisses()
    {
        // Arrange
        var logger = NullLogger<MemoryCacheService>.Instance;
        var cache = new MemoryCacheService(logger);
        const string key = "stats-key";

        // Miss first
        _ = cache.Get<string>(key);

        // Set and hit
        cache.Set(key, "value", TimeSpan.FromMinutes(1));
        _ = cache.Get<string>(key);

        // Act
        var stats = cache.GetStatistics();

        // Assert
        Assert.Equal(1, stats.Hits);
        Assert.Equal(1, stats.Misses);
        Assert.Equal(1, stats.TotalItems);
        Assert.True(stats.LastCleanup <= DateTime.UtcNow);
    }

    [Fact]
    public void MemoryCacheService_Clear_RemovesAllEntries()
    {
        // Arrange
        var logger = NullLogger<MemoryCacheService>.Instance;
        var cache = new MemoryCacheService(logger);
        cache.Set("a", 1);
        cache.Set("b", 2);

        // Act
        cache.Clear();

        // Assert
        Assert.Equal(0, cache.Count);
        var stats = cache.GetStatistics();
        Assert.Equal(0, stats.TotalItems);
    }
}
