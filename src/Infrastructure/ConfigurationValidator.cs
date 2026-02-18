#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify.Infrastructure;

/// <summary>
/// Interface for validating system and channel configurations
/// </summary>
public interface IConfigurationValidator
{
    /// <summary>Validates channel configuration against best practices</summary>
    (bool IsValid, List<string> Warnings, List<string> Errors) ValidateChannelConfiguration(ChannelConfiguration config);

    /// <summary>Validates notification service configuration</summary>
    (bool IsValid, List<string> Warnings, List<string> Errors) ValidateNotificationConfig(NotificationConfig config);

    /// <summary>Checks if all required configurations are present</summary>
    bool HasRequiredConfigurations(List<ChannelConfiguration> configs);

    /// <summary>Suggests configuration improvements</summary>
    List<string> SuggestImprovements(ChannelConfiguration config);
}

/// <summary>
/// Implementation of configuration validator
/// </summary>
public sealed class ConfigurationValidator : IConfigurationValidator
{
    private readonly ILogger<ConfigurationValidator> _logger;

    /// <summary>Initializes the configuration validator</summary>
    public ConfigurationValidator(ILogger<ConfigurationValidator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates a channel configuration thoroughly
    /// </summary>
    public (bool IsValid, List<string> Warnings, List<string> Errors) ValidateChannelConfiguration(ChannelConfiguration config)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (config is null)
        {
            errors.Add("Configuration is null");
            return (false, warnings, errors);
        }

        // Check required fields
        if (string.IsNullOrWhiteSpace(config.DisplayName))
            errors.Add("Display name is required");

        if (string.IsNullOrWhiteSpace(config.WebhookUrl))
            errors.Add("Webhook URL is required");
        else if (!Uri.TryCreate(config.WebhookUrl, UriKind.Absolute, out var uri) ||
                 (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            errors.Add("Webhook URL must be a valid HTTP/HTTPS URL");

        if (string.IsNullOrWhiteSpace(config.TargetId))
            errors.Add("Target ID (channel, chat ID, etc.) is required");

        // Check timeout
        if (config.TimeoutMs < AppConstants.MinTimeoutMs)
            errors.Add($"Timeout must be at least {AppConstants.MinTimeoutMs}ms");

        if (config.TimeoutMs > AppConstants.MaxTimeoutMs)
            warnings.Add($"Timeout is very high ({config.TimeoutMs}ms) - consider reducing it");

        // Check retry settings
        if (config.MaxRetries < 0)
            errors.Add("Max retries cannot be negative");

        if (config.MaxRetries > AppConstants.MaxRetryAttempts)
            warnings.Add($"Max retries ({config.MaxRetries}) exceeds recommended maximum ({AppConstants.MaxRetryAttempts})");

        // Check filters
        if (!config.AllowedEnvironments.Any() && !config.AllowedStatuses.Any())
            warnings.Add("No environment or status filters configured - will send all notifications");

        if (config.AllowedEnvironments.Any() && !config.AllowedEnvironments.Contains(Environment.Production))
            warnings.Add("Production environment not included in allowed environments");

        // Check for sensitive data exposure in custom headers
        foreach (var header in config.CustomHeaders)
        {
            if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Contains("Token", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Contains("Key", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(header.Value))
                    warnings.Add($"Header {header.Key} appears to require sensitive data but is empty");
            }
        }

        return (errors.Count == 0, warnings, errors);
    }

    /// <summary>
    /// Validates notification service configuration
    /// </summary>
    public (bool IsValid, List<string> Warnings, List<string> Errors) ValidateNotificationConfig(NotificationConfig config)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (config is null)
        {
            errors.Add("Configuration is null");
            return (false, warnings, errors);
        }

        // Validate retry settings
        if (config.MaxRetries < 0)
            errors.Add("MaxRetries cannot be negative");

        if (config.MaxRetries > 10)
            warnings.Add("MaxRetries is very high - may cause excessive load");

        // Validate timeout
        if (config.WebhookTimeoutMs < 1000)
            errors.Add("WebhookTimeoutMs must be at least 1000ms");

        if (config.WebhookTimeoutMs > 60000)
            warnings.Add("WebhookTimeoutMs is very high - consider reducing it");

        // Validate processing interval
        if (config.ProcessingIntervalSeconds < 5)
            warnings.Add("ProcessingIntervalSeconds is very short - may cause high CPU usage");

        if (config.ProcessingIntervalSeconds > 3600)
            warnings.Add("ProcessingIntervalSeconds is very long - notifications may be delayed");

        // Validate retention
        if (config.RetentionDays < 1)
            errors.Add("RetentionDays must be at least 1");

        if (config.RetentionDays > 365)
            warnings.Add("RetentionDays is very long - consider reducing storage requirements");

        // Validate log level
        if (!IsValidLogLevel(config.LogLevel))
            errors.Add($"Invalid LogLevel: {config.LogLevel}");

        // Validate storage path if configured
        if (!string.IsNullOrWhiteSpace(config.StoragePath) && config.StorageType == "File")
        {
            if (!IsValidPath(config.StoragePath))
                warnings.Add($"StoragePath may not be writable: {config.StoragePath}");
        }

        return (errors.Count == 0, warnings, errors);
    }

    /// <summary>
    /// Checks if all required configurations are present
    /// </summary>
    public bool HasRequiredConfigurations(List<ChannelConfiguration> configs)
    {
        // At minimum, at least one configuration should be enabled
        if (!configs.Any(c => c.IsEnabled))
        {
            _logger.LogWarning("No enabled channel configurations found");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Suggests improvements to channel configuration
    /// </summary>
    public List<string> SuggestImprovements(ChannelConfiguration config)
    {
        var suggestions = new List<string>();

        if (config.TimeoutMs > 15000)
            suggestions.Add($"Consider reducing timeout from {config.TimeoutMs}ms to improve responsiveness");

        if (config.MaxRetries > 5)
            suggestions.Add($"Consider reducing max retries from {config.MaxRetries} to reduce load");

        if (!config.IncludeCommitDetails)
            suggestions.Add("Consider enabling IncludeCommitDetails for better context");

        if (!config.IncludeBuildUrl)
            suggestions.Add("Consider enabling IncludeBuildUrl for direct access to build details");

        if (config.CustomHeaders.Count == 0)
            suggestions.Add("Consider adding custom headers for authentication or tracking");

        if (!config.AllowedEnvironments.Any())
            suggestions.Add("Consider setting AllowedEnvironments filter to reduce noise");

        if (!config.AllowedStatuses.Any())
            suggestions.Add("Consider setting AllowedStatuses filter to focus on important events");

        if (config.MinimumPriority < NotificationPriority.Normal)
            suggestions.Add($"Current MinimumPriority ({config.MinimumPriority}) includes all priorities - consider raising it");

        return suggestions;
    }

    private static bool IsValidLogLevel(string logLevel)
    {
        var validLevels = new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical", "None" };
        return validLevels.Contains(logLevel, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsValidPath(string path)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            var parentDir = Path.GetDirectoryName(fullPath);
            return !string.IsNullOrEmpty(parentDir) && Directory.Exists(parentDir);
        }
        catch
        {
            return false;
        }
    }
}
