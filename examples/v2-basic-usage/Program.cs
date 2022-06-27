#nullable enable
using System;
using System.Threading.Tasks;
using dotnet_deploy_notify;
using dotnet_deploy_notify.Core.Enums;
using dotnet_deploy_notify.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Example: Basic v2.0 usage with canary deployment support
// This example demonstrates the new features in v2.0 including canary deployments
// and enhanced configuration options

namespace V2BasicUsage
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== dotnet-deploy-notify v2.0 Basic Usage Example ===\n");

            // Build the host with service configuration
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Register the deploy-notify services
                    // Configuration is loaded from appsettings.json
                    services.AddDeployNotify(context.Configuration);

                    // Register canary deployment services (new in v2.0)
                    services.AddCanaryDeployment();
                })
                .Build();

            // Get the notification service
            var notificationService = host.Services.GetRequiredService<INotificationService>();

            // Get the canary deployment engine (new in v2.0)
            var canaryEngine = host.Services.GetRequiredService<ICanaryDeploymentEngine>();

            // Get the traffic splitter service (new in v2.0)
            var trafficSplitter = host.Services.GetRequiredService<ITrafficSplitter>();

            Console.WriteLine("🚀 Starting v2.0 Basic Usage Example...\n");

            // Example 1: Simple notification (works the same as v1.x)
            Console.WriteLine("📤 Example 1: Simple Notification");
            Console.WriteLine("--------------------------------");

            var simpleNotification = new DeploymentNotification
            {
                ProjectName = "MyApi",
                Version = "2.0.0",
                Status = BuildStatus.Success,
                Environment = "production",
                BranchName = "main",
                CommitHash = "abc123def456",
                CommitAuthor = "John Doe",
                Priority = NotificationPriority.High
            };

            var simpleResult = await notificationService.SendAsync(simpleNotification);
            Console.WriteLine($"✅ Notification sent: {simpleResult.Status}");
            Console.WriteLine();

            // Example 2: Canary deployment (new in v2.0)
            Console.WriteLine("🎯 Example 2: Canary Deployment");
            Console.WriteLine("-------------------------------");

            var canaryDeployment = new CanaryDeployment
            {
                DeploymentId = $"canary-{DateTime.UtcNow:yyyyMMdd-HHmmss}",
                TrafficPercentage = 10, // Start with 10% traffic
                BaselineVersion = "1.9.2",
                CanaryVersion = "2.0.0",
                Environment = "production",
                Description = "Gradual rollout of v2.0.0"
            };

            Console.WriteLine($"📊 Starting canary deployment: {canaryDeployment.DeploymentId}");
            Console.WriteLine($"   Traffic split: {canaryDeployment.TrafficPercentage}% canary, {100 - canaryDeployment.TrafficPercentage}% baseline");

            // Register the canary deployment
            await canaryEngine.StartDeploymentAsync(canaryDeployment);

            // Monitor the deployment
            var deploymentStatus = await canaryEngine.GetStatusAsync(canaryDeployment.DeploymentId);
            Console.WriteLine($"✅ Canary deployment started successfully");
            Console.WriteLine();

            // Example 3: Adjust traffic (new in v2.0)
            Console.WriteLine("📈 Example 3: Adjusting Traffic");
            Console.WriteLine("-------------------------------");

            Console.WriteLine("🕒 Waiting 30 seconds before traffic adjustment...");
            await Task.Delay(TimeSpan.FromSeconds(30));

            Console.WriteLine("📊 Increasing canary traffic from 10% to 25%");
            await trafficSplitter.AdjustTrafficAsync(
                deploymentId: canaryDeployment.DeploymentId,
                canaryPercentage: 25
            );

            deploymentStatus = await canaryEngine.GetStatusAsync(canaryDeployment.DeploymentId);
            Console.WriteLine($"✅ Traffic adjusted successfully");
            Console.WriteLine();

            // Example 4: Send notification with canary context (new in v2.0)
            Console.WriteLine("📤 Example 4: Notification with Canary Context");
            Console.WriteLine("---------------------------------------------");

            var canaryNotification = new DeploymentNotification
            {
                ProjectName = "MyApi",
                Version = "2.0.0",
                Status = BuildStatus.Success,
                Environment = "production",
                BranchName = "main",
                CommitHash = "abc123def456",
                CanaryDeployment = new CanaryDeploymentContext
                {
                    DeploymentId = canaryDeployment.DeploymentId,
                    IsCanary = true,
                    TrafficPercentage = 25
                },
                Priority = NotificationPriority.High
            };

            var canaryResult = await notificationService.SendAsync(canaryNotification);
            Console.WriteLine($"✅ Canary notification sent: {canaryResult.Status}");
            Console.WriteLine();

            // Example 5: Check deployment health (new in v2.0)
            Console.WriteLine("🏥 Example 5: Monitoring Deployment Health");
            Console.WriteLine("-----------------------------------------");

            deploymentStatus = await canaryEngine.GetStatusAsync(canaryDeployment.DeploymentId);
            Console.WriteLine($"📊 Deployment Status: {deploymentStatus.Status}");
            Console.WriteLine($"📊 Traffic Split: {deploymentStatus.CurrentTrafficPercentage}% canary");
            Console.WriteLine($"📊 Error Rate: {deploymentStatus.ErrorRate * 100:F2}%");
            Console.WriteLine();

            // Example 6: Rollback request (new in v2.0)
            Console.WriteLine("🔙 Example 6: Requesting Rollback");
            Console.WriteLine("-------------------------------");

            var rollbackService = host.Services.GetRequiredService<IRollbackService>();
            var rollbackRequest = new RollbackRequest
            {
                DeploymentId = canaryDeployment.DeploymentId,
                RollbackReason = "High error rate detected during canary deployment",
                RollbackToVersion = "1.9.2"
            };

            var rollbackResult = await rollbackService.RequestRollbackAsync(rollbackRequest);
            Console.WriteLine($"✅ Rollback requested: {rollbackResult.Status}");
            Console.WriteLine();

            Console.WriteLine("🎉 All v2.0 features demonstrated successfully!");
            Console.WriteLine("\n=== Summary ===");
            Console.WriteLine("✅ Simple notifications (backward compatible)");
            Console.WriteLine("✅ Canary deployments with traffic splitting");
            Console.WriteLine("✅ Traffic adjustment during deployment");
            Console.WriteLine("✅ Notifications with canary context");
            Console.WriteLine("✅ Deployment health monitoring");
            Console.WriteLine("✅ Automatic rollback requests");

            // Keep the application running for demonstration
            Console.WriteLine("\n💡 Press Ctrl+C to exit...");
            await host.WaitForShutdownAsync();
        }
    }
}
