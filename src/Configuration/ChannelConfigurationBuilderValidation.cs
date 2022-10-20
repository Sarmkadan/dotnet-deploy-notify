#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using DotNetDeployNotify.Core;

namespace DotNetDeployNotify.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="ChannelConfigurationBuilder"/> instances.
/// </summary>
public static class ChannelConfigurationBuilderValidation
{
    /// <summary>
    /// Validates the provided <see cref="ChannelConfigurationBuilder"/> instance.
    /// </summary>
    /// <param name="value">The builder to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ChannelConfigurationBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate DisplayName: must be non-empty
        if (string.IsNullOrWhiteSpace(value.DisplayName))
        {
            problems.Add("ChannelConfigurationBuilder.DisplayName must be a non-empty string.");
        }

        // Validate WebhookUrl: must be non-empty (Build() already checks this)
        if (string.IsNullOrWhiteSpace(value.WebhookUrl))
        {
            problems.Add("ChannelConfigurationBuilder.WebhookUrl must be a non-empty string.");
        }
        else if (!Uri.IsWellFormedUriString(value.WebhookUrl, UriKind.Absolute) &&
                 !value.WebhookUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                 !value.WebhookUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            problems.Add("ChannelConfigurationBuilder.WebhookUrl must be a well-formed absolute URI.");
        }

        // Validate TargetId: must be non-empty (common requirement for notification channels)
        if (string.IsNullOrWhiteSpace(value.TargetId))
        {
            problems.Add("ChannelConfigurationBuilder.TargetId must be a non-empty string.");
        }

        // Validate TimeoutMs: must be positive
        if (value.TimeoutMs <= 0)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "ChannelConfigurationBuilder.TimeoutMs must be positive, but was {0}.",
                value.TimeoutMs));
        }
        else if (value.TimeoutMs > 300000) // 5 minutes max
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "ChannelConfigurationBuilder.TimeoutMs should not exceed 300000 milliseconds (5 minutes), but was {0}.",
                value.TimeoutMs));
        }

        // Validate MaxRetries: must be non-negative
        if (value.MaxRetries < 0)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "ChannelConfigurationBuilder.MaxRetries must be non-negative, but was {0}.",
                value.MaxRetries));
        }
        else if (value.MaxRetries > 100)
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "ChannelConfigurationBuilder.MaxRetries should not exceed 100 for practical purposes, but was {0}.",
                value.MaxRetries));
        }

        // Validate MinimumPriority: must be a valid enum value
        if (!Enum.IsDefined(typeof(NotificationPriority), value.MinimumPriority))
        {
            problems.Add(string.Format(
                CultureInfo.InvariantCulture,
                "ChannelConfigurationBuilder.MinimumPriority must be a valid NotificationPriority value, but was {0}.",
                value.MinimumPriority));
        }

        // Validate AllowedEnvironments: must contain valid enum values if not empty
        if (value.AllowedEnvironments.Count > 0)
        {
            foreach (var env in value.AllowedEnvironments)
            {
                if (!Enum.IsDefined(typeof(Environment), env))
                {
                    problems.Add(string.Format(
                        CultureInfo.InvariantCulture,
                        "ChannelConfigurationBuilder.AllowedEnvironments contains invalid Environment value: {0}.",
                        env));
                    break;
                }
            }
        }

        // Validate AllowedStatuses: must contain valid enum values if not empty
        if (value.AllowedStatuses.Count > 0)
        {
            foreach (var status in value.AllowedStatuses)
            {
                if (!Enum.IsDefined(typeof(BuildStatus), status))
                {
                    problems.Add(string.Format(
                        CultureInfo.InvariantCulture,
                        "ChannelConfigurationBuilder.AllowedStatuses contains invalid BuildStatus value: {0}.",
                        status));
                    break;
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the provided <see cref="ChannelConfigurationBuilder"/> instance is valid.
    /// </summary>
    /// <param name="value">The builder to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ChannelConfigurationBuilder value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the provided <see cref="ChannelConfigurationBuilder"/> instance is valid.
    /// </summary>
    /// <param name="value">The builder to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the builder contains validation problems.</exception>
    public static void EnsureValid(this ChannelConfigurationBuilder value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "ChannelConfigurationBuilder validation failed:{0}{1}",
                    "\n",
                    string.Join("\n", problems)),
                nameof(value));
        }
    }
}