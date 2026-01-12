# Gemini Project Overview: Jellyfin.Plugin.MediathekViewDL

## Project Overview

This project is a plugin for the Jellyfin media server. Its purpose is to search and download media content from the public broadcasting services in Germany (Öffentlich-rechtliche Mediatheken), such as ARD and ZDF.

The plugin integrates with Jellyfin's scheduled task system to automatically download new episodes of subscribed shows ("Abos"). It uses the [MediathekViewWeb API](https://mediathekviewweb.de/) to find content and then downloads the media files directly from the broadcasters' CDNs. The project's GitHub repository can be found here: [CatNoir2006/jellyfin-plugin-MediathekViewDL](https://github.com/CatNoir2006/jellyfin-plugin-MediathekViewDL). When working on this project, the agent should always refer to this specific repository and not attempt to search for other repositories.

**Key Technologies:**
*   **Language:** C#
*   **Framework:** .NET 9.0
*   **Platform:** Jellyfin Plugin System

**Architecture:**
*   The project is structured as a standard .NET solution with a main plugin project (`Jellyfin.Plugin.MediathekViewDL`) and a test project (`Jellyfin.Plugin.MediathekViewDL.Tests`).
*   It follows Jellyfin's plugin architecture, with a main `Plugin` class as the entry point.
*   The core logic is encapsulated in a scheduled task (`DownloadScheduledTask`), which runs periodically to fetch content for user-defined subscriptions.
*   **Dependency Injection (DI):** Services are registered for dependency injection in `ServiceRegistrator.cs`. This class implements `IPluginServiceRegistrator` and uses `Microsoft.Extensions.DependencyInjection` to register various services (e.g., `MediathekViewApiClient`, `FFmpegService`, `FileDownloader`) with appropriate lifetimes (`AddSingleton`, `AddTransient`). This allows for loose coupling and easier testing of components.
*   Configuration is managed through a `PluginConfiguration` class and a user-facing HTML page (`configPage.html`) that is served via the Jellyfin dashboard.

## Building and Running

### Building the Plugin

The project is built using the standard .NET CLI.

1.  **Restore dependencies:**
    ```bash
    dotnet restore Jellyfin.Plugin.MediathekViewDL.sln
    ```

2.  **Build the solution:**
    ```bash
    dotnet build Jellyfin.Plugin.MediathekViewDL.sln
    ```

### Running and Testing

*   **Running the Plugin:** This is a plugin and must be run within a Jellyfin server instance. To install, place the published plugin files into the `plugins` directory of your Jellyfin installation and restart the server.
*   **Running Tests:** The solution includes a test project. Tests can be run using the .NET CLI.
    ```bash
    dotnet test Jellyfin.Plugin.MediathekViewDL.sln
    ```
*   **Release Builds:** Release builds and packaging are handled automatically by the GitHub Actions CI/CD workflow and should not be performed manually.

### Database Migrations

The project uses Entity Framework Core with SQLite. Because Jellyfin plugin dependencies must exclude runtime assets to avoid conflicts, a special build condition is required to run EF Core tools.

**To create a migration:**

You must set the `EnableEfDesign` environment variable to `true` when running `dotnet-ef`.

**PowerShell:**
```powershell
$env:EnableEfDesign="true"; dotnet tool run dotnet-ef migrations add <MigrationName> --project Jellyfin.Plugin.MediathekViewDL
```

**Bash:**
```bash
EnableEfDesign=true dotnet tool run dotnet-ef migrations add <MigrationName> --project Jellyfin.Plugin.MediathekViewDL
```

## Development Conventions

*   **File Structure:** Each class must be in its own separate file, except for classes that are nested within another class.
*   **Scope of Changes:** Strictly limit changes to the task at hand. Do not modify unrelated code, variable names, comments, or any other elements that are not directly required to fulfill the current request.
*   **Coding Style:** Always adhere to the existing coding style, formatting, and conventions prevalent in the project.
*   **Verification:** Before considering a task complete, I **must** run `dotnet build Jellyfin.Plugin.MediathekViewDL.sln`. The main plugin project (`Jellyfin.Plugin.MediathekViewDL`) treats warnings as errors, so the build must pass without any warnings to be considered successful.
*   **HTML (configPage.html) Conventions:** When editing `configPage.html`, **do not use string templates** like `` `S${season}E${Episode}` ``. Instead, use string concatenation like `'S'+season+'E'+Episode`. Jellyfin treats string templates as translation keys, which can lead to unexpected behavior.
*   **Coding Style:** The project uses standard C# and .NET conventions (e.g., `PascalCase` for classes, methods, and properties). It enforces a strict code style using analyzers like StyleCop (`StyleCop.Analyzers`).
*   **Records:** Use `record` types for DTOs and immutable types where suitable.
*   **Documentation:** All public members must have XML summary comments unless they inherit them.
*   **Nullability:** The project has nullable reference types enabled (`<Nullable>enable</Nullable>`), requiring explicit handling of `null` values.
*   **Environment:** Development and command execution happen exclusively on Windows. All shell commands must be compatible with PowerShell.
*   **Command Chaining:** When executing multiple commands in a single `run_shell_command` call, use the PowerShell separator `;`.
*   **Logging:** Structured logging is used via the `Microsoft.Extensions.Logging.ILogger` interface, which is provided by Jellyfin's runtime.
*   **Task Understanding:** If I am not sure I understand a task correctly, I must ask for clarification.
*   **Research:** For Jellyfin-specific topics, I should search the web or look at the code of other existing Jellyfin plugins for examples and best practices.