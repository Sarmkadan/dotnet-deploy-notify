# DeploymentHistoryServiceTests

The `DeploymentHistoryServiceTests` class contains unit tests for the `DeploymentHistoryService` in the `dotnet-deploy-notify` project. These tests verify the correctness of methods responsible for recording, retrieving, and analyzing deployment history entries, ensuring proper handling of valid and edge-case inputs, as well as adherence to expected behavior such as sorting, filtering, and statistical calculations.

## API

### `DeploymentHistoryServiceTests`
Constructor for the test class. Initializes test dependencies and prepares the test environment.

### `RecordDeploymentAsync_WithValidEntry_StoresEntry`
**Purpose**: Verifies that a valid deployment entry is successfully stored in the history.
**Parameters**: None (test setup uses a predefined valid entry).
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: Fails the test if the entry is not stored as expected.

### `RecordDeploymentAsync_WithNullEntry_ThrowsArgumentNullException`
**Purpose**: Ensures that passing a `null` entry to `RecordDeploymentAsync` throws an `ArgumentNullException`.
**Parameters**: None (test setup uses `null` as input).
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: `ArgumentNullException` if the input is `null`.

### `RecordDeploymentAsync_WithEmptyProjectName_ThrowsArgumentException`
**Purpose**: Validates that an entry with an empty project name throws an `ArgumentException`.
**Parameters**: None (test setup uses an entry with an empty project name).
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: `ArgumentException` if the project name is empty.

### `RecordFromNotificationAsync_WithValidNotification_CreatesEntry`
**Purpose**: Confirms that a valid deployment notification results in the creation of a corresponding history entry.
**Parameters**: None (test setup uses a predefined valid notification).
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: Fails the test if the entry is not created as expected.

### `GetProjectHistoryAsync_ReturnsNewestFirst`
**Purpose**: Tests that `GetProjectHistoryAsync` returns entries sorted by timestamp in descending order (newest first).
**Parameters**: None (test setup populates multiple entries).
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: Fails the test if entries are not sorted correctly.

### `GetProjectHistoryAsync_RespectsLimit`
**Purpose**: Ensures that `GetProjectHistoryAsync` honors the `limit` parameter, returning no more than the specified number of entries.
**Parameters**: None (test setup uses a predefined `limit`).
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: Fails the test if the returned entries exceed the limit.

### `GetProjectHistoryAsync_IsCaseInsensitive`
**Purpose**: Verifies that project name comparisons in `GetProjectHistoryAsync` are case-insensitive.
**Parameters**: None (test setup uses mixed-case project names).
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: Fails the test if case sensitivity affects the results.

### `GetProjectHistoryAsync_ReturnsEmptyForUnknownProject`
**Purpose**: Confirms that `GetProjectHistoryAsync` returns an empty collection for an unknown project name.
**Parameters**: None (test setup queries a non-existent project).
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: Fails the test if any entries are returned.

### `GetRecentDeploymentsAsync_ReturnsAcrossProjects`
**Purpose**: Tests that `GetRecentDeploymentsAsync` aggregates and returns entries from all projects, sorted by timestamp.
**Parameters**: None (test setup populates entries across multiple projects).
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: Fails the test if entries are not aggregated or sorted correctly.

### `GetRecentDeploymentsAsync_RespectsLimit`
**Purpose**: Ensures that `GetRecentDeploymentsAsync` respects the `limit` parameter when returning entries across projects.
**Parameters**: None (test setup uses a predefined `limit`).
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: Fails the test if the returned entries exceed the limit.

### `GetStatisticsAsync_CalculatesSuccessRate`
**Purpose**: Validates that `GetStatisticsAsync` correctly calculates the success rate of deployments for a project.
**Parameters**: None (test setup includes successful and failed deployments).
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: Fails the test if the success rate is miscalculated.

### `GetStatisticsAsync_CountsRollbacks`
**Purpose**: Ensures that `GetStatisticsAsync` accurately counts rollback entries in the deployment history.
**Parameters**: None (test setup includes rollback entries).
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: Fails the test if rollbacks are not counted correctly.

### `GetStatisticsAsync_CalculatesAverageDuration`
**Purpose**: Tests that `GetStatisticsAsync` computes the average duration of deployments for a project.
**Parameters**: None (test setup includes deployments with varying durations).
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: Fails the test if the average duration is miscalculated.

### `GetStatisticsAsync_ReturnsZeroRateWhenNoDeployments`
**Purpose**: Confirms that `GetStatisticsAsync` returns a zero success rate when no deployments exist for a project.
**Parameters**: None (test setup queries a project with no deployments).
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: Fails the test if the success rate is not zero.

### `GetByEnvironmentAsync_FiltersByEnvironment`
**Purpose**: Verifies that `GetByEnvironmentAsync` filters entries by the specified environment name.
**Parameters**: None (test setup uses a predefined environment name).
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: Fails the test if entries from other environments are included.

### `GetLastSuccessfulDeploymentAsync_ReturnsLatestSuccess`
**Purpose**: Ensures that `GetLastSuccessfulDeploymentAsync` returns the most recent successful deployment for a project.
**Parameters**: None (test setup includes multiple successful deployments).
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: Fails the test if the incorrect entry is returned.

### `GetLastSuccessfulDeploymentAsync_ReturnsNullWhenNoneFound`
**Purpose**: Validates that `GetLastSuccessfulDeploymentAsync` returns `null` when no successful deployments exist for a project.
**Parameters**: None (test setup queries a project with no successful deployments).
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: Fails the test if a non-null result is returned.

### `GetRollbackEntriesAsync_ReturnsOnlyRollbacks`
**Purpose**: Confirms that `GetRollbackEntriesAsync` returns only entries marked as rollbacks.
**Parameters**: None (test setup includes both rollback and non-rollback entries).
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: Fails the test if non-rollback entries are included.

### `IsSuccessful_ReflectsStatus`
**Purpose**: Tests that the `IsSuccessful` property correctly reflects the deployment status (success/failure).
**Parameters**: None (test setup uses entries with varying statuses).
**Return Value**: `void`.
**Throws**: Fails the test if the property does not match the expected status.

## Usage

### Example 1: Recording and Retrieving Deployment History
