# Jellyfin MediathekViewWeb Downloader Plugin

Ein Plugin für Jellyfin, das Inhalte aus den öffentlich-rechtlichen Mediatheken sucht und herunterlädt. Es verwendet im Hintergrund die [MediathekViewWeb-API](https://mediathekviewweb.de/) um Sendungen zu finden und lädt sie von ARD/ZDF herunter.

## Features

- **Abonnements**: Erstellen Sie Abonnements, um Sendungen automatisch herunterzuladen, sobald sie verfügbar sind.
- **Download-Manager (Neu)**:
  - **Zentrale Übersicht**: Überwachen Sie aktive Downloads im neuen "Downloads"-Tab.
  - **Kontrolle**: Laufende Downloads können jederzeit abgebrochen werden.
  - **Historie**: Einsehen vergangener Downloads und deren Status.
  - **Warteschlange**: Intelligente Verwaltung aller Downloads (Manuell & Automatisch) in einer sequenziellen Warteschlange.
- **Qualitäts-Management**:
  - **Auto-Upgrade**: Ersetzt automatisch vorhandene Dateien, wenn eine bessere Qualität verfügbar wird.
  - **Smart Fallback**: Greift automatisch auf eine niedrigere Qualität zurück, falls die HD-Version nicht verfügbar oder der Link defekt ist.
  - **URL-Check**: Optionale Prüfung der Verfügbarkeit von Download-URLs vor dem Start.
- **Metadaten & Organisation**:
  - **NFO-Erstellung**: Generiert optional `.nfo` Dateien für bessere Metadaten (Titel, Beschreibung, Ausstrahlung) in Jellyfin/Kodi.
  - **Granulare Extras-Steuerung**: Entscheiden Sie separat, ob Trailer, Interviews oder sonstige Extras gespeichert werden sollen (und ob als `.strm`).
  - **Datenbank-Backend**: Verwendet eine lokale SQLite-Datenbank (`mediathek-dl.db`) für eine zuverlässige Historienverwaltung und vermeidet doppelte Downloads.
- **Barrierefreiheit**: Wahlweises Herunterladen von Versionen mit Audiodeskription oder Gebärdensprache.
- **Untertitel**: Automatischer Download von verfügbaren Untertiteln.
- **Streaming-Unterstützung**: Anstatt die Videodatei herunterzuladen, kann das Plugin `.strm`-Dateien erstellen, die direkt auf die Streaming-URL der Mediathek verweisen.
- **Bandbreitenbegrenzung**: Begrenzen Sie die Download-Geschwindigkeit global, um Ihr Netzwerk nicht zu blockieren.
- **Sicherheit**:
  - Eine Prüfung des freien Speicherplatzes verhindert, dass die Festplatte vollläuft.
  - Downloads sind auf eine Liste bekannter und vertrauenswürdiger Domains (CDNs von ARD, ZDF, etc.) beschränkt.

## Installation

### Normal (Plugin Repository)

1.  Fügen Sie das Plugin-Repository zu Jellyfin hinzu:
    - In Jellyfin: **Administration** -> **Plugins** -> **Repositories verwalten** -> **Neues Repository**.
    - Geben Sie einen beliebigen Namen ein und verwenden Sie folgende URL:
      ```
      https://raw.githubusercontent.com/CatNoir2006/jellyfin-plugin-manifest/main/manifest.json
      ```
2.  Installieren Sie das Plugin über den Katalog.

### Manuell (Selber Bauen)

1.  **Repository klonen**:
    ```bash
    git clone https://github.com/CatNoir2006/jellyfin-plugin-MediathekViewDL.git
    cd jellyfin-plugin-MediathekViewDL
    ```

2.  **Plugin bauen**:
    Verwenden Sie das .NET SDK, um das Plugin zu bauen.
    ```bash
    dotnet build
    ```

3.  **Plugin in Jellyfin kopieren**:
    - Erstellen Sie einen Ordner für das Plugin im `plugins`-Verzeichnis Ihrer Jellyfin-Installation (z.B. `.../plugins/MediathekViewDL`).
    - Kopieren Sie **alle Dateien** aus dem `bin/Debug/net9.0`-Verzeichnis in den neu erstellten Plugin-Ordner.

4.  **Jellyfin neustarten**:
    Starten Sie den Jellyfin-Server neu, damit das Plugin geladen wird.

## Konfiguration

Die Konfiguration erfolgt über die Plugin-Seite im Jellyfin-Dashboard. Das Plugin ist auch direkt über das Hauptmenü erreichbar.

### Manuelle Suche (Tab: Manuelle Suche)

Hier können Sie die gesamte Mediathek durchsuchen, um gezielt einzelne Sendungen zu finden oder Suchanfragen für Abos zu testen.
![](/Images/ManuelleSuche.png)

*   **Suchen**: Geben Sie Titel, Thema oder Sender ein und filtern Sie die Ergebnisse.
*   **In Abo übernehmen**: Erstellt aus der aktuellen Suchanfrage und den gewählten Filtern direkt ein neues Abonnement. Dies ist der einfachste Weg, komplexe Abos zu erstellen.
*   **Download**: Startet sofort den Download der ausgewählten Sendung.
*   **Erweiterter Download**:
    ![](/Images/ManuellerDownloadErweitert.png)
    Über erweiterten Dialog können Sie explizit wählen:
    - **Download Pfad**: Wählen Sie einen spezifischen Zielordner für diesen Download.
    - **Dateiname**: Passen Sie den Dateinamen manuell an.
    - **Untertitel**: Entscheiden Sie, ob Untertitel mit heruntergeladen werden sollen.

### Allgemeine Einstellungen (Tab: Einstellungen)

- **Standard-Download-Pfad**: Der globale Ordner, in dem heruntergeladene Dateien gespeichert werden.
- **Untertitel herunterladen**: Lädt Untertitel (falls verfügbar) herunter.
- **Minimaler freier Speicherplatz**: Verhindert Downloads, wenn der Speicherplatz knapp wird (Standard: 1.5 GB).
- **Maximale Bandbreite (MBit/s)**: Begrenzt die Download-Geschwindigkeit (0 = unbegrenzt).
- **Bibliothek nach Download scannen**: Startet automatisch einen Scan der Medienbibliothek, sobald alle Downloads in der Warteschlange abgeschlossen sind.
- **Bereinigung von .strm Dateien**: Aktiviert einen Wartungstask, der nicht mehr funktionierende Streaming-Links löscht.
- **Downloads von unbekannten Domains erlauben**: Erlaubt Downloads von Quellen, die nicht in der internen Whitelist stehen (auf eigene Gefahr).

### Abonnements (Tab: Abo Verwaltung)

Hier definieren Sie Suchaufträge ("Abos"), die regelmäßig ausgeführt werden.

**Wichtige Abo-Optionen:**
- **Name**: Bestimmt den Unterordner für die Serie.
- **Download-Pfad**: Überschreibt den globalen Pfad für dieses Abo.
- **Optionen**:
    - **Nicht-Episoden als Extras behandeln**: Speichert Inhalte ohne erkennbare S/E-Nummer separat.
      - *Unteroptionen*: Wählen Sie spezifisch, ob Trailer, Interviews oder sonstige Extras geladen werden sollen.
    - **Serien-Analyse erzwingen**: Lädt nur Inhalte mit erkennbarer Staffel/Episode.
    - **Metadaten (.nfo) erstellen**: Generiert NFO-Dateien für bessere Metadaten in Jellyfin.
    - **Verbesserte Duplikat-Erkennung**: Prüft vor dem Download im Zielordner, ob die Datei bereits existiert (auch unter anderem Namen).
    - **Qualitäts-Upgrade**: Ersetzt eine existierende Datei, wenn eine Version mit höherer Auflösung gefunden wird.
    - **URL-Check vor Download**: Prüft, ob der Videolink tatsächlich erreichbar ist (kostet Zeit, erhöht aber Zuverlässigkeit).
    - **Streaming-URL-Dateien (.strm) verwenden**: Speichert nur Verknüpfungen statt Videodateien.

## Funktionsweise

Das Plugin registriert einen "Scheduled Task" in Jellyfin. Dieser läuft periodisch (oder manuell angestoßen) und durchsucht die MediathekViewWeb-API nach neuen Treffern. Gefundene Inhalte werden mit der lokalen Historie (SQLite-Datenbank) abgeglichen. Neue Inhalte wandern in den zentralen `DownloadQueueManager`, der sie sequenziell abarbeitet, um Lastspitzen zu vermeiden.

## Danksagung

Ein besonderer Dank geht an das Team von [MediathekViewWeb.de](https://mediathekviewweb.de/) für die Bereitstellung und Pflege der API.

## Disclaimer

Dieses Plugin wurde entwickelt, um den Zugriff auf öffentlich verfügbare Inhalte zu automatisieren. Stellen Sie sicher, dass Sie die Nutzungsbedingungen der jeweiligen Mediatheken einhalten. Die Nutzung erfolgt auf eigene Gefahr.
