// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core.Models;

namespace DotNetDeployNotify.Search;

/// <summary>
/// Search criteria for filtering notifications
/// </summary>
public class SearchCriteria
{
    public string? ProjectName { get; set; }
    public string? Version { get; set; }
    public BuildStatus? Status { get; set; }
    public Environment? TargetEnvironment { get; set; }
    public string? BranchName { get; set; }
    public string? CommitAuthor { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public NotificationPriority? MinimumPriority { get; set; }
    public List<NotificationChannel>? Channels { get; set; }
    public string? MessageContains { get; set; }

    public int Limit { get; set; } = 100;
    public int Offset { get; set; } = 0;
}

/// <summary>
/// Search result with pagination metadata
/// </summary>
public class SearchResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Returned { get; set; }
    public int Offset { get; set; }
    public bool HasMore => Offset + Returned < Total;
}

/// <summary>
/// Engine for searching and filtering notifications
/// </summary>
public class NotificationSearchEngine
{
    private readonly ILogger<NotificationSearchEngine> _logger;

    public NotificationSearchEngine(ILogger<NotificationSearchEngine> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Searches notifications based on criteria
    /// </summary>
    public SearchResult<DeploymentNotification> Search(
        IEnumerable<DeploymentNotification> notifications,
        SearchCriteria criteria)
    {
        _logger.LogDebug("Executing notification search with {CriteriaCount} filters", 1);

        var query = notifications.AsEnumerable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(criteria.ProjectName))
        {
            query = query.Where(n => n.ProjectName.Contains(criteria.ProjectName, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Version))
        {
            query = query.Where(n => n.Version == criteria.Version);
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(n => n.Status == criteria.Status.Value);
        }

        if (criteria.TargetEnvironment.HasValue)
        {
            query = query.Where(n => n.TargetEnvironment == criteria.TargetEnvironment.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.BranchName))
        {
            query = query.Where(n => n.BranchName == criteria.BranchName);
        }

        if (!string.IsNullOrWhiteSpace(criteria.CommitAuthor))
        {
            query = query.Where(n => n.CommitAuthor.Contains(criteria.CommitAuthor, StringComparison.OrdinalIgnoreCase));
        }

        if (criteria.CreatedAfter.HasValue)
        {
            query = query.Where(n => n.CreatedAt >= criteria.CreatedAfter.Value);
        }

        if (criteria.CreatedBefore.HasValue)
        {
            query = query.Where(n => n.CreatedAt <= criteria.CreatedBefore.Value);
        }

        if (criteria.MinimumPriority.HasValue)
        {
            query = query.Where(n => n.Priority >= criteria.MinimumPriority.Value);
        }

        if (criteria.Channels?.Any() == true)
        {
            query = query.Where(n => n.Channels.Any(c => criteria.Channels.Contains(c)));
        }

        if (!string.IsNullOrWhiteSpace(criteria.MessageContains))
        {
            query = query.Where(n => n.Message.Contains(criteria.MessageContains, StringComparison.OrdinalIgnoreCase));
        }

        var total = query.Count();
        var items = query
            .OrderByDescending(n => n.CreatedAt)
            .Skip(criteria.Offset)
            .Take(criteria.Limit)
            .ToList();

        return new SearchResult<DeploymentNotification>
        {
            Items = items,
            Total = total,
            Returned = items.Count,
            Offset = criteria.Offset
        };
    }

    /// <summary>
    /// Full-text search across notification fields
    /// </summary>
    public SearchResult<DeploymentNotification> FullTextSearch(
        IEnumerable<DeploymentNotification> notifications,
        string searchTerm,
        int limit = 100,
        int offset = 0)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new SearchResult<DeploymentNotification> { Items = new() };

        var term = searchTerm.ToLowerInvariant();

        var query = notifications.Where(n =>
            n.ProjectName.ToLowerInvariant().Contains(term) ||
            n.Version.ToLowerInvariant().Contains(term) ||
            n.BranchName.ToLowerInvariant().Contains(term) ||
            n.CommitAuthor.ToLowerInvariant().Contains(term) ||
            n.Message.ToLowerInvariant().Contains(term) ||
            n.CommitHash.ToLowerInvariant().Contains(term));

        var total = query.Count();
        var items = query
            .OrderByDescending(n => n.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToList();

        _logger.LogDebug("Full-text search for '{SearchTerm}' returned {ResultCount} results", searchTerm, items.Count);

        return new SearchResult<DeploymentNotification>
        {
            Items = items,
            Total = total,
            Returned = items.Count,
            Offset = offset
        };
    }

    /// <summary>
    /// Groups notifications by specified property
    /// </summary>
    public Dictionary<string, List<DeploymentNotification>> GroupBy(
        IEnumerable<DeploymentNotification> notifications,
        string groupByField)
    {
        return groupByField.ToLowerInvariant() switch
        {
            "project" => notifications.GroupBy(n => n.ProjectName)
                .ToDictionary(g => g.Key, g => g.ToList()),
            "status" => notifications.GroupBy(n => n.Status.ToString())
                .ToDictionary(g => g.Key, g => g.ToList()),
            "environment" => notifications.GroupBy(n => n.TargetEnvironment.ToString())
                .ToDictionary(g => g.Key, g => g.ToList()),
            "branch" => notifications.GroupBy(n => n.BranchName)
                .ToDictionary(g => g.Key, g => g.ToList()),
            "author" => notifications.GroupBy(n => n.CommitAuthor)
                .ToDictionary(g => g.Key, g => g.ToList()),
            _ => throw new ArgumentException($"Unknown group by field: {groupByField}")
        };
    }

    /// <summary>
    /// Gets aggregated statistics for notifications
    /// </summary>
    public NotificationStatistics GetStatistics(IEnumerable<DeploymentNotification> notifications)
    {
        var items = notifications.ToList();

        return new NotificationStatistics
        {
            TotalCount = items.Count,
            SuccessCount = items.Count(n => n.Status == BuildStatus.Success || n.Status == BuildStatus.DeploymentSuccess),
            FailureCount = items.Count(n => n.Status == BuildStatus.Failed || n.Status == BuildStatus.DeploymentFailed),
            AverageDuration = items.Where(n => n.DurationSeconds.HasValue).Average(n => n.DurationSeconds ?? 0),
            UniqueProjects = items.Select(n => n.ProjectName).Distinct().Count(),
            UniqueBranches = items.Select(n => n.BranchName).Distinct().Count(),
            UniqueAuthors = items.Select(n => n.CommitAuthor).Distinct().Count(),
            DateRange = (items.MinBy(n => n.CreatedAt)?.CreatedAt, items.MaxBy(n => n.CreatedAt)?.CreatedAt)
        };
    }
}

/// <summary>
/// Statistics about a set of notifications
/// </summary>
public class NotificationStatistics
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double AverageDuration { get; set; }
    public int UniqueProjects { get; set; }
    public int UniqueBranches { get; set; }
    public int UniqueAuthors { get; set; }
    public (DateTime? Min, DateTime? Max) DateRange { get; set; }

    public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount * 100 : 0;
    public double FailureRate => TotalCount > 0 ? (double)FailureCount / TotalCount * 100 : 0;
}
