<script setup>
import { ref, computed, watch } from 'vue'
import ApiService from '../utils/ApiService'
import { SubscriptionFactory } from '../utils/SubscriptionFactory'

const props = defineProps({
    open: {
        type: Boolean,
        default: false
    },
    pluginConfig: {
        type: Object,
        default: null
    }
})

const emit = defineEmits([
    'close',
    'paths-updated',
    'subscription-created',
    'tuner-added',
    'guide-added'
])

const Dashboard = window.Dashboard ?? null

const TOTAL_STEPS = 7
const currentStep = ref(1)

// Step 2: Paths (4 main + temp)
const defaultSubscriptionShowPath = ref('')
const defaultSubscriptionMoviePath = ref('')
const defaultManualShowPath = ref('')
const defaultManualMoviePath = ref('')
const tempDownloadPath = ref('')

// Setters to avoid Vue template auto-unwrap issues with path picker callbacks
const setDefaultSubscriptionShowPath = (v) => { defaultSubscriptionShowPath.value = v }
const setDefaultSubscriptionMoviePath = (v) => { defaultSubscriptionMoviePath.value = v }
const setDefaultManualShowPath = (v) => { defaultManualShowPath.value = v }
const setDefaultManualMoviePath = (v) => { defaultManualMoviePath.value = v }
const setTempDownloadPath = (v) => { tempDownloadPath.value = v }

// Step 3: .strm default
const useStreamingUrlFiles = ref(false)

// Step 4: Live TV
const liveTvState = ref('idle') // 'idle' | 'busy' | 'active' (already added)
const liveTvBusy = ref(false)

// Step 5: First subscription
const availableChannels = ref([])
const newSubChannel = ref('')
const newSubQuery = ref('')
const newSubSaving = ref(false)
const newSubError = ref(null)
const newSubCreated = ref(false)

// Internal dirty flag so parent knows when to save
const isDirty = ref(false)

const progressPercent = computed(() => Math.round((currentStep.value / TOTAL_STEPS) * 100))

const isStep5Filled = computed(() =>
    newSubChannel.value.trim() !== '' && newSubQuery.value.trim() !== ''
)

const nextButtonLabel = computed(() => {
    if (currentStep.value === 5 && !isStep5Filled.value) {
        return 'Abo später einrichten →'
    }
    return 'Weiter →'
})

watch(() => props.open, (open) => {
    if (open) {
        loadFromConfig()
        loadChannels()
        checkLiveTvState()
        currentStep.value = 1
    }
}, { immediate: true })

function loadFromConfig() {
    const cfg = props.pluginConfig
    if (!cfg) return
    defaultSubscriptionShowPath.value = cfg.Paths?.DefaultSubscriptionShowPath ?? ''
    defaultSubscriptionMoviePath.value = cfg.Paths?.DefaultSubscriptionMoviePath ?? ''
    defaultManualShowPath.value = cfg.Paths?.DefaultManualShowPath ?? ''
    defaultManualMoviePath.value = cfg.Paths?.DefaultManualMoviePath ?? ''
    tempDownloadPath.value = cfg.Paths?.TempDownloadPath ?? ''
    useStreamingUrlFiles.value = cfg.SubscriptionDefaults?.DownloadSettings?.UseStreamingUrlFiles ?? false
}

async function loadChannels() {
    try {
        const channels = await ApiService.getChannels()
        availableChannels.value = Array.isArray(channels) ? channels : []
    } catch (e) {
        console.error('Failed to load channels', e)
        availableChannels.value = []
    }
}

async function checkLiveTvState() {
    try {
        const config = await ApiService.getLiveTvConfig()
        const tuners = config?.TunerHosts || []
        const providers = config?.ListingProviders || []
        const hasTuner = Array.isArray(tuners) && tuners.some(t => (t.Type || '').toLowerCase() === 'zapp')
        const hasGuide = Array.isArray(providers) && providers.some(p => (p.Type || '').toLowerCase() === 'zapp')
        liveTvState.value = (hasTuner && hasGuide) ? 'active' : 'idle'
    } catch (e) {
        console.error('Failed to check Live TV state', e)
        liveTvState.value = 'idle'
    }
}

function selectPath(setValue, header, currentValue) {
    if (!Dashboard || !Dashboard.DirectoryBrowser) {
        const fallback = prompt(header + '\nAktueller Pfad: ' + (currentValue || ''), currentValue || '')
        if (fallback !== null && fallback.trim() !== '') setValue(fallback.trim())
        return
    }
    try {
        const picker = new Dashboard.DirectoryBrowser()
        picker.show({
            header: header,
            includeDirectories: true,
            includeFiles: false,
            callback: (path) => {
                if (path) setValue(path)
                picker.close()
            }
        })
    } catch (e) {
        const fallback = prompt(header + '\nAktueller Pfad: ' + (currentValue || ''), currentValue || '')
        if (fallback !== null && fallback.trim() !== '') setValue(fallback.trim())
    }
}

function next() {
    if (currentStep.value < TOTAL_STEPS) currentStep.value += 1
}

function prev() {
    if (currentStep.value > 1) currentStep.value -= 1
}

function skipCurrentStep() {
    next()
}

function closeWizard() {
    finish(false)
}

async function finish(skipped = false) {
    if (Dashboard) {
        const msg = skipped
            ? 'Assistent geschlossen. Du kannst ihn jederzeit über den Button im Header erneut starten.'
            : 'Einrichtung abgeschlossen!'
        Dashboard.alert(msg)
    }
    isDirty.value = true
    emit('close', {
        skipped,
        paths: {
            DefaultSubscriptionShowPath: defaultSubscriptionShowPath.value,
            DefaultSubscriptionMoviePath: defaultSubscriptionMoviePath.value,
            DefaultManualShowPath: defaultManualShowPath.value,
            DefaultManualMoviePath: defaultManualMoviePath.value,
            TempDownloadPath: tempDownloadPath.value
        },
        defaults: {
            UseStreamingUrlFiles: useStreamingUrlFiles.value
        }
    })
}

async function activateLiveTv() {
    if (liveTvBusy.value || liveTvState.value === 'active') return
    liveTvBusy.value = true
    let tunerOk = false
    let guideOk = false
    try {
        try {
            await ApiService.addTunerHost({ Type: 'zapp', Url: 'zapp', FriendlyName: 'Zapp (MediathekView)', TunerCount: 0 })
            tunerOk = true
            emit('tuner-added')
        } catch (e) {
            console.error('Error adding tuner', e)
        }
        try {
            await ApiService.addListingProvider({ Type: 'zapp', Id: 'zapp_guide', Name: 'Zapp (MediathekView)' })
            guideOk = true
            emit('guide-added')
        } catch (e) {
            console.error('Error adding guide', e)
        }
        if (tunerOk && guideOk) {
            liveTvState.value = 'active'
            if (Dashboard) Dashboard.alert('Live-TV wurde aktiviert.')
        } else {
            if (Dashboard) Dashboard.alert('Live-TV konnte nicht vollständig aktiviert werden. Bitte prüfe die Logs.')
        }
    } finally {
        liveTvBusy.value = false
    }
}

async function createFirstSubscription() {
    if (!isStep5Filled.value || newSubSaving.value) return
    newSubSaving.value = true
    newSubError.value = null
    try {
        const cfg = props.pluginConfig ?? {}
        // Build defaults from current config and override with the wizard's locally-toggled .strm setting
        const effectiveDefaults = {
            ...(cfg.SubscriptionDefaults || {}),
            DownloadSettings: {
                ...((cfg.SubscriptionDefaults && cfg.SubscriptionDefaults.DownloadSettings) || {}),
                UseStreamingUrlFiles: useStreamingUrlFiles.value
            }
        }
        const baseSub = SubscriptionFactory.createDefault(effectiveDefaults)
        baseSub.Name = newSubChannel.value + ': ' + newSubQuery.value
        baseSub.Search.Criteria = [{
            Fields: ['Title', 'Topic'],
            Query: newSubQuery.value,
            IsExclude: false
        }, {
            Fields: ['Channel'],
            Query: newSubChannel.value,
            IsExclude: false
        }]
        await ApiService.saveSubscription(baseSub)
        newSubCreated.value = true
        emit('subscription-created')
        if (Dashboard) Dashboard.alert('Erstes Abo angelegt.')
    } catch (e) {
        console.error('Save failed', e)
        newSubError.value = 'Fehler beim Anlegen des Abos.'
        if (Dashboard) Dashboard.alert('Fehler beim Anlegen des Abos.')
    } finally {
        newSubSaving.value = false
    }
}

// Expose state for parent (used for testing only)
defineExpose({
    currentStep,
    isDirty
})
</script>

<template>
    <Teleport to="body">
        <div v-if="open" class="wizard-overlay" data-testid="wizard-overlay">
            <div class="wizard-card card" data-testid="wizard-card">
                <header class="wizard-header">
                    <div>
                        <h2>Einrichtungs-Assistent</h2>
                        <p class="wizard-subtitle">Schritt {{ currentStep }} von {{ TOTAL_STEPS }}</p>
                    </div>
                    <button class="btn-icon" @click="closeWizard" title="Assistenten schließen" aria-label="Schließen" data-testid="wizard-close">✕</button>
                </header>

                <div class="wizard-progress">
                    <div class="wizard-progress-bar" :style="{ width: progressPercent + '%' }"></div>
                </div>

                <div class="wizard-content">
                    <!-- Step 1: Willkommen -->
                    <div v-if="currentStep === 1" data-testid="wizard-step-welcome">
                        <h3>Willkommen bei MediathekViewDL</h3>
                        <p>
                            Dieses Plugin durchsucht die Mediatheken von ARD, ZDF und weiteren Sendern
                            und lädt Inhalte direkt in deine Jellyfin-Bibliothek.
                        </p>
                        <p>
                            Dieser Assistent hilft dir in wenigen Schritten bei der Ersteinrichtung.
                            Du kannst ihn jederzeit über den Button im Header erneut starten.
                        </p>
                        <ul class="wizard-list">
                            <li>1. Standard-Pfade für Downloads festlegen</li>
                            <li>2. Speichermodus für neue Abos wählen</li>
                            <li>3. Optional: Live-TV-Integration einrichten</li>
                            <li>4. Optional: Dein erstes Abo anlegen</li>
                            <li>5. Übersicht über die Tabs</li>
                        </ul>
                    </div>

                    <!-- Step 2: Pfade -->
                    <div v-else-if="currentStep === 2" data-testid="wizard-step-paths">
                        <h3>Standard-Pfade festlegen</h3>
                        <p class="wizard-hint">
                            Wo sollen Inhalte standardmäßig gespeichert werden?
                            Du kannst diese Pfade später pro Abo überschreiben.
                        </p>

                        <h4 class="wizard-section-title">Abos</h4>
                        <div class="field">
                            <label class="field-label">Standard Serien Pfad (Abo)</label>
                            <div class="path-row">
                                <input v-model="defaultSubscriptionShowPath" type="text" class="field-input"
                                    placeholder="z.B. /media/jellyfin/Serien" data-testid="wizard-sub-show-path">
                                <button type="button" class="btn btn-secondary btn-sm"
                                    @click="selectPath(setDefaultSubscriptionShowPath, 'Standard Serien Pfad (Abo) wählen', defaultSubscriptionShowPath)"
                                    title="Ordner auswählen" data-testid="wizard-sub-show-pick">📁</button>
                            </div>
                        </div>

                        <div class="field">
                            <label class="field-label">Standard Film Pfad (Abo)</label>
                            <div class="path-row">
                                <input v-model="defaultSubscriptionMoviePath" type="text" class="field-input"
                                    placeholder="z.B. /media/jellyfin/Filme" data-testid="wizard-sub-movie-path">
                                <button type="button" class="btn btn-secondary btn-sm"
                                    @click="selectPath(setDefaultSubscriptionMoviePath, 'Standard Film Pfad (Abo) wählen', defaultSubscriptionMoviePath)"
                                    title="Ordner auswählen" data-testid="wizard-sub-movie-pick">📁</button>
                            </div>
                        </div>

                        <h4 class="wizard-section-title">Manuelle Downloads</h4>
                        <div class="field">
                            <label class="field-label">Standard Serien Pfad (Manuell)</label>
                            <div class="path-row">
                                <input v-model="defaultManualShowPath" type="text" class="field-input"
                                    placeholder="z.B. /media/jellyfin/Serien" data-testid="wizard-man-show-path">
                                <button type="button" class="btn btn-secondary btn-sm"
                                    @click="selectPath(setDefaultManualShowPath, 'Standard Serien Pfad (Manuell) wählen', defaultManualShowPath)"
                                    title="Ordner auswählen">📁</button>
                            </div>
                        </div>

                        <div class="field">
                            <label class="field-label">Standard Film Pfad (Manuell)</label>
                            <div class="path-row">
                                <input v-model="defaultManualMoviePath" type="text" class="field-input"
                                    placeholder="z.B. /media/jellyfin/Filme" data-testid="wizard-man-movie-path">
                                <button type="button" class="btn btn-secondary btn-sm"
                                    @click="selectPath(setDefaultManualMoviePath, 'Standard Film Pfad (Manuell) wählen', defaultManualMoviePath)"
                                    title="Ordner auswählen">📁</button>
                            </div>
                        </div>

                        <h4 class="wizard-section-title">Optional</h4>
                        <div class="field">
                            <label class="field-label">Temporärer Download Pfad <span class="wizard-optional">(optional)</span></label>
                            <div class="path-row">
                                <input v-model="tempDownloadPath" type="text" class="field-input"
                                    placeholder="Leer lassen für direkten Download" data-testid="wizard-temp-path">
                                <button type="button" class="btn btn-secondary btn-sm"
                                    @click="selectPath(setTempDownloadPath, 'Temporären Download Pfad wählen', tempDownloadPath)"
                                    title="Ordner auswählen">📁</button>
                            </div>
                            <p class="field-desc">Optionaler Ordner, in dem Downloads zwischengespeichert werden.</p>
                        </div>
                    </div>

                    <!-- Step 3: Speicher sparen -->
                    <div v-else-if="currentStep === 3" data-testid="wizard-step-strm">
                        <h3>Speicher sparen?</h3>
                        <p>
                            Du kannst neue Abos standardmäßig als <strong>Streaming-Links (.strm)</strong>
                            speichern lassen. Die Datei enthält dann nur einen Verweis auf den Online-Stream,
                            das eigentliche Video wird nicht heruntergeladen.
                        </p>
                        <div class="wizard-callout">
                            <strong>Hinweis:</strong> Beim Abspielen einer .strm-Datei ist eine
                            aktive Internetverbindung erforderlich. Falls du Inhalte auch offline
                            ansehen möchtest, lasse die Option deaktiviert.
                        </div>
                        <div class="checkbox-field">
                            <label>
                                <input v-model="useStreamingUrlFiles" type="checkbox"
                                    data-testid="wizard-strm-toggle">
                                Standardmäßig Streaming-URL-Dateien (.strm) für neue Abos verwenden
                            </label>
                        </div>
                    </div>

                    <!-- Step 4: Live TV -->
                    <div v-else-if="currentStep === 4" data-testid="wizard-step-livetv">
                        <h3>Live-TV-Integration <span class="wizard-optional">(optional)</span></h3>
                        <p>
                            Du kannst die Live-Streams von MediathekView als Jellyfin-Live-TV-Kanäle
                            und Programmführer einbinden.
                        </p>
                        <div class="btn-row">
                            <button v-if="liveTvState === 'active'" type="button" class="btn btn-secondary"
                                disabled data-testid="wizard-livetv-active">
                                ✓ Live-TV ist aktiviert
                            </button>
                            <button v-else type="button" class="btn btn-primary"
                                :disabled="liveTvBusy"
                                @click="activateLiveTv" data-testid="wizard-livetv-activate">
                                <span v-if="liveTvBusy">Wird aktiviert...</span>
                                <span v-else>📺 Live-TV aktivieren</span>
                            </button>
                        </div>
                    </div>

                    <!-- Step 5: Erstes Abo -->
                    <div v-else-if="currentStep === 5" data-testid="wizard-step-subscription">
                        <h3>Dein erstes Abo <span class="wizard-optional">(optional)</span></h3>
                        <p>
                            Lege jetzt direkt ein erstes Abo an. Du kannst später im
                            <strong>Abos</strong>-Tab weitere Abos anlegen und alle Optionen
                            feinjustieren.
                        </p>
                        <div class="field">
                            <label class="field-label">Sender</label>
                            <select v-model="newSubChannel" class="field-select" data-testid="wizard-channel"
                                :disabled="newSubCreated">
                                <option value="">-- Sender wählen --</option>
                                <option v-for="ch in availableChannels" :key="ch" :value="ch">{{ ch }}</option>
                            </select>
                        </div>
                        <div class="field">
                            <label class="field-label">Suchbegriff (Titel oder Thema)</label>
                            <input v-model="newSubQuery" type="text" class="field-input"
                                placeholder="z.B. Tatort, Tagesschau, ..." data-testid="wizard-query"
                                :disabled="newSubCreated">
                        </div>
                        <div v-if="newSubError" class="wizard-error">{{ newSubError }}</div>
                        <button v-if="newSubCreated" type="button" class="btn btn-secondary" disabled
                            data-testid="wizard-sub-created">
                            ✓ Abo angelegt
                        </button>
                        <button v-else type="button" class="btn btn-primary" :disabled="!isStep5Filled || newSubSaving"
                            @click="createFirstSubscription" data-testid="wizard-create-sub">
                            <span v-if="newSubSaving">Wird angelegt...</span>
                            <span v-else>Abo anlegen</span>
                        </button>
                    </div>

                    <!-- Step 6: Tab-Tour -->
                    <div v-else-if="currentStep === 6" data-testid="wizard-step-tour">
                        <h3>Übersicht der Tabs</h3>
                        <div class="tour-grid">
                            <div class="tour-item">
                                <h4>🔍 Suche</h4>
                                <p>Durchsuche die gesamte Mediathek nach Titeln, Themen oder Sendern.
                                Lade einzelne Sendungen direkt herunter.</p>
                            </div>
                            <div class="tour-item">
                                <h4>⚙️ Einstellungen</h4>
                                <p>Pfade, Download-Optionen, Abo-Standardwerte, Wartung und Live-TV.
                                Alles, was nicht direkt mit Inhalten zu tun hat.</p>
                            </div>
                            <div class="tour-item">
                                <h4>📺 Abos</h4>
                                <p>Verwalte deine automatischen Abonnements. Lege neue Abos an,
                                aktiviere/deaktiviere sie und starte Downloads manuell.</p>
                            </div>
                            <div class="tour-item">
                                <h4>📥 Downloads</h4>
                                <p>Hier findest du die aktiven Downloads, die Warteschlange und einen
                                Verlauf der letzten Downloads.</p>
                            </div>
                            <div class="tour-item">
                                <h4>📋 Logs</h4>
                                <p>Zeigt die Jellyfin-Server-Logs an. Filtere nach diesem Plugin,
                                um Fehler oder Warnungen schnell zu finden.</p>
                            </div>
                        </div>
                    </div>

                    <!-- Step 7: Fertig -->
                    <div v-else-if="currentStep === 7" data-testid="wizard-step-finish">
                        <h3>🎉 Du bist startklar!</h3>
                        <p>
                            Die Einrichtung ist abgeschlossen. Du kannst jetzt:
                        </p>
                        <ul class="wizard-list">
                            <li>Im <strong>Abos</strong>-Tab deine Abos verwalten</li>
                            <li>Im <strong>Suche</strong>-Tab einzelne Sendungen herunterladen</li>
                            <li>Im <strong>Einstellungen</strong>-Tab weitere Optionen anpassen</li>
                        </ul>
                        <p>
                            Den Assistenten findest du jederzeit über den Button im Header wieder.
                        </p>
                    </div>
                </div>

                <footer class="wizard-footer">
                    <button v-if="currentStep > 1" type="button" class="btn btn-secondary btn-sm"
                        @click="prev" data-testid="wizard-prev">← Zurück</button>
                    <div class="wizard-footer-spacer"></div>
                    <button v-if="currentStep < TOTAL_STEPS && currentStep !== 1 && currentStep !== 6"
                        type="button" class="btn btn-secondary btn-sm"
                        @click="skipCurrentStep" data-testid="wizard-skip">Diesen Schritt überspringen</button>
                    <button v-if="currentStep < TOTAL_STEPS" type="button" class="btn btn-primary"
                        @click="next" data-testid="wizard-next">
                        {{ nextButtonLabel }}
                    </button>
                    <button v-else type="button" class="btn btn-save"
                        @click="finish(false)" data-testid="wizard-finish">
                        Fertig
                    </button>
                </footer>
            </div>
        </div>
    </Teleport>
</template>

<style scoped>
.wizard-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.95);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 10001;
    padding: 20px;
}

.wizard-card {
    width: 100%;
    max-width: 720px;
    max-height: 90vh;
    display: flex;
    flex-direction: column;
    overflow: hidden;
}

.wizard-header {
    padding: 20px;
    border-bottom: 1px solid #3f3f46;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.wizard-header h2 {
    margin: 0;
    color: #e4e4e7;
    font-size: 1.25rem;
}

.wizard-subtitle {
    margin: 4px 0 0 0;
    color: #a1a1aa;
    font-size: 0.85rem;
}

.wizard-progress {
    height: 4px;
    background: #27272a;
}

.wizard-progress-bar {
    height: 100%;
    background: linear-gradient(90deg, #7c3aed, #10b981);
    transition: width 0.3s ease;
}

.wizard-content {
    padding: 24px;
    overflow-y: auto;
    flex: 1;
    color: #e4e4e7;
}

.wizard-content h3 {
    margin-top: 0;
    color: #7c3aed;
}

.wizard-hint {
    color: #a1a1aa;
    font-size: 0.9rem;
    margin-bottom: 20px;
}

.wizard-optional {
    color: #71717a;
    font-weight: 400;
    font-size: 0.85rem;
}

.wizard-section-title {
    margin: 18px 0 10px 0;
    color: #a1a1aa;
    font-size: 0.85rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    padding-bottom: 5px;
    border-bottom: 1px solid #3f3f46;
}

.wizard-section-title:first-of-type {
    margin-top: 0;
}

.wizard-list {
    margin: 15px 0;
    padding-left: 20px;
    color: #a1a1aa;
}

.wizard-list li {
    margin-bottom: 6px;
}

.wizard-callout {
    background: #1e3a5f;
    border: 1px solid #2563eb;
    color: #93c5fd;
    border-radius: 6px;
    padding: 12px 14px;
    font-size: 0.9rem;
    margin: 15px 0;
    line-height: 1.5;
}

.wizard-error {
    color: #f87171;
    margin: 8px 0;
    font-size: 0.9rem;
}

.path-row {
    display: flex;
    gap: 8px;
    align-items: center;
}

.path-row .field-input {
    flex: 1;
}

.btn-row {
    display: flex;
    gap: 10px;
    flex-wrap: wrap;
    margin-top: 15px;
}

.tour-grid {
    display: grid;
    grid-template-columns: 1fr;
    gap: 12px;
    margin-top: 15px;
}

@media (min-width: 600px) {
    .tour-grid {
        grid-template-columns: 1fr 1fr;
    }
}

.tour-item {
    background: #1c1c1f;
    border: 1px solid #3f3f46;
    border-radius: 6px;
    padding: 14px;
}

.tour-item h4 {
    margin: 0 0 6px 0;
    color: #e4e4e7;
    font-size: 1rem;
}

.tour-item p {
    margin: 0;
    color: #a1a1aa;
    font-size: 0.875rem;
    line-height: 1.4;
}

.wizard-footer {
    padding: 16px 20px;
    border-top: 1px solid #3f3f46;
    display: flex;
    gap: 10px;
    align-items: center;
    background: #18181b;
}

.wizard-footer-spacer {
    flex: 1;
}
</style>