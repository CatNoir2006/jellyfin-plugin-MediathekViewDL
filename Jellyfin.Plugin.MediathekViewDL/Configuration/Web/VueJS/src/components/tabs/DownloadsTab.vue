<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue'
import ApiService from '../../utils/ApiService'

const Dashboard = window.Dashboard ?? null

const activeDownloads = ref([])
const groupedHistory = ref([])
const loading = ref(true)
const error = ref(null)
const expandedGroups = ref(new Set())
const expandedActive = ref(new Set())
let refreshInterval = null

const statusMap = {
  'Queued': { label: 'In Warteschlange', class: 'status-queued' },
  'Downloading': { label: 'Wird heruntergeladen', class: 'status-downloading' },
  'Processing': { label: 'Wird verarbeitet', class: 'status-processing' },
  'Finished': { label: 'Abgeschlossen', class: 'status-finished' },
  'Failed': { label: 'Fehlgeschlagen', class: 'status-failed' },
  'Cancelled': { label: 'Abgebrochen', class: 'status-cancelled' }
}

const hasCancellableJobs = computed(() => {
  return activeDownloads.value.some(dl => isCancellable(dl.Status))
})

const hasInactiveJobs = computed(() => {
  return activeDownloads.value.some(dl => !isCancellable(dl.Status))
})

async function fetchActiveDownloads() {
  try {
    activeDownloads.value = await ApiService.getActiveDownloads()
  } catch (e) {
    console.error('Failed to fetch active downloads', e)
  }
}

async function fetchHistory() {
  try {
    groupedHistory.value = await ApiService.getDownloadHistory()
  } catch (e) {
    console.error('Failed to fetch download history', e)
    error.value = 'Fehler beim Laden des Verlaufs.'
  } finally {
    loading.value = false
  }
}

async function cancelDownload(id) {
  if (!Dashboard) return
  Dashboard.confirm('Soll dieser Download wirklich abgebrochen werden?', 'Download abbrechen', async (result) => {
    if (result) {
      try {
        await ApiService.cancelDownload(id)
        await fetchActiveDownloads()
      } catch (e) {
        console.error('Cancel failed', e)
        Dashboard.alert('Fehler beim Abbrechen des Downloads.')
      }
    }
  })
}

async function cancelAllDownloads() {
  if (!Dashboard) return
  Dashboard.confirm('Sollen wirklich ALLE aktiven Downloads abgebrochen werden?', 'Alle abbrechen', async (result) => {
    if (result) {
      try {
        await ApiService.cancelAllDownloads()
        await fetchActiveDownloads()
      } catch (e) {
        console.error('Cancel all failed', e)
        Dashboard.alert('Fehler beim Abbrechen der Downloads.')
      }
    }
  })
}

async function clearInactiveDownloads() {
  try {
    await ApiService.clearInactiveDownloads()
    await fetchActiveDownloads()
  } catch (e) {
    console.error('Clear inactive failed', e)
  }
}

function formatDate(dateStr) {
  if (!dateStr) return '-'
  return new Date(dateStr).toLocaleString()
}

function getStatusLabel(status) {
  return statusMap[status]?.label || status || 'Unbekannt'
}

function getStatusClass(status) {
  return statusMap[status]?.class || ''
}

function isCancellable(status) {
  return ['Queued', 'Downloading', 'Processing'].includes(status)
}

function showProgressBar(status) {
  return ['Downloading', 'Processing'].includes(status)
}

function toggleGroup(group) {
  const key = getGroupKey(group)
  if (expandedGroups.value.has(key)) {
    expandedGroups.value.delete(key)
  } else {
    expandedGroups.value.add(key)
  }
}

function toggleActive(id) {
  if (expandedActive.value.has(id)) {
    expandedActive.value.delete(id)
  } else {
    expandedActive.value.add(id)
  }
}

function getGroupKey(group) {
  return (group.SubscriptionId || 'manual') + '_' + (group.ItemId || group.Title)
}

function getFileIcon(path) {
  if (!path) return ''
  const ext = path.split('.').pop().toLowerCase()
  if (ext === 'vtt' || ext === 'ttml') return 'Subtitle'
  if (ext === 'nfo') return 'Metadata'
  if (ext === 'strm') return 'Stream'
  if (['mp4', 'mkv', 'webm', 'ts'].includes(ext)) return 'Video'
  return ''
}

function getFileName(path) {
  if (!path) return ''
  return path.split(/[\\\/]/).pop()
}

onMounted(async () => {
  await Promise.all([fetchActiveDownloads(), fetchHistory()])
  refreshInterval = setInterval(fetchActiveDownloads, 3000)
})

onUnmounted(() => {
  if (refreshInterval) clearInterval(refreshInterval)
})
</script>

<template>
  <div class="downloads-tab">
    <!-- Active Downloads -->
    <section class="card active-downloads-section">
      <div class="header-row">
        <h2>Aktive Downloads</h2>
        <div class="header-actions">
          <button
            v-if="hasInactiveJobs"
            @click="clearInactiveDownloads"
            class="btn btn-secondary btn-sm"
          >
            Liste bereinigen
          </button>
          <button
            v-if="hasCancellableJobs"
            @click="cancelAllDownloads"
            class="btn btn-danger btn-sm"
          >
            Alle abbrechen
          </button>
        </div>
      </div>

      <div v-if="activeDownloads.length === 0" class="no-data">
        Keine aktiven Downloads.
      </div>
      <div v-else class="list-container">
        <div v-for="dl in activeDownloads" :key="dl.Id" class="item-container">
          <div class="item-header" @click="toggleActive(dl.Id)">
            <div class="item-info">
              <div class="item-title">
                <span class="expand-icon">{{ expandedActive.has(dl.Id) ? '▼' : '▶' }}</span>
                {{ dl.Job.Title }}
              </div>
              <div class="item-meta">
                <span :class="['status-badge', getStatusClass(dl.Status)]">
                  {{ getStatusLabel(dl.Status) }}
                </span>
                <span v-if="dl.Status === 'Downloading'" class="progress-text">{{ Math.round(dl.Progress) }}%</span>
              </div>
            </div>

            <div class="item-progress" v-if="showProgressBar(dl.Status)">
              <div class="progress-bar-bg">
                <div class="progress-bar-fill" :style="{ width: dl.Progress + '%' }"></div>
              </div>
            </div>

            <div v-if="dl.ErrorMessage" class="error-msg-small">
              {{ dl.ErrorMessage }}
            </div>

            <div class="item-actions">
              <button
                v-if="isCancellable(dl.Status)"
                @click.stop="cancelDownload(dl.Id)"
                class="btn-icon btn-cancel"
                title="Abbrechen"
              >
                ✕
              </button>
            </div>
          </div>

          <!-- Active Details -->
          <div v-if="expandedActive.has(dl.Id)" class="item-details">
            <div v-for="(item, idx) in dl.Job.DownloadItems" :key="idx" class="detail-entry">
              <div class="entry-file">
                <span v-if="getFileIcon(item.DestinationPath)" class="file-type-badge">
                  {{ getFileIcon(item.DestinationPath) }}
                </span>
                <span class="file-name">{{ getFileName(item.DestinationPath) }}</span>
              </div>
              <div class="entry-path">{{ item.DestinationPath }}</div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- History -->
    <section class="card history-section">
      <h2>Download Verlauf</h2>
      <div v-if="loading" class="state-msg">
        <div class="spinner"></div>
        Lade Verlauf...
      </div>
      <div v-else-if="error" class="error-container">
        {{ error }}
      </div>
      <div v-else-if="groupedHistory.length === 0" class="no-data">
        Kein Download-Verlauf vorhanden.
      </div>
      <div v-else class="list-container">
        <div v-for="group in groupedHistory" :key="getGroupKey(group)" class="item-container">
          <div class="item-header" @click="toggleGroup(group)">
            <div class="item-info">
              <div class="item-title">
                <span class="expand-icon">{{ expandedGroups.has(getGroupKey(group)) ? '▼' : '▶' }}</span>
                {{ group.DisplayName || 'Unbekannter Titel' }}
              </div>
              <div class="item-meta">
                <span class="timestamp-text">{{ formatDate(group.LatestTimestamp) }}</span>
                <span v-if="group.Entries.length > 1" class="file-count">({{ group.Entries.length }} Dateien)</span>
              </div>
            </div>
          </div>

          <div v-if="expandedGroups.has(getGroupKey(group))" class="item-details">
            <div v-for="entry in group.Entries" :key="entry.Id" class="detail-entry">
              <div class="entry-file">
                <span v-if="getFileIcon(entry.DownloadPath)" class="file-type-badge">
                  {{ getFileIcon(entry.DownloadPath) }}
                </span>
                <span class="file-name">{{ getFileName(entry.DownloadPath) }}</span>
                <span v-if="entry.Language" class="file-lang">({{ entry.Language }})</span>
              </div>
              <div class="entry-path">{{ entry.DownloadPath }}</div>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.downloads-tab { display: flex; flex-direction: column; gap: 20px; }

.header-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; }
.header-actions { display: flex; gap: 10px; }

/* Shared List Layout */
.list-container { display: grid; gap: 10px; }
.item-container {
  background: #27272a;
  border: 1px solid #3f3f46;
  border-radius: 8px;
  overflow: hidden;
}
.item-header {
  padding: 15px;
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 10px;
  align-items: center;
  cursor: pointer;
}
.item-header:hover { background: rgba(255, 255, 255, 0.03); }

.item-info { display: flex; flex-direction: column; gap: 5px; }
.item-title { font-weight: bold; font-size: 1rem; display: flex; align-items: center; gap: 8px; }
.item-meta { display: flex; align-items: center; gap: 10px; font-size: 0.85rem; }

.expand-icon { font-size: 0.7rem; color: #71717a; width: 12px; text-align: center; }

.status-badge {
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 0.75rem;
  font-weight: 600;
  background: #3f3f46;
}
.status-downloading { background: #3b82f6; color: white; }
.status-processing { background: #8b5cf6; color: white; }
.status-finished { background: #10b981; color: white; }
.status-failed { background: #ef4444; color: white; }
.status-cancelled { background: #71717a; color: white; }
.status-queued { background: #f59e0b; color: white; }

.timestamp-text { color: #a1a1aa; }
.file-count { color: #71717a; }

.item-progress { grid-column: 1 / -1; margin-top: 5px; }
.progress-bar-bg { background: #18181b; height: 8px; border-radius: 4px; overflow: hidden; }
.progress-bar-fill { background: #7c3aed; height: 100%; transition: width 0.3s ease; }

.item-details {
  padding: 0 15px 15px 38px;
  display: flex; flex-direction: column; gap: 8px;
  border-top: 1px solid rgba(255, 255, 255, 0.05);
  padding-top: 10px;
}

.detail-entry { display: flex; flex-direction: column; gap: 2px; }
.entry-file { display: flex; align-items: center; gap: 8px; font-size: 0.9rem; font-weight: 500; }
.entry-path { font-size: 0.75rem; color: #71717a; word-break: break-all; }

.file-type-badge {
  font-size: 0.65rem; background: #3f3f46; color: #e4e4e7;
  padding: 1px 4px; border-radius: 3px; font-weight: bold;
}
.file-lang { color: #71717a; font-size: 0.8rem; }

.error-msg-small { grid-column: 1 / -1; font-size: 0.8rem; color: #ef4444; margin-top: 5px; }

.btn-icon { background: none; border: none; cursor: pointer; padding: 5px; }
.btn-cancel { color: #ef4444; font-size: 1.2rem; font-weight: bold; }
.btn-cancel:hover { background: rgba(239, 68, 68, 0.1); border-radius: 4px; }

.no-data { text-align: center; color: #a1a1aa; padding: 30px; }
.state-msg { text-align: center; padding: 30px; color: #a1a1aa; }
</style>
