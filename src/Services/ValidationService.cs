// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Exceptions;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Services;

/// <summary>
/// Service for validating notifications and configurations
/// </summary>
public interface IValidationService
{
    /// <summary>Validates a deployment notification</summary>
    ValidationResult ValidateNotification(DeploymentNotification notification);

    /// <summary>Validates a channel configuration</summary>
    ValidationResult ValidateChannelConfiguration(ChannelConfiguration config);

    /// <summary>Validates a webhook payload</summary>
    ValidationResult ValidateWebhookPayload(WebhookPayload payload);

    /// <summary>Validates individual field values</summary>
    bool IsValidUrl(string url);

    /// <summary>Validates email format</summary>
    bool IsValidEmail(string email);
}

/// <summary>
/// Represents validation result with errors
/// </summary>
public class ValidationResult
{
    /// <summary>Whether validation passed</summary>
    public bool IsValid { get; set; }

    /// <summary>List of validation error messages</summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>Creates a successful result</summary>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>Creates a failure result with errors</summary>
    public static ValidationResult Failure(params string[] errors)
    {
        return new()
        {
            IsValid = false,
            Errors = new List<string>(errors)
        };
    }
}

/// <summary>
/// Implementation of validation service
/// </summary>
public class ValidationService : IValidationService
{
    /// <summary>
    /// Validates a deployment notification for required fields and correctness
    /// </summary>
    public ValidationResult ValidateNotification(DeploymentNotification notification)
    {
        var errors = new List<string>();

        if (notification == null)
        {
            errors.Add("Notification cannot be null");
            return ValidationResult.Failure(errors.ToArray());
        }

        if (string.IsNullOrWhiteSpace(notification.ProjectName))
            errors.Add("Project name is required");

        if (string.IsNullOrWhiteSpace(notification.Version))
            errors.Add("Version is required");

        if (string.IsNullOrWhiteSpace(notification.BranchName))
            errors.Add("Branch name is required");

        if (string.IsNullOrWhiteSpace(notification.Message))
            errors.Add("Message is required");

        if (!notification.Channels.Any())
            errors.Add("At least one notification channel must be specified");

        if (notification.DeliveryAttempts < 0)
            errors.Add("Delivery attempts cannot be negative");

        if (notification.DurationSeconds.HasValue && notification.DurationSeconds < 0)
            errors.Add("Duration cannot be negative");

        return errors.Any()
            ? ValidationResult.Failure(errors.ToArray())
            : ValidationResult.Success();
    }

    /// <summary>
    /// Validates a channel configuration for required and valid values
    /// </summary>
    public ValidationResult ValidateChannelConfiguration(ChannelConfiguration config)
    {
        var errors = new List<string>();

        if (config == null)
        {
            errors.Add("Channel configuration cannot be null");
            return ValidationResult.Failure(errors.ToArray());
        }

        if (string.IsNullOrWhiteSpace(config.DisplayName))
            errors.Add("Display name is required");

        if (!IsValidUrl(config.WebhookUrl))
            errors.Add("Webhook URL is invalid or missing");

        if (string.IsNullOrWhiteSpace(config.TargetId))
            errors.Add("Target ID (chat ID, channel ID, etc.) is required");

        if (config.TimeoutMs <= 0)
            errors.Add("Timeout must be greater than 0");

        if (config.MaxRetries < 0)
            errors.Add("Max retries cannot be negative");

        if (config.CustomHeaders == null)
            errors.Add("Custom headers cannot be null");

        return errors.Any()
            ? ValidationResult.Failure(errors.ToArray())
            : ValidationResult.Success();
    }

    /// <summary>
    /// Validates a webhook payload structure and content
    /// </summary>
    public ValidationResult ValidateWebhookPayload(WebhookPayload payload)
    {
        var errors = new List<string>();

        if (payload == null)
        {
            errors.Add("Webhook payload cannot be null");
            return ValidationResult.Failure(errors.ToArray());
        }

        if (string.IsNullOrWhiteSpace(payload.EventId))
            errors.Add("Event ID is required");

        if (string.IsNullOrWhiteSpace(payload.EventType))
            errors.Add("Event type is required");

        if (payload.Data == null)
            errors.Add("Payload data is required");
        else
        {
            var dataValidation = ValidateWebhookData(payload.Data);
            if (!dataValidation.IsValid)
                errors.AddRange(dataValidation.Errors);
        }

        return errors.Any()
            ? ValidationResult.Failure(errors.ToArray())
            : ValidationResult.Success();
    }

    /// <summary>
    /// Validates webhook data fields
    /// </summary>
    private ValidationResult ValidateWebhookData(WebhookData data)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(data.ProjectName))
            errors.Add("Project name is required in webhook data");

        if (string.IsNullOrWhiteSpace(data.Version))
            errors.Add("Version is required in webhook data");

        if (string.IsNullOrWhiteSpace(data.Status))
            errors.Add("Status is required in webhook data");

        return errors.Any()
            ? ValidationResult.Failure(errors.ToArray())
            : ValidationResult.Success();
    }

    /// <summary>
    /// Validates URL format and scheme
    /// </summary>
    public bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Validates email format using basic pattern
    /// </summary>
    public bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
