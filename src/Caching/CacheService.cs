#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Caching;

/// <summary>
/// Cache entry with expiration support
/// </summary>
public sealed class CacheEntry<T>
{
    public T? Value { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
}

/// <summary>
/// In-memory cache service with TTL support and statistics
/// </summary>
public interface ICacheService
{
    T? Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan? ttl = null);
    void Remove(string key);
    void Clear();
    int Count { get; }
    CacheStatistics GetStatistics();
}

/// <summary>
/// Cache statistics for monitoring
/// </summary>
public sealed class CacheStatistics
{
    public int TotalItems { get; set; }
    public long Hits { get; set; }
    public long Misses { get; set; }
    public DateTime LastCleanup { get; set; }

    public double HitRate => (Hits + Misses) > 0 ? (double)Hits / (Hits + Misses) * 100 : 0;
}

/// <summary>
/// Default in-memory cache implementation
/// </summary>
public sealed class MemoryCacheService : ICacheService
{
    private readonly Dictionary<string, object> _cache = new();
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly TimeSpan _defaultTtl;
    private long _hits;
    private long _misses;
    private DateTime _lastCleanup = DateTime.UtcNow;

    public int Count => _cache.Count;

    public MemoryCacheService(ILogger<MemoryCacheService> logger)
    {
        _logger = logger;
        _defaultTtl = TimeSpan.FromMinutes(5);
        StartCleanupTask();
    }

    public T? Get<T>(string key)
    {
        lock (_cache)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                var cacheEntry = entry as CacheEntry<T>;
                if (cacheEntry is not null)
                {
                    if (cacheEntry.IsExpired)
                    {
                        _cache.Remove(key);
                        _misses++;
                        return default;
                    }

                    _hits++;
                    return cacheEntry.Value;
                }
            }

            _misses++;
            return default;
        }
    }

    public void Set<T>(string key, T value, TimeSpan? ttl = null)
    {
        var expiresAt = DateTime.UtcNow.Add(ttl ?? _defaultTtl);

        lock (_cache)
        {
            _cache[key] = new CacheEntry<T>
            {
                Value = value,
                ExpiresAt = expiresAt
            };

            _logger.LogDebug("Cached entry: {Key} (expires in {Ttl}s)",
                key, (expiresAt - DateTime.UtcNow).TotalSeconds);
        }
    }

    public void Remove(string key)
    {
        lock (_cache)
        {
            if (_cache.Remove(key))
            {
                _logger.LogDebug("Removed cache entry: {Key}", key);
            }
        }
    }

    public void Clear()
    {
        lock (_cache)
        {
            _cache.Clear();
            _logger.LogInformation("Cache cleared");
        }
    }

    public CacheStatistics GetStatistics()
    {
        lock (_cache)
        {
            return new CacheStatistics
            {
                TotalItems = _cache.Count,
                Hits = _hits,
                Misses = _misses,
                LastCleanup = _lastCleanup
            };
        }
    }

    /// <summary>
    /// Periodically removes expired entries from the cache
    /// </summary>
    private void StartCleanupTask()
    {
        var cleanupTask = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(1)).ConfigureAwait(false);

                    lock (_cache)
                    {
                        var expiredKeys = _cache
                            .Where(kvp => kvp.Value is IExpirableEntry && ((IExpirableEntry)kvp.Value).IsExpired)
                            .Select(kvp => kvp.Key)
                            .ToList();

                        foreach (var key in expiredKeys)
                        {
                            _cache.Remove(key);
                        }

                        if (expiredKeys.Any())
                        {
                            _lastCleanup = DateTime.UtcNow;
                            _logger.LogDebug("Cleaned up {Count} expired cache entries", expiredKeys.Count);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cache cleanup task failed");
                }
            }
        });
    }
}

/// <summary>
/// Marker interface for expirable cache entries
/// </summary>
public interface IExpirableEntry
{
    bool IsExpired { get; }
}

/// <summary>
/// Distributed cache decorator for adding caching to services
/// </summary>
public sealed class CachedRepository<T> where T : class
{
    private readonly ILogger _logger;
    private readonly ICacheService _cacheService;
    private readonly Func<Task<List<T>>> _loadFunction;
    private readonly TimeSpan _cacheDuration;

    public CachedRepository(
        ICacheService cacheService,
        ILogger logger,
        Func<Task<List<T>>> loadFunction,
        TimeSpan? cacheDuration = null)
    {
        _cacheService = cacheService;
        _logger = logger;
        _loadFunction = loadFunction;
        _cacheDuration = cacheDuration ?? TimeSpan.FromMinutes(5);
    }

    public async Task<List<T>> GetAllAsync(string cacheKey)
    {
        // Try to get from cache first
        var cached = _cacheService.Get<List<T>>(cacheKey);
        if (cached is not null)
        {
            _logger.LogDebug("Returning {Count} items from cache: {CacheKey}", cached.Count, cacheKey);
            return cached;
        }

        // Load from source
        _logger.LogDebug("Cache miss for {CacheKey}, loading from source", cacheKey);
        var data = await _loadFunction().ConfigureAwait(false);

        // Store in cache
        _cacheService.Set(cacheKey, data, _cacheDuration);

        return data;
    }

    public void InvalidateCache(string cacheKey)
    {
        _cacheService.Remove(cacheKey);
        _logger.LogInformation("Invalidated cache: {CacheKey}", cacheKey);
    }
}

/// <summary>
/// Cache key builder for consistent cache key generation
/// </summary>
public sealed class CacheKeyBuilder
{
    private readonly List<string> _parts = new();
    private const string Separator = ":";

    public CacheKeyBuilder Add(string part)
    {
        if (!string.IsNullOrEmpty(part))
            _parts.Add(part);
        return this;
    }

    public CacheKeyBuilder Add(object? value)
    {
        if (value is not null)
            _parts.Add(value.ToString()!);
        return this;
    }

    public CacheKeyBuilder Add(params string[] parts)
    {
        foreach (var part in parts.Where(p => !string.IsNullOrEmpty(p)))
            _parts.Add(part);
        return this;
    }

    public string Build()
    {
        return string.Join(Separator, _parts);
    }

    public static string Build(params object[] parts)
    {
        return string.Join(Separator, parts.Where(p => p is not null).Select(p => p.ToString()));
    }

    public override string ToString() => Build();
}
