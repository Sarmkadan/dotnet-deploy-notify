[MemoryDiagnoser]
public class TrafficSplitterBenchmarks
{
    [Benchmark]
    public void Benchmark_TrafficSplitter_Split()
    {
        // Setup
        var splitter = new TrafficSplitter();
        var data = new byte[1024];
        // Benchmark
        for (int i = 0; i < 100; i++)
        {
            splitter.Split(data);
        }
    }

    [Benchmark]
    public void Benchmark_TrafficSplitter_Merge()
    {
        // Setup
        var splitter = new TrafficSplitter();
        var data1 = new byte[1024];
        var data2 = new byte[1024];
        // Benchmark
        for (int i = 0; i < 100; i++)
        {
            splitter.Merge(data1, data2);
        }
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void Benchmark_TrafficSplitter_Split_Merge()
    {
        // Setup
        var splitter = new TrafficSplitter();
        var data = new byte[10];
        // Benchmark
        for (int i = 0; i < 100; i++)
        {
            splitter.Split(data);
            splitter.Merge(data, data);
        }
    }
}
