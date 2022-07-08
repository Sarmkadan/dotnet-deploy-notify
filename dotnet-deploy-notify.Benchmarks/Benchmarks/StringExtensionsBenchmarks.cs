using BenchmarkDotNet.Attributes;
using DotNetDeployNotify.Utilities;

namespace DotNetDeployNotify.Benchmarks.Benchmarks;

/// <summary>
/// Benchmark class for string extension methods.
/// </summary>
[MemoryDiagnoser]
public class StringExtensionsBenchmarks
{
    private const string Input = "This is a Test-String to be processed.";

    /// <summary>
    /// Benchmark for the ToSlug method.
    /// </summary>
    [Benchmark]
    public string ToSlug()
    {
        return Input.ToSlug();
    }

    /// <summary>
    /// Benchmark for the ToPascalCase method.
    /// </summary>
    [Benchmark]
    public string ToPascalCase()
    {
        return Input.ToPascalCase();
    }

    /// <summary>
    /// Benchmark for the MaskSensitive method.
    /// </summary>
    [Benchmark]
    public string MaskSensitive()
    {
        return Input.MaskSensitive();
    }
}
