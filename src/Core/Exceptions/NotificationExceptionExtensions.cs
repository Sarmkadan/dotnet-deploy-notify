#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Exceptions;

namespace DotNetDeployNotify.Core.Exceptions;

/// <summary>
/// Extension methods for notification-related exceptions that provide
/// additional functionality for error handling and diagnostics.
/// </summary>
public static class NotificationExceptionExtensions
{
    /// <summary>
    /// Creates a formatted error message that includes channel-specific details
    /// for better error reporting and debugging.
    /// </summary>
    /// <param name="exception">The notification exception to format</param>
    /// <returns>A formatted error message string</returns>
    /// <exception cref="ArgumentNullException">Thrown when exception is null</exception>
    public static string ToFormattedErrorMessage(this NotificationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            ChannelConfigurationException channelConfig => FormatChannelConfigurationError(channelConfig),
            WebhookDeliveryException webhook => FormatWebhookDeliveryError(webhook),
            NotificationValidationException validation => FormatValidationError(validation),
            NotificationDeliveryException delivery => FormatDeliveryError(delivery),
            ConfigurationMissingException configMissing => FormatConfigurationMissingError(configMissing),
            _ => FormatGenericNotificationError(exception)
        };
    }

    /// <summary>
    /// Determines if the exception represents a configuration-related issue
    /// that might be resolved by updating configuration settings.
    /// </summary>
    /// <param name="exception">The notification exception to check</param>
    /// <returns>True if the exception is configuration-related; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when exception is null</exception>
    public static bool IsConfigurationError(this NotificationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception is ChannelConfigurationException or ConfigurationMissingException;
    }

    /// <summary>
    /// Determines if the exception represents a delivery failure that might
    /// be resolved by retrying the operation.
    /// </summary>
    /// <param name="exception">The notification exception to check</param>
    /// <returns>True if the exception is a delivery failure; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when exception is null</exception>
    public static bool IsDeliveryFailure(this NotificationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception is WebhookDeliveryException or NotificationDeliveryException;
    }

    /// <summary>
    /// Determines if the exception represents a validation error that cannot
    /// be resolved without fixing the input data.
    /// </summary>
    /// <param name="exception">The notification exception to check</param>
    /// <returns>True if the exception is a validation error; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when exception is null</exception>
    public static bool IsValidationError(this NotificationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception is NotificationValidationException;
    }

    /// <summary>
    /// Gets a user-friendly error category that can be used for categorizing
    /// errors in monitoring and alerting systems.
    /// </summary>
    /// <param name="exception">The notification exception to categorize</param>
    /// <returns>A string representing the error category</returns>
    /// <exception cref="ArgumentNullException">Thrown when exception is null</exception>
    public static string GetErrorCategory(this NotificationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            ChannelConfigurationException => "Configuration",
            WebhookDeliveryException => "Delivery",
            NotificationValidationException => "Validation",
            NotificationDeliveryException => "Delivery",
            ConfigurationMissingException => "Configuration",
            RepositoryException => "Repository",
            _ => "General"
        };
    }

    /// <summary>
    /// Gets a severity level (0-100) that can be used for prioritizing
    /// error handling and alerting.
    /// </summary>
    /// <param name="exception">The notification exception to evaluate</param>
    /// <returns>An integer severity level between 0 and 100</returns>
    /// <exception cref="ArgumentNullException">Thrown when exception is null</exception>
    public static int GetSeverityLevel(this NotificationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            NotificationValidationException => 90, // High - data issues
            ConfigurationMissingException => 80,   // High - missing required config
            ChannelConfigurationException => 75,    // High - channel-specific config
            NotificationDeliveryException { HttpStatusCode: 401 or 403 } => 95, // Critical - auth issues
            NotificationDeliveryException { HttpStatusCode: not null } => 70, // Medium - delivery failed
            WebhookDeliveryException { LastStatusCode: 429 } => 85, // High - rate limited
            WebhookDeliveryException => 65,        // Medium - delivery failed
            RepositoryException => 50,             // Medium - data access
            _ => 50 // Default severity
        };
    }

    /// <summary>
    /// Creates a dictionary of diagnostic information that can be used for
    /// logging, monitoring, and error tracking systems.
    /// </summary>
    /// <param name="exception">The notification exception to analyze</param>
    /// <returns>A dictionary containing diagnostic key-value pairs</returns>
    /// <exception cref="ArgumentNullException">Thrown when exception is null</exception>
    public static IReadOnlyDictionary<string, string> GetDiagnosticInfo(this NotificationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var diagnostics = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ExceptionType"] = exception.GetType().Name,
            ["Message"] = exception.Message,
            ["ErrorCategory"] = exception.GetErrorCategory(),
            ["SeverityLevel"] = exception.GetSeverityLevel().ToString(CultureInfo.InvariantCulture)
        };

        if (exception is ChannelConfigurationException channelConfig)
        {
            diagnostics["ChannelType"] = channelConfig.ChannelType?.ToString() ?? "Unknown";
            diagnostics["ConfigurationId"] = channelConfig.ConfigurationId ?? "None";
        }

        if (exception is WebhookDeliveryException webhook)
        {
            diagnostics["Channel"] = webhook.Channel.ToString();
            diagnostics["Attempts"] = webhook.Attempts.ToString(CultureInfo.InvariantCulture);
            diagnostics["LastStatusCode"] = webhook.LastStatusCode?.ToString() ?? "Unknown";
        }

        if (exception is NotificationValidationException validation)
        {
            diagnostics["ValidationErrorsCount"] = validation.ValidationErrors.Count.ToString(CultureInfo.InvariantCulture);
            if (validation.ValidationErrors.Count > 0)
            {
                diagnostics["FirstValidationError"] = validation.ValidationErrors[0];
            }
        }

        if (exception is NotificationDeliveryException delivery)
        {
            diagnostics["Channel"] = delivery.Channel.ToString();
            diagnostics["HttpStatusCode"] = delivery.HttpStatusCode?.ToString() ?? "Unknown";
        }

        if (exception is ConfigurationMissingException configMissing)
        {
            diagnostics["ConfigurationKey"] = configMissing.ConfigurationKey ?? "Unknown";
        }

        if (exception is RepositoryException repo)
        {
            diagnostics["Operation"] = repo.Operation;
            diagnostics["EntityId"] = repo.EntityId ?? "Unknown";
        }

        diagnostics["Timestamp"] = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);

        return diagnostics;
    }

    private static string FormatChannelConfigurationError(ChannelConfigurationException exception)
    {
        var message = new List<string> { exception.Message };

        if (exception.ChannelType.HasValue)
        {
            message.Add($"Channel: {exception.ChannelType.Value}");
        }

        if (!string.IsNullOrEmpty(exception.ConfigurationId))
        {
            message.Add($"Configuration ID: {exception.ConfigurationId}");
        }

        return string.Join(" | ", message);
    }

    private static string FormatWebhookDeliveryError(WebhookDeliveryException exception)
    {
        var message = new List<string> { exception.Message };
        message.Add($"Channel: {exception.Channel}");
        message.Add($"Attempts: {exception.Attempts}");

        if (exception.LastStatusCode.HasValue)
        {
            message.Add($"Status Code: {exception.LastStatusCode.Value}");
        }

        return string.Join(" | ", message);
    }

    private static string FormatValidationError(NotificationValidationException exception)
    {
        var message = new List<string> { exception.Message };

        if (exception.ValidationErrors.Count > 0)
        {
            message.Add(string.Join("; ", exception.ValidationErrors.Take(3)));
            if (exception.ValidationErrors.Count > 3)
            {
                message.Add($"... and {exception.ValidationErrors.Count - 3} more errors");
            }
        }

        return string.Join(" | ", message);
    }

    private static string FormatDeliveryError(NotificationDeliveryException exception)
    {
        var message = new List<string> { exception.Message };
        message.Add($"Channel: {exception.Channel}");

        if (exception.HttpStatusCode.HasValue)
        {
            message.Add($"HTTP Status: {exception.HttpStatusCode.Value}");
        }

        return string.Join(" | ", message);
    }

    private static string FormatConfigurationMissingError(ConfigurationMissingException exception)
    {
        var message = new List<string> { exception.Message };

        if (!string.IsNullOrEmpty(exception.ConfigurationKey))
        {
            message.Add($"Missing Key: {exception.ConfigurationKey}");
        }

        return string.Join(" | ", message);
    }

    private static string FormatGenericNotificationError(NotificationException exception)
    {
        return exception.Message;
    }
}