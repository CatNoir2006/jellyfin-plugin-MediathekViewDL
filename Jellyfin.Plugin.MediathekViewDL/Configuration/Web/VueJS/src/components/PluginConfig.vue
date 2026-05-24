<script setup>
import { ref } from 'vue'
import SearchTab from './tabs/SearchTab.vue'
import SettingsTab from './tabs/SettingsTab.vue'
import SubscriptionsTab from './tabs/SubscriptionsTab.vue'
import DownloadsTab from './tabs/DownloadsTab.vue'

const currentTab = ref('search')
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
      <SearchTab v-if="currentTab === 'search'" />
      <SettingsTab v-if="currentTab === 'settings'" />
      <SubscriptionsTab v-if="currentTab === 'subscriptions'" />
      <DownloadsTab v-if="currentTab === 'downloads'" />
    </div>
  </div>
</template>

<style scoped>
.plugin-config { width: 100%; margin: 0 auto; padding: 1rem; color: #e4e4e7; box-sizing: border-box; }
.config-header { margin-bottom: 2rem; }
.tab-row { display: flex; gap: 10px; margin-bottom: 20px; border-bottom: 1px solid #333; padding-bottom: 10px; }
.tab-btn { background: none; border: none; color: #a1a1aa; cursor: pointer; padding: 10px; font-weight: 600; }
.tab-btn.active { color: #7c3aed; border-bottom: 2px solid #7c3aed; }
</style>
