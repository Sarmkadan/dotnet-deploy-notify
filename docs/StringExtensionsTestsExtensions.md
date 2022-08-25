# StringExtensionsTestsExtensions

Utility class containing test extensions for validating `StringExtensions` behavior. These methods provide Fluent-style assertions for common string operations, enabling concise and readable unit tests for string manipulation utilities.

## API

### `ShouldBeTruncatedTo`

Asserts that a string is truncated to the specified maximum length, appending an ellipsis if truncated.

- **Parameters**
  - `maxLength` (`int`): The maximum allowed length of the string.
  - `expected` (`string`): The expected truncated string, including ellipsis if applicable.
- **Return value**
  - `void`: Throws an exception if the assertion fails.
- **Exceptions**
  - Throws if the input string exceeds `maxLength` and is not truncated to `expected`.
  - Throws if the input string is shorter than `maxLength` but does not match `expected`.

### `ShouldBeValidSlug`

Asserts that a string is a valid URL-friendly slug (lowercase, alphanumeric, hyphens only, no leading/trailing hyphens).

- **Parameters**
  - `expected` (`string`): The expected valid slug.
- **Return value**
  - `void`: Throws an exception if the assertion fails.
- **Exceptions**
  - Throws if the input string contains invalid characters, uppercase letters, or improper hyphen placement.

### `ShouldConvertToBoolean`

Asserts that a string parses to the expected boolean value using common truthy/falsy conventions.

- **Parameters**
  - `expected` (`bool`): The expected boolean result of parsing.
  - `caseSensitive` (`bool`, optional): Whether the comparison should be case-sensitive. Defaults to `false`.
- **Return value**
  - `void`: Throws an exception if the assertion fails.
- **Exceptions**
  - Throws if the input string does not parse to `expected` under the specified rules.

### `ShouldBeProperlyMasked`

Asserts that a string is masked according to a specified pattern (e.g., credit card or SSN masking).

- **Parameters**
  - `maskPattern` (`string`): A regex pattern describing the expected masking format.
  - `visibleChars` (`int`): The number of leading characters to remain unmasked.
- **Return value**
  - `void`: Throws an exception if the assertion fails.
- **Exceptions**
  - Throws if the input string does not match `maskPattern` or if `visibleChars` exceeds the string length.

## Usage
