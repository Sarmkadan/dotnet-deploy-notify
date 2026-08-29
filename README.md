## AmbientRequestContextTests

The AmbientRequestContextTests class contains tests for the AmbientRequestContext class.

### Current_Getter_CreatesNewContext_WhenNoContextSet
Tests that the Current getter creates a new context when no context is set.

### Current_Getter_ReturnsSameContext_WithinSameLogicalCallContext
Tests that the Current getter returns the same context within the same logical call context.

### Current_Setter_Throws_WhenContextAlreadyExists
Tests that the Current setter throws an exception when the context already exists.

### SetContext_Throws_WhenContextAlreadyExists
Tests that the SetContext method throws an exception when the context already exists.

### ClearContext_RemovesCurrentContext
Tests that the ClearContext method removes the current context.

### Reset_CreatesNewContext
Tests that the Reset method creates a new context.

### Current_ContextFlowsAcrossAwaitBoundary
Tests that the current context flows across await boundaries.

### RequestContextScope_ProperlyRestoresPreviousContext
Tests that the request context scope properly restores the previous context.

### Context_IsIsolatedBetweenParallelTasks
Tests that the context is isolated between parallel tasks.

### ExecuteInContextAsync_ProperlyIsolatesContext
Tests that the ExecuteInContextAsync method properly isolates the context.

### RequestContextScope_ConstructorWithCustomContext_SetsContext
Tests that the request context scope constructor with a custom context sets the context.

### RequestContextScope_Dispose_RestoresPreviousContext
Tests that the request context scope dispose restores the previous context.

Example usage:
```csharp
public class MyClass
{
    private readonly AmbientRequestContext _context;

    public MyClass()
    {
        _context = new AmbientRequestContext();
    }

    public void MyMethod()
    {
        using (var scope = new RequestContextScope(_context))
        {
            // code that uses the context
        }
    }
}
```

## CanaryDeploymentEngineTests

The `CanaryDeploymentEngineTests` class contains unit tests for the `CanaryDeploymentEngine` class. It verifies the behavior of starting a canary deployment, advancing the rollout through steps, evaluating health for auto-rollback, handling cancellation tokens, and promoting the deployment to skip remaining steps.

Example usage:
```csharp
using DotNetDeployNotify.Tests.Canary;
using Xunit;

public class CanaryDeploymentEngineTestsExample
{
    [Fact]
    public async void TestStartCanary()
    {
        var tests = new CanaryDeploymentEngineTests();
        await tests.StartCanaryAsync_CreatesDeployment_WithInitialStep();
    }
}
```

## CanaryServiceExtensionsJsonExtensionsTests

The `CanaryServiceExtensionsJsonExtensionsTests` class contains unit tests for the `CanaryServiceExtensionsJsonExtensions` class. It verifies the JSON serialization and deserialization of `CanaryServiceExtensionsMetadata`, including handling of null and invalid inputs, and ensuring round-trip correctness for all properties.

Example usage:
```csharp
using DotNetDeployNotify.Infrastructure.Tests;
using Xunit;

public class CanaryServiceExtensionsJsonExtensionsTestsExample
{
    [Fact]
    public void TestToJson()
    {
        var tests = new CanaryServiceExtensionsJsonExtensionsTests();
        tests.ToJson_WithValidMetadata_ReturnsValidJson();
    }
}
```

## CanaryHealthEvaluatorTests

The `CanaryHealthEvaluatorTests` class contains unit tests for the `CanaryHealthEvaluator` class. It verifies the health evaluation logic under various conditions, including healthy metrics, threshold violations for error rate and latency, boundary conditions, and multiple simultaneous violations.

Example usage:
```csharp
using DotNetDeployNotify.Tests.Canary;
using Xunit;

public class CanaryHealthEvaluatorTestsExample
{
    [Fact]
    public async void TestHealthyMetrics()
    {
        var tests = new CanaryHealthEvaluatorTests();
        await tests.EvaluateAsync_MetricsUnderAllThresholds_ShouldBeHealthy();
    }
}
```

## ServiceExtensionsJsonExtensionsValidationTests

The `ServiceExtensionsJsonExtensionsValidationTests` class verifies validation of service-extension metadata, including required type, namespace, assembly, and methods values. It also checks the `Validate`, `IsValid`, and `EnsureValid` behaviors for valid metadata and representative invalid inputs.

Example usage:
```csharp
using DotNetDeployNotify.Tests;
using Xunit;

public class ServiceExtensionsValidationTestSuite
{
    private readonly ServiceExtensionsJsonExtensionsValidationTests _tests = new();

    [Fact]
    public void RunValidationChecks()
    {
        _tests.Validate_WithValidMetadata_ReturnsEmptyList();
        _tests.IsValid_WithNullType_ReturnsFalse();
        _tests.EnsureValid_WithNullType_ThrowsArgumentException();
    }
}
```
