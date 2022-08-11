# CanaryOptions

`CanaryOptions` configures the behaviour of a canary deployment strategy within the `dotnet-deploy-notify` system. It controls whether canary deployments are enabled, how the deployment steps progress linearly, the soak durations between steps, automatic rollback or advance triggers, and the thresholds that determine deployment health based on error rates and latency metrics.

## API

### `Enabled`
`public bool Enabled`

Gets or sets a value indicating whether the canary deployment strategy is active. When `false`, the deployment proceeds without canary stages, effectively falling back to a direct deployment model.

### `AutoRollbackOnFailure`
`public bool AutoRollbackOnFailure`

Gets or sets a value indicating whether an automatic rollback is triggered when any canary step fails its health checks. If `true`, the system will immediately initiate a rollback upon detecting a threshold breach; if `false`, manual intervention is required.

### `AutoAdvanceOnSuccess`
`public bool AutoAdvanceOnSuccess`

Gets or sets a value indicating whether the deployment automatically advances to the next canary step when the current step completes its soak duration without any threshold violations. When `false`, progression requires an explicit approval signal.

### `LinearStepCount`
`public int LinearStepCount`

Gets or sets the number of discrete linear steps in the canary deployment. Each step increases the traffic percentage by an equal fraction of the remaining target. A value of zero or negative is invalid and will cause configuration validation to fail.

### `StepSoakDuration`
`public TimeSpan StepSoakDuration`

Gets or sets the minimum duration each canary step must observe traffic before evaluation or progression. This soak period allows metrics to stabilise. A zero or negative duration disables soaking, which may lead to premature health assessments.

### `MaxDeploymentDuration`
`public TimeSpan MaxDeploymentDuration`

Gets or sets the absolute maximum wall-clock time the entire canary deployment is permitted to take. If the deployment does not complete within this duration, it is treated as failed and a rollback may be initiated. Must be greater than `StepSoakDuration * LinearStepCount` to be meaningful.

### `Thresholds`
`public CanaryThresholds Thresholds`

Gets or sets the composite threshold object that defines acceptable bounds for error rates and latency during canary evaluation. The `CanaryThresholds` type encapsulates the individual metric limits and multipliers.

### `AlertPriority`
`public NotificationPriority AlertPriority`

Gets or sets the priority level assigned to notifications emitted during the canary deployment. This determines the urgency tagging in downstream notification channels (e.g., low, normal, high, critical).

### `MaxErrorRatePercent`
`public double MaxErrorRatePercent`

Gets or sets the maximum allowed error rate, expressed as a percentage of total requests, before a canary step is considered unhealthy. A value of `0.0` disables error-rate checking. Values outside the range `[0.0, 100.0]` cause a validation error.

### `MaxP95LatencyMs`
`public double MaxP95LatencyMs`

Gets or sets the maximum acceptable 95th percentile latency in milliseconds. If the observed P95 latency exceeds this value, the step is marked as degraded. A value of `0.0` disables P95 latency checks.

### `MaxP99LatencyMs`
`public double MaxP99LatencyMs`

Gets or sets the maximum acceptable 99th percentile latency in milliseconds. If the observed P99 latency exceeds this value, the step is marked as degraded. A value of `0.0` disables P99 latency checks.

### `ErrorRateMultiplier`
`public double ErrorRateMultiplier`

Gets or sets a multiplier applied to the baseline error rate to derive the dynamic threshold for canary steps. For example, a value of `2.0` means the canary step tolerates up to twice the baseline error rate. Must be greater than or equal to `1.0`.

### `LatencyDegradationPercent`
`public double LatencyDegradationPercent`

Gets or sets the allowed percentage increase in latency relative to the baseline before a canary step is considered degraded. A value of `20.0` permits up to a 20% increase over baseline P95/P99 values. Must be non-negative.

## Usage

### Example 1: Basic Linear Canary with Auto-Advance

```csharp
var options = new CanaryOptions
{
    Enabled = true,
    LinearStepCount = 5,
    StepSoakDuration = TimeSpan.FromMinutes(10),
    MaxDeploymentDuration = TimeSpan.FromHours(2),
    AutoAdvanceOnSuccess = true,
    AutoRollbackOnFailure = true,
    MaxErrorRatePercent = 1.0,
    MaxP95LatencyMs = 200.0,
    MaxP99LatencyMs = 500.0,
    ErrorRateMultiplier = 2.0,
    LatencyDegradationPercent = 25.0,
    AlertPriority = NotificationPriority.High
};

// Validate before passing to the deployment orchestrator
CanaryConfigurationValidator.Validate(options);
```

### Example 2: Manual Gating with Custom Thresholds

```csharp
var thresholds = new CanaryThresholds
{
    MaxErrorRatePercent = 0.5,
    MaxP95LatencyMs = 150.0,
    MaxP99LatencyMs = 400.0,
    ErrorRateMultiplier = 1.5,
    LatencyDegradationPercent = 15.0
};

var options = new CanaryOptions
{
    Enabled = true,
    LinearStepCount = 3,
    StepSoakDuration = TimeSpan.FromMinutes(30),
    MaxDeploymentDuration = TimeSpan.FromHours(3),
    AutoAdvanceOnSuccess = false,  // requires manual approval after each step
    AutoRollbackOnFailure = true,
    Thresholds = thresholds,
    AlertPriority = NotificationPriority.Critical
};

// The orchestrator will pause after each step, awaiting an Advance signal
await orchestrator.StartCanaryAsync(options);
```

## Notes

- **Validation:** All numeric fields that represent percentages or multipliers are validated at configuration time. Values outside documented ranges throw `ArgumentOutOfRangeException` during validation. `LinearStepCount` must be a positive integer; zero or negative values are rejected.
- **Duration consistency:** `MaxDeploymentDuration` must exceed the product of `LinearStepCount` and `StepSoakDuration`. If it does not, the deployment may never complete within the allowed window, and a configuration warning is raised.
- **Threshold precedence:** When both `Thresholds` and the individual scalar properties (`MaxErrorRatePercent`, `MaxP95LatencyMs`, etc.) are set, the `Thresholds` object takes precedence. The scalar properties serve as a convenience for simple configurations and are ignored if `Thresholds` is explicitly assigned.
- **Thread safety:** `CanaryOptions` is a plain configuration object and is not inherently thread-safe. Once an instance is passed to the deployment orchestrator, it should be treated as immutable. Concurrent reads from multiple threads are safe only if no thread modifies the object after publication.
- **Default values:** Uninitialised numeric fields default to zero, which disables the corresponding checks. This can lead to deployments proceeding without health evaluation if not explicitly configured.
- **Soak duration of zero:** Setting `StepSoakDuration` to `TimeSpan.Zero` causes immediate step evaluation, which may produce false positives due to cold-start effects in metrics pipelines.
