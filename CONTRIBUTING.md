# Contributing to DotNetDeployNotify

Thanks for your interest in contributing! This guide covers how to set up, build, test, and submit changes.

## Prerequisites

- **.NET SDK 10.0** (projects target `net10.0`). The SDK is pinned in `global.json` (`10.0.100`, `rollForward: latestMinor`), so any 10.0.x SDK works.
- Git.

Check your SDK with `dotnet --version`.

## Build

The repository root contains more than one project/solution file, so pass the solution explicitly:

```bash
dotnet restore dotnet-deploy-notify.sln
dotnet build dotnet-deploy-notify.sln --configuration Release
```

## Run tests

```bash
dotnet test dotnet-deploy-notify.sln
```

Tests use xUnit, FluentAssertions, NSubstitute, and Moq. Shared fakes live in `tests/dotnet-deploy-notify.Tests/Fakes/`.

To run a subset:

```bash
dotnet test tests/dotnet-deploy-notify.Tests --filter "FullyQualifiedName~TrafficSplitter"
```

## Coding conventions

- Follow standard C# naming: `PascalCase` for types and members, `camelCase` for locals and parameters, `_camelCase` for private fields, and `I` prefix for interfaces.
- Use 4-space indentation, LF line endings, and Allman braces. The rules live in [`.editorconfig`](.editorconfig); run `dotnet format` before committing.
- Nullable reference types are enabled. Don't add new nullable warnings.
- Use file-scoped namespaces and place `using` directives inside the namespace.
- Add XML documentation comments to public members.
- Add the author header to new source files:
  ```csharp
  // Author: Vladyslav Zaiets | https://sarmkadan.com
  ```
- Use `ArgumentNullException.ThrowIfNull` for constructor guards, and `Result<T>` for expected failures. Reserve exceptions for programmer errors.

Match the style of the surrounding code. [`CLAUDE.md`](CLAUDE.md) has more detail on project structure and naming.

## Branches and commits

- Branch from `main`. Name branches `<type>/<short-description>`, for example `fix/validation-error-message` or `feat/retry-backoff`.
- Write commit messages with a conventional prefix: `feat:`, `fix:`, `refactor:`, `docs:`, `test:`, or `chore:`.

## Pull requests

1. **Open an issue first for large changes** (new features, public API changes, or refactors touching many files). Agree on the approach before writing code.
2. **Keep each PR to one logical change.** Split unrelated work into separate PRs.
3. **Add or update tests** for any behavior change or bug fix.
4. **Make sure CI passes.** The `CI` workflow runs restore, Release build, and tests on every PR to `main`. Run `dotnet format` and the test suite locally first.
5. **Describe what and why** in the PR description: what changed, the motivation, and how you verified it. Link the related issue.

Do not open public issues for security vulnerabilities; see [SECURITY.md](SECURITY.md).

## License and conduct

By contributing, you agree that your contributions are licensed under the [MIT License](LICENSE). Please follow the [Code of Conduct](CODE_OF_CONDUCT.md).
