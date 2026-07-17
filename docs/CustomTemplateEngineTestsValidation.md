# CustomTemplateEngineTestsValidation

Provides validation utilities for template engine test configurations within the `dotnet-deploy-notify` project. This static class ensures that template definitions adhere to expected structural and semantic rules before processing, preventing runtime errors due to malformed or incomplete templates.

## API

### `public static IReadOnlyList<string> Validate`
Validates the current template engine test configuration and returns a list of validation errors. If no errors are found, the returned list is empty.

**Returns:**
- `IReadOnlyList<string>`: A read-only list of error messages describing validation failures. An empty list indicates a valid configuration.

---

### `public static bool IsValid`
Determines whether the current template engine test configuration is valid.

**Returns:**
- `bool`: `true` if the configuration passes all validation checks; otherwise, `false`.

---

### `public static void EnsureValid`
Ensures the current template engine test configuration is valid. If validation fails, throws an `InvalidOperationException` with a concatenated list of error messages.

**Exceptions:**
- `InvalidOperationException`: Thrown when the configuration contains one or more validation errors.

---

### **Overloads**
The three methods above are exposed in multiple contexts (likely via partial classes or extension methods) but exhibit identical behavior in each. The documentation applies uniformly to all instances.

## Usage

### Example 1: Basic Validation
