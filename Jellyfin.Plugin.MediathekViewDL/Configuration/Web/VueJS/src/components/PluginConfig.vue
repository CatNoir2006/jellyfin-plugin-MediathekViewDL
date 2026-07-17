<script setup>
import { ref, computed, onMounted } from 'vue'
import ApiService from '../utils/ApiService'
import { SubscriptionFactory } from '../utils/SubscriptionFactory'
import SearchTab from './tabs/SearchTab.vue'
import SettingsTab from './tabs/SettingsTab.vue'
import SubscriptionsTab from './tabs/SubscriptionsTab.vue'
import DownloadsTab from './tabs/DownloadsTab.vue'
import LogsTab from './tabs/LogsTab.vue'
import SubscriptionEditor from './SubscriptionEditor.vue'
import SetupWizard from './SetupWizard.vue'

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

// Setup Wizard State
const showWizard = ref(false)
const wizardRef = ref(null)
const wizardAutoTriggered = ref(false)

async function fetchConfig() {
  if (!ApiClient) return
  pluginConfig.value = await ApiClient.getPluginConfiguration(PLUGIN_ID)
  maybeAutoShowWizard()
}

function isPathsEmpty(paths) {
  if (!paths) return true
  return !paths.DefaultDownloadPath &&
         !paths.DefaultSubscriptionShowPath &&
         !paths.DefaultSubscriptionMoviePath &&
         !paths.DefaultManualShowPath &&
         !paths.DefaultManualMoviePath &&
         !paths.TempDownloadPath &&
         !paths.UseTopicForMoviePath
}

const isFreshInstall = computed(() => {
  const cfg = pluginConfig.value
  if (!cfg) return false
  if (cfg.WizardCompleted) return false
  if (!isPathsEmpty(cfg.Paths)) return false
  if (cfg.Subscriptions && cfg.Subscriptions.length > 0) return false
  return true
})

function maybeAutoShowWizard() {
  if (wizardAutoTriggered.value) return
  if (isFreshInstall.value) {
    wizardAutoTriggered.value = true
    showWizard.value = true
  }
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

async function persistWizardResult(payload = {}) {
  const { skipped, paths, defaults } = payload
  try {
    // Refresh config first so any subscription created in the wizard is included.
    // updatePluginConfig replaces the full config on the server, so we must
    // send the up-to-date Subscriptions collection.
    await fetchConfig()
    const cfg = pluginConfig.value ? { ...pluginConfig.value } : {}
    if (paths) {
      cfg.Paths = cfg.Paths || {}
      if (paths.DefaultSubscriptionShowPath !== undefined) cfg.Paths.DefaultSubscriptionShowPath = paths.DefaultSubscriptionShowPath
      if (paths.DefaultSubscriptionMoviePath !== undefined) cfg.Paths.DefaultSubscriptionMoviePath = paths.DefaultSubscriptionMoviePath
      if (paths.DefaultManualShowPath !== undefined) cfg.Paths.DefaultManualShowPath = paths.DefaultManualShowPath
      if (paths.DefaultManualMoviePath !== undefined) cfg.Paths.DefaultManualMoviePath = paths.DefaultManualMoviePath
      if (paths.TempDownloadPath !== undefined) cfg.Paths.TempDownloadPath = paths.TempDownloadPath
    }
    if (defaults) {
      cfg.SubscriptionDefaults = cfg.SubscriptionDefaults || {}
      cfg.SubscriptionDefaults.DownloadSettings = cfg.SubscriptionDefaults.DownloadSettings || {}
      if (defaults.UseStreamingUrlFiles !== undefined) {
        cfg.SubscriptionDefaults.DownloadSettings.UseStreamingUrlFiles = defaults.UseStreamingUrlFiles
      }
    }
    cfg.WizardCompleted = true
    await ApiService.updatePluginConfig(PLUGIN_ID, cfg)
    // Refresh local state so the wizard doesn't auto-open again on next mount
    if (pluginConfig.value) {
      pluginConfig.value.WizardCompleted = true
      if (cfg.Paths) pluginConfig.value.Paths = { ...pluginConfig.value.Paths, ...cfg.Paths }
      if (cfg.SubscriptionDefaults) {
        pluginConfig.value.SubscriptionDefaults = pluginConfig.value.SubscriptionDefaults || {}
        pluginConfig.value.SubscriptionDefaults.DownloadSettings = {
          ...(pluginConfig.value.SubscriptionDefaults.DownloadSettings || {}),
          ...(cfg.SubscriptionDefaults.DownloadSettings || {})
        }
      }
    }
  } catch (e) {
    console.error('Failed to persist wizard state', e)
    if (Dashboard) Dashboard.alert('Fehler beim Speichern des Assistenten-Status.')
  }
  showWizard.value = false
  // notify subscriptions tab in case wizard created one
  if (subscriptionsTabRef.value) {
    subscriptionsTabRef.value.refresh()
  }
  // Avoid unused-var lint
  void skipped
}

async function openWizardManually() {
  showWizard.value = true
}

onMounted(() => {
  fetchConfig()
})
</script>

<template>
  <div class="plugin-config">
    <header class="config-header">
      <h1 class="config-title">MediathekViewDL</h1>
      <button class="btn btn-secondary btn-sm wizard-restart-btn" @click="openWizardManually"
        title="Einrichtungs-Assistenten erneut starten" data-testid="wizard-restart-btn">
        🧙 Einrichtungs-Assistent
      </button>
    </header>

    <div class="tab-row">
      <button class="tab-btn" :class="{ active: currentTab === 'search' }" @click="currentTab = 'search'">Suche</button>
      <button class="tab-btn" :class="{ active: currentTab === 'settings' }" @click="currentTab = 'settings'">Einstellungen</button>
      <button class="tab-btn" :class="{ active: currentTab === 'subscriptions' }" @click="currentTab = 'subscriptions'">Abos</button>
      <button class="tab-btn" :class="{ active: currentTab === 'downloads' }" @click="currentTab = 'downloads'">Downloads</button>
      <button class="tab-btn" :class="{ active: currentTab === 'logs' }" @click="currentTab = 'logs'">Logs</button>
    </div>

    <div class="tab-content">
      <SearchTab v-if="currentTab === 'search'" @create-sub="openEditor" :plugin-config="pluginConfig" />
      <SettingsTab v-if="currentTab === 'settings'" @config-saved="fetchConfig" />
      <SubscriptionsTab ref="subscriptionsTabRef" v-if="currentTab === 'subscriptions'" :on-edit="openEditor" />
      <DownloadsTab v-if="currentTab === 'downloads'" />
      <LogsTab v-if="currentTab === 'logs'" />
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

    <!-- Setup Wizard -->
    <SetupWizard ref="wizardRef" :open="showWizard" :plugin-config="pluginConfig"
      @close="persistWizardResult" @subscription-created="() => subscriptionsTabRef?.refresh?.()" />

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
.config-header {
  margin-bottom: 2rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
}
.wizard-restart-btn {
  white-space: nowrap;
}
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
