import { createApp } from 'vue'
import './style.css'
import App from './App.vue'

let appInstance = null;

function mountApp(container) {
    if (!appInstance) {
        appInstance = createApp(App);
        appInstance.mount(container);
    }
}

function unmountApp() {
    if (appInstance) {
        appInstance.unmount();
        appInstance = null;
    }
}

const pageElementId = 'configPageVueJSPage';
const page = document.getElementById(pageElementId);
const isStandalone = import.meta.env.DEV || !window.Dashboard;

if (page) {
    if (isStandalone) {
        // Standalone / dev-preview: mount immediately
        const container = page.querySelector('#app');
        if (container) {
            mountApp(container);
        }
    } else {
        // Inside Jellyfin: remount on every pageshow so config is reloaded
        page.addEventListener('pageshow', (event) => {
            const container = page.querySelector('#app');
            if (container) {
                // Always remount so onMounted fires and config is fetched fresh
                unmountApp();
                mountApp(container);
            }
        });

        page.addEventListener('pagehide', () => {
            unmountApp();
        });
    }
}
