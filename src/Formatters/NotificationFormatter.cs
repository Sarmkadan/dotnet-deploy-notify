#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Formatting;

using System.Globalization;

namespace DotNetDeployNotify.Formatters;

/// <summary>
/// Base interface for notification formatters
/// </summary>
public interface INotificationFormatter
{
    string Format(DeploymentNotification notification);
    string GetContentType();
}

/// <summary>
/// Formats notifications as JSON
/// </summary>
public sealed class JsonNotificationFormatter : INotificationFormatter
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public string Format(DeploymentNotification notification)
    {
        try
        {
            return JsonSerializer.Serialize(notification, _options);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to format notification as JSON: {ex.Message}", ex);
        }
    }

    public string GetContentType() => "application/json";
}

/// <summary>
/// Formats notifications as human-readable text
/// </summary>
public sealed class TextNotificationFormatter : INotificationFormatter
{
    /// <summary>When true, a status emoji is prepended to the Status line. Defaults to true.</summary>
    public bool EnableEmojis { get; init; } = true;

    public string Format(DeploymentNotification notification)
    {
        var statusLabel = StatusEmoji.Format(notification.Status, EnableEmojis);
        var lines = new List<string>
        {
            "╔════════════════════════════════════════════════════════════════╗",
            "║                  DEPLOYMENT NOTIFICATION                       ║",
            "╠════════════════════════════════════════════════════════════════╣",
            $"║ Status:          {PadRight(statusLabel, 40)} ║",
            $"║ Project:         {PadRight(notification.ProjectName, 40)} ║",
            $"║ Version:         {PadRight(notification.Version, 40)} ║",
            $"║ Environment:     {PadRight(notification.TargetEnvironment.ToString(), 40)} ║",
            $"║ Branch:          {PadRight(notification.BranchName, 40)} ║",
            $"║ Commit Author:   {PadRight(notification.CommitAuthor, 40)} ║",
            $"║ Commit Hash:     {PadRight(notification.CommitHash.Substring(0, Math.Min(8, notification.CommitHash.Length)), 40)} ║",
            $"║ Priority:        {PadRight(notification.Priority.ToString(), 40)} ║",
            $"║ Timestamp:       {PadRight(notification.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), 40)} ║",
            "╠════════════════════════════════════════════════════════════════╣",
            "║ MESSAGE                                                        ║",
            "╠════════════════════════════════════════════════════════════════╣"
        };

        // Add wrapped message
        var messageLines = WrapText(notification.Message, 62);
        foreach (var line in messageLines.Split(System.Environment.NewLine))
        {
            lines.Add($"║ {PadRight(line, 62)} ║");
        }

        lines.Add("╚════════════════════════════════════════════════════════════════╝");

        return string.Join(System.Environment.NewLine, lines);
    }

    public string GetContentType() => "text/plain";

    private string PadRight(string input, int length)
    {
        return (input ?? "").Length > length ? (input ?? "").Substring(0, length) : (input ?? "").PadRight(length);
    }

    private string WrapText(string text, int lineLength)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var lines = new List<string>();
        var words = text.Split(' ');
        var currentLine = new StringBuilder();

        foreach (var word in words)
        {
            if (currentLine.Length + word.Length + 1 > lineLength)
            {
                if (currentLine.Length > 0)
                    lines.Add(currentLine.ToString());
                currentLine.Clear();
            }

            if (currentLine.Length > 0)
                currentLine.Append(' ');
            currentLine.Append(word);
        }

        if (currentLine.Length > 0)
            lines.Add(currentLine.ToString());

        return string.Join(System.Environment.NewLine, lines);
    }
}

/// <summary>
/// Formats notifications as CSV
/// </summary>
public sealed class CsvNotificationFormatter : INotificationFormatter
{
    public string Format(DeploymentNotification notification)
    {
        var values = new[]
        {
            EscapeCsv(notification.Id),
            EscapeCsv(notification.ProjectName),
            EscapeCsv(notification.Version),
            EscapeCsv(notification.Status.ToString()),
            EscapeCsv(notification.TargetEnvironment.ToString()),
            EscapeCsv(notification.BranchName),
            EscapeCsv(notification.CommitAuthor),
            EscapeCsv(notification.Message),
            EscapeCsv(notification.CreatedAt.ToString("O")),
            EscapeCsv(string.Join(";", notification.Channels))
        };

        return string.Join(",", values);
    }

    public string GetContentType() => "text/csv";

    private string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}

/// <summary>
/// Formats notifications as Markdown
/// </summary>
public sealed class MarkdownNotificationFormatter : INotificationFormatter
{
    /// <summary>When true, a status emoji is prepended to the Status field. Defaults to true.</summary>
    public bool EnableEmojis { get; init; } = true;

    public string Format(DeploymentNotification notification)
    {
        var statusLabel = StatusEmoji.Format(notification.Status, EnableEmojis);
        var lines = new List<string>
        {
            "# Deployment Notification",
            "",
            $"## {notification.Status} - {notification.ProjectName}",
            "",
            "| Field | Value |",
            "|-------|-------|",
            $"| Project | `{notification.ProjectName}` |",
            $"| Version | `{notification.Version}` |",
            $"| Status | **{statusLabel}** |",
            $"| Environment | `{notification.TargetEnvironment}` |",
            $"| Branch | `{notification.BranchName}` |",
            $"| Author | `{notification.CommitAuthor}` |",
            $"| Commit | `{notification.CommitHash}` |",
            $"| Priority | `{notification.Priority}` |",
            $"| Timestamp | `{notification.CreatedAt:O}` |",
            "",
            "### Message",
            "",
            notification.Message,
            ""
        };

        if (!string.IsNullOrWhiteSpace(notification.BuildUrl))
        {
            lines.Add($"[View Build]({notification.BuildUrl})");
        }

        return string.Join(System.Environment.NewLine, lines);
    }

    public string GetContentType() => "text/markdown";
}

/// <summary>
/// Factory for creating notification formatters
/// </summary>
public sealed class NotificationFormatterFactory
{
    public static INotificationFormatter CreateFormatter(string formatType)
    {
        return formatType.ToLowerInvariant() switch
        {
            "json" => new JsonNotificationFormatter(),
            "text" => new TextNotificationFormatter(),
            "csv" => new CsvNotificationFormatter(),
            "markdown" or "md" => new MarkdownNotificationFormatter(),
            _ => throw new ArgumentException($"Unknown format type: {formatType}")
        };
    }
}
