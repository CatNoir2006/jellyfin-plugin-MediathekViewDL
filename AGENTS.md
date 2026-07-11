# AGENTS.md — Jellyfin.Plugin.MediathekViewDL

Jellyfin plugin (C# / .NET 9, Vue.js 3 frontend) that searches and downloads German public broadcasting content via MediathekViewWeb API.

## Build & Test

```bash
dotnet build Jellyfin.Plugin.MediathekViewDL.sln   # also builds Vue.js via MSBuild target
dotnet test Jellyfin.Plugin.MediathekViewDL.sln
```

Vue.js frontend can be built standalone:
```bash
cd Jellyfin.Plugin.MediathekViewDL/Configuration/Web/VueJS && npm install && npm run build
```

**Prerequisites:** .NET 9 SDK (pinned in `global.json`), Node.js (for Vue build). `TreatWarningsAsErrors=true` — build warnings are failures.

## Database Migrations

Requires `EnableEfDesign` env var (excludes EF Core design-time tools from production builds):
```bash
EnableEfDesign=true dotnet tool run dotnet-ef migrations add <Name> --project Jellyfin.Plugin.MediathekViewDL
```
`dotnet-ef` is a local tool (`.config/dotnet-tools.json`). Existing migrations are in `Data/Migrations/`.

## Architecture

- **Plugin entry:** `Plugin.cs` (BasePlugin\<PluginConfiguration\>)
- **DI:** `ServiceRegistrator.cs` (IPluginServiceRegistrator)
- **Scheduled download task:** `Tasks/DownloadScheduledTask.cs`
- **Config:** `Configuration/PluginConfiguration.cs` — uses grouped settings classes (`SearchSettings`, `DownloadSettings`, etc.)
- **Database:** EF Core SQLite via `MediathekViewDlDbContext`
- **Vue.js UI:** `Configuration/Web/VueJS/` — compiled output is an embedded resource (gitignored), built as Vite library with CSS injected inline

## Code Style

- `_camelCase` for all fields (underscore prefix), `PascalCase` for everything else
- Allman braces, 4-space indent (2 for YAML/XML)
- StyleCop enforced via `jellyfin.ruleset` — key suppressions: no `this.` prefix, usings outside namespace allowed, no file headers required
- Key analyzer rules enforced as errors: CA1305 (IFormatProvider), CA1725 (parameter names), CA2016 (CancellationToken), CA2254 (static log templates)

## Jellyfin Plugin Conventions

- Never `new HttpClient()` — use `IHttpClientFactory`
- Never hardcode paths — use `IApplicationPaths`
- `System.Text.Json` only (no Newtonsoft)
- `record` types for DTOs
- XML summary comments on public members
- Jellyfin Controller/Model/EF Core packages are excluded from plugin bundle (provided by server at runtime)
- `FuzzySharp.dll` gets special MSBuild handling — `netstandard2.1` version copied to output, native runtime folders cleaned to prevent `BadImageFormatException`

## Tests

- xUnit + Moq, flat layout in `Jellyfin.Plugin.MediathekViewDL.Tests/`
- Test naming: `MethodName_ShouldExpectedBehavior` or `MethodName_ExpectedBehavior_WhenCondition`
- Every test uses explicit `// Arrange`, `// Act`, `// Assert` comment blocks
- Constructor-based setup with mocks, no shared fixtures
- When changing or adding production code, keep existing tests up to date and add new tests for new logic

## Verification

After any change: `dotnet build` and `dotnet test` must both pass. For UI changes, verify `npm run build` in the VueJS directory succeeds. The CI delegates to `jellyfin/jellyfin-meta-plugins` reusable workflows.

Update `README.md` for user-facing changes.
