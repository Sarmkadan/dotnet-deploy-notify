# CanaryDeploymentExtensions

Static utility class providing helper methods for evaluating and managing canary deployment states, traffic splits, health scores, and promotion eligibility.

## API

### `IsActive`
Determines whether the current deployment is in an active canary state (i.e., not fully promoted or failed).
**Returns:** `true` if the deployment is active; otherwise, `false`.

### `IsPromoted`
Determines whether the current deployment has been fully promoted to 100% traffic.
**Returns:** `true` if the deployment is fully promoted; otherwise, `false`.

### `IsFailedOrAborted`
Determines whether the current deployment has failed or been aborted.
**Returns:** `true` if the deployment is failed or aborted; otherwise, `false`.

### `GetTrafficSplitDisplay`
Formats the current traffic split percentage as a human-readable string (e.g., "5% / 95%").
**Returns:** A string representing the traffic split between the canary and baseline versions.

### `CalculateHealthScore`
Computes a normalized health score for the current deployment based on error rates, latency, and stability metrics.
**Returns:** A `double` between 0.0 and 100.0 representing the health score.

### `GetStatusSummary`
Generates a concise status summary of the canary deployment, including health score, traffic split, and promotion state.
**Returns:** A string summarizing the deployment's current status.

### `CanPromote`
Evaluates whether the current deployment is eligible for promotion to the next traffic tier or full rollout.
**Returns:** `true` if promotion is allowed; otherwise, `false`.

### `GetNextTrafficPercentage`
Calculates the next recommended traffic percentage for promotion, if applicable.
**Returns:** An optional `double` representing the next traffic percentage (e.g., 25%, 50%), or `null` if no promotion is advised.

### `GetCurrentSoakRemaining`
Determines the remaining soak time before the next promotion can occur.
**Returns:** An optional `TimeSpan` indicating the remaining soak duration, or `null` if no soak is required.

### `IsCurrentSoakComplete`
Checks whether the current soak period has elapsed and the deployment is ready for promotion.
**Returns:** `true` if the soak period has completed; otherwise, `false`.

## Usage
