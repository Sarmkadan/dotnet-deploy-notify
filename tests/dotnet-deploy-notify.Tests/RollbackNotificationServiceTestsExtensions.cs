#nullable enable
using DotNetDeployNotify.Core.Models;
using FluentAssertions;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Provides extension methods for verifying rollback request and result objects in tests.
/// </summary>
public static class RollbackNotificationServiceTestsExtensions
{
    /// <summary>
    /// Verifies that a rollback request contains expected properties.
    /// </summary>
    /// <param name="request">The rollback request to verify.</param>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    public static void VerifyRollbackRequest(this RollbackRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.ProjectName.Should().NotBeNullOrEmpty();
        request.CurrentVersion.Should().NotBeNullOrEmpty();
        request.TargetVersion.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Verifies that a rollback result contains expected properties.
    /// </summary>
    /// <param name="result">The rollback result to verify.</param>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static void VerifyRollbackResult(this RollbackResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        result.ProjectName.Should().NotBeNullOrEmpty();
        result.RolledBackFromVersion.Should().NotBeNullOrEmpty();
        result.RolledBackToVersion.Should().NotBeNullOrEmpty();
    }
}
