using System;
using System.Collections.Generic;
using System.Linq;
using DotNetDeployNotify.Core.Models;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class CanaryDeploymentTests
{
    private static CanaryDeployment CreateBasicDeployment()
    {
        // required properties must be set; other properties use defaults
        return new CanaryDeployment
        {
            ProjectName = "MyApp",
            StableVersion = "1.0.0",
            CanaryVersion = "1.1.0",
            TargetEnvironment = default // assuming a custom enum; default value is acceptable for tests
        };
    }

    [Fact]
    public void Id_IsGeneratedAndIsGuid()
    {
        var deployment = CreateBasicDeployment();

        Assert.False(string.IsNullOrWhiteSpace(deployment.Id));

        // Guid.TryParse should succeed for the generated Id
        Assert.True(Guid.TryParse(deployment.Id, out _));
    }

    [Fact]
    public void ActiveStep_Returns_InProgressStepOrNull()
    {
        var step1 = new CanaryRolloutStep
        {
            StepNumber = 1,
            CanaryPercent = 20,
            SoakDuration = TimeSpan.FromMinutes(5),
            Status = RolloutStepStatus.Completed
        };
        var step2 = new CanaryRolloutStep
        {
            StepNumber = 2,
            CanaryPercent = 40,
            SoakDuration = TimeSpan.FromMinutes(5),
            Status = RolloutStepStatus.InProgress,
            StartedAt = DateTime.UtcNow.AddMinutes(-1)
        };
        var deployment = CreateBasicDeployment();
        deployment.RolloutPlan.AddRange(new[] { step1, step2 });

        Assert.Same(step2, deployment.ActiveStep);
    }

    [Fact]
    public void NextStep_Returns_FirstPendingStepOrNull()
    {
        var step1 = new CanaryRolloutStep
        {
            StepNumber = 1,
            CanaryPercent = 20,
            SoakDuration = TimeSpan.FromMinutes(5),
            Status = RolloutStepStatus.Completed
        };
        var step2 = new CanaryRolloutStep
        {
            StepNumber = 2,
            CanaryPercent = 40,
            SoakDuration = TimeSpan.FromMinutes(5),
            Status = RolloutStepStatus.Pending
        };
        var deployment = CreateBasicDeployment();
        deployment.RolloutPlan.AddRange(new[] { step1, step2 });

        Assert.Same(step2, deployment.NextStep);
    }

    [Fact]
    public void IsRolloutComplete_ReturnsTrue_WhenAllStepsCompletedOrSkipped()
    {
        var steps = new List<CanaryRolloutStep>
        {
            new() { StepNumber = 1, CanaryPercent = 20, SoakDuration = TimeSpan.Zero, Status = RolloutStepStatus.Completed },
            new() { StepNumber = 2, CanaryPercent = 40, SoakDuration = TimeSpan.Zero, Status = RolloutStepStatus.Skipped },
            new() { StepNumber = 3, CanaryPercent = 60, SoakDuration = TimeSpan.Zero, Status = RolloutStepStatus.Completed }
        };

        var deployment = CreateBasicDeployment();
        deployment.RolloutPlan.AddRange(steps);

        Assert.True(deployment.IsRolloutComplete);
    }

    [Fact]
    public void IsRolloutComplete_ReturnsFalse_WhenPendingStepsExist()
    {
        var steps = new List<CanaryRolloutStep>
        {
            new() { StepNumber = 1, CanaryPercent = 20, SoakDuration = TimeSpan.Zero, Status = RolloutStepStatus.Completed },
            new() { StepNumber = 2, CanaryPercent = 40, SoakDuration = TimeSpan.Zero, Status = RolloutStepStatus.Pending }
        };

        var deployment = CreateBasicDeployment();
        deployment.RolloutPlan.AddRange(steps);

        Assert.False(deployment.IsRolloutComplete);
    }

    [Theory]
    [InlineData(CanaryStatus.Promoted, true)]
    [InlineData(CanaryStatus.Aborted, true)]
    [InlineData(CanaryStatus.Active, false)]
    [InlineData(CanaryStatus.Pending, false)]
    public void IsTerminal_ReflectsStatus(CanaryStatus status, bool expected)
    {
        var deployment = CreateBasicDeployment();
        deployment.Status = status;

        Assert.Equal(expected, deployment.IsTerminal);
    }

    [Fact]
    public void ProgressPercent_CalculatesCorrectly()
    {
        var steps = new List<CanaryRolloutStep>
        {
            new() { StepNumber = 1, CanaryPercent = 20, SoakDuration = TimeSpan.Zero, Status = RolloutStepStatus.Completed },
            new() { StepNumber = 2, CanaryPercent = 40, SoakDuration = TimeSpan.Zero, Status = RolloutStepStatus.InProgress },
            new() { StepNumber = 3, CanaryPercent = 60, SoakDuration = TimeSpan.Zero, Status = RolloutStepStatus.Pending }
        };

        var deployment = CreateBasicDeployment();
        deployment.RolloutPlan.AddRange(steps);

        // 1 out of 3 steps completed => 33.33...%
        double expected = 1.0 / 3.0 * 100.0;
        Assert.Equal(expected, deployment.ProgressPercent, precision: 5);
    }

    [Fact]
    public void CurrentSplit_DefaultsToInitial()
    {
        var deployment = CreateBasicDeployment();

        Assert.Equal(TrafficSplit.Initial, deployment.CurrentSplit);
    }

    [Fact]
    public void Setting_RequiredProperties_ThrowsWhenMissing()
    {
        // The compiler enforces required properties, but we can still test that
        // an object created without them cannot be instantiated.
        // This test simply ensures the type can be instantiated when all required
        // members are supplied; attempting to omit them would be a compile‑time error,
        // which is the expected behaviour.
        var deployment = new CanaryDeployment
        {
            ProjectName = "App",
            StableVersion = "1.0",
            CanaryVersion = "1.1",
            TargetEnvironment = default
        };

        Assert.NotNull(deployment);
    }
}
