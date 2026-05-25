<script setup>
import {ref, watch} from 'vue'
import { SubscriptionFactory } from '../../utils/SubscriptionFactory'

const props = defineProps({
    onCreateSub: { type: Function, required: true }
})

const ApiClient = window.ApiClient ?? null
const PLUGIN_ID = 'a31b415a-5264-419d-b152-8c8192a54994'

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

let debounceTimer = null;

async function performSearch() {
    if (!ApiClient) return
    if (!searchTitle.value && !searchTopic.value && !searchChannel.value && !searchCombined.value) {
        results.value = [];
        return
    }

    loading.value = true
    try {
        const params = new URLSearchParams()
        if (searchTitle.value) params.append('title', searchTitle.value)
        if (searchTopic.value) params.append('topic', searchTopic.value)
        if (searchChannel.value) params.append('channel', searchChannel.value)
        if (searchCombined.value) params.append('combinedSearch', searchCombined.value)

        if (minDuration.value) params.append('minDuration', minDuration.value * 60)
        if (maxDuration.value) params.append('maxDuration', maxDuration.value * 60)

        if (minBroadcastDate.value) {
            params.append('minBroadcastDate', new Date(minBroadcastDate.value).toISOString())
        }
        if (maxBroadcastDate.value) {
            const d = new Date(maxBroadcastDate.value)
            d.setHours(23, 59, 59, 999)
            params.append('maxBroadcastDate', d.toISOString())
        }

        const url = ApiClient.getUrl('MediathekViewDL/Search?' + params.toString())
        results.value = await ApiClient.getJSON(url)
    } catch (e) {
        console.error('Search failed', e)
    } finally {
        loading.value = false
    }
}

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
    if (!ApiClient) return
    
    try {
        const params = new URLSearchParams()
        if (searchTitle.value) params.append('title', searchTitle.value)
        if (searchTopic.value) params.append('topic', searchTopic.value)
        if (searchChannel.value) params.append('channel', searchChannel.value)
        if (searchCombined.value) params.append('combinedSearch', searchCombined.value)
        
        // Get structured criteria from API to handle comma separation correctly
        const url = ApiClient.getUrl('MediathekViewDL/Search/Criteria?' + params.toString())
        const criteria = await ApiClient.getJSON(url)
        
        const sub = SubscriptionFactory.createDefault()
        sub.Name = searchTitle.value || searchTopic.value || searchCombined.value || 'Suche'
        sub.Search.Criteria = criteria
        sub.Search.MinDurationMinutes = minDuration.value
        sub.Search.MaxDurationMinutes = maxDuration.value
        sub.Search.MinBroadcastDate = minBroadcastDate.value ? new Date(minBroadcastDate.value).toISOString() : null
        sub.Search.MaxBroadcastDate = maxBroadcastDate.value ? new Date(maxBroadcastDate.value).toISOString() : null
        
        props.onCreateSub(sub);
    } catch (e) {
        console.error('Failed to convert criteria', e)
    }
}

async function createSubFromItem(item) {
    if (!ApiClient) return
    
    try {
        // Use the API to get clean criteria for this item
        // This is important because Title/Topic/Channel might contain commas
        const params = new URLSearchParams()
        if (item.Title) params.append('title', item.Title)
        if (item.Topic) params.append('topic', item.Topic)
        if (item.Channel) params.append('channel', item.Channel)
        
        const url = ApiClient.getUrl('MediathekViewDL/Search/Criteria?' + params.toString())
        const criteria = await ApiClient.getJSON(url)
        
        const sub = SubscriptionFactory.createDefault()
        sub.Name = item.Topic || item.Title
        sub.Search.Criteria = criteria
        
        props.onCreateSub(sub);
    } catch (e) {
        console.error('Failed to convert item criteria', e)
    }
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
                <div class="field">
                    <label>Thema</label>
                    <input v-model="searchTopic" type="text" class="field-input" placeholder="Thema / Sendereihe">
                </div>
                <div class="field">
                    <label>Sender</label>
                    <input v-model="searchChannel" type="text" class="field-input" placeholder="z.B. ARD, ZDF">
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
                    <button @click="createSubFromItem(item)" class="btn-icon" title="Abo für diese Sendung erstellen">➕</button>
                </div>
            </div>
        </div>
        <div v-else-if="!loading && (searchTitle || searchTopic || searchChannel || searchCombined)" class="no-results">
            Keine Ergebnisse gefunden.
        </div>
    </div>
</template>

<style scoped>
.search-form {
    width: 100%;
    max-width: none;
    margin-bottom: 20px;
}

.search-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    gap: 15px;
    width: 100%;
}

.field {
    display: flex;
    flex-direction: column; gap: 5px;
}

.field label {
    font-size: 0.9rem;
    color: #a1a1aa;
}

.field-input {
    background: #27272a;
    border: 1px solid #3f3f46;
    color: white;
    padding: 10px;
    border-radius: 4px;
    width: 100%;
    box-sizing: border-box;
}

.field-input:focus {
    border-color: #7c3aed;
    outline: none;
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
    color: white;
    cursor: pointer;
    font-size: 1.4rem;
    padding: 5px;
    border-radius: 4px;
}

.btn-icon:hover {
    background: #3f3f46;
}

.btn-primary {
    background: #7c3aed;
    color: white;
    border: none;
    padding: 10px 30px;
    border-radius: 4px;
    cursor: pointer;
    font-weight: 600;
    height: 42px;
}

.btn-primary:disabled {
    opacity: 0.5;
    cursor: not-allowed;
}

.btn-secondary {
    background: #3f3f46;
    color: white;
    border: none;
    padding: 10px 20px;
    border-radius: 4px;
    cursor: pointer;
    font-weight: 600;
    height: 42px;
}

.btn-icon-only {
    padding: 10px;
    font-size: 1.2rem;
}

.no-results {
    text-align: center;
    color: #a1a1aa;
    padding: 20px;
    width: 100%;
}
</style>
