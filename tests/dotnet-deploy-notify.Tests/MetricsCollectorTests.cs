#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using DotNetDeployNotify.Monitoring;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class MetricsCollectorTests
{
    private static MetricsCollector CreateCollector()
        => new MetricsCollector(NullLogger<MetricsCollector>.Instance);

    [Fact]
    public void RecordMetric_ShouldCreateMetric_WithValue()
    {
        var collector = CreateCollector();

        collector.RecordMetric("response_time", 123.4);

        var metric = collector.GetMetric("response_time");
        metric.Should().NotBeNull();
        metric!.Values.Should().ContainSingle().Which.Should().Be(123.4);
        metric.Count.Should().Be(0); // Count is for counters only
    }

    [Fact]
    public void IncrementCounter_ShouldCreateOrUpdateCounter()
    {
        var collector = CreateCollector();

        collector.IncrementCounter("hits", 2);
        collector.IncrementCounter("hits", 3);

        var metric = collector.GetMetric("hits");
        metric.Should().NotBeNull();
        metric!.Count.Should().Be(5);
        metric.Values.Should().BeEmpty(); // Counter metrics store only Count
    }

    [Fact]
    public void GetStatistics_ShouldReturnCorrectAggregates()
    {
        var collector = CreateCollector();

        // Record a known set of values
        var values = new[] { 10.0, 20.0, 30.0, 40.0, 50.0 };
        foreach (var v in values)
            collector.RecordMetric("latency", v);

        var stats = collector.GetStatistics("latency");
        stats.Should().NotBeNull();

        stats!.Count.Should().Be(values.Length);
        stats.Sum.Should().Be(values.Sum());
        stats.Average.Should().Be(values.Average());
        stats.Min.Should().Be(values.Min());
        stats.Max.Should().Be(values.Max());
        stats.Median.Should().Be(30.0); // middle value
        stats.Percentile95.Should().Be(50.0); // 95th percentile of 5 items -> last item
        stats.Percentile99.Should().Be(50.0);
    }

    [Fact]
    public void GetStatistics_NonExistingMetric_ShouldReturnNull()
    {
        var collector = CreateCollector();

        var stats = collector.GetStatistics("unknown");
        stats.Should().BeNull();
    }

    [Fact]
    public void Clear_ShouldRemoveAllMetrics()
    {
        var collector = CreateCollector();

        collector.RecordMetric("a", 1);
        collector.IncrementCounter("b", 1);

        collector.GetAllMetrics().Should().NotBeEmpty();

        collector.Clear();

        collector.GetAllMetrics().Should().BeEmpty();
    }

    [Fact]
    public void ResetMetric_ShouldRemoveSpecificMetric()
    {
        var collector = CreateCollector();

        collector.RecordMetric("metric1", 1);
        collector.RecordMetric("metric2", 2);

        collector.GetAllMetrics().Select(m => m.Name).Should().Contain(new[] { "metric1", "metric2" });

        collector.ResetMetric("metric1");

        var remaining = collector.GetAllMetrics();
        remaining.Should().ContainSingle().Which.Name.Should().Be("metric2");
    }

    [Fact]
    public void RecordMetric_NullName_ShouldThrowArgumentNullException()
    {
        var collector = CreateCollector();

        Action act = () => collector.RecordMetric(null!, 1);
        act.Should().Throw<ArgumentNullException>();
    }
}
