# EnumExtensions

A utility class providing common operations for working with .NET enums, including description retrieval, flag checking, parsing, and value generation.

## API

### `GetDescription<T>(T value)`

Retrieves the description attribute value for a given enum value.

- **Parameters**
  - `value` – The enum value to inspect.
- **Return value**
  - The `DescriptionAttribute.Description` string if present; otherwise `null`.
- **Exceptions**
  - Throws `ArgumentException` if `T` is not an enum type.

### `HasFlag<T>(T value, T flag)`

Determines whether the enum value includes the specified flag.

- **Parameters**
  - `value` – The enum value to check.
  - `flag` – The flag value to test.
- **Return value**
  - `true` if the flag is set; otherwise `false`.
- **Exceptions**
  - Throws `ArgumentException` if `T` is not an enum type.

### `GetAllValues<T>()`

Returns a list of all defined values for the enum type.

- **Return value**
  - A `List<T>` containing every defined enum value.
- **Exceptions**
  - Throws `ArgumentException` if `T` is not an enum type.

### `ToHumanReadable<T>(T value)`

Converts an enum value into a human-readable string, replacing underscores with spaces and capitalizing words.

- **Parameters**
  - `value` – The enum value to format.
- **Return value**
  - A human-readable string representation of the enum value.
- **Exceptions**
  - Throws `ArgumentException` if `T` is not an enum type.

### `TryParse<T>(string value)`

Attempts to parse a string into the corresponding enum value.

- **Parameters**
  - `value` – The string to parse.
- **Return value**
  - The parsed enum value if successful; otherwise `null`.
- **Exceptions**
  - Throws `ArgumentException` if `T` is not an enum type.

### `GetRandomValue<T>()`

Returns a randomly selected enum value.

- **Return value**
  - A random enum value of type `T`.
- **Exceptions**
  - Throws `ArgumentException` if `T` is not an enum type.
  - Throws `InvalidOperationException` if the enum has no defined values.

### `IsIn<T>(T value, params T[] values)`

Checks whether the enum value is contained within the provided set of values.

- **Parameters**
  - `value` – The enum value to check.
  - `values` – The set of values to test against.
- **Return value**
  - `true` if the value is present; otherwise `false`.
- **Exceptions**
  - Throws `ArgumentException` if `T` is not an enum type.

## Usage
