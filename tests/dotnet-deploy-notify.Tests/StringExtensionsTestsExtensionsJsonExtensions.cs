#nullable enable
using System.Text.Json;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Provides JSON serialization helpers for the <see cref="StringExtensionsTestsExtensions"/> type.
/// </summary>
/// <remarks>
/// The <c>StringExtensionsTestsExtensions</c> type is a static class that only contains
/// extension methods. Because static types cannot be instantiated, there is no meaningful
/// instance data to serialize. Therefore, this helper class only exposes a cached
/// <see cref="JsonSerializerOptions"/> instance that can be reused by any future
/// serialization logic that might target related data structures.
/// </remarks>
public static class StringExtensionsTestsExtensionsJsonExtensions
{
    /// <summary>
    /// Cached JSON serializer options that use camel-case naming.
    /// </summary>
    public static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
}
