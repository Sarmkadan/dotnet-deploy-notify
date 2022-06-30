using BenchmarkDotNet.Attributes;
using DotNetDeployNotify.Caching;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotNetDeployNotify.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class CacheBenchmarks
{
    private MemoryCacheService _cacheService;
    private CacheKeyBuilder _keyBuilder;

    [GlobalSetup]
    public void Setup()
    {
        _cacheService = new MemoryCacheService(NullLogger<MemoryCacheService>.Instance);
        _keyBuilder = new CacheKeyBuilder();
    }

    [Benchmark]
    public string BuildKey()
    {
        return new CacheKeyBuilder()
            .Add("project")
            .Add("version")
            .Add("branch")
            .Build();
    }

    [Benchmark]
    public void CacheSetAndGet()
    {
        _cacheService.Set("key1", "value1");
        var val = _cacheService.Get<string>("key1");
    }
}
