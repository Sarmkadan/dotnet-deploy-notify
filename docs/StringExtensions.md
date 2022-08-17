# StringExtensions

A utility class providing common string manipulation and formatting extensions for .NET applications, particularly in deployment and notification scenarios.

## API

### `public static string Truncate(string input, int maxLength, string suffix = "…")`

Reduces a string to a specified maximum length, appending an optional suffix if truncation occurs.

- **Parameters**
  - `input`: The string to truncate.
  - `maxLength`: The maximum allowed length of the result. Must be non-negative.
  - `suffix`: The suffix appended when truncation occurs. Defaults to "…".
- **Return value**: The truncated string, or the original string if it is shorter than `maxLength`.
- **Throws**: `ArgumentOutOfRangeException` if `maxLength` is negative.
- **Throws**: `ArgumentNullException` if `input` is `null`.

---

### `public static string ToSlug(string input)`

Converts a string into a URL-friendly slug by normalizing diacritics, removing special characters, and replacing spaces with hyphens.

- **Parameters**
  - `input`: The string to convert.
- **Return value**: A lowercase, hyphen-separated slug. Empty if input is `null` or whitespace.
- **Throws**: `ArgumentNullException` if `input` is `null`.

---

### `public static string ToPascalCase(string input)`

Transforms a string into PascalCase by capitalizing the first letter of each word and removing separators.

- **Parameters**
  - `input`: The string to convert.
- **Return value**: The PascalCase string. Returns `string.Empty` if input is `null` or empty.
- **Throws**: `ArgumentNullException` if `input` is `null`.

---

### `public static string ToCamelCase(string input)`

Transforms a string into camelCase by lowercasing the first letter of the first word and capitalizing subsequent words.

- **Parameters**
  - `input`: The string to convert.
- **Return value**: The camelCase string. Returns `string.Empty` if input is `null` or empty.
- **Throws**: `ArgumentNullException` if `input` is `null`.

---
### `public static string MaskSensitive(string input, char maskChar = '*', int keepStart = 0, int keepEnd = 0)`

Masks sensitive portions of a string by replacing characters with a specified mask, preserving a prefix and suffix.

- **Parameters**
  - `input`: The string to mask.
  - `maskChar`: The character used for masking. Defaults to `'*'`.
  - `keepStart`: Number of leading characters to preserve. Defaults to `0`.
  - `keepEnd`: Number of trailing characters to preserve. Defaults to `0`.
- **Return value**: The masked string. Returns `string.Empty` if input is `null` or empty.
- **Throws**: `ArgumentOutOfRangeException` if `keepStart` or `keepEnd` are negative or exceed the input length.
- **Throws**: `ArgumentNullException` if `input` is `null`.

---
### `public static bool ContainsAny(string input, params char[] chars)`

Determines whether the string contains any of the specified characters.

- **Parameters**
  - `input`: The string to search.
  - `chars`: The characters to look for.
- **Return value**: `true` if any character in `chars` is found; otherwise, `false`. Returns `false` if `input` is `null`.
- **Throws**: `ArgumentNullException` if `chars` is `null`.

---
### `public static string NormalizeLineEndings(string input, string newLine = "\r\n")`

Replaces all line endings in the string with the specified newline sequence.

- **Parameters**
  - `input`: The string to normalize.
  - `newLine`: The target line ending. Defaults to `\r\n`.
- **Return value**: The string with normalized line endings. Returns `string.Empty` if input is `null`.
- **Throws**: `ArgumentNullException` if `newLine` is `null`.

---
### `public static int CountOccurrences(string input, string value, StringComparison comparison = StringComparison.Ordinal)`

Counts the number of non-overlapping occurrences of a substring within a string.

- **Parameters**
  - `input`: The string to search.
  - `value`: The substring to count.
  - `comparison`: The string comparison method. Defaults to `StringComparison.Ordinal`.
- **Return value**: The number of occurrences. Returns `0` if `input` or `value` is `null` or empty.
- **Throws**: `ArgumentNullException` if `value` is `null`.

---
### `public static string RemoveDuplicateCharacters(string input, bool caseSensitive = true)`

Removes duplicate characters from a string, optionally preserving case sensitivity.

- **Parameters**
  - `input`: The string to process.
  - `caseSensitive`: Whether the comparison is case-sensitive. Defaults to `true`.
- **Return value**: A new string with duplicates removed. Returns `string.Empty` if input is `null` or empty.
- **Throws**: `ArgumentNullException` if `input` is `null`.

---
### `public static string TakeWords(string input, int maxWords)`

Extracts up to a specified number of words from a string, delimited by whitespace.

- **Parameters**
  - `input`: The string to process.
  - `maxWords`: The maximum number of words to return. Must be non-negative.
- **Return value**: A string containing the first `maxWords` words. Returns `string.Empty` if input is `null`, empty, or `maxWords` is `0`.
- **Throws**: `ArgumentOutOfRangeException` if `maxWords` is negative.
- **Throws**: `ArgumentNullException` if `input` is `null`.

---
### `public static string WrapText(string input, int maxLineLength)`

Wraps a string into multiple lines, each no longer than the specified maximum line length, without breaking words.

- **Parameters**
  - `input`: The string to wrap.
  - `maxLineLength`: The maximum length of each line. Must be greater than `0`.
- **Return value**: The wrapped string. Returns `string.Empty` if input is `null` or empty.
- **Throws**: `ArgumentOutOfRangeException` if `maxLineLength` is less than or equal to `0`.
- **Throws**: `ArgumentNullException` if `input` is `null`.

---
### `public static bool ToBooleanSafe(string input, bool defaultValue = false)`

Safely converts a string representation of a boolean to its `bool` equivalent, returning a default if parsing fails.

- **Parameters**
  - `input`: The string to convert.
  - `defaultValue`: The value returned if parsing fails. Defaults to `false`.
- **Return value**: `true` if input is `"true"` (case-insensitive); `false` if input is `"false"` (case-insensitive); otherwise, `defaultValue`.
- **Throws**: `ArgumentNullException` if `input` is `null`.

## Usage
