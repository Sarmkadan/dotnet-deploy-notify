#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Export;

/// <summary>
/// Service for exporting notifications to various formats
/// </summary>
public interface IExportService
{
    Task<string> ExportAsJsonAsync(List<DeploymentNotification> notifications);
    Task<string> ExportAsCsvAsync(List<DeploymentNotification> notifications);
    Task<byte[]> ExportAsZipAsync(List<DeploymentNotification> notifications);
    Task SaveToFileAsync(List<DeploymentNotification> notifications, string filePath, string format);
}

/// <summary>
/// Default export service implementation
/// </summary>
public sealed class ExportService : IExportService
{
    private readonly ILogger<ExportService> _logger;

    public ExportService(ILogger<ExportService> logger)
    {
        _logger = logger;
    }

    public Task<string> ExportAsJsonAsync(List<DeploymentNotification> notifications)
    {
        ArgumentNullException.ThrowIfNull(notifications);
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(
                notifications,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            _logger.LogInformation("Exported {Count} notifications as JSON", notifications.Count);
            return Task.FromResult(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export as JSON");
            throw;
        }
    }

    public Task<string> ExportAsCsvAsync(List<DeploymentNotification> notifications)
    {
        ArgumentNullException.ThrowIfNull(notifications);
        try
        {
            var csv = new StringBuilder();

            // Header
            csv.AppendLine("ID,Project,Version,Status,Environment,Branch,Author,Message,CreatedAt,Channels");

            // Rows
            foreach (var notification in notifications)
            {
                var channels = string.Join(";", notification.Channels);
                csv.AppendLine($"\"{notification.Id}\",\"{notification.ProjectName}\",\"{notification.Version}\"," +
                    $"\"{notification.Status}\",\"{notification.TargetEnvironment}\",\"{notification.BranchName}\"," +
                    $"\"{notification.CommitAuthor}\",\"{notification.Message}\",\"{notification.CreatedAt:O}\",\"{channels}\"");
            }

            _logger.LogInformation("Exported {Count} notifications as CSV", notifications.Count);
            return Task.FromResult(csv.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export as CSV");
            throw;
        }
    }

    public Task<byte[]> ExportAsZipAsync(List<DeploymentNotification> notifications)
    {
        ArgumentNullException.ThrowIfNull(notifications);
        // Note: This would require System.IO.Compression nuget package
        _logger.LogWarning("ZIP export not yet implemented");
        throw new NotImplementedException("ZIP export requires System.IO.Compression package");
    }

    public async Task SaveToFileAsync(List<DeploymentNotification> notifications, string filePath, string format)
    {
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentException.ThrowIfNullOrEmpty(format);
        try
        {
            string content = format.ToLowerInvariant() switch
            {
                "json" => await ExportAsJsonAsync(notifications),
                "csv" => await ExportAsCsvAsync(notifications),
                _ => throw new ArgumentException($"Unknown format: {format}")
            };

            await System.IO.File.WriteAllTextAsync(filePath, content);
            _logger.LogInformation("Exported {Count} notifications to {FilePath}", notifications.Count, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save export to file: {FilePath}", filePath);
            throw;
        }
    }
}

/// <summary>
/// Report generator for notifications
/// </summary>
public sealed class NotificationReportGenerator
{
    private readonly ILogger<NotificationReportGenerator> _logger;

    public NotificationReportGenerator(ILogger<NotificationReportGenerator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generates a summary report of notifications
    /// </summary>
    public NotificationReport GenerateReport(List<DeploymentNotification> notifications)
    {
        if (notifications is null || notifications.Count == 0)
            return new NotificationReport();

        var report = new NotificationReport
        {
            TotalNotifications = notifications.Count,
            GeneratedAt = DateTime.UtcNow,
            SuccessfulCount = notifications.Count(n => n.Status == BuildStatus.Success || n.Status == BuildStatus.DeploymentSuccess),
            FailedCount = notifications.Count(n => n.Status == BuildStatus.Failed || n.Status == BuildStatus.DeploymentFailed),
            CancelledCount = notifications.Count(n => n.Status == BuildStatus.Cancelled),
            EnvironmentBreakdown = GetEnvironmentBreakdown(notifications),
            StatusBreakdown = GetStatusBreakdown(notifications),
            ChannelBreakdown = GetChannelBreakdown(notifications),
            TopProjects = GetTopProjects(notifications, 10)
        };

        report.AverageDuration = notifications.Where(n => n.DurationSeconds.HasValue)
            .Average(n => n.DurationSeconds ?? 0);

        _logger.LogInformation("Generated report: {Total} total, {Success} success, {Failed} failed",
            report.TotalNotifications, report.SuccessfulCount, report.FailedCount);

        return report;
    }

    private Dictionary<string, int> GetEnvironmentBreakdown(List<DeploymentNotification> notifications)
    {
        return notifications
            .GroupBy(n => n.TargetEnvironment.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private Dictionary<string, int> GetStatusBreakdown(List<DeploymentNotification> notifications)
    {
        return notifications
            .GroupBy(n => n.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private Dictionary<string, int> GetChannelBreakdown(List<DeploymentNotification> notifications)
    {
        var breakdown = new Dictionary<string, int>();

        foreach (var notification in notifications)
        {
            foreach (var channel in notification.Channels)
            {
                var key = channel.ToString();
                if (breakdown.ContainsKey(key))
                    breakdown[key]++;
                else
                    breakdown[key] = 1;
            }
        }

        return breakdown;
    }

    private List<(string Project, int Count)> GetTopProjects(List<DeploymentNotification> notifications, int limit)
    {
        return notifications
            .GroupBy(n => n.ProjectName)
            .OrderByDescending(g => g.Count())
            .Take(limit)
            .Select(g => (g.Key, g.Count()))
            .ToList();
    }
}

/// <summary>
/// Report containing aggregated notification statistics
/// </summary>
public sealed class NotificationReport
{
    public int TotalNotifications { get; set; }
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }
    public int CancelledCount { get; set; }
    public double AverageDuration { get; set; }
    public DateTime GeneratedAt { get; set; }
    public Dictionary<string, int> EnvironmentBreakdown { get; set; } = new();
    public Dictionary<string, int> StatusBreakdown { get; set; } = new();
    public Dictionary<string, int> ChannelBreakdown { get; set; } = new();
    public List<(string Project, int Count)> TopProjects { get; set; } = new();

    public double SuccessRate => TotalNotifications > 0 ? (double)SuccessfulCount / TotalNotifications * 100 : 0;
    public double FailureRate => TotalNotifications > 0 ? (double)FailedCount / TotalNotifications * 100 : 0;

    public override string ToString()
    {
        return $"Report: {TotalNotifications} total, {SuccessRate:F1}% success, {FailureRate:F1}% failure";
    }
}
