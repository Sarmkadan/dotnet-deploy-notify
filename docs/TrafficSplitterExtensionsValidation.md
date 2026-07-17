# TrafficSplitterExtensionsValidation

TrafficSplitterExtensionsValidation is a static helper class that provides validation logic for the various canary deployment configuration scenarios supported by the **dotnet-deploy-notify** library. It exposes a set of methods that return validation errors, boolean flags indicating validity, and methods that throw when a configuration is invalid. The class is intentionally stateless and thread‑safe, making it suitable for use in both synchronous and asynchronous contexts.

## API

| Member | Purpose | Parameters | Return Value | Throws |
|--------|---------|------------|--------------|--------|
| `ValidateCreateLinearCanaryDeployment` | Validates the arguments for creating a linear canary deployment. | `int totalSteps`, `int stepSize`, `int initialPercentage` | `IReadOnlyList<string>` – a list of validation error messages; empty if valid. | None |
| `ValidateCreateExponentialCanaryDeployment` | Validates the arguments for creating an exponential canary deployment. | `int totalSteps`, `double growthFactor`, `int initialPercentage` | `IReadOnlyList<string>` – a list of validation error messages; empty if valid. | None |
| `ValidateShouldProceedToNextStepAsync` | Validates the state before proceeding to the next canary step. | `int currentStep`, `int totalSteps` | `IReadOnlyList<string>` – a list of validation error messages; empty if valid. | None |
| `ValidateGetCanaryPercentageNormalized` | Validates the arguments for normalizing a canary percentage. | `int percentage`, `int maxPercentage` | `IReadOnlyList<string>` – a list of validation error messages; empty if valid. | None |
| `ValidateCreateBlueGreenCanaryDeployment` | Validates the arguments for creating a blue‑green canary deployment. | `string blueEnvironment`, `string greenEnvironment` | `IReadOnlyList<string>` – a list of validation error messages; empty if valid. | None |
| `IsValidCreateLinearCanaryDeployment` | Returns `true` if the linear canary configuration is valid. | `int totalSteps`, `int stepSize`, `int initialPercentage` | `bool` – `true` when no validation errors. | None |
| `IsValidCreateExponentialCanaryDeployment` | Returns `true` if the exponential canary configuration is valid. | `int totalSteps`, `double growthFactor`, `int initialPercentage` | `bool` – `true` when no validation errors. | None |
| `IsValidShouldProceedToNextStepAsync` | Returns `true` if the next step can be proceeded to. | `int currentStep`, `int totalSteps` | `bool` – `true` when no validation errors. | None |
| `IsValidGetCanaryPercentageNormalized` | Returns `true` if the percentage normalization arguments are valid. | `int percentage`, `int maxPercentage` | `bool` – `true` when no validation errors. | None |
| `IsValidCreateBlueGreenCanaryDeployment` | Returns `true` if the blue‑green configuration is valid. | `string blueEnvironment`, `string greenEnvironment` | `bool` – `true` when no validation errors. | None |
| `EnsureValidCreateLinearCanaryDeployment` | Throws an exception if the linear canary configuration is invalid. | `int totalSteps`, `int stepSize`, `int initialPercentage` | `void` – throws `ArgumentException` (or a library‑specific validation exception) when invalid. | `ArgumentException` |
| `EnsureValidCreateExponentialCanaryDeployment` | Throws an exception if the exponential canary configuration is invalid. | `int totalSteps`, `double growthFactor`, `int initialPercentage` | `void` – throws `ArgumentException` when invalid. | `ArgumentException` |
| `EnsureValidShouldProceedToNextStepAsync` | Throws an exception if the next step cannot be proceeded to. | `int currentStep`, `int totalSteps` | `void` – throws `InvalidOperationException` when invalid. | `InvalidOperationException` |
| `EnsureValidGetCanaryPercentageNormalized` | Throws an exception if the percentage normalization arguments are invalid. | `int percentage`, `int maxPercentage` | `void` – throws `ArgumentOutOfRangeException` when invalid. | `ArgumentOutOfRangeException` |
| `EnsureValidCreateBlueGreenCanaryDeployment` | Throws an exception if the blue‑green configuration is invalid. | `string blueEnvironment`, `string greenEnvironment` | `void` – throws `ArgumentException` when invalid. | `ArgumentException` |

## Usage

