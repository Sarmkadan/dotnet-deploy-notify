#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Lifecycle status of a canary deployment operation
/// </summary>
public enum CanaryStatus
{
    /// <summary>Deployment has been created but the first step has not yet started</summary>
    Pending = 0,

    /// <summary>Canary is actively receiving a split percentage of production traffic</summary>
    Active = 1,

    /// <summary>Rollout temporarily suspended, awaiting manual approval to continue</summary>
    Paused = 2,

    /// <summary>Automatic rollback triggered due to health threshold violations</summary>
    RollingBack = 3,

    /// <summary>Canary successfully promoted to receive 100% of production traffic</summary>
    Promoted = 4,

    /// <summary>Deployment was manually aborted or failed to complete rollback</summary>
    Aborted = 5
}

/// <summary>
/// Traffic-splitting algorithm used to progress the canary rollout
/// </summary>
public enum CanaryStrategy
{
    /// <summary>Equal percentage increments at each step (e.g. 20% → 40% → 60% → 80% → 100%)</summary>
    Linear = 0,

    /// <summary>Exponential progression with conservative early steps (1% → 2% → 5% → 10% → 25% → 50% → 100%)</summary>
    Exponential = 1,

    /// <summary>Immediate 50/50 split followed by full promotion — suitable for low-risk changes</summary>
    BlueGreen = 2,

    /// <summary>Mirrored traffic with no initial canary production exposure, then gradual ramp-up</summary>
    Shadow = 3
}

/// <summary>
/// Progress state of a single step within the rollout plan
/// </summary>
public enum RolloutStepStatus
{
    /// <summary>Step is scheduled but has not yet started soaking</summary>
    Pending = 0,

    /// <summary>Step is active; the canary is soaking at this traffic level</summary>
    InProgress = 1,

    /// <summary>Step completed successfully; soak duration elapsed with healthy metrics</summary>
    Completed = 2,

    /// <summary>Step was bypassed due to a promote or abort action</summary>
    Skipped = 3,

    /// <summary>Step failed health checks and triggered an automatic rollback</summary>
    Failed = 4
}

/// <summary>
/// Represents a canary deployment with traffic-splitting configuration and full lifecycle tracking
/// </summary>
public sealed class CanaryDeployment
{
    /// <summary>Unique identifier for this canary deployment</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Project or application being deployed</summary>
    public required string ProjectName { get; init; }

    /// <summary>Currently stable version already serving production traffic</summary>
    public required string StableVersion { get; init; }

    /// <summary>Candidate version receiving the split canary traffic</summary>
    public required string CanaryVersion { get; init; }

    /// <summary>Target environment where the rollout is executing</summary>
    public required Environment TargetEnvironment { get; init; }

    /// <summary>Current lifecycle status of this deployment</summary>
    public CanaryStatus Status { get; set; } = CanaryStatus.Pending;

    /// <summary>Traffic-splitting algorithm applied to this rollout</summary>
    public CanaryStrategy Strategy { get; init; } = CanaryStrategy.Linear;

    /// <summary>Current percentage distribution of traffic between stable and canary versions</summary>
    public TrafficSplit CurrentSplit { get; set; } = TrafficSplit.Initial;

    /// <summary>Ordered sequence of traffic-shift steps generated from the chosen strategy</summary>
    public List<CanaryRolloutStep> RolloutPlan { get; init; } = [];

    /// <summary>Latest health metrics snapshot for the stable version</summary>
    public CanaryMetrics StableMetrics { get; set; } = new();

    /// <summary>Latest health metrics snapshot for the canary version</summary>
    public CanaryMetrics CanaryMetrics { get; set; } = new();

    /// <summary>Channels that receive lifecycle transition notifications</summary>
    public List<NotificationChannel> NotificationChannels { get; init; } = [];

    /// <summary>Priority assigned to all lifecycle notifications for this deployment</summary>
    public NotificationPriority Priority { get; init; } = NotificationPriority.High;

    /// <summary>Identity of the person or system that initiated the deployment</summary>
    public string InitiatedBy { get; init; } = string.Empty;

    /// <summary>Git branch from which the canary version was built</summary>
    public string BranchName { get; init; } = string.Empty;

    /// <summary>Git commit hash of the canary build</summary>
    public string CommitHash { get; init; } = string.Empty;

    /// <summary>Link to the CI/CD job that produced the canary artifact</summary>
    public string BuildUrl { get; init; } = string.Empty;

    /// <summary>Human-readable reason provided when aborting the deployment</summary>
    public string? AbortReason { get; set; }

    /// <summary>UTC timestamp when this deployment was created</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>UTC timestamp when the canary was successfully promoted to full traffic</summary>
    public DateTime? PromotedAt { get; set; }

    /// <summary>UTC timestamp when the deployment was aborted or failed</summary>
    public DateTime? AbortedAt { get; set; }

    /// <summary>Arbitrary key-value metadata for tracing and external integrations</summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>Returns the currently soaking rollout step, or <see langword="null"/> if no step is active</summary>
    public CanaryRolloutStep? ActiveStep =>
        RolloutPlan.FirstOrDefault(s => s.Status == RolloutStepStatus.InProgress);

    /// <summary>Returns the next pending rollout step, or <see langword="null"/> if the plan is exhausted</summary>
    public CanaryRolloutStep? NextStep =>
        RolloutPlan.FirstOrDefault(s => s.Status == RolloutStepStatus.Pending);

    /// <summary>Returns <see langword="true"/> when every rollout step is completed or skipped</summary>
    public bool IsRolloutComplete =>
        RolloutPlan.Count > 0 &&
        RolloutPlan.All(s => s.Status is RolloutStepStatus.Completed or RolloutStepStatus.Skipped);

    /// <summary>Returns <see langword="true"/> when the deployment is in a terminal state and cannot be advanced</summary>
    public bool IsTerminal =>
        Status is CanaryStatus.Promoted or CanaryStatus.Aborted;

    /// <summary>Overall rollout progress expressed as a percentage of completed steps (0–100)</summary>
    public double ProgressPercent =>
        RolloutPlan.Count == 0 ? 0
        : RolloutPlan.Count(s => s.Status == RolloutStepStatus.Completed) * 100.0 / RolloutPlan.Count;
}

/// <summary>
/// Immutable snapshot of the traffic distribution between the stable and canary versions
/// </summary>
public record TrafficSplit
{
    /// <summary>Percentage of inbound requests routed to the stable version (0–100)</summary>
    public double StablePercent { get; init; }

    /// <summary>Percentage of inbound requests routed to the canary version (0–100)</summary>
    public double CanaryPercent { get; init; }

    /// <summary>All traffic directed to stable; canary receives no requests</summary>
    public static TrafficSplit Initial => new() { StablePercent = 100, CanaryPercent = 0 };

    /// <summary>Equal 50/50 split between stable and canary</summary>
    public static TrafficSplit Equal => new() { StablePercent = 50, CanaryPercent = 50 };

    /// <summary>All traffic directed to canary; stable receives no requests</summary>
    public static TrafficSplit FullCanary => new() { StablePercent = 0, CanaryPercent = 100 };

    /// <summary>
    /// Creates a split from the desired canary traffic percentage; stable receives the remainder
    /// </summary>
    /// <param name="canaryPercent">Target canary percentage in the range [0, 100]</param>
    public static TrafficSplit FromCanaryPercent(double canaryPercent) =>
        new()
        {
            CanaryPercent = Math.Round(Math.Clamp(canaryPercent, 0, 100), 2),
            StablePercent = Math.Round(Math.Clamp(100 - canaryPercent, 0, 100), 2)
        };

    /// <inheritdoc />
    public override string ToString() =>
        $"Stable {StablePercent:F1}% / Canary {CanaryPercent:F1}%";
}

/// <summary>
/// Health signal snapshot for one deployment version at a point in time
/// </summary>
public sealed class CanaryMetrics
{
    /// <summary>Percentage of requests that resulted in an error response (4xx/5xx), range 0–100</summary>
    public double ErrorRatePercent { get; set; }

    /// <summary>95th-percentile response latency in milliseconds</summary>
    public double P95LatencyMs { get; set; }

    /// <summary>99th-percentile response latency in milliseconds</summary>
    public double P99LatencyMs { get; set; }

    /// <summary>Total number of requests observed since the current step began</summary>
    public long RequestCount { get; set; }

    /// <summary>Total number of error responses observed since the current step began</summary>
    public long ErrorCount { get; set; }

    /// <summary>UTC timestamp of the most recent metrics collection</summary>
    public DateTime? LastUpdatedAt { get; set; }
}

/// <summary>
/// A discrete traffic-shift step within the rollout plan
/// </summary>
public sealed class CanaryRolloutStep
{
    /// <summary>1-based sequence number of this step within the plan</summary>
    public int StepNumber { get; init; }

    /// <summary>Target canary traffic percentage applied while this step is active (0–100)</summary>
    public double CanaryPercent { get; init; }

    /// <summary>Minimum time to observe the canary at this traffic level before the step is eligible to advance</summary>
    public TimeSpan SoakDuration { get; init; }

    /// <summary>Current progress state of this step</summary>
    public RolloutStepStatus Status { get; set; } = RolloutStepStatus.Pending;

    /// <summary>UTC timestamp when this step began soaking</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>UTC timestamp when this step was marked complete, skipped, or failed</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Returns <see langword="true"/> when the configured soak duration has elapsed since the step started
    /// </summary>
    public bool IsSoakComplete =>
        StartedAt.HasValue && DateTime.UtcNow - StartedAt.Value >= SoakDuration;
}

/// <summary>
/// Result of a periodic health evaluation comparing canary metrics against stable baseline and configured thresholds
/// </summary>
public record CanaryEvaluationResult
{
    /// <summary>Returns <see langword="true"/> when no threshold violations were detected</summary>
    public bool IsHealthy { get; init; }

    /// <summary>Human-readable summary of the evaluation outcome</summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>Metrics snapshot collected from the stable version during this evaluation</summary>
    public CanaryMetrics StableMetrics { get; init; } = new();

    /// <summary>Metrics snapshot collected from the canary version during this evaluation</summary>
    public CanaryMetrics CanaryMetrics { get; init; } = new();

    /// <summary>Individual threshold violation descriptions, empty when the canary is healthy</summary>
    public List<string> Violations { get; init; } = [];

    /// <summary>
    /// When <see langword="true"/>, the engine should trigger an automatic rollback for this deployment
    /// </summary>
    public bool ShouldAutoRollback { get; init; }

    /// <summary>UTC timestamp when this evaluation was performed</summary>
    public DateTime EvaluatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Immutable request DTO for initiating a new canary deployment
/// </summary>
public record CanaryDeploymentRequest
{
    /// <summary>Project or application name</summary>
    public required string ProjectName { get; init; }

    /// <summary>Current stable version already serving 100% of production traffic</summary>
    public required string StableVersion { get; init; }

    /// <summary>New candidate version to roll out as the canary</summary>
    public required string CanaryVersion { get; init; }

    /// <summary>Target environment in which the canary rollout will execute</summary>
    public required Environment TargetEnvironment { get; init; }

    /// <summary>Traffic-splitting strategy to apply for this rollout</summary>
    public CanaryStrategy Strategy { get; init; } = CanaryStrategy.Linear;

    /// <summary>Notification channels that should receive lifecycle alerts</summary>
    public List<NotificationChannel> NotificationChannels { get; init; } = [];

    /// <summary>Priority applied to all lifecycle notifications for this deployment</summary>
    public NotificationPriority Priority { get; init; } = NotificationPriority.High;

    /// <summary>Identity of the user or automation that initiated the deployment</summary>
    public string InitiatedBy { get; init; } = string.Empty;

    /// <summary>Git branch name for the canary version</summary>
    public string BranchName { get; init; } = string.Empty;

    /// <summary>Git commit hash of the canary build artifact</summary>
    public string CommitHash { get; init; } = string.Empty;

    /// <summary>URL of the CI/CD pipeline that produced the canary artifact</summary>
    public string BuildUrl { get; init; } = string.Empty;

    /// <summary>Arbitrary metadata to attach to the deployment for tracing and integrations</summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}
