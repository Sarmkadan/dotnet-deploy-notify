#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Caching;

/// <summary>
/// Extension methods for <see cref="CacheEntry{T}"/>
/// </summary>
public static class CacheEntryExtensions
{
    /// <summary>
    /// Gets the remaining time-to-live for the cache entry.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="entry">The cache entry.</param>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/></exception>
    /// <returns>The remaining time-to-live, or TimeSpan.Zero if expired.</returns>
    public static TimeSpan GetTimeToLive<T>(this CacheEntry<T> entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var now = DateTime.UtcNow;
        if (now >= entry.ExpiresAt)
        {
            return TimeSpan.Zero;
        }

        return entry.ExpiresAt - now;
    }

    /// <summary>
    /// Checks if the cache entry is still valid and not expired.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="entry">The cache entry.</param>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/></exception>
    /// <returns>True if the entry is valid; otherwise, false.</returns>
    public static bool IsValid<T>(this CacheEntry<T> entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return !entry.IsExpired;
    }

    /// <summary>
    /// Gets the age of the cache entry (time since creation).
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="entry">The cache entry.</param>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/></exception>
    /// <returns>The age of the entry.</returns>
    public static TimeSpan GetAge<T>(this CacheEntry<T> entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return DateTime.UtcNow - entry.CreatedAt;
    }

    /// <summary>
    /// Gets the expiration percentage - how much of the TTL has been consumed.
    /// Returns a value between 0 (just created) and 1 (expired).
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="entry">The cache entry.</param>
    /// <exception cref="ArgumentNullException"><paramref name="entry"/> is <see langword="null"/></exception>
    /// <returns>The expiration percentage (0 to 1).</returns>
    public static double GetExpirationPercentage<T>(this CacheEntry<T> entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var ttl = entry.ExpiresAt - entry.CreatedAt;
        if (ttl <= TimeSpan.Zero)
        {
            return 1.0;
        }

        var elapsed = DateTime.UtcNow - entry.CreatedAt;
        return Math.Min(1.0, elapsed.TotalMilliseconds / ttl.TotalMilliseconds);
    }
}