#nullable enable

using DotNetDeployNotify.Canary;
using DotNetDeployNotify.Configuration;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace DotNetDeployNotify.Tests.Infrastructure;

public class CanaryServiceExtensionsTests
{
    [Fact]
    public void AddCanaryDeployment_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;
        Action<CanaryOptions>? configure = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddCanaryDeployment(configure));
    }

    [Fact]
    public void AddCanaryDeployment_WithServicesAndNullConfigure_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddCanaryDeployment(configure: null);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddCanaryDeployment_WithServicesAndConfigure_ConfiguresOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedOptions = new CanaryOptions
        {
            Enabled = false,
            AutoRollbackOnFailure = false,
            AutoAdvanceOnSuccess = true,
            LinearStepCount = 10,
            StepSoakDuration = TimeSpan.FromMinutes(15),
            MaxDeploymentDuration = TimeSpan.FromHours(6),
            AlertPriority = NotificationPriority.Low
        };

        // Act
        services.AddCanaryDeployment(options =>
        {
            options.Enabled = expectedOptions.Enabled;
            options.AutoRollbackOnFailure = expectedOptions.AutoRollbackOnFailure;
            options.AutoAdvanceOnSuccess = expectedOptions.AutoAdvanceOnSuccess;
            options.LinearStepCount = expectedOptions.LinearStepCount;
            options.StepSoakDuration = expectedOptions.StepSoakDuration;
            options.MaxDeploymentDuration = expectedOptions.MaxDeploymentDuration;
            options.AlertPriority = expectedOptions.AlertPriority;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var actualOptions = serviceProvider.GetRequiredService<IOptions<CanaryOptions>>().Value;

        actualOptions.Enabled.Should().Be(expectedOptions.Enabled);
        actualOptions.AutoRollbackOnFailure.Should().Be(expectedOptions.AutoRollbackOnFailure);
        actualOptions.AutoAdvanceOnSuccess.Should().Be(expectedOptions.AutoAdvanceOnSuccess);
        actualOptions.LinearStepCount.Should().Be(expectedOptions.LinearStepCount);
        actualOptions.StepSoakDuration.Should().Be(expectedOptions.StepSoakDuration);
        actualOptions.MaxDeploymentDuration.Should().Be(expectedOptions.MaxDeploymentDuration);
        actualOptions.AlertPriority.Should().Be(expectedOptions.AlertPriority);
    }

    [Fact]
    public void AddCanaryDeployment_WithServicesAndOptionsInstance_ConfiguresOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedOptions = new CanaryOptions
        {
            Enabled = false,
            AutoRollbackOnFailure = false,
            AutoAdvanceOnSuccess = true,
            LinearStepCount = 8,
            StepSoakDuration = TimeSpan.FromMinutes(20),
            MaxDeploymentDuration = TimeSpan.FromHours(8),
            AlertPriority = NotificationPriority.Normal
        };

        // Act
        services.AddCanaryDeployment(expectedOptions);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var actualOptions = serviceProvider.GetRequiredService<IOptions<CanaryOptions>>().Value;

        actualOptions.Enabled.Should().Be(expectedOptions.Enabled);
        actualOptions.AutoRollbackOnFailure.Should().Be(expectedOptions.AutoRollbackOnFailure);
        actualOptions.AutoAdvanceOnSuccess.Should().Be(expectedOptions.AutoAdvanceOnSuccess);
        actualOptions.LinearStepCount.Should().Be(expectedOptions.LinearStepCount);
        actualOptions.StepSoakDuration.Should().Be(expectedOptions.StepSoakDuration);
        actualOptions.MaxDeploymentDuration.Should().Be(expectedOptions.MaxDeploymentDuration);
        actualOptions.AlertPriority.Should().Be(expectedOptions.AlertPriority);
    }

    [Fact]
    public void AddCanaryDeployment_WithNullServicesAndConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddCanaryDeployment(configuration));
    }

    [Fact]
    public void AddCanaryDeployment_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfiguration? configuration = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddCanaryDeployment(configuration!));
    }

    [Fact]
    public void AddCanaryDeployment_WithConfiguration_ConfiguresOptionsFromSection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CanaryDeployment:Enabled"] = "false",
                ["CanaryDeployment:AutoRollbackOnFailure"] = "false",
                ["CanaryDeployment:AutoAdvanceOnSuccess"] = "true",
                ["CanaryDeployment:LinearStepCount"] = "12",
                ["CanaryDeployment:StepSoakDuration"] = "00:25:00",
                ["CanaryDeployment:MaxDeploymentDuration"] = "08:00:00",
                ["CanaryDeployment:AlertPriority"] = "Low"
            })
            .Build();

        // Act
        services.AddCanaryDeployment(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var actualOptions = serviceProvider.GetRequiredService<IOptions<CanaryOptions>>().Value;

        actualOptions.Enabled.Should().BeFalse();
        actualOptions.AutoRollbackOnFailure.Should().BeFalse();
        actualOptions.AutoAdvanceOnSuccess.Should().BeTrue();
        actualOptions.LinearStepCount.Should().Be(12);
        actualOptions.StepSoakDuration.Should().Be(TimeSpan.FromMinutes(25));
        actualOptions.MaxDeploymentDuration.Should().Be(TimeSpan.FromHours(8));
        actualOptions.AlertPriority.Should().Be(NotificationPriority.Low);
    }

    [Fact]
    public void AddCanaryDeployment_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCanaryDeployment();

        // Assert
        services.Should().Contain(d => d.ServiceType == typeof(ITrafficSplitter) && d.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(d => d.ServiceType == typeof(ICanaryHealthEvaluator) && d.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(d => d.ServiceType == typeof(ICanaryDeploymentService) && d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void ReplaceCanaryDeployment_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.ReplaceCanaryDeployment());
    }

    [Fact]
    public void ReplaceCanaryDeployment_RemovesExistingRegistrationsAndReRegisters()
    {
        // Arrange
        var services = new ServiceCollection();

        // First registration
        services.AddCanaryDeployment(options =>
        {
            options.Enabled = true;
            options.LinearStepCount = 5;
        });

        // Verify initial registration
        var initialProvider = services.BuildServiceProvider();
        var initialOptions = initialProvider.GetRequiredService<IOptions<CanaryOptions>>().Value;
        initialOptions.Enabled.Should().BeTrue();
        initialOptions.LinearStepCount.Should().Be(5);

        // Act - replace with different configuration
        services.ReplaceCanaryDeployment(options =>
        {
            options.Enabled = false;
            options.LinearStepCount = 10;
        });

        // Assert - new registrations added
        services.Should().Contain(d => d.ServiceType == typeof(ITrafficSplitter) && d.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(d => d.ServiceType == typeof(ICanaryHealthEvaluator) && d.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(d => d.ServiceType == typeof(ICanaryDeploymentService) && d.Lifetime == ServiceLifetime.Scoped);

        // New configuration applied
        var newProvider = services.BuildServiceProvider();
        var newOptions = newProvider.GetRequiredService<IOptions<CanaryOptions>>().Value;
        newOptions.Enabled.Should().BeFalse();
        newOptions.LinearStepCount.Should().Be(10);
    }

    [Fact]
    public void ReplaceCanaryDeployment_WithNullConfigure_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.ReplaceCanaryDeployment(configure: null);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(services);

        // Verify services are registered
        services.Should().Contain(d => d.ServiceType == typeof(ITrafficSplitter));
    }

    [Fact]
    public void AddCanaryDeployment_WithEmptyConfigurationSection_UsesDefaults()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()) // Empty configuration
            .Build();

        // Act
        services.AddCanaryDeployment(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var actualOptions = serviceProvider.GetRequiredService<IOptions<CanaryOptions>>().Value;

        // Verify defaults are used
        actualOptions.Enabled.Should().BeTrue(); // Default from CanaryOptions
        actualOptions.AutoRollbackOnFailure.Should().BeTrue(); // Default from CanaryOptions
        actualOptions.AutoAdvanceOnSuccess.Should().BeFalse(); // Default from CanaryOptions
        actualOptions.LinearStepCount.Should().Be(5); // Default from CanaryOptions
        actualOptions.StepSoakDuration.Should().Be(TimeSpan.FromMinutes(10)); // Default from CanaryOptions
        actualOptions.MaxDeploymentDuration.Should().Be(TimeSpan.FromHours(4)); // Default from CanaryOptions
        actualOptions.AlertPriority.Should().Be(NotificationPriority.High); // Default from CanaryOptions
    }

    [Fact]
    public void AddCanaryDeployment_WithCanaryOptionsInstance_NullOptionsThrows()
    {
        // Arrange
        var services = new ServiceCollection();
        CanaryOptions? options = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddCanaryDeployment(options!));
    }
}
