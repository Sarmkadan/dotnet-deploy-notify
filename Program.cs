// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

using DotNetDeployNotify.Core;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Infrastructure;
using DotNetDeployNotify.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetDeployNotify;

/// <summary>
/// Main application entry point for the deployment notification system
/// </summary>
internal sealed class Program
{
    /// <summary>
    /// Application entry point
    /// </summary>
    static async Task Main(string[] args)
    {
        try
        {
            // Build configuration
            var configuration = BuildConfiguration();

            // Setup dependency injection
            var services = new ServiceCollection();

            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .AddNotificationServices(configuration);

            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            DisplayApplicationHeader(logger);

            // Run the application
            await RunApplicationAsync(serviceProvider, logger, args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Fatal error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            System.Environment.Exit(1);
        }
    }

    /// <summary>
    /// Builds the application configuration from multiple sources
    /// </summary>
    private static IConfigurationRoot BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    /// <summary>
    /// Displays the application header with version information
    /// </summary>
    private static void DisplayApplicationHeader(ILogger<Program> logger)
    {
        logger.LogInformation("═══════════════════════════════════════════════════════");
        logger.LogInformation("🚀 {AppName} v{Version} - Deployment Notification Service",
            AppConstants.AppName, AppConstants.Version);
        logger.LogInformation("═══════════════════════════════════════════════════════");
    }

    /// <summary>
    /// Runs the main application logic
    /// </summary>
    private static async Task RunApplicationAsync(IServiceProvider serviceProvider, ILogger<Program> logger, string[] args)
    {
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        var configRepository = serviceProvider.GetRequiredService<IChannelConfigRepository>();

        // Demo: Create sample channel configurations
        await SetupSampleConfigurationsAsync(configRepository, logger);

        // Demo: Create and send sample notifications
        await RunDemoNotificationsAsync(notificationService, logger);

        // Keep application running for demo purposes
        logger.LogInformation("\n📋 Application running. Press Ctrl+C to exit...\n");
        await Task.Delay(Timeout.Infinite);
    }

    /// <summary>
    /// Sets up sample channel configurations for demonstration
    /// </summary>
    private static async Task SetupSampleConfigurationsAsync(
        IChannelConfigRepository configRepository,
        ILogger<Program> logger)
    {
        logger.LogInformation("📝 Setting up sample channel configurations...");

        // Example Telegram configuration
        var telegramConfig = CreateTelegramConfiguration();
        await configRepository.CreateAsync(telegramConfig);
        logger.LogInformation("✅ Telegram configuration created");

        // Example Slack configuration
        var slackConfig = CreateSlackConfiguration();
        await configRepository.CreateAsync(slackConfig);
        logger.LogInformation("✅ Slack configuration created");

        // Example Discord configuration
        var discordConfig = CreateDiscordConfiguration();
        await configRepository.CreateAsync(discordConfig);
        logger.LogInformation("✅ Discord configuration created\n");
    }

    /// <summary>
    /// Creates a Telegram channel configuration
    /// </summary>
    private static ChannelConfiguration CreateTelegramConfiguration()
    {
        return new ChannelConfiguration
        {
            DisplayName = "Telegram - DevOps Channel",
            ChannelType = NotificationChannel.Telegram,
            WebhookUrl = "https://api.telegram.org/bot123456/sendMessage",
            TargetId = "-1234567890",
            IncludeCommitDetails = true,
            IncludeBuildUrl = true,
            MinimumPriority = NotificationPriority.Low,
            AllowedEnvironments = new List<Environment>
            {
                Environment.Production,
                Environment.Staging
            },
            AllowedStatuses = new List<BuildStatus>
            {
                BuildStatus.Success,
                BuildStatus.Failed,
                BuildStatus.DeploymentSuccess,
                BuildStatus.DeploymentFailed
            },
            MaxRetries = 3,
            TimeoutMs = 10000
        };
    }

    /// <summary>
    /// Creates a Slack channel configuration
    /// </summary>
    private static ChannelConfiguration CreateSlackConfiguration()
    {
        return new ChannelConfiguration
        {
            DisplayName = "Slack - Deployments",
            ChannelType = NotificationChannel.Slack,
            WebhookUrl = "https://hooks.slack.com/services/YOUR/WEBHOOK/URL",
            TargetId = "#deployments",
            IncludeCommitDetails = true,
            IncludeBuildUrl = true,
            MinimumPriority = NotificationPriority.Low,
            AllowedEnvironments = new List<Environment>
            {
                Environment.Production,
                Environment.Staging,
                Environment.Development
            },
            MaxRetries = 3,
            TimeoutMs = 10000
        };
    }

    /// <summary>
    /// Creates a Discord channel configuration
    /// </summary>
    private static ChannelConfiguration CreateDiscordConfiguration()
    {
        return new ChannelConfiguration
        {
            DisplayName = "Discord - Build Status",
            ChannelType = NotificationChannel.Discord,
            WebhookUrl = "https://discordapp.com/api/webhooks/123456/ABCDEF",
            TargetId = "builds",
            IncludeCommitDetails = true,
            IncludeBuildUrl = true,
            MinimumPriority = NotificationPriority.Normal,
            AllowedEnvironments = new List<Environment>
            {
                Environment.Production
            },
            MaxRetries = 2,
            TimeoutMs = 8000
        };
    }

    /// <summary>
    /// Runs demo notifications to test the system
    /// </summary>
    private static async Task RunDemoNotificationsAsync(
        INotificationService notificationService,
        ILogger<Program> logger)
    {
        logger.LogInformation("📤 Creating and processing demo notifications...\n");

        // Demo notification 1: Successful deployment
        var successNotification = CreateSuccessNotification();
        var id1 = await notificationService.CreateNotificationAsync(successNotification);
        logger.LogInformation("✅ Created notification: {NotificationId}\n", id1);

        // Demo notification 2: Failed build
        var failedNotification = CreateFailedNotification();
        var id2 = await notificationService.CreateNotificationAsync(failedNotification);
        logger.LogInformation("✅ Created notification: {NotificationId}\n", id2);

        // Demo notification 3: Staging deployment
        var stagingNotification = CreateStagingNotification();
        var id3 = await notificationService.CreateNotificationAsync(stagingNotification);
        logger.LogInformation("✅ Created notification: {NotificationId}\n", id3);

        // Process pending notifications
        logger.LogInformation("🔄 Processing pending notifications...\n");
        var results = await notificationService.SendPendingNotificationsAsync();

        // Display results
        DisplayNotificationResults(results, logger);
    }

    /// <summary>
    /// Creates a successful deployment notification
    /// </summary>
    private static DeploymentNotification CreateSuccessNotification()
    {
        return new DeploymentNotification
        {
            ProjectName = "MyApp.Api",
            Version = "2.5.0",
            Status = BuildStatus.DeploymentSuccess,
            Message = "Deployment to production completed successfully. All health checks passed.",
            TargetEnvironment = Environment.Production,
            BranchName = "main",
            CommitHash = "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6",
            CommitAuthor = "developer@example.com",
            RepositoryUrl = "https://github.com/org/repo",
            BuildUrl = "https://ci.example.com/builds/12345",
            DurationSeconds = 145,
            Channels = new List<NotificationChannel>
            {
                NotificationChannel.Telegram,
                NotificationChannel.Slack
            },
            Priority = NotificationPriority.High
        };
    }

    /// <summary>
    /// Creates a failed build notification
    /// </summary>
    private static DeploymentNotification CreateFailedNotification()
    {
        return new DeploymentNotification
        {
            ProjectName = "MyApp.Tests",
            Version = "1.0.0",
            Status = BuildStatus.Failed,
            Message = "Build failed: 3 unit tests failed in TestSuite.Integration.\nError: Database connection timeout.",
            TargetEnvironment = Environment.Development,
            BranchName = "feature/new-auth",
            CommitHash = "b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7",
            CommitAuthor = "dev2@example.com",
            RepositoryUrl = "https://github.com/org/repo",
            BuildUrl = "https://ci.example.com/builds/12346",
            DurationSeconds = 89,
            Channels = new List<NotificationChannel>
            {
                NotificationChannel.Slack,
                NotificationChannel.Discord
            },
            Priority = NotificationPriority.Critical
        };
    }

    /// <summary>
    /// Creates a staging deployment notification
    /// </summary>
    private static DeploymentNotification CreateStagingNotification()
    {
        return new DeploymentNotification
        {
            ProjectName = "MyApp.Web",
            Version = "1.4.2",
            Status = BuildStatus.DeploymentSuccess,
            Message = "Staging deployment completed. Ready for QA testing.",
            TargetEnvironment = Environment.Staging,
            BranchName = "release/1.4",
            CommitHash = "c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8",
            CommitAuthor = "releaser@example.com",
            RepositoryUrl = "https://github.com/org/repo",
            BuildUrl = "https://ci.example.com/builds/12347",
            DurationSeconds = 234,
            Channels = new List<NotificationChannel>
            {
                NotificationChannel.Telegram,
                NotificationChannel.Slack,
                NotificationChannel.Discord
            },
            Priority = NotificationPriority.High
        };
    }

    /// <summary>
    /// Displays the notification results summary
    /// </summary>
    private static void DisplayNotificationResults(List<NotificationResult> results, ILogger<Program> logger)
    {
        // Display results
        logger.LogInformation("📊 Delivery Results:");
        logger.LogInformation("────────────────────────────────────────────");
        foreach (var result in results)
        {
            var statusIcon = result.IsSuccessful ? "✅" : "❌";
            logger.LogInformation("{StatusIcon} {Channel} | {Status} | {Duration}ms",
                statusIcon,
                result.Channel,
                result.Status,
                result.DurationMs);

            if (!result.IsSuccessful && !string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                logger.LogWarning(" Error: {ErrorMessage}", result.ErrorMessage);
            }
        }

        logger.LogInformation("────────────────────────────────────────────");

        var successCount = results.Count(r => r.IsSuccessful);
        var failureCount = results.Count(r => !r.IsSuccessful);
        logger.LogInformation(
            "Summary: {SuccessCount} succeeded, {FailureCount} failed (Total: {TotalCount})",
            successCount,
            failureCount,
            results.Count);
    }
}