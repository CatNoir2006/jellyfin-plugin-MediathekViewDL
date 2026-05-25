<script setup>
import { ref, onMounted } from 'vue'
import SubscriptionEditor from '../SubscriptionEditor.vue'

const ApiClient = window.ApiClient ?? null
const Dashboard = window.Dashboard ?? null

const subscriptions = ref([])
const loading = ref(false)
const error = ref(null)

const editingSub = ref(null)
const pluginConfig = ref(null)

const showTestModal = ref(false)
const testResults = ref([])
const testLoading = ref(false)

async function fetchSubscriptions() {
  if (!ApiClient) return
  loading.value = true
  error.value = null
  try {
    const url = ApiClient.getUrl('MediathekViewDL/Subscriptions')
    subscriptions.value = await ApiClient.getJSON(url)
  } catch (e) {
    error.value = 'Fehler beim Laden der Abonnements.'
    console.error('Failed to fetch subscriptions', e)
  } finally {
    loading.value = false
  }
}

async function fetchConfig() {
  if (!ApiClient) return
  const PLUGIN_ID = 'a31b415a-5264-419d-b152-8c8192a54994'
  pluginConfig.value = await ApiClient.getPluginConfiguration(PLUGIN_ID)
}

function createNewSubscription() {
  if (!pluginConfig.value) return

  const def = pluginConfig.value.SubscriptionDefaults || {}

  editingSub.value = {
    Name: '',
    IsEnabled: true,
    Search: {
      Criteria: [{ Fields: ['Title', 'Topic'], Query: '', IsExclude: false }],
      MinDurationMinutes: def.SearchSettings?.MinDurationMinutes || null,
      MaxDurationMinutes: def.SearchSettings?.MaxDurationMinutes || null,
      MinBroadcastDate: null,
      MaxBroadcastDate: null
    },
    Download: {
      DownloadPath: '',
      UseStreamingUrlFiles: def.DownloadSettings?.UseStreamingUrlFiles || false,
      AlwaysCreateSubfolder: def.DownloadSettings?.AlwaysCreateSubfolder || false,
      AllowFallbackToLowerQuality: def.DownloadSettings?.AllowFallbackToLowerQuality ?? true,
      EnhancedDuplicateDetection: def.DownloadSettings?.EnhancedDuplicateDetection || false,
      QualityCheckWithUrl: def.DownloadSettings?.QualityCheckWithUrl || false,
      DownloadFullVideoForSecondaryAudio: def.DownloadSettings?.DownloadFullVideoForSecondaryAudio || false
    },
    Series: {
      EnforceSeriesParsing: def.SeriesSettings?.EnforceSeriesParsing || false,
      AllowAbsoluteEpisodeNumbering: def.SeriesSettings?.AllowAbsoluteEpisodeNumbering || false,
      TreatNonEpisodesAsExtras: def.SeriesSettings?.TreatNonEpisodesAsExtras || false,
      SaveTrailers: def.SeriesSettings?.SaveTrailers ?? true,
      SaveInterviews: def.SeriesSettings?.SaveInterviews ?? true,
      SaveGenericExtras: def.SeriesSettings?.SaveGenericExtras ?? true,
      SaveExtrasAsStrm: def.SeriesSettings?.SaveExtrasAsStrm || false
    },
    Metadata: {
      OriginalLanguage: def.MetadataSettings?.OriginalLanguage || '',
      CreateNfo: def.MetadataSettings?.CreateNfo || false,
      AppendDateToTitle: def.MetadataSettings?.AppendDateToTitle || false,
      KeepOriginalTitle: def.MetadataSettings?.KeepOriginalTitle || false,
      AppendTimeToTitle: def.MetadataSettings?.AppendTimeToTitle || false
    },
    Accessibility: {
      AllowAudioDescription: def.AccessibilitySettings?.AllowAudioDescription || false,
      AllowSignLanguage: def.AccessibilitySettings?.AllowSignLanguage || false
    }
  }
}

function editSubscription(sub) {
  editingSub.value = sub
}

async function saveSubscription(sub) {
  if (!ApiClient) return

  try {
    const isNew = !sub.Id
    const url = isNew
      ? ApiClient.getUrl('MediathekViewDL/Subscriptions')
      : ApiClient.getUrl('MediathekViewDL/Subscriptions/' + sub.Id)

    await ApiClient.ajax({
      type: isNew ? 'POST' : 'PUT',
      url: url,
      data: JSON.stringify(sub),
      contentType: 'application/json'
    })

    editingSub.value = null
    await fetchSubscriptions()
    if (Dashboard) Dashboard.alert('Abonnement gespeichert.')
  } catch (e) {
    console.error('Save failed', e)
    if (Dashboard) Dashboard.alert('Fehler beim Speichern des Abonnements.')
  }
}

async function testSubscription(sub) {
  if (!ApiClient) return
  testResults.value = []
  testLoading.value = true
  showTestModal.value = true

  try {
    const url = ApiClient.getUrl('MediathekViewDL/Subscriptions/Test')
    const results = await ApiClient.ajax({
      type: 'POST',
      url: url,
      data: JSON.stringify(sub),
      contentType: 'application/json',
      dataType: 'json'
    })

    testResults.value = results;
    console.log('Final test results count:', testResults.value.length);
  } catch (e) {
    console.error('Test failed', e)
    if (Dashboard) Dashboard.alert('Fehler beim Testen des Abonnements.')
    showTestModal.value = false
  } finally {
    testLoading.value = false
  }
}

async function deleteSubscription(id) {
  if (!ApiClient || !Dashboard) return

  Dashboard.confirm('Soll dieses Abonnement wirklich gelöscht werden?', 'Löschen bestätigen', async (result) => {
    if (result) {
      try {
        const url = ApiClient.getUrl('MediathekViewDL/Subscriptions/' + id)
        await ApiClient.ajax({
          type: 'DELETE',
          url: url
        })
        await fetchSubscriptions()
        Dashboard.alert('Abonnement gelöscht.')
      } catch (e) {
        console.error('Delete failed', e)
        Dashboard.alert('Fehler beim Löschen des Abonnements.')
      }
    }
  })
}

async function processSubscription(id) {
  if (!ApiClient || !Dashboard) return
  try {
    const url = ApiClient.getUrl('MediathekViewDL/Subscriptions/' + id + '/Process')
    const count = await ApiClient.ajax({
      type: 'POST',
      url: url
    })
    Dashboard.alert(count + ' neue Elemente gefunden.')
  } catch (e) {
    console.error('Processing failed', e)
    Dashboard.alert('Fehler beim Verarbeiten.')
  }
}

onMounted(() => {
  fetchSubscriptions()
  fetchConfig()
})
</script>

<template>
  <div class="card">
    <div class="header-row">
      <h2>Abo Verwaltung</h2>
      <button class="btn btn-primary" @click="createNewSubscription" :disabled="loading">Neues Abo</button>
    </div>

    <div v-if="loading" class="state-msg">
      <div class="spinner"></div>
      Lade Abonnements...
    </div>

    <div v-else-if="error" class="error-container">
      <div class="error-msg">{{ error }}</div>
      <button @click="fetchSubscriptions" class="btn btn-secondary">Erneut versuchen</button>
    </div>

    <div v-else-if="subscriptions.length > 0" class="subscriptions-list">
      <div v-for="sub in subscriptions" :key="sub.Id" class="subscription-item" :class="{ disabled: !sub.IsEnabled }">
        <div class="sub-info">
          <div class="sub-name">
            {{ sub.Name }}
            <span v-if="!sub.IsEnabled" class="badge">Deaktiviert</span>
          </div>
          <div class="sub-meta">
            Letzter Download: {{ sub.LastDownloadedTimestamp ? new Date(sub.LastDownloadedTimestamp).toLocaleString() : 'Nie' }}
          </div>
        </div>
        <div class="sub-actions">
          <button @click="processSubscription(sub.Id)" class="btn-icon" title="Jetzt verarbeiten">🔄</button>
          <button @click="editSubscription(sub)" class="btn-icon" title="Bearbeiten">✏️</button>
          <button @click="deleteSubscription(sub.Id)" class="btn-icon btn-delete" title="Löschen">🗑️</button>
        </div>
      </div>
    </div>

    <div v-else class="no-data">
      Keine Abonnements konfiguriert.
    </div>

    <!-- Modal Editor -->
    <Teleport to="body">
      <SubscriptionEditor
        :subscription="editingSub"
        @save="saveSubscription"
        @test="testSubscription"
        @cancel="editingSub = null"
      />
    </Teleport>

    <!-- Modal Test Results -->
    <Teleport to="body">
      <div v-if="showTestModal" class="modal-overlay test-overlay">
        <div class="modal-card test-modal card">
          <header class="modal-header">
            <h2>Abo-Test Ergebnisse</h2>
            <button @click="showTestModal = false" class="btn-icon">✕</button>
          </header>
          <div class="modal-content">
            <div v-if="testLoading" class="loading-results">
              <div class="spinner"></div>
              Suche nach Treffern...
            </div>
            <div v-else-if="testResults.length === 0" class="no-hits">
              Keine Sendungen gefunden, die den Kriterien entsprechen und noch nicht geladen wurden.
            </div>
            <div v-else class="test-results-list">
              <p>Folgende {{ testResults.length }} Sendungen würden heruntergeladen werden:</p>
              <div v-for="item in testResults" :key="item.Id || item.id" class="test-item">
                <div class="test-item-title">{{ item.Title || item.title }}</div>
                <div class="test-item-meta">
                  {{ item.Channel || item.channel }} | {{ item.Topic || item.topic }} | {{ item.Duration || item.duration }}
                </div>
                <div v-if="item.Description || item.description" class="test-item-desc">
                  {{ item.Description || item.description }}
                </div>
              </div>
            </div>
          </div>
          <footer class="modal-footer">
            <button @click="showTestModal = false" class="btn btn-primary">Schließen</button>
          </footer>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.header-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.subscriptions-list { display: grid; gap: 10px; }
.subscription-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px;
  background: #27272a;
  border: 1px solid #3f3f46;
  border-radius: 8px;
}
.subscription-item.disabled { opacity: 0.6; border-style: dashed; }
.sub-name { font-weight: bold; font-size: 1.1rem; display: flex; align-items: center; gap: 10px; }
.sub-meta { font-size: 0.85rem; color: #a1a1aa; margin-top: 4px; }
.badge { background: #3f3f46; color: #e4e4e7; padding: 2px 8px; border-radius: 4px; font-size: 0.7rem; font-weight: normal; }
.sub-actions { display: flex; gap: 15px; }
.btn-icon { background: none; border: none; cursor: pointer; font-size: 1.4rem; padding: 5px; border-radius: 4px; filter: grayscale(1); color: white; }
.btn-icon:hover { background: #3f3f46; filter: none; }
.btn-delete:hover { color: #ef4444; }
.btn-primary { background: #7c3aed; color: white; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer; font-weight: 600; }
.btn-secondary { background: #3f3f46; color: white; border: none; padding: 8px 16px; border-radius: 4px; cursor: pointer; margin-top: 10px; }
.state-msg { text-align: center; padding: 40px; color: #a1a1aa; }
.error-container { text-align: center; padding: 30px; background: rgba(239, 68, 68, 0.1); border: 1px solid #ef4444; border-radius: 8px; color: #ef4444; }
.error-msg { margin-bottom: 10px; font-weight: bold; }
.no-data { text-align: center; color: #a1a1aa; padding: 40px; background: #27272a; border-radius: 8px; border: 1px dashed #3f3f46; }

/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.95);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 10000;
  padding: 20px;
}
.modal-card {
  width: 100%;
  max-width: 700px;
  max-height: 80vh;
  display: flex;
  flex-direction: column;
  background: #18181b;
  border: 1px solid #3f3f46;
  border-radius: 8px;
  overflow: hidden;
}
.modal-header { padding: 20px; border-bottom: 1px solid #3f3f46; display: flex; justify-content: space-between; align-items: center; }
.modal-content { padding: 20px; overflow-y: auto; flex: 1; }
.modal-footer { padding: 20px; border-top: 1px solid #3f3f46; display: flex; justify-content: flex-end; }

.test-item { padding: 12px; border-bottom: 1px solid #333; }
.test-item-title { font-weight: bold; margin-bottom: 2px; }
.test-item-meta { font-size: 0.8rem; color: #a1a1aa; }
.test-item-desc { font-size: 0.75rem; color: #71717a; margin-top: 4px; }
.loading-results, .no-hits { text-align: center; padding: 40px; color: #a1a1aa; }

.spinner {
  border: 3px solid rgba(255, 255, 255, 0.1);
  border-radius: 50%;
  border-top: 3px solid #7c3aed;
  width: 24px;
  height: 24px;
  animation: spin 1s linear infinite;
  margin: 0 auto 10px;
}
@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
</style>
