# AGENTS.md — Jellyfin.Plugin.MediathekViewDL

Jellyfin plugin (C# / .NET 9, Vue.js 3 frontend) that searches and downloads German public broadcasting content via MediathekViewWeb API.

## Upstream / Original Repo

The original/canonical upstream is **`CatNoir2006/jellyfin-plugin-MediathekViewDL`**.

- The local `origin` remote in this checkout points at a personal fork (`CatNoirBot/...`) and is **not** the source of truth.
- The plugin manifest (`build.yaml` → `owner: "CatNoir2006"`) and the Jellyfin plugin manifest repository (`CatNoir2006/jellyfin-plugin-manifest`) are owned by `CatNoir2006`.
- **When searching GitHub for issues, PRs, releases, or historical context, always scope queries to `CatNoir2006/jellyfin-plugin-MediathekViewDL`** (e.g. via `repo:CatNoir2006/jellyfin-plugin-MediathekViewDL` in `search_issues`, `search_pull_requests`, `search_code`, `list_issues`, etc.).

## Build & Test

```bash
dotnet build Jellyfin.Plugin.MediathekViewDL.sln   # also builds Vue.js via MSBuild target
dotnet test  Jellyfin.Plugin.MediathekViewDL.sln
```

Frontend-only:

```bash
cd Jellyfin.Plugin.MediathekViewDL/Configuration/Web/VueJS && npm install && npm run build
npm test                                            # runs vitest run (also via "npm run test")
```

**Prerequisites:** .NET 9 SDK (pinned in `global.json`), Node.js (for Vue build). `TreatWarningsAsErrors=true` — warnings fail the build.

## Database Migrations

EF design-time tools are excluded from production builds via the `PluginDependencyExcludeAssets` flag. To add a migration:

```bash
EnableEfDesign=true dotnet tool run dotnet-ef migrations add <Name> --project Jellyfin.Plugin.MediathekViewDL
```

`dotnet-ef` is a local tool declared in `.config/dotnet-tools.json`. Existing migrations live in `Data/Migrations/`.

## Architecture

- **Plugin entry:** `Plugin.cs` (`BasePlugin<PluginConfiguration>`, `IHasWebPages`)
- **DI:** `ServiceRegistrator.cs` (`IPluginServiceRegistrator`) — registers `DbContext`, `IHttpClientFactory` clients (typed `IMediathekViewApiClient` + named `FileDownloaderClient`), `MigrationHostedService`, and the `LiveTv` tuner/listings
- **Scheduled tasks:** `Tasks/DownloadScheduledTask.cs`, `Tasks/StrmCleanupTask.cs`, `Tasks/TempFileCleanup.cs`
- **Config:** `Configuration/PluginConfiguration.cs` — grouped settings under `Configuration/Groups/` and `Configuration/SubscriptionSettings/`
- **Database:** EF Core SQLite via `MediathekViewDlDbContext`; DB file at `<DataFolderPath>/mediathek-dl.db`
- **Vue.js UI:** `Configuration/Web/VueJS/` — Vite library build, CSS inlined into the JS via `cssInjectedByJs()` plugin, output (`MediathekViewDLVueJS.js`) is gitignored and embedded as a resource by `Jellyfin.Plugin.MediathekViewDL.csproj`
- **External APIs:** MediathekViewWeb (`/api/query`) + Zapp (`api.zapp.mediathekview.de`) — see `MediathekViewDL_API.md`
- **Dev container:** `docker-compose.yml` builds via `Debug/build-jprm.sh` + `Debug/Dockerfile.builder` and serves the UI through nginx on port `8097`

## Code Style

- `_camelCase` for all fields (underscore prefix), `PascalCase` for everything else
- Allman braces, 4-space indent (2 for YAML/XML), `LF` line endings
- StyleCop + analyzers enforced via `jellyfin.ruleset`; key suppressions: no `this.` prefix, usings outside namespace allowed, no file headers required, `_` field prefix allowed
- Analyzer rules enforced as errors (from `jellyfin.ruleset`): CA1305 (IFormatProvider), CA1725 (parameter names), CA1727 (call async methods when in an async method), CA1843 (`WaitAll` with a single task), CA2016 (CancellationToken forwarding), CA2254 (static log templates)
- XML `<summary>` comments on public members; `GenerateDocumentationFile=true`

## Jellyfin Plugin Conventions

- Never `new HttpClient()` — inject `IHttpClientFactory` / use `AddHttpClient<T>` / `AddHttpClient(name)`
- Never hardcode paths — use `IApplicationPaths` / `Plugin.Instance!.DataFolderPath`
- `System.Text.Json` only (no Newtonsoft)
- `record` types for DTOs
- Jellyfin Controller/Model/EF Core packages are excluded from the plugin bundle via `PluginDependencyExcludeAssets` (provided by the server at runtime)
- `FuzzySharp.dll` gets special MSBuild handling — the `netstandard2.1` build is copied to output and the `runtimes/` native folders are cleaned to prevent `BadImageFormatException`

## Tests

- xUnit + Moq, flat layout in `Jellyfin.Plugin.MediathekViewDL.Tests/`
- Test naming: `MethodName_ShouldExpectedBehavior` or `MethodName_ExpectedBehavior_WhenCondition`
- Every test uses explicit `// Arrange`, `// Act`, `// Assert` comment blocks
- Constructor-based setup with mocks; no shared fixtures
- Coverlet collector wired for coverage
- When changing or adding production code, keep existing tests up to date and add new tests for new logic

## CI / Verification

After any change: `dotnet build` and `dotnet test` must pass. For UI changes, also verify `npm run build` and `npm test` in the VueJS directory succeed.

CI workflows (`.github/workflows/`) delegate to `jellyfin/jellyfin-meta-plugins` reusable workflows:
- `build.yaml` — runs only on `master` and PRs targeting `master`, ignoring `**/*.md`
- `test.yaml` — same triggers as build
- `scan-codeql.yaml` — weekly + push/PR, against `jellyfin/jellyfin-plugin-template`
- `frontend-tests.yaml` — only when `Configuration/Web/**` changes
- `pages.yml` — deploys a static preview to GitHub Pages
- `release.yaml` — builds via JPRM, uploads artifact to the GitHub release, and pushes a new entry into `CatNoir2006/jellyfin-plugin-manifest` (requires `MANIFEST_PAT` secret)

## Housekeeping

- Update `README.md` for user-facing changes.
- Use `RELEASE_TEMPLATE.md` as the body for new GitHub releases.
- Branch convention: `feature/issue-<number>-<short-description>` off `master`. Commit messages follow `[Improvement]:`, `[Feature]:`, `[bug]` style (mix of German and English).