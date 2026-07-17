# dotnet-deploy-notify

A .NET application for sending deployment notifications to various channels (Slack, Discord, Telegram, etc.).

## Features

- Send deployment notifications to multiple channels
- Support for Slack, Discord, and Telegram webhooks
- Batch notification processing
- Configurable channel strategies
- Integration with deployment pipelines

## Architecture

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the module breakdown, composition roots, data flow, extension points, and known limitations. Short version: a console app whose active wiring is `AddNotificationServices` (validation → in-memory repositories → webhook dispatch via a typed `HttpClient`); the event bus, middleware pipeline, background workers, and canary engine are optional opt-in subsystems.

## BatchNotification

The `BatchNotification` class represents a collection of notifications to be sent together, allowing for batch processing and improved delivery efficiency. It provides properties and methods to manage the batch's status, notifications, channels, and delivery results.

Example usage:
```csharp
var batchNotification = new BatchNotification
{
  Name = "Deployment Alerts",
  Description = "Alerts for deployment notifications",
  Notifications = new List<DeploymentNotification>
  {
    new DeploymentNotification { /* initialize notification properties */ },
    new DeploymentNotification { /* initialize notification properties */ }
  },
  Channels = new List<NotificationChannel>
  {
    new NotificationChannel { /* initialize channel properties */ }
  }
};

if (batchNotification.IsValid())
{
  Console.WriteLine($"Batch {batchNotification.Name} is valid.");
  // Process the batch
  batchNotification.MarkAsSent();
  Console.WriteLine($"Batch {batchNotification.Name} sent successfully. Success rate: {batchNotification.GetSuccessRate():F1}%");
}
else
{
  Console.WriteLine("Invalid batch notification.");
}
```

## IChannelStrategy

The `IChannelStrategy` interface defines the contract for channel-specific notification strategies. It enables polymorphic handling of different notification channels (Slack, Discord, Telegram, etc.) through a unified interface, allowing the system to send notifications to various channels without tight coupling to specific implementations.

Each strategy implementation provides channel-specific logic for determining support (`CanHandle`) and sending notifications (`SendAsync`).

Example usage:
```csharp
// Register strategies
var resolver = new ChannelStrategyResolver(logger);
resolver.RegisterStrategy(new SlackChannelStrategy(webhookClient, logger));
resolver.RegisterStrategy(new DiscordChannelStrategy(webhookClient, logger));
resolver.RegisterStrategy(new TelegramChannelStrategy(webhookClient, logger));

// Get appropriate strategy for a channel
var channel = NotificationChannel.Slack;
if (resolver.IsSupported(channel))
{
  var strategy = resolver.GetStrategy(channel);
  var result = await strategy.SendAsync(
    new DeploymentNotification { /* notification data */ },
    new ChannelConfiguration { WebhookUrl = "https://hooks.slack.com/..." },
    "{ \"text\": \"Deployment completed!\" }"
  );

  if (result)
  {
    Console.WriteLine("Notification sent successfully!");
  }
}
```

## ChannelStrategyResolver

The `ChannelStrategyResolver` class manages the registration and retrieval of channel strategies. It provides methods to register strategies, check support for channels, and retrieve strategies.

Example usage:
```csharp
var resolver = new ChannelStrategyResolver(logger);

// Register strategies
resolver.RegisterStrategy(new SlackChannelStrategy(webhookClient, slackLogger));
resolver.RegisterStrategy(new DiscordChannelStrategy(webhookClient, discordLogger));

// Check if a channel is supported
bool isSlackSupported = resolver.IsSupported(NotificationChannel.Slack);

// Get a strategy
var strategy = resolver.GetStrategy(NotificationChannel.Discord);

// Get all registered strategies
var allStrategies = resolver.GetAllStrategies();
```

## ChannelAdapter

The `ChannelAdapter` class provides backward compatibility and simplifies sending notifications through the channel system. It automatically handles payload building and strategy resolution.

Example usage:
```csharp
var adapter = new ChannelAdapter(resolver, payloadBuilderFactory, logger);

var config = new ChannelConfiguration
{
  ChannelType = NotificationChannel.Telegram,
  WebhookUrl = "https://api.telegram.org/bot..."
};

bool success = await adapter.SendAsync(
  new DeploymentNotification
  {
    ProjectName = "MyApp",
    Version = "1.0.0",
    Status = DeploymentStatus.Success
  },
  config
);

if (success)
{
  Console.WriteLine("Notification sent!");
}
```

## Result

The `Result<T>` type provides a functional way to handle operations that might fail, avoiding exceptions for expected control flow. It encapsulates both successful values and error messages, allowing for chaining operations like `Map` and `Bind` to create clean, expressive pipelines.

Example usage:
```csharp
// Simple usage
public Result<int> Divide(int numerator, int denominator)
{
  if (denominator == 0)
    return Result<int>.Fail("Cannot divide by zero.");

  return Result<int>.Ok(numerator / denominator);
}

// Chaining operations
var result = Divide(10, 2)
  .OnSuccess(val => Console.WriteLine($"Result: {val}"))
  .OnFailure(err => Console.WriteLine($"Error: {err}"));

if (result.IsSuccess)
{
  var value = result.GetValueOrThrow();
}
```

## ResultExtensions

The `ResultExtensions` class provides fluent extension methods for the `Result` and `Result<T>` types, enabling cleaner, more readable functional pipelines. It includes `Try` wrappers for safe exception handling and various composition methods like `Select`, `SelectMany`, `Where`, and `Do` for processing successful results while maintaining state.

Example usage:

```csharp
// Use Try to safely execute operations
Result<string> GetUserResult() => ResultExtensions.Try(() =>
  FetchUserFromDb() ?? throw new Exception("User not found"));

// Fluent composition with Select, Where, and Do
var result = GetUserResult()
  .Where(user => user.IsActive, "User is inactive")
  .Select(user => user.Email)
  .Do(email => Console.WriteLine($"Processing email: {email}"));

if (result.IsSuccess)
{
  Console.WriteLine($"Successfully processed: {result.Value}");
}
else
{
  Console.WriteLine($"Operation failed: {result.Error}");
}

// Combine multiple results
var results = new List<Result<int>> { Result<int>.Ok(1), Result<int>.Ok(2) };
Result<IReadOnlyList<int>> combined = results.Combine();
```

## ResultJsonExtensions

The `ResultJsonExtensions` class provides System.Text.Json serialization helpers for `Result` and `Result<T>` types. It enables converting result objects to and from JSON format, supporting configurable formatting options such as indentation for readability.

This extension class is useful for persisting functional operation results to configuration files, databases, or remote services, and for restoring them back into application memory. It provides serialization (`ToJson`), deserialization (`FromJson`), and safe deserialization with error handling (`TryFromJson`).

Example usage:
```csharp
// Create a successful result
var result = Result<string>.Ok("Operation completed successfully");

// Serialize to JSON string (compact format)
string jsonCompact = result.ToJson();
Console.WriteLine(jsonCompact);
// Output: {"value":"Operation completed successfully","isSuccess":true}

// Serialize to JSON string (indented format)
string jsonIndented = result.ToJson(indented: true);
Console.WriteLine(jsonIndented);
/* Output:
{
  "value": "Operation completed successfully",
  "isSuccess": true
}
*/

// Deserialize from JSON string
var deserializedResult = ResultJsonExtensions.FromJson<string>(jsonCompact);
if (deserializedResult != null && deserializedResult.IsSuccess)
{
  Console.WriteLine($"Deserialized value: {deserializedResult.Value}");
}

// Try deserialization with error handling
if (ResultJsonExtensions.TryFromJson<string>(jsonCompact, out var resultValue))
{
  Console.WriteLine("Successfully deserialized result");
}
else
{
  Console.WriteLine("Failed to deserialize result");
}
```

## StringExtensionsJsonExtensions

The `StringExtensionsJsonExtensions` class provides System.Text.Json serialization helpers for `StringExtensions` metadata. It enables converting string extension type information to and from JSON format with configurable formatting options, supporting both compact and indented output formats.

This extension class is particularly useful for persisting string extension configuration metadata to configuration files, databases, or remote services, and for restoring it back into application memory. It provides three main methods: `ToJson()` for serialization, `FromJson()` for deserialization, and `TryFromJson()` for safe deserialization with error handling.

Example usage:

```csharp
// Serialize to JSON string (compact format)
string jsonCompact = StringExtensionsJsonExtensions.ToJson();
Console.WriteLine(jsonCompact);
// Output: {"type":"StringExtensions","namespace":"DotNetDeployNotify.Utilities","assembly":"DotNetDeployNotify","methods":["IsBase64","IsGuid","IsNumeric","ToBase64","ToGuid","TrimToLength"]}

// Serialize to JSON string (indented format)
string jsonIndented = StringExtensionsJsonExtensions.ToJson(indented: true);
Console.WriteLine(jsonIndented);
/* Output:
{
  "type": "StringExtensions",
  "namespace": "DotNetDeployNotify.Utilities",
  "assembly": "DotNetDeployNotify",
  "methods": [
    "IsBase64",
    "IsGuid",
    "IsNumeric",
    "ToBase64",
    "ToGuid",
    "TrimToLength"
  ]
}
*/

// Deserialize from JSON string
var deserializedMetadata = StringExtensionsJsonExtensions.FromJson(jsonCompact);
if (deserializedMetadata != null)
{
  Console.WriteLine($"Deserialized type: {deserializedMetadata.Type}");
  Console.WriteLine($"Namespace: {deserializedMetadata.Namespace}");
  Console.WriteLine($"Assembly: {deserializedMetadata.Assembly}");
  Console.WriteLine($"Methods count: {deserializedMetadata.Methods?.Length ?? 0}");
}

// Try deserialization with error handling
if (StringExtensionsJsonExtensions.TryFromJson(jsonCompact, out var result))
{
  Console.WriteLine("Successfully deserialized metadata");
}
else
{
  Console.WriteLine("Failed to deserialize metadata");
}
```

## TypeHelperJsonExtensions

The `TypeHelperJsonExtensions` class provides System.Text.Json serialization helpers for the `TypeHelper` type information. It enables converting type metadata to and from JSON format with configurable formatting options, supporting both compact and indented output formats.

This extension class is particularly useful for persisting type information to configuration files, databases, or remote services, and for restoring it back into application memory. It provides three main methods: `ToJson()` for serialization, `FromJson()` for deserialization, and `TryFromJson()` for safe deserialization with error handling.

Example usage:

```csharp
// Serialize TypeHelper metadata to JSON string (compact format)
string jsonCompact = TypeHelperJsonExtensions.ToJson();
Console.WriteLine(jsonCompact);
// Output: {"type":"TypeHelper","namespace":"DotNetDeployNotify.Utilities","assembly":"DotNetDeployNotify","methods":["GetPublicStaticMethodNames","ToJson","FromJson","TryFromJson"]}

// Serialize to JSON string (indented format)
string jsonIndented = TypeHelperJsonExtensions.ToJson(indented: true);
Console.WriteLine(jsonIndented);
/* Output:
{
  "type": "TypeHelper",
  "namespace": "DotNetDeployNotify.Utilities",
  "assembly": "DotNetDeployNotify",
  "methods": [
    "GetPublicStaticMethodNames",
    "ToJson",
    "FromJson",
    "TryFromJson"
  ]
}
*/

// Deserialize from JSON string
var deserializedMetadata = TypeHelperJsonExtensions.FromJson(jsonCompact);
if (deserializedMetadata != null)
{
  Console.WriteLine($"Deserialized type: {deserializedMetadata.Type}");
  Console.WriteLine($"Namespace: {deserializedMetadata.Namespace}");
  Console.WriteLine($"Assembly: {deserializedMetadata.Assembly}");
  Console.WriteLine($"Methods count: {deserializedMetadata.Methods?.Length ?? 0}");
}

// Try deserialization with error handling
if (TypeHelperJsonExtensions.TryFromJson(jsonCompact, out var result))
{
  Console.WriteLine("Successfully deserialized metadata");
}
else
{
  Console.WriteLine("Failed to deserialize metadata");
}
```

## GuardExtensionsValidation

The `GuardExtensionsValidation` class provides comprehensive validation helpers for common types with detailed error reporting capabilities. It includes extension methods for validating objects, strings, collections, boolean conditions, integers, URLs, and regular expressions. Each validation method returns an `IReadOnlyList<string>` containing human-readable error messages, making it easy to collect and report multiple validation problems at once.

The class also provides convenience methods (`IsValid` family) for quick validation checks that return boolean values, and `EnsureValid` methods that throw exceptions when validation fails.

Example usage:

```csharp
var validationResults = myObject.ValidateObject(nameof(myObject));
if (validationResults.Any())
{
    foreach (var error in validationResults)
    {
        Console.WriteLine(error);
    }
}

var stringErrors = "".ValidateString(nameof(myString));
if (stringErrors.Any())
{
    Console.WriteLine(stringErrors.First());
}
```

## ValidationRuleValidation

The `ValidationRuleValidation` class provides validation extension methods for `ValidationRule<T>` types, enabling comprehensive validation of validation rules themselves. It supports validating individual rules, checking validity, ensuring validation passes, and bulk validation of multiple rules. The class provides both detailed error reporting (returning collections of error messages) and convenience methods for quick boolean checks.

Example usage:

```csharp
// Define validation rules for a type
var projectNameRule = new ValidationRule<DeploymentNotification>(
    nameof(DeploymentNotification.ProjectName),
    notification => !string.IsNullOrWhiteSpace(notification.ProjectName),
    "Project name is required");

var versionRule = new ValidationRule<DeploymentNotification>(
    nameof(DeploymentNotification.Version),
    notification => !string.IsNullOrWhiteSpace(notification.Version),
    "Version is required");

var statusRule = new ValidationRule<DeploymentNotification>(
    nameof(DeploymentNotification.Status),
    notification => notification.Status != null,
    "Status must be specified");

// Validate a single rule
var problems = projectNameRule.Validate();
if (problems.Any())
{
    foreach (var error in problems)
    {
        Console.WriteLine(error);
    }
}

// Quick validation check
if (projectNameRule.IsValid())
{
    Console.WriteLine("Project name rule is valid");
}

// Ensure validation passes (throws if invalid)
projectNameRule.EnsureValid();

// Bulk validation of multiple rules
var rules = new List<ValidationRule<DeploymentNotification>> { projectNameRule, versionRule, statusRule };
var allErrors = rules.ValidateAll();
foreach (var kvp in allErrors)
{
    Console.WriteLine($"Rule '{kvp.Key.GetErrorMessage()}': {string.Join(", ", kvp.Value)}");
}

// Check if all rules are valid
if (rules.AllValid())
{
    Console.WriteLine("All validation rules are valid");
}

// Get the first problem for a rule
var firstProblem = projectNameRule.GetFirstProblem();
if (firstProblem != null)
{
    Console.WriteLine($"First validation problem: {firstProblem}");
}
```

## RetryPolicyExtensions

The `RetryPolicyExtensions` class provides extension methods for the `RetryPolicy` type to simplify retry operations. It includes factory methods for creating retry policies with common patterns (immediate retries, exponential backoff), delay calculation methods with and without jitter, and helper methods for executing operations with retry logic. The extensions support both synchronous and asynchronous operations, custom retry conditions, and policy validation.

Example usage:

```csharp
// Create a retry policy with exponential backoff
var retryPolicy = RetryPolicyExtensions.WithExponentialBackoff(
    maxAttempts: 5,
    initialDelay: TimeSpan.FromSeconds(1),
    backoffMultiplier: 2.0,
    maxDelay: TimeSpan.FromSeconds(30)
);

Console.WriteLine(retryPolicy.FormatConfiguration());

// Execute an operation with retry logic
var result = await retryPolicy.ExecuteAsync(async () =>
{
    var response = await httpClient.GetAsync("https://api.example.com/data");
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync();
});

// Get retry delays for monitoring
var delays = retryPolicy.GetRetryDelaysWithJitter();
foreach (var delay in delays)
{
    Console.WriteLine($"Retry delay: {delay.TotalMilliseconds}ms");
}

// Create a policy with custom retry condition
var customPolicy = RetryPolicyExtensions.WithCustomRetryCondition(
    maxAttempts: 3,
    initialDelay: TimeSpan.FromMilliseconds(200),
    shouldRetry: ex => ex is HttpRequestException or TimeoutException
);

// Validate policy configuration
if (retryPolicy.IsValid())
{
    Console.WriteLine("Retry policy is valid");
}
```

## ValidationRuleJsonExtensions

The `ValidationRuleJsonExtensions` class provides System.Text.Json serialization and deserialization helpers for `ValidationRule` types. It enables converting validation rules to and from JSON format with configurable formatting options, supporting both compact and indented output formats.

This extension class is particularly useful for persisting validation rule configuration to configuration files, databases, or remote services, and for restoring them back into application memory. It provides serialization methods for string and numeric validation rules, along with deserialization and safe deserialization methods.

Example usage:

```csharp
// Create a string validation rule (NotEmptyRule)
var notEmptyRule = new NotEmptyRule { ErrorMessage = "Field cannot be empty" };

// Serialize to JSON string (compact format)
string jsonCompact = notEmptyRule.ToJson();
Console.WriteLine(jsonCompact);
// Output: {"errorMessage":"Field cannot be empty","type":"NotEmpty"}

// Serialize to JSON string (indented format)
string jsonIndented = notEmptyRule.ToJson(indented: true);
Console.WriteLine(jsonIndented);
/* Output:
{
  "errorMessage": "Field cannot be empty",
  "type": "NotEmpty"
}
*/

// Create a numeric validation rule (RangeRule)
var rangeRule = new RangeRule { Min = 1, Max = 100, ErrorMessage = "Value must be between 1 and 100" };

// Serialize numeric validation rule
string rangeJson = rangeRule.ToJson();
Console.WriteLine(rangeJson);
// Output: {"min":1,"max":100,"errorMessage":"Value must be between 1 and 100","type":"Range"}

// Deserialize string validation rule from JSON
string ruleJson = "{\"errorMessage\":\"Must not be empty\",\"type\":\"NotEmpty\"}";
var deserializedRule = ValidationRuleJsonExtensions.FromJsonString(ruleJson);
Console.WriteLine(deserializedRule.ErrorMessage); // Output: Must not be empty

// Deserialize numeric validation rule from JSON
string rangeRuleJson = "{\"min\":10,\"max\":100,\"errorMessage\":\"Must be between 10 and 100\",\"type\":\"Range\"}";
var deserializedRangeRule = ValidationRuleJsonExtensions.FromJsonInt(rangeRuleJson);
Console.WriteLine($"Range: {deserializedRangeRule.Min}-{deserializedRangeRule.Max}");

// Try deserialization with error handling
if (ValidationRuleJsonExtensions.TryFromJson(ruleJson, out var safeRule))
{
    Console.WriteLine("Successfully deserialized validation rule");
}
else
{
    Console.WriteLine("Failed to deserialize validation rule");
}
```

Example usage:

```csharp
// Serialize MathExtensions metadata to JSON string (compact format)
string jsonCompact = MathExtensionsJsonExtensions.ToJson();
Console.WriteLine(jsonCompact);
// Output: {"type":"MathExtensions","namespace":"DotNetDeployNotify.Utilities","assembly":"DotNetDeployNotify","methods":["Clamp","IsBetween","ToPercentage","RoundTo","Average","Median","SafeSum","SafeAverage","ToHumanReadableSize","ToHumanReadableDuration","CalculateCompoundInterest","RandomBetween"]}

// Serialize to JSON string (indented format)
string jsonIndented = MathExtensionsJsonExtensions.ToJson(indented: true);
Console.WriteLine(jsonIndented);
/* Output:
{
  "type": "MathExtensions",
  "namespace": "DotNetDeployNotify.Utilities",
  "assembly": "DotNetDeployNotify",
  "methods": [
    "Clamp",
    "IsBetween",
    "ToPercentage",
    "RoundTo",
    "Average",
    "Median",
    "SafeSum",
    "SafeAverage",
    "ToHumanReadableSize",
    "ToHumanReadableDuration",
    "CalculateCompoundInterest",
    "RandomBetween"
  ]
}
*/

// Deserialize from JSON string
var deserializedMetadata = MathExtensionsJsonExtensions.FromJson(jsonCompact);
if (deserializedMetadata != null)
{
    Console.WriteLine($"Deserialized type: {deserializedMetadata.Type}");
    Console.WriteLine($"Namespace: {deserializedMetadata.Namespace}");
    Console.WriteLine($"Assembly: {deserializedMetadata.Assembly}");
    Console.WriteLine($"Methods count: {deserializedMetadata.Methods?.Length ?? 0}");
}

// Try deserialization with error handling
if (MathExtensionsJsonExtensions.TryFromJson(jsonCompact, out var result))
{
    Console.WriteLine("Successfully deserialized metadata");
}
else
{
    Console.WriteLine("Failed to deserialize metadata");
}
```

## MathExtensionsValidation

The `MathExtensionsValidation` class provides mathematical validation and calculation utilities with detailed error reporting. It includes methods for validating numeric ranges, clamping values, calculating percentages, rounding numbers, computing statistical measures (average, median), performing safe arithmetic operations, and converting between numeric representations. Each validation method returns an `IReadOnlyList<string>` containing human-readable error messages, enabling comprehensive validation of mathematical operations.

The class also provides convenience methods (`IsValid` family) for quick validation checks that return boolean values, and `EnsureValid` methods that throw exceptions when validation fails.

Example usage:

```csharp
// Validate that a value is within a specific range
var rangeErrors = 15.ValidateIsBetween(10, 20, nameof(value));
if (rangeErrors.Any())
{
    Console.WriteLine(rangeErrors.First()); // "value must be between 10 and 20 (inclusive), but was 15"
}

// Validate and clamp a value to a range
var clampedValue = 25.Clamp(0, 100);
Console.WriteLine(clampedValue); // Output: 100

// Validate percentage conversion
var percentageErrors = 1.25.ValidateToPercentage(nameof(ratio));
if (percentageErrors.Any())
{
    Console.WriteLine(percentageErrors.First());
}
else
{
    Console.WriteLine($"Percentage: {1.25.ToPercentage()}%"); // Output: Percentage: 125%
}

// Validate rounding to a specific decimal place
var roundErrors = 3.14159.ValidateRoundTo(2, nameof(pi));
if (roundErrors.Any())
{
    Console.WriteLine(roundErrors.First());
}
else
{
    Console.WriteLine($"Rounded: {3.14159.RoundTo(2)}"); // Output: Rounded: 3.14
}

// Validate safe arithmetic operations
var sumErrors = (5, 10).ValidateSafeSum(nameof(values));
if (sumErrors.Any())
{
    Console.WriteLine(sumErrors.First());
}
else
{
    Console.WriteLine($"Sum: {(5, 10).SafeSum()}"); // Output: Sum: 15
}

// Validate statistical calculations
var statsErrors = new[] { 10, 20, 30, 40, 50 }.ValidateAverage(nameof(data));
if (statsErrors.Any())
{
    Console.WriteLine(statsErrors.First());
}
else
{
    Console.WriteLine($"Average: {new[] { 10, 20, 30, 40, 50 }.Average()}"); // Output: Average: 30
}

// Validate human-readable size conversion
var sizeErrors = 1024.ValidateToHumanReadableSize(nameof(bytes));
if (sizeErrors.Any())
{
    Console.WriteLine(sizeErrors.First());
}
else
{
    Console.WriteLine($"Size: {1024.ToHumanReadableSize()}"); // Output: Size: 1.00 KB
}

// Quick validation checks
if (!15.IsValidIsBetween(10, 20))
{
    Console.WriteLine("Value is not between 10 and 20");
}

// Exception-throwing validation
try
{
    5.EnsureValidIsBetween(10, 20, nameof(value));
}
catch (ArgumentException ex)
{
    Console.WriteLine(ex.Message);
}
```

[...previous content...]

## ObjectExtensionsJsonExtensions

The `ObjectExtensionsJsonExtensions` class provides System.Text.Json serialization helpers for `ObjectExtensions` metadata. It enables converting object extension type information to and from JSON format with configurable formatting options, supporting both compact and indented output formats.


This extension class is particularly useful for persisting object extension configuration metadata to configuration files, databases, or remote services, and for restoring it back into application memory. It provides three main methods: `ToJson()` for serialization, `FromJson()` for deserialization, and `TryFromJson()` for safe deserialization with error handling.

Example usage:

```csharp
// Serialize to JSON string (compact format)
string jsonCompact = ObjectExtensionsJsonExtensions.ToJson();
Console.WriteLine(jsonCompact);
// Output: {"type":"ObjectExtensions","namespace":"DotNetDeployNotify.Utilities","assembly":"DotNetDeployNotify","methods":["SafeCast","IsNull","IsNotNull","IfNotNull","Map","ShallowCopy","GetPropertyValue","SetPropertyValue","ToDictionary","EqualsAny","IsDefault","GetValueOrDefault","ToStringSafe","GetTypeName","GetFullTypeName","Chain","Validate"]}

// Serialize to JSON string (indented format)
string jsonIndented = ObjectExtensionsJsonExtensions.ToJson(indented: true);
Console.WriteLine(jsonIndented);
/* Output:
{
  "type": "ObjectExtensions",
  "namespace": "DotNetDeployNotify.Utilities",
  "assembly": "DotNetDeployNotify",
  "methods": [
    "SafeCast",
    "IsNull",
    "IsNotNull",
    "IfNotNull",
    "Map",
    "ShallowCopy",
    "GetPropertyValue",
    "SetPropertyValue",
    "ToDictionary",
    "EqualsAny",
    "IsDefault",
    "GetValueOrDefault",
    "ToStringSafe",
    "GetTypeName",
    "GetFullTypeName",
    "Chain",
    "Validate"
  ]
}
*/

// Deserialize from JSON string
var deserializedMetadata = ObjectExtensionsJsonExtensions.FromJson(jsonCompact);
if (deserializedMetadata != null)
{
    Console.WriteLine($"Deserialized type: {deserializedMetadata.Type}");
    Console.WriteLine($"Namespace: {deserializedMetadata.Namespace}");
    Console.WriteLine($"Assembly: {deserializedMetadata.Assembly}");
    Console.WriteLine($"Methods count: {deserializedMetadata.Methods?.Length ?? 0}");
}

// Try deserialization with error handling
if (ObjectExtensionsJsonExtensions.TryFromJson(jsonCompact, out var result))
{
    Console.WriteLine("Successfully deserialized metadata");
}
else
{
    Console.WriteLine("Failed to deserialize metadata");
}
```

## CollectionExtensionsJsonExtensions

The `CollectionExtensionsJsonExtensions` class provides System.Text.Json serialization helpers for collection metadata. It enables converting collection extension type information to and from JSON format with configurable formatting options, supporting both compact and indented output formats.

This extension class is particularly useful for persisting collection extension configuration metadata to configuration files, databases, or remote services, and for restoring it back into application memory. It provides three main methods: `ToJson()` for serialization, `FromJson()` for deserialization, and `TryFromJson()` for safe deserialization with error handling.

Example usage:

```csharp
// Serialize to JSON string (compact format)
string jsonCompact = CollectionExtensionsJsonExtensions.ToJson();
Console.WriteLine(jsonCompact);
// Output: {"type":"CollectionExtensions","namespace":"DotNetDeployNotify.Utilities","assembly":"DotNetDeployNotify","methods":["IsNullOrEmpty","ToReadOnlyCollection","AddRange"]}

// Serialize to JSON string (indented format)
string jsonIndented = CollectionExtensionsJsonExtensions.ToJson(indented: true);
Console.WriteLine(jsonIndented);
/* Output:
{
  "type": "CollectionExtensions",
  "namespace": "DotNetDeployNotify.Utilities",
  "assembly": "DotNetDeployNotify",
  "methods": [
    "IsNullOrEmpty",
    "ToReadOnlyCollection",
    "AddRange"
  ]
}
*/

// Deserialize from JSON string
var deserializedMetadata = CollectionExtensionsJsonExtensions.FromJson(jsonCompact);
if (deserializedMetadata != null)
{
    Console.WriteLine($"Deserialized type: {deserializedMetadata.Type}");
    Console.WriteLine($"Namespace: {deserializedMetadata.Namespace}");
    Console.WriteLine($"Assembly: {deserializedMetadata.Assembly}");
    Console.WriteLine($"Methods count: {deserializedMetadata.Methods?.Length ?? 0}");
}

// Try deserialization with error handling
if (CollectionExtensionsJsonExtensions.TryFromJson(jsonCompact, out var result))
{
    Console.WriteLine("Successfully deserialized metadata");
}
else
{
    Console.WriteLine("Failed to deserialize metadata");
}
```

```csharp
// Validate an object reference
var validationResults = myObject.ValidateObject(nameof(myObject));
if (validationResults.Any())
{
    foreach (var error in validationResults)
    {
        Console.WriteLine(error);
    }
}

// Validate a string
var stringErrors = "".ValidateString(nameof(myString));
if (stringErrors.Any())
{
    Console.WriteLine(stringErrors.First()); // "myString cannot be null or empty"
}

// Validate a collection
var collection = new List<string>();
var collectionErrors = collection.ValidateCollection(nameof(myCollection));
if (collectionErrors.Any())
{
    Console.WriteLine(collectionErrors.First()); // "myCollection cannot be empty"
}

// Validate a condition
var conditionErrors = false.ValidateCondition(nameof(condition), "Condition must be true");
if (conditionErrors.Any())
{
    Console.WriteLine(conditionErrors.First()); // "Condition must be true"
}

// Validate an integer minimum
var minErrors = 5.ValidateMinimum(10, nameof(value));
if (minErrors.Any())
{
    Console.WriteLine(minErrors.First()); // "value must be at least 10, but was 5"
}

// Validate a string maximum length
var maxLengthErrors = "toolongstring".ValidateMaxLength(5, nameof(myString));
if (maxLengthErrors.Any())
{
    Console.WriteLine(maxLengthErrors.First()); // "myString cannot be longer than 5 characters, but was 12"
}

// Validate a URL
var urlErrors = "invalidurl".ValidateUrl(nameof(myUrl));
if (urlErrors.Any())
{
    Console.WriteLine(urlErrors.First()); // "myUrl is not a valid URL"
}

// Validate a nullable reference
var nullErrors = ((string)null).ValidateNotNull<string>(nameof(myNullable));
if (nullErrors.Any())
{
    Console.WriteLine(nullErrors.First()); // "myNullable cannot be null"
}

// Validate a range
var rangeErrors = 15.ValidateRange(10, 20, nameof(value));
if (rangeErrors.Any())
{
    Console.WriteLine(rangeErrors.First()); // "value must be between 10 and 20 (inclusive), but was 15"
}

// Validate against a regular expression pattern
var patternErrors = "invalid123".ValidatePattern("^[A-Za-z]+", nameof(myString));
if (patternErrors.Any())
{
    Console.WriteLine(patternErrors.First()); // "myString does not match the required pattern"
}

// Quick boolean validation checks
if (!myObject.IsValid())
{
    Console.WriteLine("Object is null");
}

if (!myString.IsValid())
{
    Console.WriteLine("String is null or whitespace");
}

if (!myCollection.IsValid())
{
    Console.WriteLine("Collection is null or empty");
}

if (!myUrl.IsValidUrl())
{
    Console.WriteLine("URL is invalid");
}

// Exception-throwing validation
try
{
    myNullable.EnsureValidNotNull(nameof(myNullable));
    myUrl.EnsureValidUrl(nameof(myUrl));
    5.EnsureValidMinimum(10, nameof(value));
}
catch (ArgumentException ex)
{
    Console.WriteLine(ex.Message);
}
## NotificationServiceTestsExtensions

The `NotificationServiceTestsExtensions` class provides extension methods for `NotificationServiceTests` to simplify unit test creation. It includes factory methods for creating test objects (`DeploymentNotification`, `ChannelConfiguration`, `NotificationResult`) and verification helpers for asserting repository interactions. These extensions make tests more readable and maintainable by encapsulating common test setup patterns.

Example usage:

```csharp
// Create test data
var notification = this.CreateTestNotification(
    projectName: "MyWebApp",
    version: "2.1.0",
    environment: Environment.Production,
    status: BuildStatus.Success
);

var channelConfig = this.CreateTestChannelConfiguration(
    channelType: NotificationChannel.Slack,
    webhookUrl: "https://hooks.slack.com/services/T/B/X",
    displayName: "Production Slack"
);

// Create expected result
var successResult = this.CreateSuccessfulResult(
    notificationId: notification.Id,
    channel: NotificationChannel.Slack,
    configurationId: channelConfig.Id
);

// Verify repository interactions
var mockRepo = new Mock<INotificationRepository>();
this.VerifyNotificationCreated(mockRepo, notification);

var mockResultRepo = new Mock<INotificationResultRepository>();
this.VerifyResultCreated(mockResultRepo, successResult);

// Setup mocks
var mockValidation = new Mock<IValidationService>();
this.SetupValidationResult(mockValidation, isValid: true);

var mockChannelRepo = new Mock<IChannelConfigRepository>();
this.SetupChannelConfigurations(mockChannelRepo, new[] { channelConfig });

var mockDispatcher = new Mock<IWebhookDispatcher>();
this.SetupWebhookDispatch(mockDispatcher, successResult);
```

## ChannelPayloadTests

The `ChannelPayloadTests` class verifies per-channel payload formatting by driving the real `WebhookDispatcher` and `PayloadBuilder` through a `FakeWebhookTransport` and asserting on the captured wire payload. These tests ensure that notifications sent to different channels (Slack, Discord, Telegram, generic webhooks) are properly formatted according to each platform's requirements.

Example usage:

```csharp
// Test Slack payload with attachment block and status color
[Fact]
public async Task Slack_payload_contains_attachment_block_with_status_color()
{
    var transport = new FakeWebhookTransport();
    var dispatcher = CreateDispatcher(transport);

    var result = await dispatcher.SendToWebhookAsync(
        Config(NotificationChannel.Slack, "https://hooks.slack.com/services/T/B/X"),
        SampleNotification());

    result.IsSuccessful.Should().BeTrue();
    
    // Verify Slack-specific formatting
    var data = ParseData(transport.LastRequest!.Body);
    var slack = data.GetProperty("CustomProperties").GetProperty("slack_format");
    var attachment = slack.GetProperty("attachments")[0];
    
    attachment.GetProperty("color").GetString().Should().Be("#00ff00"); // Success color
    attachment.GetProperty("title").GetString().Should().Contain("Checkout.Api v3.2.1");
}

// Test Slack Block Kit payload
[Fact]
public async Task Slack_block_kit_payload_uses_blocks_when_enabled()
{
    var transport = new FakeWebhookTransport();
    var dispatcher = CreateDispatcher(transport);
    var config = Config(NotificationChannel.Slack, "https://hooks.slack.com/services/T/B/X");
    config.UseSlackBlockKit = true;

    await dispatcher.SendToWebhookAsync(config, SampleNotification());

    var data = ParseData(transport.LastRequest!.Body);
    var slack = data.GetProperty("CustomProperties").GetProperty("slack_format");
    var firstBlock = slack.GetProperty("blocks")[0];

    firstBlock.GetProperty("type").GetString().Should().Be("header");
}

// Test Telegram payload with HTML formatting
[Fact]
public async Task Telegram_payload_carries_html_formatted_text()
{
    var transport = new FakeWebhookTransport();
    var dispatcher = CreateDispatcher(transport);

    await dispatcher.SendToWebhookAsync(
        Config(NotificationChannel.Telegram, "https://api.telegram.org/bot123/sendMessage"),
        SampleNotification());

    var data = ParseData(transport.LastRequest!.Body);
    var text = data.GetProperty("CustomProperties").GetProperty("telegram_text").GetString();

    text.Should().Contain("<b>Checkout.Api</b> v3.2.1");
    text.Should().Contain("<b>Status:</b>");
}

// Test generic webhook payload mapping
[Fact]
public async Task Generic_webhook_payload_maps_notification_fields()
{
    var transport = new FakeWebhookTransport();
    var dispatcher = CreateDispatcher(transport);

    await dispatcher.SendToWebhookAsync(
        Config(NotificationChannel.Webhook, "https://example.com/hooks/deploy"),
        SampleNotification());

    var data = ParseData(transport.LastRequest!.Body);
    
    data.GetProperty("ProjectName").GetString().Should().Be("Checkout.Api");
    data.GetProperty("Version").GetString().Should().Be("3.2.1");
    data.GetProperty("Status").GetString().Should().Be("DeploymentSuccess");
    data.GetProperty("Environment").GetString().Should().Be("Production");
}

// Test custom headers forwarding
[Fact]
public async Task Custom_headers_are_forwarded_to_transport()
{
    var transport = new FakeWebhookTransport();
    var dispatcher = CreateDispatcher(transport);
    var config = Config(NotificationChannel.Webhook, "https://example.com/hooks/deploy");
    config.CustomHeaders["X-Deploy-Token"] = "s3cr3t";

    await dispatcher.SendToWebhookAsync(config, SampleNotification());

    transport.LastRequest!.Headers.Should().ContainKey("X-Deploy-Token");
    transport.LastRequest.Headers["X-Deploy-Token"].Should().Be("s3cr3t");
}

// Test failed status mapping
[Fact]
public async Task Failed_status_maps_to_red_slack_color()
{
    var transport = new FakeWebhookTransport();
    var dispatcher = CreateDispatcher(transport);

    await dispatcher.SendToWebhookAsync(
        Config(NotificationChannel.Slack, "https://hooks.slack.com/services/T/B/X"),
        SampleNotification(BuildStatus.DeploymentFailed));

    var data = ParseData(transport.LastRequest!.Body);
    var color = data.GetProperty("CustomProperties")
        .GetProperty("slack_format")
        .GetProperty("attachments")[0]
        .GetProperty("color").GetString();

    color.Should().Be("#ff0000"); // Failed color
}

// Test HTTP response handling
[Fact]
public async Task Non_success_http_response_is_reported_as_failure()
{
    var transport = new FakeWebhookTransport(HttpStatusCode.InternalServerError, "boom");
    var dispatcher = CreateDispatcher(transport);

    var result = await dispatcher.SendToWebhookAsync(
        Config(NotificationChannel.Slack, "https://hooks.slack.com/services/T/B/X"),
        SampleNotification());

    result.IsSuccessful.Should().BeFalse();
    result.HttpStatusCode.Should().Be(500);
}
```
