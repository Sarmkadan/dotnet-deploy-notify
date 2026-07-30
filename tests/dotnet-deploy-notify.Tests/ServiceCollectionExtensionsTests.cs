#nullable enable
using DotNetDeployNotify.BackgroundWorkers;
using DotNetDeployNotify.Caching;
using DotNetDeployNotify.CLI;
using DotNetDeployNotify.DependencyInjection;
using DotNetDeployNotify.Events;
using DotNetDeployNotify.Formatters;
using DotNetDeployNotify.Integration;
using DotNetDeployNotify.Middleware;
using DotNetDeployNotify.Serialization;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DotNetDeployNotify.Tests;

public class ServiceCollectionExtensionsTests
{
    private readonly ILogger<NotificationPipeline> _logger = Substitute.For<ILogger<NotificationPipeline>>();

    [Fact]
    public void AddFormattingServices_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddFormattingServices();
        services.Any(d => d.ServiceType == typeof(NotificationFormatterFactory)).Should().BeTrue();
    }

    [Fact]
    public void AddSerializationServices_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddSerializationServices();
        services.Any(d => d.ServiceType == typeof(JsonSerializationHelper)).Should().BeTrue();
    }

    [Fact]
    public void AddIntegrationServices_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddLogging(); // Need logging for CircuitBreakerRegistry
        services.AddIntegrationServices();
        services.Any(d => d.ServiceType == typeof(WebhookClient)).Should().BeTrue();
    }

    [Fact]
    public void AddCliServices_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddCliServices();
        services.Any(d => d.ServiceType == typeof(CommandParser)).Should().BeTrue();
        services.Any(d => d.ServiceType == typeof(CommandHandler)).Should().BeTrue();
    }

    [Fact]
    public void AddCliServices_NullServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddCliServices());
    }

    [Fact]
    public void AddCachingServices_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddCachingServices();
        services.Any(d => d.ServiceType == typeof(ICacheService)).Should().BeTrue();
    }

    [Fact]
    public void AddMiddlewareServices_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddMiddlewareServices();
        services.Any(d => d.ServiceType == typeof(NotificationPipeline)).Should().BeTrue();
    }

    [Fact]
    public void ConfigureNotificationPipeline_ConfiguresSuccessfully()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<DotNetDeployNotify.Data.IChannelConfigRepository>(Substitute.For<DotNetDeployNotify.Data.IChannelConfigRepository>());
        services.AddMiddlewareServices();
        var sp = services.BuildServiceProvider();
        var pipeline = new NotificationPipeline(_logger);
        
        var configuredPipeline = pipeline.ConfigureNotificationPipeline(sp);
        
        configuredPipeline.Should().BeSameAs(pipeline);
    }

    [Fact]
    public void ConfigureNotificationPipeline_NullInputs_ThrowsArgumentNullException()
    {
        var sp = new ServiceCollection().BuildServiceProvider();
        Assert.Throws<ArgumentNullException>(() => ((NotificationPipeline)null!).ConfigureNotificationPipeline(sp));
        Assert.Throws<ArgumentNullException>(() => new NotificationPipeline(_logger).ConfigureNotificationPipeline(null!));
    }

    [Fact]
    public void AddConfiguredHttpClient_AddsClientAndConfigures()
    {
        var services = new ServiceCollection();
        var builder = services.AddConfiguredHttpClient("testClient", 45);
        
        builder.Should().NotBeNull();
        services.Any(d => d.ServiceType == typeof(IHttpClientFactory)).Should().BeTrue();
    }

    [Fact]
    public void AddEventBusServices_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddEventBusServices();
        services.Any(d => d.ServiceType == typeof(IEventBus)).Should().BeTrue();
        services.Any(d => d.ServiceType == typeof(NotificationObservable)).Should().BeTrue();
    }

    [Fact]
    public void AddBackgroundWorkers_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddBackgroundWorkers();
        // Hosted services are registered as IHostedService
        services.Any(d => d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService)).Should().BeTrue();
    }
}
