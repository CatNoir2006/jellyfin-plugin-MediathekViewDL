<script setup>
import { ref, computed, watch, onMounted, onUnmounted, nextTick } from 'vue'
import ApiService from '../../utils/ApiService'

const Dashboard = window.Dashboard ?? null

const ENTRY_PATTERN = /^\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d+ [^\]]+)\] \[(\w+)\] \[(\d+)\]/

const logFiles = ref([])
const selectedFile = ref(null)
const rawContent = ref('')
const parsedEntries = ref([])
const filterPluginOnly = ref(true)
const searchQuery = ref('')
const searchRegex = ref(false)
const searchError = ref(null)
const autoScroll = ref(true)
const autoUpdate = ref(false)
const loading = ref(false)
const loadingFiles = ref(false)
const error = ref(null)
const copiedEntryIdx = ref(-1)
const logContainer = ref(null)

let autoUpdateInterval = null

const levelClassMap = {
  'INF': 'log-inf',
  'WRN': 'log-wrn',
  'ERR': 'log-err',
  'DBG': 'log-dbg',
  'TRC': 'log-trc',
  'VRB': 'log-vrb'
}

const filteredEntries = computed(() => {
  if (!parsedEntries.value.length) return []
  let entries = parsedEntries.value
  if (filterPluginOnly.value) {
    entries = entries.filter(entry => entry.text.includes('MediathekViewDL'))
  }
  const q = searchQuery.value.trim()
  if (!q) return entries
  searchError.value = null
  if (searchRegex.value) {
    try {
      const re = new RegExp(q, 'i')
      return entries.filter(entry => re.test(entry.text))
    } catch (e) {
      searchError.value = 'Ungültiger Regex: ' + e.message
      return entries
    }
  }
  const lower = q.toLowerCase()
  return entries.filter(entry => entry.text.toLowerCase().includes(lower))
})

function parseEntries(raw) {
  if (!raw) return []
  const lines = raw.split('\n')
  const entries = []
  let current = null

  for (const line of lines) {
    const match = line.match(ENTRY_PATTERN)
    if (match) {
      if (current) entries.push(current)
      current = {
        timestamp: match[1],
        level: match[2],
        thread: match[3],
        lines: [line],
        text: line,
        levelClass: levelClassMap[match[2]] || ''
      }
    } else if (current) {
      current.lines.push(line)
      current.text += '\n' + line
    }
  }
  if (current) entries.push(current)
  return entries
}

function formatFileSize(bytes) {
  if (!bytes || bytes === 0) return '0 B'
  if (bytes < 1024) return bytes + ' B'
  if (bytes < 1048576) return (bytes / 1024).toFixed(1) + ' KB'
  return (bytes / 1048576).toFixed(1) + ' MB'
}

function formatDate(dateStr) {
  if (!dateStr) return ''
  return new Date(dateStr).toLocaleString()
}

function scrollToBottom() {
  if (!autoScroll.value || !logContainer.value) return
  nextTick(() => {
    if (logContainer.value) {
      logContainer.value.scrollTop = logContainer.value.scrollHeight
    }
  })
}

async function fetchLogFiles() {
  loadingFiles.value = true
  error.value = null
  try {
    logFiles.value = await ApiService.getServerLogs()
    if (logFiles.value.length > 0 && !selectedFile.value) {
      selectedFile.value = logFiles.value[0].Name
    }
  } catch (e) {
    console.error('Failed to fetch log files', e)
    error.value = 'Fehler beim Laden der Log-Dateien. Möglicherweise fehlen Admin-Rechte.'
  } finally {
    loadingFiles.value = false
  }
}

async function fetchLogContent() {
  if (!selectedFile.value) return
  const isFirstLoad = !rawContent.value
  if (isFirstLoad) loading.value = true
  error.value = null
  try {
    rawContent.value = await ApiService.getLogFileContent(selectedFile.value)
    parsedEntries.value = parseEntries(rawContent.value)
    if (autoScroll.value) scrollToBottom()
  } catch (e) {
    console.error('Failed to fetch log content', e)
    error.value = 'Fehler beim Laden des Log-Inhalts.'
  } finally {
    loading.value = false
  }
}

async function refresh() {
  await fetchLogContent()
}

function startAutoUpdate() {
  stopAutoUpdate()
  autoUpdateInterval = setInterval(fetchLogContent, 5000)
}

function stopAutoUpdate() {
  if (autoUpdateInterval) {
    clearInterval(autoUpdateInterval)
    autoUpdateInterval = null
  }
}

async function copyFilteredLogs() {
  const text = filteredEntries.value.map(e => e.text).join('\n')
  try {
    if (window.isSecureContext) {
      await navigator.clipboard.writeText(text)
      if (Dashboard) Dashboard.alert('Logs in die Zwischenablage kopiert.')
    } else {
      prompt('Bitte manuell kopieren:', text)
    }
  } catch (e) {
    console.error('Failed to copy logs', e)
    prompt('Bitte manuell kopieren:', text)
  }
}

async function copyEntry(entry, idx) {
  try {
    if (window.isSecureContext) {
      await navigator.clipboard.writeText(entry.text)
      copiedEntryIdx.value = idx
      setTimeout(() => { copiedEntryIdx.value = -1 }, 1500)
      if (Dashboard) Dashboard.alert('Eintrag kopiert.')
    } else {
      prompt('Bitte manuell kopieren:', entry.text)
    }
  } catch (e) {
    prompt('Bitte manuell kopieren:', entry.text)
  }
}

watch(selectedFile, () => {
  rawContent.value = ''
  parsedEntries.value = []
  fetchLogContent()
})

watch(autoUpdate, (val) => {
  if (val) startAutoUpdate()
  else stopAutoUpdate()
})

onMounted(async () => {
  await fetchLogFiles()
  if (selectedFile.value) {
    await fetchLogContent()
  }
  if (autoUpdate.value) startAutoUpdate()
})

onUnmounted(() => {
  stopAutoUpdate()
})
</script>

<template>
  <div class="logs-tab">
    <div class="card">
      <div class="header-row">
        <h2>Server Logs</h2>
        <div class="header-actions">
          <button class="btn btn-secondary btn-sm" @click="refresh" :disabled="loading || !selectedFile">
            <span v-if="loading" class="spinner-sm"></span>
            Aktualisieren
          </button>
          <button class="btn btn-secondary btn-sm" @click="copyFilteredLogs" :disabled="filteredEntries.length === 0">
            Kopieren
          </button>
        </div>
      </div>

      <div class="controls-row">
        <div class="field log-file-select">
          <label class="field-label" for="log-file-select">Log-Datei</label>
          <select id="log-file-select" class="field-select" v-model="selectedFile" :disabled="loadingFiles">
            <option v-if="loadingFiles" value="" disabled>Lade Dateien...</option>
            <option v-for="file in logFiles" :key="file.Name" :value="file.Name">
              {{ file.Name }} ({{ formatFileSize(file.Size) }}, {{ formatDate(file.DateModified) }})
            </option>
          </select>
        </div>

        <div class="filter-toggles">
          <label class="checkbox-field">
            <input type="checkbox" v-model="filterPluginOnly" />
            <span>Nur MediathekViewDL</span>
          </label>
          <label class="checkbox-field">
            <input type="checkbox" v-model="autoScroll" />
            <span>Auto-Scroll</span>
          </label>
          <label class="checkbox-field">
            <input type="checkbox" v-model="autoUpdate" />
            <span>Auto-Update (5s)</span>
          </label>
        </div>
      </div>

      <div class="search-row">
        <div class="field search-input-wrap">
          <input
            type="text"
            class="field-input"
            v-model="searchQuery"
            placeholder="Suchen..."
          />
          <button
            v-if="searchQuery"
            class="btn-icon search-clear"
            @click="searchQuery = ''"
            title="Suche löschen"
          >✕</button>
        </div>
        <label class="checkbox-field">
          <input type="checkbox" v-model="searchRegex" />
          <span>Regex</span>
        </label>
        <span class="entry-count">{{ filteredEntries.length }} Einträge</span>
        <span v-if="searchError" class="search-error">{{ searchError }}</span>
      </div>

      <div v-if="loadingFiles" class="state-msg">
        <div class="spinner"></div>
        Lade Log-Dateien...
      </div>
      <div v-else-if="error" class="error-msg">
        {{ error }}
      </div>
      <div v-else-if="logFiles.length === 0" class="no-data">
        Keine Log-Dateien gefunden.
      </div>
      <div v-else-if="loading && !rawContent" class="state-msg">
        <div class="spinner"></div>
        Lade Log-Inhalt...
      </div>
      <div v-else-if="filteredEntries.length === 0" class="no-data">
        {{ rawContent ? 'Keine passenden Log-Einträge gefunden.' : 'Log-Datei ist leer.' }}
      </div>
      <div v-else ref="logContainer" class="log-content">
        <div
          v-for="(entry, idx) in filteredEntries"
          :key="idx"
          :class="['log-entry', entry.levelClass, { 'entry-copied': copiedEntryIdx === idx }]"
          @dblclick="copyEntry(entry, idx)"
          title="Doppelklick zum Kopieren"
        >
          <template v-for="(line, lIdx) in entry.lines" :key="lIdx">
            <div class="log-line"><span v-if="lIdx > 0" class="log-continuation"></span>{{ line }}</div>
          </template>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.logs-tab { display: flex; flex-direction: column; gap: 20px; }

.header-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; }
.header-actions { display: flex; gap: 10px; }

.controls-row {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: 20px;
  margin-bottom: 15px;
}

.log-file-select { flex: 1; margin-bottom: 0; }

.filter-toggles {
  display: flex;
  align-items: center;
  gap: 15px;
  flex-shrink: 0;
  flex-wrap: wrap;
}

.filter-toggles .checkbox-field { margin-bottom: 0; }

.entry-count {
  font-size: 0.8rem;
  color: #71717a;
}

.search-row {
  display: flex;
  align-items: center;
  gap: 15px;
  margin-bottom: 15px;
}

.search-input-wrap {
  flex: 1;
  margin-bottom: 0;
  position: relative;
}

.search-clear {
  position: absolute;
  right: 8px;
  top: 50%;
  transform: translateY(-50%);
  font-size: 0.9rem;
  padding: 2px 4px;
}

.search-error {
  font-size: 0.8rem;
  color: #ef4444;
}

.log-content {
  background: #0a0a0a;
  border: 1px solid #27272a;
  border-radius: 6px;
  padding: 12px;
  max-height: 600px;
  overflow-y: auto;
  font-family: 'Cascadia Code', 'Fira Code', 'Consolas', monospace;
  font-size: 0.8rem;
  line-height: 1.5;
}

.log-entry {
  padding: 2px 4px;
  border-radius: 2px;
  cursor: pointer;
  position: relative;
}

.log-entry:hover { background: rgba(255, 255, 255, 0.03); }

.entry-copied {
  background: rgba(124, 58, 237, 0.15) !important;
}

.log-line {
  white-space: pre-wrap;
  word-break: break-all;
}

.log-continuation {
  display: inline-block;
  width: 16px;
}

.log-inf { color: #e4e4e7; }
.log-wrn { color: #f59e0b; }
.log-err { color: #ef4444; }
.log-dbg { color: #a1a1aa; }
.log-trc { color: #71717a; }
.log-vrb { color: #71717a; }

.spinner-sm {
  display: inline-block;
  width: 12px;
  height: 12px;
  border: 2px solid rgba(255, 255, 255, 0.2);
  border-radius: 50%;
  border-top-color: #e4e4e7;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-msg { text-align: center; padding: 30px; color: #ef4444; }
.no-data { text-align: center; color: #a1a1aa; padding: 30px; }
.state-msg { text-align: center; padding: 30px; color: #a1a1aa; }
</style>
