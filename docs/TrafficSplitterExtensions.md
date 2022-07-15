# TrafficSplitterExtensions

Provides extension methods for configuring and managing traffic splitting in deployment scenarios, particularly for canary and blue-green deployments. These utilities simplify the creation of deployment strategies and decision-making during progressive rollouts.

## API

### `CreateLinearCanaryDeployment`
Creates a `CanaryDeployment` with a linear traffic increase over a specified number of steps.

- **Parameters**
  - `totalSteps` (int): The total number of steps in the linear progression.
  - `initialPercentage` (double): The starting traffic percentage (normalized between 0.0 and 1.0).
  - `finalPercentage` (double): The ending traffic percentage (normalized between 0.0 and 1.0).
- **Return Value**
  Returns a `CanaryDeployment` configured with linearly increasing traffic percentages.
- **Exceptions**
  Throws `ArgumentOutOfRangeException` if `totalSteps` is less than 1, or if `initialPercentage` or `finalPercentage` are outside the [0.0, 1.0] range.

---

### `CreateExponentialCanaryDeployment`
Creates a `CanaryDeployment` with an exponential traffic increase over a specified number of steps.

- **Parameters**
  - `totalSteps` (int): The total number of steps in the exponential progression.
  - `initialPercentage` (double): The starting traffic percentage (normalized between 0.0 and 1.0).
  - `finalPercentage` (double): The ending traffic percentage (normalized between 0.0 and 1.0).
- **Return Value**
  Returns a `CanaryDeployment` configured with exponentially increasing traffic percentages.
- **Exceptions**
  Throws `ArgumentOutOfRangeException` if `totalSteps` is less than 1, or if `initialPercentage` or `finalPercentage` are outside the [0.0, 1.0] range.

---
### `ShouldProceedToNextStepAsync`
Determines whether a deployment should proceed to the next step based on a health check or validation result.

- **Parameters**
  - `context` (object): The deployment context containing current state and metrics.
  - `healthCheck` (Func<Task<bool>>): An asynchronous function that evaluates whether the current step is healthy.
- **Return Value**
  Returns `Task<bool>` where `true` indicates the deployment should proceed to the next step, `false` otherwise.
- **Exceptions**
  Throws `ArgumentNullException` if `healthCheck` is `null`.

---
### `GetCanaryPercentageNormalized`
Retrieves the current traffic percentage for a canary deployment at a given step.

- **Parameters**
  - `deployment` (CanaryDeployment): The canary deployment configuration.
  - `currentStep` (int): The current step in the deployment progression (1-based index).
- **Return Value**
  Returns the normalized traffic percentage (between 0.0 and 1.0) for the specified step.
- **Exceptions**
  Throws `ArgumentNullException` if `deployment` is `null`.
  Throws `ArgumentOutOfRangeException` if `currentStep` is less than 1 or exceeds the total steps in the deployment.

---
### `CreateBlueGreenCanaryDeployment`
Creates a `CanaryDeployment` representing a blue-green deployment strategy with immediate traffic shift.

- **Parameters**
  - `percentage` (double): The traffic percentage to allocate to the new version (normalized between 0.0 and 1.0).
- **Return Value**
  Returns a `CanaryDeployment` configured for a single-step blue-green transition.
- **Exceptions**
  Throws `ArgumentOutOfRangeException` if `percentage` is outside the [0.0, 1.0] range.

## Usage

### Example 1: Linear Canary Deployment
