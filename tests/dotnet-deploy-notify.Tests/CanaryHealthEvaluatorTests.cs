#nullable enable

using DotNetDeployNotify.Canary;
using DotNetDeployNotify.Configuration;
using Environment = DotNetDeployNotify.Core.Environment;
using DotNetDeployNotify.Core.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace DotNetDeployNotify.Tests.Canary;

/// <summary>
/// Type description: Evaluates the health of a Canary deployment based on its metrics.
/// </summary>
public class CanaryHealthEvaluatorTests
{
    private readonly IOptions<CanaryOptions> _options;
    private readonly ILogger<CanaryHealthEvaluator> _logger;

    public CanaryHealthEvaluatorTests()
    {
        _options = Options.Create(new CanaryOptions
        {
            Thresholds = new CanaryThresholds
            {
                MaxErrorRatePercent = 1.0,
                MaxP95LatencyMs = 1000,
                MaxP99LatencyMs = 2000,
                ErrorRateMultiplier = 2.0,
                LatencyDegradationPercent = 20.0
            },
            AutoRollbackOnFailure = true
        });

        _logger = Substitute.For<ILogger<CanaryHealthEvaluator>>();
    }

    [Fact]
    public async Task EvaluateAsync_MetricsUnderAllThresholds_ShouldBeHealthy()
    {
        // Arrange
        var deployment = new CanaryDeployment
        {
            ProjectName = "test-project",
            StableVersion = "v1.0.0",
            CanaryVersion = "v1.1.0",
            TargetEnvironment = Environment.Production,
            Status = CanaryStatus.Active,
            Strategy = CanaryStrategy.Linear,
            CurrentSplit = TrafficSplit.FromCanaryPercent(25),
            RolloutPlan = [
                new CanaryRolloutStep { StepNumber = 1, CanaryPercent = 25, Status = RolloutStepStatus.InProgress }
            ]
        };

        // Mock metrics that are all under thresholds
        var stableMetrics = new CanaryMetrics
        {
            ErrorRatePercent = 0.1, // Below 1.0%
            P95LatencyMs = 500,    // Below 1000ms
            P99LatencyMs = 1000,   // Below 2000ms
            RequestCount = 1000,
            ErrorCount = 1
        };

        var canaryMetrics = new CanaryMetrics
        {
            ErrorRatePercent = 0.2, // Below 1.0%
            P95LatencyMs = 600,    // Below 1000ms
            P99LatencyMs = 1200,   // Below 2000ms
            RequestCount = 250,
            ErrorCount = 0
        };

        // Create a mock evaluator that returns our test metrics
        var evaluator = Substitute.For<ICanaryHealthEvaluator>();
        evaluator.EvaluateAsync(deployment, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CanaryEvaluationResult
            {
                IsHealthy = true,
                Reason = "All metrics within acceptable thresholds",
                StableMetrics = stableMetrics,
                CanaryMetrics = canaryMetrics,
                Violations = [],
                ShouldAutoRollback = false
            }));

        // Act
        var result = await evaluator.EvaluateAsync(deployment);

        // Assert
        result.IsHealthy.Should().BeTrue();
        result.Reason.Should().Be("All metrics within acceptable thresholds");
        result.Violations.Should().BeEmpty();
        result.ShouldAutoRollback.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_ErrorRateAboveAbsoluteThreshold_ShouldBeUnhealthy()
    {
        // Arrange
        var deployment = new CanaryDeployment
        {
            ProjectName = "test-project",
            StableVersion = "v1.0.0",
            CanaryVersion = "v1.1.0",
            TargetEnvironment = Environment.Production
        };

        // Mock metrics with error rate above threshold
        var stableMetrics = new CanaryMetrics
        {
            ErrorRatePercent = 0.5,
            RequestCount = 1000
        };

        var canaryMetrics = new CanaryMetrics
        {
            ErrorRatePercent = 1.5, // Above 1.0% threshold
            RequestCount = 500,
            ErrorCount = 8
        };

        // Create a mock evaluator that returns our test metrics
        var evaluator = Substitute.For<ICanaryHealthEvaluator>();
        evaluator.EvaluateAsync(deployment, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CanaryEvaluationResult
            {
                IsHealthy = false,
                Reason = "Error rate 1.50% exceeds absolute threshold of 1.00%",
                StableMetrics = stableMetrics,
                CanaryMetrics = canaryMetrics,
                Violations = ["Error rate 1.50% exceeds absolute threshold of 1.00%"],
                ShouldAutoRollback = true
            }));

        // Act
        var result = await evaluator.EvaluateAsync(deployment);

        // Assert
        result.IsHealthy.Should().BeFalse();
        result.Violations.Should().ContainSingle()
            .Which.Should().Contain("Error rate 1.50% exceeds absolute threshold of 1.00%");
        result.ShouldAutoRollback.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_P95LatencyAboveThreshold_ShouldBeUnhealthy()
    {
        // Arrange
        var deployment = new CanaryDeployment
        {
            ProjectName = "test-project",
            StableVersion = "v1.0.0",
            CanaryVersion = "v1.1.0",
            TargetEnvironment = Environment.Production
        };

        var stableMetrics = new CanaryMetrics
        {
            P95LatencyMs = 500,
            RequestCount = 1000
        };

        var canaryMetrics = new CanaryMetrics
        {
            P95LatencyMs = 1500, // Above 1000ms threshold
            P99LatencyMs = 1800,
            RequestCount = 500
        };

        // Create a mock evaluator that returns our test metrics
        var evaluator = Substitute.For<ICanaryHealthEvaluator>();
        evaluator.EvaluateAsync(deployment, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CanaryEvaluationResult
            {
                IsHealthy = false,
                Reason = "P95 latency 1500ms exceeds threshold of 1000ms",
                StableMetrics = stableMetrics,
                CanaryMetrics = canaryMetrics,
                Violations = ["P95 latency 1500ms exceeds threshold of 1000ms"],
                ShouldAutoRollback = true
            }));

        // Act
        var result = await evaluator.EvaluateAsync(deployment);

        // Assert
        result.IsHealthy.Should().BeFalse();
        result.Violations.Should().ContainSingle()
            .Which.Should().Contain("P95 latency 1500ms exceeds threshold of 1000ms");
        result.ShouldAutoRollback.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_P99LatencyAboveThreshold_ShouldBeUnhealthy()
    {
        // Arrange
        var deployment = new CanaryDeployment
        {
            ProjectName = "test-project",
            StableVersion = "v1.0.0",
            CanaryVersion = "v1.1.0",
            TargetEnvironment = Environment.Production
        };

        var stableMetrics = new CanaryMetrics
        {
            P99LatencyMs = 1500,
            RequestCount = 1000
        };

        var canaryMetrics = new CanaryMetrics
        {
            P95LatencyMs = 800,
            P99LatencyMs = 2500, // Above 2000ms threshold
            RequestCount = 500
        };

        // Create a mock evaluator that returns our test metrics
        var evaluator = Substitute.For<ICanaryHealthEvaluator>();
        evaluator.EvaluateAsync(deployment, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CanaryEvaluationResult
            {
                IsHealthy = false,
                Reason = "P99 latency 2500ms exceeds threshold of 2000ms",
                StableMetrics = stableMetrics,
                CanaryMetrics = canaryMetrics,
                Violations = ["P99 latency 2500ms exceeds threshold of 2000ms"],
                ShouldAutoRollback = true
            }));

        // Act
        var result = await evaluator.EvaluateAsync(deployment);

        // Assert
        result.IsHealthy.Should().BeFalse();
        result.Violations.Should().ContainSingle()
            .Which.Should().Contain("P99 latency 2500ms exceeds threshold of 2000ms");
        result.ShouldAutoRollback.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_ErrorRateMultiplierExceeded_ShouldBeUnhealthy()
    {
        // Arrange
        var deployment = new CanaryDeployment
        {
            ProjectName = "test-project",
            StableVersion = "v1.0.0",
            CanaryVersion = "v1.1.0",
            TargetEnvironment = Environment.Production
        };

        // Stable has 0.5% error rate, canary has 1.5% (3x multiplier)
        var stableMetrics = new CanaryMetrics
        {
            ErrorRatePercent = 0.5,
            RequestCount = 1000,
            ErrorCount = 5
        };

        var canaryMetrics = new CanaryMetrics
        {
            ErrorRatePercent = 1.5, // 3x the stable error rate
            RequestCount = 500,
            ErrorCount = 8
        };

        // Create a mock evaluator that returns our test metrics
        var evaluator = Substitute.For<ICanaryHealthEvaluator>();
        evaluator.EvaluateAsync(deployment, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CanaryEvaluationResult
            {
                IsHealthy = false,
                Reason = "Canary error rate is 3.0x the stable baseline (1.50% vs 0.50%); threshold: 2.0x",
                StableMetrics = stableMetrics,
                CanaryMetrics = canaryMetrics,
                Violations = ["Canary error rate is 3.0x the stable baseline (1.50% vs 0.50%); threshold: 2.0x"],
                ShouldAutoRollback = true
            }));

        // Act
        var result = await evaluator.EvaluateAsync(deployment);

        // Assert
        result.IsHealthy.Should().BeFalse();
        result.Violations.Should().ContainSingle()
            .Which.Should().Contain("Canary error rate is 3.0x the stable baseline (1.50% vs 0.50%); threshold: 2.0x");
        result.ShouldAutoRollback.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_LatencyDegradationExceeded_ShouldBeUnhealthy()
    {
        // Arrange
        var deployment = new CanaryDeployment
        {
            ProjectName = "test-project",
            StableVersion = "v1.0.0",
            CanaryVersion = "v1.1.0",
            TargetEnvironment = Environment.Production
        };

        // Stable has 500ms P95 latency, canary has 650ms (30% degradation)
        var stableMetrics = new CanaryMetrics
        {
            P95LatencyMs = 500,
            RequestCount = 1000
        };

        var canaryMetrics = new CanaryMetrics
        {
            P95LatencyMs = 650, // 30% slower than stable
            P99LatencyMs = 1200,
            RequestCount = 500
        };

        // Create a mock evaluator that returns our test metrics
        var evaluator = Substitute.For<ICanaryHealthEvaluator>();
        evaluator.EvaluateAsync(deployment, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CanaryEvaluationResult
            {
                IsHealthy = false,
                Reason = "P95 latency degraded 30.0% vs stable baseline (650ms vs 500ms); threshold: 20.0%",
                StableMetrics = stableMetrics,
                CanaryMetrics = canaryMetrics,
                Violations = ["P95 latency degraded 30.0% vs stable baseline (650ms vs 500ms); threshold: 20.0%"],
                ShouldAutoRollback = true
            }));

        // Act
        var result = await evaluator.EvaluateAsync(deployment);

        // Assert
        result.IsHealthy.Should().BeFalse();
        result.Violations.Should().ContainSingle()
            .Which.Should().Contain("P95 latency degraded 30.0% vs stable baseline (650ms vs 500ms); threshold: 20.0%");
        result.ShouldAutoRollback.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_BoundaryValues_ExactlyAtThreshold_ShouldBeHealthy()
    {
        // Arrange
        var deployment = new CanaryDeployment
        {
            ProjectName = "test-project",
            StableVersion = "v1.0.0",
            CanaryVersion = "v1.1.0",
            TargetEnvironment = Environment.Production
        };

        // Test boundary values exactly at thresholds
        var stableMetrics = new CanaryMetrics
        {
            ErrorRatePercent = 0.0,
            P95LatencyMs = 0,
            RequestCount = 1000
        };

        var canaryMetrics = new CanaryMetrics
        {
            ErrorRatePercent = 1.0, // Exactly at threshold
            P95LatencyMs = 1000,   // Exactly at threshold
            P99LatencyMs = 2000,   // Exactly at threshold
            RequestCount = 500
        };

        // Create a mock evaluator that returns our test metrics
        var evaluator = Substitute.For<ICanaryHealthEvaluator>();
        evaluator.EvaluateAsync(deployment, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CanaryEvaluationResult
            {
                IsHealthy = true,
                Reason = "All metrics within acceptable thresholds",
                StableMetrics = stableMetrics,
                CanaryMetrics = canaryMetrics,
                Violations = [],
                ShouldAutoRollback = false
            }));

        // Act
        var result = await evaluator.EvaluateAsync(deployment);

        // Assert - values exactly at threshold should be healthy (not exceeding)
        result.IsHealthy.Should().BeTrue();
        result.Violations.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluateAsync_EmptyMetrics_ShouldBeHealthy()
    {
        // Arrange
        var deployment = new CanaryDeployment
        {
            ProjectName = "test-project",
            StableVersion = "v1.0.0",
            CanaryVersion = "v1.1.0",
            TargetEnvironment = Environment.Production
        };

        // Empty/zero metrics - should be healthy as there's nothing to violate
        var stableMetrics = new CanaryMetrics();
        var canaryMetrics = new CanaryMetrics();

        // Create a mock evaluator that returns our test metrics
        var evaluator = Substitute.For<ICanaryHealthEvaluator>();
        evaluator.EvaluateAsync(deployment, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CanaryEvaluationResult
            {
                IsHealthy = true,
                Reason = "All metrics within acceptable thresholds",
                StableMetrics = stableMetrics,
                CanaryMetrics = canaryMetrics,
                Violations = [],
                ShouldAutoRollback = false
            }));

        // Act
        var result = await evaluator.EvaluateAsync(deployment);

        // Assert
        result.IsHealthy.Should().BeTrue();
        result.Reason.Should().Be("All metrics within acceptable thresholds");
        result.Violations.Should().BeEmpty();
        result.StableMetrics.ErrorRatePercent.Should().Be(0);
        result.StableMetrics.P95LatencyMs.Should().Be(0);
        result.CanaryMetrics.ErrorRatePercent.Should().Be(0);
        result.CanaryMetrics.P95LatencyMs.Should().Be(0);
    }

    [Fact]
    public async Task EvaluateAsync_MultipleViolations_ShouldReportAll()
    {
        // Arrange
        var deployment = new CanaryDeployment
        {
            ProjectName = "test-project",
            StableVersion = "v1.0.0",
            CanaryVersion = "v1.1.0",
            TargetEnvironment = Environment.Production
        };

        // Multiple violations: error rate, P95 latency, P99 latency
        var stableMetrics = new CanaryMetrics
        {
            ErrorRatePercent = 0.1,
            P95LatencyMs = 400,
            RequestCount = 1000
        };

        var canaryMetrics = new CanaryMetrics
        {
            ErrorRatePercent = 2.0, // Above threshold
            P95LatencyMs = 1500,   // Above threshold
            P99LatencyMs = 2500,   // Above threshold
            RequestCount = 100
        };

        // Create a mock evaluator that returns our test metrics
        var evaluator = Substitute.For<ICanaryHealthEvaluator>();
        evaluator.EvaluateAsync(deployment, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CanaryEvaluationResult
            {
                IsHealthy = false,
                Reason = "Error rate 2.00% exceeds absolute threshold of 1.00%; P95 latency 1500ms exceeds threshold of 1000ms; P99 latency 2500ms exceeds threshold of 2000ms",
                StableMetrics = stableMetrics,
                CanaryMetrics = canaryMetrics,
                Violations = [
                    "Error rate 2.00% exceeds absolute threshold of 1.00%",
                    "P95 latency 1500ms exceeds threshold of 1000ms",
                    "P99 latency 2500ms exceeds threshold of 2000ms"
                ],
                ShouldAutoRollback = true
            }));

        // Act
        var result = await evaluator.EvaluateAsync(deployment);

        // Assert
        result.IsHealthy.Should().BeFalse();
        result.Violations.Should().HaveCount(3);
        result.Violations.Should().Contain(v => v.Contains("Error rate 2.00% exceeds absolute threshold"));
        result.Violations.Should().Contain(v => v.Contains("P95 latency 1500ms exceeds threshold"));
        result.Violations.Should().Contain(v => v.Contains("P99 latency 2500ms exceeds threshold"));
        result.ShouldAutoRollback.Should().BeTrue();
    }

    [Fact]
    public async Task CollectMetricsAsync_ShouldReturnZeroBaseline()
    {
        // Arrange
        var evaluator = new CanaryHealthEvaluator(_options, _logger);
        var version = "v1.0.0";
        var environment = Environment.Production;

        // Act
        var metrics = await evaluator.CollectMetricsAsync(version, environment);

        // Assert
        metrics.Should().NotBeNull();
        metrics.ErrorRatePercent.Should().Be(0);
        metrics.P95LatencyMs.Should().Be(0);
        metrics.P99LatencyMs.Should().Be(0);
        metrics.RequestCount.Should().Be(0);
        metrics.ErrorCount.Should().Be(0);
        metrics.LastUpdatedAt.Should().NotBeNull();
    }
}