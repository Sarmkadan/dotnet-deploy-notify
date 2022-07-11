# CanaryServiceExtensions

Provides extension methods on `IServiceCollection` for registering and replacing a canary deployment notification service in the `dotnet-deploy-notify` dependency injection container. These methods allow consumers to configure the canary deployment pipeline with different implementations or lifecycle options.

## API

### AddCanaryDeployment (three overloads)

```csharp
public static IServiceCollection AddCanaryDeployment(this IServiceCollection services)
public static IServiceCollection AddCanaryDeployment(this IServiceCollection services, Action<CanaryDeploymentOptions> configure)
public static IServiceCollection AddCanaryDeployment(this IServiceCollection services, Func<IServiceProvider, ICanaryDeploymentService> factory)
```

**Purpose**  
Registers the default canary deployment service and its dependencies into the service collection. The parameterless overload uses default options. The `Action<CanaryDeploymentOptions>` overload applies caller-supplied configuration. The factory overload accepts a custom delegate that resolves the `ICanaryDeploymentService` implementation from the service provider.

**Parameters**  
- `services` — The `IServiceCollection` to modify. Must not be null.  
- `configure` — A delegate that receives a `CanaryDeploymentOptions` instance for in-place configuration.  
- `factory` — A delegate that receives the `IServiceProvider` and returns an `ICanaryDeploymentService` instance.

**Return Value**  
The same `IServiceCollection` instance, enabling fluent chaining.

**Throws**  
- `ArgumentNullException` when `services` is null.  
- `ArgumentNullException` when `configure` or `factory` is null in the respective overloads.  
- May throw during service resolution if the factory delegate returns null or throws internally.

---

### ReplaceCanaryDeployment

```csharp
public static IServiceCollection ReplaceCanaryDeployment(this IServiceCollection services, Func<IServiceProvider, ICanaryDeploymentService> factory)
```

**Purpose**  
Removes any existing `ICanaryDeploymentService` registration and replaces it with a new one produced by the supplied factory. Useful when a previously registered canary service must be swapped out entirely without clearing the whole collection.

**Parameters**  
- `services` — The `IServiceCollection` to modify. Must not be null.  
- `factory` — A delegate that receives the `IServiceProvider` and returns the replacement `ICanaryDeploymentService`.

**Return Value**  
The same `IServiceCollection` instance, enabling fluent chaining.

**Throws**  
- `ArgumentNullException` when `services` or `factory` is null.  
- May throw during resolution if the factory returns null or encounters an error.

## Usage

### Example 1: Basic registration with default options

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCanaryDeployment();

var app = builder.Build();
// The default ICanaryDeploymentService is now available via DI
```

### Example 2: Custom factory registration followed by replacement

```csharp
var builder = WebApplication.CreateBuilder(args);

// Initial registration with a custom factory
builder.Services.AddCanaryDeployment(sp =>
{
    var logger = sp.GetRequiredService<ILogger<CustomCanaryService>>();
    return new CustomCanaryService(logger, TimeSpan.FromMinutes(5));
});

// Later, replace with a different implementation
builder.Services.ReplaceCanaryDeployment(sp =>
{
    var notifier = sp.GetRequiredService<IDeploymentNotifier>();
    return new AdvancedCanaryService(notifier);
});

var app = builder.Build();
```

## Notes

- All methods return the original `IServiceCollection` and are safe for fluent chaining with other extension methods.
- `ReplaceCanaryDeployment` removes all prior `ServiceDescriptor` entries for `ICanaryDeploymentService` before adding the new one. If multiple registrations exist, all are removed.
- These methods are not thread-safe. They should be called during application startup, before the service provider is built and before any concurrent access to the collection occurs.
- The factory overloads defer service instantiation until first resolution. If the factory throws or returns null, the exception surfaces at resolution time rather than registration time.
- Calling `ReplaceCanaryDeployment` without a prior `AddCanaryDeployment` registration is valid; it simply adds the new service descriptor.
