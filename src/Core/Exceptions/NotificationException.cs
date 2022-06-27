#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Core.Exceptions;

/// <summary>
/// Base exception for all notification-related errors
/// </summary>
public class NotificationException : Exception
{
    /// <summary>Initializes a new instance with a message</summary>
    public NotificationException(string message) : base(message) { }

    /// <summary>Initializes a new instance with a message and inner exception</summary>
    public NotificationException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Thrown when a channel configuration is invalid or not found
/// </summary>
public class ChannelConfigurationException : NotificationException
{
    /// <summary>The channel type that caused the issue</summary>
    public NotificationChannel? ChannelType { get; set; }

    /// <summary>Configuration ID that was problematic</summary>
    public string? ConfigurationId { get; set; }

    /// <summary>Initializes a new instance with a message</summary>
    public ChannelConfigurationException(string message) : base(message) { }

    /// <summary>Initializes a new instance with detailed information</summary>
    public ChannelConfigurationException(string message, NotificationChannel channel, string configId)
        : base(message)
    {
        ChannelType = channel;
        ConfigurationId = configId;
    }
}

/// <summary>
/// Thrown when webhook delivery fails after all retries
/// </summary>
public class WebhookDeliveryException : NotificationException
{
    /// <summary>The channel being delivered to</summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>Number of attempts made</summary>
    public int Attempts { get; set; }

    /// <summary>Last HTTP response code received</summary>
    public int? LastStatusCode { get; set; }

    /// <summary>Initializes a new instance</summary>
    public WebhookDeliveryException(string message, NotificationChannel channel, int attempts, int? lastStatusCode)
        : base(message)
    {
        Channel = channel;
        Attempts = attempts;
        LastStatusCode = lastStatusCode;
    }
}

/// <summary>
/// Thrown when validation of notification data fails
/// </summary>
public class NotificationValidationException : NotificationException
{
    /// <summary>Validation errors encountered</summary>
    public List<string> ValidationErrors { get; set; } = new();

    /// <summary>Initializes a new instance with validation errors</summary>
    public NotificationValidationException(string message, List<string> errors)
        : base(message)
    {
        ValidationErrors = errors;
    }

    /// <summary>Initializes a new instance with a single error</summary>
    public NotificationValidationException(string message) : base(message) { }
}

/// <summary>
/// Thrown when a required configuration is missing
/// </summary>
public class ConfigurationMissingException : NotificationException
{
    /// <summary>The missing configuration key</summary>
    public string? ConfigurationKey { get; set; }

    /// <summary>Initializes a new instance with configuration key</summary>
    public ConfigurationMissingException(string message, string configKey)
        : base(message)
    {
        ConfigurationKey = configKey;
    }

    /// <summary>Initializes a new instance with a message</summary>
    public ConfigurationMissingException(string message) : base(message) { }
}

/// <summary>
/// Thrown when data repository operations fail
/// </summary>
public class RepositoryException : NotificationException
{
    /// <summary>The operation that failed (Create, Read, Update, Delete, Query)</summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>Entity ID that was being operated on</summary>
    public string? EntityId { get; set; }

    /// <summary>Initializes a new instance</summary>
    public RepositoryException(string message, string operation, string? entityId = null)
        : base(message)
    {
        Operation = operation;
        EntityId = entityId;
    }

    /// <summary>Initializes a new instance with inner exception</summary>
    public RepositoryException(string message, string operation, Exception innerException, string? entityId = null)
        : base(message, innerException)
    {
        Operation = operation;
        EntityId = entityId;
    }
}
