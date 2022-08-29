#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Canary;
using DotNetDeployNotify.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetDeployNotify.Infrastructure;

/// <summary>
/// Extension methods for registering the canary deployment stack in the
/// dependency injection container.
/// </summary>
public static class CanaryServiceExtensions
{
	/// <summary>
	/// Adds the complete canary deployment stack with an optional inline configuration delegate.
	/// Registers <see cref="ITrafficSplitter"/>, <see cref="ICanaryHealthEvaluator"/>,
	/// and <see cref="ICanaryDeploymentService"/> with their default implementations.
	/// </summary>
	/// <param name="services">The service collection to configure.</param>
	/// <param name="configure">
	/// Optional delegate to override <see cref="CanaryOptions"/> defaults.
	/// When <see langword="null"/>, the out-of-box defaults are used.
	/// </param>
	/// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
	/// <returns>The same <see cref="IServiceCollection"/> for fluent chaining.</returns>
	public static IServiceCollection AddCanaryDeployment(
		this IServiceCollection services,
		Action<CanaryOptions>? configure = null)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddOptions<CanaryOptions>();

		if (configure is not null)
			services.Configure(configure);

		RegisterCoreServices(services);
		return services;
	}

	/// <summary>
	/// Adds the complete canary deployment stack, binding <see cref="CanaryOptions"/>
	/// from the <c>CanaryDeployment</c> section of the supplied <see cref="IConfiguration"/>.
	/// </summary>
	/// <param name="services">The service collection to configure.</param>
	/// <param name="configuration">Application configuration root or a scoped configuration section.</param>
	/// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/>.</exception>
	/// <returns>The same <see cref="IServiceCollection"/> for fluent chaining.</returns>
	public static IServiceCollection AddCanaryDeployment(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configuration);

		services.Configure<CanaryOptions>(
			configuration.GetSection(CanaryOptions.SectionName));

		RegisterCoreServices(services);
		return services;
	}

	/// <summary>
	/// Adds the complete canary deployment stack with a pre-built <see cref="CanaryOptions"/> instance.
	/// Useful in integration tests or scenarios where options are constructed in code.
	/// </summary>
	/// <param name="services">The service collection to configure.</param>
	/// <param name="options">The fully initialised options instance to use.</param>
	/// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
	/// <returns>The same <see cref="IServiceCollection"/> for fluent chaining.</returns>
	public static IServiceCollection AddCanaryDeployment(
		this IServiceCollection services,
		CanaryOptions options)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(options);

		services.Configure<CanaryOptions>(o =>
		{
			o.Enabled = options.Enabled;
			o.AutoRollbackOnFailure = options.AutoRollbackOnFailure;
			o.AutoAdvanceOnSuccess = options.AutoAdvanceOnSuccess;
			o.LinearStepCount = options.LinearStepCount;
			o.StepSoakDuration = options.StepSoakDuration;
			o.MaxDeploymentDuration = options.MaxDeploymentDuration;
			o.Thresholds = options.Thresholds;
			o.AlertPriority = options.AlertPriority;
		});

		RegisterCoreServices(services);
		return services;
	}

	/// <summary>
	/// Removes any previously registered canary deployment services and re-registers them
	/// with the supplied configuration delegate. Useful for test harnesses that call
	/// <c>AddCanaryDeployment</c> multiple times or need to swap implementations.
	/// </summary>
	/// <param name="services">The service collection to reconfigure.</param>
	/// <param name="configure">Optional delegate to customise the replacement options.</param>
	/// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
	/// <returns>The same <see cref="IServiceCollection"/> for fluent chaining.</returns>
	public static IServiceCollection ReplaceCanaryDeployment(
		this IServiceCollection services,
		Action<CanaryOptions>? configure = null)
	{
		ArgumentNullException.ThrowIfNull(services);

		RemoveCanaryDescriptors(services);
		return services.AddCanaryDeployment(configure);
	}

	// -------------------------------------------------------------------------
	// Private helpers
	// -------------------------------------------------------------------------

	private static void RegisterCoreServices(IServiceCollection services)
	{
		services.AddSingleton<ITrafficSplitter, TrafficSplitter>();
		services.AddSingleton<ICanaryHealthEvaluator, CanaryHealthEvaluator>();
		services.AddScoped<ICanaryDeploymentService, CanaryDeploymentEngine>();
	}

	private static void RemoveCanaryDescriptors(IServiceCollection services)
	{
		var canaryTypes = new[]
		{
			typeof(ICanaryDeploymentService),
			typeof(ITrafficSplitter),
			typeof(ICanaryHealthEvaluator)
		};

		foreach (var type in canaryTypes)
		{
			var descriptors = services.Where(d => d.ServiceType == type).ToList();
			foreach (var descriptor in descriptors)
				services.Remove(descriptor);
		}
	}
}