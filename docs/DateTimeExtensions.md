# DateTimeExtensions

A utility class providing common date and time manipulation and formatting operations for `System.DateTime` in .NET applications.

## API

### `public static string ToRelativeTimeString(DateTime date)`

Converts a `DateTime` instance into a human-readable relative time string (e.g., "2 minutes ago", "in 3 hours"). Uses current system time as the reference point.

- **Parameters**
  - `date`: The `DateTime` to convert.
- **Returns**
  - A localized string representing the relative time.
- **Throws**
  - `ArgumentOutOfRangeException`: If `date` is outside the representable range for relative time calculation.

---

### `public static string ToIsoString(DateTime date)`

Formats a `DateTime` instance as an ISO 8601 compliant string (e.g., "2024-04-05T14:30:00Z").

- **Parameters**
  - `date`: The `DateTime` to format.
- **Returns**
  - An ISO 8601 formatted string.
- **Throws**
  - `ArgumentOutOfRangeException`: If `date` is outside the valid range for ISO formatting.

---

### `public static string ToFormattedString(DateTime date, string format = "yyyy-MM-dd HH:mm:ss")`

Formats a `DateTime` using a custom format string. Falls back to a default format if none is provided.

- **Parameters**
  - `date`: The `DateTime` to format.
  - `format`: Optional format string. Defaults to `"yyyy-MM-dd HH:mm:ss"`.
- **Returns**
  - A formatted string representation of the `DateTime`.
- **Throws**
  - `FormatException`: If the provided `format` string is invalid.
  - `ArgumentOutOfRangeException`: If `date` is outside the valid range for formatting.

---

### `public static bool IsPast(DateTime date)`

Determines whether the given `DateTime` is in the past relative to the current system time.

- **Parameters**
  - `date`: The `DateTime` to evaluate.
- **Returns**
  - `true` if `date` is earlier than `DateTime.UtcNow`; otherwise, `false`.
- **Throws**
  - None.

---

### `public static bool IsFuture(DateTime date)`

Determines whether the given `DateTime` is in the future relative to the current system time.

- **Parameters**
  - `date`: The `DateTime` to evaluate.
- **Returns**
  - `true` if `date` is later than `DateTime.UtcNow`; otherwise, `false`.
- **Throws**
  - None.

---
### `public static int GetMinutesElapsed(DateTime start, DateTime end)`

Calculates the total number of whole minutes between two `DateTime` instances.

- **Parameters**
  - `start`: The starting `DateTime`.
  - `end`: The ending `DateTime`.
- **Returns**
  - The total number of whole minutes between `start` and `end`. Returns `0` if `end` is before `start`.
- **Throws**
  - None.

---
### `public static int GetSecondsElapsed(DateTime start, DateTime end)`

Calculates the total number of whole seconds between two `DateTime` instances.

- **Parameters**
  - `start`: The starting `DateTime`.
  - `end`: The ending `DateTime`.
- **Returns**
  - The total number of whole seconds between `start` and `end`. Returns `0` if `end` is before `start`.
- **Throws**
  - None.

---
### `public static DateTime RoundToNearestMinute(DateTime date)`

Rounds a `DateTime` to the nearest minute.

- **Parameters**
  - `date`: The `DateTime` to round.
- **Returns**
  - A new `DateTime` rounded to the nearest minute.
- **Throws**
  - None.

---
### `public static DateTime RoundToNearestHour(DateTime date)`

Rounds a `DateTime` to the nearest hour.

- **Parameters**
  - `date`: The `DateTime` to round.
- **Returns**
  - A new `DateTime` rounded to the nearest hour.
- **Throws**
  - None.

---
### `public static DateTime GetStartOfDay(DateTime date)`

Returns a `DateTime` representing the start of the day (00:00:00) for the given date.

- **Parameters**
  - `date`: The `DateTime` to evaluate.
- **Returns**
  - A new `DateTime` at the start of the day.
- **Throws**
  - None.

---
### `public static DateTime GetEndOfDay(DateTime date)`

Returns a `DateTime` representing the end of the day (23:59:59.999) for the given date.

- **Parameters**
  - `date`: The `DateTime` to evaluate.
- **Returns**
  - A new `DateTime` at the end of the day.
- **Throws**
  - None.

---
### `public static DateTime GetStartOfWeek(DateTime date, DayOfWeek startDay = DayOfWeek.Monday)`

Returns a `DateTime` representing the start of the week (based on the specified `startDay`) for the given date.

- **Parameters**
  - `date`: The `DateTime` to evaluate.
  - `startDay`: The day of the week considered the start of the week. Defaults to `DayOfWeek.Monday`.
- **Returns**
  - A new `DateTime` at the start of the week.
- **Throws**
  - None.

---
### `public static DateTime GetStartOfMonth(DateTime date)`

Returns a `DateTime` representing the start of the month (1st day at 00:00:00) for the given date.

- **Parameters**
  - `date`: The `DateTime` to evaluate.
- **Returns**
  - A new `DateTime` at the start of the month.
- **Throws**
  - None.

---
### `public static DateTime GetEndOfMonth(DateTime date)`

Returns a `DateTime` representing the end of the month (last day at 23:59:59.999) for the given date.

- **Parameters**
  - `date`: The `DateTime` to evaluate.
- **Returns**
  - A new `DateTime` at the end of the month.
- **Throws**
  - None.

---
### `public static bool IsToday(DateTime date)`

Determines whether the given `DateTime` falls on the current system date.

- **Parameters**
  - `date`: The `DateTime` to evaluate.
- **Returns**
  - `true` if `date` is on the current system date; otherwise, `false`.
- **Throws**
  - None.

---
### `public static bool IsYesterday(DateTime date)`

Determines whether the given `DateTime` falls on the day before the current system date.

- **Parameters**
  - `date`: The `DateTime` to evaluate.
- **Returns**
  - `true` if `date` is on the previous system date; otherwise, `false`.
- **Throws**
  - None.

---
### `public static int GetBusinessDaysBetween(DateTime start, DateTime end, IEnumerable<DateTime> holidays = null)`

Calculates the number of business days (Monday through Friday) between two `DateTime` instances, excluding specified holidays.

- **Parameters**
  - `start`: The starting `DateTime`.
  - `end`: The ending `DateTime`.
  - `holidays`: Optional collection of `DateTime` instances representing holidays to exclude. Defaults to `null`.
- **Returns**
  - The total number of business days between `start` and `end`. Returns `0` if `end` is before `start`.
- **Throws**
  - `ArgumentOutOfRangeException`: If `start` or `end` are outside the valid range for date arithmetic.

---
### `public static DateTime FromUnixTimestamp(long timestamp)`

Converts a Unix timestamp (seconds since 1970-01-01T00:00:00Z) to a `DateTime`.

- **Parameters**
  - `timestamp`: The Unix timestamp to convert.
- **Returns**
  - A `DateTime` representing the timestamp.
- **Throws**
  - `ArgumentOutOfRangeException`: If `timestamp` is outside the range representable by `DateTime`.

---
### `public static long ToUnixTimestamp(DateTime date)`

Converts a `DateTime` to a Unix timestamp (seconds since 1970-01-01T00:00:00Z).

- **Parameters**
  - `date`: The `DateTime` to convert.
- **Returns**
  - The Unix timestamp as a `long`.
- **Throws**
  - `ArgumentOutOfRangeException`: If `date` is outside the range representable by Unix timestamps.

## Usage
