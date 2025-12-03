# Plan für das Jellyfin-Plugin "MediathekViewDL" (Finale Version 6)

## Phase 1: Projekt-Grundlagen und Konfiguration

1.  **Projektstruktur anpassen:**
    *   Umbenennen der `Jellyfin.Plugin.Template.*`-Dateien und -Projekte in `Jellyfin.Plugin.MediathekViewDL.*`.
    *   Anpassen der Projekt-GUIDs und Assembly-Informationen.

2.  **Konfigurationsmodell (`PluginConfiguration.cs`) erweitern:**
    *   `DefaultDownloadPath`: Ein Standard-Pfad.
    *   `Subscriptions`: Eine Liste von "Abo"-Objekten:
        *   `Id`: Eindeutige ID.
        *   `Name`: Name des Abos (wird als Serien-Ordnername verwendet).
        *   `SearchQuery`: Suchanfrage für die API.
        *   `DownloadPath`: Optionaler, spezifischer Download-Pfad für dieses Abo.
        *   `EnforceSeriesParsing`: Boolean für Staffel/Episode-Parsing.
        *   `AllowAudioDescription`: Boolean, ob "AD"-Versionen geladen werden.
    *   `LastRun`: Zeitstempel der letzten Ausführung.

## Phase 2: API-Kommunikation

1.  **Erstellen eines API-Clients (`MediathekViewApiClient.cs`):**
    *   Kapselt die Logik für die Kommunikation mit `mediathekviewweb.de/api/query`.
    *   Implementiert `SearchAsync(string query)`.
    *   Definiert Datenmodelle (`ApiSearchQuery`, `ApiSearchResult`) in `DataContracts.cs`.

## Phase 3: Benutzeroberfläche (UI)

1.  **Entwicklung der Konfigurationsseite (`configPage.html`):**
    *   Verwaltet die Abo-Liste clientseitig (JavaScript).
    *   **Tab 1: Einstellungen & Suche**:
        *   Eingabefeld für `DefaultDownloadPath`.
        *   Live-Suche in der Mediathek.
        *   Ergebnisliste mit "Einzel-Download"- und "Als Abo speichern"-Buttons.
    *   **Tab 2: Abonnements**:
        *   Anzeige aller Abos.
        *   Eingabefelder für Name, Suchquery, `DownloadPath`.
        *   Checkboxen für `EnforceSeriesParsing` und `AllowAudioDescription`.
        *   Button zum Löschen.

## Phase 4: Kernfunktionalität

1.  **Erstellen eines `FFmpegService.cs`:**
    *   Wrapper für `ffmpeg`-Befehle.
    *   Implementiert `ExtractAudioAsync(string tempVideoPath, string outputAudioPath, string languageCode)`.
    *   Korrigiert Sprach-Metadaten bei der Extraktion.

2.  **Implementierung der "Abo"-Logik als geplanter Task:**
    *   **Task-Ablauf:**
        1. API-Ergebnisse abrufen und nach Audiodeskription filtern.
        2. **Für jedes Video-Ergebnis:**
            a. Basis-Identität, Sprache, Staffel, Episode, Titel bestimmen.
            b. Zielpfad und Dateinamen gemäß Phase 5 generieren (inkl. Erstellen von Serien-/Staffel-Ordnern).
            c. **Logik basierend auf Sprache:**
                *   **Deutsch:** Wenn Master-Video (`.mkv`) nicht existiert -> Herunterladen.
                *   **Andere Sprache:** Wenn externe Audio-Datei (`.mka`) nicht existiert -> Video temporär laden, Audio mit `FFmpegService` extrahieren, temporäres Video löschen.
            d. **Untertitel-Download:** Passende `.srt`/.ttml-Datei herunterladen, falls nicht vorhanden.

## Phase 5: Integration und Finalisierung

1.  **Finale Dateistruktur:**
    *   Die vom Plugin erzeugte Struktur wird exakt dem Jellyfin-Standard für Serien entsprechen:
        ```
        {Abo.DownloadPath}/
        └── {Abo.Name}/
            └── Staffel {SS}/
                ├── S{SS}E{EE} - {Titel}.mkv
                ├── S{SS}E{EE} - {Titel}.eng.mka
                └── S{SS}E{EE} - {Titel}.de.srt
        ```
2.  **Jellyfin-Bibliothek aktualisieren:**
    *   Nach erfolgreichem Download einen Bibliotheks-Scan für den betroffenen Ordner auslösen.
