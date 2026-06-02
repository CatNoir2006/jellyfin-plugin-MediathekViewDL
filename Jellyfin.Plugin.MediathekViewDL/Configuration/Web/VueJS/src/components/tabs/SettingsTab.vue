<script setup>
import { ref, computed, onMounted } from 'vue'
import ApiService from "../../utils/ApiService.js";

const Dashboard = window.Dashboard ?? null
const PLUGIN_ID = 'a31b415a-5264-419d-b152-8c8192a54994'
const PLUGIN_NAME = 'MediathekViewDL'

const emit = defineEmits(['config-saved'])

// --- State ---
const loading = ref(false)
const saving = ref(false)
const lastRun = ref(null)

// General
const activeWebUi = ref('VueJS')

// Paths
const useTopicForMoviePath = ref(false)
const defaultDownloadPath = ref('')
const subscriptionShowPath = ref('')
const subscriptionMoviePath = ref('')
const manualShowPath = ref('')
const manualMoviePath = ref('')
const tempDownloadPath = ref('')

// Download
const downloadSubtitles = ref(false)
const enableDirectAudioExtraction = ref(true)
const scanLibraryAfterDownload = ref(true)
const minFreeDiskSpaceMiB = ref('')
const maxBandwidthMBits = ref(0)

// Search
const fetchStreamSizes = ref(false)
const searchInFutureBroadcasts = ref(false)
const searchPageSize = ref(50)
const searchMaxPages = ref(5)

// Network
const allowUnknownDomains = ref(false)
const allowHttp = ref(false)

// Subscription Defaults - Search
const defMinDuration = ref('')
const defMaxDuration = ref('')

// Subscription Defaults - Download
const defUseStreamingUrlFiles = ref(false)
const defDownloadFullVideoSecondaryAudio = ref(false)
const defAlwaysCreateSubfolder = ref(false)
const defEnhancedDuplicateDetection = ref(false)
const defAllowFallbackToLowerQuality = ref(true)
const defQualityCheckWithUrl = ref(false)

// Subscription Defaults - Series
const defEnforceSeries = ref(false)
const defAllowAbsoluteEpisodeNumbering = ref(false)
const defTreatNonEpisodesAsExtras = ref(false)
const defSaveExtrasAsStrm = ref(false)
const defSaveTrailers = ref(true)
const defSaveInterviews = ref(true)
const defSaveGenericExtras = ref(true)

// Subscription Defaults - Metadata
const defOriginalLanguage = ref('')
const defCreateNfo = ref(false)
const defAppendDateToTitle = ref(false)
const defKeepOriginalTitle = ref(false)
const defAppendTimeToTitle = ref(false)

// Subscription Defaults - Accessibility
const defAllowAudioDesc = ref(false)
const defAllowSignLanguage = ref(false)

// Maintenance
const enableStrmCleanup = ref(false)
const allowDownloadOnUnknownDiskSpace = ref(false)

// Computed
const searchTotalItems = computed(() => {
  const ps = parseInt(searchPageSize.value) || 0
  const mp = parseInt(searchMaxPages.value) || 0
  return ps * mp
})

// --- API ---
async function loadConfig() {
  loading.value = true
  try {
    const config = await ApiService.getPluginConfig(PLUGIN_ID)

    activeWebUi.value = config.ActiveWebUi ?? 'VueJS'
    lastRun.value = config.LastRun ? new Date(config.LastRun).toLocaleString() : 'Noch nie'

    // Paths
    useTopicForMoviePath.value = config.Paths?.UseTopicForMoviePath ?? false
    defaultDownloadPath.value = config.Paths?.DefaultDownloadPath ?? ''
    subscriptionShowPath.value = config.Paths?.DefaultSubscriptionShowPath ?? ''
    subscriptionMoviePath.value = config.Paths?.DefaultSubscriptionMoviePath ?? ''
    manualShowPath.value = config.Paths?.DefaultManualShowPath ?? ''
    manualMoviePath.value = config.Paths?.DefaultManualMoviePath ?? ''
    tempDownloadPath.value = config.Paths?.TempDownloadPath ?? ''

    // Download
    downloadSubtitles.value = config.Download?.DownloadSubtitles ?? false
    enableDirectAudioExtraction.value = config.Download?.EnableDirectAudioExtraction ?? true
    scanLibraryAfterDownload.value = config.Download?.ScanLibraryAfterDownload ?? true
    minFreeDiskSpaceMiB.value = config.Download?.MinFreeDiskSpaceBytes
      ? (config.Download.MinFreeDiskSpaceBytes / (1024 * 1024)).toString()
      : ''
    maxBandwidthMBits.value = config.Download?.MaxBandwidthMBits ?? 0

    // Search
    fetchStreamSizes.value = config.Search?.FetchStreamSizes ?? false
    searchInFutureBroadcasts.value = config.Search?.SearchInFutureBroadcasts ?? false
    searchPageSize.value = config.Search?.PageSize ?? 50
    searchMaxPages.value = config.Search?.MaxPages ?? 5

    // Network
    allowUnknownDomains.value = config.Network?.AllowUnknownDomains ?? false
    allowHttp.value = config.Network?.AllowHttp ?? false

    // Maintenance
    enableStrmCleanup.value = config.Maintenance?.EnableStrmCleanup ?? false
    allowDownloadOnUnknownDiskSpace.value = config.Maintenance?.AllowDownloadOnUnknownDiskSpace ?? false

    // Subscription Defaults
    const def = config.SubscriptionDefaults ?? {}
    const defDl = def.DownloadSettings ?? {}
    const defSearch = def.SearchSettings ?? {}
    const defSeries = def.SeriesSettings ?? {}
    const defMeta = def.MetadataSettings ?? {}
    const defAccess = def.AccessibilitySettings ?? {}

    defMinDuration.value = defSearch.MinDurationMinutes ?? ''
    defMaxDuration.value = defSearch.MaxDurationMinutes ?? ''

    defUseStreamingUrlFiles.value = defDl.UseStreamingUrlFiles ?? false
    defDownloadFullVideoSecondaryAudio.value = defDl.DownloadFullVideoForSecondaryAudio ?? false
    defAlwaysCreateSubfolder.value = defDl.AlwaysCreateSubfolder ?? false
    defEnhancedDuplicateDetection.value = defDl.EnhancedDuplicateDetection ?? false
    defAllowFallbackToLowerQuality.value = defDl.AllowFallbackToLowerQuality !== undefined ? defDl.AllowFallbackToLowerQuality : true
    defQualityCheckWithUrl.value = defDl.QualityCheckWithUrl ?? false

    defEnforceSeries.value = defSeries.EnforceSeriesParsing ?? false
    defAllowAbsoluteEpisodeNumbering.value = defSeries.AllowAbsoluteEpisodeNumbering ?? false
    defTreatNonEpisodesAsExtras.value = defSeries.TreatNonEpisodesAsExtras ?? false
    defSaveExtrasAsStrm.value = defSeries.SaveExtrasAsStrm ?? false
    defSaveTrailers.value = defSeries.SaveTrailers !== undefined ? defSeries.SaveTrailers : true
    defSaveInterviews.value = defSeries.SaveInterviews !== undefined ? defSeries.SaveInterviews : true
    defSaveGenericExtras.value = defSeries.SaveGenericExtras !== undefined ? defSeries.SaveGenericExtras : true

    defOriginalLanguage.value = defMeta.OriginalLanguage ?? ''
    defCreateNfo.value = defMeta.CreateNfo ?? false
    defAppendDateToTitle.value = defMeta.AppendDateToTitle ?? false
    defKeepOriginalTitle.value = defMeta.KeepOriginalTitle ?? false
    defAppendTimeToTitle.value = defMeta.AppendTimeToTitle ?? false

    defAllowAudioDesc.value = defAccess.AllowAudioDescription ?? false
    defAllowSignLanguage.value = defAccess.AllowSignLanguage ?? false

  } catch (e) {
    console.error('Failed to load config', e)
  } finally {
    loading.value = false
  }
}

async function saveConfig() {
  saving.value = true
  try {
    const config = await ApiService.getPluginConfig(PLUGIN_ID)

    config.ActiveWebUi = activeWebUi.value

    // Paths
    if (!config.Paths) config.Paths = {}
    config.Paths.UseTopicForMoviePath = useTopicForMoviePath.value
    config.Paths.DefaultDownloadPath = defaultDownloadPath.value
    config.Paths.DefaultSubscriptionShowPath = subscriptionShowPath.value
    config.Paths.DefaultSubscriptionMoviePath = subscriptionMoviePath.value
    config.Paths.DefaultManualShowPath = manualShowPath.value
    config.Paths.DefaultManualMoviePath = manualMoviePath.value
    config.Paths.TempDownloadPath = tempDownloadPath.value

    // Download
    if (!config.Download) config.Download = {}
    config.Download.DownloadSubtitles = downloadSubtitles.value
    config.Download.EnableDirectAudioExtraction = enableDirectAudioExtraction.value
    config.Download.ScanLibraryAfterDownload = scanLibraryAfterDownload.value
    const minFree = parseInt(minFreeDiskSpaceMiB.value)
    config.Download.MinFreeDiskSpaceBytes = isNaN(minFree) ? (1.5 * 1024 * 1024 * 1024) : (minFree * 1024 * 1024)
    const maxBw = parseInt(maxBandwidthMBits.value)
    config.Download.MaxBandwidthMBits = isNaN(maxBw) ? 0 : maxBw

    // Search
    if (!config.Search) config.Search = {}
    config.Search.FetchStreamSizes = fetchStreamSizes.value
    config.Search.SearchInFutureBroadcasts = searchInFutureBroadcasts.value
    config.Search.PageSize = parseInt(searchPageSize.value) || 50
    config.Search.MaxPages = parseInt(searchMaxPages.value) || 5

    // Network
    if (!config.Network) config.Network = {}
    config.Network.AllowUnknownDomains = allowUnknownDomains.value
    config.Network.AllowHttp = allowHttp.value

    // Maintenance
    if (!config.Maintenance) config.Maintenance = {}
    config.Maintenance.EnableStrmCleanup = enableStrmCleanup.value
    config.Maintenance.AllowDownloadOnUnknownDiskSpace = allowDownloadOnUnknownDiskSpace.value

    // Subscription Defaults
    config.SubscriptionDefaults = {
      SearchSettings: {
        MinDurationMinutes: defMinDuration.value !== '' ? parseInt(defMinDuration.value) : null,
        MaxDurationMinutes: defMaxDuration.value !== '' ? parseInt(defMaxDuration.value) : null
      },
      DownloadSettings: {
        UseStreamingUrlFiles: defUseStreamingUrlFiles.value,
        DownloadFullVideoForSecondaryAudio: defDownloadFullVideoSecondaryAudio.value,
        AlwaysCreateSubfolder: defAlwaysCreateSubfolder.value,
        EnhancedDuplicateDetection: defEnhancedDuplicateDetection.value,
        AllowFallbackToLowerQuality: defAllowFallbackToLowerQuality.value,
        QualityCheckWithUrl: defQualityCheckWithUrl.value
      },
      SeriesSettings: {
        EnforceSeriesParsing: defEnforceSeries.value,
        AllowAbsoluteEpisodeNumbering: defAllowAbsoluteEpisodeNumbering.value,
        TreatNonEpisodesAsExtras: defTreatNonEpisodesAsExtras.value,
        SaveExtrasAsStrm: defSaveExtrasAsStrm.value,
        SaveTrailers: defSaveTrailers.value,
        SaveInterviews: defSaveInterviews.value,
        SaveGenericExtras: defSaveGenericExtras.value
      },
      MetadataSettings: {
        OriginalLanguage: defOriginalLanguage.value,
        CreateNfo: defCreateNfo.value,
        AppendDateToTitle: defAppendDateToTitle.value,
        KeepOriginalTitle: defKeepOriginalTitle.value,
        AppendTimeToTitle: defAppendTimeToTitle.value
      },
      AccessibilitySettings: {
        AllowAudioDescription: defAllowAudioDesc.value,
        AllowSignLanguage: defAllowSignLanguage.value
      }
    }

     await ApiService.updatePluginConfig(PLUGIN_ID, config)
     if (Dashboard) Dashboard.alert('Einstellungen gespeichert.')
     // Notify parent to refresh config
     emit('config-saved')
  } catch (e) {
    console.error('Failed to save config', e)
    if (Dashboard) Dashboard.alert('Fehler beim Speichern der Einstellungen.')
  } finally {
    saving.value = false
  }
}

async function copyConfig() {
  try {
    const config = await ApiService.getPluginConfig(PLUGIN_ID)
    const text = JSON.stringify(config, null, 2)
    if (window.isSecureContext) {
      await navigator.clipboard.writeText(text)
      if (Dashboard) Dashboard.alert('Konfiguration in die Zwischenablage kopiert.')
    } else {
      prompt('Bitte manuell kopieren:', text)
    }
  } catch (e) {
    console.error('Failed to copy config', e)
  }
}

function selectPath(targetRef, header) {
  if (!Dashboard) return
  try {
    const picker = new Dashboard.DirectoryBrowser()
    picker.show({
      header: header,
      includeDirectories: true,
      includeFiles: false,
      callback: (path) => {
        if (path) targetRef.value = path
        picker.close()
      }
    })
  } catch (e) {
    const newPath = prompt(header + '\nAktueller Pfad: ' + targetRef.value, targetRef.value)
    if (newPath !== null && newPath.trim() !== '') targetRef.value = newPath.trim()
  }
}

async function setupTuner() {
  try {
    if (Dashboard) Dashboard.showLoadingMsg()
    await ApiService.addTunerHost({ Type: 'zapp', Url: 'zapp', FriendlyName: 'Zapp (MediathekView)', TunerCount: 0 })
    if (Dashboard) { Dashboard.hideLoadingMsg(); Dashboard.alert('Zapp Tuner erfolgreich hinzugefügt.') }
  } catch (e) {
    if (Dashboard) Dashboard.hideLoadingMsg()
    console.error('Error adding tuner', e)
    if (Dashboard) Dashboard.alert('Fehler beim Hinzufügen des Tuners.')
  }
}

async function setupGuide() {
  try {
    if (Dashboard) Dashboard.showLoadingMsg()
    await ApiService.addListingProvider({ Type: 'zapp', Id: 'zapp_guide', Name: 'Zapp (MediathekView)' })
    if (Dashboard) { Dashboard.hideLoadingMsg(); Dashboard.alert('Zapp Guide Provider erfolgreich hinzugefügt.') }
  } catch (e) {
    if (Dashboard) Dashboard.hideLoadingMsg()
    console.error('Error adding guide', e)
    if (Dashboard) Dashboard.alert('Fehler beim Hinzufügen des Guide Providers.')
  }
}

onMounted(() => {
  loadConfig()
})
</script>

<template>
  <div class="card settings-card">
    <div v-if="loading" class="state-msg"><div class="spinner"></div> Einstellungen werden geladen...</div>

    <form v-else @submit.prevent="saveConfig">

      <!-- ===== ALLGEMEIN ===== -->
      <details open class="settings-section">
        <summary class="section-title">Allgemeine Einstellungen</summary>
        <div class="section-body">
          <div class="field">
            <label class="field-label" for="optActiveWebUi">Aktive WebUI</label>
            <select id="optActiveWebUi" v-model="activeWebUi" class="field-select">
              <option value="VueJS">VueJS</option>
              <option value="ShowBoth">Beide anzeigen</option>
              <option value="Html">HTML Legacy</option>
            </select>
          </div>
        </div>
      </details>

      <!-- ===== PFADE ===== -->
      <details class="settings-section">
        <summary class="section-title">Pfade-Einstellungen</summary>
        <div class="section-body">
          <div class="checkbox-field">
            <label>
              <input v-model="useTopicForMoviePath" type="checkbox"> Beim Film Downloads Ordner für das Thema erstellen
            </label>
            <p class="field-desc">
              Beim Film Downloads einen Ordner für das Thema (bzw. den Abo-Namen bei Abonnements) erstellen.<br>
              z.B. Wenn an: /Filme/Filme im Ersten/Filmname/Filmname.mkv<br>
              Wenn aus: /Filme/Filmname/Filmname.mkv
            </p>
          </div>

          <div class="field">
            <label class="field-label">Globaler Standard Download Pfad <span class="badge-deprecated">Veraltet</span></label>
            <div class="path-row">
              <input v-model="defaultDownloadPath" type="text" class="field-input" placeholder="Leer lassen für Standard">
              <button type="button" class="btn btn-secondary btn-sm" @click="selectPath(defaultDownloadPath, 'Globalen Standard Download Pfad wählen')" title="Ordner auswählen">📁</button>
            </div>
            <p class="field-desc">Der globale Fallback-Ordner für alle Downloads. <span style="color:#f87171;font-weight:bold;">Hinweis: Dieser Pfad ist veraltet und wird in einer zukünftigen Version entfernt.</span></p>
          </div>

          <div class="field">
            <label class="field-label">Standard Serien Pfad (Abo)</label>
            <div class="path-row">
              <input v-model="subscriptionShowPath" type="text" class="field-input" placeholder="Leer lassen für Standard">
              <button type="button" class="btn btn-secondary btn-sm" @click="selectPath(subscriptionShowPath, 'Standard Serien Pfad (Abo) wählen')" title="Ordner auswählen">📁</button>
            </div>
            <p class="field-desc">Standard-Ordner für Serien-Downloads in Abonnements.</p>
          </div>

          <div class="field">
            <label class="field-label">Standard Film Pfad (Abo)</label>
            <div class="path-row">
              <input v-model="subscriptionMoviePath" type="text" class="field-input" placeholder="Leer lassen für Standard">
              <button type="button" class="btn btn-secondary btn-sm" @click="selectPath(subscriptionMoviePath, 'Standard Film Pfad (Abo) wählen')" title="Ordner auswählen">📁</button>
            </div>
            <p class="field-desc">Standard-Ordner für Film-Downloads in Abonnements.</p>
          </div>

          <div class="field">
            <label class="field-label">Standard Serien Pfad (Manuell)</label>
            <div class="path-row">
              <input v-model="manualShowPath" type="text" class="field-input" placeholder="Leer lassen für Standard">
              <button type="button" class="btn btn-secondary btn-sm" @click="selectPath(manualShowPath, 'Standard Serien Pfad (Manuell) wählen')" title="Ordner auswählen">📁</button>
            </div>
            <p class="field-desc">Standard-Ordner für manuelle Serien-Downloads.</p>
          </div>

          <div class="field">
            <label class="field-label">Standard Film Pfad (Manuell)</label>
            <div class="path-row">
              <input v-model="manualMoviePath" type="text" class="field-input" placeholder="Leer lassen für Standard">
              <button type="button" class="btn btn-secondary btn-sm" @click="selectPath(manualMoviePath, 'Standard Film Pfad (Manuell) wählen')" title="Ordner auswählen">📁</button>
            </div>
            <p class="field-desc">Standard-Ordner für manuelle Film-Downloads.</p>
          </div>

          <div class="field">
            <label class="field-label">Temporärer Download Pfad</label>
            <div class="path-row">
              <input v-model="tempDownloadPath" type="text" class="field-input" placeholder="Leer lassen für direkten Download">
              <button type="button" class="btn btn-secondary btn-sm" @click="selectPath(tempDownloadPath, 'Temporären Download Pfad wählen')" title="Ordner auswählen">📁</button>
            </div>
            <p class="field-desc">Ein optionaler Ordner, in dem Downloads zwischengespeichert werden. Leer lassen, um direkt in den Zielordner herunterzuladen.</p>
          </div>
        </div>
      </details>

      <!-- ===== DOWNLOAD ===== -->
      <details class="settings-section">
        <summary class="section-title">Download-Einstellungen</summary>
        <div class="section-body">
          <div class="checkbox-field">
            <label><input v-model="downloadSubtitles" type="checkbox"> Untertitel herunterladen (wenn verfügbar)</label>
            <p class="field-desc">Lädt eine separate VTT- oder TTML-Datei für Untertitel herunter, falls vom Sender bereitgestellt.</p>
          </div>
          <div class="checkbox-field">
            <label><input v-model="enableDirectAudioExtraction" type="checkbox"> Direkte Audio-Extraktion aktivieren</label>
            <p class="field-desc">Wenn aktiviert, wird versucht, Audio direkt aus der URL zu extrahieren. Die Bandbreitenbegrenzung funktioniert hierbei nicht, aber es müssen generell weniger Daten heruntergeladen werden. (Standard: An)</p>
          </div>
          <div class="checkbox-field">
            <label><input v-model="scanLibraryAfterDownload" type="checkbox"> Bibliotheks-Scan nach Download</label>
            <p class="field-desc">Startet automatisch einen Scan der Medienbibliothek, nachdem neue Inhalte heruntergeladen wurden.</p>
          </div>
          <div class="grid-2">
            <div class="field">
              <label class="field-label">Mindestfreier Speicherplatz (MiB)</label>
              <input v-model="minFreeDiskSpaceMiB" type="number" class="field-input" placeholder="z.B. 1536">
              <p class="field-desc">Minimaler freier Speicherplatz um einen neuen Download zu starten.</p>
            </div>
            <div class="field">
              <label class="field-label">Maximale Download-Bandbreite (MBit/s)</label>
              <input v-model="maxBandwidthMBits" type="number" class="field-input" min="0" placeholder="0 = unbegrenzt">
              <p class="field-desc">Begrenzt die Download-Geschwindigkeit. 0 bedeutet unbegrenzt.</p>
            </div>
          </div>
        </div>
      </details>

      <!-- ===== SUCHE ===== -->
      <details class="settings-section">
        <summary class="section-title">Suche-Einstellungen</summary>
        <div class="section-body">
          <div class="checkbox-field">
            <label><input v-model="fetchStreamSizes" type="checkbox"> Größe des Streams abrufen</label>
            <p class="field-desc">Wenn aktiviert, wird die Größe der Video-Dateien bei der Suche abgerufen. Dies verlangsamt die Suche erheblich.</p>
          </div>
          <div class="checkbox-field">
            <label><input v-model="searchInFutureBroadcasts" type="checkbox"> Suche in zukünftigen Ausstrahlungen</label>
            <p class="field-desc">Manchmal sind Videos auch schon vor der eigentlichen TV-Ausstrahlung in der Mediathek verfügbar.</p>
          </div>
          <div class="grid-2">
            <div class="field">
              <label class="field-label">Seitengröße (API-Anfragen)</label>
              <input v-model="searchPageSize" type="number" class="field-input" min="1" max="100">
              <p class="field-desc">Wie viele Ergebnisse pro Seite von der API abgefragt werden.</p>
            </div>
            <div class="field">
              <label class="field-label">Maximale Seitenanzahl</label>
              <input v-model="searchMaxPages" type="number" class="field-input" min="1" max="100">
              <p class="field-desc">Maximale Anzahl an Seiten pro Suchanfrage / Abo-Lauf.</p>
            </div>
          </div>
          <div v-if="searchTotalItems > 0" class="info-msg">
            Aktuelle Konfiguration: Bis zu <strong>{{ searchTotalItems }}</strong> Medien können pro Suche/Abo-Lauf gefunden werden.
          </div>
        </div>
      </details>

      <!-- ===== NETZWERK & SICHERHEIT ===== -->
      <details class="settings-section">
        <summary class="section-title">Netzwerk &amp; Sicherheit</summary>
        <div class="section-body">
          <div class="checkbox-field">
            <label><input v-model="allowUnknownDomains" type="checkbox"> Downloads von unbekannten Domains erlauben</label>
            <p class="field-desc">Ermöglicht das Herunterladen von Inhalten von Domains, die nicht auf der Whitelist stehen. Dies kann nützlich sein, wenn ARD oder ZDF neue CDNs hinzufügen. <strong>Sicherheitsrisiko – mit Vorsicht verwenden.</strong></p>
          </div>
          <div class="checkbox-field">
            <label><input v-model="allowHttp" type="checkbox"> HTTP Downloads erlauben</label>
            <p class="field-desc">Dies kann notwendig sein, da manche URLs kein HTTPS unterstützen. Es wird empfohlen, dies deaktiviert zu lassen.</p>
          </div>
        </div>
      </details>

      <!-- ===== ABO-STANDARDWERTE ===== -->
      <details class="settings-section">
        <summary class="section-title">Abo-Standardwerte</summary>
        <div class="section-body">
          <p class="field-desc" style="margin-bottom:15px;">Diese Werte werden als Standard für neue Abonnements verwendet.</p>

          <div class="sub-section-title">Suche</div>
          <div class="grid-2">
            <div class="field">
              <label class="field-label">Min. Dauer (Minuten)</label>
              <input v-model="defMinDuration" type="number" class="field-input" placeholder="Kein Limit">
            </div>
            <div class="field">
              <label class="field-label">Max. Dauer (Minuten)</label>
              <input v-model="defMaxDuration" type="number" class="field-input" placeholder="Kein Limit">
            </div>
          </div>

          <div class="sub-section-title">Download</div>
          <div class="checkbox-field">
            <label><input v-model="defUseStreamingUrlFiles" type="checkbox"> Streaming-URL-Dateien (.strm) verwenden</label>
          </div>
          <div v-if="!defUseStreamingUrlFiles" class="sub-options">
            <div class="checkbox-field">
              <label><input v-model="defDownloadFullVideoSecondaryAudio" type="checkbox"> Vollständiges Video für sekundäre Audiosprachen</label>
            </div>
          </div>
          <div class="checkbox-field">
            <label><input v-model="defAlwaysCreateSubfolder" type="checkbox"> Unterordner für Abo erstellen</label>
          </div>
          <div class="checkbox-field">
            <label><input v-model="defEnhancedDuplicateDetection" type="checkbox"> Erweiterte Duplikaterkennung</label>
          </div>
          <div class="checkbox-field">
            <label><input v-model="defAllowFallbackToLowerQuality" type="checkbox"> Fallback auf niedrigere Qualität erlauben</label>
          </div>
          <div v-if="defAllowFallbackToLowerQuality" class="sub-options">
            <div class="checkbox-field">
              <label><input v-model="defQualityCheckWithUrl" type="checkbox"> URL-Gültigkeit prüfen</label>
            </div>
          </div>

          <div class="sub-section-title">Serien</div>
          <div class="checkbox-field">
            <label><input v-model="defEnforceSeries" type="checkbox"> Nur Serien herunterladen</label>
          </div>
          <div v-if="defEnforceSeries" class="sub-options">
            <div class="checkbox-field">
              <label><input v-model="defAllowAbsoluteEpisodeNumbering" type="checkbox"> Absolute Episodennummerierung erlauben</label>
            </div>
          </div>
          <div v-if="!defEnforceSeries">
            <div class="checkbox-field">
              <label><input v-model="defTreatNonEpisodesAsExtras" type="checkbox"> Nicht-Episoden als Extras behandeln</label>
            </div>
            <div v-if="defTreatNonEpisodesAsExtras" class="sub-options">
              <div class="checkbox-field">
                <label><input v-model="defSaveExtrasAsStrm" type="checkbox"> Extras als Stream (.strm) speichern</label>
              </div>
              <div class="checkbox-field">
                <label><input v-model="defSaveTrailers" type="checkbox"> Trailer speichern</label>
              </div>
              <div class="checkbox-field">
                <label><input v-model="defSaveInterviews" type="checkbox"> Interviews speichern</label>
              </div>
              <div class="checkbox-field">
                <label><input v-model="defSaveGenericExtras" type="checkbox"> Generische Extras speichern</label>
              </div>
            </div>
          </div>

          <div class="sub-section-title">Metadaten</div>
          <div class="field">
            <label class="field-label">Originalsprache (ISO Code, z.B. 'eng')</label>
            <input v-model="defOriginalLanguage" type="text" class="field-input" placeholder="z.B. eng oder fra">
          </div>
          <div class="checkbox-field">
            <label><input v-model="defCreateNfo" type="checkbox"> NFO Dateien erstellen</label>
          </div>
          <div class="checkbox-field">
            <label><input v-model="defAppendDateToTitle" type="checkbox"> Datum an Titel anhängen</label>
          </div>
          <div v-if="defAppendDateToTitle" class="sub-options">
            <div class="checkbox-field">
              <label><input v-model="defAppendTimeToTitle" type="checkbox"> Uhrzeit an Titel anhängen</label>
            </div>
          </div>
          <div class="checkbox-field">
            <label><input v-model="defKeepOriginalTitle" type="checkbox"> Originaltitel beibehalten</label>
          </div>

          <div class="sub-section-title">Barrierefreiheit</div>
          <div class="checkbox-field">
            <label><input v-model="defAllowAudioDesc" type="checkbox"> Audiodeskription erlauben</label>
          </div>
          <div class="checkbox-field">
            <label><input v-model="defAllowSignLanguage" type="checkbox"> Gebärdensprache erlauben</label>
          </div>
        </div>
      </details>

      <!-- ===== WARTUNG ===== -->
      <details class="settings-section">
        <summary class="section-title">Wartung</summary>
        <div class="section-body">
          <div class="checkbox-field">
            <label><input v-model="enableStrmCleanup" type="checkbox"> Bereinigung ungültiger Streaming-Dateien (.strm) aktivieren</label>
            <p class="field-desc">Überprüft regelmäßig alle erstellten .strm Dateien auf Gültigkeit der Links. Wenn ein Link nicht mehr erreichbar ist (z.B. 404), wird die Datei gelöscht.</p>
          </div>
          <div class="checkbox-field">
            <label><input v-model="allowDownloadOnUnknownDiskSpace" type="checkbox"> Download bei unbekanntem Speicherplatz erlauben</label>
            <p class="field-desc">Ermöglicht den Download auch dann, wenn der verfügbare Speicherplatz nicht ermittelt werden kann (z.B. bei manchen Netzwerkfreigaben).</p>
          </div>
        </div>
      </details>

      <!-- ===== LIVE TV ===== -->
      <details class="settings-section">
        <summary class="section-title">Live TV Integration</summary>
        <div class="section-body">
          <p class="field-desc">Hier können Sie die Live TV Integration einrichten. Da Jellyfin Plugins nicht automatisch als Guide-Provider anzeigt, können Sie diese hier manuell hinzufügen.</p>
          <div class="btn-row">
            <button type="button" class="btn btn-secondary" @click="setupTuner">Zapp Tuner hinzufügen</button>
            <button type="button" class="btn btn-secondary" @click="setupGuide">Zapp Guide Provider hinzufügen</button>
          </div>
          <p class="field-desc" style="margin-top:10px;"><strong>Hinweis:</strong> Nach dem Hinzufügen müssen Sie möglicherweise die Seite neu laden oder den Guide-Refresh Task in Jellyfin starten, damit die Daten angezeigt werden.</p>
        </div>
      </details>

      <!-- ===== LETZTER LAUF + BUTTONS ===== -->
      <div class="footer-row">
        <div class="last-run">
          <span class="field-label">Letzter Lauf:</span>
          <span>{{ lastRun ?? 'Noch nie' }}</span>
        </div>
        <div class="action-row">
          <button type="button" class="btn btn-secondary btn-sm" @click="copyConfig" title="Konfiguration kopieren">📋 Kopieren</button>
          <button type="submit" class="btn btn-save" :disabled="saving">
            {{ saving ? 'Speichert...' : 'Speichern' }}
          </button>
        </div>
      </div>

    </form>
  </div>
</template>

<style scoped>
.settings-card {
  padding: 0;
}

.settings-section {
  border-bottom: 1px solid #3f3f46;
}

.settings-section:last-of-type {
  border-bottom: none;
}

.section-title {
  list-style: none;
  padding: 16px 20px;
  font-weight: 600;
  font-size: 1rem;
  color: #e4e4e7;
  cursor: pointer;
  user-select: none;
  display: flex;
  align-items: center;
  gap: 8px;
}

.section-title::before {
  content: '▶';
  font-size: 0.7em;
  color: #7c3aed;
  transition: transform 0.2s;
}

details[open] > .section-title::before {
  transform: rotate(90deg);
}

.section-title::-webkit-details-marker {
  display: none;
}

.section-body {
  padding: 10px 20px 20px 20px;
  background: #1c1c1f;
  border-top: 1px solid #3f3f46;
}

.grid-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 15px;
}

@media (max-width: 600px) {
  .grid-2 { grid-template-columns: 1fr; }
}

.path-row {
  display: flex;
  gap: 8px;
  align-items: center;
}

.path-row .field-input {
  flex: 1;
}

.sub-options {
  margin-left: 25px;
  border-left: 2px solid #3f3f46;
  padding-left: 15px;
  margin-top: 5px;
  margin-bottom: 5px;
}

.sub-section-title {
  font-size: 0.9rem;
  font-weight: 700;
  color: #a1a1aa;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin: 20px 0 10px;
  padding-bottom: 5px;
  border-bottom: 1px solid #3f3f46;
}

.badge-deprecated {
  background: #7f1d1d;
  color: #fca5a5;
  font-size: 0.7rem;
  padding: 2px 6px;
  border-radius: 4px;
  margin-left: 8px;
  font-weight: 600;
}

.info-msg {
  background: #1e3a5f;
  border: 1px solid #2563eb;
  color: #93c5fd;
  border-radius: 6px;
  padding: 10px 14px;
  font-size: 0.875rem;
  margin-top: 10px;
}

.btn-row {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
  margin-top: 10px;
}

.footer-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  background: #18181b;
  border-top: 1px solid #3f3f46;
  flex-wrap: wrap;
  gap: 10px;
}

.last-run {
  display: flex;
  gap: 8px;
  align-items: center;
  font-size: 0.875rem;
  color: #a1a1aa;
}

.action-row {
  display: flex;
  gap: 10px;
  align-items: center;
}

.state-msg {
  text-align: center;
  padding: 40px;
  color: #a1a1aa;
}
</style>
