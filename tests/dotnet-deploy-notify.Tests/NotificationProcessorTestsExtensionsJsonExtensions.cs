#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Provides System.Text.Json serialization options for types used with <see cref="NotificationProcessorTestsExtensions"/>.
/// </summary>
/// <remarks>
/// The <c>NotificationProcessorTestsExtensions</c> type is a static class that only contains
/// extension methods for creating test instances. Because static types cannot be instantiated or
/// directly serialized, this helper class provides a cached <see cref="JsonSerializerOptions"/> instance
/// that can be reused by any JSON serialization logic that targets the data types created by
/// the extension methods in <see cref="NotificationProcessorTestsExtensions"/>.
/// </remarks>
public static class NotificationProcessorTestsExtensionsJsonExtensions
{
    /// <summary>
    /// Cached JSON serializer options with camelCase naming policy and web defaults.
    /// Suitable for serializing types created by <see cref="NotificationProcessorTestsExtensions"/> extension methods.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
