# StringExtensionsTests

Unit test suite for the `StringExtensions` helper class in the `dotnet-deploy-notify` project. This class validates string manipulation utilities used throughout deployment notification pipelines, covering truncation, slug generation, safe parsing, substring counting, and sensitive data masking.

## API

### `Truncate_StringExceedsMaxLength_TruncatesAndAppendsSuffix`

Verifies that when a string exceeds a specified maximum length, the `Truncate` method shortens it to exactly that length and appends a configurable suffix (typically an ellipsis). The test supplies a known long input and asserts the output length equals the max length plus suffix length, with original prefix preserved.

- **Purpose**: Confirm truncation behavior with suffix attachment.
- **Parameters**: None (test method; input values are hardcoded in the test body).
- **Return value**: `void` (asserts pass or fail).
- **Throws**: No direct exceptions; test fails on assertion violation.

### `Truncate_NullOrWhitespace_ReturnsEmptyString`

Ensures that `Truncate` returns `string.Empty` when the input is `null`, an empty string, or consists solely of whitespace characters. This guards against null-reference exceptions and meaningless truncation attempts.

- **Purpose**: Validate null/whitespace guard clause.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: No direct exceptions.

### `ToSlug_StringWithSpacesAndUppercase_ReturnsLowercaseHyphenated`

Tests that `ToSlug` transforms a mixed-case string containing spaces into a lowercase, hyphen-separated slug suitable for URLs or identifiers. The test asserts that uppercase letters become lowercase and whitespace sequences collapse into single hyphens.

- **Purpose**: Confirm URL-safe slug generation.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: No direct exceptions.

### `ToBooleanSafe_KnownStringValues_ReturnsExpectedBoolean`

Validates that `ToBooleanSafe` correctly interprets common truthy and falsy string representations (`"true"`, `"false"`, `"yes"`, `"no"`, `"1"`, `"0"`, case-insensitive variants) and returns the corresponding `bool`. The test also confirms that unrecognized inputs return a safe default (typically `false`) rather than throwing.

- **Purpose**: Ensure robust string-to-boolean conversion.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: No direct exceptions.

### `CountOccurrences_SubstringAppearsMultipleTimes_ReturnsCorrectCount`

Exercises `CountOccurrences` with a substring that appears multiple times, including overlapping scenarios if applicable. The test asserts the returned integer matches the expected count, verifying ordinal comparison and proper iteration.

- **Purpose**: Validate substring frequency counting.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: No direct exceptions.

### `MaskSensitive_StringShorterThanVisibleCharThreshold_ReturnsAllStars`

Confirms that when the input string length is less than or equal to the visible-character threshold, `MaskSensitive` replaces the entire string with asterisks (`*`), preserving no original characters. This prevents partial exposure of very short secrets.

- **Purpose**: Test full masking for short sensitive values.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: No direct exceptions.

### `MaskSensitive_LongApiToken_ShowsOnlyFirstFourCharsAndMasksRest`

Verifies that for a long API token (length exceeding the visible-character threshold), `MaskSensitive` retains only the first four characters in plaintext and replaces all subsequent characters with asterisks. The test asserts the output starts with the original four-character prefix followed by a mask of appropriate length.

- **Purpose**: Confirm partial masking for long tokens.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: No direct exceptions.

## Usage

```csharp
using Xunit;

public class StringProcessingTests
{
    private readonly StringExtensionsTests _tests = new StringExtensionsTests();

    [Fact]
    public void ProcessDeploymentLog_TruncateAndMask()
    {
        // Truncate a long error message for notification display
        _tests.Truncate_StringExceedsMaxLength_TruncatesAndAppendsSuffix();

        // Mask an API token before logging
        _tests.MaskSensitive_LongApiToken_ShowsOnlyFirstFourCharsAndMasksRest();
    }

    [Fact]
    public void BuildReleaseSlug_AndCountMarkers()
    {
        // Generate a URL-safe slug from a release title
        _tests.ToSlug_StringWithSpacesAndUppercase_ReturnsLowercaseHyphenated();

        // Count occurrences of a deployment marker in logs
        _tests.CountOccurrences_SubstringAppearsMultipleTimes_ReturnsCorrectCount();
    }
}
```

## Notes

- **Edge cases**: `Truncate` tests assume the suffix itself does not exceed the max length; if the suffix is longer than the allowed maximum, behavior is implementation-defined and not covered by these tests. `ToBooleanSafe` treats unrecognized input as `false` by convention—callers relying on strict parsing should validate separately. `CountOccurrences` uses ordinal comparison; culture-sensitive counting is not tested here. `MaskSensitive` with a threshold of zero or negative values is not exercised; the implementation may clamp or throw.
- **Thread safety**: These are pure unit tests with no shared mutable state. The underlying static extension methods they exercise operate on immutable `string` instances and do not modify shared resources, making them inherently thread-safe. No synchronization concerns apply.
