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
    /// <exception cref="ArgumentNullException"><paramref name="criteria"/> or <paramref name="additionalCriteria"/> is null.</exception>
    public static SearchCriteria Combine(this SearchCriteria criteria, SearchCriteria additionalCriteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        ArgumentNullException.ThrowIfNull(additionalCriteria);

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
            Offset = checked(criteria.Offset + additionalCriteria.Offset)
        };

        // Combine channels using pattern matching
        result.Channels = (criteria.Channels, additionalCriteria.Channels) switch
        {
            ({ } c, { } a) => c.Intersect(a).ToList(),
            ({ } c, null) => new List<NotificationChannel>(c),
            (null, { } a) => new List<NotificationChannel>(a),
            _ => null
        };

        return result;
    }

    /// <summary>
    /// Creates a copy of the SearchCriteria with all filters cleared
    /// </summary>
    /// <param name="criteria">The criteria to clone</param>
    /// <returns>A new SearchCriteria with same pagination but no filters</returns>
    /// <exception cref="ArgumentNullException"><paramref name="criteria"/> is null.</exception>
    public static SearchCriteria ClearFilters(this SearchCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

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
    /// <exception cref="ArgumentNullException"><paramref name="criteria"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="limit"/> is not positive or <paramref name="offset"/> is negative.</exception>
    public static SearchCriteria WithPagination(this SearchCriteria criteria, int limit, int offset = 0)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(limit, 0);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);

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
    /// <exception cref="ArgumentNullException"><paramref name="notifications"/> or <paramref name="criteria"/> is null.</exception>
    public static IEnumerable<DeploymentNotification> FilterByPriority(
        this IEnumerable<DeploymentNotification> notifications,
        SearchCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(criteria);

        return criteria.MinimumPriority.HasValue
            ? notifications.Where(n => n.Priority >= criteria.MinimumPriority.Value)
            : notifications;
    }
}