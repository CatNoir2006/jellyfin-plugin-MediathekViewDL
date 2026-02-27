/**
 * Minimal type definitions for Jellyfin Web globals to provide Intellisense.
 */

interface Dashboard {
    alert(message: string): void;
    confirm(message: string, title: string, callback: (result: boolean) => void): void;
    showLoadingMsg(): void;
    hideLoadingMsg(): void;
    processPluginConfigurationUpdateResult(result: any): void;
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

interface ApiClient {
    getUrl(endpoint: string): string;
    getJSON(url: string): Promise<any>;
    getPluginConfiguration(pluginId: string): Promise<any>;
    updatePluginConfiguration(pluginId: string, config: any): Promise<any>;
    ajax(options: {
        type: string;
        url: string;
        data?: string;
        contentType?: string;
        dataType?: string;
    }): Promise<any>;
}

declare var Dashboard: Dashboard;
declare var ApiClient: ApiClient;

interface Window {
    Dashboard: Dashboard;
    ApiClient: ApiClient;
    MediathekViewDL: any;
}
