#nullable enable

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Provides validation helpers for RollbackNotificationServiceTests.
/// Validates the test class structure and its dependencies.
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
            // Validate INotificationService mock setup
            try
            {
                var createNotificationTask = notificationService.CreateNotificationAsync(Arg.Any<DeploymentNotification>());
                if (createNotificationTask.Status == System.Threading.Tasks.TaskStatus.Faulted)
                {
                    problems.Add("INotificationService.CreateNotificationAsync mock must not throw exceptions.");
                }

                var sendNotificationTask = notificationService.SendNotificationAsync(Arg.Any<string>(), Arg.Any<List<NotificationChannel>?>());
                if (sendNotificationTask.Status == System.Threading.Tasks.TaskStatus.Faulted)
                {
                    problems.Add("INotificationService.SendNotificationAsync mock must not throw exceptions.");
                }
            }
            catch
            {
                problems.Add("INotificationService mock setup must be valid.");
            }
        }

        var loggerField = value.GetType().GetField("_service",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (loggerField?.GetValue(value) is not RollbackNotificationService service)
        {
            problems.Add("RollbackNotificationServiceTests._service must not be null.");
        }
        else
        {
            // Validate RollbackNotificationService instance
            if (service is null)
            {
                problems.Add("RollbackNotificationService instance must not be null.");
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