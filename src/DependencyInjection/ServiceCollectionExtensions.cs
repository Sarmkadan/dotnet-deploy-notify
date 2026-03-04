#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.BackgroundWorkers;
using DotNetDeployNotify.Caching;
using DotNetDeployNotify.CLI;
using DotNetDeployNotify.Events;
using DotNetDeployNotify.Formatters;
using DotNetDeployNotify.Integration;
using DotNetDeployNotify.Middleware;
using DotNetDeployNotify.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetDeployNotify.DependencyInjection;

/// <summary>
/// Extension methods for configuring application services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all CLI-related services
    /// </summary>
    public static IServiceCollection AddCliServices(this IServiceCollection services)
    {
        services.AddSingleton<CommandParser>();
        services.AddScoped<CommandHandler>();
        return services;
    }

    /// <summary>
    /// Registers caching services
    /// </summary>
    public static IServiceCollection AddCachingServices(this IServiceCollection services)
    {
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddScoped<CacheKeyBuilder>();
        return services;
    }

    /// <summary>
    /// Registers formatting services
    /// </summary>
    public static IServiceCollection AddFormattingServices(this IServiceCollection services)
    {
        services.AddSingleton<NotificationFormatterFactory>();
        services.AddScoped<INotificationFormatter, JsonNotificationFormatter>();
        return services;
    }

    /// <summary>
    /// Registers serialization services
    /// </summary>
    public static IServiceCollection AddSerializationServices(this IServiceCollection services)
    {
        services.AddSingleton<JsonSerializationHelper>();
        services.AddSingleton<SafeJsonParser>();
        return services;
    }

    /// <summary>
    /// Registers event bus and event handlers
    /// </summary>
    public static IServiceCollection AddEventBusServices(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        services.AddScoped<IEventHandler<NotificationCreatedEvent>, NotificationCreatedEventHandler>();
        services.AddScoped<IEventHandler<ChannelDeliveryFailedEvent>, ChannelDeliveryFailedEventHandler>();
        services.AddScoped<NotificationObservable>();
        return services;
    }

    /// <summary>
    /// Registers middleware and interceptors
    /// </summary>
    public static IServiceCollection AddMiddlewareServices(this IServiceCollection services)
    {
        services.AddScoped<ErrorHandlingInterceptor>();
        services.AddScoped<RateLimitingInterceptor>();
        services.AddScoped<LoggingInterceptor>();
        services.AddScoped<PerformanceInterceptor>();
        services.AddScoped<ValidationProcessor>();
        services.AddScoped<EnrichmentProcessor>();
        services.AddScoped<FilterProcessor>();
        services.AddScoped<SanitizationProcessor>();
        services.AddScoped<NotificationPipeline>();
        return services;
    }

    /// <summary>
    /// Registers integration services
    /// </summary>
    public static IServiceCollection AddIntegrationServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddScoped<DotNetDeployNotify.Integration.IHttpClientFactory, DefaultHttpClientFactory>();
        services.AddScoped<WebhookPayloadBuilderFactory>();
        services.AddScoped<WebhookClient>();
        services.AddScoped<RetryableHttpClient>();
        return services;
    }

    /// <summary>
    /// Registers background workers
    /// </summary>
    public static IServiceCollection AddBackgroundWorkers(this IServiceCollection services)
    {
        services.AddHostedService<NotificationProcessingWorker>();
        services.AddHostedService<HealthCheckWorker>();
        services.AddHostedService<ScheduledTaskWorker>();
        return services;
    }

    /// <summary>
    /// Registers the complete notification pipeline
    /// </summary>
    public static NotificationPipeline ConfigureNotificationPipeline(
        this NotificationPipeline pipeline,
        IServiceProvider serviceProvider)
    {
        pipeline
            .Use(serviceProvider.GetRequiredService<ValidationProcessor>())
            .Use(serviceProvider.GetRequiredService<EnrichmentProcessor>())
            .Use(serviceProvider.GetRequiredService<FilterProcessor>())
            .Use(serviceProvider.GetRequiredService<SanitizationProcessor>());

        return pipeline;
    }

    /// <summary>
    /// Configures HTTP client with default headers and timeout
    /// </summary>
    public static IHttpClientBuilder AddConfiguredHttpClient(
        this IServiceCollection services,
        string name = "default",
        int timeoutSeconds = 30)
    {
        return services.AddHttpClient(name)
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                client.DefaultRequestHeaders.Add("User-Agent", "DotNetDeployNotify/1.0");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
    }

    /// <summary>
    /// Configures event handlers registration
    /// </summary>
    public static IServiceCollection RegisterEventHandlers(
        this IServiceCollection services,
        IEventBus eventBus,
        IServiceProvider serviceProvider)
    {
        var createdHandler = serviceProvider.GetRequiredService<IEventHandler<NotificationCreatedEvent>>();
        eventBus.Subscribe(createdHandler);

        var failedHandler = serviceProvider.GetRequiredService<IEventHandler<ChannelDeliveryFailedEvent>>();
        eventBus.Subscribe(failedHandler);

        return services;
    }
}

/// <summary>
/// Configuration builder for fluent service setup
/// </summary>
public sealed class ServiceConfigurationBuilder
{
    private readonly IServiceCollection _services;

    public ServiceConfigurationBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public ServiceConfigurationBuilder WithCaching()
    {
        _services.AddCachingServices();
        return this;
    }

    public ServiceConfigurationBuilder WithFormatting()
    {
        _services.AddFormattingServices();
        return this;
    }

    public ServiceConfigurationBuilder WithSerialization()
    {
        _services.AddSerializationServices();
        return this;
    }

    public ServiceConfigurationBuilder WithEventBus()
    {
        _services.AddEventBusServices();
        return this;
    }

    public ServiceConfigurationBuilder WithMiddleware()
    {
        _services.AddMiddlewareServices();
        return this;
    }

    public ServiceConfigurationBuilder WithIntegration()
    {
        _services.AddIntegrationServices();
        return this;
    }

    public ServiceConfigurationBuilder WithBackgroundWorkers()
    {
        _services.AddBackgroundWorkers();
        return this;
    }

    public ServiceConfigurationBuilder WithCliSupport()
    {
        _services.AddCliServices();
        return this;
    }

    public ServiceConfigurationBuilder WithAll()
    {
        return this
            .WithCaching()
            .WithFormatting()
            .WithSerialization()
            .WithEventBus()
            .WithMiddleware()
            .WithIntegration()
            .WithBackgroundWorkers()
            .WithCliSupport();
    }

    public IServiceCollection Build() => _services;
}
