# CLAUDE.md

## Overview
Console app (.NET 10, `net10.0`) that sends deployment notifications to webhook channels (Slack, Discord, Telegram, generic webhooks), with optional canary rollout, retry, pipeline and background-worker subsystems. See `docs/ARCHITECTURE.md` for the full module map.

## Build
```bash
dotnet restore
dotnet build --configuration Release
dotnet run --project dotnet-deploy-notify.csproj   # demo flow, config from appsettings.json
dotnet publish --configuration Release --output ./publish
docker compose up --build                          # Dockerfile / docker-compose.yml
```
SDK pinned in `global.json` (10.0.100, rollForward latestMinor). Shared props in `Directory.Build.props` (nullable, implicit usings, warnings not errors).

## Tests
```bash
dotnet test                                         # whole solution (dotnet-deploy-notify.sln)
dotnet test tests/dotnet-deploy-notify.Tests --filter "FullyQualifiedName~TrafficSplitter"
python3 aider_buildcmd.py                           # dotnet test --no-build, minimal verbosity
```
Stack: xUnit 2.9, FluentAssertions 7, NSubstitute + Moq. Fakes live in `tests/dotnet-deploy-notify.Tests/Fakes/`. Benchmarks (BenchmarkDotNet) in `dotnet-deploy-notify.Benchmarks/`; run with `dotnet run -c Release --project dotnet-deploy-notify.Benchmarks`.

CI (`.github/workflows/ci.yml`, `build.yml`) runs restore -> build Release -> test -> publish on push/PR to `main`.

## Lint / format
No analyzer package; style is enforced by `.editorconfig` only. Run `dotnet format` before committing. Key rules: 4-space indent, LF, Allman braces, file-scoped namespaces, `var` everywhere, usings inside namespace, System usings first.

## Key directories and entry points
- `Program.cs` - entry point; builds `IConfiguration` + plain `ServiceCollection`, calls `Infrastructure.DependencyInjection.AddNotificationServices()`, runs a demo send loop. Not a Generic Host.
- `GlobalUsings.cs` - global usings for the main project.
- `src/Core/` - enums, domain models, exceptions (no dependencies on other modules).
- `src/Configuration/` - `DotnetDeployNotifyOptions` (section `DotnetDeployNotify`), `CanaryOptions` (section `CanaryDeployment`), DataAnnotations validation.
- `src/Services/` - `NotificationService`, `WebhookDispatcher` (typed HttpClient), `PayloadBuilder`, `ValidationService`, rollback/history/batch/metrics services.
- `src/Data/Repositories.cs` - in-memory singleton repositories (the only state; no persistence).
- `src/Channels/` - `IChannelStrategy` strategy pattern + `ChannelStrategyResolver`.
- `src/Canary/` - `CanaryDeploymentEngine`, `TrafficSplitter`.
- `src/Infrastructure/DependencyInjection.cs` - minimal composition root used by `Program.cs`.
- `src/DependencyInjection/ServiceCollectionExtensions.cs` - opt-in extras (CLI, caching, event bus, middleware, workers).
- `src/Middleware/`, `src/Events/`, `src/BackgroundWorkers/` - pipeline, in-memory event bus, `IHostedService` workers (not started by `Program.cs`).
- `tests/dotnet-deploy-notify.Tests/` - flat xUnit project, one file per type under test.
- `examples/` - usage samples (excluded from compile via csproj `Compile Remove`).
- `docs/` - per-type reference docs, `ARCHITECTURE.md`, `docker-guide.md`, `migration-guide-v2.md`.

## Conventions
- Root namespace `DotNetDeployNotify`; sub-namespace mirrors folder under `src/` (`DotNetDeployNotify.Services`, `.Canary`, ...).
- Interfaces prefixed `I`, implementations same name without prefix (`INotificationService` / `NotificationService`).
- Each type commonly has a helper triple: `XExtensions.cs`, `XJsonExtensions.cs`, `XValidation.cs`; tests mirror them as `XTests.cs`, `XExtensionsTests.cs`, `XJsonExtensionsTests.cs`, `XValidationTests.cs`.
- Test method names: `Method_Scenario_ExpectedResult` (e.g. `Current_Setter_Throws_WhenContextAlreadyExists`).
- File header comment `Author: Vladyslav Zaiets | https://sarmkadan.com` on source files; XML doc comments on public members (`GenerateDocumentationFile` is on).
- Constructor guard clauses via `ArgumentNullException.ThrowIfNull`; `Result<T>` in `src/Results/` for expected failures, exceptions for programmer errors.
- Register HTTP-using services as typed clients (`AddHttpClient<IFoo, Foo>()`), never a parallel `AddScoped` (loses configured headers).
- Commit messages: conventional prefixes (`feat:`, `fix:`, `refactor:`, `docs:`, `chore:`).
- Do not commit `bin/`, `obj/`, `.aider*`, `verbose.proj`, `appsettings.json` secrets (use `appsettings.example.json` as template).
