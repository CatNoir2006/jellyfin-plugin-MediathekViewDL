<script setup>
import { ref, onMounted } from 'vue'

// ── Jellyfin globals (available when loaded inside Jellyfin)
const Dashboard = window.Dashboard ?? null
const ApiClient = window.ApiClient ?? null

const PLUGIN_ID = 'eb5d7894-8eef-4b36-aa6f-5d124e828ce1'

// ── Reactive config state
const activeWebUi  = ref('VueJS')
const anInteger    = ref(2)
const trueFalse    = ref(true)
const aString      = ref('string')

const loading      = ref(false)
const saveSuccess  = ref(false)
const saveError    = ref(false)
const actionStatus = ref('')

// ── Load config on mount
onMounted(async () => {
  if (!ApiClient) return          // standalone preview — keep defaults
  loading.value = true
  Dashboard?.showLoadingMsg?.()
  try {
    const config = await ApiClient.getPluginConfiguration(PLUGIN_ID)
    activeWebUi.value = config.ActiveWebUi ?? 'VueJS'
    anInteger.value   = config.AnInteger ?? 2
    trueFalse.value   = config.TrueFalseSetting ?? true
    aString.value     = config.AString   ?? 'string'
  } catch (e) {
    console.error('Failed to load config', e)
  } finally {
    loading.value = false
    Dashboard?.hideLoadingMsg?.()
  }
})

// ── Save config
async function saveConfig() {
  saveSuccess.value = false
  saveError.value   = false
  loading.value     = true
  Dashboard?.showLoadingMsg?.()
  try {
    const config = await ApiClient.getPluginConfiguration(PLUGIN_ID)
    config.ActiveWebUi       = activeWebUi.value
    config.AnInteger         = Number(anInteger.value)
    config.TrueFalseSetting  = trueFalse.value
    config.AString           = aString.value
    const result = await ApiClient.updatePluginConfiguration(PLUGIN_ID, config)
    Dashboard?.processPluginConfigurationUpdateResult?.(result)
    saveSuccess.value = true
  } catch (e) {
    console.error('Failed to save config', e)
    saveError.value = true
  } finally {
    loading.value = false
    Dashboard?.hideLoadingMsg?.()
  }
}

// ── Action helpers
async function apiAction(method, url, body, successMsg) {
  actionStatus.value = ''
  Dashboard?.showLoadingMsg?.()
  try {
    await ApiClient.ajax({
      type: method,
      url: ApiClient.getUrl(url),
      data: body ? JSON.stringify(body) : undefined,
      contentType: body ? 'application/json' : undefined,
      dataType: body ? undefined : 'text',
    })
    actionStatus.value = successMsg
    Dashboard?.alert?.(successMsg)
  } catch (e) {
    actionStatus.value = 'Action failed.'
  } finally {
    Dashboard?.hideLoadingMsg?.()
  }
}

function addTuner() {
  apiAction('POST', 'LiveTv/TunerHosts', {
    Type: 'template_livetv',
    Url: 'template',
    FriendlyName: 'Template Tuner',
    TunerCount: 0,
  }, 'Tuner successfully added!')
}

function addGuide() {
  apiAction('POST', 'LiveTv/ListingProviders', {
    Type: 'template_livetv',
    Id: 'template_guide',
    Name: 'Template Guide',
  }, 'Guide successfully added!')
}

function ping() {
  apiAction('GET', 'MyPlugin/ping', null, 'Ping OK!')
}
</script>

<template>
  <div class="plugin-config">

    <!-- ── Header ───────────────────────────────────────── -->
    <header class="config-header">
      <h1 class="config-title">Template Plugin</h1>
      <p class="config-subtitle">Manage your plugin settings below</p>
    </header>

    <!-- ── Live TV Section ──────────────────────────────── -->
    <section class="card">
      <h2 class="card-title">
        <span class="card-icon">📡</span> Live TV Setup
      </h2>
      <div class="action-row">
        <button class="btn btn-primary" :disabled="!ApiClient" @click="addTuner">
          Add Tuner
        </button>
        <button class="btn btn-secondary" :disabled="!ApiClient" @click="addGuide">
          Add Guide
        </button>
        <button class="btn btn-ghost" :disabled="!ApiClient" @click="ping">
          Ping
        </button>
      </div>
      <p v-if="actionStatus" class="status-msg">{{ actionStatus }}</p>
      <p v-if="!ApiClient" class="info-msg">
        ⚠️ Running in standalone preview — API actions are disabled.
      </p>
    </section>

    <!-- ── Configuration Form ───────────────────────────── -->
    <section class="card">
      <h2 class="card-title">
        <span class="card-icon">⚙️</span> Configuration
      </h2>

      <form class="config-form" @submit.prevent="saveConfig">

        <!-- Dropdown -->
        <div class="field">
          <label class="field-label" for="optActiveWebUi">Active WebUI</label>
          <div class="select-wrapper">
            <select id="optActiveWebUi" v-model="activeWebUi" class="field-select">
              <option value="VueJS">VueJS</option>
              <option value="ShowBoth">Show Both</option>
              <option value="Html">HTML Legacy</option>
            </select>
            <span class="select-arrow">▾</span>
          </div>
        </div>

        <!-- Integer -->
        <div class="field">
          <label class="field-label" for="optAnInteger">An Integer</label>
          <input
            id="optAnInteger"
            v-model.number="anInteger"
            type="number"
            min="0"
            class="field-input"
            placeholder="0"
          />
          <p class="field-hint">A Description</p>
        </div>

        <!-- Checkbox -->
        <div class="field field-checkbox">
          <label class="checkbox-label" for="optTrueFalse">
            <span class="checkbox-track" :class="{ active: trueFalse }">
              <input
                id="optTrueFalse"
                v-model="trueFalse"
                type="checkbox"
                class="checkbox-native"
              />
              <span class="checkbox-thumb" />
            </span>
            A Checkbox
          </label>
        </div>

        <!-- String -->
        <div class="field">
          <label class="field-label" for="optAString">A String</label>
          <input
            id="optAString"
            v-model="aString"
            type="text"
            class="field-input"
            placeholder="Enter a value…"
          />
          <p class="field-hint">Another Description</p>
        </div>

        <!-- Submit -->
        <div class="form-footer">
          <button type="submit" class="btn btn-save" :disabled="loading">
            <span v-if="loading" class="spinner" />
            <span>{{ loading ? 'Saving…' : 'Save Settings' }}</span>
          </button>
          <transition name="fade">
            <span v-if="saveSuccess" class="save-feedback success">✓ Saved!</span>
            <span v-else-if="saveError" class="save-feedback error">✗ Error saving.</span>
          </transition>
        </div>

      </form>
    </section>

  </div>
</template>

<style scoped>
/* ── Layout ──────────────────────────────────────────────── */
.plugin-config {
  max-width: 640px;
  margin: 0 auto;
  padding: 2rem 1.25rem;
  font-family: 'Inter', 'Segoe UI', system-ui, sans-serif;
  color: #e4e4e7;
}

/* ── Header ──────────────────────────────────────────────── */
.config-header { margin-bottom: 2rem; }
.config-title {
  font-size: 1.75rem;
  font-weight: 700;
  margin: 0 0 .25rem;
  background: linear-gradient(135deg, #00b4d8, #7c3aed);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}
.config-subtitle { color: #71717a; margin: 0; font-size: .95rem; }

/* ── Cards ───────────────────────────────────────────────── */
.card {
  background: #18181b;
  border: 1px solid #27272a;
  border-radius: 12px;
  padding: 1.5rem;
  margin-bottom: 1.5rem;
  box-shadow: 0 4px 24px rgba(0,0,0,.4);
}
.card-title {
  font-size: 1.05rem;
  font-weight: 600;
  margin: 0 0 1.25rem;
  display: flex;
  align-items: center;
  gap: .5rem;
  color: #f4f4f5;
}
.card-icon { font-size: 1.1rem; }

/* ── Action buttons row ───────────────────────────────────── */
.action-row {
  display: flex;
  flex-wrap: wrap;
  gap: .75rem;
}

/* ── Buttons ─────────────────────────────────────────────── */
.btn {
  display: inline-flex;
  align-items: center;
  gap: .4rem;
  padding: .55rem 1.15rem;
  border: none;
  border-radius: 8px;
  font-size: .9rem;
  font-weight: 500;
  cursor: pointer;
  transition: filter .15s, transform .1s;
}
.btn:active { transform: scale(.97); }
.btn:disabled { opacity: .4; cursor: not-allowed; }

.btn-primary  { background: #7c3aed; color: #fff; }
.btn-primary:hover:not(:disabled)  { filter: brightness(1.15); }
.btn-secondary { background: #0ea5e9; color: #fff; }
.btn-secondary:hover:not(:disabled) { filter: brightness(1.15); }
.btn-ghost { background: #27272a; color: #a1a1aa; border: 1px solid #3f3f46; }
.btn-ghost:hover:not(:disabled) { background: #3f3f46; color: #e4e4e7; }
.btn-save {
  background: linear-gradient(135deg, #7c3aed, #0ea5e9);
  color: #fff;
  padding: .65rem 1.5rem;
  font-size: .95rem;
}
.btn-save:hover:not(:disabled) { filter: brightness(1.12); }

/* ── Status messages ─────────────────────────────────────── */
.status-msg { margin: .75rem 0 0; font-size: .9rem; color: #a3e635; }
.info-msg   { margin: .75rem 0 0; font-size: .85rem; color: #f59e0b; }

/* ── Form fields ─────────────────────────────────────────── */
.config-form { display: flex; flex-direction: column; gap: 1.25rem; }

.field { display: flex; flex-direction: column; gap: .45rem; }
.field-label {
  font-size: .85rem;
  font-weight: 500;
  color: #a1a1aa;
  text-transform: uppercase;
  letter-spacing: .05em;
}
.field-input,
.field-select {
  background: #09090b;
  border: 1px solid #3f3f46;
  border-radius: 8px;
  padding: .6rem .85rem;
  color: #f4f4f5;
  font-size: .95rem;
  width: 100%;
  transition: border-color .2s, box-shadow .2s;
  box-sizing: border-box;
}
.field-input:focus,
.field-select:focus {
  outline: none;
  border-color: #7c3aed;
  box-shadow: 0 0 0 3px rgba(124,58,237,.25);
}
.field-hint { font-size: .82rem; color: #52525b; margin: 0; }

/* ── Select wrapper ──────────────────────────────────────── */
.select-wrapper { position: relative; }
.field-select { appearance: none; padding-right: 2rem; }
.select-arrow {
  position: absolute;
  right: .75rem;
  top: 50%;
  transform: translateY(-50%);
  pointer-events: none;
  color: #71717a;
  font-size: .85rem;
}

/* ── Toggle switch ───────────────────────────────────────── */
.field-checkbox { flex-direction: row; align-items: center; }
.checkbox-label {
  display: flex;
  align-items: center;
  gap: .75rem;
  cursor: pointer;
  font-size: .95rem;
  color: #d4d4d8;
  user-select: none;
}
.checkbox-track {
  position: relative;
  width: 44px;
  height: 24px;
  border-radius: 999px;
  background: #3f3f46;
  transition: background .2s;
  flex-shrink: 0;
}
.checkbox-track.active { background: #7c3aed; }
.checkbox-native {
  position: absolute;
  inset: 0;
  opacity: 0;
  cursor: pointer;
  width: 100%;
  height: 100%;
  margin: 0;
}
.checkbox-thumb {
  position: absolute;
  top: 3px;
  left: 3px;
  width: 18px;
  height: 18px;
  border-radius: 50%;
  background: #fff;
  transition: transform .2s;
  pointer-events: none;
}
.checkbox-track.active .checkbox-thumb { transform: translateX(20px); }

/* ── Form footer ─────────────────────────────────────────── */
.form-footer {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding-top: .25rem;
}
.save-feedback {
  font-size: .9rem;
  font-weight: 500;
}
.save-feedback.success { color: #4ade80; }
.save-feedback.error   { color: #f87171; }

/* ── Spinner ─────────────────────────────────────────────── */
.spinner {
  display: inline-block;
  width: 14px;
  height: 14px;
  border: 2px solid rgba(255,255,255,.3);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin .6s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

/* ── Fade transition ─────────────────────────────────────── */
.fade-enter-active, .fade-leave-active { transition: opacity .3s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>
