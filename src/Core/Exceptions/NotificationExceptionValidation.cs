#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Core.Exceptions;

/// <summary>
/// Validation extension methods for NotificationException and its derived types
/// </summary>
public static class NotificationExceptionValidation
{
    /// <summary>
    /// Validates a <see cref="NotificationException"/> and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The <see cref="NotificationException"/> to validate</param>
    /// <returns>List of validation problems; empty list if valid</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null</exception>
    /// <remarks>
    /// This method validates the exception message and performs type-specific validation
    /// for each derived exception type using pattern matching.
    /// </remarks>
    public static IReadOnlyList<string> Validate(this NotificationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Message property (base Exception class)
        if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("Exception message is null, empty, or whitespace");
        }

        // Validate derived types based on their actual runtime type
        switch (value)
        {
            case ChannelConfigurationException channelConfigEx:
                ValidateChannelConfigurationException(channelConfigEx, problems);
                break;

            case WebhookDeliveryException webhookEx:
                ValidateWebhookDeliveryException(webhookEx, problems);
                break;

            case NotificationValidationException validationEx:
                ValidateNotificationValidationException(validationEx, problems);
                break;

            case NotificationDeliveryException deliveryEx:
                ValidateNotificationDeliveryException(deliveryEx, problems);
                break;

            case ConfigurationMissingException configMissingEx:
                ValidateConfigurationMissingException(configMissingEx, problems);
                break;
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a <see cref="NotificationException"/> is valid
    /// </summary>
    /// <param name="value">The <see cref="NotificationException"/> to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null</exception>
    /// <remarks>
    /// This method is a convenience wrapper around <see cref="Validate(NotificationException)"/> that
    /// returns a boolean result instead of a list of problems.
    /// </remarks>
    public static bool IsValid(this NotificationException value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="NotificationException"/> is valid, throwing an <see cref="ArgumentException"/> if not
    /// </summary>
    /// <param name="value">The <see cref="NotificationException"/> to validate</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing the list of problems</exception>
    /// <remarks>
    /// This method calls <see cref="Validate(NotificationException)"/> and throws an exception with detailed
    /// validation messages if any problems are found.
    /// </remarks>
    public static void EnsureValid(this NotificationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"NotificationException validation failed:{System.Environment.NewLine}{string.Join(System.Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Validates a <see cref="ChannelConfigurationException"/> instance
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <param name="problems">List to accumulate validation problems</param>
    private static void ValidateChannelConfigurationException(
        ChannelConfigurationException value,
        List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(problems);

        // ChannelType can be null (nullable property)
        if (value.ChannelType.HasValue && !Enum.IsDefined(typeof(NotificationChannel), value.ChannelType.Value))
        {
            problems.Add($"ChannelType '{value.ChannelType}' is not a valid NotificationChannel value");
        }

        // ConfigurationId can be null or empty
        if (value.ConfigurationId is not null && string.IsNullOrWhiteSpace(value.ConfigurationId))
        {
            problems.Add("ChannelConfigurationException.ConfigurationId is empty or whitespace");
        }
    }

    /// <summary>
    /// Validates a <see cref="WebhookDeliveryException"/> instance
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <param name="problems">List to accumulate validation problems</param>
    private static void ValidateWebhookDeliveryException(
        WebhookDeliveryException value,
        List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(problems);

        // Channel must be defined
        if (!Enum.IsDefined(typeof(NotificationChannel), value.Channel))
        {
            problems.Add($"WebhookDeliveryException.Channel '{value.Channel}' is not a valid NotificationChannel value");
        }

        // Attempts must be positive
        if (value.Attempts <= 0)
        {
            problems.Add("WebhookDeliveryException.Attempts must be a positive integer");
        }

        // LastStatusCode if present must be a valid HTTP status code range
        if (value.LastStatusCode.HasValue)
        {
            if (value.LastStatusCode.Value < 100 || value.LastStatusCode.Value > 599)
            {
                problems.Add("WebhookDeliveryException.LastStatusCode must be a valid HTTP status code (100-599)");
            }
        }
    }

    /// <summary>
    /// Validates a <see cref="NotificationValidationException"/> instance
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <param name="problems">List to accumulate validation problems</param>
    private static void ValidateNotificationValidationException(
        NotificationValidationException value,
        List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(problems);

        // ValidationErrors collection should not be null and should not contain null/empty strings
        if (value.ValidationErrors is null)
        {
            problems.Add("NotificationValidationException.ValidationErrors collection is null");
        }
        else if (value.ValidationErrors.Count == 0)
        {
            problems.Add("NotificationValidationException.ValidationErrors collection is empty");
        }
        else
        {
            for (int i = 0; i < value.ValidationErrors.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(value.ValidationErrors[i]))
                {
                    problems.Add($"NotificationValidationException.ValidationErrors[{i}] is null, empty, or whitespace");
                }
            }
        }
    }

    /// <summary>
    /// Validates a <see cref="NotificationDeliveryException"/> instance
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <param name="problems">List to accumulate validation problems</param>
    private static void ValidateNotificationDeliveryException(
        NotificationDeliveryException value,
        List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(problems);

        // Channel must be defined
        if (!Enum.IsDefined(typeof(NotificationChannel), value.Channel))
        {
            problems.Add($"NotificationDeliveryException.Channel '{value.Channel}' is not a valid NotificationChannel value");
        }

        // HttpStatusCode if present must be a valid HTTP status code range
        if (value.HttpStatusCode.HasValue)
        {
            if (value.HttpStatusCode.Value < 100 || value.HttpStatusCode.Value > 599)
            {
                problems.Add("NotificationDeliveryException.HttpStatusCode must be a valid HTTP status code (100-599)");
            }
        }
    }

    /// <summary>
    /// Validates a <see cref="ConfigurationMissingException"/> instance
    /// </summary>
    /// <param name="value">The exception to validate</param>
    /// <param name="problems">List to accumulate validation problems</param>
    private static void ValidateConfigurationMissingException(
        ConfigurationMissingException value,
        List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(problems);

        // ConfigurationKey can be null or empty
        if (value.ConfigurationKey is not null && string.IsNullOrWhiteSpace(value.ConfigurationKey))
        {
            problems.Add("ConfigurationMissingException.ConfigurationKey is empty or whitespace");
        }
    }
}