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
    public static string ToRelativeTimeString(this DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();

        if (timeSpan.TotalSeconds < 60)
            return "just now";

        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minute(s) ago";

        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hour(s) ago";

        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} day(s) ago";

        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)} week(s) ago";

        if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)} month(s) ago";

        return $"{(int)(timeSpan.TotalDays / 365)} year(s) ago";
    }

    /// <summary>
    /// Formats DateTime to ISO 8601 string with milliseconds
    /// </summary>
    public static string ToIsoString(this DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }

    /// <summary>
    /// Formats DateTime to human-readable string with timezone
    /// </summary>
    public static string ToFormattedString(this DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss UTC");
    }

    /// <summary>
    /// Checks if DateTime is in the past
    /// </summary>
    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if DateTime is in the future
    /// </summary>
    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the number of minutes elapsed since the DateTime
    /// </summary>
    public static int GetMinutesElapsed(this DateTime dateTime)
    {
        return (int)(DateTime.UtcNow - dateTime.ToUniversalTime()).TotalMinutes;
    }

    /// <summary>
    /// Gets the number of seconds elapsed since the DateTime
    /// </summary>
    public static int GetSecondsElapsed(this DateTime dateTime)
    {
        return (int)(DateTime.UtcNow - dateTime.ToUniversalTime()).TotalSeconds;
    }

    /// <summary>
    /// Rounds DateTime to the nearest minute
    /// </summary>
    public static DateTime RoundToNearestMinute(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour,
            dateTime.Minute, 0, dateTime.Kind);
    }

    /// <summary>
    /// Rounds DateTime to the nearest hour
    /// </summary>
    public static DateTime RoundToNearestHour(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, dateTime.Kind);
    }

    /// <summary>
    /// Gets the start of the day (00:00:00)
    /// </summary>
    public static DateTime GetStartOfDay(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, dateTime.Kind);
    }

    /// <summary>
    /// Gets the end of the day (23:59:59)
    /// </summary>
    public static DateTime GetEndOfDay(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, dateTime.Kind);
    }

    /// <summary>
    /// Gets the start of the week (Monday)
    /// </summary>
    public static DateTime GetStartOfWeek(this DateTime dateTime)
    {
        var dayOfWeek = (int)dateTime.DayOfWeek;
        var diff = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        return dateTime.AddDays(-diff).GetStartOfDay();
    }

    /// <summary>
    /// Gets the start of the month
    /// </summary>
    public static DateTime GetStartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Kind);
    }

    /// <summary>
    /// Gets the end of the month
    /// </summary>
    public static DateTime GetEndOfMonth(this DateTime dateTime)
    {
        var lastDay = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
        return new DateTime(dateTime.Year, dateTime.Month, lastDay, 23, 59, 59, dateTime.Kind);
    }

    /// <summary>
    /// Checks if DateTime is today
    /// </summary>
    public static bool IsToday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Checks if DateTime is yesterday
    /// </summary>
    public static bool IsYesterday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.UtcNow.AddDays(-1).Date;
    }

    /// <summary>
    /// Gets business days between two dates
    /// </summary>
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
    public static DateTime FromUnixTimestamp(long timestamp)
    {
        return DateTime.UnixEpoch.AddSeconds(timestamp);
    }

    /// <summary>
    /// Converts DateTime to unix timestamp
    /// </summary>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return (long)(dateTime.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds;
    }
}
