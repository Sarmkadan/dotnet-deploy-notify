#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Data;
using DotNetDeployNotify.Services;

namespace DotNetDeployNotify.CLI;

/// <summary>
/// Handles execution of parsed CLI commands
/// </summary>
public sealed class CommandHandler
{
    private readonly INotificationService _notificationService;
    private readonly IChannelConfigRepository _configRepository;
    private readonly IDeploymentHistoryService _historyService;
    private readonly IRollbackNotificationService _rollbackNotificationService;
    private readonly ILogger<CommandHandler> _logger;

    public CommandHandler(
        INotificationService notificationService,
        IChannelConfigRepository configRepository,
        IDeploymentHistoryService historyService,
        IRollbackNotificationService rollbackNotificationService,
        ILogger<CommandHandler> logger)
    {
        _notificationService = notificationService;
        _configRepository = configRepository;
        _historyService = historyService;
        _rollbackNotificationService = rollbackNotificationService;
        _logger = logger;
    }

    /// <summary>
    /// Executes the parsed command
    /// </summary>
    public async Task<int> ExecuteAsync(ParsedCommand command)
    {
        try
        {
            if (!command.Success)
            {
                Console.Error.WriteLine($"❌ {command.Error}");
                return 1;
            }

            if (!string.IsNullOrWhiteSpace(command.Output))
            {
                Console.WriteLine(command.Output);
                return 0;
            }

            return command.CommandName switch
            {
                "send" => await HandleSendCommandAsync(command),
                "list" => await HandleListCommandAsync(command),
                "config" => await HandleConfigCommandAsync(command),
                "health" => await HandleHealthCommandAsync(command),
                "history" => await HandleHistoryCommandAsync(command),
                "rollback" => await HandleRollbackCommandAsync(command),
                _ => throw new InvalidOperationException($"Command handler not found: {command.CommandName}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command: {CommandName}", command.CommandName);
            Console.Error.WriteLine($"❌ Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Handles the 'send' command to send a deployment notification
    /// </summary>
    private async Task<int> HandleSendCommandAsync(ParsedCommand command)
    {
        var projectName = command.GetParameter("project");
        var version = command.GetParameter("version");
        var statusStr = command.GetOption("status");

        if (string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(version) ||
            string.IsNullOrWhiteSpace(statusStr))
        {
            Console.Error.WriteLine("❌ Missing required options: --status");
            return 1;
        }

        if (!Enum.TryParse<BuildStatus>(statusStr, true, out var status))
        {
            Console.Error.WriteLine($"❌ Invalid status: {statusStr}");
            return 1;
        }

        var environmentStr = command.GetOption("environment") ?? "Production";
        if (!Enum.TryParse<DotNetDeployNotify.Core.Environment>(environmentStr, true, out var environment))
        {
            Console.Error.WriteLine($"❌ Invalid environment: {environmentStr}");
            return 1;
        }

        var channelsStr = command.GetOption("channels") ?? "Slack,Telegram";
        var channels = ParseChannels(channelsStr);

        var notification = new DeploymentNotification
        {
            ProjectName = projectName,
            Version = version,
            Status = status,
            TargetEnvironment = environment,
            BranchName = command.GetOption("branch") ?? "main",
            Message = command.GetOption("message") ?? $"Notification for {projectName} v{version}",
            CommitHash = Guid.NewGuid().ToString().Substring(0, 8),
            CommitAuthor = "cli-user",
            RepositoryUrl = "https://github.com/org/repo",
            BuildUrl = "https://ci.example.com/builds/cli",
            DurationSeconds = 0,
            Channels = channels,
            Priority = status == BuildStatus.Failed ? NotificationPriority.Critical : NotificationPriority.Normal
        };

        var notificationId = await _notificationService.CreateNotificationAsync(notification);
        var results = await _notificationService.SendPendingNotificationsAsync();

        var successCount = results.Count(r => r.IsSuccessful);
        Console.WriteLine($"✅ Notification {notificationId} sent to {successCount}/{results.Count} channels");

        return successCount == results.Count ? 0 : 1;
    }

    /// <summary>
    /// Handles the 'list' command to display configurations or notifications
    /// </summary>
    private async Task<int> HandleListCommandAsync(ParsedCommand command)
    {
        var listType = command.GetParameter("type")?.ToLowerInvariant();
        var limitStr = command.GetOption("limit") ?? "10";

        if (!int.TryParse(limitStr, out var limit) || limit <= 0)
        {
            limit = 10;
        }

        if (listType == "configs")
        {
            var configs = await _configRepository.GetAllAsync();
            if (configs.Count == 0)
            {
                Console.WriteLine("No channel configurations found.");
                return 0;
            }

            Console.WriteLine("Channel Configurations:");
            Console.WriteLine("───────────────────────────────────────────────────");

            var count = 0;
            foreach (var config in configs.Take(limit))
            {
                Console.WriteLine($"  • {config.DisplayName}");
                Console.WriteLine($"    Type: {config.ChannelType}");
                Console.WriteLine($"    Webhook: {(config.WebhookUrl?.Length > 50 ? config.WebhookUrl.Substring(0, 47) + "..." : config.WebhookUrl)}");
                Console.WriteLine($"    Min Priority: {config.MinimumPriority}");
                Console.WriteLine();
                count++;
            }

            if (configs.Count > limit)
                Console.WriteLine($"... and {configs.Count - limit} more (use --limit to see more)");

            return 0;
        }

        if (listType == "notifications")
        {
            Console.WriteLine("Recent notifications would be displayed here");
            return 0;
        }

        Console.Error.WriteLine($"❌ Unknown list type: {listType}. Use 'configs' or 'notifications'");
        return 1;
    }

    /// <summary>
    /// Handles the 'config' command to manage channel configurations
    /// </summary>
    private async Task<int> HandleConfigCommandAsync(ParsedCommand command)
    {
        var action = command.GetParameter("action")?.ToLowerInvariant();

        if (action == "list")
        {
            var configs = await _configRepository.GetAllAsync();
            Console.WriteLine($"Found {configs.Count} configurations");
            return 0;
        }

        if (action == "add")
        {
            var channelType = command.GetOption("type");
            var webhookUrl = command.GetOption("webhook");

            if (string.IsNullOrWhiteSpace(channelType) || string.IsNullOrWhiteSpace(webhookUrl))
            {
                Console.Error.WriteLine("❌ Missing required options: --type and --webhook");
                return 1;
            }

            if (!Enum.TryParse<NotificationChannel>(channelType, true, out var channel))
            {
                Console.Error.WriteLine($"❌ Invalid channel type: {channelType}");
                return 1;
            }

            var config = new ChannelConfiguration
            {
                DisplayName = $"{channel} - CLI Config",
                ChannelType = channel,
                WebhookUrl = webhookUrl,
                TargetId = "default",
                MaxRetries = 3,
                TimeoutMs = 10000
            };

            await _configRepository.CreateAsync(config);
            Console.WriteLine($"✅ Configuration added: {config.DisplayName}");
            return 0;
        }

        if (action == "remove")
        {
            Console.WriteLine("Remove action requires configuration ID (not yet implemented)");
            return 0;
        }

        Console.Error.WriteLine($"❌ Unknown action: {action}");
        return 1;
    }

    /// <summary>
    /// Handles the 'health' command to check system status
    /// </summary>
    private async Task<int> HandleHealthCommandAsync(ParsedCommand command)
    {
        var detailed = command.HasOption("detailed");

        Console.WriteLine("Health Check Results:");
        Console.WriteLine("───────────────────────────────────────────────────");

        try
        {
            var configs = await _configRepository.GetAllAsync();
            Console.WriteLine($"✅ Database: OK (found {configs.Count} configurations)");

            if (detailed)
            {
                foreach (var config in configs)
                {
                    var status = string.IsNullOrWhiteSpace(config.WebhookUrl) ? "❌" : "✅";
                    Console.WriteLine($"   {status} {config.DisplayName}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Database: FAILED ({ex.Message})");
            return 1;
        }

        Console.WriteLine("✅ Overall Status: Healthy");
        return 0;
    }

    /// <summary>
    /// Parses comma-separated channel names into NotificationChannel enums
    /// </summary>
    private List<NotificationChannel> ParseChannels(string channelsStr)
    {
        var channels = new List<NotificationChannel>();

        foreach (var channelName in channelsStr.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (Enum.TryParse<NotificationChannel>(channelName.Trim(), true, out var channel))
            {
                channels.Add(channel);
            }
        }

        return channels.Any() ? channels : new List<NotificationChannel> { NotificationChannel.Slack };
    }

    /// <summary>
    /// Handles the 'history' command to show deployment history and statistics
    /// </summary>
    private async Task<int> HandleHistoryCommandAsync(ParsedCommand command)
    {
        var projectName = command.GetParameter("project");
        if (string.IsNullOrWhiteSpace(projectName))
        {
            Console.Error.WriteLine("❌ Missing required parameter: project");
            return 1;
        }

        var limitStr = command.GetOption("limit") ?? "20";
        if (!int.TryParse(limitStr, out var limit) || limit <= 0)
            limit = 20;

        var showStats = command.HasOption("stats");

        var history = await _historyService.GetProjectHistoryAsync(projectName, limit);

        Console.WriteLine($"Deployment History — {projectName}");
        Console.WriteLine("───────────────────────────────────────────────────");

        if (history.Count == 0)
        {
            Console.WriteLine("No deployment history found.");
            return 0;
        }

        foreach (var entry in history)
        {
            var icon = entry.IsSuccessful ? "✅" : "❌";
            var rollbackTag = entry.IsRollback ? " [ROLLBACK]" : string.Empty;
            Console.WriteLine($"  {icon} v{entry.Version}{rollbackTag}  {entry.FinalStatus}  {entry.TargetEnvironment}  {entry.DeployedAt:yyyy-MM-dd HH:mm} UTC");
            if (!string.IsNullOrWhiteSpace(entry.CommitAuthor))
                Console.WriteLine($"     By: {entry.CommitAuthor}  Commit: {entry.CommitHash[..Math.Min(7, entry.CommitHash.Length)]}");
        }

        if (showStats)
        {
            var stats = await _historyService.GetStatisticsAsync(projectName);
            Console.WriteLine();
            Console.WriteLine("Statistics:");
            Console.WriteLine($"  Total:       {stats.TotalDeployments}");
            Console.WriteLine($"  Successful:  {stats.SuccessfulDeployments}");
            Console.WriteLine($"  Failed:      {stats.FailedDeployments}");
            Console.WriteLine($"  Rollbacks:   {stats.RollbackCount}");
            Console.WriteLine($"  Success rate:{stats.SuccessRate:F1}%");
            if (stats.AverageDurationSeconds.HasValue)
                Console.WriteLine($"  Avg duration:{stats.AverageDurationSeconds:F0}s");
        }

        return 0;
    }

    /// <summary>
    /// Handles the 'rollback' command to initiate a rollback and send notifications
    /// </summary>
    private async Task<int> HandleRollbackCommandAsync(ParsedCommand command)
    {
        var projectName    = command.GetParameter("project");
        var targetVersion  = command.GetParameter("target-version");
        var currentVersion = command.GetOption("current-version");

        if (string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(targetVersion))
        {
            Console.Error.WriteLine("❌ Missing required parameters: project and target-version");
            return 1;
        }

        var environmentStr = command.GetOption("environment") ?? "Production";
        if (!Enum.TryParse<DotNetDeployNotify.Core.Environment>(environmentStr, true, out var environment))
        {
            Console.Error.WriteLine($"❌ Invalid environment: {environmentStr}");
            return 1;
        }

        var channelsStr = command.GetOption("channels") ?? "Slack";
        var channels = ParseChannels(channelsStr);

        var request = new RollbackRequest
        {
            ProjectName = projectName,
            TargetVersion = targetVersion,
            CurrentVersion = currentVersion ?? "unknown",
            TargetEnvironment = environment,
            RequestedBy = command.GetOption("requested-by") ?? "cli-user",
            Reason = command.GetOption("reason") ?? string.Empty,
            Channels = channels,
            Priority = NotificationPriority.High
        };

        Console.WriteLine($"🔄 Initiating rollback: {projectName} → v{targetVersion} [{environment}]");

        var results = await _rollbackNotificationService.NotifyRollbackInitiatedAsync(request);
        var successCount = results.Count(r => r.IsSuccessful);

        Console.WriteLine($"✅ Rollback notification dispatched to {successCount}/{results.Count} channels");
        return successCount > 0 || !results.Any() ? 0 : 1;
    }
}
