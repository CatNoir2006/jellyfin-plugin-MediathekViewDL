# MediathekViewWeb API Dokumentation

Dieses Dokument fasst die Struktur und Verwendung des MediathekViewWeb API-Endpunkts zusammen.

## Endpunkt

- **URL:** `https://mediathekviewweb.de/api/query`
- **Methode:** `POST`
- **Content-Type:** `application/json`

## Request Body

Der Body der Anfrage ist ein JSON-Objekt, das die Suche steuert.

```json
{
  "queries": [
    {
      "fields": ["title", "topic"],
      "query": "Suchbegriff"
    }
  ],
  "sortBy": "timestamp",
  "sortOrder": "desc",
  "future": false,
  "offset": 0,
  "size": 10,
  "duration_min": 1200,
  "duration_max": 6000
}
```

- `queries` (Array): Eine Liste von Suchfiltern.
  - `fields` (Array of Strings): Die zu durchsuchenden Felder. Gültige Werte sind `title`, `topic`, `channel`, `description`.
  - `query` (String): Der eigentliche Suchtext.
- `sortBy` (String): Das Feld für die Sortierung. Empfohlen: `timestamp`.
- `sortOrder` (String): Sortierrichtung. `desc` (absteigend, neuste zuerst) oder `asc` (aufsteigend).
- `future` (Boolean): `true`, um auch zukünftige Sendungen zu finden.
- `offset` (Integer): Startpunkt für die Paginierung (z.B. für Seite 2 bei `size: 10` wäre `offset: 10`).
- `size` (Integer): Anzahl der Ergebnisse, die zurückgegeben werden sollen.
- `duration_min` (Integer, Optional): Mindestdauer in Sekunden.
- `duration_max` (Integer, Optional): Maximaldauer in Sekunden.

## Response Body

Die Antwort ist ein JSON-Objekt, das die Suchergebnisse enthält.

```json
{
  "result": {
    "results": [
      {
        "channel": "ZDF",
        "topic": "Serienname",
        "title": "Episodentitel",
        "description": "Beschreibung der Sendung...",
        "timestamp": 1672531200,
        "duration": "2700",
        "size": 123456789,
        "url_website": "https://...",
        "url_subtitle": "https://.../sub.ttml",
        "url_video": "https://.../q4.mp4",
        "url_video_low": "https://.../q2.mp4",
        "url_video_hd": "https://.../q8.mp4",
        "id": "eindeutigeBase64Id=="
      }
    ],
    "queryInfo": {
      "filmlisteTimestamp" : 1765124760,
      "searchEngineTime" : "4.67",
      "resultCount" : 100,
      "totalResults" : 1,
      "totalRelation" : "eq",
      "totalEntries" : 689777
    }
  },
  "err": null
}
```

- `result.results` (Array): Die Liste der gefundenen Medienobjekte.
  - `channel` (String): Der Sender (z.B. "ORF", "ARD-alpha").
  - `topic` (String): Die Sendung oder Serie, zu der der Beitrag gehört (z.B. "Universum"). Nützlich zur Gruppierung.
  - `title` (String): Der vollständige Titel des Beitrags.
  - `description` (String): Die Beschreibung.
  - `timestamp` (Integer): Unix-Timestamp der Ausstrahlung.
  - `duration` (String/Integer): Dauer in Sekunden.
  - `url_website` (String): Link zur Webseite des Beitrags.
  - `url_subtitle` (String): Direkter Link zur Untertitel-Datei (oft im TTML-Format).
  - `url_video`, `url_video_low`, `url_video_hd` (String): Direkte Links zu den Videostreams in verschiedenen Qualitätsstufen. Oft `.m3u8` (HLS Playlist) oder `.mp4`.
  - `id` (String): Eine eindeutige ID für diesen Eintrag. Perfekt zum Verfolgen bereits heruntergeladener Dateien.
- `queryInfo`: Enthält Metadaten über die Suche (z.B. Gesamtzahl der Treffer).
- `err`: Ist `null`, wenn kein Fehler aufgetreten ist.

---

# Zapp API Dokumentation

Die Zapp API bietet Zugriff auf Live-TV-Streams und Informationen zum aktuellen Programm.

## Basis-URL
`https://api.zapp.mediathekview.de/v1/`

## Endpunkte

### 1. Senderliste und Streams

- **URL:** `channelInfoList`
- **Methode:** `GET`

Gibt eine Liste aller verfügbaren Sender mit ihren Stream-URLs (HLS) zurück.

**Response Body:**
```json
{
  "das_erste": {
    "streamUrl": "https://daserste-live.ard-mcdn.de/daserste/live/hls/de/master.m3u8",
    "name": "Das Erste"
  },
  "zdf": {
    "streamUrl": "https://zdf-hls-15.akamaized.net/hls/live/2016498/de/high/master.m3u8",
    "name": "ZDF"
  }
}
```

### 2. Aktuelle Sendungsinformationen

- **URL:** `shows/:channelId`
- **Methode:** `GET`

Gibt Informationen zur aktuell laufenden Sendung eines Senders zurück.

- `channelId`: Die ID des Senders aus der `channelInfoList` (z.B. `das_erste`, `zdf`).

**Response Body:**
```json
{
  "shows": [
    {
      "title": "Sendungstitel",
      "subtitle": "Untertitel",
      "description": "Beschreibung der Sendung...",
      "startTime": "2024-03-20T20:00:00.000+01:00",
      "endTime": "2024-03-20T20:15:00.000+01:00"
    }
  ]
}
```


---

# Zappel API Dokumentation
