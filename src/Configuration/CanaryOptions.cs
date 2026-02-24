// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Configuration;

/// <summary>
/// Top-level configuration for the canary deployment engine.  Bind this class
/// from the <c>CanaryDeployment</c> section of <c>appsettings.json</c> or via
/// environment variables prefixed with <c>CanaryDeployment__</c>.
/// </summary>
public sealed class CanaryOptions
{
    /// <summary>Configuration section key used when binding from <c>appsettings.json</c></summary>
    public const string SectionName = "CanaryDeployment";

    /// <summary>
    /// When <see langword="false"/>, the canary engine is completely disabled and all
    /// <c>ICanaryDeploymentService</c> operations become no-ops or throw.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Automatically trigger a rollback notification via <c>IRollbackService</c> when
    /// health evaluation detects threshold violations.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool AutoRollbackOnFailure { get; set; } = true;

    /// <summary>
    /// Automatically advance to the next rollout step once the soak duration elapses
    /// and health checks pass.  When <see langword="false"/>, callers must invoke
    /// <c>AdvanceRolloutAsync</c> explicitly to move to the next step.
    /// Defaults to <see langword="false"/> (manual advancement).
    /// </summary>
    public bool AutoAdvanceOnSuccess { get; set; } = false;

    /// <summary>
    /// Number of equal-sized traffic increments for the <c>Linear</c> strategy.
    /// Must be in the range [2, 20].  Defaults to 5 (20% per step).
    /// </summary>
    public int LinearStepCount { get; set; } = 5;

    /// <summary>
    /// Minimum observation window at each traffic percentage level before the step
    /// is eligible to advance.  Short durations reduce safety; longer durations slow rollouts.
    /// Defaults to 10 minutes.
    /// </summary>
    public TimeSpan StepSoakDuration { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Maximum wall-clock duration allowed for an entire canary deployment before it is
    /// considered stale.  Stale deployments may be flagged for manual review or automatic abort.
    /// Defaults to 4 hours.
    /// </summary>
    public TimeSpan MaxDeploymentDuration { get; set; } = TimeSpan.FromHours(4);

    /// <summary>
    /// Health metric thresholds used during canary evaluation.  Violations of any single
    /// threshold count as an unhealthy evaluation and can trigger automatic rollback.
    /// </summary>
    public CanaryThresholds Thresholds { get; set; } = new();

    /// <summary>
    /// Notification priority assigned to canary lifecycle events (start, step advance,
    /// promote, abort).  High priority ensures these alerts surface above routine notifications.
    /// Defaults to <see cref="NotificationPriority.High"/>.
    /// </summary>
    public NotificationPriority AlertPriority { get; set; } = NotificationPriority.High;
}

/// <summary>
/// Threshold definitions used by the canary health evaluator.  A canary that exceeds any
/// single threshold produces a violation entry; sufficient violations trigger an automatic
/// rollback when <see cref="CanaryOptions.AutoRollbackOnFailure"/> is enabled.
/// </summary>
public sealed class CanaryThresholds
{
    /// <summary>
    /// Maximum acceptable error rate for the canary, expressed as a percentage of
    /// total requests in the range [0, 100].  Defaults to 1.0% (1 error per 100 requests).
    /// </summary>
    public double MaxErrorRatePercent { get; set; } = 1.0;

    /// <summary>
    /// Maximum acceptable 95th-percentile response latency in milliseconds.
    /// Defaults to 1,000 ms (1 second).
    /// </summary>
    public double MaxP95LatencyMs { get; set; } = 1_000;

    /// <summary>
    /// Maximum acceptable 99th-percentile response latency in milliseconds.
    /// Defaults to 2,000 ms (2 seconds).
    /// </summary>
    public double MaxP99LatencyMs { get; set; } = 2_000;

    /// <summary>
    /// Maximum ratio of canary error rate to stable baseline error rate before a violation
    /// is raised.  A value of 2.0 means the canary may have at most twice the stable error rate.
    /// Only applied when the stable version has observable traffic.  Defaults to 2.0.
    /// </summary>
    public double ErrorRateMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Maximum acceptable P95 latency degradation relative to the stable baseline,
    /// expressed as a percentage.  For example, 20.0 means the canary may be at most 20% slower.
    /// Only applied when the stable baseline has a measurable P95 latency.  Defaults to 20.0%.
    /// </summary>
    public double LatencyDegradationPercent { get; set; } = 20.0;
}
