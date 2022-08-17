# MathExtensions

A utility class providing common mathematical operations and conversions for numeric types, including clamping, rounding, statistical measures, and human-readable formatting.

## API

### `public static T Clamp<T>(T value, T min, T max)`

Restricts a value to a specified range.

- **Parameters**
  - `value`: The value to clamp.
  - `min`: The lower bound of the range.
  - `max`: The upper bound of the range.
- **Returns**: The clamped value.
- **Throws**: `ArgumentException` if `min` is greater than `max`.

---

### `public static bool IsBetween<T>(T value, T lower, T upper)`

Determines whether a value lies within a specified range, inclusive.

- **Parameters**
  - `value`: The value to check.
  - `lower`: The lower bound of the range.
  - `upper`: The upper bound of the range.
- **Returns**: `true` if `value` is between `lower` and `upper` (inclusive); otherwise, `false`.
- **Throws**: `ArgumentException` if `lower` is greater than `upper`.

---

### `public static double ToPercentage(double value, double total)`

Converts a value to a percentage of a total.

- **Parameters**
  - `value`: The part value.
  - `total`: The whole value.
- **Returns**: The percentage as a `double` between 0.0 and 100.0.
- **Throws**: `ArgumentException` if `total` is zero or negative.

---

### `public static double ToPercentage(decimal value, decimal total)`

Converts a value to a percentage of a total.

- **Parameters**
  - `value`: The part value.
  - `total`: The whole value.
- **Returns**: The percentage as a `double` between 0.0 and 100.0.
- **Throws**: `ArgumentException` if `total` is zero or negative.

---
### `public static decimal RoundTo(decimal value, int digits)`

Rounds a decimal value to a specified number of fractional digits.

- **Parameters**
  - `value`: The value to round.
  - `digits`: The number of fractional digits.
- **Returns**: The rounded value.
- **Throws**: `ArgumentOutOfRangeException` if `digits` is negative.

---
### `public static double RoundTo(double value, int digits)`

Rounds a double value to a specified number of fractional digits.

- **Parameters**
  - `value`: The value to round.
  - `digits`: The number of fractional digits.
- **Returns**: The rounded value.
- **Throws**: `ArgumentOutOfRangeException` if `digits` is negative.

---
### `public static double Average(params double[] values)`

Calculates the arithmetic mean of a sequence of values.

- **Parameters**
  - `values`: The values to average.
- **Returns**: The average of the values.
- **Throws**: `ArgumentNullException` if `values` is `null`.
- **Throws**: `ArgumentException` if `values` is empty.

---
### `public static double Median(params double[] values)`

Calculates the median of a sequence of values.

- **Parameters**
  - `values`: The values to compute the median for.
- **Returns**: The median value.
- **Throws**: `ArgumentNullException` if `values` is `null`.
- **Throws**: `ArgumentException` if `values` is empty.

---
### `public static int SafeSum(params int[] values)`

Sums a sequence of integers, returning zero if the sequence is empty or null.

- **Parameters**
  - `values`: The values to sum.
- **Returns**: The sum of the values, or zero if `values` is `null` or empty.

---
### `public static double SafeAverage(params double[] values)`

Calculates the arithmetic mean of a sequence of values, returning zero if the sequence is empty or null.

- **Parameters**
  - `values`: The values to average.
- **Returns**: The average of the values, or zero if `values` is `null` or empty.

---
### `public static string ToHumanReadableSize(long bytes)`

Converts a byte count into a human-readable file size string (e.g., "1.23 MB").

- **Parameters**
  - `bytes`: The byte count to format.
- **Returns**: A human-readable size string.

---
### `public static string ToHumanReadableDuration(TimeSpan duration)`

Converts a time span into a human-readable duration string (e.g., "2h 30m").

- **Parameters**
  - `duration`: The duration to format.
- **Returns**: A human-readable duration string.

---
### `public static string ToHumanReadableDuration(double seconds)`

Converts a duration in seconds into a human-readable duration string (e.g., "2h 30m").

- **Parameters**
  - `seconds`: The duration in seconds to format.
- **Returns**: A human-readable duration string.

---
### `public static decimal CalculateCompoundInterest(decimal principal, decimal rate, int periods)`

Calculates the compound interest for a principal amount over a number of periods at a given rate.

- **Parameters**
  - `principal`: The initial amount.
  - `rate`: The interest rate per period (as a decimal, e.g., 0.05 for 5%).
  - `periods`: The number of compounding periods.
- **Returns**: The total amount after compounding.
- **Throws**: `ArgumentOutOfRangeException` if `rate` is negative or `periods` is negative.

---
### `public static int RandomBetween(int min, int max)`

Generates a random integer within a specified range, inclusive.

- **Parameters**
  - `min`: The inclusive lower bound.
  - `max`: The inclusive upper bound.
- **Returns**: A random integer between `min` and `max`.
- **Throws**: `ArgumentException` if `min` is greater than `max`.

## Usage
