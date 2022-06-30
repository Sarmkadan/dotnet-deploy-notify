using BenchmarkDotNet.Running;

namespace DotNetDeployNotify.Benchmarks;

/// <summary>
/// Entry point for running benchmarks
/// </summary>
public static class Program
{
    /// <summary>
    /// Main entry point for benchmark execution
    /// </summary>
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}