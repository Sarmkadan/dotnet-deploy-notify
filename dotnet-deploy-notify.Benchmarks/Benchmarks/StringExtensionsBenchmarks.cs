using BenchmarkDotNet.Attributes;
using DotNetDeployNotify.Utilities;

namespace DotNetDeployNotify.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class StringExtensionsBenchmarks
{
    private const string Input = "This is a Test-String to be processed.";

    [Benchmark]
    public string ToSlug()
    {
        return Input.ToSlug();
    }

    [Benchmark]
    public string ToPascalCase()
    {
        return Input.ToPascalCase();
    }

    [Benchmark]
    public string MaskSensitive()
    {
        return Input.MaskSensitive();
    }
}
