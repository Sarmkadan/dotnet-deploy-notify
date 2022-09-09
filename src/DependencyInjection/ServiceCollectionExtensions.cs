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
	/// Registers all CLI-related services including command parsing and handling capabilities.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
	public static IServiceCollection AddCliServices(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddSingleton<CommandParser>();
		services.AddScoped<CommandHandler>();
		return services;
	}

	/// <summary>
	/// Registers caching services including memory cache implementation and key builder.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
	public static IServiceCollection AddCachingServices(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddSingleton<ICacheService, MemoryCacheService>();
		services.AddScoped<CacheKeyBuilder>();
		return services;
	}

	/// <summary>
	/// Registers formatting services including formatter factory and implementations.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
	public static IServiceCollection AddFormattingServices(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddSingleton<NotificationFormatterFactory>();
		services.AddScoped<INotificationFormatter, JsonNotificationFormatter>();
		return services;
	}

	/// <summary>
	/// Registers serialization services including JSON serialization helpers.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
	public static IServiceCollection AddSerializationServices(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddSingleton<JsonSerializationHelper>();
		services.AddSingleton<SafeJsonParser>();
		return services;
	}

	/// <summary>
	/// Registers event bus infrastructure and event handlers for notification system.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
	public static IServiceCollection AddEventBusServices(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddSingleton<IEventBus, InMemoryEventBus>();
		services.AddScoped<IEventHandler<NotificationCreatedEvent>, NotificationCreatedEventHandler>();
		services.AddScoped<IEventHandler<ChannelDeliveryFailedEvent>, ChannelDeliveryFailedEventHandler>();
		services.AddScoped<NotificationObservable>();
		return services;
	}

	/// <summary>
	/// Registers middleware and interceptors for the notification processing pipeline.
	/// Includes error handling, rate limiting, logging, performance monitoring, and validation.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
	public static IServiceCollection AddMiddlewareServices(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

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
	/// Registers integration services for webhook delivery and HTTP communication.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
	public static IServiceCollection AddIntegrationServices(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddHttpClient();
		services.AddScoped<DotNetDeployNotify.Integration.IHttpClientFactory, DefaultHttpClientFactory>();
		services.AddScoped<WebhookPayloadBuilderFactory>();
		services.AddScoped<WebhookClient>();
		services.AddScoped<RetryableHttpClient>();
		return services;
	}

	/// <summary>
	/// Registers background workers for processing notifications and system tasks.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
	public static IServiceCollection AddBackgroundWorkers(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddHostedService<NotificationProcessingWorker>();
		services.AddHostedService<HealthCheckWorker>();
		services.AddHostedService<ScheduledTaskWorker>();
		return services;
	}

	/// <summary>
	/// Configures the notification processing pipeline with required processors.
	/// </summary>
	/// <param name="pipeline">The <see cref="NotificationPipeline"/> instance to configure.</param>
	/// <param name="serviceProvider">The <see cref="IServiceProvider"/> to resolve services from.</param>
	/// <returns>The configured <see cref="NotificationPipeline"/> instance.</returns>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="pipeline"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.
	/// </exception>
	public static NotificationPipeline ConfigureNotificationPipeline(
		this NotificationPipeline pipeline,
		IServiceProvider serviceProvider)
	{
		ArgumentNullException.ThrowIfNull(pipeline);
		ArgumentNullException.ThrowIfNull(serviceProvider);

		pipeline
			.Use(serviceProvider.GetRequiredService<ValidationProcessor>())
			.Use(serviceProvider.GetRequiredService<EnrichmentProcessor>())
			.Use(serviceProvider.GetRequiredService<FilterProcessor>())
			.Use(serviceProvider.GetRequiredService<SanitizationProcessor>());

		return pipeline;
	}

	/// <summary>
	/// Configures an <see cref="IHttpClientBuilder"/> with default headers and timeout settings.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to add the HTTP client to.</param>
	/// <param name="name">The name of the HTTP client configuration.</param>
	/// <param name="timeoutSeconds">The timeout duration in seconds for HTTP requests.</param>
	/// <returns>An <see cref="IHttpClientBuilder"/> for further configuration.</returns>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="services"/> is <see langword="null"/>.
	/// </exception>
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
	/// Registers and subscribes event handlers to the event bus.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <param name="eventBus">The <see cref="IEventBus"/> to subscribe handlers to.</param>
	/// <param name="serviceProvider">The <see cref="IServiceProvider"/> to resolve services from.</param>
	/// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="services"/>, <paramref name="eventBus"/>, or <paramref name="serviceProvider"/> is <see langword="null"/>.
	/// </exception>
	public static IServiceCollection RegisterEventHandlers(
		this IServiceCollection services,
		IEventBus eventBus,
		IServiceProvider serviceProvider)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(eventBus);
		ArgumentNullException.ThrowIfNull(serviceProvider);

		var createdHandler = serviceProvider.GetRequiredService<IEventHandler<NotificationCreatedEvent>>();
		eventBus.Subscribe(createdHandler);

		var failedHandler = serviceProvider.GetRequiredService<IEventHandler<ChannelDeliveryFailedEvent>>();
		eventBus.Subscribe(failedHandler);

		return services;
	}
}

/// <summary>
/// Configuration builder for fluent service setup and composition.
/// </summary>
public sealed class ServiceConfigurationBuilder
{
	private readonly IServiceCollection _services;

	/// <summary>
	/// Initializes a new instance of the <see cref="ServiceConfigurationBuilder"/> class.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to build upon.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
	public ServiceConfigurationBuilder(IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);
		_services = services;
	}

	/// <summary>
	/// Adds caching services to the configuration.
	/// </summary>
	/// <returns>The same <see cref="ServiceConfigurationBuilder"/> instance for chaining.</returns>
	public ServiceConfigurationBuilder WithCaching()
	{
		_services.AddCachingServices();
		return this;
	}

	/// <summary>
	/// Adds formatting services to the configuration.
	/// </summary>
	/// <returns>The same <see cref="ServiceConfigurationBuilder"/> instance for chaining.</returns>
	public ServiceConfigurationBuilder WithFormatting()
	{
		_services.AddFormattingServices();
		return this;
	}

	/// <summary>
	/// Adds serialization services to the configuration.
	/// </summary>
	/// <returns>The same <see cref="ServiceConfigurationBuilder"/> instance for chaining.</returns>
	public ServiceConfigurationBuilder WithSerialization()
	{
		_services.AddSerializationServices();
		return this;
	}

	/// <summary>
	/// Adds event bus services to the configuration.
	/// </summary>
	/// <returns>The same <see cref="ServiceConfigurationBuilder"/> instance for chaining.</returns>
	public ServiceConfigurationBuilder WithEventBus()
	{
		_services.AddEventBusServices();
		return this;
	}

	/// <summary>
	/// Adds middleware services to the configuration.
	/// </summary>
	/// <returns>The same <see cref="ServiceConfigurationBuilder"/> instance for chaining.</returns>
	public ServiceConfigurationBuilder WithMiddleware()
	{
		_services.AddMiddlewareServices();
		return this;
	}

	/// <summary>
	/// Adds integration services to the configuration.
	/// </summary>
	/// <returns>The same <see cref="ServiceConfigurationBuilder"/> instance for chaining.</returns>
	public ServiceConfigurationBuilder WithIntegration()
	{
		_services.AddIntegrationServices();
		return this;
	}

	/// <summary>
	/// Adds background workers to the configuration.
	/// </summary>
	/// <returns>The same <see cref="ServiceConfigurationBuilder"/> instance for chaining.</returns>
	public ServiceConfigurationBuilder WithBackgroundWorkers()
	{
		_services.AddBackgroundWorkers();
		return this;
	}

	/// <summary>
	/// Adds CLI support services to the configuration.
	/// </summary>
	/// <returns>The same <see cref="ServiceConfigurationBuilder"/> instance for chaining.</returns>
	public ServiceConfigurationBuilder WithCliSupport()
	{
		_services.AddCliServices();
		return this;
	}

	/// <summary>
	/// Adds all available services to the configuration.
	/// </summary>
	/// <returns>The same <see cref="ServiceConfigurationBuilder"/> instance for chaining.</returns>
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

	/// <summary>
	/// Builds and returns the configured <see cref="IServiceCollection"/>.
	/// </summary>
	/// <returns>The configured <see cref="IServiceCollection"/>.</returns>
	public IServiceCollection Build() => _services;
}