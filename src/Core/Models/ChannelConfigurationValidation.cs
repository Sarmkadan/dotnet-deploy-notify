#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core;
using System.Globalization;

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides validation helpers for <see cref="ChannelConfiguration"/> instances
/// </summary>
public static class ChannelConfigurationValidation
{
    /// <summary>
    /// Validates the channel configuration and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The configuration to validate</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this ChannelConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("Id must be a non-empty string.");
        }

        // Validate ChannelType (enum has valid values by design)
        if (value.ChannelType is not (NotificationChannel.Telegram or NotificationChannel.Slack or NotificationChannel.Discord or NotificationChannel.Webhook or NotificationChannel.Email))
        {
            errors.Add("ChannelType must be a valid NotificationChannel value.");
        }

        // Validate WebhookUrl
        if (string.IsNullOrWhiteSpace(value.WebhookUrl))
        {
            errors.Add("WebhookUrl must be a non-empty string.");
        }
        else if (!Uri.IsWellFormedUriString(value.WebhookUrl, UriKind.Absolute) && !value.WebhookUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !value.WebhookUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("WebhookUrl must be a well-formed absolute URI.");
        }

        // Validate ApiToken (can be empty for some channel types, but if present should be valid)
        if (!string.IsNullOrEmpty(value.ApiToken) && string.IsNullOrWhiteSpace(value.ApiToken))
        {
            errors.Add("ApiToken must be a non-empty string if provided.");
        }

        // Validate TargetId
        if (string.IsNullOrWhiteSpace(value.TargetId))
        {
            errors.Add("TargetId must be a non-empty string.");
        }

        // Validate DisplayName
        if (string.IsNullOrWhiteSpace(value.DisplayName))
        {
            errors.Add("DisplayName must be a non-empty string.");
        }

        // Validate MinimumPriority
        if (value.MinimumPriority is not (NotificationPriority.Low or NotificationPriority.Normal or NotificationPriority.High or NotificationPriority.Critical))
        {
            errors.Add("MinimumPriority must be a valid NotificationPriority value.");
        }

        // Validate AllowedEnvironments
        if (value.AllowedEnvironments is null)
        {
            errors.Add("AllowedEnvironments must not be null.");
        }
        else
        {
            foreach (var env in value.AllowedEnvironments)
            {
                if (env is not (Environment.Development or Environment.Staging or Environment.Production or Environment.Testing or Environment.PreProduction))
                {
                    errors.Add($"AllowedEnvironments contains invalid Environment value: {env}.");
                }
            }
        }

        // Validate AllowedStatuses
        if (value.AllowedStatuses is null)
        {
            errors.Add("AllowedStatuses must not be null.");
        }
        else
        {
            foreach (var status in value.AllowedStatuses)
            {
                if (status is not (BuildStatus.Started or BuildStatus.InProgress or BuildStatus.Success or BuildStatus.Failed or BuildStatus.Cancelled or BuildStatus.SuccessWithWarnings or BuildStatus.Deploying or BuildStatus.DeploymentSuccess or BuildStatus.DeploymentFailed))
                {
                    errors.Add($"AllowedStatuses contains invalid BuildStatus value: {status}.");
                }
            }
        }

        // Validate MaxRetries
        if (value.MaxRetries < 0)
        {
            errors.Add("MaxRetries must be a non-negative integer.");
        }
        else if (value.MaxRetries > 100)
        {
            errors.Add("MaxRetries should not exceed 100 for practical purposes.");
        }

        // Validate TimeoutMs
        if (value.TimeoutMs <= 0)
        {
            errors.Add("TimeoutMs must be a positive integer greater than zero.");
        }
        else if (value.TimeoutMs > 300000) // 5 minutes max
        {
            errors.Add("TimeoutMs should not exceed 300000 milliseconds (5 minutes) for practical purposes.");
        }

        // Validate CustomHeaders
        if (value.CustomHeaders is null)
        {
            errors.Add("CustomHeaders must not be null.");
        }
        else
        {
            foreach (var kvp in value.CustomHeaders)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    errors.Add("CustomHeaders contains an entry with null or empty key.");
                    break;
                }
            }
        }

        // Validate Settings
        if (value.Settings is null)
        {
            errors.Add("Settings must not be null.");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt must be set to a valid DateTime.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt cannot be in the future.");
        }

        // Validate UpdatedAt
        if (value.UpdatedAt.HasValue)
        {
            if (value.UpdatedAt.Value == default)
            {
                errors.Add("UpdatedAt must be a valid DateTime if set.");
            }
            else if (value.UpdatedAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("UpdatedAt cannot be in the future.");
            }

            if (value.CreatedAt != default && value.UpdatedAt < value.CreatedAt)
            {
                errors.Add("UpdatedAt cannot be earlier than CreatedAt.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the channel configuration is valid
    /// </summary>
    /// <param name="value">The configuration to check</param>
    /// <returns>True if the configuration is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static bool IsValid(this ChannelConfiguration value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the channel configuration is valid, throwing an exception if it is not
    /// </summary>
    /// <param name="value">The configuration to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when the configuration is invalid, containing a list of validation errors</exception>
    public static void EnsureValid(this ChannelConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException("ChannelConfiguration is invalid. See inner exception for details.", nameof(value), new AggregateException(errors.Select(e => new ArgumentException(e))));
        }
    }
}
