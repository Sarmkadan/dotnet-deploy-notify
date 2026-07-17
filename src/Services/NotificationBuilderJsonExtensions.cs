#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Services;

/// <summary>
/// Provides System.Text.Json serialization extensions for NotificationBuilder
/// </summary>
public static class NotificationBuilderJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a NotificationBuilder instance to a JSON string
    /// </summary>
    /// <param name="value">The NotificationBuilder instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the NotificationBuilder</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this NotificationBuilder value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value.Build(), options);
    }

    /// <summary>
    /// Deserializes a JSON string to a NotificationBuilder instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A NotificationBuilder instance populated from the JSON</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null or empty</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized</exception>
    public static NotificationBuilder? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        var notification = JsonSerializer.Deserialize<DeploymentNotification>(json, _jsonOptions);
        return notification is null ? null : new NotificationBuilder().WithNotification(notification);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a NotificationBuilder instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">The resulting NotificationBuilder instance, or null if deserialization fails</param>
    /// <returns>True if deserialization succeeds; otherwise, false</returns>
    public static bool TryFromJson(string json, out NotificationBuilder? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            var notification = JsonSerializer.Deserialize<DeploymentNotification>(json, _jsonOptions);
            if (notification is not null)
            {
                value = new NotificationBuilder().WithNotification(notification);
                return true;
            }

            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Populates a NotificationBuilder from a deserialized DeploymentNotification
    /// </summary>
    /// <param name="builder">The NotificationBuilder instance to populate</param>
    /// <param name="notification">The DeploymentNotification containing the data</param>
    /// <returns>The populated NotificationBuilder instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder or notification is null</exception>
    private static NotificationBuilder WithNotification(this NotificationBuilder builder, DeploymentNotification notification)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(notification);

        return builder
            .WithProject(notification.ProjectName, notification.Version ?? string.Empty)
            .WithStatus(notification.Status, notification.Message)
            .WithEnvironment(notification.TargetEnvironment)
            .WithBranch(
                notification.BranchName ?? string.Empty,
                notification.CommitHash ?? string.Empty,
                notification.CommitAuthor ?? string.Empty)
            .WithRepository(notification.RepositoryUrl ?? string.Empty)
            .WithBuildUrl(notification.BuildUrl ?? string.Empty)
            .WithDuration(notification.DurationSeconds ?? 0)
            .WithChannels(notification.Channels ?? [])
            .WithPriority(notification.Priority)
            .WithMetadata(notification.Metadata ?? new Dictionary<string, object>());
    }
}