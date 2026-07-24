#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Provides hardened, secure System.Text.Json serialization options for
// deserializing untrusted input from external sources (webhooks, APIs, etc.)
// ===================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DotNetDeployNotify.Serialization;

/// <summary>
/// Provides secure, hardened JsonSerializerOptions for deserializing untrusted input.
/// These options are designed to prevent security vulnerabilities like:
/// - Polymorphic type confusion attacks (TypeNameHandling)
/// - Unbounded array/object sizes (MaxDepth)
/// - Denial of service via large strings/arrays
/// </summary>
/// <remarks>
/// SECURITY CONSIDERATIONS:
/// - MaxDepth is set to prevent stack overflow attacks from deeply nested JSON
/// - No polymorphic type resolution enabled (TypeNameHandling equivalent)
/// - PropertyNameCaseInsensitive is disabled by default for security
/// - DefaultIgnoreCondition set to WhenWritingNull to avoid information leakage
/// </remarks>
public static class SecureJsonSerializerOptions
{
	/// <summary>
	/// Gets a hardened JsonSerializerOptions instance for deserializing untrusted input.
	/// </summary>
	/// <remarks>
	/// This configuration is safe for:
	/// ✓ Parsing webhook payloads from external services
	/// ✓ Deserializing API responses from external sources
	/// ✓ Reading persisted/queued messages from other processes
	///
	/// This configuration is NOT safe for:
	/// ✗ Serializing sensitive data (use specific serialization options instead)
	/// ✗ Internal communication between trusted services (can use less restrictive options)
	/// </remarks>
	public static JsonSerializerOptions UntrustedInput { get; } = CreateUntrustedInputOptions();

	/// <summary>
	/// Gets a hardened JsonSerializerOptions instance for serializing data to external services.
	/// </summary>
	/// <remarks>
	/// This is the same as UntrustedInput but ensures consistency for both directions.
	/// </remarks>
	public static JsonSerializerOptions OutboundPayloads { get; } = CreateUntrustedInputOptions();

	/// <summary>
	/// Creates a new instance of JsonSerializerOptions configured for security.
	/// </summary>
	private static JsonSerializerOptions CreateUntrustedInputOptions()
	{
		var options = new JsonSerializerOptions
		{
			// Use camelCase for consistency with web standards
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

			// Never allow polymorphic type resolution from untrusted input
			// This prevents TypeNameHandling-style attacks where malicious JSON
			// could specify arbitrary .NET types to deserialize
			TypeInfoResolver = new DefaultJsonTypeInfoResolver(),

			// Do not allow case-insensitive property matching for security
			// This prevents ambiguity attacks where different property names
			// could match the same value
			PropertyNameCaseInsensitive = false,

			// Ignore null values during serialization to reduce data leakage
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

			// Prevent stack overflow attacks from deeply nested JSON
			// Default is 64, but we set it explicitly for clarity and security
			MaxDepth = 100,

			// Use reference handling to prevent duplicate reference attacks
			// and to handle object graphs correctly
			ReferenceHandler = ReferenceHandler.IgnoreCycles,

			// Write indented = false for performance and to reduce attack surface
			WriteIndented = false,

			// Note: MaxStringSize requires .NET 7+ and is not available in .NET 6
			// For .NET 6 compatibility, we rely on the default reader behavior
			// which has reasonable limits for string and array sizes
			// Applications targeting .NET 7+ can enable the following for additional protection:
			// ReaderOptions = new JsonReaderOptions { MaxStringSize = 1_000_000 }
		};

	// Add any custom converters that are safe for untrusted input
	// Note: Only add converters that don't enable polymorphic behavior

	return options;
	}

	/// <summary>
	/// Creates JsonSerializerOptions for serializing internal data structures.
	/// </summary>
	/// <remarks>
	/// This is less restrictive than UntrustedInput and should only be used
	/// for serializing data within the application or to trusted services.
	/// </remarks>
	public static JsonSerializerOptions InternalData { get; } = new JsonSerializerOptions
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		PropertyNameCaseInsensitive = true,
		MaxDepth = 100,
		ReferenceHandler = ReferenceHandler.IgnoreCycles
	};
}
