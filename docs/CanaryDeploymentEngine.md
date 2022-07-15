# CanaryDeploymentEngine

The `CanaryDeploymentEngine` coordinates progressive rollouts of application updates using a canary deployment strategy. It manages the lifecycle of canary deployments, including initiation, health evaluation, promotion, and rollback, while maintaining a history of deployment states for audit and analysis.

## API

### `public CanaryDeploymentEngine`

Initializes a new instance of the canary deployment engine. The engine requires configuration for health evaluation, deployment tracking, and notification services, which are typically provided via dependency injection.

### `public async Task<CanaryDeployment> StartCanaryAsync(CanaryDeploymentOptions options)`

Initiates a new canary deployment with the specified options. The deployment begins with a small subset of traffic routed to the new version, allowing health and stability to be monitored before a full rollout.

- **Parameters**:
  - `options`: Configuration for the canary deployment, including target version, traffic split, duration, and evaluation criteria.
- **Return value**: A `Task<CanaryDeployment>` resolving to the created deployment object, which includes the current state and metadata.
- **Exceptions**:
  - Throws `ArgumentException` if required options are invalid (e.g., missing version or invalid traffic split).
  - Throws `InvalidOperationException` if a deployment is already in progress for the same application.

### `public async Task<CanaryDeployment> AdvanceRolloutAsync(string deploymentId)`

Progressively increases the traffic percentage routed to the new version in an active canary deployment. This allows controlled exposure while monitoring system health.

- **Parameters**:
  - `deploymentId`: The unique identifier of the active canary deployment.
- **Return value**: A `Task<CanaryDeployment>` resolving to the updated deployment object reflecting the new traffic split.
- **Exceptions**:
  - Throws `ArgumentException` if `deploymentId` is invalid.
  - Throws `InvalidOperationException` if the deployment is not in a state that allows advancement (e.g., already completed or aborted).
  - Throws `TimeoutException` if the operation exceeds the configured timeout.

### `public async Task<CanaryDeployment> PromoteAsync(string deploymentId)`

Promotes the canary deployment to full rollout, routing all traffic to the new version. This finalizes the deployment and marks it as successful.

- **Parameters**:
  - `deploymentId`: The unique identifier of the active canary deployment.
- **Return value**: A `Task<CanaryDeployment>` resolving to the updated deployment object in a promoted state.
- **Exceptions**:
  - Throws `ArgumentException` if `deploymentId` is invalid.
  - Throws `InvalidOperationException` if the deployment is not in a state that allows promotion (e.g., not yet evaluated or already promoted/aborted).

### `public async Task<CanaryDeployment> AbortAsync(string deploymentId)`

Terminates the canary deployment and rolls back traffic to the previous stable version. This is used when health checks fail or errors are detected during the rollout.

- **Parameters**:
  - `deploymentId`: The unique identifier of the active canary deployment.
- **Return value**: A `Task<CanaryDeployment>` resolving to the updated deployment object in an aborted state.
- **Exceptions**:
  - Throws `ArgumentException` if `deploymentId` is invalid.
  - Throws `InvalidOperationException` if the deployment is not in a state that allows abortion (e.g., already completed or aborted).

### `public async Task<CanaryEvaluationResult> EvaluateHealthAsync(string deploymentId)`

Evaluates the health of the current canary deployment by checking configured metrics and thresholds. The result determines whether the deployment can proceed, be paused, or should be aborted.

- **Parameters**:
  - `deploymentId`: The unique identifier of the active canary deployment.
- **Return value**: A `Task<CanaryEvaluationResult>` containing health status, metric values, and a recommendation (e.g., continue, pause, abort).
- **Exceptions**:
  - Throws `ArgumentException` if `deploymentId` is invalid.
  - Throws `InvalidOperationException` if the deployment is not active or lacks evaluation configuration.

### `public Task<CanaryDeployment?> GetDeploymentAsync(string deploymentId)`

Retrieves the current state of a specific canary deployment by its identifier.

- **Parameters**:
  - `deploymentId`: The unique identifier of the canary deployment.
- **Return value**: A `Task<CanaryDeployment?>` resolving to the deployment object if found, or `null` if not found.
- **Exceptions**:
  - Throws `ArgumentException` if `deploymentId` is invalid.

### `public Task<List<CanaryDeployment>> GetActiveDeploymentsAsync()`

Retrieves all canary deployments that are currently active (i.e., not completed, aborted, or promoted).

- **Return value**: A `Task<List<CanaryDeployment>>` containing all active deployments.
- **Exceptions**: None.

### `public Task<List<CanaryDeployment>> GetDeploymentHistoryAsync()`

Retrieves the historical record of all completed, aborted, or promoted canary deployments, typically used for auditing and rollback analysis.

- **Return value**: A `Task<List<CanaryDeployment>>` containing deployment history in chronological order.
- **Exceptions**: None.

## Usage

### Example 1: Initiating and Monitoring a Canary Deployment
