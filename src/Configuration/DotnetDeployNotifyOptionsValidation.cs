#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace DotNetDeployNotify.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="DotnetDeployNotifyOptions"/> configuration.
/// </summary>
public static class DotnetDeployNotifyOptionsValidation
{
    /// <summary>
    /// Validates the provided <see cref="DotnetDeployNotifyOptions"/> instance.
    /// </summary>
    /// <param name="value">The configuration to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DotnetDeployNotifyOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate NotificationConfig
        problems.AddRange(value.Notification.Validate());

        // Validate CanaryOptions
        problems.AddRange(value.Canary.Validate());

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="NotificationConfig"/> instance.
    /// </summary>
    /// <param name="value">The configuration to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this NotificationConfig value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.MaxRetries < 0 || value.MaxRetries > 100)
        {
            problems.Add($"Notification.MaxRetries must be between 0 and 100, but was {value.MaxRetries}.");
        }

        if (value.WebhookTimeoutMs < 100 || value.WebhookTimeoutMs > 60000)
        {
            problems.Add($"Notification.WebhookTimeoutMs must be between 100 and 60000, but was {value.WebhookTimeoutMs}.");
        }

        if (value.RetryDelayMs < 100 || value.RetryDelayMs > 60000)
        {
            problems.Add($"Notification.RetryDelayMs must be between 100 and 60000, but was {value.RetryDelayMs}.");
        }

        if (value.ProcessingIntervalSeconds < 1 || value.ProcessingIntervalSeconds > 3600)
        {
            problems.Add($"Notification.ProcessingIntervalSeconds must be between 1 and 3600, but was {value.ProcessingIntervalSeconds}.");
        }

        if (string.IsNullOrWhiteSpace(value.StorageType))
        {
            problems.Add("Notification.StorageType is required and cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(value.DefaultPriority))
        {
            problems.Add("Notification.DefaultPriority is required and cannot be empty.");
        }
        else if (value.DefaultPriority.Length > 50)
        {
            problems.Add($"Notification.DefaultPriority must be 50 characters or less, but was {value.DefaultPriority.Length}.");
        }

        if (value.RetentionDays < 1 || value.RetentionDays > 365)
        {
            problems.Add($"Notification.RetentionDays must be between 1 and 365, but was {value.RetentionDays}.");
        }

        if (value.EnvironmentChannels is null)
        {
            problems.Add("Notification.EnvironmentChannels is required and cannot be null.");
        }
        else
        {
            foreach (var kvp in value.EnvironmentChannels)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    problems.Add("Notification.EnvironmentChannels contains an entry with null or empty key.");
                }

                if (kvp.Value is null)
                {
                    problems.Add($"Notification.EnvironmentChannels['{kvp.Key}'] is required and cannot be null.");
                }
                else
                {
                    problems.AddRange(kvp.Value.Validate());
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(value.StoragePath) && value.StoragePath.Length > 1024)
        {
            problems.Add($"Notification.StoragePath must be 1024 characters or less, but was {value.StoragePath.Length}.");
        }

        if (value.LogLevel is not null && value.LogLevel.Length > 50)
        {
            problems.Add($"Notification.LogLevel must be 50 characters or less, but was {value.LogLevel.Length}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="EnvironmentChannelConfig"/> instance.
    /// </summary>
    /// <param name="value">The configuration to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this EnvironmentChannelConfig value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.WebhookUrl))
        {
            problems.Add("EnvironmentChannelConfig.WebhookUrl is required and cannot be empty.");
        }
        else if (value.WebhookUrl.Length > 2048)
        {
            problems.Add($"EnvironmentChannelConfig.WebhookUrl must be 2048 characters or less, but was {value.WebhookUrl.Length}.");
        }

        if (string.IsNullOrWhiteSpace(value.DisplayName))
        {
            problems.Add("EnvironmentChannelConfig.DisplayName is required and cannot be empty.");
        }
        else if (value.DisplayName.Length > 100)
        {
            problems.Add($"EnvironmentChannelConfig.DisplayName must be 100 characters or less, but was {value.DisplayName.Length}.");
        }

        if (string.IsNullOrWhiteSpace(value.TargetId))
        {
            problems.Add("EnvironmentChannelConfig.TargetId is required and cannot be empty.");
        }
        else if (value.TargetId.Length > 100)
        {
            problems.Add($"EnvironmentChannelConfig.TargetId must be 100 characters or less, but was {value.TargetId.Length}.");
        }

        if (string.IsNullOrWhiteSpace(value.ChannelType))
        {
            problems.Add("EnvironmentChannelConfig.ChannelType is required and cannot be empty.");
        }
        else if (value.ChannelType.Length > 50)
        {
            problems.Add($"EnvironmentChannelConfig.ChannelType must be 50 characters or less, but was {value.ChannelType.Length}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the provided <see cref="CanaryThresholds"/> instance.
    /// </summary>
    /// <param name="value">The configuration to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CanaryThresholds value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.MaxErrorRatePercent < 0 || value.MaxErrorRatePercent > 100)
        {
            problems.Add($"Canary.Thresholds.MaxErrorRatePercent must be between 0 and 100, but was {value.MaxErrorRatePercent}.");
        }

        if (value.MaxP95LatencyMs < 0)
        {
            problems.Add($"Canary.Thresholds.MaxP95LatencyMs must be non-negative, but was {value.MaxP95LatencyMs}.");
        }

        if (value.MaxP99LatencyMs < 0)
        {
            problems.Add($"Canary.Thresholds.MaxP99LatencyMs must be non-negative, but was {value.MaxP99LatencyMs}.");
        }

        if (value.ErrorRateMultiplier < 0)
        {
            problems.Add($"Canary.Thresholds.ErrorRateMultiplier must be non-negative, but was {value.ErrorRateMultiplier}.");
        }

        if (value.LatencyDegradationPercent < 0)
        {
            problems.Add($"Canary.Thresholds.LatencyDegradationPercent must be non-negative, but was {value.LatencyDegradationPercent}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the provided <see cref="DotnetDeployNotifyOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The configuration to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this DotnetDeployNotifyOptions value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the provided <see cref="DotnetDeployNotifyOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The configuration to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the configuration contains validation problems.</exception>
    public static void EnsureValid(this DotnetDeployNotifyOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DotnetDeployNotifyOptions validation failed:{"\n"}{string.Join("\n", problems)}");
        }
    }
}