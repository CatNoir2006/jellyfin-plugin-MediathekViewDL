<script setup>
import { ref, onMounted } from 'vue'

const ApiClient = window.ApiClient ?? null
const PLUGIN_ID = 'a31b415a-5264-419d-b152-8c8192a54994'

const activeWebUi = ref('VueJS')
const loading = ref(false)

onMounted(async () => {
  if (!ApiClient) return
  const config = await ApiClient.getPluginConfiguration(PLUGIN_ID)
  activeWebUi.value = config.ActiveWebUi ?? 'VueJS'
})

async function saveConfig() {
  loading.value = true
  try {
    const config = await ApiClient.getPluginConfiguration(PLUGIN_ID)
    config.ActiveWebUi = activeWebUi.value
    await ApiClient.updatePluginConfiguration(PLUGIN_ID, config)
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="card">
    <h2>Einstellungen</h2>
    <form @submit.prevent="saveConfig">
      <div class="field">
        <label class="field-label" for="optActiveWebUi">Aktive WebUI</label>
        <select id="optActiveWebUi" v-model="activeWebUi" class="field-select">
          <option value="VueJS">VueJS</option>
          <option value="ShowBoth">Beide anzeigen</option>
          <option value="Html">HTML Legacy</option>
        </select>
      </div>
      <button type="submit" class="btn btn-save" :disabled="loading">Speichern</button>
    </form>
  </div>
</template>
