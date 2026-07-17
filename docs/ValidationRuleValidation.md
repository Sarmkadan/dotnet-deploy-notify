# ValidationRuleValidation

Provides static methods for validating objects against a set of `ValidationRule<T>` instances. Each method operates on a collection of rules and an object of type `T`, returning validation problems as strings, checking validity, or throwing on failure. The class is designed to be stateless and reusable across different validation scenarios.

## API

### `Validate<T>`
```csharp
public static IReadOnlyList<string> Validate<T>(T obj, params ValidationRule<T>[] rules)
```
**Purpose:** Evaluates all specified rules against the given object and returns a list of problem descriptions.  
**Parameters:**  
- `obj` – The object to validate.  
- `rules` – One or more `ValidationRule<T>` instances to apply.  
**Returns:** A read-only list of strings, each describing a validation failure. If no rules fail, the list is empty.  
**Throws:** Nothing.

### `IsValid<T>`
```csharp
public static bool IsValid<T>(T obj, params ValidationRule<T>[] rules)
```
**Purpose:** Determines whether the object passes all specified rules.  
**Parameters:**  
- `obj` – The object to validate.  
- `rules` – One or more `ValidationRule<T>` instances.  
**Returns:** `true` if every rule passes; otherwise `false`.  
**Throws:** Nothing.

### `EnsureValid<T>`
```csharp
public static void EnsureValid<T>(T obj, params ValidationRule<T>[] rules)
```
**Purpose:** Validates the object and throws an exception if any rule fails.  
**Parameters:**  
- `obj` – The object to validate.  
- `rules` – One or more `ValidationRule<T>` instances.  
**Returns:** Nothing.  
**Throws:** An exception if any validation rule fails. The exact exception type is implementation-defined (typically `InvalidOperationException` or a custom validation exception).

### `ValidateAll<T>`
```csharp
public static IReadOnlyDictionary<ValidationRule<T>, IReadOnlyList<string>> ValidateAll<T>(T obj, params ValidationRule<T>[] rules)
```
**Purpose:** Evaluates each rule independently and returns a dictionary mapping each rule to its list of problems.  
**Parameters:**  
- `obj` – The object to validate.  
- `rules` – One or more `ValidationRule<T>` instances.  
**Returns:** A read-only dictionary where each key is a `ValidationRule<T>` and the value is a read-only list of problem strings for that rule. Rules that pass have an empty list.  
**Throws:** Nothing.

### `AllValid<T>`
```csharp
public static bool AllValid<T>(T obj, params ValidationRule<T>[] rules)
```
**Purpose:** Checks whether all specified rules pass for the object.  
**Parameters:**  
- `obj` – The object to validate.  
- `rules` – One or more `ValidationRule<T>` instances.  
**Returns:** `true` if every rule passes; otherwise `false`.  
**Throws:** Nothing.

### `GetFirstProblem<T>`
```csharp
public static string? GetFirstProblem<T>(T obj, params ValidationRule<T>[] rules)
```
**Purpose:** Returns the first validation problem encountered, or `null` if all rules pass.  
**Parameters:**  
- `obj` – The object to validate.  
- `rules` – One or more `ValidationRule<T>` instances.  
**Returns:** A string describing the first failure, or `null` if no failures occur.  
**Throws:** Nothing.

## Usage

### Example 1: Simple validation with `Validate` and `IsValid`

```csharp
using System.Collections.Generic;
using static dotnet_deploy_notify.ValidationRuleValidation;

public class User
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public static class UserRules
{
    public static readonly ValidationRule<User> NameNotEmpty = 
        new ValidationRule<User>(u => string.IsNullOrWhiteSpace(u.Name) ? "Name is required" : null);

    public static readonly ValidationRule<User> AgePositive = 
        new ValidationRule<User>(u => u.Age <= 0 ? "Age must be positive" : null);
}

// Usage
var user = new User { Name = "", Age = -5 };
IReadOnlyList<string> problems = Validate(user, UserRules.NameNotEmpty, UserRules.AgePositive);
// problems = ["Name is required", "Age must be positive"]

bool valid = IsValid(user, UserRules.NameNotEmpty, UserRules.AgePositive);
// valid = false
```

### Example 2: Detailed diagnostics with `ValidateAll` and `EnsureValid`

```csharp
using System;
using System.Linq;
using static dotnet_deploy_notify.ValidationRuleValidation;

public class Order
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
}

public static class OrderRules
{
    public static readonly ValidationRule<Order> IdPositive = 
        new ValidationRule<Order>(o => o.Id <= 0 ? "Order ID must be positive" : null);

    public static readonly ValidationRule<Order> AmountPositive = 
        new ValidationRule<Order>(o => o.Amount <= 0 ? "Amount must be positive" : null);
}

// Usage
var order = new Order { Id = 0, Amount = 100m };

// Get per-rule problems
var allProblems = ValidateAll(order, OrderRules.IdPositive, OrderRules.AmountPositive);
foreach (var kvp in allProblems)
{
    Console.WriteLine($"Rule: {kvp.Key}, Problems: {string.Join(", ", kvp.Value)}");
}

// Throw on failure
try
{
    EnsureValid(order, OrderRules.IdPositive, OrderRules.AmountPositive);
}
catch (Exception ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

## Notes

- **Empty rules:** When no rules are provided, `Validate` returns an empty list, `IsValid` and `AllValid` return `true`, `GetFirstProblem` returns `null`, and `EnsureValid` does not throw.
- **Null object:** The behavior when `obj` is `null` depends entirely on the individual `ValidationRule<T>` implementations. Rules that do not handle null may throw a `NullReferenceException`. It is recommended that rules explicitly check for null if the object is expected to be nullable.
- **Thread safety:** All methods are static and do not maintain internal mutable state. They are thread-safe as long as the `ValidationRule<T>` instances passed to them are also thread-safe (i.e., their evaluation delegates do not rely on shared mutable state). The returned collections (lists and dictionaries) are read-only snapshots and can be safely enumerated by multiple threads.
- **Exception from `EnsureValid`:** The exact exception type thrown is not specified by the API. Callers should catch a general `Exception` or rely on the documented behavior that it throws only when validation fails.
