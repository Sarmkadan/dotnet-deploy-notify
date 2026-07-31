// Copyright (c) 2024
// SPDX-License-Identifier: MIT

using System;
using DotNetDeployNotify.Caching;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class CacheEntryExtensionsTests
{
    [Fact]
    public void GetTimeToLive_ValidEntry_ReturnsCorrectTime()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;
        var expiresAt = createdAt.AddSeconds(10);
        var entry = new CacheEntry<string> { CreatedAt = createdAt, ExpiresAt = expiresAt };

        // Act
        var ttl = entry.GetTimeToLive();

        // Assert
        Assert.True(ttl.TotalSeconds > 0 && ttl.TotalSeconds <= 10);
    }

    [Fact]
    public void GetTimeToLive_ExpiredEntry_ReturnsZero()
    {
        // Arrange
        var entry = new CacheEntry<string> { ExpiresAt = DateTime.UtcNow.AddSeconds(-1) };

        // Act
        var ttl = entry.GetTimeToLive();

        // Assert
        Assert.Equal(TimeSpan.Zero, ttl);
    }

    [Fact]
    public void IsValid_ValidEntry_ReturnsTrue()
    {
        // Arrange
        var entry = new CacheEntry<string> { ExpiresAt = DateTime.UtcNow.AddSeconds(10) };

        // Act
        var isValid = entry.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_ExpiredEntry_ReturnsFalse()
    {
        // Arrange
        var entry = new CacheEntry<string> { ExpiresAt = DateTime.UtcNow.AddSeconds(-1) };

        // Act
        var isValid = entry.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void GetAge_ValidEntry_ReturnsCorrectAge()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddSeconds(-5);
        var entry = new CacheEntry<string> { CreatedAt = createdAt };

        // Act
        var age = entry.GetAge();

        // Assert
        Assert.True(age.TotalSeconds >= 5);
    }

    [Fact]
    public void GetExpirationPercentage_ValidEntry_ReturnsCorrectPercentage()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddSeconds(-2);
        var expiresAt = createdAt.AddSeconds(10);
        var entry = new CacheEntry<string> { CreatedAt = createdAt, ExpiresAt = expiresAt };

        // Act
        var percentage = entry.GetExpirationPercentage();

        // Assert
        Assert.True(percentage > 0 && percentage < 1);
    }

    [Fact]
    public void GetExpirationPercentage_ExpiredEntry_ReturnsOne()
    {
        // Arrange
        var entry = new CacheEntry<string> { ExpiresAt = DateTime.UtcNow.AddSeconds(-1) };

        // Act
        var percentage = entry.GetExpirationPercentage();

        // Assert
        Assert.Equal(1.0, percentage);
    }

    [Fact]
    public void GetTimeToLive_NullEntry_ThrowsArgumentNullException()
    {
        // Arrange
        CacheEntry<string>? entry = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entry!.GetTimeToLive());
    }
}
