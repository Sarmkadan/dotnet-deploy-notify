#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Configuration;
using DotNetDeployNotify.Core.Models;
using Microsoft.Extensions.Options;

namespace DotNetDeployNotify.Canary;

/// <summary>
/// Produces per-request routing decisions and generates strategy-specific rollout plans
/// based on the current <see cref="CanaryOptions"/> configuration.
/// </summary>
public sealed class TrafficSplitter : ITrafficSplitter
{
    private readonly CanaryOptions _options;
    private readonly ILogger<TrafficSplitter> _logger;

    /// <summary>Initialises the splitter with options and a logger</summary>
    public TrafficSplitter(IOptions<CanaryOptions> options, ILogger<TrafficSplitter> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public TrafficSplit ComputeNextSplit(CanaryDeployment deployment)
    {
        var activeStep = deployment.RolloutPlan
            .FirstOrDefault(s => s.Status == RolloutStepStatus.InProgress);

        if (activeStep is null)
        {
            _logger.LogDebug(
                "No active rollout step for {Project} — returning current split {Split}",
                deployment.ProjectName,
                deployment.CurrentSplit);

            return deployment.CurrentSplit;
        }

        var split = TrafficSplit.FromCanaryPercent(activeStep.CanaryPercent);

        _logger.LogDebug(
            "Computed split for {Project} step {Step}/{Total}: {Split}",
            deployment.ProjectName,
            activeStep.StepNumber,
            deployment.RolloutPlan.Count,
            split);

        return split;
    }

    /// <inheritdoc />
    public bool ShouldRouteToCanary(TrafficSplit split)
    {
        if (split.CanaryPercent <= 0) return false;
        if (split.CanaryPercent >= 100) return true;

        return Random.Shared.NextDouble() * 100 < split.CanaryPercent;
    }

    /// <inheritdoc />
    public List<CanaryRolloutStep> GenerateRolloutPlan(CanaryStrategy strategy)
    {
        var plan = strategy switch
        {
            CanaryStrategy.Linear      => GenerateLinearPlan(),
            CanaryStrategy.Exponential => GenerateExponentialPlan(),
            CanaryStrategy.BlueGreen   => GenerateBlueGreenPlan(),
            CanaryStrategy.Shadow      => GenerateShadowPlan(),
            _                          => GenerateLinearPlan()
        };

        _logger.LogDebug(
            "Generated {Strategy} rollout plan: {Steps} step(s), soak {Soak}",
            strategy,
            plan.Count,
            _options.StepSoakDuration);

        return plan;
    }

    // -------------------------------------------------------------------------
    // Strategy plan generators
    // -------------------------------------------------------------------------

    private List<CanaryRolloutStep> GenerateLinearPlan()
    {
        var stepCount = Math.Clamp(_options.LinearStepCount, 2, 20);
        var stepPercent = 100.0 / stepCount;

        return Enumerable.Range(1, stepCount)
            .Select(i => new CanaryRolloutStep
            {
                StepNumber    = i,
                CanaryPercent = Math.Min(100.0, Math.Round(stepPercent * i, 1)),
                SoakDuration  = _options.StepSoakDuration
            })
            .ToList();
    }

    private List<CanaryRolloutStep> GenerateExponentialPlan()
    {
        double[] percentages = [1, 2, 5, 10, 25, 50, 100];

        return percentages
            .Select((p, i) => new CanaryRolloutStep
            {
                StepNumber    = i + 1,
                CanaryPercent = p,
                // Soak duration increases linearly with step index to allow more observation at higher traffic
                SoakDuration  = _options.StepSoakDuration * (i + 1)
            })
            .ToList();
    }

    private List<CanaryRolloutStep> GenerateBlueGreenPlan() =>
    [
        new() { StepNumber = 1, CanaryPercent = 50,  SoakDuration = _options.StepSoakDuration },
        new() { StepNumber = 2, CanaryPercent = 100, SoakDuration = _options.StepSoakDuration }
    ];

    private List<CanaryRolloutStep> GenerateShadowPlan() =>
    [
        // Step 1: mirror-only — canary handles 0% production traffic while observing request shapes
        new() { StepNumber = 1, CanaryPercent = 0,   SoakDuration = _options.StepSoakDuration },
        new() { StepNumber = 2, CanaryPercent = 5,   SoakDuration = _options.StepSoakDuration },
        new() { StepNumber = 3, CanaryPercent = 25,  SoakDuration = _options.StepSoakDuration * 2 },
        new() { StepNumber = 4, CanaryPercent = 100, SoakDuration = _options.StepSoakDuration }
    ];
}

/// <summary>
/// Evaluates canary health by comparing live metrics against configured thresholds
/// and the stable version baseline.
/// </summary>
public sealed class CanaryHealthEvaluator : ICanaryHealthEvaluator
{
    private readonly CanaryOptions _options;
    private readonly ILogger<CanaryHealthEvaluator> _logger;

    /// <summary>Initialises the evaluator with options and a logger</summary>
    public CanaryHealthEvaluator(IOptions<CanaryOptions> options, ILogger<CanaryHealthEvaluator> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CanaryEvaluationResult> EvaluateAsync(
        CanaryDeployment deployment,
        CancellationToken cancellationToken = default)
    {
        var stableTask = CollectMetricsAsync(deployment.StableVersion, deployment.TargetEnvironment, cancellationToken);
        var canaryTask = CollectMetricsAsync(deployment.CanaryVersion, deployment.TargetEnvironment, cancellationToken);

        await Task.WhenAll(stableTask, canaryTask).ConfigureAwait(false);

        var stableMetrics = stableTask.Result;
        var canaryMetrics = canaryTask.Result;

        var violations = BuildViolationList(stableMetrics, canaryMetrics);
        var isHealthy = violations.Count == 0;

        var reason = isHealthy
            ? "All metrics within acceptable thresholds"
            : string.Join("; ", violations);

        _logger.LogInformation(
            "Health evaluation for {Project} v{Canary} on step {Step}: {Verdict} — {Reason}",
            deployment.ProjectName,
            deployment.CanaryVersion,
            deployment.ActiveStep?.StepNumber ?? 0,
            isHealthy ? "HEALTHY" : "UNHEALTHY",
            reason);

        return new CanaryEvaluationResult
        {
            IsHealthy        = isHealthy,
            Reason           = reason,
            StableMetrics    = stableMetrics,
            CanaryMetrics    = canaryMetrics,
            Violations       = violations,
            ShouldAutoRollback = !isHealthy && _options.AutoRollbackOnFailure
        };
    }

    /// <inheritdoc />
    public Task<CanaryMetrics> CollectMetricsAsync(
        string version,
        Environment environment,
        CancellationToken cancellationToken = default)
    {
        // Pluggable integration point: replace with Prometheus, Datadog, CloudWatch,
        // Application Insights, or any other observability back-end.
        // Returns a zero-baseline snapshot appropriate for integration testing.
        return Task.FromResult(new CanaryMetrics
        {
            ErrorRatePercent = 0,
            P95LatencyMs     = 0,
            P99LatencyMs     = 0,
            RequestCount     = 0,
            ErrorCount       = 0,
            LastUpdatedAt    = DateTime.UtcNow
        });
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private List<string> BuildViolationList(CanaryMetrics stable, CanaryMetrics canary)
    {
        var t = _options.Thresholds;
        var violations = new List<string>();

        if (canary.ErrorRatePercent > t.MaxErrorRatePercent)
            violations.Add(
                $"Error rate {canary.ErrorRatePercent:F2}% exceeds absolute threshold of {t.MaxErrorRatePercent}%");

        if (canary.P95LatencyMs > t.MaxP95LatencyMs)
            violations.Add(
                $"P95 latency {canary.P95LatencyMs:F0}ms exceeds threshold of {t.MaxP95LatencyMs:F0}ms");

        if (canary.P99LatencyMs > t.MaxP99LatencyMs)
            violations.Add(
                $"P99 latency {canary.P99LatencyMs:F0}ms exceeds threshold of {t.MaxP99LatencyMs:F0}ms");

        // Baseline-relative checks — only meaningful when stable has observable traffic
        if (stable.RequestCount > 0)
        {
            if (stable.ErrorRatePercent > 0)
            {
                var errorMultiplier = canary.ErrorRatePercent / stable.ErrorRatePercent;
                if (errorMultiplier > t.ErrorRateMultiplier)
                    violations.Add(
                        $"Canary error rate is {errorMultiplier:F1}x the stable baseline " +
                        $"({canary.ErrorRatePercent:F2}% vs {stable.ErrorRatePercent:F2}%); " +
                        $"threshold: {t.ErrorRateMultiplier:F1}x");
            }

            if (stable.P95LatencyMs > 0)
            {
                var latencyDelta = (canary.P95LatencyMs - stable.P95LatencyMs) / stable.P95LatencyMs * 100;
                if (latencyDelta > t.LatencyDegradationPercent)
                    violations.Add(
                        $"P95 latency degraded {latencyDelta:F1}% vs stable baseline " +
                        $"({canary.P95LatencyMs:F0}ms vs {stable.P95LatencyMs:F0}ms); " +
                        $"threshold: {t.LatencyDegradationPercent:F1}%");
            }
        }

        return violations;
    }
}
