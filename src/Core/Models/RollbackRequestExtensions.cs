#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides extension methods for <see cref="RollbackRequest"/> to enhance functionality
/// and provide additional utility operations for rollback request processing.
/// </summary>
public static class RollbackRequestExtensions
{
	/// <summary>
	/// Validates that the rollback request meets all business rules for execution.
	/// </summary>
	/// <param name="request">The rollback request to validate</param>
	/// <returns>True if the request is valid and can be executed; otherwise false</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null</exception>
	public static bool IsValidForExecution(this RollbackRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);
		return request.IsValid()
			&& !string.IsNullOrWhiteSpace(request.Reason)
			&& request.TargetEnvironment != Environment.Development
			&& request.Channels.Count > 0;
	}

	/// <summary>
	/// Gets the effective priority for rollback notifications, considering both
	/// the request priority and the environment severity.
	/// </summary>
	/// <param name="request">The rollback request</param>
	/// <returns>The effective notification priority</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null</exception>
	public static NotificationPriority GetEffectivePriority(this RollbackRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);

		// Production rollbacks are always critical regardless of configured priority
		if (request.TargetEnvironment == Environment.Production)
		{
			return NotificationPriority.Critical;
		}

		// For non-production, use the configured priority
		return request.Priority;
	}

	/// <summary>
	/// Gets the list of notification channels that should receive rollback notifications,
	/// filtered by priority and environment rules.
	/// </summary>
	/// <param name="request">The rollback request</param>
	/// <returns>Filtered list of notification channels</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null</exception>
	public static IReadOnlyList<NotificationChannel> GetEffectiveChannels(this RollbackRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);

		// Always include all channels for production rollbacks
		if (request.TargetEnvironment == Environment.Production)
		{
			return request.Channels.AsReadOnly();
		}

		// For non-production, filter based on priority
		var effectivePriority = request.GetEffectivePriority();

		if (effectivePriority < NotificationPriority.High)
		{
			// Low and Normal priority: only include Email and Webhook
			return request.Channels
				.Where(c => c == NotificationChannel.Email || c == NotificationChannel.Webhook)
				.ToList()
				.AsReadOnly();
		}

		// High and Critical priority: include all channels
		return request.Channels.AsReadOnly();
	}

	/// <summary>
	/// Creates a deep copy of the rollback request to allow safe modifications
	/// without affecting the original request.
	/// </summary>
	/// <param name="request">The rollback request to copy</param>
	/// <returns>A new RollbackRequest instance with the same values</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null</exception>
	public static RollbackRequest DeepCopy(this RollbackRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);

		return new RollbackRequest
		{
			Id = request.Id,
			ProjectName = request.ProjectName,
			TargetVersion = request.TargetVersion,
			CurrentVersion = request.CurrentVersion,
			TargetEnvironment = request.TargetEnvironment,
			RequestedBy = request.RequestedBy,
			Reason = request.Reason,
			Channels = new List<NotificationChannel>(request.Channels),
			Priority = request.Priority,
			CreatedAt = request.CreatedAt,
			Metadata = new Dictionary<string, object>(request.Metadata),
			// Note: IsValid and GetSummary are computed properties, not copied
		};
	}
}
