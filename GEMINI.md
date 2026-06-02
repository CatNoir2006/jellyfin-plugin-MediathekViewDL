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
*   **Configuration:** Managed via `PluginConfiguration` and a Vue.js-based dashboard.

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
# Build VueJS manually to verify syntax and bundling
cd Jellyfin.Plugin.MediathekViewDL/Configuration/Web/VueJS && npm run build
```

## 📜 Development Conventions

### 💻 Coding Standards
*   **Style:** Standard C# conventions (PascalCase). StyleCop analyzers are enforced.
*   **Clean Code:** Strictly adhere to Clean Code principles (SOLID, DRY, KISS) for both C# backend and Vue.js frontend code.
*   **Types:** Use `record` for DTOs/immutable types.
*   **Nullability:** Nullable reference types are enabled; handle `null` explicitly.
*   **Documentation:** XML summary comments for all public members.
*   **Logging:** Use `ILogger` provided by Jellyfin.
*   **Vue.js:** Use Vue 3 (Composition API / `<script setup>`).

### 🚫 Anti-Patterns (Do NOT do)
* **No Manual HttpClients:** Never instantiate `new HttpClient()`. Always inject `IHttpClientFactory` or use Jellyfin's built-in HTTP handling.
* **No Environment-Specific Paths:** Use Jellyfin's `IApplicationPaths` for plugin data storage; never hardcode paths or use `Environment.SpecialFolder`.
* **JSON Serialization:** Use `System.Text.Json` matching Jellyfin's standard configuration, avoid introducing external JSON libraries.

### 🌐 UI & Web (Vue.js)
*   **Frontend:** The web interface is built with Vue.js located in `Configuration/Web/VueJS`.
*   **Workflow:** UI changes require `npm run build` within the `VueJS` directory to generate the required `MediathekViewDLVueJS.js`.
*   **Preview:** UI previews are managed via GitHub Pages, updated automatically via GitHub Actions upon push to `master`.

## 📝 Documentation & Maintenance

*   **README.md:** Must be updated for all user-facing changes (new features, settings).
    *   **Auto-Update & Reminders:** If a task adds, removes, or modifies user-facing features or settings, automatically update the `README.md` or explicitly remind the user to do so.
    *   **Footer:** Always include the version and last commit hash: `last update plugin vX.X.X.X, commit: [hash]`.
*   **GEMINI.md:** Must be updated for architectural changes or new development rules.

## 🔍 Workflow & Verification

1.  **Understand:** Ask for clarification if a task is ambiguous.
2.  **Verify:** Always run `dotnet build` and ensure the Vue.js build completes successfully before finishing.
3.  **Research:** Use the web or other Jellyfin plugins for best practices.
