#nullable enable

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Provides validation helpers for <see cref="RollbackNotificationServiceTests"/>.
/// Validates the test class structure and its dependencies to ensure proper mocking and initialization.
/// </summary>
public static class RollbackNotificationServiceTestsValidation
{
    /// <summary>
    /// Validates a RollbackNotificationServiceTests instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RollbackNotificationServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate private fields
        var notificationServiceField = value.GetType().GetField("_notificationService",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (notificationServiceField?.GetValue(value) is not INotificationService notificationService)
        {
            problems.Add("RollbackNotificationServiceTests._notificationService must not be null.");
        }
        else
        {
            // Validate INotificationService mock setup - simplified validation using NSubstitute's Received
            try
            {
                notificationService.Received(Arg.Any<int>()).CreateNotificationAsync(Arg.Any<DeploymentNotification>());
                notificationService.Received(Arg.Any<int>()).SendNotificationAsync(Arg.Any<string>(), Arg.Any<List<NotificationChannel>?>());
            }
            catch
            {
                problems.Add("INotificationService mock setup must be valid.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a RollbackNotificationServiceTests instance is valid.
    /// </summary>
    /// <param name="value">The test instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this RollbackNotificationServiceTests value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that a RollbackNotificationServiceTests instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the test instance is invalid.</exception>
    public static void EnsureValid(this RollbackNotificationServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"RollbackNotificationServiceTests is invalid:{System.Environment.NewLine} - {string.Join($"{System.Environment.NewLine} - ", problems)}");
        }
    }
}