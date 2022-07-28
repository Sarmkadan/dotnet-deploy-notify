# IDeploymentHistoryService

The `IDeploymentHistoryService` interface defines a contract for recording, querying, and analyzing deployment history within the `dotnet-deploy-notify` project. It provides methods to track deployments, retrieve historical data, compute statistics, and identify rollback candidates, enabling reliable deployment monitoring and auditing.

## API

### `Task RecordDeploymentAsync`

Records a new deployment entry in the history.

- **Parameters**
  - `projectName` (string): The name of the project being deployed.
  - `environment` (string): The target environment (e.g., "Production", "Staging").
  - `version` (string): The version identifier of the deployed artifact.
  - `status` (DeploymentStatus): The outcome of the deployment (e.g., Successful, Failed).
  - `timestamp` (DateTimeOffset): The time when the deployment was initiated.
  - `initiatedBy` (string): The identifier of the user or system that initiated the deployment.
  - `notes` (string, optional): Additional context about the deployment.

- **Return value**
  Returns a `Task` that completes when the record is persisted.

- **Exceptions**
  Throws `ArgumentException` if `projectName`, `environment`, `version`, or `status` are null or empty.
  Throws `ArgumentOutOfRangeException` if `timestamp` is in the future.

---

### `Task RecordFromNotificationAsync`

Records a deployment entry derived from a notification payload.

- **Parameters**
  - `notification` (DeploymentNotification): The incoming notification containing deployment details.

- **Return value**
  Returns a `Task` that completes when the record is persisted.

- **Exceptions**
  Throws `ArgumentNullException` if `notification` is null.
  Propagates exceptions from `RecordDeploymentAsync` if validation or persistence fails.

---

### `Task<List<DeploymentHistoryEntry>> GetProjectHistoryAsync`

Retrieves the complete deployment history for a specific project.

- **Parameters**
  - `projectName` (string): The name of the project.

- **Return value**
  Returns a `Task` resolving to a list of `DeploymentHistoryEntry` objects, ordered chronologically by `Timestamp` (newest first). Returns an empty list if no entries exist.

- **Exceptions**
  Throws `ArgumentException` if `projectName` is null or empty.

---

### `Task<List<DeploymentHistoryEntry>> GetRecentDeploymentsAsync`

Retrieves the most recent deployments across all projects.

- **Parameters**
  - `limit` (int): The maximum number of entries to return.

- **Return value**
  Returns a `Task` resolving to a list of `DeploymentHistoryEntry` objects, ordered chronologically by `Timestamp` (newest first). Returns an empty list if no entries exist or if `limit` ≤ 0.

- **Exceptions**
  Throws `ArgumentOutOfRangeException` if `limit` < 0.

---

### `Task<DeploymentStatistics> GetStatisticsAsync`

Computes summary statistics about deployments.

- **Parameters**
  None.

- **Return value**
  Returns a `Task` resolving to a `DeploymentStatistics` object containing counts of successful, failed, and total deployments, grouped by environment.

- **Exceptions**
  Returns default statistics (all counts zero) if no data exists; never throws.

---

### `Task<List<DeploymentHistoryEntry>> GetByEnvironmentAsync`

Retrieves all deployment entries for a specific environment.

- **Parameters**
  - `environment` (string): The target environment.

- **Return value**
  Returns a `Task` resolving to a list of `DeploymentHistoryEntry` objects, ordered chronologically by `Timestamp` (newest first). Returns an empty list if no entries exist.

- **Exceptions**
  Throws `ArgumentException` if `environment` is null or empty.

---
### `Task<DeploymentHistoryEntry?> GetLastSuccessfulDeploymentAsync`

Retrieves the most recent successful deployment entry.

- **Parameters**
  - `projectName` (string): The name of the project.
  - `environment` (string): The target environment.

- **Return value**
  Returns a `Task` resolving to the most recent `DeploymentHistoryEntry` with `Status` equal to `DeploymentStatus.Successful`, or `null` if none exists.

- **Exceptions**
  Throws `ArgumentException` if `projectName` or `environment` are null or empty.

---
### `Task<List<DeploymentHistoryEntry>> GetRollbackEntriesAsync`

Retrieves deployment entries that may serve as rollback candidates for a given environment.

- **Parameters**
  - `projectName` (string): The name of the project.
  - `environment` (string): The target environment.
  - `since` (DateTimeOffset): The cutoff timestamp; only entries older than this are considered.

- **Return value**
  Returns a `Task` resolving to a list of `DeploymentHistoryEntry` objects, ordered chronologically by `Timestamp` (oldest first). Returns an empty list if no candidates exist.

- **Exceptions**
  Throws `ArgumentException` if `projectName` or `environment` are null or empty.
  Throws `ArgumentOutOfRangeException` if `since` is in the future.

## Usage

### Example 1: Recording a deployment
