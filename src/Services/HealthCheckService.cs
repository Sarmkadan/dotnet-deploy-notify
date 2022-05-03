// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Data;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Services;

/// <summary>
/// Interface for system health checking
/// </summary>
public interface IHealthCheckService
{
    /// <summary>Performs a complete system health check</summary>
    Task<HealthStatus> CheckSystemHealthAsync();

    /// <summary>Checks if a specific channel configuration is healthy</summary>
    Task<ChannelHealthStatus> CheckChannelHealthAsync(string configurationId);

    /// <summary>Checks all channel configurations</summary>
    Task<List<ChannelHealthStatus>> CheckAllChannelsAsync();

    /// <summary>Gets overall system statistics and health</summary>
    Task<SystemHealthReport> GetHealthReportAsync();
}

/// <summary>
/// Represents the overall system health status
/// </summary>
public class HealthStatus
{
    /// <summary>Overall system health (Healthy, Degraded, Unhealthy)</summary>
    public string Status { get; set; } = "Healthy";

    /// <summary>Health percentage (0-100)</summary>
    public double HealthPercentage { get; set; }

    /// <summary>Number of pending notifications</summary>
    public int PendingNotifications { get; set; }

    /// <summary>Number of failing channels</summary>
    public int FailingChannels { get; set; }

    /// <summary>Recent error messages</summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>Timestamp of the check</summary>
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Determines if the system is healthy enough to operate</summary>
    public bool IsOperational => Status != "Unhealthy" && HealthPercentage >= 50;
}

/// <summary>
/// Represents health status of a single channel
/// </summary>
public class ChannelHealthStatus
{
    /// <summary>Channel configuration ID</summary>
    public string ConfigurationId { get; set; } = string.Empty;

    /// <summary>Channel type</summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>Configuration display name</summary>
    public string ConfigName { get; set; } = string.Empty;

    /// <summary>Is the channel enabled</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Health status (Healthy, Degraded, Unhealthy)</summary>
    public string Status { get; set; } = "Unknown";

    /// <summary>Success rate percentage for recent deliveries</summary>
    public double SuccessRate { get; set; }

    /// <summary>Average delivery time in milliseconds</summary>
    public long AvgDeliveryTimeMs { get; set; }

    /// <summary>Last successful delivery time</summary>
    public DateTime? LastSuccessAt { get; set; }

    /// <summary>Last failed delivery time</summary>
    public DateTime? LastFailureAt { get; set; }

    /// <summary>Number of consecutive failures</summary>
    public int ConsecutiveFailures { get; set; }

    /// <summary>Error message if unhealthy</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Is channel operational</summary>
    public bool IsOperational => IsEnabled && SuccessRate >= 80;
}

/// <summary>
/// Complete system health report
/// </summary>
public class SystemHealthReport
{
    /// <summary>Overall system status</summary>
    public HealthStatus SystemStatus { get; set; } = new();

    /// <summary>Health status for each channel</summary>
    public List<ChannelHealthStatus> ChannelStatuses { get; set; } = new();

    /// <summary>Total notifications processed</summary>
    public int TotalNotifications { get; set; }

    /// <summary>Total delivery attempts</summary>
    public int TotalDeliveryAttempts { get; set; }

    /// <summary>Successful deliveries</summary>
    public int SuccessfulDeliveries { get; set; }

    /// <summary>Failed deliveries</summary>
    public int FailedDeliveries { get; set; }

    /// <summary>System uptime information</summary>
    public string UptimeInfo { get; set; } = "Unknown";

    /// <summary>Gets the overall success rate</summary>
    public double OverallSuccessRate =>
        TotalDeliveryAttempts > 0 ? (SuccessfulDeliveries * 100.0) / TotalDeliveryAttempts : 0;
}

/// <summary>
/// Implementation of health check service
/// </summary>
public class HealthCheckService : IHealthCheckService
{
    private readonly IChannelConfigRepository _configRepository;
    private readonly INotificationResultRepository _resultRepository;
    private readonly IWebhookDispatcher _dispatcher;
    private readonly ILogger<HealthCheckService> _logger;
    private readonly DateTime _startTime = DateTime.UtcNow;

    /// <summary>Initializes the health check service</summary>
    public HealthCheckService(
        IChannelConfigRepository configRepository,
        INotificationResultRepository resultRepository,
        IWebhookDispatcher dispatcher,
        ILogger<HealthCheckService> logger)
    {
        _configRepository = configRepository;
        _resultRepository = resultRepository;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    /// <summary>
    /// Performs a complete system health check
    /// </summary>
    public async Task<HealthStatus> CheckSystemHealthAsync()
    {
        _logger.LogInformation("Performing system health check");

        var status = new HealthStatus();

        try
        {
            // Check all channels
            var channelStatuses = await CheckAllChannelsAsync();
            var operationalChannels = channelStatuses.Count(c => c.IsOperational);
            var totalChannels = channelStatuses.Count;

            status.FailingChannels = totalChannels - operationalChannels;
            status.HealthPercentage = totalChannels > 0 ? (operationalChannels * 100.0) / totalChannels : 0;

            // Determine overall status
            if (status.HealthPercentage >= 90)
                status.Status = "Healthy";
            else if (status.HealthPercentage >= 50)
                status.Status = "Degraded";
            else
                status.Status = "Unhealthy";

            // Check for unhealthy channels
            foreach (var channel in channelStatuses.Where(c => c.Status == "Unhealthy"))
            {
                status.Errors.Add($"Channel {channel.Channel} ({channel.ConfigName}): {channel.ErrorMessage}");
            }

            _logger.LogInformation(
                "Health check completed: {Status} ({HealthPercentage:F1}%) - {OperationalChannels}/{TotalChannels} channels operational",
                status.Status,
                status.HealthPercentage,
                operationalChannels,
                totalChannels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check");
            status.Status = "Unhealthy";
            status.Errors.Add(ex.Message);
        }

        return status;
    }

    /// <summary>
    /// Checks the health of a specific channel configuration
    /// </summary>
    public async Task<ChannelHealthStatus> CheckChannelHealthAsync(string configurationId)
    {
        var config = await _configRepository.GetByIdAsync(configurationId);
        if (config == null)
        {
            return new ChannelHealthStatus
            {
                ConfigurationId = configurationId,
                Status = "Unhealthy",
                ErrorMessage = "Configuration not found"
            };
        }

        var healthStatus = new ChannelHealthStatus
        {
            ConfigurationId = config.Id,
            Channel = config.ChannelType,
            ConfigName = config.DisplayName,
            IsEnabled = config.IsEnabled
        };

        if (!config.IsEnabled)
        {
            healthStatus.Status = "Disabled";
            return healthStatus;
        }

        try
        {
            // Test webhook connectivity
            var isReachable = await _dispatcher.ValidateWebhookAsync(config.WebhookUrl, config.TimeoutMs);

            if (!isReachable)
            {
                healthStatus.Status = "Unhealthy";
                healthStatus.ErrorMessage = "Webhook validation failed";
                return healthStatus;
            }

            // Get recent delivery statistics
            var recentResults = await _resultRepository.GetByChannelAsync(config.ChannelType, 100);
            if (recentResults.Any())
            {
                var successCount = recentResults.Count(r => r.IsSuccessful);
                healthStatus.SuccessRate = (successCount * 100.0) / recentResults.Count;
                healthStatus.AvgDeliveryTimeMs = (long)recentResults.Average(r => r.DurationMs);
                healthStatus.LastSuccessAt = recentResults
                    .Where(r => r.IsSuccessful)
                    .MaxBy(r => r.AttemptedAt)?
                    .AttemptedAt;
                healthStatus.LastFailureAt = recentResults
                    .Where(r => !r.IsSuccessful)
                    .MaxBy(r => r.AttemptedAt)?
                    .AttemptedAt;
            }

            // Determine health status
            if (healthStatus.SuccessRate >= 90)
                healthStatus.Status = "Healthy";
            else if (healthStatus.SuccessRate >= 70)
                healthStatus.Status = "Degraded";
            else
                healthStatus.Status = "Unhealthy";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking health for channel {ConfigName}", config.DisplayName);
            healthStatus.Status = "Unhealthy";
            healthStatus.ErrorMessage = ex.Message;
        }

        return healthStatus;
    }

    /// <summary>
    /// Checks health of all channel configurations
    /// </summary>
    public async Task<List<ChannelHealthStatus>> CheckAllChannelsAsync()
    {
        var allConfigs = await _configRepository.GetAllAsync(0, 1000);
        var healthStatuses = new List<ChannelHealthStatus>();

        foreach (var config in allConfigs)
        {
            var status = await CheckChannelHealthAsync(config.Id);
            healthStatuses.Add(status);
        }

        return healthStatuses;
    }

    /// <summary>
    /// Generates a complete health report
    /// </summary>
    public async Task<SystemHealthReport> GetHealthReportAsync()
    {
        var report = new SystemHealthReport
        {
            SystemStatus = await CheckSystemHealthAsync(),
            ChannelStatuses = await CheckAllChannelsAsync(),
            UptimeInfo = $"Running for {(DateTime.UtcNow - _startTime).TotalHours:F2} hours"
        };

        // Get statistics
        var allResults = await _resultRepository.GetAllAsync(0, 10000);
        if (allResults.Any())
        {
            report.TotalDeliveryAttempts = allResults.Count;
            report.SuccessfulDeliveries = allResults.Count(r => r.IsSuccessful);
            report.FailedDeliveries = allResults.Count(r => r.Status == DeliveryStatus.Failed);
        }

        return report;
    }
}
