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

## CanaryServiceExtensionsTests

The `CanaryServiceExtensionsTests` class verifies the dependency-injection extensions that add or replace canary deployment services. It covers argument validation, options supplied directly or through configuration, default values, and registration of the required service lifetimes.

Example usage:
```csharp
using DotNetDeployNotify.Tests.Infrastructure;
using Xunit;

public class CanaryServiceExtensionsTestSuite
{
    private readonly CanaryServiceExtensionsTests _tests = new();

    [Fact]
    public void RunRegistrationChecks()
    {
        _tests.AddCanaryDeployment_WithServicesAndConfigure_ConfiguresOptions();
        _tests.AddCanaryDeployment_WithConfiguration_ConfiguresOptionsFromSection();
        _tests.ReplaceCanaryDeployment_RemovesExistingRegistrationsAndReRegisters();
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

## ServiceExtensionsJsonExtensionsTests

The `ServiceExtensionsJsonExtensionsTests` class verifies JSON serialization and deserialization for service-extension metadata, including indented and compact output, valid, partial, empty, invalid, and camel-case JSON, and the `TryFromJson` pattern. It also checks that metadata properties are nullable and that an empty methods array can be serialized and deserialized.

Example usage:
```csharp
using DotNetDeployNotify.Tests;
using Xunit;

public class ServiceExtensionsJsonTestSuite
{
    [Fact]
    public void RunJsonChecks()
    {
        var serializationTests = new ServiceExtensionsJsonExtensionsTests.ToJson();
        serializationTests.ToJson_ShouldSerializeMetadataWithCorrectProperties();
        serializationTests.ToJson_WithIndentedTrue_ShouldFormatJsonWithIndentation();

        var deserializationTests = new ServiceExtensionsJsonExtensionsTests.FromJson();
        deserializationTests.FromJson_WithValidJson_ShouldDeserializeCorrectly();

        var tryFromJsonTests = new ServiceExtensionsJsonExtensionsTests.TryFromJson();
        tryFromJsonTests.TryFromJson_WithInvalidJson_ShouldReturnFalseAndNullValue();

        var propertyTests =
            new ServiceExtensionsJsonExtensionsTests.ServiceExtensionsMetadataProperties();
        propertyTests.MethodsProperty_WithEmptyArray_ShouldSerializeAndDeserialize();
    }
}
```

## ServiceExtensionsValidationTests

The `ServiceExtensionsValidationTests` class verifies the validation extensions for `DeploymentNotification` and `NotificationResult`. It covers valid objects, null arguments, missing or invalid property values, and the aggregation of multiple validation errors.

Example usage:
```csharp
using DotNetDeployNotify.Tests;
using Xunit;

public class ServiceExtensionsValidationTestSuite
{
    private readonly ServiceExtensionsValidationTests _tests = new();

    [Fact]
    public void RunValidationChecks()
    {
        _tests.Validate_DeploymentNotification_WithAllValidProperties_ReturnsEmptyList();
        _tests.Validate_DeploymentNotification_WithMultipleProblems_ReturnsAllErrors();
        _tests.Validate_NotificationResult_WithAllValidProperties_ReturnsEmptyList();
        _tests.Validate_NotificationResult_WithEmptyNotificationId_ReturnsError();
    }
}
```
