# Gemini Project Overview: Jellyfin.Plugin.MediathekViewDL

## 📋 Project Summary

This project is a plugin for the Jellyfin media server. Its purpose is to search and download media content from the public broadcasting services in Germany (Öffentlich-rechtliche Mediatheken), such as ARD and ZDF.

The plugin integrates with Jellyfin's scheduled task system to automatically download new episodes of subscribed shows ("Abos"). It uses the [MediathekViewWeb API](https://mediathekviewweb.de/) to find content and then downloads the media files directly from the broadcasters' CDNs.

*   **GitHub:** [CatNoir2006/jellyfin-plugin-MediathekViewDL](https://github.com/CatNoir2006/jellyfin-plugin-MediathekViewDL)
*   **Technologies:** C#, .NET 9.0, SQLite (EF Core)
*   **Platform:** Jellyfin Plugin System

## 🏗️ Technical Architecture

*   **Plugin Entry:** Main `Plugin` class.
*   **Core Logic:** Encapsulated in `DownloadScheduledTask`.
*   **Dependency Injection (DI):** Services are registered in `ServiceRegistrator.cs` using `Microsoft.Extensions.DependencyInjection`.
*   **Database:** Uses EF Core with SQLite for download history and quality cache.
*   **Configuration:** Managed via `PluginConfiguration` and a `configPage.html` dashboard.

## 🛠️ Development Environment

*   **OS Support:** Development happens on Windows (PowerShell) and Linux (Bash).
*   **Command Chaining:** Use `;` (PS/Bash) or `&&` (Bash) as appropriate.
*   **Database Migrations:**
    *   Set `EnableEfDesign=true` before running `dotnet-ef`.
    *   **Bash:** `EnableEfDesign=true dotnet tool run dotnet-ef migrations add <Name> --project Jellyfin.Plugin.MediathekViewDL`
    *   **PowerShell:** `$env:EnableEfDesign="true"; dotnet tool run dotnet-ef migrations add <Name> --project Jellyfin.Plugin.MediathekViewDL`

## 🚀 Building & Testing

### Building
```bash
dotnet restore Jellyfin.Plugin.MediathekViewDL.sln
dotnet build Jellyfin.Plugin.MediathekViewDL.sln
```

### Testing
```bash
dotnet test Jellyfin.Plugin.MediathekViewDL.sln
# Verify JS Syntax
node --check Jellyfin.Plugin.MediathekViewDL/Configuration/Web/configPage.js
```

## 📜 Development Conventions

### 💻 Coding Standards
*   **Style:** Standard C# conventions (PascalCase). StyleCop analyzers are enforced.
*   **Types:** Use `record` for DTOs/immutable types.
*   **Nullability:** Nullable reference types are enabled; handle `null` explicitly.
*   **Documentation:** XML summary comments for all public members.
*   **Logging:** Use `ILogger` provided by Jellyfin.
*   **JavaScript:** Since `dotnet build` does not check JavaScript, always verify syntax using `node --check`.

### 📁 Structure & Scope
*   **File per Class:** One class per file (except nested classes).
*   **Minimal Changes:** Strictly limit changes to the requested task. Do not refactor unrelated code.

### 🌐 UI & Web (configPage.html)
*   **No String Templates:** Do **not** use `` `S${season}E${Episode}` ``. Use string concatenation (`'S'+s+'E'+e`).
*   **JS Verification:** Always run `node --check <path_to_js>` to ensure no syntax errors were introduced.

## 📝 Documentation & Maintenance

*   **README.md:** Must be updated for all user-facing changes (new features, settings).
    *   **Footer:** Always include the version and last commit hash: `last update plugin vX.X.X.X, commit: [hash]`.
*   **GEMINI.md:** Must be updated for architectural changes or new development rules.

## 🔍 Workflow & Verification

1.  **Understand:** Ask for clarification if a task is ambiguous.
2.  **Verify:** Always run `dotnet build` before finishing. The project treats warnings as errors. Additionally, check all modified JavaScript files using `node --check`.
3.  **Research:** Use the web or other Jellyfin plugins for best practices.
