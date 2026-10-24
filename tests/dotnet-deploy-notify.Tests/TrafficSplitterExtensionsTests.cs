#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Canary;
using DotNetDeployNotify.Configuration;
using DotNetDeployNotify.Core.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Environment = DotNetDeployNotify.Core.Environment;
using Xunit;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Tests for <see cref="TrafficSplitterExtensions"/> deployment factories, rollout gating and percentage helpers.
/// </summary>
public class TrafficSplitterExtensionsTests
{
    private readonly TrafficSplitter _splitter;
    private readonly CanaryHealthEvaluator _evaluator;

    public TrafficSplitterExtensionsTests()
    {
        var options = Options.Create(new CanaryOptions());
        _splitter = new TrafficSplitter(options, Substitute.For<ILogger<TrafficSplitter>>());
        _evaluator = new CanaryHealthEvaluator(options, Substitute.For<ILogger<CanaryHealthEvaluator>>());
    }

    [Fact]
    public void CreateLinearCanaryDeployment_WithValidArguments_BuildsLinearPlanFromOptions()
    {
        // Act
        var deployment = _splitter.CreateLinearCanaryDeployment("Api", "2.0.0", "1.0.0", Environment.Staging);

        // Assert
        deployment.ProjectName.Should().Be("Api");
        deployment.CanaryVersion.Should().Be("2.0.0");
        deployment.StableVersion.Should().Be("1.0.0");
        deployment.TargetEnvironment.Should().Be(Environment.Staging);
        deployment.Strategy.Should().Be(CanaryStrategy.Linear);
        deployment.CurrentSplit.CanaryPercent.Should().Be(0);
        deployment.RolloutPlan.Select(s => s.CanaryPercent).Should().Equal(20, 40, 60, 80, 100);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateLinearCanaryDeployment_WithStepCountBelowOne_ThrowsArgumentOutOfRange(int stepCount)
    {
        // Act
        Action act = () => _splitter.CreateLinearCanaryDeployment("Api", "2.0.0", "1.0.0", Environment.Staging, stepCount);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("stepCount");
    }

    [Fact]
    public void CreateExponentialCanaryDeployment_WithValidArguments_BuildsSevenStepPlanWithGrowingSoak()
    {
        // Act
        var deployment = _splitter.CreateExponentialCanaryDeployment("Api", "2.0.0", "1.0.0", Environment.Staging);

        // Assert
        deployment.Strategy.Should().Be(CanaryStrategy.Exponential);
        deployment.CurrentSplit.CanaryPercent.Should().Be(0);
        deployment.RolloutPlan.Select(s => s.CanaryPercent).Should().Equal(1, 2, 5, 10, 25, 50, 100);
        deployment.RolloutPlan.Select(s => s.SoakDuration).Should().Equal(
            Enumerable.Range(1, 7).Select(i => TimeSpan.FromMinutes(10 * i)));
    }

    [Fact]
    public void CreateBlueGreenCanaryDeployment_WithValidArguments_SplitsFiftyThenFullTraffic()
    {
        // Act
        var deployment = _splitter.CreateBlueGreenCanaryDeployment("Api", "2.0.0", "1.0.0", Environment.Development);

        // Assert
        deployment.Strategy.Should().Be(CanaryStrategy.BlueGreen);
        deployment.CurrentSplit.CanaryPercent.Should().Be(0);
        deployment.RolloutPlan.Select(s => s.CanaryPercent).Should().Equal(50, 100);
    }

    [Fact]
    public async Task CanaryFactories_WithNullArguments_ThrowsArgumentNullException()
    {
        // Arrange
        var deployment = _splitter.CreateLinearCanaryDeployment("Api", "2.0.0", "1.0.0", Environment.Staging);

        Action nullSplitter = () => TrafficSplitterExtensions.CreateLinearCanaryDeployment(
            null!, "Api", "2.0.0", "1.0.0", Environment.Staging);
        Action nullProjectName = () => _splitter.CreateLinearCanaryDeployment(null!, "2.0.0", "1.0.0", Environment.Staging);
        Action nullCanaryVersion = () => _splitter.CreateExponentialCanaryDeployment("Api", null!, "1.0.0", Environment.Staging);
        Action nullStableVersion = () => _splitter.CreateBlueGreenCanaryDeployment("Api", "2.0.0", null!, Environment.Staging);
        Action nullSplit = () => _splitter.GetCanaryPercentageNormalized(null!);
        Func<Task> nullDeployment = () => _splitter.ShouldProceedToNextStepAsync(null!, _evaluator);
        Func<Task> nullEvaluator = () => _splitter.ShouldProceedToNextStepAsync(deployment, null!);

        // Act & Assert
        nullSplitter.Should().Throw<ArgumentNullException>();
        nullProjectName.Should().Throw<ArgumentNullException>();
        nullCanaryVersion.Should().Throw<ArgumentNullException>();
        nullStableVersion.Should().Throw<ArgumentNullException>();
        nullSplit.Should().Throw<ArgumentNullException>();
        await nullDeployment.Should().ThrowAsync<ArgumentNullException>();
        await nullEvaluator.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(15, true)]
    [InlineData(1, false)]
    public async Task ShouldProceedToNextStepAsync_ReturnsWhetherActiveStepSoakElapsed(int minutesSinceStart, bool expected)
    {
        // Arrange - default StepSoakDuration is 10 minutes; the evaluator's stub metrics are healthy
        var deployment = _splitter.CreateLinearCanaryDeployment("Api", "2.0.0", "1.0.0", Environment.Staging);
        var step = deployment.RolloutPlan[0];
        step.Status = RolloutStepStatus.InProgress;
        step.StartedAt = DateTime.UtcNow.AddMinutes(-minutesSinceStart);

        // Act
        var result = await _splitter.ShouldProceedToNextStepAsync(deployment, _evaluator);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task ShouldProceedToNextStepAsync_WithNoActiveStep_ReturnsFalse()
    {
        // Arrange - a fresh plan has only Pending steps; an emptied plan has no steps at all
        var pendingPlan = _splitter.CreateLinearCanaryDeployment("Api", "2.0.0", "1.0.0", Environment.Staging);
        var emptyPlan = _splitter.CreateLinearCanaryDeployment("Api", "2.0.0", "1.0.0", Environment.Staging);
        emptyPlan.RolloutPlan.Clear();

        // Act
        var pendingResult = await _splitter.ShouldProceedToNextStepAsync(pendingPlan, _evaluator);
        var emptyResult = await _splitter.ShouldProceedToNextStepAsync(emptyPlan, _evaluator);

        // Assert
        pendingResult.Should().BeFalse();
        emptyResult.Should().BeFalse();
    }

    [Theory]
    [InlineData(0, 0.0)]
    [InlineData(12.5, 0.125)]
    [InlineData(100, 1.0)]
    public void GetCanaryPercentageNormalized_ReturnsFractionOfOneHundred(double canaryPercent, double expected)
    {
        // Arrange
        var split = TrafficSplit.FromCanaryPercent(canaryPercent);

        // Act
        var normalized = _splitter.GetCanaryPercentageNormalized(split);

        // Assert
        normalized.Should().BeApproximately(expected, 1e-9);
    }
}
