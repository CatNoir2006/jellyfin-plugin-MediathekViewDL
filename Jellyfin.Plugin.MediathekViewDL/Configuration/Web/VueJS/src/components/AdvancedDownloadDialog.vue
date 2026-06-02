<script setup>
import { ref, watch } from 'vue'
import ApiService from '../utils/ApiService'

const props = defineProps({
    item: {
        type: Object,
        default: null
    },
    pluginConfig: { type: Object, default: null }
})

const emit = defineEmits(['close', 'download'])

const Dashboard = window.Dashboard ?? null

const downloadPath = ref('')
const downloadFileName = ref('')
const downloadSubtitles = ref(true)
const subtitleFileName = ref('')
const isLoading = ref(false)
const isDownloading = ref(false)

watch(() => props.item, async (newItem) => {
    if (newItem) {
        downloadSubtitles.value = props.pluginConfig?.Download?.DownloadSubtitles !== false
        await loadRecommendedPath(newItem)
    }
}, { immediate: true })

async function loadRecommendedPath(item) {
    try {
        isLoading.value = true

        // Set defaults from config if available
        if (props.pluginConfig?.Paths?.DefaultDownloadPath) {
            downloadPath.value = props.pluginConfig.Paths.DefaultDownloadPath
        }

        console.log('📤 Parsing item:', item)

        try {
            const videoInfo = await ApiService.parseItem(item)
            console.log('📤 Requesting recommended path with VideoInfo:', videoInfo)

            const data = await ApiService.getRecommendedPath(videoInfo)

            if (data && data.Path) {
                downloadPath.value = data.Path
                downloadFileName.value = data.FileName || item.Title || 'download'
                subtitleFileName.value = data.SubtitleName || ''
                console.log('✓ Applied recommended settings:')
                console.log('  Path:', downloadPath.value)
                console.log('  FileName:', downloadFileName.value)
                console.log('  SubtitleName:', subtitleFileName.value)
            } else if (data) {
                console.warn('Response lacks Path field:', data)
            }
        } catch (apiError) {
            console.warn('⚠ API call failed:', apiError)
            // Fallback: just use the item title as filename
            downloadFileName.value = item.Title || 'download'
        }
    } catch (e) {
        console.error('❌ Error in loadRecommendedPath:', e)
    } finally {
        isLoading.value = false
    }
}

function selectDownloadPath() {
    if (!Dashboard) return
    const picker = new Dashboard.DirectoryBrowser()
    picker.show({
        header: 'Download-Pfad wählen',
        includeDirectories: true,
        includeFiles: false,
        callback: (path) => {
            if (path) {
                downloadPath.value = path
            }
            picker.close()
        }
    })
}

async function startDownload() {
    if (!props.item || !downloadPath.value || !downloadFileName.value) {
        if (Dashboard) Dashboard.alert('Bitte füllen Sie alle erforderlichen Felder aus.')
        return
    }

    try {
        isDownloading.value = true
        const options = {
            Item: props.item,
            DownloadPath: downloadPath.value,
            FileName: downloadFileName.value,
            DownloadSubtitles: downloadSubtitles.value,
            SubtitleName: subtitleFileName.value
        }
        await ApiService.advancedDownload(options)
        if (Dashboard) Dashboard.alert('Erweiterte Download erfolgreich in Warteschlange eingereiht.')
        emit('download')
        emit('close')
    } catch (e) {
        console.error('Advanced download failed', e)
        if (Dashboard) Dashboard.alert('Fehler beim Starten des erweiterten Downloads: ' + (e?.message || 'Unbekannter Fehler'))
    } finally {
        isDownloading.value = false
    }
}

function closeDialog() {
    emit('close')
}
</script>

<template>
    <div class="modal-overlay" @click.self="closeDialog">
        <div class="modal-dialog">
            <div class="modal-header">
                <h3>Erweiterte Download-Optionen</h3>
                <button @click="closeDialog" class="modal-close" :disabled="isDownloading">✕</button>
            </div>
            <div class="modal-content">
                <div v-if="item" class="modal-item-info">
                    <div class="modal-item-title">{{ item.Title }}</div>
                    <div class="modal-item-meta">{{ item.Topic }} | {{ item.Channel }}</div>
                </div>

                <div v-if="isLoading" class="loading-state">
                    <div class="spinner"></div>
                    Lade empfohlene Einstellungen...
                </div>

                <template v-else>
                    <div class="modal-field">
                        <label>Download-Pfad *</label>
                        <div class="path-input-group">
                            <input v-model="downloadPath" type="text" class="field-input" placeholder="Wählen Sie einen Pfad" readonly>
                            <button @click="selectDownloadPath" class="btn btn-secondary btn-sm" type="button" :disabled="isDownloading">
                                Durchsuchen
                            </button>
                        </div>
                    </div>

                    <div class="modal-field">
                        <label>Dateiname *</label>
                        <input v-model="downloadFileName" type="text" class="field-input" placeholder="Dateiname ohne Erweiterung" :disabled="isDownloading">
                    </div>

                    <div class="modal-field checkbox-field">
                        <input v-model="downloadSubtitles" type="checkbox" class="field-checkbox" id="dl-subtitles" :disabled="isDownloading">
                        <label for="dl-subtitles">Untertitel herunterladen</label>
                    </div>

                    <div v-if="downloadSubtitles" class="modal-field">
                        <label>Untertitel-Dateiname</label>
                        <input v-model="subtitleFileName" type="text" class="field-input" placeholder="Leer lassen für automatischen Namen" :disabled="isDownloading">
                    </div>
                </template>
            </div>
            <div class="modal-footer">
                <button @click="closeDialog" class="btn btn-secondary" :disabled="isDownloading">
                    Abbrechen
                </button>
                <button @click="startDownload" class="btn btn-primary" :disabled="isDownloading || !downloadPath || !downloadFileName || isLoading">
                    {{ isDownloading ? 'Wird heruntergeladen...' : 'Download starten' }}
                </button>
            </div>
        </div>
    </div>
</template>

<style scoped>
.modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.7);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
}

.modal-dialog {
    background: #1e1e1e;
    border: 1px solid #3f3f46;
    border-radius: 8px;
    max-width: 500px;
    width: 90%;
    max-height: 90vh;
    overflow-y: auto;
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.5);
}

.modal-header {
    padding: 20px;
    border-bottom: 1px solid #3f3f46;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.modal-header h3 {
    margin: 0;
    font-size: 1.2rem;
}

.modal-close {
    background: none;
    border: none;
    color: #a1a1aa;
    font-size: 1.5rem;
    cursor: pointer;
    padding: 0;
    line-height: 1;
}

.modal-close:hover:not(:disabled) {
    color: #fff;
}

.modal-close:disabled {
    opacity: 0.5;
    cursor: not-allowed;
}

.modal-content {
    padding: 20px;
    min-height: 150px;
}

.loading-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 150px;
    color: #a1a1aa;
}

.spinner {
    width: 30px;
    height: 30px;
    border: 3px solid #3f3f46;
    border-top-color: #7c3aed;
    border-radius: 50%;
    animation: spin 0.8s linear infinite;
    margin-bottom: 10px;
}

@keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
}

.modal-item-info {
    margin-bottom: 20px;
    padding: 15px;
    background: #27272a;
    border-radius: 6px;
}

.modal-item-title {
    font-weight: bold;
    margin-bottom: 4px;
}

.modal-item-meta {
    font-size: 0.85rem;
    color: #a1a1aa;
}

.modal-field {
    margin-bottom: 15px;
}

.modal-field label {
    display: block;
    margin-bottom: 6px;
    font-weight: 500;
}

.path-input-group {
    display: flex;
    gap: 10px;
    align-items: center;
}

.path-input-group input {
    flex: 1;
}

.checkbox-field {
    display: flex;
    align-items: center;
    gap: 8px;
}

.checkbox-field input {
    width: auto;
}

.checkbox-field label {
    margin: 0;
}

.modal-footer {
    padding: 15px 20px;
    border-top: 1px solid #3f3f46;
    display: flex;
    gap: 10px;
    justify-content: flex-end;
}

.field-input {
    width: 100%;
    padding: 8px 12px;
    background: #27272a;
    border: 1px solid #3f3f46;
    color: #e4e4e7;
    border-radius: 4px;
    font-size: 0.9rem;
}

.field-input:focus {
    outline: none;
    border-color: #7c3aed;
    box-shadow: 0 0 0 3px rgba(124, 58, 237, 0.1);
}

.field-input:disabled {
    opacity: 0.6;
    cursor: not-allowed;
}

.field-checkbox {
    cursor: pointer;
}
</style>







