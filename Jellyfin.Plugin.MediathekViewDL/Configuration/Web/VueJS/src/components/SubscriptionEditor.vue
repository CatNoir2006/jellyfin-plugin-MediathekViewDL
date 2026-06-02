<script setup>
import {ref, watch} from 'vue'
import { MS_PER_DAY_MINUS_ONE } from '../utils/Constants'

const props = defineProps({
    subscription: {
        type: Object,
        default: null
    }
})

const emit = defineEmits(['save', 'cancel'])

const editedSub = ref(null)
const activeTab = ref('basic')

const Dashboard = window.Dashboard ?? null

watch(() => props.subscription, (newVal) => {
    if (newVal) {
        // Deep copy
        const copy = JSON.parse(JSON.stringify(newVal))
        // Ensure nested objects are initialized to prevent template crashes
        copy.Search = copy.Search || {}
        copy.Search.Criteria = copy.Search.Criteria || []
        copy.Download = copy.Download || {}
        copy.Series = copy.Series || {}
        copy.Metadata = copy.Metadata || {}
        copy.Accessibility = copy.Accessibility || {}
        editedSub.value = copy
        // Reset active tab when a new subscription is opened
        activeTab.value = 'basic'
    } else {
        editedSub.value = null
    }
}, {immediate: true, deep: true})

function addQuery() {
    editedSub.value.Search.Criteria.push({
        Fields: ['Title', 'Topic'],
        Query: '',
        IsExclude: false
    })
}

function removeQuery(index) {
    editedSub.value.Search.Criteria.splice(index, 1)
}

function toggleField(query, field) {
    const index = query.Fields.indexOf(field)
    if (index > -1) {
        if (query.Fields.length > 1) {
            query.Fields.splice(index, 1)
        }
    } else {
        query.Fields.push(field)
    }
}

async function save() {
    emit('save', editedSub.value)
}

function cancel() {
    emit('cancel')
}

function selectPath() {
    if (!Dashboard) return
    const picker = new Dashboard.DirectoryBrowser()
    picker.show({
        header: 'Abo Pfad wählen',
        includeDirectories: true,
        includeFiles: false,
        callback: (path) => {
            if (path) {
                editedSub.value.Download.DownloadPath = path
            }
            picker.close()
        }
    })
}

// Utility to format date for input[type=date]
function formatDate(dateStr) {
    if (!dateStr) return ''
    return dateStr.split('T')[0]
}

function updateDate(target, field, value) {
    if (!value) {
        target[field] = null
        return
    }
    let date = new Date(value)
    if (field === 'MaxBroadcastDate') {
        date = new Date(date.getTime() + MS_PER_DAY_MINUS_ONE)
    }
    target[field] = date.toISOString()
}
</script>

<template>
    <div v-if="editedSub" class="editor-overlay">
        <div class="editor-modal card">
            <header class="editor-header">
                <h2>{{ editedSub.Id ? 'Abonnement bearbeiten' : 'Neues Abonnement' }}</h2>
                <div class="header-actions">
                    <button @click="cancel" class="btn-icon">✕</button>
                </div>
            </header>

            <div class="editor-tabs">
                <button class="tab-btn" :class="{ active: activeTab === 'basic' }" @click="activeTab = 'basic'">Allgemein</button>
                <button class="tab-btn" :class="{ active: activeTab === 'search' }" @click="activeTab = 'search'">Suche</button>
                <button class="tab-btn" :class="{ active: activeTab === 'download' }" @click="activeTab = 'download'">Download</button>
                <button class="tab-btn" :class="{ active: activeTab === 'series' }" @click="activeTab = 'series'">Serien</button>
                <button class="tab-btn" :class="{ active: activeTab === 'metadata' }" @click="activeTab = 'metadata'">Metadaten</button>
                <button class="tab-btn" :class="{ active: activeTab === 'accessibility' }" @click="activeTab = 'accessibility'">Barrierefreiheit</button>
            </div>

            <div class="editor-content">
                <!-- Allgemein Tab -->
                <div v-if="activeTab === 'basic'" class="tab-pane">
                    <div class="field">
                        <label>Name (Serienname)</label>
                        <input v-model="editedSub.Name" type="text" class="field-input" placeholder="z.B. Tatort" required>
                    </div>
                    <div class="checkbox-field">
                        <label>
                            <input v-model="editedSub.IsEnabled" type="checkbox"> Aktiviert
                        </label>
                    </div>

                    <div class="checkbox-field" hidden>
                        <label>
                            <input v-model="editedSub.IgnoreLocalFiles" type="checkbox"> Lokale Dateien ignorieren
                        </label>
                        <p class="field-desc">Erzwingt den Download, auch wenn die Datei bereits lokal existiert.</p>
                    </div>
                    <div class="checkbox-field" hidden>
                        <label>
                            <input v-model="editedSub.IgnoreHistory" type="checkbox"> Download-Verlauf ignorieren
                        </label>
                        <p class="field-desc">Erzwingt den Download, auch wenn die Sendung bereits früher geladen wurde.</p>
                    </div>
                </div>

                <!-- Suche Tab -->
                <div v-if="activeTab === 'search'" class="tab-pane">
                    <h3>Suchanfragen</h3>
                    <div v-for="(query, idx) in editedSub.Search.Criteria" :key="idx" class="query-row">
                        <div class="query-fields">
                            <button
                                v-for="f in ['Title', 'Topic', 'Description', 'Channel']"
                                :key="f"
                                @click="toggleField(query, f)"
                                class="field-tag"
                                :class="{ active: query.Fields.includes(f) }"
                            >
                                {{ f === 'Title' ? 'Titel' : f === 'Topic' ? 'Thema' : f === 'Description' ? 'Beschreibung' : 'Sender' }}
                            </button>
                        </div>
                        <div class="query-input-row">
                            <input v-model="query.Query" type="text" class="field-input" :placeholder="query.IsExclude ? 'Ausschließen...' : 'Suchen...'">
                            <button @click="query.IsExclude = !query.IsExclude" class="btn-small" :class="{ 'btn-danger': query.IsExclude }">
                                {{ query.IsExclude ? 'NICHT' : 'SUCHE' }}
                            </button>
                            <button @click="removeQuery(idx)" class="btn-icon">🗑️</button>
                        </div>
                    </div>
                    <button @click="addQuery" class="btn btn-secondary">Anfrage hinzufügen</button>

                    <hr>
                    <div class="grid-2">
                        <div class="field">
                            <label>Min. Dauer (Minuten)</label>
                            <input v-model="editedSub.Search.MinDurationMinutes" type="number" class="field-input">
                        </div>
                        <div class="field">
                            <label>Max. Dauer (Minuten)</label>
                            <input v-model="editedSub.Search.MaxDurationMinutes" type="number" class="field-input">
                        </div>
                    </div>
                    <div class="grid-2">
                        <div class="field">
                            <label>Min. Sendedatum</label>
                            <input :value="formatDate(editedSub.Search.MinBroadcastDate)" @input="updateDate(editedSub.Search, 'MinBroadcastDate', $event.target.value)" type="date" class="field-input">
                        </div>
                        <div class="field">
                            <label>Max. Sendedatum</label>
                            <input :value="formatDate(editedSub.Search.MaxBroadcastDate)" @input="updateDate(editedSub.Search, 'MaxBroadcastDate', $event.target.value)" type="date" class="field-input">
                        </div>
                    </div>
                </div>

                <!-- Download Tab -->
                <div v-if="activeTab === 'download'" class="tab-pane">
                    <div class="field">
                        <label>Download Pfad (Optional)</label>
                        <div class="input-with-btn">
                            <input v-model="editedSub.Download.DownloadPath" type="text" class="field-input" placeholder="Wenn leer werden die Standardpfade verwendet">
                            <button @click="selectPath" class="btn btn-secondary">Wählen</button>
                        </div>
                        <p class="field-desc">Leer lassen, um den Standardpfad zu nutzen. Bei Serien wird automatisch ein Unterordner mit dem Abo-Namen erstellt. Bei Filmen wird ein Unterordner mit dem Abo-Namen nur erstellt, wenn die Option "Ordner für das Thema erstellen" in den Einstellungen aktiviert ist.</p>
                    </div>
                    <div class="checkbox-field">
                        <label>
                            <input v-model="editedSub.Download.UseStreamingUrlFiles" type="checkbox"> Streaming-URL-Dateien (.strm) verwenden
                        </label>
                        <p class="field-desc">Verwendet Streaming-URL-Dateien (.strm) anstelle des Herunterladens der tatsächlichen Videodateien. Es werden keine Videodateien gespeichert, die Videos werden von ARD/ZDF direkt gestreamt. Untertitel sind hiervon nicht betroffen.</p>
                    </div>
                    <div v-if="!editedSub.Download.UseStreamingUrlFiles" class="sub-options">
                        <div class="checkbox-field">
                            <label>
                                <input v-model="editedSub.Download.DownloadFullVideoForSecondaryAudio" type="checkbox"> Vollständiges Video für sekundäre Audiosprachen herunterladen
                            </label>
                            <p class="field-desc">Wenn aktiviert, wird das vollständige Video heruntergeladen, auch wenn es eine andere Audiosprache als Deutsch enthält. Andernfalls wird nur die Audiospur dieser Sprache extrahiert.</p>
                        </div>
                    </div>
                    <div class="checkbox-field">
                        <label>
                            <input v-model="editedSub.Download.AlwaysCreateSubfolder" type="checkbox"> Unterordner für dieses Abo erstellen
                        </label>
                        <p class="field-desc">Erstellt immer einen Unterordner mit dem Namen des Abonnements, auch wenn es sich um Filme handelt und die globale Einstellung "Beim Film Downloads Ordner für das Thema erstellen" deaktiviert ist.</p>
                    </div>
                    <div class="checkbox-field">
                        <label>
                            <input v-model="editedSub.Download.EnhancedDuplicateDetection" type="checkbox"> Erweiterte Duplikaterkennung
                        </label>
                        <p class="field-desc">Scannt das Zielverzeichnis nach vorhandenen Dateien mit passenden SxxExx-Mustern (oder absoluter Nummerierung), um doppelte Downloads zu vermeiden (auch bei abweichenden Dateinamen).</p>
                    </div>
                    <div class="checkbox-field">
                        <label>
                            <input v-model="editedSub.Download.AllowFallbackToLowerQuality" type="checkbox"> Fallback auf niedrigere Qualität erlauben
                        </label>
                        <p class="field-desc">Wenn aktiviert, wird beim Herunterladen einer Episode geprüft, ob eine niedrigere Qualität verfügbar ist falls die HD-URL nicht gesetzt ist.</p>
                    </div>
                    <div v-if="editedSub.Download.AllowFallbackToLowerQuality" class="sub-options">
                        <div class="checkbox-field">
                            <label>
                                <input v-model="editedSub.Download.QualityCheckWithUrl" type="checkbox"> Prüft ob die URLs gültig ist.
                            </label>
                            <p class="field-desc">Wenn aktiviert wird auch geprüft, ob die URLs von MediathekView noch verfügbar sind und ggf. die nächst niedrigere versucht. HD → Default → SD</p>
                        </div>
                    </div>
                </div>

                <!-- Serien Tab -->
                <div v-if="activeTab === 'series'" class="tab-pane">
                    <div class="checkbox-field">
                        <label>
                            <input v-model="editedSub.Series.EnforceSeriesParsing" type="checkbox"> Nur Serien herunterladen
                        </label>
                        <p class="field-desc">Nur Videos herunterladen, die als Serie erkannt werden</p>
                    </div>
                    <div v-if="editedSub.Series.EnforceSeriesParsing" class="sub-options">
                        <div class="checkbox-field">
                            <label>
                                <input v-model="editedSub.Series.AllowAbsoluteEpisodeNumbering" type="checkbox"> Absolute Episodennummerierung erlauben
                            </label>
                            <p class="field-desc">Episoden auch herunterladen, wenn nur Absolute Episodennummerierung vorliegt (z.B. "Episode 5" statt "Staffel 1, Episode 5").</p>
                        </div>
                    </div>
                    <div v-else class="sub-options">
                        <div class="checkbox-field">
                            <label>
                                <input v-model="editedSub.Series.TreatNonEpisodesAsExtras" type="checkbox"> Nicht Episoden als Extras behandeln
                            </label>
                            <p class="field-desc">Nicht als Episoden erkannte Videos als Extras behandeln.</p>
                        </div>
                        <div v-if="editedSub.Series.TreatNonEpisodesAsExtras" class="sub-options">
                            <div class="checkbox-field">
                                <label><input v-model="editedSub.Series.SaveTrailers" type="checkbox"> Trailer speichern</label>
                                <p class="field-desc">Trailer werden gespeichert.</p>
                            </div>
                            <div class="checkbox-field">
                                <label><input v-model="editedSub.Series.SaveInterviews" type="checkbox"> Interviews speichern</label>
                                <p class="field-desc">Interviews werden gespeichert.</p>
                            </div>
                            <div class="checkbox-field">
                                <label><input v-model="editedSub.Series.SaveGenericExtras" type="checkbox"> Generische Extras speichern</label>
                                <p class="field-desc">Alle anderen Extras (nicht Trailer/Interviews) werden gespeichert.</p>
                            </div>
                            <div class="checkbox-field">
                                <label><input v-model="editedSub.Series.SaveExtrasAsStrm" type="checkbox"> Extras als Stream (.strm) speichern</label>
                                <p class="field-desc">Extras werden als .strm Dateien gespeichert (spart Speicherplatz).</p>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Metadaten Tab -->
                <div v-if="activeTab === 'metadata'" class="tab-pane">
                    <div class="field">
                        <label>Originalsprache (ISO Code, z.B. 'eng')</label>
                        <input v-model="editedSub.Metadata.OriginalLanguage" type="text" class="field-input" placeholder="z.B. eng oder fra">
                        <p class="field-desc">Falls gesetzt, wird dieser Sprachcode verwendet, wenn der Inhalt als Originalversion (OV/OmU) erkannt wird (statt 'und').</p>
                    </div>
                    <div class="checkbox-field">
                        <label>
                            <input v-model="editedSub.Metadata.CreateNfo" type="checkbox"> NFO Dateien erstellen
                        </label>
                        <p class="field-desc">Erstellt eine .nfo Datei mit Metadaten (Beschreibung, Episodennummer) neben der Videodatei.</p>
                    </div>
                    <div class="checkbox-field">
                        <label>
                            <input v-model="editedSub.Metadata.AppendDateToTitle" type="checkbox"> Datum an Titel anhängen
                        </label>
                        <p class="field-desc">Hängt das Sendedatum an den Titel an (z.B. "Titel - 2026-01-01") und erzwingt die Erkennung als Serie. Nützlich für Sendungen wie "Tagesschau in 100 Sekunden", die kein Release-Datum im Titel haben.</p>
                    </div>
                    <div v-if="editedSub.Metadata.AppendDateToTitle" class="sub-options">
                        <div class="checkbox-field">
                            <label>
                                <input v-model="editedSub.Metadata.AppendTimeToTitle" type="checkbox"> Uhrzeit an Titel anhängen
                            </label>
                            <p class="field-desc">Hängt die Uhrzeit an den Titel an (z.B. "Titel - 2026-01-01 20-00").</p>
                        </div>
                    </div>
                    <div class="checkbox-field">
                        <label>
                            <input v-model="editedSub.Metadata.KeepOriginalTitle" type="checkbox"> Originaltitel beibehalten
                        </label>
                        <p class="field-desc">Behält den Originaltitel bei und entfernt keine Informationen wie (AD), Gebärdensprache oder Episodennummern aus dem Titel.</p>
                    </div>
                </div>

                <!-- Barrierefreiheit Tab -->
                <div v-if="activeTab === 'accessibility'" class="tab-pane">
                    <div class="checkbox-field">
                        <label>
                            <input v-model="editedSub.Accessibility.AllowAudioDescription" type="checkbox"> Versionen mit Audiodeskription herunterladen
                        </label>
                        <p class="field-desc">Lädt auch Inhalte mit Audiodeskription herunter (sofern verfügbar).</p>
                    </div>
                    <div class="checkbox-field">
                        <label>
                            <input v-model="editedSub.Accessibility.AllowSignLanguage" type="checkbox"> Versionen mit Gebärdensprache herunterladen
                        </label>
                        <p class="field-desc">Lädt auch Inhalte mit Gebärdensprache herunter. (sofern verfügbar).</p>
                    </div>
                </div>
            </div>

            <footer class="editor-footer">
                <button @click="cancel" class="btn btn-secondary">Abbrechen</button>
                <button @click="$emit('test', editedSub)" class="btn btn-secondary">Abo prüfen (Dry Run)</button>
                <button @click="save" class="btn btn-primary">Abo Speichern</button>
            </footer>
        </div>
    </div>
</template>

<style scoped>
.editor-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.8);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 9999;
    padding: 20px;
}

.editor-modal {
    width: 100%;
    max-width: 800px;
    height: 80vh;
    min-height: 500px;
    display: flex;
    flex-direction: column;
    overflow: hidden;
}

.editor-header {
    padding: 20px;
    border-bottom: 1px solid #3f3f46;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.editor-tabs {
    display: flex;
    background: #27272a;
    border-bottom: 1px solid #3f3f46;
    overflow-x: auto;
}

.editor-content {
    padding: 20px;
    overflow-y: auto;
    flex: 1;
}

.editor-footer {
    padding: 20px;
    border-top: 1px solid #3f3f46;
    display: flex;
    justify-content: flex-end;
    gap: 15px;
}

.tab-btn {
    padding: 12px 20px;
    background: none;
    border: none;
    color: #a1a1aa;
    cursor: pointer;
    white-space: nowrap;
}

.tab-btn.active {
    color: #7c3aed;
    background: #18181b;
    border-bottom: 2px solid #7c3aed;
}

.grid-2 {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 20px;
}

.query-row {
    background: #27272a;
    padding: 15px;
    border-radius: 8px;
    margin-bottom: 15px;
    border: 1px solid #3f3f46;
}

.query-fields {
    display: flex;
    gap: 8px;
    margin-bottom: 10px;
}

.field-tag {
    padding: 4px 10px;
    border-radius: 12px;
    background: #3f3f46;
    border: none;
    color: #a1a1aa;
    font-size: 0.75rem;
    cursor: pointer;
}

.field-tag.active {
    background: #7c3aed;
    color: white;
}

.query-input-row {
    display: flex;
    gap: 10px;
    align-items: center;
}

.input-with-btn {
    display: flex;
    gap: 10px;
}

.sub-options {
    margin-left: 25px;
    border-left: 2px solid #3f3f46;
    padding-left: 15px;
    margin-top: 10px;
    margin-bottom: 10px;
}

.btn-small {
    padding: 5px 10px;
    border-radius: 4px;
    border: 1px solid #3f3f46;
    background: #27272a;
    color: white;
    cursor: pointer;
    font-size: 0.75rem;
}

.btn-danger {
    background: #ef4444;
    border-color: #ef4444;
}
</style>
