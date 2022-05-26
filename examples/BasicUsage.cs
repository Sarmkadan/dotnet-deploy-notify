using Microsoft.Extensions.DependencyInjection;
using DotNetDeployNotify.Core.Models;
using DotNetDeployNotify.Services;
using DotNetDeployNotify.DependencyInjection;
using Microsoft.Extensions.Hosting;

// 1. Setup DI
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddDeployNotify(builder.Configuration);
var host = builder.Build();

// 2. Resolve service
var notifier = host.Services.GetRequiredService<INotificationService>();

// 3. Create and send a basic notification
await notifier.CreateNotificationAsync(new DeploymentNotification
{
    ProjectName = "MyWebApp",
    Version = "1.0.0",
    Status = BuildStatus.Success,
    TargetEnvironment = "Production"
});
await notifier.SendPendingNotificationsAsync();
