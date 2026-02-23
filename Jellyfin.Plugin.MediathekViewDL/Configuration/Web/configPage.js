/**
 * Helper Class that contains various utility methods.
 */
class Helper {
    static config() {
        return window.MediathekViewDL.config;
    }

    /**
     * Shows a confirmation popup.
     * @param message The message to display
     * @param title The title of the popup
     * @param resultCallback The callback to receive the result (true/false)
     */
    static confirmationPopup(message, title, resultCallback) {
        // noinspection JSUnresolvedReference
        if (typeof Dashboard !== 'undefined' && typeof Dashboard.confirm === 'function') {
            // noinspection JSUnresolvedReference,JSCheckFunctionSignatures
            Dashboard.confirm(message, title, resultCallback);
        } else {
            const result = confirm(title + "\n\n" + message);
            resultCallback(result);
        }
    }

    /**
     * Shows a toast/alert message.
     * @param message The message to display
     */
    static showToast(message) {
        // noinspection JSUnresolvedReference
        if (typeof Dashboard !== 'undefined' && typeof Dashboard.alert === 'function') {
            // noinspection JSUnresolvedReference
            Dashboard.alert(message);
        } else {
            alert(message);
        }
    }

    /**
     * Opens a folder selection dialog and sets the selected path to the input element.
     * @param inputId The ID of the input element to set the path
     * @param headerText The Title of the dialog
     */
    static openFolderDialog(inputId, headerText) {
        try {
            // noinspection JSUnresolvedReference
            if (typeof Dashboard !== 'undefined' && Dashboard.DirectoryBrowser) {
                // noinspection JSUnresolvedReference
                const picker = new Dashboard.DirectoryBrowser();
                picker.show({
                    header: headerText,
                    includeDirectories: true,
                    includeFiles: false,
                    callback: (path) => {
                        if (path) {
                            document.getElementById(inputId).value = path;
                        }
                        picker.close();
                    }
                });
            } else {
                let currentValue = document.getElementById(inputId).value;
                let newPath = prompt(headerText + '\n' + Language.General.CurrentPath + ': ' + currentValue, currentValue);
                if (newPath !== null && newPath.trim() !== '') {
                    document.getElementById(inputId).value = newPath.trim();
                }
            }
        } catch (e) {
            console.error('Error opening folder dialog:', e);
            let currentValue = document.getElementById(inputId).value;
            let newPath = prompt(headerText + '\n' + Language.General.CurrentPath + ': ' + currentValue, currentValue);
            if (newPath !== null && newPath.trim() !== '') {
                document.getElementById(inputId).value = newPath.trim();
            }
        }
    }

    /**
     * Generates a UUID (version 4).
     * @returns {string}
     */
    static genUUID() {
        try {
            return crypto.randomUUID();
        } catch (e) {
            console.error('Error generating UUID using crypto.randomUUID():', e);
        }
        console.warn('Falling back to manual UUID generation.');
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            let r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    /**
     * Opens a DuckDuckGo search in a new tab.
     * @param search The search query
     * @param siteFilter Filter to a specific site (optional)
     * @param openPage Open the search results page (true) or just the query (false)
     */
    static openDuckDuckGoSearch(search = '', siteFilter = '', openPage = false) {
        let queryString = (search).trim();
        if (siteFilter) {
            queryString += ' site:' + siteFilter;
        }
        const query = encodeURIComponent(queryString);
        const searchUrl = 'https://duckduckgo.com/?q=' + (openPage ? '\\' : '');
        window.open(searchUrl + query, '_blank');
    }

    /**
     * Extracts the file name from a given path.
     * @param path The full file path
     * @returns {string} The file name or empty string if none
     */
    static getFileNameFromPath(path) {
        if (!path) return '';
        const parts = path.split(/[\\\/]/);
        return parts[parts.length - 1];
    }

    /**
     * Extracts the file extension from a given path.
     * @param path The full file path
     * @returns {string} The file extension in lowercase, or empty string if none
     */
    static getFileExtensionFromPath(path) {
        if (!path) return '';
        const parts = path.split('.');
        return parts.length > 1 ? parts[parts.length - 1].toLowerCase() : '';
    }

    /**
     * Extracts a human-readable error message from an API error response.
     * Supports both legacy XHR objects and modern Fetch Response objects.
     * @param err The error object from the API call
     * @param defaultMessage A fallback message if no specific error is found
     * @returns {Promise<string>} The extracted error message
     */
    static async getErrorMessage(err, defaultMessage = Language.General.UnknownError) {
        if (!err) return defaultMessage;

        // Support for Fetch Response objects (asynchronous)
        if (err instanceof Response) {
            try {
                const contentType = err.headers.get("content-type");
                if (contentType && contentType.includes("application/json")) {
                    const json = await err.json();
                    return json.Detail || json.detail || json.Message || json.message || err.statusText || defaultMessage;
                } else {
                    const text = await err.text();
                    if (text && !text.trim().startsWith('<!DOCTYPE')) {
                        return text;
                    }
                }
            } catch (e) {
                console.error("Error parsing fetch response", e);
            }
            return err.statusText || defaultMessage;
        }

        // Legacy support for XHR/jQuery-like objects (synchronous)
        if (err.responseJSON) {
            const r = err.responseJSON;
            if (r.Detail) return r.Detail;
            if (r.detail) return r.detail;
            if (r.Message) return r.Message;
            if (r.message) return r.message;
        }

        if (err.responseText && !err.responseText.trim().startsWith('<!DOCTYPE')) {
            return err.responseText;
        }

        return err.statusText || err.message || defaultMessage;
    }

    /**
     * Shows an error message extracted from an API error response in a toast/alert.
     * @param err The error object from the API call
     * @param msgPrefix Text to prefix the error message with (optional)
     * @param defaultMessage A fallback message if no specific error is found
     */
    static showError(err, msgPrefix = '', defaultMessage = Language.General.UnknownError) {
        this.getErrorMessage(err, defaultMessage).then(message => {
            Helper.showToast(msgPrefix + message);
        });
    }
}

/**
 * Helper class for DOM manipulation to reduce verbosity.
 */
class DomHelper {
    /**
     * Creates an HTML element with specified options.
     * @param {string} tag - The HTML tag name.
     * @param {Object} [options] - Options for the element.
     * @param {string} [options.className] - CSS class names (space separated).
     * @param {string} [options.text] - Text content.
     * @param {string} [options.id] - Element ID.
     * @param {Object} [options.attributes] - Key-value pair of attributes.
     * @param {string} [options.type] - Input type (if tag is input/button).
     * @param {string} [options.value] - Input value.
     * @param {boolean} [options.checked] - Checkbox state.
     * @param {Function} [options.onClick] - Click handler.
     * @param {HTMLElement[]} [options.children] - Array of child elements to append.
     * @returns {HTMLElement} The created element.
     */
    create(tag, options = {}) {
        const el = document.createElement(tag);
        if (options.className) el.className = options.className;
        if (options.text) el.textContent = options.text;
        if (options.id) el.id = options.id;
        if (options.type) el.type = options.type;
        if (options.value) el.value = options.value;
        if (options.checked) el.checked = true;

        if (options.attributes) {
            for (const [key, val] of Object.entries(options.attributes)) {
                el.setAttribute(key, val);
            }
        }

        if (options.onClick) {
            el.addEventListener('click', options.onClick);
        }

        if (options.children) {
            options.children.forEach(child => {
                if (child) el.appendChild(child);
            });
        }

        return el;
    }

    createIconButton(icon, title, onClick, id, ariaLabel) {
        const span = this.create('span', {
            className: 'material-icons ' + icon,
            attributes: {'aria-hidden': 'true'}
        });

        const btnOptions = {
            type: 'button',
            className: 'paper-icon-button-light',
            attributes: {
                'is': 'emby-button',
                'title': title,
                'aria-label': ariaLabel || title
            },
            onClick: onClick,
            children: [span]
        };
        if (id) btnOptions.id = id;

        return this.create('button', btnOptions);
    }

    createCheckbox(label, checked, options = {}) {
        const {id, value, description, className, onChange} = options;

        const inputOptions = {
            type: 'checkbox',
            checked: checked,
            attributes: {'is': 'emby-checkbox'}
        };
        if (id) inputOptions.id = id;
        if (value) inputOptions.value = value;
        if (className) inputOptions.className = className;

        const input = this.create('input', inputOptions);
        if (onChange) input.addEventListener('change', onChange);

        const span = this.create('span', {text: label});

        const labelEl = this.create('label', {
            className: 'emby-checkbox-label',
            children: [input, span]
        });

        if (description) {
            const descEl = this.create('div', {
                className: 'fieldDescription',
                text: description
            });
            return this.create('div', {
                className: 'checkboxContainer checkboxContainer-withDescription',
                children: [labelEl, descEl]
            });
        }

        return labelEl;
    }
}

class StringHelper {
    /**
     * Sanitizes a string to be safe for use as a filename.
     * @param {string} input - The input string.
     * @returns {string} The sanitized filename.
     */
    static sanitizeForFilename(input) {
        return input.replace(/[\/\\?%*:|"<>]/g, '_').trim();
    }

    /**
     * Checks if a string is null, undefined, or consists only of whitespace.
     * @param {string} input - The input string.
     * @returns {boolean} True if null/whitespace, false otherwise.
     */
    static isNullOrWhitespace(input) {
        return !input || !input.trim?.();
    }

    /**
     * Parses a .NET TimeSpan string (e.g., "01:30:00" or "1.01:30:00") into seconds.
     * @param {string} ts - The TimeSpan string or seconds as number.
     * @returns {number} Seconds.
     */
    static parseTimeSpan(ts) {
        if (!ts) return 0;
        if (typeof ts === 'number') return ts;
        // Handle .NET TimeSpan format
        const match = ts.match(/(?:(\d+)\.)?(\d+):(\d+):(\d+)/);
        if (!match) return 0;
        const days = parseInt(match[1] || 0, 10);
        const hours = parseInt(match[2], 10);
        const minutes = parseInt(match[3], 10);
        const seconds = parseInt(match[4], 10);
        return days * 86400 + hours * 3600 + minutes * 60 + seconds;
    }
}

const Language = {
    General: {
        Cancel: "Abbrechen",
        Ok: "OK",
        Save: "Speichern",
        EmptyId: '00000000-0000-0000-0000-000000000000',
        CurrentPath: "Aktueller Pfad",
        NotConfigured: "Nicht konfiguriert",
        SettingsSaved: "Einstellungen gespeichert.",
        InitializingController: "Initialisiere DownloadsController",
        SelectGlobalDefaultDownloadPath: "Globalen Standard Download Pfad wählen",
        SelectDefaultShowPathAbo: "Standard Serien Pfad (Abo) wählen",
        SelectDefaultMoviePathAbo: "Standard Film Pfad (Abo) wählen",
        SelectDefaultShowPathManual: "Standard Serien Pfad (Manuell) wählen",
        SelectDefaultMoviePathManual: "Standard Film Pfad (Manuell) wählen",
        SelectTempDownloadPath: "Temporären Download Pfad wählen",
        MinutesShort: " min",
        UnknownError: "Unbekannter Fehler",
        AboNamePlaceholder: "[AboName]"
    },
    Search: {
        SearchTearm: "Bitte Suchbegriff eingeben",
        NoResults: "Keine Ergebnisse gefunden.",
        Results: "Suchergebnisse: ",
        Play: "Video abspielen",
        SearchDuckDuckGo: "Video über DuckDuckGo suchen",
        Download: "Video herunterladen",
        AdvancedDownload: "Erweiterter Download",
        Abo: "Abo erstellen",
        SubAvailable: "Untertitel verfügbar",
        VideoMissing: "Keine Video-URL verfügbar.",
        Error: "Fehler bei der Suche: ",
        ErrorDownloadStart: "Fehler beim Starten des Downloads: ",
        InQueue: (title) => 'Download für ' + title + ' in Warteschlange.',
        DownloadStarted: (title) => "Download für '" + title + "' gestartet.",
        NoSubtitles: "Keine Untertitel verfügbar für dieses Video.",
        ErrorTestingAbo: "Fehler beim Testen des Abonnements: ",
        NoTestHits: "Keine Treffer für diese Konfiguration.",
        TestResultsCount: (count) => count + " Einträge gefunden, die heruntergeladen würden:",
        Video: "Video"
    },
    Download: {
        Queued: "Warteschlange",
        Downloading: "Herunterladen...",
        Processing: "Verarbeiten...",
        Finished: "Fertig",
        Failed: "Fehler",
        Cancelled: "Abgebrochen",
        Unknown: "Unbekannt",
        Progress: (progress) => 'Fortschritt: ' + progress,
        NoAktivDownloads: "Keine aktiven Downloads.",
        NoHistory: "Kein Verlauf verfügbar.",
        TriggerManual: " (Manuell)",
        TriggerAbo: (subName) => subName ? " (Abo: " + subName + ")" : " (Abo)",
        Added: "Hinzugefügt: ",
        UnknownTitle: "Unbekannter Titel",
        ShowFiles: "Dateien anzeigen",
        HideFiles: "Dateien ausblenden",
        Subtitle: "[Untertitel]",
        Metadata: "[Metadaten]",
        Stream: "[Streaming-URL]",
        CancelRequested: "Abbruch angefordert",
        CancelFailed: "Fehler beim Abbrechen",
        ErrorAktivDownloads: "Fehler beim Abrufen aktiver Downloads: ",
        ErrorDownloadHistory: "Fehler beim Abrufen des Downloadverlaufs: ",
        Date: "Datum: ",
        Files: " Dateien"
    },
    Adoption: {
        NoData: "Noch keine Daten ..."
    },
    LiveTv: {
        TunerAdded: "Zapp Tuner erfolgreich hinzugefügt.",
        ErrorAddTuner: "Fehler beim Hinzufügen des Tuners: ",
        GuideProviderAdded: "Zapp Guide Provider erfolgreich hinzugefügt.",
        ErrorAddGuideProvider: "Fehler beim Hinzufügen des Guide Providers: "
    },
    Subscription: {
        UsedPaths: "Verwendete Pfade:",
        Movies: "Filme",
        Series: "Serien",
        SelectedPath: "Ausgewählter Pfad für Film und Serie:",
        DefaultPathUsed: "Standartpfad wird verwendet:",
        EditSubscription: "Abonnement bearbeiten",
        CreateSubscription: "Neues Abonnement erstellen",
        NotConfigured: "Nicht konfiguriert", // Already added, but good to keep it in mind
        NoActiveSubscriptions: "Keine aktiven Abonnements.",
        Disable: "Deaktivieren",
        Enable: "Aktivieren",
        ResetProcessedItems: "Verarbeitete Items zurücksetzen",
        Edit: "Bearbeiten",
        Delete: "Löschen",
        ProcessedItemsReset: "Verarbeitete Items für Abonnement zurückgesetzt.",
        ErrorResettingProcessedItems: "Fehler beim Zurücksetzen der verarbeiteten Items: ",
        DefineAtLeastOneQuery: "Bitte mindestens eine Suchanfrage definieren.",
        ConfirmDelete: "Soll dieses Abonnement wirklich gelöscht werden?",
        ConfirmDeleteTitle: "Löschen bestätigen",
        Queries: "Queries: ",
        LastDownload: "Letzter Download: ",
        Never: "Nie",
        Disabled: " (Deaktiviert)",
        ConfirmResetProcessedItemsMessage: "Dies wird die Liste der bereits verarbeiteten Items für dieses Abonnement zurücksetzen. Es kann dazu führen, dass bereits heruntergeladene Inhalte erneut heruntergeladen werden, wenn sie noch in den Suchergebnissen der MediathekView API erscheinen. Fortfahren?",
        SearchText: "Suchtext",
        Title: "Titel",
        Topic: "Thema",
        Description: "Beschreibung",
        Channel: "Sender",
        RemoveQuery: "Anfrage entfernen",
        SelectAboPath: "Abo Pfad wählen",
        ErrorInitializationStatus: "Fehler beim Prüfen des Initialisierungsstatus"
    },
    AdvancedDownload: {
        SelectDownloadPath: "Download Pfad wählen",
        TitlePrefix: "Erweiterter Download: "
    }
}

const DomIds = {
    Tabs: {
        Subscription: "subscription"
    },
    Download: {
        AktivList: "activeDownloadsList",
        HistoryList: "downloadHistoryList"
    },
    Search: {
        Form: "mvpl-form-search",
        Title: "txtSearchQuery",
        Topic: "txtSearchTopic",
        Channel: "txtSearchChannel",
        Combined: "txtSearchCombined",
        MinDuration: "numMinDuration",
        MaxDuration: "numMaxDuration",
        MinBroadcastDate: "dateMinBroadcast",
        MaxBroadcastDate: "dateMaxBroadcast",
        Results: "searchResults"
    },
    LiveTv:{
        Tuner: "mvpl-btn-setup-tuner",
        Guide: "mvpl-btn-setup-guide"
    },
    Adoption: {
        AboSelector: "selectAdoptionAbo",
        TableContainer: "adoptionTableContainer",
        LocalTable: "localFilesTable",
        ExternalTable: "apiResultsTable",
    },
    AdvancedDownload: {
        Modal: "advancedDownloadModal",
        Title: "advancedDownloadTitle",
        Path: "advDlPath",
        Filename: "advDlFilename",
        Subtitles: "advDlSubtitles",
        SubtitlesDesc: "advDlSubtitlesDesc",
        BtnSelectPath: "btnSelectPath"
    }
}

const Icons = {
    Search: 'search',
    Download: 'file_download',
    Play: 'play_arrow',
    Settings: 'settings',
    Add: "add",
    Subtitle: "closed_caption",
    Cancel: "cancel",
    Expand: "expand_more",
    Collapse: "expand_less",
    ToggleOn: 'pause_circle_outline',
    ToggleOff: 'play_circle_outline',
    Refresh: 'refresh',
    Edit: 'edit',
    Delete: 'delete',
    Remove: 'remove_circle_outline'
}

/**
 * Handles search operations.
 */
class SearchController {
    constructor(config) {
        this.config = config;
        this.dom = config.dom;
        this.currentSearchResults = [];
    }

    init() {
        document.getElementById(DomIds.Search.Form).addEventListener('submit', (e) => {
            e.preventDefault();
            this.performSearch();
            return false;
        });
    }

    performSearch() {
        const title = document.getElementById(DomIds.Search.Title).value;
        const topic = document.getElementById(DomIds.Search.Topic).value;
        const channel = document.getElementById(DomIds.Search.Channel).value;
        const combinedSearch = document.getElementById(DomIds.Search.Combined).value;
        const minD = document.getElementById(DomIds.Search.MinDuration).value;
        const maxD = document.getElementById(DomIds.Search.MaxDuration).value;
        const minDate = document.getElementById(DomIds.Search.MinBroadcastDate).value;
        const maxDate = document.getElementById(DomIds.Search.MaxBroadcastDate).value;

        if (!title && !topic && !channel && !combinedSearch) {
            Helper.showToast(Language.Search.SearchTearm);
            return;
        }
        // noinspection JSUnresolvedReference
        Dashboard.showLoadingMsg();
        // noinspection JSUnresolvedReference
        let url = ApiClient.getUrl('/' + this.config.pluginName + '/Search');

        const params = [];
        if (title) params.push('title=' + encodeURIComponent(title));
        if (topic) params.push('topic=' + encodeURIComponent(topic));
        if (channel) params.push('channel=' + encodeURIComponent(channel));
        if (combinedSearch) params.push('combinedSearch=' + encodeURIComponent(combinedSearch));
        if (minD) params.push('minDuration=' + (parseInt(minD) * 60));
        if (maxD) params.push('maxDuration=' + (parseInt(maxD) * 60));
        if (minDate) params.push('minBroadcastDate=' + encodeURIComponent(new Date(minDate).toISOString()));
        if (maxDate) {
            const d = new Date(maxDate);
            d.setHours(23, 59, 59, 999);
            params.push('maxBroadcastDate=' + encodeURIComponent(d.toISOString()));
        }

        if (params.length > 0) {
            url += '?' + params.join('&');
        }

        // noinspection JSUnresolvedReference
        ApiClient.getJSON(url).then((results) => {
            this.currentSearchResults = results;
            this.renderSearchResults();
            // noinspection JSUnresolvedReference
            Dashboard.hideLoadingMsg();
        }).catch((err) => {
            // noinspection JSUnresolvedReference
            Dashboard.hideLoadingMsg();
            Helper.showError(err, Language.Search.Error);
        });
    }

    renderSearchResults() {
        const container = document.getElementById(DomIds.Search.Results);
        container.textContent = "";

        if (!this.currentSearchResults || this.currentSearchResults.length === 0) {
            const noRes = document.createElement('p');
            noRes.textContent = Language.Search.NoResults;
            container.appendChild(noRes);
            return;
        }

        const paperList = document.createElement('div');
        paperList.classList.add('paperList');

        this.config.debugLog(Language.Search.Results, this.currentSearchResults);
        this.currentSearchResults.forEach((item, index) => {
            paperList.appendChild(this.createSearchResultItem(item, index));
        });
        container.appendChild(paperList);
    }

    createSearchResultItem(item, index) {
        const durationSeconds = StringHelper.parseTimeSpan(item.Duration);
        const durationStr = Math.max(1, Math.floor(durationSeconds / 60)) + Language.General.MinutesShort; // Each Video should show up with at least 1 min.
        const actions = document.createElement('div');
        actions.classList.add('flex-gap-10');

        actions.appendChild(this.dom.createIconButton(Icons.Play, Language.Search.Play, () => {
            const videoUrls = item.VideoUrls;
            // Sort by quality descending and get the first one
            const bestVideo = [...videoUrls].sort((a, b) => (b.Quality || 0) - (a.Quality || 0))[0];
            const videoUrl = bestVideo ? bestVideo.Url : null;
            if (videoUrl) {
                window.open(videoUrl, '_blank');
            } else {
                Helper.showToast(Language.Search.VideoMissing);
            }
        }))
        actions.appendChild(this.dom.createIconButton(Icons.Search, Language.Search.SearchDuckDuckGo, () => {
            const queryString = item.Topic + ' ' + item.Title;
            Helper.openDuckDuckGoSearch(queryString);
        }));
        actions.appendChild(this.dom.createIconButton(Icons.Download, Language.Search.Download, () => this.downloadItem(index)));
        actions.appendChild(this.dom.createIconButton(Icons.Settings, Language.Search.AdvancedDownload, () => this.config.openAdvancedDownloadDialog(this.currentSearchResults[index])));
        actions.appendChild(this.dom.createIconButton(Icons.Add, Language.Search.Abo, () => this.createSubFromSearch(null, item.Title, item.Channel, item.Topic)));


        // Build BodyText1
        const body1 = document.createElement('div');
        body1.classList.add('flex-align-center');
        body1.style.gap = '8px';
        const textSpan = document.createElement('span');
        textSpan.textContent = item.Channel + ' | ' + item.Topic + ' | ' + durationStr;
        body1.appendChild(textSpan);

        const subtitleUrls = item.SubtitleUrls || [];
        if (subtitleUrls.length > 0) {
            const sep = document.createElement('span');
            sep.textContent = ' | ';
            body1.appendChild(sep);
            const icon = document.createElement('span');
            icon.classList.add('material-icons', Icons.Subtitle);
            icon.title = Language.Search.SubAvailable;
            body1.appendChild(icon);
        }

        const bodyText2 = item.Description || '';

        return this.config.createListItem(item.Title, body1, bodyText2, actions);
    }

    downloadItem(index) {
        const item = this.currentSearchResults[index];
        if (!item) return;
        // noinspection JSUnresolvedReference
        const url = ApiClient.getUrl('/' + this.config.pluginName + '/Download');
        // noinspection JSUnresolvedReference
        Dashboard.showLoadingMsg();
        // noinspection JSUnresolvedReference
        ApiClient.ajax({
            type: "POST",
            url: url,
            data: JSON.stringify(item),
            contentType: 'application/json'
        }).then((result) => {
            // noinspection JSUnresolvedReference
            Dashboard.hideLoadingMsg();
            Helper.showToast(Language.Search.InQueue(item.Title));
        }).catch((err) => {
            // noinspection JSUnresolvedReference
            Dashboard.hideLoadingMsg();
            Helper.showError(err, Language.Search.ErrorDownloadStart);
        });
    }

    createSubFromSearch(btn, title, channel, topic) {
        this.config.switchTab(DomIds.Tabs.Subscription);
        const def = this.config.currentConfig.SubscriptionDefaults || {};
        const defDl = def.DownloadSettings || {};
        const defSearch = def.SearchSettings || {};
        const defSeries = def.SeriesSettings || {};
        const defMeta = def.MetadataSettings || {};
        const defAccess = def.AccessibilitySettings || {};

        const newSub = {
            Id: null,
            Name: topic,
            Search: {
                Criteria: [
                    {Fields: ["Title"], Query: title},
                    {Fields: ["Channel"], Query: channel},
                    {Fields: ["Topic"], Query: topic}
                ],
                MinDurationMinutes: defSearch.MinDurationMinutes || null,
                MaxDurationMinutes: defSearch.MaxDurationMinutes || null,
            },
            Download: {
                DownloadPath: "",
                UseStreamingUrlFiles: defDl.UseStreamingUrlFiles || false,
                DownloadFullVideoForSecondaryAudio: defDl.DownloadFullVideoForSecondaryAudio || false,
                AlwaysCreateSubfolder: defDl.AlwaysCreateSubfolder || false,
                EnhancedDuplicateDetection: defDl.EnhancedDuplicateDetection || false,
                AutoUpgradeToHigherQuality: defDl.AutoUpgradeToHigherQuality || false,
                AllowFallbackToLowerQuality: defDl.AllowFallbackToLowerQuality !== undefined ? defDl.AllowFallbackToLowerQuality : true,
                QualityCheckWithUrl: defDl.QualityCheckWithUrl || false,
            },
            Series: {
                EnforceSeriesParsing: defSeries.EnforceSeriesParsing || false,
                AllowAbsoluteEpisodeNumbering: defSeries.AllowAbsoluteEpisodeNumbering || false,
                TreatNonEpisodesAsExtras: defSeries.TreatNonEpisodesAsExtras || false,
                SaveTrailers: defSeries.SaveTrailers !== undefined ? defSeries.SaveTrailers : true,
                SaveInterviews: defSeries.SaveInterviews !== undefined ? defSeries.SaveInterviews : true,
                SaveGenericExtras: defSeries.SaveGenericExtras !== undefined ? defSeries.SaveGenericExtras : true,
                SaveExtrasAsStrm: defSeries.SaveExtrasAsStrm || false,
            },
            Metadata: {
                OriginalLanguage: defMeta.OriginalLanguage || "",
                CreateNfo: defMeta.CreateNfo || false,
                AppendDateToTitle: defMeta.AppendDateToTitle || false,
                AppendTimeToTitle: defMeta.AppendTimeToTitle || false,
            },
            Accessibility: {
                AllowAudioDescription: defAccess.AllowAudioDescription || false,
                AllowSignLanguage: defAccess.AllowSignLanguage || false,
            }
        };
        this.config.subscriptionEditor.show(newSub);
    }
}

class DownloadsController {
    constructor(config) {
        this.config = config;
        this.dom = config.dom;
        this.pollTimeout = null;
        this.isPolling = false;
        this.expandedGroups = new Set();
        this.statusMapping = {
            'Queued': {text: Language.Download.Queued}, // Queued
            'Downloading': {text: Language.Download.Downloading}, // Downloading
            'Processing': {text: Language.Download.Processing}, // Processing
            'Finished': {text: Language.Download.Finished}, // Finished
            'Failed': {text: Language.Download.Failed}, // Failed
            'Cancelled': {text: Language.Download.Cancelled} // Cancelled
        };
    }

    init() {
        this.config.debugLog(Language.General.InitializingController);
        // Initial load handled by switchTab
    }

    startPolling() {
        this.stopPolling();
        this.poll();
    }

    stopPolling() {
        if (this.pollTimeout) {
            clearTimeout(this.pollTimeout);
            this.pollTimeout = null;
        }
        this.isPolling = false;
    }

    poll() {
        this.isPolling = true;
        this.refreshData().finally(() => {
            if (this.isPolling) {
                this.pollTimeout = setTimeout(() => this.poll(), 3000);
            }
        });
    }

    refreshData() {
        // Return a promise that resolves when both requests are done
        return Promise.all([this.fetchActive(), this.fetchHistory()]);
    }

    fetchActive() {
        const url = ApiClient.getUrl('/' + this.config.pluginName + '/Downloads/Active');
        return ApiClient.getJSON(url).then((downloads) => {
            this.renderActive(downloads);
        }).catch((err) => {
            console.error(Language.Download.ErrorAktivDownloads, err);
        });
    }

    fetchHistory() {
        const url = ApiClient.getUrl('/' + this.config.pluginName + '/Downloads/History?limit=20');
        return ApiClient.getJSON(url).then((history) => {
            this.renderHistory(history);
        }).catch((err) => {
            console.error(Language.Download.ErrorDownloadHistory, err);
        });
    }

    renderActive(downloads) {
        const container = document.getElementById(DomIds.Download.AktivList);
        container.textContent = "";

        if (!downloads || downloads.length === 0) {
            const noRes = document.createElement('p');
            noRes.textContent = Language.Download.NoAktivDownloads;
            container.appendChild(noRes);
            return;
        }

        downloads.forEach((dl) => {
            const statusInfo = this.statusMapping[dl.Status] || {text: Language.Download.Unknown};
            const progress = dl.Status === 'Downloading' ? Math.round(dl.Progress || 0) + '%' : '-';

            const actions = document.createElement('div');
            actions.classList.add('flex-gap-10');

            // Show cancel button only if not finished/failed/cancelled
            if (['Queued', 'Downloading', 'Processing'].includes(dl.Status)) {
                actions.appendChild(this.dom.createIconButton(Icons.Cancel, Language.General.Cancel, () => this.cancelDownload(dl.Id)));
            }

            const statusBadge = document.createElement('span');
            statusBadge.classList.add('mvpl-download-status');
            statusBadge.setAttribute('data-status', dl.Status);
            statusBadge.textContent = statusInfo.text;


            const body1 = document.createElement('div');
            body1.classList.add('flex-align-center');
            body1.appendChild(document.createTextNode(Language.Download.Progress(progress)));
            body1.appendChild(statusBadge);

            if (dl.ErrorMessage) {
                const errorText = document.createElement('div');
                errorText.style.color = '#F44336';
                errorText.style.fontSize = '0.85em';
                errorText.style.marginTop = '4px';
                errorText.textContent = dl.ErrorMessage;
                body1.appendChild(errorText);
            }

            const createdAt = new Date(dl.CreatedAt).toLocaleString();

            let downloadTrigger = Language.Download.TriggerManual;
            if (!StringHelper.isNullOrWhitespace(dl.SubscriptionId)) {
                const sub = this.config.currentConfig?.Subscriptions?.find(s => s.Id === dl.SubscriptionId);
                downloadTrigger = Language.Download.TriggerAbo(sub.Name);
            }

            const body2 = Language.Download.Added + createdAt + downloadTrigger;

            container.appendChild(this.config.createListItem(dl.Job.Title, body1, body2, actions));
        });
    }

    renderHistory(history) {
        const container = document.getElementById(DomIds.Download.HistoryList);
        container.textContent = "";

        if (!history || history.length === 0) {
            const noRes = document.createElement('p');
            noRes.textContent = Language.Download.NoHistory;
            container.appendChild(noRes);
            return;
        }

        const groups = this.groupHistoryEntries(history);
        groups.forEach((group) => {
            container.appendChild(this.renderHistoryGroup(group));
        });
    }

    /**
     * Toggles the expanded state of a history group.
     * @param {string} groupKey
     * @param {boolean} expand
     * @param {HTMLElement} groupItem
     */
    toggleGroupState(groupKey, expand, groupItem) {
        const details = groupItem.querySelector('.mvpl-history-details');
        const expandBtn = groupItem.querySelector('.mvpl-btn-expand');
        const collapseBtn = groupItem.querySelector('.mvpl-btn-collapse');

        if (expand) {
            details.classList.remove('mvpl-hidden');
            if (expandBtn) expandBtn.style.display = 'none';
            if (collapseBtn) collapseBtn.style.display = 'inline-flex';
            this.expandedGroups.add(groupKey);
        } else {
            details.classList.add('mvpl-hidden');
            if (expandBtn) expandBtn.style.display = 'inline-flex';
            if (collapseBtn) collapseBtn.style.display = 'none';
            this.expandedGroups.delete(groupKey);
        }
    }

    /**
     * Generates a unique key for a group to track its state.
     * @param {Object} group
     * @returns {string}
     */
    getGroupKey(group) {
        return (group.subscriptionId || 'manual') + '_' + (group.itemId || group.title);
    }

    /**
     * Groups history entries by SubscriptionId and (ItemId or Title).
     * @param {Array} history - The history entries.
     * @returns {Array} The grouped entries.
     */
    groupHistoryEntries(history) {
        const groups = [];

        history.forEach((entry) => {
            const entrySubId = entry.SubscriptionId || Language.General.EmptyId;
            const entryItemId = entry.ItemId || '';
            const entryTitle = entry.Title || '';
            const entryFileName = Helper.getFileNameFromPath(entry.DownloadPath);
            const entryDisplayName = !StringHelper.isNullOrWhitespace(entryTitle) ? entryTitle : entryFileName;

            // Match logic: Same SubId AND (Same ItemId OR Same Title OR same DisplayName)
            let group = groups.find(g => {
                if (g.subscriptionId !== entrySubId) return false;
                if (entryItemId && g.itemId && entryItemId === g.itemId) return true;
                if (entryTitle && g.title && entryTitle === g.title) return true;
                return entryDisplayName && g.displayName && entryDisplayName === g.displayName;
            });

            if (!group) {
                group = {
                    subscriptionId: entrySubId,
                    title: entryTitle,
                    displayName: entryDisplayName,
                    itemId: entryItemId,
                    latestTimestamp: entry.Timestamp,
                    entries: []
                };
                groups.push(group);
            }

            group.entries.push(entry);

            // Preference for display name: use shortest one available
            if (entryDisplayName && (!group.displayName || entryDisplayName.length < group.displayName.length)) {
                group.displayName = entryDisplayName;
            }

            if (new Date(entry.Timestamp) > new Date(group.latestTimestamp)) {
                group.latestTimestamp = entry.Timestamp;
            }
        });

        return groups.sort((a, b) => new Date(b.latestTimestamp) - new Date(a.latestTimestamp));
    }

    /**
     * Renders a single history group item.
     * @param {Object} group - The grouped history data.
     * @returns {HTMLElement} The group DOM element.
     */
    renderHistoryGroup(group) {
        const groupKey = this.getGroupKey(group);
        const isExpanded = this.expandedGroups.has(groupKey);
        const timestamp = new Date(group.latestTimestamp).toLocaleString();

        let downloadTrigger = Language.Download.TriggerManual;
        if (group.subscriptionId && group.subscriptionId !== Language.General.EmptyId) {
            const sub = this.config.currentConfig?.Subscriptions?.find(s => s.Id === group.subscriptionId);
            downloadTrigger = Language.Download.TriggerAbo(sub.Name);
        }

        const displayTitle = group.displayName || Language.Download.UnknownTitle;

        const actions = document.createElement('div');
        actions.className = 'listItemButtons flex-gap-10';

        const expandBtn = this.dom.createIconButton(Icons.Expand, Language.Download.ShowFiles, () => {
            this.toggleGroupState(groupKey, true, groupItem);
        });
        expandBtn.classList.add('mvpl-btn-expand');
        expandBtn.style.display = isExpanded ? 'none' : 'inline-flex';

        const collapseBtn = this.dom.createIconButton(Icons.Collapse, Language.Download.HideFiles, () => {
            this.toggleGroupState(groupKey, false, groupItem);
        });
        collapseBtn.classList.add('mvpl-btn-collapse');
        collapseBtn.style.display = isExpanded ? 'inline-flex' : 'none';

        actions.appendChild(expandBtn);
        actions.appendChild(collapseBtn);

        const body1 = document.createElement('div');
        body1.className = 'listItemBodyText secondary';
        body1.textContent = Language.Download.Date + timestamp + (group.entries.length > 1 ? ' (' + group.entries.length + Language.Download.Files + ')' : '');

        const groupItem = this.config.createListItem(displayTitle + downloadTrigger, body1, "", actions);

        // Match standard list item appearance while supporting collapsible details
        groupItem.style.flexDirection = 'column';
        groupItem.style.alignItems = 'stretch';
        groupItem.style.padding = '0'; // We'll move padding to the header

        // Wrap existing children into a row to maintain row layout for the header
        const headerRow = document.createElement('div');
        headerRow.style.display = 'flex';
        headerRow.style.flexDirection = 'row';
        headerRow.style.alignItems = 'center';
        headerRow.style.width = '100%';
        headerRow.style.padding = '10px 15px';

        while (groupItem.firstChild) {
            headerRow.appendChild(groupItem.firstChild);
        }
        groupItem.appendChild(headerRow);

        // Details section for files
        const detailsDiv = document.createElement('div');
        detailsDiv.className = 'mvpl-history-details' + (isExpanded ? '' : ' mvpl-hidden');
        detailsDiv.style.paddingLeft = '30px';
        detailsDiv.style.paddingRight = '15px';
        detailsDiv.style.paddingBottom = isExpanded ? '15px' : '0';
        detailsDiv.style.fontSize = '0.85em';

        group.entries.forEach(entry => {
            const entryDiv = document.createElement('div');
            entryDiv.className = 'mvpl-history-entry';

            let fileTypeInfo = "";
            const ext = Helper.getFileExtensionFromPath(entry.DownloadPath);
            if (ext === 'vtt' || ext === 'ttml') fileTypeInfo = Language.Download.Subtitle;
            else if (ext === 'nfo') fileTypeInfo = Language.Download.Metadata;
            else if (ext === 'strm') fileTypeInfo = Language.Download.Stream;

            const langInfo = !StringHelper.isNullOrWhitespace(entry.Language) ? (" (" + entry.Language + ")") : "";
            const fileNameOnly = Helper.getFileNameFromPath(entry.DownloadPath);

            entryDiv.innerHTML = '<span class="secondary" style="font-weight:bold;">' + fileTypeInfo + fileNameOnly + langInfo + '</span><br/>' +
                '<span class="secondary mvpl-history-entry-path">' + entry.DownloadPath + '</span>';
            detailsDiv.appendChild(entryDiv);
        });

        groupItem.appendChild(detailsDiv);
        return groupItem;
    }

    cancelDownload(id) {
        const url = ApiClient.getUrl('/' + this.config.pluginName + '/Downloads/' + id);
        ApiClient.ajax({
            type: "DELETE",
            url: url
        }).then(() => {
            Helper.showToast(Language.Download.CancelRequested);
            this.refreshData();
        }).catch((err) => {
            Helper.showError(err, Language.Download.CancelFailed);
        });
    }
}

/**
 * Handles adoption operations.
 */
class AdoptionController {
    constructor(config) {
        this.config = config;
        this.dom = config.dom;
    }

    init() {
        document.getElementById(DomIds.Adoption.AboSelector).addEventListener('change', (e) => {
            this.onAboSelected(e.target.value);
        });
    }

    onAboSelected(aboId) {
        const container = document.getElementById(DomIds.Adoption.TableContainer);
        if (aboId) {
            container.style.display = 'block';
            this.refreshData(aboId);
        } else {
            container.style.display = 'none';
        }
    }

    refreshData(aboId) {
        // Vorerst noch keine Daten anzeigen
        document.getElementById(DomIds.Adoption.LocalTable).textContent = Language.Adoption.NoData;
        document.getElementById(DomIds.Adoption.ExternalTable).textContent = Language.Adoption.NoData;
    }

    populateAbos(subscriptions) {
        const select = document.getElementById(DomIds.Adoption.AboSelector);
        // Clear existing options except the first one
        while (select.options.length > 1) {
            select.remove(1);
        }

        if (subscriptions) {
            subscriptions.forEach(sub => {
                const opt = document.createElement('option');
                opt.value = sub.Id;
                opt.text = sub.Name;
                select.add(opt);
            });
        }
    }
}

/**
 * Handles Live TV setup operations.
 */
class SetupLiveTvController {
    constructor(config) {
        this.config = config;
    }

    init() {
        document.getElementById(DomIds.LiveTv.Tuner).addEventListener('click', () => this.setupTuner());
        document.getElementById(DomIds.LiveTv.Guide).addEventListener('click', () => this.setupGuide());
    }

    setupTuner() {
        const tunerInfo = {
            Type: 'zapp',
            Url: 'zapp',
            FriendlyName: 'Zapp (MediathekView)',
            TunerCount: 0
        };

        Dashboard.showLoadingMsg();
        ApiClient.ajax({
            type: 'POST',
            url: ApiClient.getUrl('LiveTv/TunerHosts'),
            data: JSON.stringify(tunerInfo),
            contentType: 'application/json'
        }).then(() => {
            Dashboard.hideLoadingMsg();
            Helper.showToast(Language.LiveTv.TunerAdded);
        }).catch((err) => {
            Dashboard.hideLoadingMsg();
            Helper.showError(err, Language.LiveTv.ErrorAddTuner);
        });
    }

    setupGuide() {
        const guideInfo = {
            Type: 'zapp',
            Id: 'zapp_guide',
            Name: 'Zapp (MediathekView)',
        };

        Dashboard.showLoadingMsg();
        ApiClient.ajax({
            type: 'POST',
            url: ApiClient.getUrl('LiveTv/ListingProviders'),
            data: JSON.stringify(guideInfo),
            contentType: 'application/json'
        }).then(() => {
            Dashboard.hideLoadingMsg();
            Helper.showToast(Language.LiveTv.GuideProviderAdded);
        }).catch((err) => {
            Dashboard.hideLoadingMsg();
            Helper.showError(err, Language.LiveTv.ErrorAddGuideProvider);
        });
    }
}

/**
 * Manages UI dependencies (showing/hiding fields based on others).
 */
class DependencyManager {
    constructor() {
        this.rules = [
            {
                controllerId: 'subTreatNonEpisodesAsExtras',
                dependentId: 'subSaveTrailersContainer',
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: 'subTreatNonEpisodesAsExtras',
                dependentId: 'subSaveInterviewsContainer',
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: 'subTreatNonEpisodesAsExtras',
                dependentId: 'subSaveGenericExtrasContainer',
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: 'subTreatNonEpisodesAsExtras',
                dependentId: 'subSaveExtrasAsStrmContainer',
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: 'subEnforceSeries',
                dependentId: 'subAllowAbsoluteEpisodeNumberingContainer',
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: 'subUseStreamingUrlFiles',
                dependentId: 'subDownloadFullVideoForSecondaryAudioContainer',
                showWhen: false,
                disableWhenHidden: true
            },
            {
                controllerId: 'subDownloadFullVideoForSecondaryAudio',
                dependentId: 'subUseStreamingUrlFilesContainer',
                showWhen: false,
                disableWhenHidden: true
            },
            {
                controllerId: 'subAllowFallbackToLowerQuality',
                dependentId: 'subQualityCheckWithUrlContainer',
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: 'subAppendDateToTitle',
                dependentId: 'subAppendTimeToTitleContainer',
                showWhen: true,
                disableWhenHidden: true
            },
            // Rules for Subscription Defaults
            {
                controllerId: 'defSubTreatNonEpisodesAsExtras',
                dependentId: 'defSubSaveTrailersContainer',
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: 'defSubTreatNonEpisodesAsExtras',
                dependentId: 'defSubSaveInterviewsContainer',
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: 'defSubTreatNonEpisodesAsExtras',
                dependentId: 'defSubSaveGenericExtrasContainer',
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: 'defSubTreatNonEpisodesAsExtras',
                dependentId: 'defSubSaveExtrasAsStrmContainer',
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: 'defSubEnforceSeries',
                dependentId: 'defSubAllowAbsoluteEpisodeNumberingContainer',
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: 'defSubUseStreamingUrlFiles',
                dependentId: 'defSubDownloadFullVideoForSecondaryAudioContainer',
                showWhen: false,
                disableWhenHidden: true
            },
            {
                controllerId: 'defSubDownloadFullVideoForSecondaryAudio',
                dependentId: 'defSubUseStreamingUrlFilesContainer',
                showWhen: false,
                disableWhenHidden: true
            },
            {
                controllerId: 'defSubAllowFallbackToLowerQuality',
                dependentId: 'defSubQualityCheckWithUrlContainer',
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: 'defSubAppendDateToTitle',
                dependentId: 'defSubAppendTimeToTitleContainer',
                showWhen: true,
                disableWhenHidden: true
            }
        ];
    }

    init() {
        const controllerIds = [...new Set(this.rules.map(rule => rule.controllerId))];
        controllerIds.forEach(id => {
            const controller = document.getElementById(id);
            if (controller) {
                controller.addEventListener('change', () => this.applyDependencies());
            }
        });
    }

    applyDependencies() {
        this.rules.forEach(rule => {
            const controller = document.getElementById(rule.controllerId);
            const dependentContainer = document.getElementById(rule.dependentId);

            if (controller && dependentContainer) {
                const shouldShow = controller.checked === rule.showWhen;

                // Ensure the container has the base animation class
                dependentContainer.classList.add('mvpl-animated-container');

                if (shouldShow) {
                    dependentContainer.classList.remove('mvpl-hidden');
                } else {
                    dependentContainer.classList.add('mvpl-hidden');
                }

                if (rule.disableWhenHidden && !shouldShow) {
                    const dependentInput = dependentContainer.querySelector('input[type="checkbox"]');
                    if (dependentInput) {
                        dependentInput.checked = false;
                    }
                }
            }
        });
    }
}

/**
 * Handles subscription editor logic, populating fields and gathering values.
 */
class SubscriptionEditor {
    /**
     * @param {MediathekPluginConfig} configInstance
     */
    constructor(configInstance) {
        this.config = configInstance;
    }

    /**
     * Updates the hover text (title attribute) for the subPath input.
     */
    updateSubPathHoverText() {
        const el = document.getElementById('subPath');
        if (!el) return;

        if (StringHelper.isNullOrWhitespace(el.value)) {
            const defaultMoviePath = window.MediathekViewDL.config.currentConfig.Paths.DefaultSubscriptionMoviePath || Language.General.NotConfigured;
            const defaultShowPath = window.MediathekViewDL.config.currentConfig.Paths.DefaultSubscriptionShowPath || Language.General.NotConfigured;
            const useTopicForMoviePath = window.MediathekViewDL.config.currentConfig.Paths.UseTopicForMoviePath;
            const alwaysCreateSubfolder = document.getElementById('subAlwaysCreateSubfolder').checked;
            const subName = document.getElementById('subName').value || Language.General.AboNamePlaceholder;

            const joinPath = (path, part) => {
                if (!path || path === Language.General.NotConfigured) return path;
                const separator = path.indexOf('\\') !== -1 ? '\\' : '/';
                if (path.endsWith('/') || path.endsWith('\\')) {
                    return path + part;
                }
                return path + separator + part;
            };

            const resolvedMoviePath = (useTopicForMoviePath || alwaysCreateSubfolder) ? joinPath(defaultMoviePath, subName) : defaultMoviePath;
            const resolvedShowPath = joinPath(defaultShowPath, subName);
            this.updatePathPlaceholderText(resolvedMoviePath, resolvedShowPath);
            el.title = Language.Subscription.UsedPaths + '\n' +
                Language.Subscription.Movies + ': ' + resolvedMoviePath + '\n' +
                Language.Subscription.Series + ': ' + resolvedShowPath;
        } else {
            el.title = Language.Subscription.SelectedPath + '\n' + el.value;
        }
    }

    updatePathPlaceholderText(defaultMoviePath = '-', defaultShowPath = '-') {
        const el = document.getElementById('subPath');
        if (!el) return;

        let message = Language.Subscription.DefaultPathUsed;
        message += Language.Subscription.Series + ': "' + defaultShowPath + '"';
        message += Language.Subscription.Movies + ': "' + defaultMoviePath + '"';
        el.placeholder = message;
    }

    /**
     * Populates the editor form with values from a subscription object.
     * @param {Object|null} sub - The subscription object or null for a new subscription.
     */
    setEditorValues(sub) {
        if (!sub) {
            // Reset values using SubscriptionDefaults
            const def = this.config.currentConfig.SubscriptionDefaults || {};
            const defDl = def.DownloadSettings || {};
            const defSearch = def.SearchSettings || {};
            const defSeries = def.SeriesSettings || {};
            const defMeta = def.MetadataSettings || {};
            const defAccess = def.AccessibilitySettings || {};

            document.getElementById('subId').value = "";
            document.getElementById('subName').value = "";
            document.getElementById('subOriginalLanguage').value = defMeta.OriginalLanguage || "";
            document.getElementById('subMinDuration').value = defSearch.MinDurationMinutes || "";
            document.getElementById('subMaxDuration').value = defSearch.MaxDurationMinutes || "";
            document.getElementById('subMinBroadcastDate').value = "";
            document.getElementById('subMaxBroadcastDate').value = "";
            document.getElementById('subPath').value = "";
            this.updateSubPathHoverText();

            document.getElementById('subEnforceSeries').checked = defSeries.EnforceSeriesParsing || false;
            document.getElementById('subCreateNfo').checked = defMeta.CreateNfo || false;
            document.getElementById('subAllowAudioDesc').checked = defAccess.AllowAudioDescription || false;
            document.getElementById('subAllowAbsoluteEpisodeNumbering').checked = defSeries.AllowAbsoluteEpisodeNumbering || false;
            document.getElementById('subAppendDateToTitle').checked = defMeta.AppendDateToTitle || false;
            document.getElementById('subAppendTimeToTitle').checked = defMeta.AppendTimeToTitle || false;
            document.getElementById('subAllowSignLanguage').checked = defAccess.AllowSignLanguage || false;
            document.getElementById('subTreatNonEpisodesAsExtras').checked = defSeries.TreatNonEpisodesAsExtras || false;
            document.getElementById('subSaveTrailers').checked = defSeries.SaveTrailers !== undefined ? defSeries.SaveTrailers : true;
            document.getElementById('subSaveInterviews').checked = defSeries.SaveInterviews !== undefined ? defSeries.SaveInterviews : true;
            document.getElementById('subSaveGenericExtras').checked = defSeries.SaveGenericExtras !== undefined ? defSeries.SaveGenericExtras : true;
            document.getElementById('subSaveExtrasAsStrm').checked = defSeries.SaveExtrasAsStrm || false;
            document.getElementById('subUseStreamingUrlFiles').checked = defDl.UseStreamingUrlFiles || false;
            document.getElementById('subDownloadFullVideoForSecondaryAudio').checked = defDl.DownloadFullVideoForSecondaryAudio || false;
            document.getElementById('subAlwaysCreateSubfolder').checked = defDl.AlwaysCreateSubfolder || false;
            document.getElementById('subEnhancedDuplicateDetection').checked = defDl.EnhancedDuplicateDetection || false;
            document.getElementById('subAutoUpgradeToHigherQuality').checked = defDl.AutoUpgradeToHigherQuality || false;
            document.getElementById('subAllowFallbackToLowerQuality').checked = defDl.AllowFallbackToLowerQuality !== undefined ? defDl.AllowFallbackToLowerQuality : true;
            document.getElementById('subQualityCheckWithUrl').checked = defDl.QualityCheckWithUrl || false;

            document.getElementById('queriesContainer').textContent = '';
            this.config.addQueryRow(null);

            // Force update dependencies to hide unchecked dependent fields
            this.config.dependencyManager.applyDependencies();
            return;
        }

        document.getElementById('subId').value = sub.Id || "";
        document.getElementById('subName').value = sub.Name;

        // Ensure nested objects exist to avoid errors
        const search = sub.Search || {};
        const download = sub.Download || {};
        const series = sub.Series || {};
        const metadata = sub.Metadata || {};
        const accessibility = sub.Accessibility || {};

        document.getElementById('subOriginalLanguage').value = metadata.OriginalLanguage || "";
        document.getElementById('subMinDuration').value = search.MinDurationMinutes || "";
        document.getElementById('subMaxDuration').value = search.MaxDurationMinutes || "";
        document.getElementById('subMinBroadcastDate').value = search.MinBroadcastDate ? search.MinBroadcastDate.split('T')[0] : "";
        document.getElementById('subMaxBroadcastDate').value = search.MaxBroadcastDate ? search.MaxBroadcastDate.split('T')[0] : "";
        document.getElementById('subPath').value = download.DownloadPath || "";
        this.updateSubPathHoverText();

        document.getElementById('subEnforceSeries').checked = series.EnforceSeriesParsing;
        document.getElementById('subCreateNfo').checked = metadata.CreateNfo !== undefined ? metadata.CreateNfo : false;
        document.getElementById('subAllowAudioDesc').checked = accessibility.AllowAudioDescription;
        document.getElementById('subAllowAbsoluteEpisodeNumbering').checked = series.AllowAbsoluteEpisodeNumbering;
        document.getElementById('subAppendDateToTitle').checked = metadata.AppendDateToTitle !== undefined ? metadata.AppendDateToTitle : false;
        document.getElementById('subAppendTimeToTitle').checked = metadata.AppendTimeToTitle !== undefined ? metadata.AppendTimeToTitle : false;
        document.getElementById('subAllowSignLanguage').checked = accessibility.AllowSignLanguage;
        document.getElementById('subAlwaysCreateSubfolder').checked = download.AlwaysCreateSubfolder || false;
        document.getElementById('subEnhancedDuplicateDetection').checked = download.EnhancedDuplicateDetection;
        document.getElementById('subAutoUpgradeToHigherQuality').checked = download.AutoUpgradeToHigherQuality !== undefined ? download.AutoUpgradeToHigherQuality : false;
        document.getElementById('subTreatNonEpisodesAsExtras').checked = series.TreatNonEpisodesAsExtras;
        document.getElementById('subSaveTrailers').checked = series.SaveTrailers !== undefined ? series.SaveTrailers : true;
        document.getElementById('subSaveInterviews').checked = series.SaveInterviews !== undefined ? series.SaveInterviews : true;
        document.getElementById('subSaveGenericExtras').checked = series.SaveGenericExtras !== undefined ? series.SaveGenericExtras : true;
        document.getElementById('subSaveExtrasAsStrm').checked = series.SaveExtrasAsStrm;
        document.getElementById('subUseStreamingUrlFiles').checked = download.UseStreamingUrlFiles;
        document.getElementById('subDownloadFullVideoForSecondaryAudio').checked = download.DownloadFullVideoForSecondaryAudio;
        document.getElementById('subAllowFallbackToLowerQuality').checked = download.AllowFallbackToLowerQuality !== undefined ? download.AllowFallbackToLowerQuality : true;
        document.getElementById('subQualityCheckWithUrl').checked = download.QualityCheckWithUrl !== undefined ? download.QualityCheckWithUrl : false;


        const queriesContainer = document.getElementById('queriesContainer');
        queriesContainer.textContent = '';
        const criteria = search.Criteria || [];
        if (criteria.length > 0) {
            criteria.forEach((c) => {
                this.config.addQueryRow(c);
            });
        } else {
            this.config.addQueryRow(null);
        }
    }

    /**
     * Collects values from the editor form to create a subscription object.
     * @returns {Object} The subscription object.
     */
    getEditorValues() {
        const id = document.getElementById('subId').value;
        const name = document.getElementById('subName').value;
        const originalLanguage = document.getElementById('subOriginalLanguage').value;
        const minDuration = document.getElementById('subMinDuration').value;
        const maxDuration = document.getElementById('subMaxDuration').value;
        const minBroadcastDate = document.getElementById('subMinBroadcastDate').value;
        const maxBroadcastDate = document.getElementById('subMaxBroadcastDate').value;
        const path = document.getElementById('subPath').value;
        const enforce = document.getElementById('subEnforceSeries').checked;
        const createNfo = document.getElementById('subCreateNfo').checked;
        const allowAudio = document.getElementById('subAllowAudioDesc').checked;
        const allowAbsolute = document.getElementById('subAllowAbsoluteEpisodeNumbering').checked;
        const appendDateToTitle = document.getElementById('subAppendDateToTitle').checked;
        const appendTimeToTitle = document.getElementById('subAppendTimeToTitle').checked;
        const allowSignLanguage = document.getElementById('subAllowSignLanguage').checked;
        const alwaysCreateSubfolder = document.getElementById('subAlwaysCreateSubfolder').checked;
        const enhancedDuplicateDetection = document.getElementById('subEnhancedDuplicateDetection').checked;
        const autoUpgradeToHigherQuality = document.getElementById('subAutoUpgradeToHigherQuality').checked;
        const treatNonEpisodesAsExtras = document.getElementById('subTreatNonEpisodesAsExtras').checked;
        const saveTrailers = document.getElementById('subSaveTrailers').checked;
        const saveInterviews = document.getElementById('subSaveInterviews').checked;
        const saveGenericExtras = document.getElementById('subSaveGenericExtras').checked;
        const saveExtrasAsStrm = document.getElementById('subSaveExtrasAsStrm').checked;
        const useStreamingUrlFiles = document.getElementById('subUseStreamingUrlFiles').checked;
        const downloadFullVideoForSecondaryAudio = document.getElementById('subDownloadFullVideoForSecondaryAudio').checked;
        const allowFallbackToLowerQuality = document.getElementById('subAllowFallbackToLowerQuality').checked;
        const qualityCheckWithUrl = document.getElementById('subQualityCheckWithUrl').checked;

        const criteria = [];
        document.querySelectorAll('#queriesContainer .mvpl-query-row').forEach(function (row) {
            const queryText = row.querySelector('.subQueryText').value;
            if (queryText) {
                const fields = [];
                row.querySelectorAll('.subQueryField:checked').forEach(function (fieldCheckbox) {
                    fields.push(fieldCheckbox.value);
                });
                criteria.push({Query: queryText, Fields: fields});
            }
        });

        return {
            Id: id,
            Name: name,
            Search: {
                Criteria: criteria,
                MinDurationMinutes: minDuration ? parseInt(minDuration, 10) : null,
                MaxDurationMinutes: maxDuration ? parseInt(maxDuration, 10) : null,
                MinBroadcastDate: minBroadcastDate ? new Date(minBroadcastDate).toISOString() : null,
                MaxBroadcastDate: maxBroadcastDate ? (() => {
                    const d = new Date(maxBroadcastDate);
                    d.setHours(23, 59, 59, 999);
                    return d.toISOString();
                })() : null,
            },
            Download: {
                DownloadPath: path,
                UseStreamingUrlFiles: useStreamingUrlFiles,
                DownloadFullVideoForSecondaryAudio: downloadFullVideoForSecondaryAudio,
                AlwaysCreateSubfolder: alwaysCreateSubfolder,
                AllowFallbackToLowerQuality: allowFallbackToLowerQuality,
                QualityCheckWithUrl: qualityCheckWithUrl,
                AutoUpgradeToHigherQuality: autoUpgradeToHigherQuality,
                EnhancedDuplicateDetection: enhancedDuplicateDetection,
            },
            Series: {
                EnforceSeriesParsing: enforce,
                AllowAbsoluteEpisodeNumbering: allowAbsolute,
                TreatNonEpisodesAsExtras: treatNonEpisodesAsExtras,
                SaveTrailers: saveTrailers,
                SaveInterviews: saveInterviews,
                SaveGenericExtras: saveGenericExtras,
                SaveExtrasAsStrm: saveExtrasAsStrm,
            },
            Metadata: {
                CreateNfo: createNfo,
                OriginalLanguage: originalLanguage,
                AppendDateToTitle: appendDateToTitle,
                AppendTimeToTitle: appendTimeToTitle,
            },
            Accessibility: {
                AllowAudioDescription: allowAudio,
                AllowSignLanguage: allowSignLanguage,
            }
        };
    }

    /**
     * Opens the subscription editor modal.
     * @param {Object|null} sub - The subscription to edit or null for new.
     * @param {string|null} titleText - Optional title override.
     */
    show(sub, titleText) {
        const editor = document.getElementById('subscriptionEditor');
        const title = document.getElementById('subEditorTitle');

        if (titleText) {
            title.innerText = titleText;
        } else {
            title.innerText = sub ? Language.Subscription.EditSubscription : Language.Subscription.CreateSubscription;
        }

        this.setEditorValues(sub);
        this.config.dependencyManager.applyDependencies();

        editor.style.display = 'block';
        editor.scrollIntoView({behavior: 'smooth'});
    }

    /**
     * Closes the subscription editor modal.
     */
    close() {
        document.getElementById('subscriptionEditor').style.display = 'none';
    }
}

/**
 * Main configuration class for the plugin.
 */
class MediathekPluginConfig {
    constructor() {
        this.debug = false;
        this.pluginId = "a31b415a-5264-419d-b152-8c8192a54994";
        this.pluginName = "MediathekViewDL";
        this.dom = new DomHelper();
        this.searchController = new SearchController(this);
        this.downloadsController = new DownloadsController(this);
        this.liveTvController = new SetupLiveTvController(this);
        this.adoptionController = new AdoptionController(this);
        this.dependencyManager = new DependencyManager();
        this.currentConfig = null;
        this.currentItemForAdvancedDl = null;
        this.subscriptionEditor = new SubscriptionEditor(this);
    }

    // --- Helper Functions ---
    debugLog(message, ...optionalParams) {
        if (this.debug) {
            console.log("[MediathekViewDL DEBUG] " + message, ...optionalParams);
        }
    }

    setupAutoGrowInputs() {
        const inputs = [
            'txtSearchCombined',
            'txtSearchQuery',
            'txtSearchTopic',
            'txtSearchChannel'
        ];

        inputs.forEach(id => {
            const el = document.getElementById(id);
            if (el) {
                this.enableAutoGrow(el);
            }
        });
    }

    enableAutoGrow(input) {
        if (!input) return;
        const minWidth = 150; // Match duration field width

        // Create measuring span if not exists
        if (!this.measureSpan) {
            this.measureSpan = document.createElement('span');
            this.measureSpan.style.visibility = 'hidden';
            this.measureSpan.style.position = 'absolute';
            this.measureSpan.style.whiteSpace = 'pre';
            this.measureSpan.style.top = '-9999px';
            document.body.appendChild(this.measureSpan);
        }

        const updateWidth = () => {
            // Copy styles that affect width
            const styles = window.getComputedStyle(input);
            this.measureSpan.style.fontFamily = styles.fontFamily;
            this.measureSpan.style.fontSize = styles.fontSize;
            this.measureSpan.style.fontWeight = styles.fontWeight;
            this.measureSpan.style.letterSpacing = styles.letterSpacing;
            this.measureSpan.style.textTransform = styles.textTransform;

            const text = input.value || input.placeholder || '';
            this.measureSpan.textContent = text;

            // Add some padding (e.g. 25px) to account for internal padding of input
            let newWidth = Math.max(minWidth, this.measureSpan.offsetWidth + 25);
            newWidth = Math.min(newWidth, 500);
            input.style.width = newWidth + 'px';
            input.style.flexGrow = '0'; // Ensure it doesn't grow via flex
        };

        input.addEventListener('input', updateWidth);
        // Also update on change or blur just in case
        input.addEventListener('change', updateWidth);

        // Initial update
        setTimeout(updateWidth, 0);
    }

    // --- Core Logic ---

    /**
     * Loads the configuration from the server.
     */
    loadConfig() {
        // Check for initialization errors first
        const errorUrl = ApiClient.getUrl('/' + this.pluginName + '/InitializationError');
        ApiClient.getJSON(errorUrl).then((errorMessage) => {
            const overlay = document.getElementById('mvpl-critical-error-overlay');
            const errorMsg = document.getElementById('mvpl-critical-error-message');
            if (errorMessage) {
                overlay.classList.remove('mvpl-hidden');
                errorMsg.textContent = errorMessage;
                // Also disable the main form to be safe
                document.getElementById('MediathekGeneralConfigForm').style.pointerEvents = 'none';
                document.getElementById('MediathekGeneralConfigForm').style.opacity = '0.5';
            } else {
                overlay.classList.add('mvpl-hidden');
            }
        }).catch(err => console.error(Language.Subscription.ErrorInitializationStatus, err));

        // noinspection JSUnresolvedReference
        Dashboard.showLoadingMsg();
        // noinspection JSUnresolvedReference
        ApiClient.getPluginConfiguration(this.pluginId).then((config) => {
            this.currentConfig = config;

            document.querySelector('#txtDefaultDownloadPath').value = config.Paths.DefaultDownloadPath || "";
            document.querySelector('#txtDefaultSubscriptionShowPath').value = config.Paths.DefaultSubscriptionShowPath || "";
            document.querySelector('#txtDefaultSubscriptionMoviePath').value = config.Paths.DefaultSubscriptionMoviePath || "";
            document.querySelector('#txtDefaultManualShowPath').value = config.Paths.DefaultManualShowPath || "";
            document.querySelector('#txtDefaultManualMoviePath').value = config.Paths.DefaultManualMoviePath || "";
            document.querySelector('#txtTempDownloadPath').value = config.Paths.TempDownloadPath || "";
            document.querySelector('#chkDownloadSubtitles').checked = config.Download.DownloadSubtitles;
            document.querySelector('#chkAllowUnknownDomains').checked = config.Network.AllowUnknownDomains;
            document.querySelector('#chkAllowHttp').checked = config.Network.AllowHttp;
            document.querySelector('#chkScanLibraryAfterDownload').checked = config.Download.ScanLibraryAfterDownload;
            document.querySelector('#chkEnableDirectAudioExtraction').checked = config.Download.EnableDirectAudioExtraction;
            document.querySelector('#chkEnableStrmCleanup').checked = config.Maintenance.EnableStrmCleanup;
            document.querySelector('#chkFetchStreamSizes').checked = config.Search.FetchStreamSizes;
            document.querySelector('#chkSearchInFutureBroadcasts').checked = config.Search.SearchInFutureBroadcasts;
            document.querySelector('#chkAllowDownloadOnUnknownDiskSpace').checked = config.Maintenance.AllowDownloadOnUnknownDiskSpace;
            document.querySelector('#txtMinFreeDiskSpaceMiB').value = config.Download.MinFreeDiskSpaceBytes ? (config.Download.MinFreeDiskSpaceBytes / (1024 * 1024)) : "";
            document.querySelector('#txtMaxBandwidthMBits').value = config.Download.MaxBandwidthMBits || 0;
            document.querySelector('#lblLastRun').innerText = config.LastRun ? new Date(config.LastRun).toLocaleString() : Language.Subscription.Never;
            document.querySelector('#chkMoviePathWithTopic').checked = config.Paths.UseTopicForMoviePath;

            // Load Subscription Defaults
            const def = config.SubscriptionDefaults || {};
            const defDl = def.DownloadSettings || {};
            const defSearch = def.SearchSettings || {};
            const defSeries = def.SeriesSettings || {};
            const defMeta = def.MetadataSettings || {};
            const defAccess = def.AccessibilitySettings || {};

            document.querySelector('#defSubMinDuration').value = defSearch.MinDurationMinutes || "";
            document.querySelector('#defSubMaxDuration').value = defSearch.MaxDurationMinutes || "";
            document.querySelector('#defSubUseStreamingUrlFiles').checked = defDl.UseStreamingUrlFiles || false;
            document.querySelector('#defSubDownloadFullVideoForSecondaryAudio').checked = defDl.DownloadFullVideoForSecondaryAudio || false;
            document.querySelector('#defSubAlwaysCreateSubfolder').checked = defDl.AlwaysCreateSubfolder || false;
            document.querySelector('#defSubEnhancedDuplicateDetection').checked = defDl.EnhancedDuplicateDetection || false;
            document.querySelector('#defSubAutoUpgradeToHigherQuality').checked = defDl.AutoUpgradeToHigherQuality || false;
            document.querySelector('#defSubAllowFallbackToLowerQuality').checked = defDl.AllowFallbackToLowerQuality !== undefined ? defDl.AllowFallbackToLowerQuality : true;
            document.querySelector('#defSubQualityCheckWithUrl').checked = defDl.QualityCheckWithUrl || false;

            document.querySelector('#defSubEnforceSeries').checked = defSeries.EnforceSeriesParsing || false;
            document.querySelector('#defSubAllowAbsoluteEpisodeNumbering').checked = defSeries.AllowAbsoluteEpisodeNumbering || false;
            document.querySelector('#defSubTreatNonEpisodesAsExtras').checked = defSeries.TreatNonEpisodesAsExtras || false;
            document.querySelector('#defSubSaveExtrasAsStrm').checked = defSeries.SaveExtrasAsStrm || false;
            document.querySelector('#defSubSaveTrailers').checked = defSeries.SaveTrailers !== undefined ? defSeries.SaveTrailers : true;
            document.querySelector('#defSubSaveInterviews').checked = defSeries.SaveInterviews !== undefined ? defSeries.SaveInterviews : true;
            document.querySelector('#defSubSaveGenericExtras').checked = defSeries.SaveGenericExtras !== undefined ? defSeries.SaveGenericExtras : true;

            document.querySelector('#defSubOriginalLanguage').value = defMeta.OriginalLanguage || "";
            document.querySelector('#defSubCreateNfo').checked = defMeta.CreateNfo || false;
            document.querySelector('#defSubAppendDateToTitle').checked = defMeta.AppendDateToTitle || false;
            document.querySelector('#defSubAppendTimeToTitle').checked = defMeta.AppendTimeToTitle || false;

            document.querySelector('#defSubAllowAudioDesc').checked = defAccess.AllowAudioDescription || false;
            document.querySelector('#defSubAllowSignLanguage').checked = defAccess.AllowSignLanguage || false;

            this.renderSubscriptionsList();
            this.adoptionController.populateAbos(config.Subscriptions);
            // noinspection JSUnresolvedReference
            Dashboard.hideLoadingMsg();
        });
    }

    /**
     * Saves the global configuration to the server.
     */
    saveGlobalConfig() {
        // noinspection JSUnresolvedReference
        Dashboard.showLoadingMsg();
        // noinspection JSUnresolvedReference
        ApiClient.updatePluginConfiguration(this.pluginId, this.currentConfig).then((result) => {
            // noinspection JSUnresolvedReference
            Dashboard.processPluginConfigurationUpdateResult(result);
            Helper.showToast(Language.General.SettingsSaved);
            this.loadConfig();
        });
    }

    /**
     * Switches the visible tab.
     * @param {string} tabId - The ID suffix of the tab to show ('search', 'settings', 'subscriptions').
     */
    switchTab(tabId) {
        document.querySelectorAll('.mvpl-tab-content').forEach(el => el.style.display = 'none');
        document.getElementById('tab-' + tabId).style.display = 'block';

        const buttons = document.querySelectorAll('.mvpl-tabs-spacer button');
        buttons.forEach(btn => {
            btn.classList.remove('selected');
            btn.setAttribute('aria-selected', 'false');
        });

        const selectedBtn = document.getElementById('mvpl-btn-tab-' + tabId);
        if (selectedBtn) {
            selectedBtn.classList.add('selected');
            selectedBtn.setAttribute('aria-selected', 'true');
        }

        if (tabId === 'downloads') {
            this.downloadsController.startPolling();
        } else {
            this.downloadsController.stopPolling();
        }
    }

    // --- SEARCH LOGIC ---

    createListItem(title, bodyText1, bodyText2, actions) {
        const listItem = document.createElement('div');
        listItem.classList.add('listItem', 'listItem-border');

        const body = document.createElement('div');
        body.classList.add('listItemBody', 'two-line');

        const titleEl = document.createElement('h3');
        titleEl.classList.add('listItemBodyText');
        titleEl.textContent = title;

        const text1El = document.createElement('div');
        text1El.classList.add('listItemBodyText', 'secondary');
        if (typeof bodyText1 === 'string') {
            text1El.textContent = bodyText1;
        } else if (bodyText1 instanceof Node) {
            text1El.appendChild(bodyText1);
        }

        const text2El = document.createElement('div');
        text2El.classList.add('listItemBodyText', 'secondary');
        if (typeof bodyText2 === 'string') {
            text2El.textContent = bodyText2;
        } else if (bodyText2 instanceof Node) {
            text2El.appendChild(bodyText2);
        }

        body.appendChild(titleEl);
        body.appendChild(text1El);
        body.appendChild(text2El);

        listItem.appendChild(body);

        if (actions) {
            listItem.appendChild(actions);
        }

        return listItem;
    }


    // ---SUBSCRIPTION LOGIC ---
    renderSubscriptionsList() {
        const list = document.getElementById('subscriptionList');
        list.textContent = "";

        if (!this.currentConfig.Subscriptions || this.currentConfig.Subscriptions.length === 0) {
            const noSubs = document.createElement('p');
            noSubs.textContent = Language.Subscription.NoActiveSubscriptions;
            list.appendChild(noSubs);
            return;
        }

        this.currentConfig.Subscriptions.forEach((sub) => {
            // Handle IsEnabled default true if undefined
            if (sub.IsEnabled === undefined) sub.IsEnabled = true;

            const search = sub.Search || {};
            const queriesSummary = (search.Criteria || []).map(function (q) {
                return q.Query;
            }).join(', ');
            const lastDownloadText = sub.LastDownloadedTimestamp ? new Date(sub.LastDownloadedTimestamp).toLocaleString() : Language.Subscription.Never;

            const actions = document.createElement('div');
            actions.classList.add('flex-gap-5');

            // Toggle Button
            const toggleIcon = sub.IsEnabled ? Icons.ToggleOn : Icons.ToggleOff;
            const toggleTitle = sub.IsEnabled ? Language.Subscription.Disable : Language.Subscription.Enable;
            actions.appendChild(this.dom.createIconButton(toggleIcon, toggleTitle, () => this.toggleSubscription(sub.Id)));

            actions.appendChild(this.dom.createIconButton(Icons.Refresh, Language.Subscription.ResetProcessedItems, () => this.resetProcessedItems(sub.Id)));
            actions.appendChild(this.dom.createIconButton(Icons.Edit, Language.Subscription.Edit, () => this.subscriptionEditor.show(sub)));
            actions.appendChild(this.dom.createIconButton(Icons.Delete, Language.Subscription.Delete, () => this.deleteSubscription(sub.Id)));

            // Add Status to title
            const statusText = sub.IsEnabled ? "" : Language.Subscription.Disabled;
            const title = sub.Name + statusText;
            const bodyText1 = Language.Subscription.Queries + queriesSummary;
            const bodyText2 = Language.Subscription.LastDownload + lastDownloadText;

            const listItem = this.createListItem(title, bodyText1, bodyText2, actions);

            // Visual cue for disabled state
            if (!sub.IsEnabled) {
                listItem.classList.add('sub-disabled');
            }

            list.appendChild(listItem);
        });

        this.adoptionController.populateAbos(this.currentConfig.Subscriptions);
    }

    toggleSubscription(id) {
        const idx = this.currentConfig.Subscriptions.findIndex(function (s) {
            return s.Id === id;
        });
        if (idx > -1) {
            // Toggle
            if (this.currentConfig.Subscriptions[idx].IsEnabled === undefined) {
                this.currentConfig.Subscriptions[idx].IsEnabled = false; // Was true (implicit), now false
            } else {
                this.currentConfig.Subscriptions[idx].IsEnabled = !this.currentConfig.Subscriptions[idx].IsEnabled;
            }

            this.saveGlobalConfig();
        }
    }

    resetProcessedItems(id) {
        Helper.confirmationPopup(Language.Subscription.ConfirmResetProcessedItemsMessage, Language.Subscription.ResetProcessedItems, (confirmed) => {
            if (confirmed) {
                // noinspection JSUnresolvedReference
                Dashboard.showLoadingMsg();
                // noinspection JSUnresolvedReference
                ApiClient.ajax({
                    type: "POST",
                    url: ApiClient.getUrl('/' + this.pluginName + '/ResetProcessedItems?subscriptionId=' + id),
                }).then((result) => {
                    // noinspection JSUnresolvedReference
                    Dashboard.hideLoadingMsg();
                    Helper.showToast(Language.Subscription.ProcessedItemsReset);
                    this.loadConfig(); // Refresh the configuration to update the UI
                }).catch((err) => {
                    // noinspection JSUnresolvedReference
                    Dashboard.hideLoadingMsg();
                    Helper.showError(err, Language.Subscription.ErrorResettingProcessedItems);
                });
            }
        });
    }

    addQueryRow(query) {
        if (query == null) {
            query = {Query: '', Fields: ['Title', 'Topic']};
        }
        const queryText = query ? query.Query : '';
        const fields = query ? query.Fields : ['Title', 'Topic'];

        const input = this.dom.create('input', {
            type: 'text',
            className: 'subQueryText',
            value: queryText,
            attributes: {
                'is': 'emby-input',
                'placeholder': Language.Subscription.SearchText,
                'required': 'true'
            }
        });

        const cbTitle = this.dom.createCheckbox(Language.Subscription.Title, fields.includes('Title'), {
            value: 'Title',
            className: 'subQueryField'
        });
        const cbTopic = this.dom.createCheckbox(Language.Subscription.Topic, fields.includes('Topic'), {
            value: 'Topic',
            className: 'subQueryField'
        });
        const cbDescription = this.dom.createCheckbox(Language.Subscription.Description, fields.includes('Description'), {
            value: 'Description',
            className: 'subQueryField'
        });
        const cbChannel = this.dom.createCheckbox(Language.Subscription.Channel, fields.includes('Channel'), {
            value: 'Channel',
            className: 'subQueryField'
        });

        const removeBtn = this.dom.createIconButton(Icons.Remove, Language.Subscription.RemoveQuery, (e) => {
            e.target.closest('.mvpl-query-row').remove();
        });
        removeBtn.classList.add('btnRemoveQuery');

        const newRow = this.dom.create('div', {
            className: 'mvpl-query-row',
            children: [
                this.dom.create('div', {className: 'flex-grow', children: [input]}),
                this.dom.create('div', {
                    className: 'query-checkboxes',
                    children: [cbTitle, cbTopic, cbDescription, cbChannel]
                }),
                removeBtn
            ]
        });

        document.getElementById('queriesContainer').appendChild(newRow);
    }

    saveSubscription() {
        const subData = this.subscriptionEditor.getEditorValues();

        if (!subData.Search || !subData.Search.Criteria || subData.Search.Criteria.length === 0) {
            Helper.showToast(Language.Subscription.DefineAtLeastOneQuery);
            return;
        }

        if (!this.currentConfig.Subscriptions) this.currentConfig.Subscriptions = [];

        if (subData.Id) {
            const idx = this.currentConfig.Subscriptions.findIndex(function (s) {
                return s.Id === subData.Id;
            });
            if (idx > -1) {
                // Keep existing ID logic if needed, but here subData already has ID from hidden input if set
                var existingId = this.currentConfig.Subscriptions[idx].Id;

                // Preserve IsEnabled state
                var existingIsEnabled = this.currentConfig.Subscriptions[idx].IsEnabled;
                if (existingIsEnabled === undefined) existingIsEnabled = true;

                this.currentConfig.Subscriptions[idx] = subData;
                this.currentConfig.Subscriptions[idx].Id = existingId; // Ensure ID consistency
                this.currentConfig.Subscriptions[idx].IsEnabled = existingIsEnabled;
            }
        } else {
            subData.Id = Helper.genUUID();
            subData.IsEnabled = true; // Default enabled for new subs
            this.currentConfig.Subscriptions.push(subData);
        }

        this.saveGlobalConfig();
        this.subscriptionEditor.close();
        this.renderSubscriptionsList();
    }

    deleteSubscription(id) {
        Helper.confirmationPopup(Language.Subscription.ConfirmDelete, Language.Subscription.ConfirmDeleteTitle, (confirmed) => {
            if (confirmed) {
                this.currentConfig.Subscriptions = this.currentConfig.Subscriptions.filter(function (s) {
                    return s.Id !== id;
                });
                this.saveGlobalConfig();
                this.renderSubscriptionsList();
            }
        });
    }

    /**
     * Parses a search item into video information.
     * @param {Object} item - The search result item.
     * @param {Function} callback - The callback function to handle the result.
     */
    getVideoInfo(item, callback) {
        const url = ApiClient.getUrl('/' + this.pluginName + '/Items/Parse');
        ApiClient.ajax({
            type: "POST",
            url: url,
            data: JSON.stringify(item),
            contentType: 'application/json',
            dataType: 'json'
        }).then((result) => {
            if (typeof result === 'string') {
                try {
                    result = JSON.parse(result);
                } catch (e) {
                    console.error("Failed to parse VideoInfo JSON", e);
                }
            }
            callback(result);
        }).catch((err) => {
            console.error("Error parsing video info", err);
        });
    }

    /**
     * Gets the recommended download path for a given video info.
     * @param {Object} videoInfo - The parsed video information.
     * @param {Function} callback - The callback function to handle the result.
     */
    getRecommendedPath(videoInfo, callback) {
        const url = ApiClient.getUrl('/' + this.pluginName + '/Items/RecommendedPath');
        ApiClient.ajax({
            type: "POST",
            url: url,
            data: JSON.stringify(videoInfo),
            contentType: 'application/json',
            dataType: 'json'
        }).then((result) => {
            if (typeof result === 'string') {
                try {
                    result = JSON.parse(result);
                } catch (e) {
                    console.error("Failed to parse RecommendedPath JSON", e);
                }
            }
            callback(result);
        }).catch((err) => {
            console.error("Error getting recommended path", err);
        });
    }

    /**
     * Opens the advanced download dialog for a search item.
     * @param {Object} item - The search result item.
     */
    openAdvancedDownloadDialog(item) {
        this.currentItemForAdvancedDl = item;
        if (!this.currentItemForAdvancedDl) return;

        document.getElementById(DomIds.AdvancedDownload.Title).innerText = Language.AdvancedDownload.TitlePrefix + this.currentItemForAdvancedDl.Title;
        document.getElementById('advancedDownloadIndex').value = ""; // Not needed anymore contextually but keeping element

        document.getElementById(DomIds.AdvancedDownload.Path).value = this.currentConfig.Paths.DefaultDownloadPath || '';

        let proposedFilename = (this.currentItemForAdvancedDl.Topic || Language.Search.Video) + " - " + (this.currentItemForAdvancedDl.Title || Language.Search.Video);
        proposedFilename = proposedFilename.replace(/["\/\\?%*:|<>]/g, '-') + '.mp4';
        document.getElementById(DomIds.AdvancedDownload.Filename).value = proposedFilename;

        this.getVideoInfo(item, (videoInfo) => {
            console.log("Got VideoInfo: ", videoInfo);
            this.getRecommendedPath(videoInfo, (recommended) => {
                console.log("Got RecommendedPath: ", recommended);
                if (recommended) {
                    if (recommended.FileName) {
                        document.getElementById('advDlFilename').value = recommended.FileName;
                    }
                    if (recommended.Path) {
                        document.getElementById('advDlPath').value = recommended.Path;
                    }
                }
            });
        });


        let advDlSub = document.getElementById('advDlSubtitles');
        let advDlSubDesc = document.getElementById('advDlSubtitlesDesc');
        const subtitleUrls = this.currentItemForAdvancedDl.SubtitleUrls || [];
        if (subtitleUrls.length === 0) {
            advDlSub.checked = false;
            advDlSub.disabled = true;
            advDlSubDesc.textContent = Language.Search.NoSubtitles;
        } else {
            advDlSub.disabled = false;
            advDlSubDesc.textContent = "";
        }


        document.getElementById('advancedDownloadModal').style.display = 'flex';
    }

    closeAdvancedDownloadDialog() {
        document.getElementById('advancedDownloadModal').style.display = 'none';
    }

    performAdvancedDownload() {
        if (!this.currentItemForAdvancedDl) return;

        const downloadOptions = {
            item: this.currentItemForAdvancedDl,
            downloadPath: document.getElementById('advDlPath').value,
            fileName: document.getElementById('advDlFilename').value,
            downloadSubtitles: document.getElementById('advDlSubtitles').checked
        };

        const url = ApiClient.getUrl('/' + this.pluginName + '/AdvancedDownload');

        Dashboard.showLoadingMsg();
        ApiClient.ajax({
            type: "POST",
            url: url,
            data: JSON.stringify(downloadOptions),
            contentType: 'application/json'
        }).then((result) => {
            Dashboard.hideLoadingMsg();
            this.closeAdvancedDownloadDialog();
            Helper.showToast(Language.Search.DownloadStarted(this.currentItemForAdvancedDl.Title));
        }).catch((err) => {
            Dashboard.hideLoadingMsg();
            Helper.showError(err, Language.Search.ErrorDownloadStart);
        });
    }

    testSubscription() {
        const subData = this.subscriptionEditor.getEditorValues();
        if (!subData.Search || !subData.Search.Criteria || subData.Search.Criteria.length === 0) {
            Helper.showToast(Language.Subscription.DefineAtLeastOneQuery);
            return;
        }

        // If ID is empty (new subscription), generate a temporary one for the backend to accept the object
        if (!subData.Id) {
            subData.Id = Helper.genUUID();
        }

        const url = ApiClient.getUrl('/' + this.pluginName + '/TestSubscription');

        Dashboard.showLoadingMsg();
        ApiClient.ajax({
            type: "POST",
            url: url,
            data: JSON.stringify(subData),
            contentType: 'application/json',
            dataType: 'json'
        }).then((results) => {
            Dashboard.hideLoadingMsg();
            if (typeof results === 'string') {
                try {
                    results = JSON.parse(results);
                } catch (e) {
                    console.error("Failed to parse JSON results", e);
                }
            }
            this.renderTestResults(results);
            document.getElementById('testSubscriptionModal').style.display = 'flex';
        }).catch((err) => {
            Dashboard.hideLoadingMsg();
            console.error("Test subscription error:", err);
            Helper.showError(err, Language.Search.ErrorTestingAbo);
        });
    }

    closeTestSubscriptionDialog() {
        document.getElementById('testSubscriptionModal').style.display = 'none';
    }

    renderTestResults(results) {
        const container = document.getElementById('testSubscriptionResults');
        const countContainer = document.getElementById('testSubscriptionCount');
        container.textContent = "";
        countContainer.textContent = "";

        if (!results || results.length === 0) {
            const noRes = document.createElement('p');
            noRes.textContent = Language.Search.NoTestHits;
            container.appendChild(noRes);
            return;
        }

                    countContainer.textContent = Language.Search.TestResultsCount(results.length);
        const paperList = document.createElement('div');
        paperList.classList.add('paperList');

        results.forEach((item) => {
            const durationSeconds = StringHelper.parseTimeSpan(item.Duration);
            const durationStr = Math.floor(durationSeconds / 60) + " Min";
            const title = item.Title;
            const bodyText1 = item.Channel + ' | ' + item.Topic + ' | ' + durationStr;
            const bodyText2 = item.Description || '';

            paperList.appendChild(this.createListItem(title, bodyText1, bodyText2, null));
        });
        container.appendChild(paperList);
    }

    /**
     * Binds all event listeners for static HTML elements.
     */
    bindEvents() {
        // Page Show
        document.querySelector('#MediathekViewDLConfigPage').addEventListener('pageshow', () => {
            this.loadConfig();
        });

        // Tab Navigation
        document.getElementById('mvpl-btn-tab-search').addEventListener('click', () => this.switchTab('search'));
        document.getElementById('mvpl-btn-tab-settings').addEventListener('click', () => this.switchTab('settings'));
        document.getElementById('mvpl-btn-tab-subscriptions').addEventListener('click', () => this.switchTab('subscriptions'));
        document.getElementById('mvpl-btn-tab-downloads').addEventListener('click', () => this.switchTab('downloads'));
        document.getElementById('mvpl-btn-tab-adoption').addEventListener('click', () => this.switchTab('adoption'));

        // Main Config Form
        document.getElementById('MediathekGeneralConfigForm').addEventListener('submit', (e) => {
            e.preventDefault();
            this.currentConfig.Paths.DefaultDownloadPath = document.querySelector('#txtDefaultDownloadPath').value;
            this.currentConfig.Paths.DefaultSubscriptionShowPath = document.querySelector('#txtDefaultSubscriptionShowPath').value;
            this.currentConfig.Paths.DefaultSubscriptionMoviePath = document.querySelector('#txtDefaultSubscriptionMoviePath').value;
            this.currentConfig.Paths.DefaultManualShowPath = document.querySelector('#txtDefaultManualShowPath').value;
            this.currentConfig.Paths.DefaultManualMoviePath = document.querySelector('#txtDefaultManualMoviePath').value;
            this.currentConfig.Paths.TempDownloadPath = document.querySelector('#txtTempDownloadPath').value;

            this.currentConfig.Download.DownloadSubtitles = document.querySelector('#chkDownloadSubtitles').checked;
            this.currentConfig.Network.AllowUnknownDomains = document.querySelector('#chkAllowUnknownDomains').checked;
            this.currentConfig.Network.AllowHttp = document.querySelector('#chkAllowHttp').checked;
            this.currentConfig.Download.ScanLibraryAfterDownload = document.querySelector('#chkScanLibraryAfterDownload').checked;
            this.currentConfig.Download.EnableDirectAudioExtraction = document.querySelector('#chkEnableDirectAudioExtraction').checked;
            this.currentConfig.Maintenance.EnableStrmCleanup = document.querySelector('#chkEnableStrmCleanup').checked;
            this.currentConfig.Search.FetchStreamSizes = document.querySelector('#chkFetchStreamSizes').checked;
            this.currentConfig.Search.SearchInFutureBroadcasts = document.querySelector('#chkSearchInFutureBroadcasts').checked;
            this.currentConfig.Maintenance.AllowDownloadOnUnknownDiskSpace = document.querySelector('#chkAllowDownloadOnUnknownDiskSpace').checked;

            const minFreeSpaceMiB = parseInt(document.querySelector('#txtMinFreeDiskSpaceMiB').value, 10);
            this.currentConfig.Download.MinFreeDiskSpaceBytes = isNaN(minFreeSpaceMiB) ? (1.5 * 1024 * 1024 * 1024) : (minFreeSpaceMiB * 1024 * 1024);
            this.currentConfig.Paths.UseTopicForMoviePath = document.querySelector('#chkMoviePathWithTopic').checked;

            const maxBandwidth = parseInt(document.querySelector('#txtMaxBandwidthMBits').value, 10);
            this.currentConfig.Download.MaxBandwidthMBits = isNaN(maxBandwidth) ? 0 : maxBandwidth;

            // Save Subscription Defaults
            this.currentConfig.SubscriptionDefaults = {
                DownloadSettings: {
                    UseStreamingUrlFiles: document.querySelector('#defSubUseStreamingUrlFiles').checked,
                    DownloadFullVideoForSecondaryAudio: document.querySelector('#defSubDownloadFullVideoForSecondaryAudio').checked,
                    AlwaysCreateSubfolder: document.querySelector('#defSubAlwaysCreateSubfolder').checked,
                    EnhancedDuplicateDetection: document.querySelector('#defSubEnhancedDuplicateDetection').checked,
                    AutoUpgradeToHigherQuality: document.querySelector('#defSubAutoUpgradeToHigherQuality').checked,
                    AllowFallbackToLowerQuality: document.querySelector('#defSubAllowFallbackToLowerQuality').checked,
                    QualityCheckWithUrl: document.querySelector('#defSubQualityCheckWithUrl').checked
                },
                SearchSettings: {
                    MinDurationMinutes: document.querySelector('#defSubMinDuration').value ? parseInt(document.querySelector('#defSubMinDuration').value, 10) : null,
                    MaxDurationMinutes: document.querySelector('#defSubMaxDuration').value ? parseInt(document.querySelector('#defSubMaxDuration').value, 10) : null
                },
                SeriesSettings: {
                    EnforceSeriesParsing: document.querySelector('#defSubEnforceSeries').checked,
                    AllowAbsoluteEpisodeNumbering: document.querySelector('#defSubAllowAbsoluteEpisodeNumbering').checked,
                    TreatNonEpisodesAsExtras: document.querySelector('#defSubTreatNonEpisodesAsExtras').checked,
                    SaveExtrasAsStrm: document.querySelector('#defSubSaveExtrasAsStrm').checked,
                    SaveTrailers: document.querySelector('#defSubSaveTrailers').checked,
                    SaveInterviews: document.querySelector('#defSubSaveInterviews').checked,
                    SaveGenericExtras: document.querySelector('#defSubSaveGenericExtras').checked
                },
                MetadataSettings: {
                    OriginalLanguage: document.querySelector('#defSubOriginalLanguage').value,
                    CreateNfo: document.querySelector('#defSubCreateNfo').checked,
                    AppendDateToTitle: document.querySelector('#defSubAppendDateToTitle').checked,
                    AppendTimeToTitle: document.querySelector('#defSubAppendTimeToTitle').checked
                },
                AccessibilitySettings: {
                    AllowAudioDescription: document.querySelector('#defSubAllowAudioDesc').checked,
                    AllowSignLanguage: document.querySelector('#defSubAllowSignLanguage').checked
                }
            };

            this.subscriptionEditor.updateSubPathHoverText();
            this.saveGlobalConfig();
            return false;
        });

        // Path selector in main config
        document.getElementById('btnSelectPath').addEventListener('click', () => {
            Helper.openFolderDialog('txtDefaultDownloadPath', Language.General.SelectGlobalDefaultDownloadPath);
        });
        document.getElementById('btnSelectSubscriptionShowPath').addEventListener('click', () => {
            Helper.openFolderDialog('txtDefaultSubscriptionShowPath', Language.General.SelectDefaultShowPathAbo);
        });
        document.getElementById('btnSelectSubscriptionMoviePath').addEventListener('click', () => {
            Helper.openFolderDialog('txtDefaultSubscriptionMoviePath', Language.General.SelectDefaultMoviePathAbo);
        });
        document.getElementById('btnSelectManualShowPath').addEventListener('click', () => {
            Helper.openFolderDialog('txtDefaultManualShowPath', Language.General.SelectDefaultShowPathManual);
        });
        document.getElementById('btnSelectManualMoviePath').addEventListener('click', () => {
            Helper.openFolderDialog('txtDefaultManualMoviePath', Language.General.SelectDefaultMoviePathManual);
        });
        document.getElementById('btnSelectTempPath').addEventListener('click', () => {
            Helper.openFolderDialog('txtTempDownloadPath', Language.General.SelectTempDownloadPath);
        });

        document.getElementById('chkMoviePathWithTopic').addEventListener('change', (e) => {
            this.currentConfig.Paths.UseTopicForMoviePath = e.target.checked;
            this.subscriptionEditor.updateSubPathHoverText();
        });

        // Path selectors in subscription editor
        document.getElementById('btnSelectSubPath').addEventListener('click', () => {
            Helper.openFolderDialog('subPath', Language.Subscription.SelectAboPath);
            this.subscriptionEditor.updateSubPathHoverText();
        });
        document.getElementById('subPath').addEventListener('input', () => {
            this.subscriptionEditor.updateSubPathHoverText();
        });
        document.getElementById('subName').addEventListener('input', () => {
            this.subscriptionEditor.updateSubPathHoverText();
        });
        document.getElementById('subAlwaysCreateSubfolder').addEventListener('change', () => {
            this.subscriptionEditor.updateSubPathHoverText();
        });
        // Path selector in advanced download dialog
        document.getElementById(DomIds.AdvancedDownload.BtnSelectPath).addEventListener('click', () => {
            Helper.openFolderDialog(DomIds.AdvancedDownload.Path, Language.AdvancedDownload.SelectDownloadPath);
        });

        // Subscription Management
        document.getElementById('mvpl-btn-new-sub').addEventListener('click', () => {
            this.subscriptionEditor.show(null);
        });

        document.getElementById('mvpl-form-subscription').addEventListener('submit', (e) => {
            e.preventDefault();
            this.saveSubscription();
            return false;
        });

        document.getElementById('mvpl-btn-add-query').addEventListener('click', () => {
            this.addQueryRow();
        });

        document.getElementById('mvpl-btn-search-help').addEventListener('click', () => {
            window.open('https://github.com/mediathekview/mediathekviewweb/blob/master/README.md#suchlogik-anwenden', '_blank');
        });

        document.getElementById('mvpl-btn-test-sub').addEventListener('click', () => {
            this.testSubscription();
        });

        document.getElementById('mvpl-btn-cancel-sub').addEventListener('click', () => {
            this.subscriptionEditor.close();
        });

        // Test Results
        document.getElementById('mvpl-btn-close-test-results').addEventListener('click', () => {
            this.closeTestSubscriptionDialog();
        });

        // Advanced Download
        document.getElementById('mvpl-adv-download-form').addEventListener('submit', (e) => {
            e.preventDefault();
            this.performAdvancedDownload();
            return false;
        });

        document.getElementById('mvpl-btn-close-adv-download').addEventListener('click', () => {
            this.closeAdvancedDownloadDialog();
        });

        document.getElementById('mvpl-btn-duckduckgo-tmdb').addEventListener('click', () => {
            const query = this.currentItemForAdvancedDl.Topic + ' ' + this.currentItemForAdvancedDl.Title;
            Helper.openDuckDuckGoSearch(query, 'themoviedb.org', true);
        });

        document.getElementById('mvpl-btn-duckduckgo').addEventListener('click', () => {
            const query = this.currentItemForAdvancedDl.Topic + ' ' + this.currentItemForAdvancedDl.Title;
            Helper.openDuckDuckGoSearch(query, 'themoviedb.org');
        });
    }

    init() {
        this.bindEvents();
        this.searchController.init();
        this.liveTvController.init();
        this.adoptionController.init();
        this.dependencyManager.init();
        this.setupAutoGrowInputs();
    }
}

// 2. Der Haupteinstiegspunkt für das System
export default function (view, params) {
    const mediathekConfig = new MediathekPluginConfig();
    window.MediathekViewDL = {
        config: mediathekConfig,
        editor: mediathekConfig.subscriptionEditor
    };
    // Events binden, wenn die Seite angezeigt wird
    view.addEventListener('viewshow', function () {
        mediathekConfig.init();
    });
}
