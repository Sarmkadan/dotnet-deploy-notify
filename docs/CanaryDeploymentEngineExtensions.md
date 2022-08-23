# CanaryDeploymentEngineExtensions
The `CanaryDeploymentEngineExtensions` class provides a set of extension methods for managing canary deployments. It allows developers to advance, promote, abort, and query the status of canary deployments in an asynchronous manner. These methods are designed to be used in conjunction with a canary deployment engine, enabling flexible and controlled rollout of new software versions.

## API
* `TryAdvanceRolloutAsync`: Attempts to advance the rollout of a canary deployment. This method returns a `bool` value indicating whether the advancement was successful. It does not take any parameters and does not throw any exceptions.
* `TryPromoteAsync`: Attempts to promote a canary deployment. This method returns a `bool` value indicating whether the promotion was successful. It does not take any parameters and does not throw any exceptions.
* `TryAbortAsync`: Attempts to abort a canary deployment. This method returns a `CanaryDeployment?` value, which is `null` if the abortion was successful, or the original canary deployment if it was not. It does not take any parameters and does not throw any exceptions.
* `GetCanaryPercentageNormalizedAsync`: Retrieves the normalized percentage of a canary deployment. This method returns a `double?` value, which is `null` if the percentage cannot be determined, or the normalized percentage as a value between 0 and 1. It does not take any parameters and does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `CanaryDeploymentEngineExtensions` class:
```csharp
// Advance the rollout of a canary deployment
bool advancementSuccessful = await CanaryDeploymentEngineExtensions.TryAdvanceRolloutAsync();
if (advancementSuccessful)
{
    Console.WriteLine("Rollout advancement successful");
}
else
{
    Console.WriteLine("Rollout advancement failed");
}

// Promote a canary deployment and retrieve its normalized percentage
bool promotionSuccessful = await CanaryDeploymentEngineExtensions.TryPromoteAsync();
double? normalizedPercentage = await CanaryDeploymentEngineExtensions.GetCanaryPercentageNormalizedAsync();
if (promotionSuccessful && normalizedPercentage.HasValue)
{
    Console.WriteLine($"Promotion successful, normalized percentage: {normalizedPercentage.Value}");
}
else
{
    Console.WriteLine("Promotion failed or percentage cannot be determined");
}
```

## Notes
When using the `CanaryDeploymentEngineExtensions` class, note that the `TryAdvanceRolloutAsync`, `TryPromoteAsync`, and `TryAbortAsync` methods do not throw exceptions. Instead, they return values indicating the success or failure of the operation. The `GetCanaryPercentageNormalizedAsync` method returns `null` if the percentage cannot be determined. Additionally, these methods are designed to be used in an asynchronous context, and their usage should be coordinated accordingly to avoid conflicts and ensure thread safety. The class itself does not maintain any state, making it safe to use from multiple threads concurrently. However, the underlying canary deployment engine may have its own threading constraints, which should be respected when using these extension methods.
