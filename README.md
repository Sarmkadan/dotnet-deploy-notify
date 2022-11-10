// existing content ...

## CanaryDeploymentExtensions

The `CanaryDeploymentExtensions` class provides helper methods for analyzing and managing canary deployment status, health, and rollout progress. It includes methods to check deployment status, calculate health scores, determine promotion eligibility, and track traffic progression.

Example usage:
```csharp
var deployment = new CanaryDeployment
{
    Status = CanaryStatus.Active,
    CurrentSplit = new TrafficSplit { StablePercent = 80, CanaryPercent = 20 },
    CanaryMetrics = new CanaryMetrics { ErrorRatePercent = 0.5, P95LatencyMs = 150 },
    RolloutPlan = new List<RolloutStep>
    {
        new RolloutStep { CanaryPercent = 20, SoakDuration = TimeSpan.FromMinutes(10) },
        new RolloutStep { CanaryPercent = 50, SoakDuration = TimeSpan.FromMinutes(15) }
    },
    ActiveStep = new RolloutStep { CanaryPercent = 20, StartedAt = DateTime.UtcNow, Status = RolloutStepStatus.InProgress }
};

bool isActive = deployment.IsActive();
string statusSummary = deployment.GetStatusSummary();
bool canPromote = deployment.CanPromote();
double? nextPercentage = deployment.GetNextTrafficPercentage();
TimeSpan? soakRemaining = deployment.GetCurrentSoakRemaining();

Console.WriteLine(statusSummary);
Console.WriteLine($"Can promote: {canPromote}");
Console.WriteLine($"Next traffic percentage: {nextPercentage}%");
Console.WriteLine($"Soak remaining: {soakRemaining?.TotalMinutes:F1} minutes");
```
// rest of existing content remains unchanged
