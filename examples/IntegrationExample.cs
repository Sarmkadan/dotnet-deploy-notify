using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using DotNetDeployNotify.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Integration in ASP.NET Core
builder.Services.AddDeployNotify(builder.Configuration);

var app = builder.Build();

app.MapPost("/deploy", async (INotificationService notifier) =>
{
    await notifier.CreateNotificationAsync(new DeploymentNotification
    {
        ProjectName = "MyAPI",
        Version = "1.0.1",
        Status = BuildStatus.Success,
        TargetEnvironment = "Production"
    });
    
    await notifier.SendPendingNotificationsAsync();
    return Results.Ok();
});

app.Run();
