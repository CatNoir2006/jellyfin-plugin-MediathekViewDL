<script setup>
import { ref, onMounted } from 'vue'
import ApiService from '../utils/ApiService'
import { SubscriptionFactory } from '../utils/SubscriptionFactory'
import SearchTab from './tabs/SearchTab.vue'
import SettingsTab from './tabs/SettingsTab.vue'
import SubscriptionsTab from './tabs/SubscriptionsTab.vue'
import DownloadsTab from './tabs/DownloadsTab.vue'
import SubscriptionEditor from './SubscriptionEditor.vue'

const Dashboard = window.Dashboard ?? null
const PLUGIN_ID = 'a31b415a-5264-419d-b152-8c8192a54994'

const currentTab = ref('search')
const pluginConfig = ref(null)

// Subscription Editor State
const editingSub = ref(null)
const showTestModal = ref(false)
const testResults = ref([])
const testLoading = ref(false)
const subscriptionsTabRef = ref(null)

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
    editingSub.value = SubscriptionFactory.createDefault(def)
  }
}

async function saveSubscription(sub) {
  try {
     await ApiService.saveSubscription(sub)
     editingSub.value = null
     if (Dashboard) Dashboard.alert('Abonnement gespeichert.')
     // Refresh subscriptions tab
     if (subscriptionsTabRef.value) {
       subscriptionsTabRef.value.refresh()
     }
  } catch (e) {
    console.error('Save failed', e)
    if (Dashboard) Dashboard.alert('Fehler beim Speichern des Abonnements.')
  }
}

async function testSubscription(sub) {
  testResults.value = []
  testLoading.value = true
  showTestModal.value = true
  try {
    const results = await ApiService.testSubscription(sub)

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
    </header>

    <div class="tab-row">
      <button class="tab-btn" :class="{ active: currentTab === 'search' }" @click="currentTab = 'search'">Suche</button>
      <button class="tab-btn" :class="{ active: currentTab === 'settings' }" @click="currentTab = 'settings'">Einstellungen</button>
      <button class="tab-btn" :class="{ active: currentTab === 'subscriptions' }" @click="currentTab = 'subscriptions'">Abos</button>
      <button class="tab-btn" :class="{ active: currentTab === 'downloads' }" @click="currentTab = 'downloads'">Downloads</button>
    </div>

    <div class="tab-content">
      <SearchTab v-if="currentTab === 'search'" @create-sub="openEditor" :plugin-config="pluginConfig" />
      <SettingsTab v-if="currentTab === 'settings'" @config-saved="fetchConfig" />
      <SubscriptionsTab ref="subscriptionsTabRef" v-if="currentTab === 'subscriptions'" :on-edit="openEditor" />
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
                <div class="test-item-title">{{ item.Title }}</div>
                  <div class="test-item-meta">{{ item.Channel }} | {{ item.Topic }} | {{ item.Duration }}</div>
                  <div class="test-item-meta">{{ item.Description }}</div>
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
  overflow: hidden;
}
.modal-header { padding: 20px; border-bottom: 1px solid #3f3f46; display: flex; justify-content: space-between; align-items: center; }
.modal-content { padding: 20px; overflow-y: auto; flex: 1; }
.modal-footer { padding: 20px; border-top: 1px solid #3f3f46; display: flex; justify-content: flex-end; }
.test-item { padding: 12px; border-bottom: 1px solid #333; }
.test-item-title { font-weight: bold; margin-bottom: 2px; }
.test-item-meta { font-size: 0.8rem; color: #a1a1aa; }
.state-msg, .no-data { text-align: center; padding: 40px; color: #a1a1aa; }
@keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
</style>
