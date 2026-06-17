<script setup>
import {ref, watch, computed, nextTick} from 'vue'
import { SubscriptionFactory } from '../../utils/SubscriptionFactory'
import { MS_PER_DAY_MINUS_ONE } from '../../utils/Constants'
import AdvancedDownloadDialog from '../AdvancedDownloadDialog.vue'
import ApiService from '../../utils/ApiService'

const props = defineProps({
    onCreateSub: { type: Function, required: true },
    pluginConfig: { type: Object, default: null }
})

const Dashboard = window.Dashboard ?? null

const searchTitle = ref('')
const searchTopic = ref('')
const searchChannel = ref('')
const searchCombined = ref('')
const minDuration = ref(null)
const maxDuration = ref(null)
const minBroadcastDate = ref(null)
const maxBroadcastDate = ref(null)

const results = ref([])
const loading = ref(false)

const availableChannels = ref([])
const availableTopics = ref([])

const showTopicSuggestions = ref(false)
const showChannelSuggestions = ref(false)
const selectedSuggestionIndex = ref(-1)
const topicSuggestionsRef = ref(null)
const channelSuggestionsRef = ref(null)

const filteredTopics = computed(() => {
    const rawParts = searchTopic.value.split(',')
    const lastPart = rawParts[rawParts.length - 1]
    const query = lastPart.trim().toLowerCase()
    const existing = new Set(rawParts.map(p => p.trim().toLowerCase()).filter(p => p))
    
    let baseList = availableTopics.value.filter(t => !existing.has(t.toLowerCase()))

    // If it ends with comma or is empty but focused, or we have a query
    const shouldShowAll = searchTopic.value.endsWith(',') || searchTopic.value.endsWith(', ')
    
    if (shouldShowAll) {
        return baseList.slice(0, 15)
    }
    
    if (!query) return []
    
    return baseList
        .filter(t => t.toLowerCase().includes(query))
        .slice(0, 15)
})

const filteredChannels = computed(() => {
    const rawParts = searchChannel.value.split(',')
    const lastPart = rawParts[rawParts.length - 1]
    const query = lastPart.trim().toLowerCase()
    const existing = new Set(rawParts.map(p => p.trim().toLowerCase()).filter(p => p))
    
    let baseList = availableChannels.value.filter(c => !existing.has(c.toLowerCase()))

    const shouldShowAll = searchChannel.value.endsWith(',') || searchChannel.value.endsWith(', ')

    if (shouldShowAll) {
        return baseList.slice(0, 15)
    }

    if (!query) return []
    
    return baseList
        .filter(c => c.toLowerCase().includes(query))
        .slice(0, 15)
})

async function scrollToSelected(field) {
    await nextTick()
    const container = field === 'topic' ? topicSuggestionsRef.value : channelSuggestionsRef.value
    if (!container) return
    
    const selectedItem = container.querySelector('.selected')
    if (selectedItem) {
        selectedItem.scrollIntoView({ block: 'nearest' })
    }
}

function onKeyDown(e, field) {
    const suggestions = field === 'topic' ? filteredTopics.value : filteredChannels.value
    
    if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
        if (field === 'topic') showTopicSuggestions.value = true
        else showChannelSuggestions.value = true
    }

    const isVisible = field === 'topic' ? showTopicSuggestions.value : showChannelSuggestions.value
    if (!isVisible || suggestions.length === 0) return

    if (e.key === 'ArrowDown') {
        e.preventDefault()
        selectedSuggestionIndex.value = (selectedSuggestionIndex.value + 1) % suggestions.length
        scrollToSelected(field)
    } else if (e.key === 'ArrowUp') {
        e.preventDefault()
        selectedSuggestionIndex.value = (selectedSuggestionIndex.value - 1 + suggestions.length) % suggestions.length
        scrollToSelected(field)
    } else if (e.key === 'Enter' || e.key === 'Tab') {
        if (selectedSuggestionIndex.value >= 0) {
            e.preventDefault()
            selectSuggestion(field, suggestions[selectedSuggestionIndex.value])
        } else if (suggestions.length > 0 && (e.key === 'Tab' || e.key === 'Enter')) {
             // Optional: auto-select first one on tab if nothing selected? 
             // Better keep it explicit for now.
        }
    } else if (e.key === 'Escape') {
        showTopicSuggestions.value = false
        showChannelSuggestions.value = false
    }
}

function onBlur(field) {
    // Timeout to allow mousedown to fire on suggestion
    setTimeout(() => {
        if (field === 'topic') showTopicSuggestions.value = false
        else showChannelSuggestions.value = false
    }, 200)
}

function selectSuggestion(field, value) {
    if (field === 'topic') {
        const parts = searchTopic.value.split(',')
        parts.pop() // remove the current partial query
        parts.push(value)
        searchTopic.value = parts.map(p => p.trim()).filter(p => p).join(', ') + ', '
        showTopicSuggestions.value = true
    } else if (field === 'channel') {
        const parts = searchChannel.value.split(',')
        parts.pop()
        parts.push(value)
        searchChannel.value = parts.map(p => p.trim()).filter(p => p).join(', ') + ', '
        showChannelSuggestions.value = true
    }
    selectedSuggestionIndex.value = -1
}

// Download dialog state
const showAdvancedDownload = ref(false)
const selectedItemForDownload = ref(null)
const isDownloading = ref(false)

let debounceTimer = null;

async function performSearch() {
    if (!searchTitle.value && !searchTopic.value && !searchChannel.value && !searchCombined.value) {
        results.value = [];
        return
    }

    loading.value = true
    try {
        const filters = {
            title: searchTitle.value,
            topic: searchTopic.value,
            channel: searchChannel.value,
            combinedSearch: searchCombined.value,
            minDuration: minDuration.value ? minDuration.value * 60 : null,
            maxDuration: maxDuration.value ? maxDuration.value * 60 : null,
            minBroadcastDate: minBroadcastDate.value ? new Date(minBroadcastDate.value).toISOString() : null,
            maxBroadcastDate: maxBroadcastDate.value
                ? new Date(new Date(maxBroadcastDate.value).getTime() + MS_PER_DAY_MINUS_ONE).toISOString()
                : null
        }

        results.value = await ApiService.search(filters)
    } catch (e) {
        console.error('Search failed', e)
        if (Dashboard) Dashboard.alert('Fehler bei der Suche: ' + (e?.message || 'Unbekannter Fehler'))
    } finally {
        loading.value = false
    }
}

async function loadAutocompleteData() {
    try {
        const [channels, topics] = await Promise.all([
            ApiService.getChannels(),
            ApiService.getTopics()
        ])
        availableChannels.value = channels || []
        availableTopics.value = topics || []
    } catch (e) {
        console.error('Failed to load autocomplete data', e)
    }
}

loadAutocompleteData()

function debouncedSearch() {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
        performSearch();
    }, 500);
}

// Watch all search inputs for changes
watch([searchTitle, searchTopic, searchChannel, searchCombined, minDuration, maxDuration, minBroadcastDate, maxBroadcastDate], () => {
    if (!searchTitle.value && !searchTopic.value && !searchChannel.value && !searchCombined.value) {
        results.value = [];
        return;
    }
    debouncedSearch();
});

function openVideo(item) {
    const bestVideo = [...item.VideoUrls].sort((a, b) => (b.Quality || 0) - (a.Quality || 0))[0]
    if (bestVideo?.Url) {
        window.open(bestVideo.Url, '_blank')
    }
}

async function createSubFromSearch() {
     try {
         const params = {
             title: searchTitle.value,
             topic: searchTopic.value,
             channel: searchChannel.value,
             combinedSearch: searchCombined.value
         }

         const criteria = await ApiService.getSearchCriteria(params)

         const defaults = props.pluginConfig?.SubscriptionDefaults || {}
         const sub = SubscriptionFactory.createDefault(defaults)
         sub.Name = searchTitle.value || searchTopic.value || searchCombined.value || 'Suche'
         sub.Search.Criteria = criteria
         sub.Search.MinDurationMinutes = minDuration.value
         sub.Search.MaxDurationMinutes = maxDuration.value
         sub.Search.MinBroadcastDate = minBroadcastDate.value ? new Date(minBroadcastDate.value).toISOString() : null
         sub.Search.MaxBroadcastDate = maxBroadcastDate.value ? new Date(maxBroadcastDate.value).toISOString() : null

         props.onCreateSub(sub);
     } catch (e) {
         console.error('Failed to convert criteria', e)
         if (Dashboard) Dashboard.alert('Fehler beim Erstellen des Abos: ' + (e?.message || 'Unbekannter Fehler'))
     }
}

async function createSubFromItem(item) {
      try {
          const params = {
              title: item.Title,
              topic: item.Topic,
              channel: item.Channel
          }

          const criteria = await ApiService.getSearchCriteria(params)

          const defaults = props.pluginConfig?.SubscriptionDefaults || {}
          const sub = SubscriptionFactory.createDefault(defaults)
          sub.Name = item.Topic || item.Title
          sub.Search.Criteria = criteria

          props.onCreateSub(sub);
      } catch (e) {
          console.error('Failed to convert item criteria', e)
          if (Dashboard) Dashboard.alert('Fehler beim Erstellen des Abos: ' + (e?.message || 'Unbekannter Fehler'))
      }
  }

async function simpleDownload(item) {
      try {
          isDownloading.value = true
          await ApiService.downloadItem(item)
          if (Dashboard) Dashboard.alert('Download erfolgreich in Warteschlange eingereiht.')
      } catch (e) {
          console.error('Simple download failed', e)
          if (Dashboard) Dashboard.alert('Fehler beim Starten des Downloads: ' + (e?.message || 'Unbekannter Fehler'))
      } finally {
          isDownloading.value = false
      }
}

function openAdvancedDownloadDialog(item) {
      selectedItemForDownload.value = item
      showAdvancedDownload.value = true
}

function closeAdvancedDownloadDialog() {
      showAdvancedDownload.value = false
      selectedItemForDownload.value = null
}
</script>

<template>
    <div class="card">
        <h2>Suche</h2>
        <form @submit.prevent="performSearch" class="search-form">
            <div class="search-grid">
                <div class="field">
                    <label>Titel</label>
                    <input v-model="searchTitle" type="text" class="field-input" placeholder="Titel der Sendung">
                </div>
                <div class="field autocomplete-wrapper">
                    <label>Thema</label>
                    <input v-model="searchTopic" type="text" class="field-input" placeholder="Thema / Sendereihe" 
                        @focus="showTopicSuggestions = true; selectedSuggestionIndex = -1" 
                        @blur="onBlur('topic')"
                        @keydown="onKeyDown($event, 'topic')">
                    <ul v-if="showTopicSuggestions && filteredTopics.length > 0" ref="topicSuggestionsRef" class="suggestions-list" tabindex="-1">
                        <li v-for="(topic, index) in filteredTopics" :key="topic" 
                            :class="{ selected: index === selectedSuggestionIndex }"
                            @mousedown.prevent="selectSuggestion('topic', topic)"
                            tabindex="-1">
                            {{ topic }}
                        </li>
                    </ul>
                </div>
                <div class="field autocomplete-wrapper">
                    <label>Sender</label>
                    <input v-model="searchChannel" type="text" class="field-input" placeholder="z.B. ARD, ZDF" 
                        @focus="showChannelSuggestions = true; selectedSuggestionIndex = -1" 
                        @blur="onBlur('channel')"
                        @keydown="onKeyDown($event, 'channel')">
                    <ul v-if="showChannelSuggestions && filteredChannels.length > 0" ref="channelSuggestionsRef" class="suggestions-list" tabindex="-1">
                        <li v-for="(channel, index) in filteredChannels" :key="channel" 
                            :class="{ selected: index === selectedSuggestionIndex }"
                            @mousedown.prevent="selectSuggestion('channel', channel)"
                            tabindex="-1">
                            {{ channel }}
                        </li>
                    </ul>
                </div>
                <div class="field">
                    <label>Kombinierte Suche</label>
                    <input v-model="searchCombined" type="text" class="field-input" placeholder="Sucht in Titel und Thema">
                </div>

                <div class="field">
                    <label>Min. Dauer (Minuten)</label>
                    <input v-model="minDuration" type="number" class="field-input" placeholder="0">
                </div>
                <div class="field">
                    <label>Max. Dauer (Minuten)</label>                    <input v-model="maxDuration" type="number" class="field-input" placeholder="unbegrenzt">
                </div>

                <div class="field">
                    <label>Von Datum</label>
                    <input v-model="minBroadcastDate" type="date" class="field-input">
                </div>
                <div class="field">
                    <label>Bis Datum</label>
                    <input v-model="maxBroadcastDate" type="date" class="field-input">
                </div>
            </div>

            <div class="form-actions">
                <button type="submit" class="btn btn-primary" :disabled="loading">
                    {{ loading ? 'Suche läuft...' : 'Suche starten' }}
                </button>
                <button type="button" @click="createSubFromSearch" class="btn btn-secondary btn-icon-only" title="Abo aus Suche erstellen">
                    ➕
                </button>
            </div>
        </form>

        <div v-if="results.length > 0" class="results-list">
            <h3>Ergebnisse ({{ results.length < 50 ? results.length : '50+'}})</h3>
            <div v-for="item in results" :key="item.Id" class="result-item">
                <div class="result-info">
                    <div class="result-title">{{ item.Title }}</div>
                    <div class="result-meta">
                        {{ item.Channel }} | {{ item.Topic }} | {{ item.Duration }} |
                        <span v-if="item.SubtitleUrls && item.SubtitleUrls.length > 0" class="material-icons closed_caption" title="Untertitel verfügbar"></span>
                    </div>
                    <div class="result-meta">{{ item.Description }}</div>
                </div>
                <div class="result-actions">
                    <button @click="openVideo(item)" class="btn-icon" title="Im Browser abspielen">▶</button>
                    <button @click="simpleDownload(item)" class="btn-icon" title="Schneller Download" :disabled="isDownloading">⬇</button>
                    <button @click="openAdvancedDownloadDialog(item)" class="btn-icon" title="Erweiterte Download-Optionen">⬇⚙</button>
                    <button @click="createSubFromItem(item)" class="btn-icon" title="Abo für diese Sendung erstellen">➕</button>
                </div>
            </div>
        </div>
        <div v-else-if="!loading && (searchTitle || searchTopic || searchChannel || searchCombined)" class="no-results">
            Keine Ergebnisse gefunden.
        </div>
    </div>

    <!-- Advanced Download Dialog -->
    <AdvancedDownloadDialog
        v-if="showAdvancedDownload"
        :item="selectedItemForDownload"
        :pluginConfig="pluginConfig"
        @close="closeAdvancedDownloadDialog"
        @download="() => {}"
    />
</template>

<style scoped>
.search-form {
    width: 100%;
    max-width: none;
    margin-bottom: 20px;
}

.autocomplete-wrapper {
    position: relative;
}

.suggestions-list {
    position: absolute;
    top: 100%;
    left: 0;
    right: 0;
    background: #18181b;
    border: 1px solid #3f3f46;
    border-radius: 4px;
    margin: 0;
    padding: 0;
    list-style: none;
    z-index: 1000;
    max-height: 200px;
    overflow-y: auto;
    box-shadow: 0 4px 12px rgba(0,0,0,0.5);
}

.suggestions-list li {
    padding: 8px 12px;
    cursor: pointer;
    font-size: 0.9rem;
}

.suggestions-list li:hover,
.suggestions-list li.selected {
    background: #7c3aed;
    color: white;
}

.search-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    gap: 15px;
    width: 100%;
}

.form-actions {
    margin-top: 20px;
    display: flex;
    justify-content: flex-start;
    gap: 15px;
    align-items: center;
}

.results-list {
    margin-top: 20px;
}

.result-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 12px;
    border-bottom: 1px solid #333;
}

.result-item:last-child {
    border-bottom: none;
}

.result-title {
    font-weight: bold;
    margin-bottom: 2px;
}

.result-meta {
    font-size: 0.8rem;
    color: #a1a1aa;
    display: flex;
    align-items: center;
    gap: 8px;
}

.closed_caption {
    font-size: 1.1rem;
}

.result-actions {
    display: flex;
    gap: 10px;
}

.btn-icon {
    background: none;
    border: none;
    cursor: pointer;
    padding: 5px 8px;
    font-size: 1rem;
    color: #a1a1aa;
    transition: color 0.2s;
}

.btn-icon:hover:not(:disabled) {
    color: #fff;
}

.btn-icon:disabled {
    opacity: 0.5;
    cursor: not-allowed;
}

.no-results {
    text-align: center;
    color: #a1a1aa;
    padding: 20px;
    width: 100%;
}
</style>
