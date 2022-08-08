# IRollbackService

The `IRollbackService` interface defines the contract for services that manage rollback operations for deployments. It provides methods to initiate rollbacks, query rollback history, check rollback status, and cancel ongoing rollbacks. Implementations of this interface are responsible for coordinating with deployment systems to execute and track rollback actions.

## API

### `RollbackService`

The concrete service class that implements `IRollbackService`. This class is responsible for executing the rollback logic by interacting with the underlying deployment infrastructure.

### `InitiateRollbackAsync`

Initiates a new rollback operation for a deployment.

- **Parameters**:
  - None
- **Return value**: A `Task<RollbackResult>` that resolves to a `RollbackResult` object representing the outcome of the rollback initiation.
- **Exceptions**: May throw exceptions if the rollback cannot be initiated (e.g., invalid deployment state, missing permissions, or system errors).

### `GetRollbackHistoryAsync`

Retrieves the history of rollback operations.

- **Parameters**:
  - None
- **Return value**: A `Task<List<RollbackResult>>` that resolves to a list of `RollbackResult` objects, each representing a past rollback operation.
- **Exceptions**: May throw exceptions if the history cannot be retrieved (e.g., storage unavailability or permission issues).

### `GetRollbackStatusAsync`

Checks the current status of an ongoing rollback operation.

- **Parameters**:
  - None
- **Return value**: A `Task<RollbackResult?>` that resolves to the `RollbackResult` of the most recent rollback if it is still in progress, or `null` if no active rollback exists.
- **Exceptions**: May throw exceptions if the status cannot be determined (e.g., network failures or invalid operation state).

### `CancelRollbackAsync`

Attempts to cancel an ongoing rollback operation.

- **Parameters**:
  - None
- **Return value**: A `Task<bool>` that resolves to `true` if the cancellation was successful, or `false` if no active rollback was found or cancellation failed.
- **Exceptions**: May throw exceptions if the cancellation cannot be processed (e.g., system errors or permission issues).

## Usage

### Example 1: Initiating and Monitoring a Rollback
