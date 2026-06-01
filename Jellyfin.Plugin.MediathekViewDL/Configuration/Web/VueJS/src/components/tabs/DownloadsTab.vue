<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue'

const ApiClient = window.ApiClient ?? null
const Dashboard = window.Dashboard ?? null

const activeDownloads = ref([])
const history = ref([])
const loading = ref(true)
const error = ref(null)
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
  if (!ApiClient) return
  try {
    const url = ApiClient.getUrl('MediathekViewDL/Downloads/Active')
    activeDownloads.value = await ApiClient.getJSON(url)
  } catch (e) {
    console.error('Failed to fetch active downloads', e)
  }
}

async function fetchHistory() {
  if (!ApiClient) return
  try {
    const url = ApiClient.getUrl('MediathekViewDL/Downloads/History')
    history.value = await ApiClient.getJSON(url)
  } catch (e) {
    console.error('Failed to fetch download history', e)
    error.value = 'Fehler beim Laden des Verlaufs.'
  } finally {
    loading.value = false
  }
}

async function cancelDownload(id) {
  if (!ApiClient || !Dashboard) return
  Dashboard.confirm('Soll dieser Download wirklich abgebrochen werden?', 'Download abbrechen', async (result) => {
    if (result) {
      try {
        const url = ApiClient.getUrl('MediathekViewDL/Downloads/' + id)
        await ApiClient.ajax({ type: 'DELETE', url })
        await fetchActiveDownloads()
      } catch (e) {
        console.error('Cancel failed', e)
        Dashboard.alert('Fehler beim Abbrechen des Downloads.')
      }
    }
  })
}

async function cancelAllDownloads() {
  if (!ApiClient || !Dashboard) return
  Dashboard.confirm('Sollen wirklich ALLE aktiven Downloads abgebrochen werden?', 'Alle abbrechen', async (result) => {
    if (result) {
      try {
        const url = ApiClient.getUrl('MediathekViewDL/Downloads')
        await ApiClient.ajax({ type: 'DELETE', url })
        await fetchActiveDownloads()
      } catch (e) {
        console.error('Cancel all failed', e)
        Dashboard.alert('Fehler beim Abbrechen der Downloads.')
      }
    }
  })
}

async function clearInactiveDownloads() {
  if (!ApiClient) return
  try {
    const url = ApiClient.getUrl('MediathekViewDL/Downloads/ClearInactive')
    await ApiClient.ajax({ type: 'POST', url })
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
      <div v-else class="active-list">
        <div v-for="dl in activeDownloads" :key="dl.Id" class="active-item">
          <div class="active-item-info">
            <div class="active-item-title">{{ dl.Job.Title }}</div>
            <div class="active-item-meta">
              <span :class="['status-badge', getStatusClass(dl.Status)]">
                {{ getStatusLabel(dl.Status) }}
              </span>
              <span v-if="dl.Status === 'Downloading'" class="progress-text">{{ Math.round(dl.Progress) }}%</span>
            </div>
          </div>
          
          <div class="active-item-progress" v-if="showProgressBar(dl.Status)">
            <div class="progress-bar-bg">
              <div class="progress-bar-fill" :style="{ width: dl.Progress + '%' }"></div>
            </div>
          </div>

          <div v-if="dl.ErrorMessage" class="error-msg-small">
            {{ dl.ErrorMessage }}
          </div>

          <div class="active-item-actions">
            <button 
              v-if="isCancellable(dl.Status)" 
              @click="cancelDownload(dl.Id)" 
              class="btn-icon btn-cancel" 
              title="Abbrechen"
            >
              ✕
            </button>
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
      <div v-else-if="history.length === 0" class="no-data">
        Kein Download-Verlauf vorhanden.
      </div>
      <div v-else class="history-list">
        <div v-for="entry in history" :key="entry.Id" class="history-item">
          <div class="history-info">
            <div class="history-title">{{ entry.Title }}</div>
            <div class="history-path">{{ entry.DownloadPath }}</div>
          </div>
          <div class="history-meta">
            {{ formatDate(entry.Timestamp) }}
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

.btn { border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer; font-weight: 600; font-size: 0.85rem; }
.btn-sm { padding: 4px 8px; }
.btn-secondary { background: #3f3f46; color: white; }
.btn-secondary:hover { background: #52525b; }
.btn-danger { background: #ef4444; color: white; }
.btn-danger:hover { background: #dc2626; }

/* Active Downloads */
.active-list { display: grid; gap: 15px; }
.active-item {
  background: #27272a;
  border: 1px solid #3f3f46;
  border-radius: 8px;
  padding: 15px;
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 10px;
  align-items: center;
}
.active-item-info { display: flex; flex-direction: column; gap: 5px; }
.active-item-title { font-weight: bold; font-size: 1rem; }
.active-item-meta { display: flex; align-items: center; gap: 10px; font-size: 0.85rem; }

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

.active-item-progress { grid-column: 1 / -1; margin-top: 5px; }
.progress-bar-bg { background: #18181b; height: 8px; border-radius: 4px; overflow: hidden; }
.progress-bar-fill { background: #7c3aed; height: 100%; transition: width 0.3s ease; }

.error-msg-small { grid-column: 1 / -1; font-size: 0.8rem; color: #ef4444; margin-top: 5px; }

.btn-cancel { color: #ef4444; font-size: 1.2rem; font-weight: bold; background: none; border: none; cursor: pointer; padding: 5px; }
.btn-cancel:hover { background: rgba(239, 68, 68, 0.1); border-radius: 4px; }

/* History */
.history-list { display: flex; flex-direction: column; }
.history-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px;
  border-bottom: 1px solid #27272a;
}
.history-item:last-child { border-bottom: none; }
.history-info { display: flex; flex-direction: column; gap: 2px; }
.history-title { font-weight: 600; font-size: 0.95rem; }
.history-path { font-size: 0.75rem; color: #71717a; word-break: break-all; }
.history-meta { font-size: 0.85rem; color: #a1a1aa; white-space: nowrap; margin-left: 15px; }

.no-data { text-align: center; color: #a1a1aa; padding: 30px; }
.state-msg { text-align: center; padding: 30px; color: #a1a1aa; }
.spinner {
  border: 3px solid rgba(255, 255, 255, 0.1);
  border-radius: 50%;
  border-top: 3px solid #7c3aed;
  width: 24px;
  height: 24px;
  animation: spin 1s linear infinite;
  margin: 0 auto 10px;
}
@keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
</style>
