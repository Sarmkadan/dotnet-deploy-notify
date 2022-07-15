#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Search;

/// <summary>
/// Extension methods for SearchCriteria to provide additional filtering and manipulation capabilities
/// </summary>
public static class SearchCriteriaExtensions
{
    /// <summary>
    /// Combines multiple SearchCriteria objects using AND logic
    /// </summary>
    /// <param name="criteria">The base criteria</param>
    /// <param name="additionalCriteria">Additional criteria to combine</param>
    /// <returns>A new SearchCriteria with combined filters</returns>
    public static SearchCriteria Combine(this SearchCriteria criteria, SearchCriteria additionalCriteria)
    {
        if (criteria == null) throw new ArgumentNullException(nameof(criteria));
        if (additionalCriteria == null) throw new ArgumentNullException(nameof(additionalCriteria));

        var result = new SearchCriteria
        {
            ProjectName = criteria.ProjectName ?? additionalCriteria.ProjectName,
            Version = criteria.Version ?? additionalCriteria.Version,
            Status = criteria.Status ?? additionalCriteria.Status,
            TargetEnvironment = criteria.TargetEnvironment ?? additionalCriteria.TargetEnvironment,
            BranchName = criteria.BranchName ?? additionalCriteria.BranchName,
            CommitAuthor = criteria.CommitAuthor ?? additionalCriteria.CommitAuthor,
            CreatedAfter = criteria.CreatedAfter ?? additionalCriteria.CreatedAfter,
            CreatedBefore = criteria.CreatedBefore ?? additionalCriteria.CreatedBefore,
            MinimumPriority = criteria.MinimumPriority ?? additionalCriteria.MinimumPriority,
            MessageContains = criteria.MessageContains ?? additionalCriteria.MessageContains,
            Limit = Math.Min(criteria.Limit, additionalCriteria.Limit),
            Offset = criteria.Offset + additionalCriteria.Offset
        };

        // Combine channels using intersection
        if (criteria.Channels != null && additionalCriteria.Channels != null)
        {
            result.Channels = criteria.Channels.Intersect(additionalCriteria.Channels).ToList();
        }
        else if (criteria.Channels != null)
        {
            result.Channels = new List<NotificationChannel>(criteria.Channels);
        }
        else if (additionalCriteria.Channels != null)
        {
            result.Channels = new List<NotificationChannel>(additionalCriteria.Channels);
        }

        return result;
    }

    /// <summary>
    /// Creates a copy of the SearchCriteria with all filters cleared
    /// </summary>
    /// <param name="criteria">The criteria to clone</param>
    /// <returns>A new SearchCriteria with same pagination but no filters</returns>
    public static SearchCriteria ClearFilters(this SearchCriteria criteria)
    {
        if (criteria == null) throw new ArgumentNullException(nameof(criteria));

        return new SearchCriteria
        {
            Limit = criteria.Limit,
            Offset = criteria.Offset
        };
    }

    /// <summary>
    /// Sets the pagination parameters (Limit and Offset)
    /// </summary>
    /// <param name="criteria">The criteria to modify</param>
    /// <param name="limit">Maximum number of items to return</param>
    /// <param name="offset">Number of items to skip</param>
    /// <returns>The modified SearchCriteria for method chaining</returns>
    public static SearchCriteria WithPagination(this SearchCriteria criteria, int limit, int offset = 0)
    {
        if (criteria == null) throw new ArgumentNullException(nameof(criteria));
        if (limit <= 0) throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be positive");
        if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative");

        criteria.Limit = limit;
        criteria.Offset = offset;
        return criteria;
    }

    /// <summary>
    /// Filters notifications by priority level (inclusive of minimum priority)
    /// </summary>
    /// <param name="notifications">The notifications to filter</param>
    /// <param name="criteria">The search criteria containing MinimumPriority</param>
    /// <returns>Filtered notifications</returns>
    public static IEnumerable<DeploymentNotification> FilterByPriority(
        this IEnumerable<DeploymentNotification> notifications,
        SearchCriteria criteria)
    {
        if (notifications == null) throw new ArgumentNullException(nameof(notifications));
        if (criteria == null) throw new ArgumentNullException(nameof(criteria));

        if (criteria.MinimumPriority.HasValue)
        {
            var minPriority = criteria.MinimumPriority.Value;
            return notifications.Where(n => n.Priority >= minPriority);
        }

        return notifications;
    }
}