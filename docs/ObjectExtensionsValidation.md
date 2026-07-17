# ObjectExtensionsValidation

Provides extension methods for validating objects and their properties using data annotations attributes.

## API

### `Validate(object target)`
Validates the specified object using data annotations attributes and returns a list of validation error messages.

- **Parameters**
  - `target` (object): The object to validate.
- **Return value**
  - `IReadOnlyList<string>`: A read-only list of validation error messages. Empty if validation succeeds.
- **Throws**
  - `ArgumentNullException`: If `target` is `null`.

### `ValidateProperty(object target, string propertyName)`
Validates a specific property on the specified object using data annotations attributes and returns a list of validation error messages.

- **Parameters**
  - `target` (object): The object containing the property to validate.
  - `propertyName` (string): The name of the property to validate.
- **Return value**
  - `IReadOnlyList<string>`: A read-only list of validation error messages. Empty if validation succeeds.
- **Throws**
  - `ArgumentNullException`: If `target` is `null`.
  - `ArgumentNullException`: If `propertyName` is `null`.
  - `ArgumentException`: If the property does not exist on the object.

### `IsValid(object target)`
Determines whether the specified object is valid according to data annotations attributes.

- **Parameters**
  - `target` (object): The object to validate.
- **Return value**
  - `bool`: `true` if the object is valid; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `target` is `null`.

### `IsValidProperty(object target, string propertyName)`
Determines whether a specific property on the specified object is valid according to data annotations attributes.

- **Parameters**
  - `target` (object): The object containing the property to validate.
  - `propertyName` (string): The name of the property to validate.
- **Return value**
  - `bool`: `true` if the property is valid; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `target` is `null`.
  - `ArgumentNullException`: If `propertyName` is `null`.
  - `ArgumentException`: If the property does not exist on the object.

### `EnsureValid(object target)`
Validates the specified object and throws an exception if validation fails.

- **Parameters**
  - `target` (object): The object to validate.
- **Throws**
  - `ArgumentNullException`: If `target` is `null`.
  - `ValidationException`: If the object is invalid.

### `EnsureValidProperty(object target, string propertyName)`
Validates a specific property on the specified object and throws an exception if validation fails.

- **Parameters**
  - `target` (object): The object containing the property to validate.
  - `propertyName` (string): The name of the property to validate.
- **Throws**
  - `ArgumentNullException`: If `target` is `null`.
  - `ArgumentNullException`: If `propertyName` is `null`.
  - `ArgumentException`: If the property does not exist on the object.
  - `ValidationException`: If the property is invalid.

## Usage

```csharp
using System.ComponentModel.DataAnnotations;

public class Person
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [Range(0, 120)]
    public int Age { get; set; }
}

// Example 1: Validate an entire object
var person = new Person { Name = null, Age = 150 };
var errors = ObjectExtensionsValidation.Validate(person);
if (errors.Any())
{
    Console.WriteLine("Validation failed:");
    foreach (var error in errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Example 2: Validate a single property
var isNameValid = ObjectExtensionsValidation.IsValidProperty(person, nameof(Person.Name));
if (!isNameValid)
{
    Console.WriteLine("Name is invalid.");
}
```

## Notes

- All methods are thread-safe as they do not modify shared state.
- Validation relies on `System.ComponentModel.DataAnnotations` attributes. Ensure these are applied to the target object or property.
- `EnsureValid` and `EnsureValidProperty` throw `ValidationException` on failure, which includes the validation error messages in its `Message` property.
