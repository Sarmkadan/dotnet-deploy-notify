using Microsoft.Extensions.DependencyInjection;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using DotNetDeployNotify.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddDeployNotify(builder.Configuration);
var host = builder.Build();

var notifier = host.Services.GetRequiredService<INotificationService>();

try
{
    // Custom notification with specific options and error handling
    var notification = new DeploymentNotification
    {
        ProjectName = "EnterpriseApp",
        Version = "2.0.0-rc1",
        Status = BuildStatus.Failed,
        TargetEnvironment = "Staging",
        // Overriding default behavior or adding metadata
        Metadata = new Dictionary<string, string> { { "Region", "us-east-1" } }
    };

    await notifier.CreateNotificationAsync(notification);
    var result = await notifier.SendPendingNotificationsAsync();
    
    if (!result.IsSuccess)
    {
        Console.WriteLine($"Notification failed: {result.ErrorMessage}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred during notification processing: {ex.Message}");
}
