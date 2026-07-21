#nullable enable

using DotNetDeployNotify.Canary;
using DotNetDeployNotify.Configuration;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace DotNetDeployNotify.Tests.Canary;

public class CanaryDeploymentEngineTests
{
    private readonly IOptions<CanaryOptions> _options;
    private readonly ILogger<CanaryDeploymentEngine> _logger;
    private readonly INotificationService _notificationService;
    private readonly IRollbackService _rollbackService;
    private readonly ITrafficSplitter _trafficSplitter;
    private readonly ICanaryHealthEvaluator _healthEvaluator;

    public CanaryDeploymentEngineTests()
    {
        _options = Options.Create(new CanaryOptions
        {
            Enabled = true,
            AutoRollbackOnFailure = true,
            AutoAdvanceOnSuccess = true, // Enable auto-advance for testing
            LinearStepCount = 3, // 33%, 66%, 100% for easier testing
            StepSoakDuration = TimeSpan.FromSeconds(1),
            Thresholds = new CanaryThresholds
            {
                MaxErrorRatePercent = 1.0,
                MaxP95LatencyMs = 1000,
                MaxP99LatencyMs = 2000,
                ErrorRateMultiplier = 2.0,
                LatencyDegradationPercent = 20.0
            }
        });

        _logger = Substitute.For<ILogger<CanaryDeploymentEngine>>();
        _notificationService = Substitute.For<INotificationService>();
        _rollbackService = Substitute.For<IRollbackService>();
        _trafficSplitter = Substitute.For<ITrafficSplitter>();
        _healthEvaluator = Substitute.For<ICanaryHealthEvaluator>();

        // Setup default traffic splitter behavior
        _trafficSplitter.GenerateRolloutPlan(Arg.Any<CanaryStrategy>())
            .Returns(new List<CanaryRolloutStep>
            {
                new() { StepNumber = 1, CanaryPercent = 33, SoakDuration = TimeSpan.FromSeconds(1) },
                new() { StepNumber = 2, CanaryPercent = 66, SoakDuration = TimeSpan.FromSeconds(1) },
                new() { StepNumber = 3, CanaryPercent = 100, SoakDuration = TimeSpan.FromSeconds(1) }
            });

        // Setup default notification service responses
        _notificationService.CreateNotificationAsync(Arg.Any<DeploymentNotification>())
            .Returns(Task.FromResult("notification-id"));
        _notificationService.SendNotificationAsync(Arg.Any<string>(), Arg.Any<List<NotificationChannel>>())
            .Returns(Task.FromResult(new List<NotificationResult>()));
    }

    private CanaryDeploymentEngine CreateEngine(bool autoAdvanceOnSuccess = true)
    {
        var options = Options.Create(new CanaryOptions
        {
            Enabled = true,
            AutoRollbackOnFailure = true,
            AutoAdvanceOnSuccess = autoAdvanceOnSuccess,
            LinearStepCount = 3,
            StepSoakDuration = TimeSpan.FromSeconds(1),
            Thresholds = new CanaryThresholds
            {
                MaxErrorRatePercent = 1.0,
                MaxP95LatencyMs = 1000,
                MaxP99LatencyMs = 2000,
                ErrorRateMultiplier = 2.0,
                LatencyDegradationPercent = 20.0
            }
        });

        return new CanaryDeploymentEngine(
            _notificationService,
            _rollbackService,
            _trafficSplitter,
            _healthEvaluator,
            options,
            _logger);
    }

    private static List<NotificationChannel> CreateChannels()
    {
        return new List<NotificationChannel> { NotificationChannel.Slack };
    }

    [Fact]
    public async Task StartCanaryAsync_CreatesDeployment_WithInitialStep()
    {
        // Arrange
        var engine = CreateEngine();
        var request = new CanaryDeploymentRequest
        {
            ProjectName = "test-project",
            StableVersion = "v1.0.0",
            CanaryVersion = "v1.1.0",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Production,
            Strategy = CanaryStrategy.Linear,
            NotificationChannels = CreateChannels()
        };

        // Act
        var deployment = await engine.StartCanaryAsync(request);

        // Assert
        deployment.Should().NotBeNull();
        deployment.ProjectName.Should().Be("test-project");
        deployment.StableVersion.Should().Be("v1.0.0");
        deployment.CanaryVersion.Should().Be("v1.1.0");
        deployment.Status.Should().Be(CanaryStatus.Active);
        deployment.CurrentSplit.Should().Be(TrafficSplit.FromCanaryPercent(33)); // First step
        deployment.RolloutPlan.Count.Should().Be(3);
        deployment.ActiveStep.Should().NotBeNull();
        deployment.ActiveStep!.StepNumber.Should().Be(1);
        deployment.ActiveStep.Status.Should().Be(RolloutStepStatus.InProgress);
    }

    [Fact]
    public async Task AdvanceRolloutAsync_ProgressesThroughSteps_WhenEvaluationsAreHealthy()
    {
        // Arrange
        var engine = CreateEngine();
        var request = new CanaryDeploymentRequest
        {
            ProjectName = "test-project",
            StableVersion = "v1.0.0",
            CanaryVersion = "v1.1.0",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Production,
            Strategy = CanaryStrategy.Linear,
            NotificationChannels = CreateChannels()
        };

        var deployment = await engine.StartCanaryAsync(request);
        var deploymentId = deployment.Id;

        // Mock healthy evaluations
        _healthEvaluator.EvaluateAsync(Arg.Any<CanaryDeployment>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CanaryEvaluationResult
            {
                IsHealthy = true,
                Reason = "All metrics within acceptable thresholds",
                StableMetrics = new CanaryMetrics(),
                CanaryMetrics = new CanaryMetrics(),
                Violations = new List<string>(),
                ShouldAutoRollback = false
            }));

        // Act - Advance to step 2
        var deploymentAfterStep1 = await engine.AdvanceRolloutAsync(deploymentId);

        // Assert - Step 1 completed, step 2 started
        deploymentAfterStep1.Status.Should().Be(CanaryStatus.Active);
        deploymentAfterStep1.CurrentSplit.Should().Be(TrafficSplit.FromCanaryPercent(66)); // Second step
        deploymentAfterStep1.ActiveStep.Should().NotBeNull();
        deploymentAfterStep1.ActiveStep!.StepNumber.Should().Be(2);
        deploymentAfterStep1.ActiveStep.Status.Should().Be(RolloutStepStatus.InProgress);

        // Verify step 1 is completed
        var step1 = deploymentAfterStep1.RolloutPlan.First(s => s.StepNumber == 1);
        step1.Status.Should().Be(RolloutStepStatus.Completed);

        // Act - Advance to step 3
        var deploymentAfterStep2 = await engine.AdvanceRolloutAsync(deploymentId);

        // Assert - Step 2 completed, step 3 started
        deploymentAfterStep2.Status.Should().Be(CanaryStatus.Active);
        deploymentAfterStep2.CurrentSplit.Should().Be(TrafficSplit.FromCanaryPercent(100)); // Third step
        deploymentAfterStep2.ActiveStep.Should().NotBeNull();
        deploymentAfterStep2.ActiveStep!.StepNumber.Should().Be(3);
        deploymentAfterStep2.ActiveStep.Status.Should().Be(RolloutStepStatus.InProgress);

        // Verify step 2 is completed
        var step2 = deploymentAfterStep2.RolloutPlan.First(s => s.StepNumber == 2);
        step2.Status.Should().Be(RolloutStepStatus.Completed);
    }

    [Fact]
    public async Task EvaluateHealthAsync_TriggersAbort_WhenEvaluationIsUnhealthy()
    {
        // Arrange
        var engine = CreateEngine();
        var request = new CanaryDeploymentRequest
        {
            ProjectName = "test-project",
            StableVersion = "v1.0.0",
            CanaryVersion = "v1.1.0",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Production,
            Strategy = CanaryStrategy.Linear,
            NotificationChannels = CreateChannels()
        };

        var deployment = await engine.StartCanaryAsync(request);
        var deploymentId = deployment.Id;

        // Mock unhealthy evaluation that should trigger auto-rollback
        _healthEvaluator.EvaluateAsync(Arg.Any<CanaryDeployment>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new CanaryEvaluationResult
            {
                IsHealthy = false,
                Reason = "Error rate 2.00% exceeds absolute threshold of 1.00%",
                StableMetrics = new CanaryMetrics { ErrorRatePercent = 0.5 },
                CanaryMetrics = new CanaryMetrics { ErrorRatePercent = 2.0 },
                Violations = new List<string> { "Error rate 2.00% exceeds absolute threshold of 1.00%" },
                ShouldAutoRollback = true
            }));

        // Act
        var result = await engine.EvaluateHealthAsync(deploymentId);

        // Assert
        result.IsHealthy.Should().BeFalse();
        result.ShouldAutoRollback.Should().BeTrue();
        result.Violations.Should().ContainSingle()
            .Which.Should().Contain("Error rate 2.00% exceeds absolute threshold of 1.00%");

        // Verify deployment was aborted
        var abortedDeployment = await engine.GetDeploymentAsync(deploymentId);
        abortedDeployment.Status.Should().Be(CanaryStatus.Aborted);
        abortedDeployment.AbortReason.Should().Be("Automatic rollback: Error rate 2.00% exceeds absolute threshold of 1.00%");
        abortedDeployment.CurrentSplit.Should().Be(TrafficSplit.Initial); // Traffic returned to stable
    }

    [Fact]
    public async Task AdvanceRolloutAsync_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var engine = CreateEngine(autoAdvanceOnSuccess: false); // Disable auto-advance to test manual progression
        var request = new CanaryDeploymentRequest
        {
            ProjectName = "test-project",
            StableVersion = "v1.0.0",
            CanaryVersion = "v1.1.0",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Production,
            Strategy = CanaryStrategy.Linear,
            NotificationChannels = CreateChannels()
        };

        var deployment = await engine.StartCanaryAsync(request);
        var deploymentId = deployment.Id;

        // Create a cancellation token (not cancelled)
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act - Advance with valid token
        var deploymentAfterAttempt = await engine.AdvanceRolloutAsync(deploymentId, token);

        // Assert - Deployment should progress to next step
        deploymentAfterAttempt.Status.Should().Be(CanaryStatus.Active);
        deploymentAfterAttempt.CurrentSplit.Should().Be(TrafficSplit.FromCanaryPercent(66)); // Second step
        deploymentAfterAttempt.ActiveStep.Should().NotBeNull();
        deploymentAfterAttempt.ActiveStep!.StepNumber.Should().Be(2);
        deploymentAfterAttempt.ActiveStep.Status.Should().Be(RolloutStepStatus.InProgress);
    }

    [Fact]
    public async Task PromoteAsync_SkipsRemainingSteps_WhenCalled()
    {
        // Arrange
        var engine = CreateEngine(autoAdvanceOnSuccess: false); // Manual advance mode
        var request = new CanaryDeploymentRequest
        {
            ProjectName = "test-project",
            StableVersion = "v1.0.0",
            CanaryVersion = "v1.1.0",
            TargetEnvironment = global::DotNetDeployNotify.Core.Environment.Production,
            Strategy = CanaryStrategy.Linear,
            NotificationChannels = CreateChannels()
        };

        var deployment = await engine.StartCanaryAsync(request);
        var deploymentId = deployment.Id;

        // Advance to step 2 manually
        await engine.AdvanceRolloutAsync(deploymentId);

        // Act - Promote from step 2
        var promotedDeployment = await engine.PromoteAsync(deploymentId);

        // Assert
        promotedDeployment.Status.Should().Be(CanaryStatus.Promoted);
        promotedDeployment.CurrentSplit.Should().Be(TrafficSplit.FullCanary); // 100% canary
        promotedDeployment.PromotedAt.Should().NotBeNull();

        // Verify all remaining steps are skipped
        var step3 = promotedDeployment.RolloutPlan.First(s => s.StepNumber == 3);
        step3.Status.Should().Be(RolloutStepStatus.Skipped);

        // Verify step 1 is completed, step 2 is completed (active step completed before promotion)
        var step1 = promotedDeployment.RolloutPlan.First(s => s.StepNumber == 1);
        step1.Status.Should().Be(RolloutStepStatus.Completed);

        var step2 = promotedDeployment.RolloutPlan.First(s => s.StepNumber == 2);
        step2.Status.Should().Be(RolloutStepStatus.Completed);
        step2.CompletedAt.Should().NotBeNull();
    }
}