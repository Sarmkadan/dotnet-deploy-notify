using BenchmarkDotNet.Attributes;
using DotNetDeployNotify.Caching;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotNetDeployNotify.Benchmarks.Benchmarks;

/// <summary>
/// Benchmark class for cache operations.
/// </summary>
[MemoryDiagnoser]
public class CacheBenchmarks
{
    private MemoryCacheService _cacheService;
    private CacheKeyBuilder _keyBuilder;

    /// <summary>
    /// Initializes the cache service and key builder.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _cacheService = new MemoryCacheService(NullLogger<MemoryCacheService>.Instance);
        _keyBuilder = new CacheKeyBuilder();
    }

    /// <summary>
    /// Builds a cache key by adding project, version, and branch parts.
    /// </summary>
    /// <returns>A cache key string.</returns>
    [Benchmark]
    public string BuildKey()
    {
        return new CacheKeyBuilder()
            .Add("project")
            .Add("version")
            .Add("branch")
            .Build();
    }

    /// <summary>
    /// Sets and gets a value in the cache.
    /// </summary>
    [Benchmark]
    public void CacheSetAndGet()
    {
        _cacheService.Set("key1", "value1");
        var val = _cacheService.Get<string>("key1");
    }
}
