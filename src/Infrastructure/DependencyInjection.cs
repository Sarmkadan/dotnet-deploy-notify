// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Data;
using DotNetDeployNotify.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Infrastructure;

/// <summary>
/// Extension methods for registering services in dependency injection container
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds all notification services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddNotificationServices(this IServiceCollection services)
    {
        // Core services
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IPayloadBuilder, PayloadBuilder>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IWebhookDispatcher, WebhookDispatcher>();

        // Data access
        services.AddSingleton<INotificationRepository, NotificationRepository>();
        services.AddSingleton<IChannelConfigRepository, ChannelConfigRepository>();
        services.AddSingleton<INotificationResultRepository, NotificationResultRepository>();

        // HTTP client for webhooks
        services.AddHttpClient<WebhookDispatcher>()
            .ConfigureHttpClient(client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("DotNetDeployNotify/1.0");
                client.DefaultRequestHeaders.Add("X-Client-Name", "dotnet-deploy-notify");
            });

        return services;
    }

    /// <summary>
    /// Adds logging configuration
    /// </summary>
    public static ILoggingBuilder AddDeployNotifyLogging(this ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();
        return logging;
    }
}

/// <summary>
/// Configuration settings for the notification system
/// </summary>
public class NotificationConfig
{
    /// <summary>Section name in appsettings.json</summary>
    public const string SectionName = "NotificationService";

    /// <summary>Maximum number of retry attempts</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>Timeout for webhook calls in milliseconds</summary>
    public int WebhookTimeoutMs { get; set; } = 10000;

    /// <summary>Delay between retry attempts in milliseconds</summary>
    public int RetryDelayMs { get; set; } = 5000;

    /// <summary>Whether to process notifications automatically</summary>
    public bool AutoProcessNotifications { get; set; } = true;

    /// <summary>Interval to check for pending notifications in seconds</summary>
    public int ProcessingIntervalSeconds { get; set; } = 30;

    /// <summary>Storage type (InMemory, File, Database, etc.)</summary>
    public string StorageType { get; set; } = "InMemory";

    /// <summary>Log level for the service</summary>
    public string LogLevel { get; set; } = "Information";

    /// <summary>Path to data storage file (if using File storage)</summary>
    public string? StoragePath { get; set; }

    /// <summary>Whether to include full commit details in messages</summary>
    public bool IncludeCommitDetails { get; set; } = true;

    /// <summary>Whether to include build URLs in messages</summary>
    public bool IncludeBuildUrl { get; set; } = true;

    /// <summary>Default notification priority level</summary>
    public string DefaultPriority { get; set; } = "Normal";

    /// <summary>Enable audit logging of all deliveries</summary>
    public bool EnableAuditLogging { get; set; } = true;

    /// <summary>Days to retain delivery result history</summary>
    public int RetentionDays { get; set; } = 30;
}
