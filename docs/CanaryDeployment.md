# CanaryDeployment

Represents a single canary deployment operation within the `dotnet-deploy-notify` system. This type models the full lifecycle of a controlled, gradual rollout where a new version of a project (the canary) is deployed alongside the existing stable version, with traffic split between them according to a defined strategy. It tracks the deployment’s identity, version metadata, target environment, current status, rollout plan, performance metrics for both versions, notification configuration, and audit information such as who initiated the deployment and when it was promoted or aborted.

## API

### `public string Id`
A unique identifier for this canary deployment instance. This value is assigned at creation and remains immutable for the lifetime of the object. It is used to correlate events, notifications, and metric data with a specific deployment.

### `public required string ProjectName`
The name of the project undergoing the canary deployment. This is a required property that must be supplied at object initialization. It identifies the logical application or service being updated.

### `public required string StableVersion`
The version string of the currently stable, production-serving deployment. This is a required property. It serves as the baseline against which the canary version’s metrics are compared.

### `public required string CanaryVersion`
The version string of the new, candidate deployment being gradually rolled out. This is a required property. It represents the software version under evaluation before full promotion.

### `public required Environment TargetEnvironment`
The deployment environment where the canary rollout is taking place (e.g., staging, production). This is a required property. The `Environment` type encapsulates environment-specific configuration such as endpoints and infrastructure targets.

### `public CanaryStatus Status`
The current phase of the canary deployment lifecycle. The `CanaryStatus` enumeration includes states such as `Pending`, `InProgress`, `Paused`, `Promoted`, `Aborted`, and `Failed`. This property is updated by the deployment orchestration logic as the rollout progresses or encounters termination conditions.

### `public CanaryStrategy Strategy`
Defines the approach used to evaluate the canary against the stable version. The `CanaryStrategy` type specifies the comparison methodology, duration, and success criteria (e.g., error rate thresholds, latency percentiles, or custom metric comparisons) that must be satisfied before promotion can occur.

### `public TrafficSplit CurrentSplit`
The current distribution of traffic between the stable and canary versions, expressed as a `TrafficSplit` object. This typically contains percentage or weight values for each target. The split is adjusted at each rollout step according to the `RolloutPlan`.

### `public List<CanaryRolloutStep> RolloutPlan`
An ordered list of steps defining the traffic-shifting schedule. Each `CanaryRolloutStep` specifies a target traffic split and a duration or gating condition. The deployment engine advances through this plan, updating `CurrentSplit` at each step, until the canary reaches 100% traffic or the deployment is aborted.

### `public CanaryMetrics StableMetrics`
Aggregated performance metrics collected from the stable version during the canary evaluation window. The `CanaryMetrics` type holds counters and gauges such as request count, error rate, average latency, and saturation indicators. These metrics serve as the control group for comparison.

### `public CanaryMetrics CanaryMetrics`
Aggregated performance metrics collected from the canary version over the same evaluation window. The deployment engine compares these against `StableMetrics` using the criteria defined in `Strategy` to determine whether the canary is healthy and eligible for promotion.

### `public List<NotificationChannel> NotificationChannels`
A list of channels through which deployment lifecycle notifications are sent. Each `NotificationChannel` encapsulates a destination (e.g., Slack webhook, email address, Microsoft Teams connector) and channel-specific configuration. Notifications are dispatched at key state transitions and when metric thresholds are breached.

### `public NotificationPriority Priority`
The minimum severity level required for a notification to be sent through the configured channels. The `NotificationPriority` enumeration defines levels such as `Low`, `Normal`, `High`, and `Critical`. Events below this priority are suppressed; events at or above it trigger delivery.

### `public string InitiatedBy`
The identity of the user, service account, or automated process that initiated the canary deployment. This value is recorded at creation for audit trail purposes.

### `public string BranchName`
The source control branch from which the canary version was built. Provides traceability from the deployment back to the code change that triggered it.

### `public string CommitHash`
The full or abbreviated commit hash corresponding to the canary version’s build. Used to correlate the deployment with a specific point in the repository history.

### `public string BuildUrl`
A URL pointing to the CI/CD build that produced the canary artifact. Enables operators to quickly navigate to build logs, test results, and artifact details.

### `public string? AbortReason`
A human-readable explanation for why the canary deployment was aborted, if applicable. This property is `null` when the deployment has not been aborted. It is populated by the aborting actor (manual operator or automated safety gate) at the time of termination.

### `public DateTime CreatedAt`
The timestamp, in UTC, when the canary deployment object was created. This marks the beginning of the deployment lifecycle and is immutable.

### `public DateTime? PromotedAt`
The timestamp, in UTC, when the canary version was fully promoted to stable status, replacing the previous stable version. This property is `null` until promotion completes successfully. If the deployment is aborted or fails, it remains `null`.

## Usage

### Example 1: Creating and Initiating a Basic Canary Deployment

```csharp
var deployment = new CanaryDeployment
{
    ProjectName = "payment-service",
    StableVersion = "2.4.1",
    CanaryVersion = "2.5.0-rc1",
    TargetEnvironment = Environments.Production,
    Strategy = new CanaryStrategy
    {
        EvaluationDuration = TimeSpan.FromMinutes(30),
        ErrorRateThreshold = 0.01, // 1% maximum error rate
        LatencyP99ThresholdMs = 200
    },
    RolloutPlan = new List<CanaryRolloutStep>
    {
        new() { TrafficPercent = 5, Duration = TimeSpan.FromMinutes(10) },
        new() { TrafficPercent = 25, Duration = TimeSpan.FromMinutes(10) },
        new() { TrafficPercent = 100, Duration = TimeSpan.FromMinutes(10) }
    },
    NotificationChannels = new List<NotificationChannel>
    {
        new SlackNotificationChannel { WebhookUrl = "https://hooks.slack.com/services/..." }
    },
    Priority = NotificationPriority.High,
    InitiatedBy = "ci-pipeline",
    BranchName = "feature/new-checkout-flow",
    CommitHash = "a3f8b2c",
    BuildUrl = "https://ci.example.com/builds/9821"
};

// The deployment engine would then persist and begin execution:
// orchestrator.StartDeployment(deployment);
```

### Example 2: Evaluating Metrics and Promoting a Canary

```csharp
// Assume deployment is in progress and metrics have been collected
var deployment = orchestrator.GetDeployment(deploymentId);

bool shouldPromote = deployment.Strategy.Evaluate(
    deployment.StableMetrics,
    deployment.CanaryMetrics
);

if (shouldPromote && deployment.CurrentSplit.CanaryPercent >= 100)
{
    orchestrator.PromoteCanary(deployment.Id);

    // After promotion, PromotedAt is set and Status becomes Promoted
    Console.WriteLine(
        $"Canary {deployment.CanaryVersion} promoted at {deployment.PromotedAt:O}");
}
else if (!shouldPromote)
{
    orchestrator.AbortDeployment(deployment.Id, 
        "Error rate exceeded threshold during 25% traffic step");

    // AbortReason is now populated, Status becomes Aborted
    Console.WriteLine($"Deployment aborted: {deployment.AbortReason}");
}
```

## Notes

- **Required members**: `ProjectName`, `StableVersion`, `CanaryVersion`, and `TargetEnvironment` are marked `required`. Any code constructing a `CanaryDeployment` must initialize these properties, either via object initializer syntax or constructor (if a parameterized constructor exists elsewhere in the type). Failure to do so will result in a compile-time error when using C# 11 or later with nullable reference types enabled.

- **Nullability of `AbortReason` and `PromotedAt`**: These properties are nullable by design. `AbortReason` is `null` for any status other than `Aborted`. `PromotedAt` is `null` until the deployment reaches the `Promoted` status. Consumers should perform null checks before dereferencing these values.

- **Metric comparison timing**: `StableMetrics` and `CanaryMetrics` are snapshots populated by the metrics collection subsystem. They are only meaningful when `Status` is `InProgress` or a terminal state that preserves the last known values. Comparing them before the evaluation window has elapsed may produce unreliable results; the `Strategy` object typically enforces a minimum observation duration.

- **Thread safety**: This type is not inherently thread-safe. In a typical deployment orchestration scenario, a single coordinator thread owns mutation of `Status`, `CurrentSplit`, `StableMetrics`, `CanaryMetrics`, `AbortReason`, and `PromotedAt`. If multiple threads require read access while a mutation is in progress, external synchronization (e.g., a lock or concurrent collection wrapper) must be applied by the consuming code.

- **Rollout plan ordering**: The `RolloutPlan` list is expected to be ordered from lowest to highest canary traffic percentage. The deployment engine advances through steps sequentially. An empty or null list indicates an immediate cutover and should be treated as a degenerate case where traffic shifts directly to 100% canary.

- **Notification suppression**: If `Priority` is set to `Critical` but a state transition event is classified at `Normal` severity, no notification will be dispatched. Operators configuring `NotificationPriority` should ensure it aligns with the severity levels emitted by the deployment engine for the events they wish to receive.

- **Immutable identity fields**: `Id`, `CreatedAt`, `InitiatedBy`, `BranchName`, `CommitHash`, and `BuildUrl` are set at creation and should not be modified thereafter. Doing so would corrupt the audit trail and break correlation with external systems.
