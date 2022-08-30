#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetDeployNotify.Infrastructure;

/// <summary>
/// Extension methods for registering rollback services in the dependency injection container
/// </summary>
public static class RollbackServiceExtensions
{
    /// <summary>
    /// Adds deployment rollback services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <returns>The same service collection for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddRollbackServices(this IServiceCollection services)
        => services.AddScoped<IRollbackService, RollbackService>();
}
