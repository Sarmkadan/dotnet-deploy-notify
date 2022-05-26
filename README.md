# DotNetDeployNotify

A comprehensive deployment notification pipeline for .NET applications. Send build status updates to Slack, Discord, Telegram, and webhooks.

![Build](https://github.com/sarmkadan/dotnet-deploy-notify/actions/workflows/build.yml/badge.svg)
![License](https://img.shields.io/badge/License-MIT-blue.svg)
[![NuGet](https://img.shields.io/nuget/v/Zaiets.dotnet.deploy.notify.svg)](https://www.nuget.org/packages/Zaiets.dotnet.deploy.notify/)

## Installation

```bash
dotnet add package Zaiets.dotnet.deploy.notify
```

## Quick Start

```csharp
// Register services
builder.Services.AddDeployNotify(configuration);

// Send notification
var notifier = host.Services.GetRequiredService<INotificationService>();
await notifier.CreateNotificationAsync(new DeploymentNotification
{
    ProjectName = "MyApp",
    Version = "2.1.0",
    Status = BuildStatus.Success,
    TargetEnvironment = Environment.Production,
    Channels = new() { NotificationChannel.Slack }
});
await notifier.SendPendingNotificationsAsync();
```

## Configuration (`appsettings.json`)

```json
{
  "NotificationService": {
    "EnvironmentChannels": {
      "Production": {
        "WebhookUrl": "https://hooks.slack.com/services/...",
        "ChannelType": "Slack"
      }
    }
  }
}
```

## License

MIT © 2026 Vladyslav Zaiets
