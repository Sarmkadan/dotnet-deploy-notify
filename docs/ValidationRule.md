# ValidationRule
Base abstract class for defining validation logic used within the dotnet-deploy-notify project. Concrete implementations provide specific checks such as non‑empty strings, length constraints, URL format, email format, regular‑expression patterns, and numeric ranges.

## API
### Validate (abstract)
```csharp
public abstract bool Validate;
```
**Purpose**  
Determines whether the current validation rule is satisfied.

**Parameters**  
None.

**Return value**  
`true` if the validation passes; otherwise `false`.

**Exceptions**  
May throw exceptions thrown by the concrete implementation (e.g., `InvalidOperationException` if the rule is in an inconsistent state).

### GetErrorMessage (abstract)
```csharp
public abstract string GetErrorMessage;
```
**Purpose**  
Retrieves the error message associated with a failed validation.

**Parameters**  
None.

**Return value**  
A non‑null string describing why the validation failed.

**Exceptions**  
May throw exceptions thrown by the concrete implementation (e.g., `NullReferenceException` if internal state is not properly initialized).

### NotEmptyRule (class)
A concrete `ValidationRule` that validates that a supplied value is not null or empty.

#### Validate (NotEmptyRule)
```csharp
public override bool Validate;
```
**Purpose**  
Returns `true` when the target value is neither `null` nor `System.String.Empty`.

**Parameters**  
None.

**Return value**  
`true` for non‑empty values; `false` otherwise.

**Exceptions**  
May throw if the underlying value source throws when accessed.

#### GetErrorMessage (NotEmptyRule)
```csharp
public override string GetErrorMessage;
```
**Purpose**  
Provides a message indicating that the value is empty or null.

**Parameters**  
None.

**Return value**  
A string such as “Value must not be empty.”

**Exceptions**  
Same as the base `GetErrorMessage`.

### LengthRule (class)
A concrete `ValidationRule` that validates the length of a string value against a defined minimum and/or maximum.

#### Validate (LengthRule)
```csharp
public override bool Validate;
```
**Purpose**  
Returns `true` when the target string’s length falls within the inclusive bounds specified at construction.

**Parameters**  
None.

**Return value**  
`true` if length is valid; `false` otherwise.

**Exceptions**  
May throw `ArgumentOutOfRangeException` if internal bounds are invalid.

#### GetErrorMessage (LengthRule)
```csharp
public override string GetErrorMessage;
```
**Purpose**  
Supplies a message describing the length constraint violation.

**Parameters**  
None.

**Return value**  
A string such as “Value must be between 5 and 20 characters.”

**Exceptions**  
Same as the base `GetErrorMessage`.

### UrlRule (class)
A concrete `ValidationRule` that validates that a string conforms to a well‑formed URL.

#### Validate (UrlRule)
```csharp
public override bool Validate;
```
**Purpose**  
Returns `true` when the target string can be parsed as a valid absolute URI.

**Parameters**  
None.

**Return value**  
`true` for valid URLs; `false` otherwise.

**Exceptions**  
May throw `UriFormatException` if the internal parsing logic encounters an unexpected format.

#### GetErrorMessage (UrlRule)
```csharp
public override string GetErrorMessage;
```
**Purpose**  
Provides a message indicating that the value is not a valid URL.

**Parameters**  
None.

**Return value**  
A string such as “Value must be a valid URL.”

**Exceptions**  
Same as the base `GetErrorMessage`.

### EmailRule (class)
A concrete `ValidationRule` that validates that a string matches a typical email address pattern.

#### Validate (EmailRule)
```csharp
public override bool Validate;
```
**Purpose**  
Returns `true` when the target string matches the email regular expression used by the rule.

**Parameters**  
None.

**Return value**  
`true` for valid email addresses; `false` otherwise.

**Exceptions**  
May throw `ArgumentException` if the internal regex pattern is malformed.

#### GetErrorMessage (EmailRule)
```csharp
public override string GetErrorMessage;
```
**Purpose**  
Supplies a message indicating that the value is not a valid email address.

**Parameters**  
None.

**Return value**  
A string such as “Value must be a valid email address.”

**Exceptions**  
Same as the base `GetErrorMessage`.

### PatternRule (class)
A concrete `ValidationRule` that validates a string against a user‑supplied regular expression.

#### Validate (PatternRule)
```csharp
public override bool Validate;
```
**Purpose**  
Returns `true` when the target string matches the regular expression provided at construction.

**Parameters**  
None.

**Return value**  
`true` if the string matches the pattern; `false` otherwise.

**Exceptions**  
May throw `ArgumentNullException` if the pattern is `null`; `ArgumentException` if the pattern is not a valid regular expression.

#### GetErrorMessage (PatternRule)
```csharp
public override string GetErrorMessage;
```
**Purpose**  
Provides a message indicating that the value does not match the expected pattern.

**Parameters**  
None.

**Return value**  
A string such as “Value does not match the required pattern.”

**Exceptions**  
Same as the base `GetErrorMessage`.

### RangeRule (class)
A concrete `ValidationRule` that validates a numeric value falls within a specified inclusive range.

#### Validate (RangeRule)
```csharp
public override bool Validate;
```
**Purpose**  
Returns `true` when the target numeric value is greater than or equal to the minimum and less than or equal to the maximum defined at construction.

**Parameters**  
None.

**Return value**  
`true` for values within the range; `false` otherwise.

**Exceptions**  
May throw `InvalidOperationException` if the rule has not been initialized with proper bounds.

#### GetErrorMessage (RangeRule)
```csharp
public override string GetErrorMessage;
```
**Purpose**  
Supplies a message indicating that the value is outside the allowed range.

**Parameters**  
None.

**Return value**  
A string such as “Value must be between 1 and 100.”

**Exceptions**  
Same as the base `GetErrorMessage`.

## Usage
### Example 1: Validating a configuration URL
```csharp
using System;
using DotNetDeployNotify.Validation; // hypothetical namespace

var urlRule = new UrlRule();
// Assume the value to validate is stored elsewhere; the rule accesses it internally.
bool isValid = urlRule.Validate;
if (!isValid)
{
    Console.WriteLine($"Validation failed: {urlRule.GetErrorMessage}");
}
```

### Example 2: Combining multiple rules for a user‑supplied email
```csharp
using System.Collections.Generic;
using System.Linq;
using DotNetDeployNotify.Validation;

IEnumerable<ValidationRule> rules = new List<ValidationRule>
{
    new NotEmptyRule(),
    new EmailRule(),
    new PatternRule(@"^[^@\s]+@[^@\s]+\.[^@\s]+$") // custom pattern example
};

string input = userInput; // obtained from UI or config
// In this simplified example each rule internally reads `input`.
var failures = rules.Where(r => !r.Validate).ToList();

if (failures.Any())
{
    foreach (var rule in failures)
    {
        Console.WriteLine(rule.GetErrorMessage);
    }
}
else
{
    Console.WriteLine("All validations passed.");
}
```

## Notes
- The abstract `Validate` and `GetErrorMessage` members contain no parameters; concrete implementations are expected to obtain the value to validate from internal state (e.g., a field or property set at construction).  
- All concrete rule classes are stateless aside from their configuration (bounds, patterns, etc.) and therefore safe to use concurrently by multiple threads, provided that their configuration is not modified after construction.  
- If a rule’s internal state is invalid (e.g., a `LengthRule` with a negative maximum), the `Validate` method may throw an exception; callers should handle such cases when constructing rules.  
- `NotEmptyRule` treats a `null` reference as invalid; `LengthRule`, `UrlRule`, `EmailRule`, `PatternRule`, and `RangeRule` exhibit similar null‑sensitivity unless otherwise documented.  
- Culture‑specific considerations (e.g., case‑insensitive email matching) are encapsulated within the individual rule implementations and are not exposed through the base class.  
- Inheriting from `ValidationRule` requires overriding both `Validate` and `GetErrorMessage`; failure to do so results in a compile‑time error.  
- The provided examples assume the rules access a shared validation target internally; in real usage the target may be passed via constructor or property, depending on the actual implementation of the project.
