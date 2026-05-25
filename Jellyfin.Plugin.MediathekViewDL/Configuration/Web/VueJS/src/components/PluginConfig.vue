<script setup>
import { ref, onMounted } from 'vue'
import SearchTab from './tabs/SearchTab.vue'
import SettingsTab from './tabs/SettingsTab.vue'
import SubscriptionsTab from './tabs/SubscriptionsTab.vue'
import DownloadsTab from './tabs/DownloadsTab.vue'
import SubscriptionEditor from './SubscriptionEditor.vue'

const ApiClient = window.ApiClient ?? null
const Dashboard = window.Dashboard ?? null
const PLUGIN_ID = 'a31b415a-5264-419d-b152-8c8192a54994'

const currentTab = ref('search')
const pluginConfig = ref(null)

// Subscription Editor State
const editingSub = ref(null)
const showTestModal = ref(false)
const testResults = ref([])
const testLoading = ref(false)

async function fetchConfig() {
  if (!ApiClient) return
  pluginConfig.value = await ApiClient.getPluginConfiguration(PLUGIN_ID)
}

function openEditor(subData = null) {
  if (subData) {
    editingSub.value = subData
  } else {
    // New subscription with defaults
    const def = pluginConfig.value?.SubscriptionDefaults || {}
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
    if (Dashboard) Dashboard.alert('Abonnement gespeichert.')
    // Notify child components to refresh (could use a key or ref)
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
    
    let finalArray = [];
    if (Array.isArray(results)) finalArray = results;
    else if (results && Array.isArray(results.Items)) finalArray = results.Items;
    else if (results && Array.isArray(results.data)) finalArray = results.data;
    
    testResults.value = finalArray;
  } catch (e) {
    console.error('Test failed', e)
    if (Dashboard) Dashboard.alert('Fehler beim Testen des Abonnements.')
    showTestModal.value = false
  } finally {
    testLoading.value = false
  }
}

onMounted(() => {
  fetchConfig()
})
</script>

<template>
  <div class="plugin-config">
    <header class="config-header">
      <h1 class="config-title">MediathekViewDL</h1>
      <p class="config-subtitle">Plugin Konfiguration</p>
    </header>

    <div class="tab-row">
      <button class="tab-btn" :class="{ active: currentTab === 'search' }" @click="currentTab = 'search'">Suche</button>
      <button class="tab-btn" :class="{ active: currentTab === 'settings' }" @click="currentTab = 'settings'">Einstellungen</button>
      <button class="tab-btn" :class="{ active: currentTab === 'subscriptions' }" @click="currentTab = 'subscriptions'">Abos</button>
      <button class="tab-btn" :class="{ active: currentTab === 'downloads' }" @click="currentTab = 'downloads'">Downloads</button>
    </div>

    <div class="tab-content">
      <SearchTab v-if="currentTab === 'search'" @create-sub="openEditor" />
      <SettingsTab v-if="currentTab === 'settings'" />
      <SubscriptionsTab v-if="currentTab === 'subscriptions'" :on-edit="openEditor" />
      <DownloadsTab v-if="currentTab === 'downloads'" />
    </div>

    <!-- Shared Subscription Editor -->
    <Teleport to="body">
      <SubscriptionEditor 
        :subscription="editingSub" 
        @save="saveSubscription" 
        @test="testSubscription"
        @cancel="editingSub = null"
      />
    </Teleport>

    <!-- Shared Test Results Modal -->
    <Teleport to="body">
      <div v-if="showTestModal" class="modal-overlay">
        <div class="modal-card test-modal card">
          <header class="modal-header">
            <h2>Abo-Test Ergebnisse</h2>
            <button @click="showTestModal = false" class="btn-icon">✕</button>
          </header>
          <div class="modal-content">
            <div v-if="testLoading" class="state-msg">
              <div class="spinner"></div>
              Suche nach Treffern...
            </div>
            <div v-else-if="testResults.length === 0" class="no-data">
              Keine Sendungen gefunden.
            </div>
            <div v-else class="test-results-list">
              <p>Folgende {{ testResults.length }} Sendungen würden heruntergeladen werden:</p>
              <div v-for="(item, idx) in testResults" :key="idx" class="test-item">
                <div class="test-item-title">{{ item.Title || item.title }}</div>
                <div class="test-item-meta">{{ item.Channel || item.channel }} | {{ item.Topic || item.topic }} | {{ item.Duration || item.duration }}</div>
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
.plugin-config { width: 100%; margin: 0 auto; padding: 1rem; color: #e4e4e7; box-sizing: border-box; }
.config-header { margin-bottom: 2rem; }
.tab-row { display: flex; gap: 10px; margin-bottom: 20px; border-bottom: 1px solid #333; padding-bottom: 10px; }
.tab-btn { background: none; border: none; color: #a1a1aa; cursor: pointer; padding: 10px; font-weight: 600; }
.tab-btn.active { color: #7c3aed; border-bottom: 2px solid #7c3aed; }

/* Shared Modal Styles */
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
  max-width: 800px;
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
.state-msg, .no-data { text-align: center; padding: 40px; color: #a1a1aa; }
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
.btn-icon { background: none; border: none; color: white; cursor: pointer; font-size: 1.2rem; }
.btn-primary { background: #7c3aed; color: white; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer; font-weight: 600; }
</style>
