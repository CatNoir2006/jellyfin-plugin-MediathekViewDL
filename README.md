# Jellyfin MediathekViewWeb Downloader Plugin

Ein Plugin für Jellyfin, das Inhalte aus den öffentlich-rechtlichen Mediatheken sucht und herunterlädt. Es verwendet im Hintergrund die [MediathekViewWeb-API](https://mediathekviewweb.de/) um Sendungen zu finden und lädt sie von ARD/ZDF herunter.

## Features

- **Abonnements**: Erstellen Sie Abonnements, um Sendungen automatisch herunterzuladen, sobald sie verfügbar sind.
- **Geplante Downloads**: Ein Hintergrund-Task prüft regelmäßig auf neue Inhalte für Ihre Abonnements.
- **Flexible Filter**: Filtern Sie Suchergebnisse nach Dauer, um nur Inhalte der gewünschten Länge zu laden.
- **Umgang mit Inhalten**:
  - Konfigurierbare Behandlung von Inhalten, die nicht als Serien-Episode erkannt werden (z.B. als "Extras" speichern).
  - Erzwingen Sie eine strikte Serien/Episoden-Analyse, um nur korrekt benannte Dateien zu laden.
  - Unterstützung für absolute Episodennummern.
- **Barrierefreiheit**: Wahlweises Herunterladen von Versionen mit Audiodeskription oder Gebärdensprache.
- **Untertitel**: Automatischer Download von verfügbaren Untertiteln.
- **Streaming-Unterstützung**: Anstatt die Videodatei herunterzuladen, kann das Plugin `.strm`-Dateien erstellen, die direkt auf die Streaming-URL der Mediathek verweisen.
- **Anpassbare Speicherorte**: Legen Sie einen globalen Download-Pfad fest und überschreiben Sie diesen bei Bedarf für einzelne Abonnements.
- **Sicherheit**:
  - Eine Prüfung des freien Speicherplatzes verhindert, dass die Festplatte vollläuft.
  - Downloads sind auf eine Liste bekannter und vertrauenswürdiger Domains (CDNs von ARD, ZDF, etc.) beschränkt, um die Sicherheit zu erhöhen.
## Installation
### Normal

- Für die Installation muss mein Jellyfin Plugin Repository zu Jellyfin hinzugefügt werden:
- In Jellyfin: Administration -> Plugins -> Repositories verwalten -> Neues Repository
- Es kann dan ein Belibiger Name angebeben werden un folgende URL:
    ```
    https://raw.githubusercontent.com/CatNoir2006/jellyfin-plugin-manifest/main/    manifest.json
    ```

### Manuell - Selber Bauen

1.  **Repository klonen**:
    ```bash
    git clone https://github.com/CatNoir2006/jellyfin-plugin-MediathekViewDL.git
    cd jellyfin-plugin-MediathekViewDL
    ```

2.  **Plugin bauen**:
    Verwenden Sie das .NET SDK, um das Plugin zu veröffentlichen. Der folgende Befehl erstellt ein `dist`-Verzeichnis mit den notwendigen Dateien.
    ```bash
    dotnet publish -c Release -o ./dist
    ```

3.  **Plugin in Jellyfin kopieren**:
    - Erstellen Sie einen Ordner für das Plugin im `plugins`-Verzeichnis Ihrer Jellyfin-Installation (z.B. `.../plugins/MediathekViewDL`).
    - Kopieren Sie **alle Dateien** aus dem `dist`-Verzeichnis in den neu erstellten Plugin-Ordner.

4.  **Jellyfin neustarten**:
    Starten Sie den Jellyfin-Server neu, damit das Plugin geladen wird.

## Konfiguration

Die Konfiguration erfolgt über die Plugin-Seite im Jellyfin-Dashboard. Die Benutzeroberfläche ist in drei Tabs unterteilt:

- **Manuelle Suche**: Hier können Sie die Mediathek direkt durchsuchen, um einzelne Sendungen zu finden und herunterzuladen oder um die Suchkriterien für ein neues Abonnement zu testen.
    ![](/Images/ManuelleSuche.png)
    ![](/Images/ManuellerDownloadErweitert.png)
- **Einstellungen**: Dient zur Konfiguration der globalen Plugin-Einstellungen.
  ![](/Images/Einstellungen.png)
- **Abo Verwaltung**: Hier verwalten Sie Ihre Abonnements. Sie können neue erstellen, bestehende bearbeiten oder löschen.
  ![](/Images/Abos.png)
  ![](/Images/AbosBearbeiten.png)

### Allgemeine Einstellungen (Tab: Einstellungen)

- **Standard-Download-Pfad**: Der globale Ordner, in dem heruntergeladene Dateien gespeichert werden. Wenn ein Abonnement einen eigenen Pfad hat, wird dieser bevorzugt.
- **Untertitel herunterladen**: Wenn aktiviert, werden Untertitel (falls verfügbar) zusammen mit dem Video heruntergeladen.
- **Minimaler freier Speicherplatz**: Legt fest, wie viel Gigabyte auf dem Laufwerk frei sein müssen, bevor ein neuer Download gestartet wird. Dies verhindert, dass das Laufwerk vollläuft.
- **Downloads von unbekannten Domains erlauben**: Standardmäßig lässt das Plugin nur Downloads von einer vordefinierten Liste von Domains (z.B. `ard.de`, `zdf.de`) zu. Aktivieren Sie diese Option auf eigene Gefahr, wenn Sie Probleme mit neuen oder unbekannten CDNs haben.

### Abonnements (Tab: Abo Verwaltung)

Das Herzstück des Plugins. Ein Abonnement ist eine gespeicherte Suche, die regelmäßig ausgeführt wird.

1.  **Hinzufügen eines Abonnements**: Klicken Sie auf "Neues Abo".
2.  **Name**: Geben Sie einen Namen für das Abonnement ein. Dieser Name wird als **Ordnername für die Serie** innerhalb des Download-Pfads verwendet.
3.  **Abfrage (Query)**:
    - Definieren Sie eine oder mehrere Suchanfragen.
    - Eine Anfrage besteht aus einem Suchbegriff (`Query`) und den zu durchsuchenden Feldern (`Fields`: Titel, Thema, Sender).
    - Beispiel: Um nach "Tatort" zu suchen, erstellen Sie eine Abfrage mit dem `Query`-Text `Tatort` und dem Feld `Titel`.
4.  **Download-Pfad**: Optional. Legen Sie hier einen spezifischen Ordner für dieses Abonnement fest, um den Standard-Pfad zu überschreiben.
5.  **Optionen**:
    - **Aktiviert**: Schalten Sie dieses Abonnement ein oder aus.
    - **Nicht-Episoden als Extras behandeln**: Wenn eine Sendung nicht als Episode erkannt wird, wird sie in einem "Extras"-Ordner gespeichert.
    - **Serien-Analyse erzwingen**: Lädt nur Inhalte herunter, bei denen Staffel und Episode aus dem Titel erkannt werden können.
    - **Absolute Episodennummerierung erlauben**: Erlaubt Formate wie `Episode 123` anstelle von `S01E01`.
    - **Audiodeskription/Gebärdensprache erlauben**: Lädt auch Versionen mit diesen Merkmalen herunter.
    - **Dauer (Min/Max)**: Filtern Sie Ergebnisse nach ihrer Länge in Minuten.
    - **Streaming-URL-Dateien (.strm) verwenden**: Anstatt das Video herunterzuladen, wird eine kleine Textdatei mit der URL zum Stream erstellt.

## Funktionsweise

Das Plugin fügt eine geplante Aufgabe zu Jellyfin hinzu ("Download-Task für Mediathek-Inhalte"), die alle paar Stunden ausgeführt wird. Bei jeder Ausführung durchläuft der Task alle **aktivierten** Abonnements, sendet die definierten Suchanfragen an die MediathekViewWeb-API und lädt alle neuen, noch nicht vorhandenen Inhalte herunter, die den Filterkriterien entsprechen.

## Danksagung

Ein besonderer Dank geht an das Team von [MediathekViewWeb.de](https://mediathekviewweb.de/) für die Bereitstellung und Pflege der API, die dieses Plugin erst möglich macht.

## Disclaimer

Dieses Plugin wurde entwickelt, um den Zugriff auf öffentlich verfügbare Inhalte zu automatisieren. Stellen Sie sicher, dass Sie die Nutzungsbedingungen der jeweiligen Mediatheken einhalten. Die Nutzung erfolgt auf eigene Gefahr.
