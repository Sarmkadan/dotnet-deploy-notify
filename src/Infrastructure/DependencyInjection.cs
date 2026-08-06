#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Data;
using DotNetDeployNotify.Integration;
using DotNetDeployNotify.Notifications;
using DotNetDeployNotify.Persistence;
using DotNetDeployNotify.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DotNetDeployNotify.Configuration;

namespace DotNetDeployNotify.Infrastructure;

/// <summary>
/// Selects and configures the storage backend used for deployment history when registering
/// notification services via <see cref="DependencyInjection.AddNotificationServices"/>
/// </summary>
public sealed class NotificationServicesOptions
{
    /// <summary>
    /// The JSON file path to persist deployment history to, or <see langword="null"/> to keep history
    /// in memory only (the default)
    /// </summary>
    public string? HistoryFilePath { get; private set; }

    /// <summary>
    /// Selects the JSON-file-backed deployment history repository, so history survives process restarts
    /// </summary>
    /// <param name="path">Path of the JSON file used to store deployment history</param>
    /// <returns>The same options instance, to allow chaining</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is <see langword="null"/> or empty</exception>
    public NotificationServicesOptions UseFileHistory(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        HistoryFilePath = path;
        return this;
    }
}

/// <summary>
/// Extension methods for registering services in dependency injection container
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds all notification services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="configureOptions">Optional callback to select the deployment history storage backend.</param>
    /// <returns>The service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddNotificationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<NotificationServicesOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var notificationServicesOptions = new NotificationServicesOptions();
        configureOptions?.Invoke(notificationServicesOptions);

        // Core services
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IPayloadBuilder, PayloadBuilder>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDryRunRenderer, DryRunRenderer>();

        // Configuration
        services.AddOptions<DotnetDeployNotifyOptions>()
            .Bind(configuration.GetSection(DotnetDeployNotifyOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Deployment history tracking: file-backed when UseFileHistory(path) was configured, in-memory otherwise
        services.AddSingleton<IDeploymentHistoryRepository>(_ => notificationServicesOptions.HistoryFilePath is { } historyFilePath
            ? new JsonFileDeploymentHistoryRepository(historyFilePath)
            : new InMemoryDeploymentHistoryRepository());
        services.AddSingleton<IDeploymentHistoryService, DeploymentHistoryService>();

        // Rollback notifications
        services.AddScoped<IRollbackNotificationService, RollbackNotificationService>();

        // Custom template engine
        services.AddSingleton<ICustomTemplateEngine, CustomTemplateEngine>();

        // Data access
        services.AddSingleton<INotificationRepository, NotificationRepository>();
        services.AddSingleton<INotificationResultRepository, NotificationResultRepository>();

        // Register channel config repository
        services.AddSingleton<IChannelConfigRepository>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<DotnetDeployNotifyOptions>>().Value;
            var logger = sp.GetRequiredService<ILogger<ChannelConfigRepository>>();
            
            // Map the new options to the structure expected by ChannelConfigRepository
            var initialChannels = BuildChannelConfigsFromSettings(options.Notification);
            
            return new ChannelConfigRepository(logger, initialChannels);
        });

        // HTTP client for webhooks.
        // Registered as a typed client so the configured HttpClient (User-Agent,
        // X-Client-Name headers) is the one actually injected. A separate
        // AddScoped<IWebhookDispatcher, WebhookDispatcher> registration would
        // resolve the default unnamed HttpClient and silently drop these headers.
        services.AddHttpClient<IWebhookDispatcher, WebhookDispatcher>()
            .ConfigureHttpClient(client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("DotNetDeployNotify/1.0");
                client.DefaultRequestHeaders.Add("X-Client-Name", "dotnet-deploy-notify");
            });

        // Register integration services
        services.AddScoped<IWebhookClient, WebhookClientAdapter>();

        // Register notification channels
        services.AddScoped<INotificationChannel, SlackChannel>();
        services.AddScoped<INotificationChannel, DiscordChannel>();
        services.AddScoped<INotificationChannel, TelegramChannel>();
    services.AddScoped<INotificationChannel, TeamsChannel>();
        services.AddScoped<NotificationDispatcher>();

        return services;
    }

    private static List<DotNetDeployNotify.Core.Models.ChannelConfiguration> BuildChannelConfigsFromSettings(NotificationConfig config)
    {
        var channels = new List<DotNetDeployNotify.Core.Models.ChannelConfiguration>();

        foreach (var (envName, envConfig) in config.EnvironmentChannels)
        {
            if (string.IsNullOrWhiteSpace(envConfig.WebhookUrl))
                continue;

            if (!Enum.TryParse<DotNetDeployNotify.Core.NotificationChannel>(envConfig.ChannelType, ignoreCase: true, out var channelType))
                channelType = DotNetDeployNotify.Core.NotificationChannel.Slack;

            var allowedEnvs = new List<DotNetDeployNotify.Core.Environment>();
            if (Enum.TryParse<DotNetDeployNotify.Core.Environment>(envName, ignoreCase: true, out var parsedEnv))
                allowedEnvs.Add(parsedEnv);

            channels.Add(new DotNetDeployNotify.Core.Models.ChannelConfiguration
            {
                ChannelType = channelType,
                WebhookUrl = envConfig.WebhookUrl,
                TargetId = envConfig.TargetId,
                DisplayName = string.IsNullOrWhiteSpace(envConfig.DisplayName)
                    ? $"{envName}-{channelType}"
                    : envConfig.DisplayName,
                AllowedEnvironments = allowedEnvs,
                MaxRetries = config.MaxRetries,
                TimeoutMs = config.WebhookTimeoutMs
            });
        }

        return channels;
    }

    /// <summary>
    /// Adds logging configuration.
    /// </summary>
    /// <param name="logging">The logging builder.</param>
    /// <returns>The logging builder.</returns>
    public static ILoggingBuilder AddDeployNotifyLogging(this ILoggingBuilder logging)
    {
        ArgumentNullException.ThrowIfNull(logging);
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();
        return logging;
    }
}
