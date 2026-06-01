/**
 * Comprehensive Jellyfin Web Development Definitions
 */

// --- Dashboard & Globals ---
interface Loading {
    show(): void;
    hide(): void;
}

interface Dashboard {
    alert(message: string): void;
    confirm(message: string, title: string, callback: (result: boolean) => void): void;
    showLoadingMsg(): void;
    hideLoadingMsg(): void;
    processPluginConfigurationUpdateResult(result: any): void;
    hideModal(): void;
    
    // Dialog Helper integration
    dialogHelper: {
        open(dlg: HTMLElement): Promise<any>;
        close(dlg: HTMLElement): void;
        createDialog(options?: {
            id?: string,
            size?: 'small' | 'fullscreen' | string,
            scrollY?: boolean,
            scrollX?: boolean,
            removeOnClose?: boolean,
            enableHistory?: boolean,
            modal?: boolean,
            autoFocus?: boolean
        }): HTMLElement;
    };

    DirectoryBrowser: new () => {
        show(options: {
            header: string;
            includeDirectories: boolean;
            includeFiles: boolean;
            callback: (path: string) => void;
        }): void;
        close(): void;
    };
}

// --- ApiClient (Simplified abstraction) ---
interface ApiClient {
    getUrl(endpoint: string): string;
    getJSON(url: string, includeAuthorization?: boolean): Promise<any>;
    getPluginConfiguration(pluginId: string): Promise<any>;
    updatePluginConfiguration(pluginId: string, config: any): Promise<any>;
    ajax(options: {
        type: string;
        url: string;
        data?: string | FormData | object;
        contentType?: string | boolean;
        dataType?: string;
    }): Promise<any>;
    
    // Server interaction helpers
    getServerConfiguration(): Promise<any>;
    getScheduledTasks(options?: any): Promise<any[]>;
    getScheduledTask(id: string): Promise<any>;
    startScheduledTask(id: string): Promise<any>;
    stopScheduledTask(id: string): Promise<any>;
    getSystemInfo(): Promise<any>;
}

// --- Events ---
interface Events {
    on(obj: any, eventName: string, fn: (e: any, ...args: any[]) => void): void;
    off(obj: any, eventName: string, fn: (e: any, ...args: any[]) => void): void;
    trigger(obj: any, eventName: string, ...args: any[]): void;
}

// --- Global Scope ---
declare var Dashboard: Dashboard;
declare var ApiClient: ApiClient;
declare var Loading: Loading;
declare var Events: Events;

interface Window {
    Dashboard: Dashboard;
    ApiClient: ApiClient;
    Loading: Loading;
    Events: Events;
    Template: any;
    NativeShell: any;
}
