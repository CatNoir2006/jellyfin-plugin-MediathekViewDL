<script setup>
import { ref, onMounted } from 'vue'

const props = defineProps({
  onEdit: { type: Function, required: true }
})

const ApiClient = window.ApiClient ?? null
const Dashboard = window.Dashboard ?? null

const subscriptions = ref([])
const loading = ref(false)
const error = ref(null)

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

async function deleteSubscription(id) {
  if (!ApiClient || !Dashboard) return
  Dashboard.confirm('Soll dieses Abonnement wirklich gelöscht werden?', 'Löschen bestätigen', async (result) => {
    if (result) {
      try {
        const url = ApiClient.getUrl('MediathekViewDL/Subscriptions/' + id)
        await ApiClient.ajax({ type: 'DELETE', url })
        await fetchSubscriptions()
        Dashboard.alert('Abonnement gelöscht.')
      } catch (e) {
        console.error('Delete failed', e)
        Dashboard.alert('Fehler beim Löschen des Abonnements.')
      }
    }
  })
}

async function resetProcessedItems(id) {
  if (!ApiClient || !Dashboard) return
  Dashboard.confirm('Soll der Verlauf der bereits verarbeiteten Elemente für dieses Abonnement wirklich zurückgesetzt werden?', 'Zurücksetzen bestätigen', async (result) => {
    if (result) {
      try {
        const url = ApiClient.getUrl('MediathekViewDL/Subscriptions/' + id + '/ResetHistory')
        await ApiClient.ajax({ type: 'POST', url })
        Dashboard.alert('Verlauf wurde zurückgesetzt.')
        await fetchSubscriptions()
      } catch (e) {
        console.error('Reset failed', e)
        Dashboard.alert('Fehler beim Zurücksetzen.')
      }
    }
  })
}

async function processSubscription(id) {
  if (!ApiClient || !Dashboard) return
  try {
    const url = ApiClient.getUrl('MediathekViewDL/Subscriptions/' + id + '/Process')
    const response = await ApiClient.ajax({ type: 'POST', url })
    const reader = (response.body || response).getReader()
    const { value } = await reader.read()
    Dashboard.alert(new TextDecoder().decode(value) + ' neue Elemente gefunden.')
  } catch (e) {
    console.error('Processing failed', e)
    Dashboard.alert('Fehler beim Verarbeiten.')
  }
}

async function toggleActive(sub) {
  if (!ApiClient) return
  const newState = !sub.IsEnabled
  try {
    const url = ApiClient.getUrl('MediathekViewDL/Subscriptions/' + sub.Id + '/Active?active=' + newState)
    const response = await ApiClient.ajax({ type: 'POST', url })
    
    let result = response
    if (response && typeof response.text === 'function') {
      const text = await response.text()
      result = text === 'true'
    }
    
    sub.IsEnabled = result === true || result === 'true'
  } catch (e) {
    console.error('Toggle failed', e)
    if (Dashboard) Dashboard.alert('Fehler beim Ändern des Status.')
  }
}

onMounted(() => {
  fetchSubscriptions()
})

// Expose refresh to parent if needed
defineExpose({ refresh: fetchSubscriptions })
</script>

<template>
  <div class="card">
    <div class="header-row">
      <h2>Abo Verwaltung</h2>
      <button class="btn btn-primary" @click="onEdit()" :disabled="loading">Neues Abo</button>
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
        <div class="sub-left">
          <label class="switch" title="Abonnement aktivieren/deaktivieren">
            <input type="checkbox" :checked="sub.IsEnabled" @change="toggleActive(sub)">
            <span class="slider round"></span>
          </label>

          <div class="sub-info">
            <div class="sub-name">
              {{ sub.Name }}
            </div>
            <div class="sub-meta">
              Letzter Download: {{ sub.LastDownloadedTimestamp ? new Date(sub.LastDownloadedTimestamp).toLocaleString() : 'Nie' }}
            </div>
          </div>
        </div>
        <div class="sub-actions">
          <button @click="resetProcessedItems(sub.Id)" class="btn-icon" title="Verlauf zurücksetzen">↩️</button>
          <button @click="processSubscription(sub.Id)" class="btn-icon" title="Jetzt verarbeiten">🔄</button>
          <button @click="onEdit(sub)" class="btn-icon" title="Bearbeiten">✏️</button>
          <button @click="deleteSubscription(sub.Id)" class="btn-icon btn-delete" title="Löschen">🗑️</button>
        </div>
      </div>
    </div>
    
    <div v-else class="no-data">
      Keine Abonnements konfiguriert.
    </div>
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
}
.subscription-item.disabled { opacity: 0.6; border-style: dashed; }
.sub-left { display: flex; align-items: center; gap: 20px; }
.sub-name { font-weight: bold; font-size: 1.1rem; display: flex; align-items: center; gap: 10px; }
.sub-meta { font-size: 0.85rem; color: #a1a1aa; margin-top: 4px; }
.sub-actions { display: flex; gap: 15px; }
.btn-icon { background: none; border: none; cursor: pointer; font-size: 1.4rem; padding: 5px; border-radius: 4px; filter: grayscale(1); color: white; }
.btn-icon:hover { background: #3f3f46; filter: none; }
.btn-delete:hover { color: #ef4444; }
.state-msg { text-align: center; padding: 40px; color: #a1a1aa; }
.error-container { text-align: center; padding: 30px; background: rgba(239, 68, 68, 0.1); border: 1px solid #ef4444; border-radius: 8px; color: #ef4444; }
.error-msg { margin-bottom: 10px; font-weight: bold; }
.no-data { text-align: center; color: #a1a1aa; padding: 40px; }

/* Switch Toggle Styles */
.switch {
  position: relative;
  display: inline-block;
  width: 44px;
  height: 24px;
}
.switch input { opacity: 0; width: 0; height: 0; }
.slider {
  position: absolute;
  cursor: pointer;
  top: 0; left: 0; right: 0; bottom: 0;
  background-color: #3f3f46;
  transition: .4s;
}
.slider:before {
  position: absolute;
  content: "";
  height: 18px; width: 18px;
  left: 3px; bottom: 3px;
  background-color: white;
  transition: .4s;
}
input:checked + .slider { background-color: #7c3aed; }
input:focus + .slider { box-shadow: 0 0 1px #7c3aed; }
input:checked + .slider:before { transform: translateX(20px); }
.slider.round { border-radius: 24px; }
.slider.round:before { border-radius: 50%; }

</style>
