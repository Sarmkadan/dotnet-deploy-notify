# TrafficSplitter

TrafficSplitter is a component responsible for managing traffic distribution between primary and canary deployments, determining routing decisions based on health evaluations, and generating rollout plans for gradual traffic shifts. It integrates with health evaluation logic to dynamically adjust traffic splits during deployment processes.

## API

### TrafficSplitter()

**Purpose:** Initializes a new instance of the TrafficSplitter class.

**Parameters:** None.

**Return Value:** A new TrafficSplitter instance.

**Exceptions:** None.

---

### TrafficSplit ComputeNextSplit(TrafficSplit currentSplit)

**Purpose:** Calculates the next traffic split configuration based on the current split and internal rollout logic.

**Parameters:**  
- `currentSplit` (TrafficSplit): The current traffic distribution configuration.

**Return Value:** TrafficSplit representing the next calculated split.

**Exceptions:**  
- `ArgumentNullException`: Thrown when `currentSplit` is null.

---

### bool ShouldRouteToCanary(TrafficSplit currentSplit)

**Purpose:** Determines whether incoming traffic should be routed to the canary deployment based on the current split configuration.

**Parameters:**  
- `currentSplit` (TrafficSplit): The current traffic distribution configuration.

**Return Value:** True if traffic should route to canary; otherwise, false.

**Exceptions:**  
- `ArgumentNullException`: Thrown when `currentSplit` is null.

---

### List<CanaryRolloutStep> GenerateRolloutPlan(double targetCanaryPercentage)

**Purpose:** Generates a sequence of incremental traffic shift steps to reach a target canary percentage.

**Parameters:**  
- `targetCanaryPercentage` (double): The desired final canary traffic percentage (0.0 to 100.0).

**Return Value:** A list of CanaryRolloutStep objects defining incremental traffic shifts.

**Exceptions:**  
- `ArgumentOutOfRangeException`: Thrown when `targetCanaryPercentage` is outside the valid range (0.0–100.0).

---

### CanaryHealthEvaluator

**Purpose:** Gets the health evaluator used to assess canary deployment status.

**Parameters:** None.

**Return Value:** CanaryHealthEvaluator instance.

**Exceptions:** None.

---

### async Task<CanaryEvaluationResult> EvaluateAsync(CanaryMetrics metrics)

**Purpose:** Asynchronously evaluates the health of a canary deployment using provided metrics.

**Parameters:**  
- `metrics` (CanaryMetrics): Metrics collected from the canary deployment.

**Return Value:** CanaryEvaluationResult indicating health status and recommendations.

**Exceptions:**  
- `ArgumentNullException`: Thrown when `metrics` is null.

---

### Task<CanaryMetrics> CollectMetricsAsync()

**Purpose:** Asynchronously collects metrics required for canary health evaluation.

**Parameters:** None.

**Return Value:** CanaryMetrics containing performance and health data.

**Exceptions:** None.

---

## Usage

```csharp
// Example 1: Generating and applying a rollout plan
var splitter = new TrafficSplitter();
var rolloutPlan = splitter.GenerateRolloutPlan(25.0);

foreach (var step in rolloutPlan)
{
    var currentSplit = splitter.ComputeNextSplit(step.CurrentSplit);
    // Apply currentSplit to routing configuration
}
```

```csharp
// Example 2: Evaluating canary health and routing decisions
var splitter = new TrafficSplitter();
var metrics = await splitter.CollectMetricsAsync();
var evaluation = await splitter.EvaluateAsync(metrics);

if (evaluation.IsHealthy && splitter.ShouldRouteToCanary(evaluation.CurrentSplit))
{
    // Route traffic to canary
}
else
{
    // Route traffic to primary
}
```

---

## Notes

- Thread Safety: Public members are safe for concurrent invocation. Internal state mutations during `ComputeNextSplit` and `EvaluateAsync` are synchronized.
- Edge Cases: `GenerateRolloutPlan` returns an empty list if `targetCanaryPercentage` is 0.0 or 100.0. `ShouldRouteToCanary` returns false if canary percentage in `currentSplit` is 0.
- Dependencies: Requires a properly configured `CanaryHealthEvaluator` instance for meaningful health assessments. Null checks are enforced on all reference-type parameters.
