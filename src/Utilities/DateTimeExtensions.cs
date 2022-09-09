#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Extension methods for DateTime manipulation and formatting
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts UTC DateTime to relative time string (e.g., "2 hours ago")
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <returns>A relative time string representation</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is invalid</exception>
    public static string ToRelativeTimeString(this DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();

        if (timeSpan.TotalSeconds < 60)
            return "just now";

        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minute{(Math.Abs((int)timeSpan.TotalMinutes) == 1 ? "" : "s")} ago";

        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hour{(Math.Abs((int)timeSpan.TotalHours) == 1 ? "" : "s")} ago";

        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} day{(Math.Abs((int)timeSpan.TotalDays) == 1 ? "" : "s")} ago";

        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)} week{(Math.Abs((int)(timeSpan.TotalDays / 7)) == 1 ? "" : "s")} ago";

        if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)} month{(Math.Abs((int)(timeSpan.TotalDays / 30)) == 1 ? "" : "s")} ago";

        return $"{(int)(timeSpan.TotalDays / 365)} year{(Math.Abs((int)(timeSpan.TotalDays / 365)) == 1 ? "" : "s")} ago";
    }

    /// <summary>
    /// Formats DateTime to ISO 8601 string with milliseconds
    /// </summary>
    /// <param name="dateTime">The DateTime to format</param>
    /// <returns>An ISO 8601 formatted string</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is invalid</exception>
    public static string ToIsoString(this DateTime dateTime) => dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

    /// <summary>
    /// Formats DateTime to human-readable string with timezone
    /// </summary>
    /// <param name="dateTime">The DateTime to format</param>
    /// <returns>A formatted string representation</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is invalid</exception>
    public static string ToFormattedString(this DateTime dateTime) => dateTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss UTC");

    /// <summary>
    /// Checks if DateTime is in the past
    /// </summary>
    /// <param name="dateTime">The DateTime to check</param>
    /// <returns>true if dateTime is in the past; otherwise, false</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is invalid</exception>
    public static bool IsPast(this DateTime dateTime) => dateTime < DateTime.UtcNow;

    /// <summary>
    /// Checks if DateTime is in the future
    /// </summary>
    /// <param name="dateTime">The DateTime to check</param>
    /// <returns>true if dateTime is in the future; otherwise, false</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is invalid</exception>
    public static bool IsFuture(this DateTime dateTime) => dateTime > DateTime.UtcNow;

    /// <summary>
    /// Gets the number of minutes elapsed since the DateTime
    /// </summary>
    /// <param name="dateTime">The DateTime to calculate from</param>
    /// <returns>The number of minutes elapsed</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is invalid</exception>
    public static int GetMinutesElapsed(this DateTime dateTime) => (int)(DateTime.UtcNow - dateTime.ToUniversalTime()).TotalMinutes;

    /// <summary>
    /// Gets the number of seconds elapsed since the DateTime
    /// </summary>
    /// <param name="dateTime">The DateTime to calculate from</param>
    /// <returns>The number of seconds elapsed</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is invalid</exception>
    public static int GetSecondsElapsed(this DateTime dateTime) => (int)(DateTime.UtcNow - dateTime.ToUniversalTime()).TotalSeconds;

    /// <summary>
    /// Rounds DateTime to the nearest minute
    /// </summary>
    /// <param name="dateTime">The DateTime to round</param>
    /// <returns>A DateTime rounded to the nearest minute</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is invalid</exception>
    public static DateTime RoundToNearestMinute(this DateTime dateTime) => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0, dateTime.Kind);

    /// <summary>
    /// Rounds DateTime to the nearest hour
    /// </summary>
    /// <param name="dateTime">The DateTime to round</param>
    /// <returns>A DateTime rounded to the nearest hour</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is invalid</exception>
    public static DateTime RoundToNearestHour(this DateTime dateTime) => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, dateTime.Kind);

    /// <summary>
    /// Gets the start of the day (00:00:00)
    /// </summary>
    /// <param name="dateTime">The DateTime to process</param>
    /// <returns>A DateTime set to the start of the day</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is invalid</exception>
    public static DateTime GetStartOfDay(this DateTime dateTime) => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, dateTime.Kind);

    /// <summary>
    /// Gets the end of the day (23:59:59)
    /// </summary>
    /// <param name="dateTime">The DateTime to process</param>
    /// <returns>A DateTime set to the end of the day</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is invalid</exception>
    public static DateTime GetEndOfDay(this DateTime dateTime) => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, dateTime.Kind);

    /// <summary>
    /// Gets the start of the week (Monday)
    /// <note>This method assumes Monday is the first day of the week, which may not be correct for all cultures.</note>
    /// </summary>
    /// <param name="dateTime">The DateTime to process</param>
    /// <returns>A DateTime set to the start of the week</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is invalid</exception>
    public static DateTime GetStartOfWeek(this DateTime dateTime)
    {
        var dayOfWeek = (int)dateTime.DayOfWeek;
        var diff = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        return dateTime.AddDays(-diff).GetStartOfDay();
    }

    /// <summary>
    /// Gets the start of the month
    /// </summary>
    /// <param name="dateTime">The DateTime to process</param>
    /// <returns>A DateTime set to the start of the month</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is invalid</exception>
    public static DateTime GetStartOfMonth(this DateTime dateTime) => new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Kind);

    /// <summary>
    /// Gets the end of the month
    /// </summary>
    /// <param name="dateTime">The DateTime to process</param>
    /// <returns>A DateTime set to the end of the month</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is invalid</exception>
    public static DateTime GetEndOfMonth(this DateTime dateTime)
    {
        var lastDay = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
        return new DateTime(dateTime.Year, dateTime.Month, lastDay, 23, 59, 59, dateTime.Kind);
    }

    /// <summary>
    /// Checks if DateTime is today
    /// </summary>
    /// <param name="dateTime">The DateTime to check</param>
    /// <returns>true if dateTime is today; otherwise, false</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is invalid</exception>
    public static bool IsToday(this DateTime dateTime) => dateTime.Date == DateTime.UtcNow.Date;

    /// <summary>
    /// Checks if DateTime is yesterday
    /// </summary>
    /// <param name="dateTime">The DateTime to check</param>
    /// <returns>true if dateTime is yesterday; otherwise, false</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is invalid</exception>
    public static bool IsYesterday(this DateTime dateTime) => dateTime.Date == DateTime.UtcNow.AddDays(-1).Date;

    /// <summary>
    /// Gets business days between two dates
    /// <note>This method assumes Saturday and Sunday are weekend days, which may not be correct for all cultures.</note>
    /// </summary>
    /// <param name="startDate">The start date</param>
    /// <param name="endDate">The end date</param>
    /// <returns>The number of business days between the two dates</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when startDate or endDate is invalid</exception>
    public static int GetBusinessDaysBetween(this DateTime startDate, DateTime endDate)
    {
        int businessDays = 0;
        var current = startDate.GetStartOfDay();
        var end = endDate.GetStartOfDay();

        while (current <= end)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                businessDays++;

            current = current.AddDays(1);
        }

        return businessDays;
    }

    /// <summary>
    /// Converts unix timestamp to DateTime
    /// </summary>
    /// <param name="timestamp">The unix timestamp to convert</param>
    /// <returns>A DateTime representing the unix timestamp</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when timestamp is invalid</exception>
    public static DateTime FromUnixTimestamp(long timestamp) => DateTime.UnixEpoch.AddSeconds(timestamp);

    /// <summary>
    /// Converts DateTime to unix timestamp
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <returns>A unix timestamp representing the DateTime</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is invalid</exception>
    public static long ToUnixTimestamp(this DateTime dateTime) => (long)(dateTime.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds;
}
