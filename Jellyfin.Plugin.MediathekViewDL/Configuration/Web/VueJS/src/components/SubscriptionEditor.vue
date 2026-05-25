<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  subscription: {
    type: Object,
    default: null
  }
})

const emit = defineEmits(['save', 'cancel'])

const editedSub = ref(null)
const activeTab = ref('basic')

const ApiClient = window.ApiClient ?? null
const Dashboard = window.Dashboard ?? null

watch(() => props.subscription, (newVal) => {
  if (newVal) {
    // Deep copy
    editedSub.value = JSON.parse(JSON.stringify(newVal))
  } else {
    editedSub.value = null
  }
}, { immediate: true })

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
      </div>

      <div class="editor-content">
        <!-- Basic Tab -->
        <div v-if="activeTab === 'basic'" class="tab-pane">
          <div class="field">
            <label>Name</label>
            <input v-model="editedSub.Name" type="text" class="field-input" placeholder="z.B. Tatort">
          </div>
          <div class="checkbox-field">
            <label>
              <input v-model="editedSub.IsEnabled" type="checkbox"> Aktiviert
            </label>
          </div>
          <div class="checkbox-field">
            <label>
              <input v-model="editedSub.IgnoreLocalFiles" type="checkbox"> Lokale Dateien ignorieren
            </label>
          </div>
          <div class="checkbox-field">
            <label>
              <input v-model="editedSub.IgnoreHistory" type="checkbox"> Download-Verlauf ignorieren
            </label>
          </div>
        </div>

        <!-- Search Tab -->
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
                {{ f }}
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
              <label>Min. Dauer (Min)</label>
              <input v-model="editedSub.Search.MinDurationMinutes" type="number" class="field-input">
            </div>
            <div class="field">
              <label>Max. Dauer (Min)</label>
              <input v-model="editedSub.Search.MaxDurationMinutes" type="number" class="field-input">
            </div>
          </div>
        </div>

        <!-- Download Tab -->
        <div v-if="activeTab === 'download'" class="tab-pane">
          <div class="field">
            <label>Speicherpfad (optional)</label>
            <div class="input-with-btn">
              <input v-model="editedSub.Download.DownloadPath" type="text" class="field-input">
              <button @click="selectPath" class="btn btn-secondary">Wählen</button>
            </div>
            <p class="field-desc">Leer lassen, um den globalen Standardpfad zu verwenden.</p>
          </div>
          <div class="checkbox-field">
            <label>
              <input v-model="editedSub.Download.UseStreamingUrlFiles" type="checkbox"> .strm Dateien verwenden (kein Download)
            </label>
          </div>
          <div class="checkbox-field">
            <label>
              <input v-model="editedSub.Download.AlwaysCreateSubfolder" type="checkbox"> Immer Unterordner erstellen
            </label>
          </div>
          <div class="checkbox-field">
            <label>
              <input v-model="editedSub.Download.AllowFallbackToLowerQuality" type="checkbox"> Qualität-Fallback erlauben
            </label>
          </div>
        </div>

        <!-- Series Tab -->
        <div v-if="activeTab === 'series'" class="tab-pane">
          <div class="checkbox-field">
            <label>
              <input v-model="editedSub.Series.EnforceSeriesParsing" type="checkbox"> Serien-Erkennung erzwingen (S01E01)
            </label>
          </div>
          <div v-if="editedSub.Series.EnforceSeriesParsing" class="sub-options">
            <div class="checkbox-field">
              <label>
                <input v-model="editedSub.Series.AllowAbsoluteEpisodeNumbering" type="checkbox"> Absolute Episodennummerierung erlauben
              </label>
            </div>
          </div>
          <div v-else class="sub-options">
            <div class="checkbox-field">
              <label>
                <input v-model="editedSub.Series.TreatNonEpisodesAsExtras" type="checkbox"> Nicht erkannte Folgen als Extras behandeln
              </label>
            </div>
          </div>
        </div>

        <!-- Metadata Tab -->
        <div v-if="activeTab === 'metadata'" class="tab-pane">
          <div class="field">
            <label>Originalsprache (3-Letter ISO)</label>
            <input v-model="editedSub.Metadata.OriginalLanguage" type="text" class="field-input" placeholder="z.B. deu">
          </div>
          <div class="checkbox-field">
            <label>
              <input v-model="editedSub.Metadata.CreateNfo" type="checkbox"> .nfo Dateien erstellen
            </label>
          </div>
          <div class="checkbox-field">
            <label>
              <input v-model="editedSub.Metadata.AppendDateToTitle" type="checkbox"> Datum an Titel anhängen
            </label>
          </div>
          <div class="checkbox-field">
            <label>
              <input v-model="editedSub.Metadata.KeepOriginalTitle" type="checkbox"> Originaltitel beibehalten
            </label>
          </div>
        </div>
      </div>

      <footer class="editor-footer">
        <button @click="cancel" class="btn btn-secondary">Abbrechen</button>
        <button @click="save" class="btn btn-primary">Speichern</button>
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
  align-items: center; /* Back to center for stability */
  z-index: 9999;
  padding: 20px;
}
.editor-modal {
  width: 100%;
  max-width: 800px;
  height: 80vh; /* Fixed height relative to viewport */
  min-height: 500px; /* Ensure it doesn't get too small */
  display: flex;
  flex-direction: column;
  background: #18181b;
  border: 1px solid #3f3f46;
  border-radius: 8px;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.5);
  overflow: hidden; /* Prevent modal itself from scrolling */
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
.field { margin-bottom: 15px; }
.field label { display: block; margin-bottom: 5px; color: #a1a1aa; font-size: 0.9rem; }
.field-input {
  width: 100%;
  background: #27272a;
  border: 1px solid #3f3f46;
  color: white;
  padding: 10px;
  border-radius: 4px;
  box-sizing: border-box;
}
.checkbox-field { margin-bottom: 10px; }
.checkbox-field label { display: flex; align-items: center; gap: 10px; cursor: pointer; }
.grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
.query-row { 
  background: #27272a; 
  padding: 15px; 
  border-radius: 8px; 
  margin-bottom: 15px; 
  border: 1px solid #3f3f46;
}
.query-fields { display: flex; gap: 8px; margin-bottom: 10px; }
.field-tag { 
  padding: 4px 10px; 
  border-radius: 12px; 
  background: #3f3f46; 
  border: none; 
  color: #a1a1aa; 
  font-size: 0.75rem; 
  cursor: pointer;
}
.field-tag.active { background: #7c3aed; color: white; }
.query-input-row { display: flex; gap: 10px; align-items: center; }
.input-with-btn { display: flex; gap: 10px; }
.sub-options { margin-left: 25px; border-left: 2px solid #3f3f46; padding-left: 15px; margin-top: 10px; }
.btn-small { padding: 5px 10px; border-radius: 4px; border: 1px solid #3f3f46; background: #27272a; color: white; cursor: pointer; font-size: 0.75rem; }
.btn-danger { background: #ef4444; border-color: #ef4444; }
</style>
