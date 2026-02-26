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
        Video: "Video",
        TotalItemsInfo: (total) => "Aktuelle Konfiguration: Bis zu " + total + " Medien können pro Suche/Abo-Lauf gefunden werden."
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
        NoData: "Noch keine Daten ...",
        LocalFile: "Lokale Datei",
        ApiMatch: "API Treffer",
        Confidence: "Sicherheit",
        Confirmed: "Bestätigt",
        Confirm: "Bestätigen",
        Search: "Suche",
        ErrorLoading: "Fehler beim Laden der Adoption-Kandidaten: ",
        ErrorSaving: "Fehler beim Speichern des Mappings: ",
        MappingSaved: "Mapping erfolgreich gespeichert."
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
        Not: {
            Title: "Ausschluss-Filter (NICHT)",
            On: "Alle Ergebnisse, die in einem der ausgewählten Felder diesen Text enthalten, werden ignoriert. Klicken Sie erneut, um zum Suchparameter zu wechseln.",
            Off: "Nur Ergebnisse, die diesen Text enthalten, werden berücksichtigt. Klicken Sie, um diesen Begriff auszuschließen."
        },
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
    Common: {
        View: "MediathekViewDLConfigPage",
        CriticalErrorOverlay: "mvpl-critical-error-overlay",
        CriticalErrorMessage: "mvpl-critical-error-message"
    },
    Tabs: {
        Prefix: "tab-",
        ButtonPrefix: "mvpl-btn-tab-",
        Container: "mvpl-tabs-spacer",
        Search: "search",
        Settings: "settings",
        Subscriptions: "subscriptions",
        Downloads: "downloads",
        Adoption: "adoption",
        Buttons: {
            Search: "mvpl-btn-tab-search",
            Settings: "mvpl-btn-tab-settings",
            Subscriptions: "mvpl-btn-tab-subscriptions",
            Downloads: "mvpl-btn-tab-downloads",
            Adoption: "mvpl-btn-tab-adoption"
        }
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
        Results: "searchResults",
        BtnHelp: "mvpl-btn-search-help"
    },
    Settings: {
        Form: "MediathekGeneralConfigForm",
        LastRun: "lblLastRun",
        Paths: {
            DefaultDownload: "txtDefaultDownloadPath",
            SubscriptionShow: "txtDefaultSubscriptionShowPath",
            SubscriptionMovie: "txtDefaultSubscriptionMoviePath",
            ManualShow: "txtDefaultManualShowPath",
            ManualMovie: "txtDefaultManualMoviePath",
            TempDownload: "txtTempDownloadPath",
            UseTopicForMoviePath: "chkMoviePathWithTopic",
            Buttons: {
                SelectDefaultDownload: "btnSelectPath",
                SelectSubscriptionShow: "btnSelectSubscriptionShowPath",
                SelectSubscriptionMovie: "btnSelectSubscriptionMoviePath",
                SelectManualShow: "btnSelectManualShowPath",
                SelectManualMovie: "btnSelectManualMoviePath",
                SelectTemp: "btnSelectTempPath"
            }
        },
        Download: {
            Subtitles: "chkDownloadSubtitles",
            ScanLibrary: "chkScanLibraryAfterDownload",
            DirectAudioExtraction: "chkEnableDirectAudioExtraction",
            MinFreeDiskSpace: "txtMinFreeDiskSpaceMiB",
            MaxBandwidth: "txtMaxBandwidthMBits"
        },
        Network: {
            AllowUnknownDomains: "chkAllowUnknownDomains",
            AllowHttp: "chkAllowHttp"
        },
        Maintenance: {
            StrmCleanup: "chkEnableStrmCleanup",
            AllowDownloadUnknownDiskSpace: "chkAllowDownloadOnUnknownDiskSpace"
        },
        Search: {
            FetchStreamSizes: "chkFetchStreamSizes",
            SearchFutureBroadcasts: "chkSearchInFutureBroadcasts",
            PageSize: "txtSearchPageSize",
            MaxPages: "txtSearchMaxPages",
            TotalItemsInfo: "lblSearchTotalItemsInfo"
        },
        Defaults: {
            MinDuration: "defSubMinDuration",
            MaxDuration: "defSubMaxDuration",
            UseStreamingUrlFiles: "defSubUseStreamingUrlFiles",
            DownloadFullVideoSecondaryAudio: "defSubDownloadFullVideoForSecondaryAudio",
            AlwaysCreateSubfolder: "defSubAlwaysCreateSubfolder",
            EnhancedDuplicateDetection: "defSubEnhancedDuplicateDetection",
            AutoUpgradeHigherQuality: "defSubAutoUpgradeToHigherQuality",
            AllowFallbackLowerQuality: "defSubAllowFallbackToLowerQuality",
            QualityCheckWithUrl: "defSubQualityCheckWithUrl",
            EnforceSeries: "defSubEnforceSeries",
            AbsoluteEpisodeNumbering: "defSubAllowAbsoluteEpisodeNumbering",
            TreatNonEpisodesAsExtras: "defSubTreatNonEpisodesAsExtras",
            SaveExtrasAsStrm: "defSubSaveExtrasAsStrm",
            SaveTrailers: "defSubSaveTrailers",
            SaveInterviews: "defSubSaveInterviews",
            SaveGenericExtras: "defSubSaveGenericExtras",
            OriginalLanguage: "defSubOriginalLanguage",
            CreateNfo: "defSubCreateNfo",
            AppendDate: "defSubAppendDateToTitle",
            AppendTime: "defSubAppendTimeToTitle",
            AllowAudioDesc: "defSubAllowAudioDesc",
            AllowSignLanguage: "defSubAllowSignLanguage",
            Containers: {
                SaveTrailers: "defSubSaveTrailersContainer",
                SaveInterviews: "defSubSaveInterviewsContainer",
                SaveGenericExtras: "defSubSaveGenericExtrasContainer",
                SaveExtrasAsStrm: "defSubSaveExtrasAsStrmContainer",
                AbsoluteEpisodeNumbering: "defSubAllowAbsoluteEpisodeNumberingContainer",
                DownloadFullVideoSecondaryAudio: "defSubDownloadFullVideoForSecondaryAudioContainer",
                UseStreamingUrlFiles: "defSubUseStreamingUrlFilesContainer",
                QualityCheckWithUrl: "defSubQualityCheckWithUrlContainer",
                AppendTime: "defSubAppendTimeToTitleContainer"
            }
        }
    },
    LiveTv: {
        Tuner: "mvpl-btn-setup-tuner",
        Guide: "mvpl-btn-setup-guide"
    },
    Adoption: {
        AboSelector: "selectAdoptionAbo",
        FilterContainer: "adoptionFilterContainer",
        MinConfidence: "numAdoptionMinConfidence",
        MaxConfidence: "numAdoptionMaxConfidence",
        SourceFilter: "selectAdoptionSource",
        BtnSaveAll: "mvpl-btn-adoption-save-all",
        TableContainer: "adoptionTableContainer",
        List: "adoptionList",
    },
    Subscription: {
        List: "subscriptionList",
        BtnNew: "mvpl-btn-new-sub",
        Editor: {
            Container: "subscriptionEditor",
            Form: "mvpl-form-subscription",
            Title: "subEditorTitle",
            Id: "subId",
            Name: "subName",
            OriginalLanguage: "subOriginalLanguage",
            MinDuration: "subMinDuration",
            MaxDuration: "subMaxDuration",
            MinBroadcastDate: "subMinBroadcastDate",
            MaxBroadcastDate: "subMaxBroadcastDate",
            Path: "subPath",
            EnforceSeries: "subEnforceSeries",
            CreateNfo: "subCreateNfo",
            AllowAudioDesc: "subAllowAudioDesc",
            AbsoluteEpisodeNumbering: "subAllowAbsoluteEpisodeNumbering",
            AppendDate: "subAppendDateToTitle",
            AppendTime: "subAppendTimeToTitle",
            AllowSignLanguage: "subAllowSignLanguage",
            AlwaysCreateSubfolder: "subAlwaysCreateSubfolder",
            EnhancedDuplicateDetection: "subEnhancedDuplicateDetection",
            AutoUpgradeHigherQuality: "subAutoUpgradeToHigherQuality",
            TreatNonEpisodesAsExtras: "subTreatNonEpisodesAsExtras",
            SaveTrailers: "subSaveTrailers",
            SaveInterviews: "subSaveInterviews",
            SaveGenericExtras: "subSaveGenericExtras",
            SaveExtrasAsStrm: "subSaveExtrasAsStrm",
            UseStreamingUrlFiles: "subUseStreamingUrlFiles",
            DownloadFullVideoSecondaryAudio: "subDownloadFullVideoForSecondaryAudio",
            AllowFallbackLowerQuality: "subAllowFallbackToLowerQuality",
            QualityCheckWithUrl: "subQualityCheckWithUrl",
            QueriesContainer: "queriesContainer",
            BtnAddQuery: "mvpl-btn-add-query",
            BtnTest: "mvpl-btn-test-sub",
            BtnCancel: "mvpl-btn-cancel-sub",
            BtnSelectPath: "btnSelectSubPath",
            Containers: {
                SaveTrailers: "subSaveTrailersContainer",
                SaveInterviews: "subSaveInterviewsContainer",
                SaveGenericExtras: "subSaveGenericExtrasContainer",
                SaveExtrasAsStrm: "subSaveExtrasAsStrmContainer",
                AbsoluteEpisodeNumbering: "subAllowAbsoluteEpisodeNumberingContainer",
                DownloadFullVideoSecondaryAudio: "subDownloadFullVideoForSecondaryAudioContainer",
                UseStreamingUrlFiles: "subUseStreamingUrlFilesContainer",
                QualityCheckWithUrl: "subQualityCheckWithUrlContainer",
                AppendTime: "subAppendTimeToTitleContainer"
            }
        },
        TestModal: {
            Container: "testSubscriptionModal",
            Results: "testSubscriptionResults",
            Count: "testSubscriptionCount",
            BtnClose: "mvpl-btn-close-test-results"
        }
    },
    AdvancedDownload: {
        Modal: "advancedDownloadModal",
        Form: "mvpl-adv-download-form",
        Title: "advancedDownloadTitle",
        Path: "advDlPath",
        Filename: "advDlFilename",
        Subtitles: "advDlSubtitles",
        SubtitlesDesc: "advDlSubtitlesDesc",
        BtnSelectPath: "btnSelectPath",
        BtnClose: "mvpl-btn-close-adv-download",
        BtnDuckDuckGoTmdb: "mvpl-btn-duckduckgo-tmdb",
        BtnDuckDuckGo: "mvpl-btn-duckduckgo",
        Index: "advancedDownloadIndex"
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
        }).then(() => {
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
        this.config.switchTab(DomIds.Tabs.Subscriptions);
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
        this.lastAboId = null;
        this.lastAdoptionInfo = null;
    }

    init() {
        document.getElementById(DomIds.Adoption.AboSelector).addEventListener('change', (e) => {
            this.onAboSelected(e.target.value);
        });

        document.getElementById(DomIds.Adoption.MinConfidence).addEventListener('input', () => this.applyFilters());
        document.getElementById(DomIds.Adoption.MaxConfidence).addEventListener('input', () => this.applyFilters());
        document.getElementById(DomIds.Adoption.SourceFilter).addEventListener('change', () => this.applyFilters());
        document.getElementById(DomIds.Adoption.BtnSaveAll).addEventListener('click', () => this.saveAllFiltered());
    }

    onAboSelected(aboId) {
        const tableContainer = document.getElementById(DomIds.Adoption.TableContainer);
        const filterContainer = document.getElementById(DomIds.Adoption.FilterContainer);

        if (aboId) {
            tableContainer.style.display = 'block';
            filterContainer.style.display = 'flex';
            this.lastAboId = aboId;
            this.refreshData(aboId);
        } else {
            tableContainer.style.display = 'none';
            filterContainer.style.display = 'none';
            this.lastAboId = null;
            this.lastAdoptionInfo = null;
        }
    }

    refreshData(aboId) {
        Dashboard.showLoadingMsg();
        const url = ApiClient.getUrl('/' + this.config.pluginName + '/Adoption/Candidates/' + aboId);

        ApiClient.getJSON(url).then((info) => {
            this.lastAdoptionInfo = info;
            this.applyFilters();
            Dashboard.hideLoadingMsg();
        }).catch((err) => {
            Dashboard.hideLoadingMsg();
            Helper.showError(err, Language.Adoption.ErrorLoading);
        });
    }

    applyFilters() {
        if (!this.lastAdoptionInfo || !this.lastAboId) return;

        const min = parseInt(document.getElementById(DomIds.Adoption.MinConfidence).value, 10) || 0;
        const max = parseInt(document.getElementById(DomIds.Adoption.MaxConfidence).value, 10) || 100;
        const sourceFilter = document.getElementById(DomIds.Adoption.SourceFilter).value;

        const filteredCandidates = this.lastAdoptionInfo.Candidates.filter(c => {
            const bestMatch = (c.Matches && c.Matches.length > 0) ? c.Matches[0] : null;
            const confidence = bestMatch ? Math.round(bestMatch.Confidence) : 0;

            if (confidence < min || confidence > max) return false;
            
            if (sourceFilter === "Unconfirmed") {
                if (bestMatch && bestMatch.IsConfirmed) return false;
            } else if (sourceFilter !== "All") {
                if (!bestMatch || bestMatch.Source !== sourceFilter) return false;
            }

            return true;
        });

        this.lastFilteredCandidates = filteredCandidates;
        this.renderCandidates(this.lastAboId, filteredCandidates, this.lastAdoptionInfo.ApiResults);
    }

    saveAllFiltered() {
        if (!this.lastFilteredCandidates || !this.lastAboId) return;

        const unconfirmed = this.lastFilteredCandidates.filter(c => c.Matches && c.Matches.length > 0 && !c.Matches[0].IsConfirmed);
        if (unconfirmed.length === 0) return;

        const mappings = unconfirmed.map(c => ({
            CandidateId: c.Id,
            ApiId: c.Matches[0].ApiId,
            VideoUrl: c.Matches[0].VideoUrl
        }));

        Dashboard.showLoadingMsg();
        const url = ApiClient.getUrl('/' + this.config.pluginName + '/Adoption/Mappings?subscriptionId=' + this.lastAboId);

        ApiClient.ajax({
            type: "POST",
            url: url,
            data: JSON.stringify(mappings),
            contentType: 'application/json'
        }).then(() => {
            Dashboard.hideLoadingMsg();
            Helper.showToast(Language.Adoption.MappingSaved);
            this.refreshData(this.lastAboId);
        }).catch((err) => {
            Dashboard.hideLoadingMsg();
            Helper.showError(err, Language.Adoption.ErrorSaving);
        });
    }

    renderCandidates(aboId, candidates, apiResults) {

        const list = document.getElementById(DomIds.Adoption.List);
        list.textContent = "";

        if ((!candidates || candidates.length === 0) && (!apiResults || apiResults.length === 0)) {
            const p = document.createElement('p');
            p.textContent = Language.Adoption.NoData;
            list.appendChild(p);
            return;
        }

        // 1. Show Local Files (Candidates)
        if (candidates && candidates.length > 0) {
            const header = document.createElement('h2');
            header.textContent = "Lokale Dateien & Übereinstimmungen";
            header.style.margin = "20px 0 10px 0";
            list.appendChild(header);

            candidates.forEach((candidate) => {
                list.appendChild(this.createAdoptionRow(aboId, candidate, apiResults));
            });
        }

        // 2. Show Unmatched API Results
        const matchedApiIds = new Set();
        candidates.forEach(c => {
            if (c.Matches) {
                c.Matches.forEach(m => matchedApiIds.add(m.ApiId));
            }
        });

        const unmatchedResults = apiResults.filter(res => !matchedApiIds.has(res.Item.Id));

        if (unmatchedResults.length > 0) {
            const header = document.createElement('h2');
            header.textContent = "Nicht zugeordnete API-Ergebnisse";
            header.style.margin = "30px 0 10px 0";
            list.appendChild(header);

            unmatchedResults.forEach(res => {
                list.appendChild(this.createUnmatchedApiRow(res));
            });
        }
    }

    createAdoptionRow(aboId, candidate, apiResults) {
        const matches = candidate.Matches || [];
        const bestMatch = matches.length > 0 ? matches[0] : null;

        const actions = document.createElement('div');
        actions.className = 'listItemButtons flex-gap-10';

        // 1. Local Part (Title and Path)
        const localContainer = document.createElement('div');
        localContainer.style.flex = '1';
        localContainer.style.minWidth = '0';

        const localTitle = document.createElement('div');
        localTitle.style.fontWeight = 'bold';
        localTitle.textContent = Helper.getFileNameFromPath(candidate.Id);
        localContainer.appendChild(localTitle);

        const localPath = document.createElement('div');
        localPath.className = 'secondary mvpl-history-entry-path';
        localPath.style.fontSize = '0.85em';
        localPath.textContent = candidate.Id;
        localContainer.appendChild(localPath);

        // 2. Connector Arrow
        const arrow = document.createElement('div');
        arrow.className = 'material-icons';
        arrow.textContent = 'arrow_forward';
        arrow.style.margin = '0 20px';
        arrow.style.opacity = '0.5';

        // 3. API Part (Match or Select)
        const apiContainer = document.createElement('div');
        apiContainer.style.flex = '1';
        apiContainer.style.minWidth = '0';

        const matchInfo = document.createElement('div');
        matchInfo.className = 'flex-align-center flex-gap-10';

        if (bestMatch) {
            const apiTitle = document.createElement('span');
            apiTitle.style.fontWeight = 'bold';
            apiTitle.textContent = bestMatch.ApiTitle || bestMatch.ApiId;
            matchInfo.appendChild(apiTitle);

            const apiId = document.createElement('span');
            apiId.className = 'mvpl-api-id';
            apiId.textContent = '(ID: ' + bestMatch.ApiId + ')';
            matchInfo.appendChild(apiId);

            const score = Math.round(bestMatch.Confidence);
            const confidenceSpan = document.createElement('span');
            confidenceSpan.textContent = score + '%';
            if (score > 80) confidenceSpan.className = 'mvpl-confidence-high';
            else if (score > 50) confidenceSpan.className = 'mvpl-confidence-medium';
            else confidenceSpan.className = 'mvpl-confidence-low';
            matchInfo.appendChild(confidenceSpan);

            if (bestMatch.IsConfirmed) {
                const confirmedBadge = document.createElement('span');
                confirmedBadge.className = 'mvpl-download-status';
                confirmedBadge.setAttribute('data-status', 'Finished');
                confirmedBadge.textContent = Language.Adoption.Confirmed;
                matchInfo.appendChild(confirmedBadge);
            } else {
                actions.appendChild(this.dom.createIconButton(Icons.Add, Language.Adoption.Confirm, () => this.saveMapping(aboId, candidate.Id, bestMatch.ApiId, bestMatch.VideoUrl)));
            }
        } else {
            const noMatch = document.createElement('span');
            noMatch.textContent = "- Kein Treffer -";
            noMatch.style.fontStyle = 'italic';
            matchInfo.appendChild(noMatch);
        }

        apiContainer.appendChild(matchInfo);

        // Dropdown for manual correction (if not confirmed)
        if (!bestMatch || !bestMatch.IsConfirmed) {
            const selectContainer = document.createElement('div');
            selectContainer.style.marginTop = '5px';

            const select = document.createElement('select');
            select.className = 'emby-select';
            select.style.width = '100%';
            select.style.maxWidth = '400px';

            const defaultOpt = document.createElement('option');
            defaultOpt.value = "";
            defaultOpt.text = "Anderes API-Ergebnis wählen...";
            select.appendChild(defaultOpt);

            // Add other matches first
            if (matches.length > 1) {
                const group = document.createElement('optgroup');
                group.label = "Top Treffer";
                matches.forEach(m => {
                    const res = apiResults.find(r => r.Item.Id === m.ApiId);
                    const channelInfo = res ? ' - ' + res.Item.Channel : '';
                    const opt = document.createElement('option');
                    opt.value = m.ApiId;
                    opt.text = (m.ApiTitle || m.ApiId) + channelInfo + ' (' + Math.round(m.Confidence) + '%)';
                    if (bestMatch && m.ApiId === bestMatch.ApiId) opt.selected = true;
                    group.appendChild(opt);
                });
                select.appendChild(group);
            }

            const allGroup = document.createElement('optgroup');
            allGroup.label = "Alle API Ergebnisse";
            apiResults.forEach(res => {
                // Skip if already in Top Treffer
                if (matches.some(m => m.ApiId === res.Item.Id)) return;

                const opt = document.createElement('option');
                opt.value = res.Item.Id;
                opt.text = res.Item.Title + ' - ' + res.Item.Channel + ' (ID: ' + res.Item.Id + ')';
                allGroup.appendChild(opt);
            });
            select.appendChild(allGroup);

            select.addEventListener('change', (e) => {
                const selectedId = e.target.value;
                if (selectedId) {
                    const selectedItem = apiResults.find(r => r.Item.Id === selectedId);
                    // find in matches first to get videoUrl if available
                    const m = matches.find(m => m.ApiId === selectedId);
                    const videoUrl = m ? m.VideoUrl : (selectedItem ? (this.getBestVideoUrl(selectedItem.Item) || "") : "");
                    this.saveMapping(aboId, candidate.Id, selectedId, videoUrl);
                }
            });
            selectContainer.appendChild(select);
            apiContainer.appendChild(selectContainer);
        }

        const listItem = document.createElement('div');
        listItem.className = 'listItem listItem-border';
        if (bestMatch && bestMatch.IsConfirmed) {
            listItem.classList.add('mvpl-adoption-item-confirmed');
        }

        const body = document.createElement('div');
        body.className = 'listItemBody flex-align-center';
        body.style.width = '100%';
        body.style.padding = '10px 15px';
        body.appendChild(localContainer);
        body.appendChild(arrow);
        body.appendChild(apiContainer);

        listItem.appendChild(body);
        if (actions.hasChildNodes()) {
            listItem.appendChild(actions);
        }

        return listItem;
    }

    createUnmatchedApiRow(apiResult) {
        const item = apiResult.Item;
        const listItem = document.createElement('div');
        listItem.className = 'listItem listItem-border';

        const body = document.createElement('div');
        body.className = 'listItemBody two-line';

        const title = document.createElement('h3');
        title.className = 'listItemBodyText';
        title.textContent = item.Title;
        body.appendChild(title);

        const subText = document.createElement('div');
        subText.className = 'listItemBodyText secondary';
        subText.textContent = item.Channel + ' | ' + item.Topic + ' (ID: ' + item.Id + ')';
        body.appendChild(subText);

        listItem.appendChild(body);
        return listItem;
    }

    getBestVideoUrl(item) {
        if (!item.VideoUrls || item.VideoUrls.length === 0) return null;
        const sorted = [...item.VideoUrls].sort((a, b) => (b.Quality || 0) - (a.Quality || 0));
        return sorted[0].Url;
    }

    saveMapping(aboId, candidateId, apiId, videoUrl) {

        Dashboard.showLoadingMsg();

        const url = ApiClient.getUrl('/' + this.config.pluginName + '/Adoption/Match');


        const data = {

            CandidateId: candidateId,

            ApiId: apiId,

            VideoUrl: videoUrl

        };


        ApiClient.ajax({

            type: "POST",

            url: url + '?subscriptionId=' + aboId,

            data: JSON.stringify(data),

            contentType: 'application/json'

        }).then(() => {

            Dashboard.hideLoadingMsg();

            Helper.showToast(Language.Adoption.MappingSaved);

            this.refreshData(aboId);

        }).catch((err) => {

            Dashboard.hideLoadingMsg();

            Helper.showError(err, Language.Adoption.ErrorSaving);

        });

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
                controllerId: DomIds.Subscription.Editor.TreatNonEpisodesAsExtras,
                dependentId: DomIds.Subscription.Editor.Containers.SaveTrailers,
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: DomIds.Subscription.Editor.TreatNonEpisodesAsExtras,
                dependentId: DomIds.Subscription.Editor.Containers.SaveInterviews,
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: DomIds.Subscription.Editor.TreatNonEpisodesAsExtras,
                dependentId: DomIds.Subscription.Editor.Containers.SaveGenericExtras,
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: DomIds.Subscription.Editor.TreatNonEpisodesAsExtras,
                dependentId: DomIds.Subscription.Editor.Containers.SaveExtrasAsStrm,
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: DomIds.Subscription.Editor.EnforceSeries,
                dependentId: DomIds.Subscription.Editor.Containers.AbsoluteEpisodeNumbering,
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: DomIds.Subscription.Editor.UseStreamingUrlFiles,
                dependentId: DomIds.Subscription.Editor.Containers.DownloadFullVideoSecondaryAudio,
                showWhen: false,
                disableWhenHidden: true
            },
            {
                controllerId: DomIds.Subscription.Editor.DownloadFullVideoSecondaryAudio,
                dependentId: DomIds.Subscription.Editor.Containers.UseStreamingUrlFiles,
                showWhen: false,
                disableWhenHidden: true
            },
            {
                controllerId: DomIds.Subscription.Editor.AllowFallbackLowerQuality,
                dependentId: DomIds.Subscription.Editor.Containers.QualityCheckWithUrl,
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: DomIds.Subscription.Editor.AppendDate,
                dependentId: DomIds.Subscription.Editor.Containers.AppendTime,
                showWhen: true,
                disableWhenHidden: true
            },
            // Rules for Subscription Defaults
            {
                controllerId: DomIds.Settings.Defaults.TreatNonEpisodesAsExtras,
                dependentId: DomIds.Settings.Defaults.Containers.SaveTrailers,
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: DomIds.Settings.Defaults.TreatNonEpisodesAsExtras,
                dependentId: DomIds.Settings.Defaults.Containers.SaveInterviews,
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: DomIds.Settings.Defaults.TreatNonEpisodesAsExtras,
                dependentId: DomIds.Settings.Defaults.Containers.SaveGenericExtras,
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: DomIds.Settings.Defaults.TreatNonEpisodesAsExtras,
                dependentId: DomIds.Settings.Defaults.Containers.SaveExtrasAsStrm,
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: DomIds.Settings.Defaults.EnforceSeries,
                dependentId: DomIds.Settings.Defaults.Containers.AbsoluteEpisodeNumbering,
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: DomIds.Settings.Defaults.UseStreamingUrlFiles,
                dependentId: DomIds.Settings.Defaults.Containers.DownloadFullVideoSecondaryAudio,
                showWhen: false,
                disableWhenHidden: true
            },
            {
                controllerId: DomIds.Settings.Defaults.DownloadFullVideoSecondaryAudio,
                dependentId: DomIds.Settings.Defaults.Containers.UseStreamingUrlFiles,
                showWhen: false,
                disableWhenHidden: true
            },
            {
                controllerId: DomIds.Settings.Defaults.AllowFallbackLowerQuality,
                dependentId: DomIds.Settings.Defaults.Containers.QualityCheckWithUrl,
                showWhen: true,
                disableWhenHidden: true
            },
            {
                controllerId: DomIds.Settings.Defaults.AppendDate,
                dependentId: DomIds.Settings.Defaults.Containers.AppendTime,
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
        const el = document.getElementById(DomIds.Subscription.Editor.Path);
        if (!el) return;

        if (StringHelper.isNullOrWhitespace(el.value)) {
            const defaultMoviePath = window.MediathekViewDL.config.currentConfig.Paths.DefaultSubscriptionMoviePath || Language.General.NotConfigured;
            const defaultShowPath = window.MediathekViewDL.config.currentConfig.Paths.DefaultSubscriptionShowPath || Language.General.NotConfigured;
            const useTopicForMoviePath = window.MediathekViewDL.config.currentConfig.Paths.UseTopicForMoviePath;
            const alwaysCreateSubfolder = document.getElementById(DomIds.Subscription.Editor.AlwaysCreateSubfolder).checked;
            const subName = document.getElementById(DomIds.Subscription.Editor.Name).value || Language.General.AboNamePlaceholder;

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
        const el = document.getElementById(DomIds.Subscription.Editor.Path);
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

            document.getElementById(DomIds.Subscription.Editor.Id).value = "";
            document.getElementById(DomIds.Subscription.Editor.Name).value = "";
            document.getElementById(DomIds.Subscription.Editor.OriginalLanguage).value = defMeta.OriginalLanguage || "";
            document.getElementById(DomIds.Subscription.Editor.MinDuration).value = defSearch.MinDurationMinutes || "";
            document.getElementById(DomIds.Subscription.Editor.MaxDuration).value = defSearch.MaxDurationMinutes || "";
            document.getElementById(DomIds.Subscription.Editor.MinBroadcastDate).value = "";
            document.getElementById(DomIds.Subscription.Editor.MaxBroadcastDate).value = "";
            document.getElementById(DomIds.Subscription.Editor.Path).value = "";
            this.updateSubPathHoverText();

            document.getElementById(DomIds.Subscription.Editor.EnforceSeries).checked = defSeries.EnforceSeriesParsing || false;
            document.getElementById(DomIds.Subscription.Editor.CreateNfo).checked = defMeta.CreateNfo || false;
            document.getElementById(DomIds.Subscription.Editor.AllowAudioDesc).checked = defAccess.AllowAudioDescription || false;
            document.getElementById(DomIds.Subscription.Editor.AbsoluteEpisodeNumbering).checked = defSeries.AllowAbsoluteEpisodeNumbering || false;
            document.getElementById(DomIds.Subscription.Editor.AppendDate).checked = defMeta.AppendDateToTitle || false;
            document.getElementById(DomIds.Subscription.Editor.AppendTime).checked = defMeta.AppendTimeToTitle || false;
            document.getElementById(DomIds.Subscription.Editor.AllowSignLanguage).checked = defAccess.AllowSignLanguage || false;
            document.getElementById(DomIds.Subscription.Editor.TreatNonEpisodesAsExtras).checked = defSeries.TreatNonEpisodesAsExtras || false;
            document.getElementById(DomIds.Subscription.Editor.SaveTrailers).checked = defSeries.SaveTrailers !== undefined ? defSeries.SaveTrailers : true;
            document.getElementById(DomIds.Subscription.Editor.SaveInterviews).checked = defSeries.SaveInterviews !== undefined ? defSeries.SaveInterviews : true;
            document.getElementById(DomIds.Subscription.Editor.SaveGenericExtras).checked = defSeries.SaveGenericExtras !== undefined ? defSeries.SaveGenericExtras : true;
            document.getElementById(DomIds.Subscription.Editor.SaveExtrasAsStrm).checked = defSeries.SaveExtrasAsStrm || false;
            document.getElementById(DomIds.Subscription.Editor.UseStreamingUrlFiles).checked = defDl.UseStreamingUrlFiles || false;
            document.getElementById(DomIds.Subscription.Editor.DownloadFullVideoSecondaryAudio).checked = defDl.DownloadFullVideoForSecondaryAudio || false;
            document.getElementById(DomIds.Subscription.Editor.AlwaysCreateSubfolder).checked = defDl.AlwaysCreateSubfolder || false;
            document.getElementById(DomIds.Subscription.Editor.EnhancedDuplicateDetection).checked = defDl.EnhancedDuplicateDetection || false;
            document.getElementById(DomIds.Subscription.Editor.AutoUpgradeHigherQuality).checked = defDl.AutoUpgradeToHigherQuality || false;
            document.getElementById(DomIds.Subscription.Editor.AllowFallbackLowerQuality).checked = defDl.AllowFallbackToLowerQuality !== undefined ? defDl.AllowFallbackToLowerQuality : true;
            document.getElementById(DomIds.Subscription.Editor.QualityCheckWithUrl).checked = defDl.QualityCheckWithUrl || false;

            document.getElementById(DomIds.Subscription.Editor.QueriesContainer).textContent = '';
            this.config.addQueryRow(null);

            // Force update dependencies to hide unchecked dependent fields
            this.config.dependencyManager.applyDependencies();
            return;
        }

        document.getElementById(DomIds.Subscription.Editor.Id).value = sub.Id || "";
        document.getElementById(DomIds.Subscription.Editor.Name).value = sub.Name;

        // Ensure nested objects exist to avoid errors
        const search = sub.Search || {};
        const download = sub.Download || {};
        const series = sub.Series || {};
        const metadata = sub.Metadata || {};
        const accessibility = sub.Accessibility || {};

        document.getElementById(DomIds.Subscription.Editor.OriginalLanguage).value = metadata.OriginalLanguage || "";
        document.getElementById(DomIds.Subscription.Editor.MinDuration).value = search.MinDurationMinutes || "";
        document.getElementById(DomIds.Subscription.Editor.MaxDuration).value = search.MaxDurationMinutes || "";
        document.getElementById(DomIds.Subscription.Editor.MinBroadcastDate).value = search.MinBroadcastDate ? search.MinBroadcastDate.split('T')[0] : "";
        document.getElementById(DomIds.Subscription.Editor.MaxBroadcastDate).value = search.MaxBroadcastDate ? search.MaxBroadcastDate.split('T')[0] : "";
        document.getElementById(DomIds.Subscription.Editor.Path).value = download.DownloadPath || "";
        this.updateSubPathHoverText();

        document.getElementById(DomIds.Subscription.Editor.EnforceSeries).checked = series.EnforceSeriesParsing;
        document.getElementById(DomIds.Subscription.Editor.CreateNfo).checked = metadata.CreateNfo !== undefined ? metadata.CreateNfo : false;
        document.getElementById(DomIds.Subscription.Editor.AllowAudioDesc).checked = accessibility.AllowAudioDescription;
        document.getElementById(DomIds.Subscription.Editor.AbsoluteEpisodeNumbering).checked = series.AllowAbsoluteEpisodeNumbering;
        document.getElementById(DomIds.Subscription.Editor.AppendDate).checked = metadata.AppendDateToTitle !== undefined ? metadata.AppendDateToTitle : false;
        document.getElementById(DomIds.Subscription.Editor.AppendTime).checked = metadata.AppendTimeToTitle !== undefined ? metadata.AppendTimeToTitle : false;
        document.getElementById(DomIds.Subscription.Editor.AllowSignLanguage).checked = accessibility.AllowSignLanguage;
        document.getElementById(DomIds.Subscription.Editor.AlwaysCreateSubfolder).checked = download.AlwaysCreateSubfolder || false;
        document.getElementById(DomIds.Subscription.Editor.EnhancedDuplicateDetection).checked = download.EnhancedDuplicateDetection;
        document.getElementById(DomIds.Subscription.Editor.AutoUpgradeHigherQuality).checked = download.AutoUpgradeToHigherQuality !== undefined ? download.AutoUpgradeToHigherQuality : false;
        document.getElementById(DomIds.Subscription.Editor.TreatNonEpisodesAsExtras).checked = series.TreatNonEpisodesAsExtras;
        document.getElementById(DomIds.Subscription.Editor.SaveTrailers).checked = series.SaveTrailers !== undefined ? series.SaveTrailers : true;
        document.getElementById(DomIds.Subscription.Editor.SaveInterviews).checked = series.SaveInterviews !== undefined ? series.SaveInterviews : true;
        document.getElementById(DomIds.Subscription.Editor.SaveGenericExtras).checked = series.SaveGenericExtras !== undefined ? series.SaveGenericExtras : true;
        document.getElementById(DomIds.Subscription.Editor.SaveExtrasAsStrm).checked = series.SaveExtrasAsStrm;
        document.getElementById(DomIds.Subscription.Editor.UseStreamingUrlFiles).checked = download.UseStreamingUrlFiles;
        document.getElementById(DomIds.Subscription.Editor.DownloadFullVideoSecondaryAudio).checked = download.DownloadFullVideoForSecondaryAudio;
        document.getElementById(DomIds.Subscription.Editor.AllowFallbackLowerQuality).checked = download.AllowFallbackToLowerQuality !== undefined ? download.AllowFallbackToLowerQuality : true;
        document.getElementById(DomIds.Subscription.Editor.QualityCheckWithUrl).checked = download.QualityCheckWithUrl !== undefined ? download.QualityCheckWithUrl : false;


        const queriesContainer = document.getElementById(DomIds.Subscription.Editor.QueriesContainer);
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
        const id = document.getElementById(DomIds.Subscription.Editor.Id).value;
        const name = document.getElementById(DomIds.Subscription.Editor.Name).value;
        const originalLanguage = document.getElementById(DomIds.Subscription.Editor.OriginalLanguage).value;
        const minDuration = document.getElementById(DomIds.Subscription.Editor.MinDuration).value;
        const maxDuration = document.getElementById(DomIds.Subscription.Editor.MaxDuration).value;
        const minBroadcastDate = document.getElementById(DomIds.Subscription.Editor.MinBroadcastDate).value;
        const maxBroadcastDate = document.getElementById(DomIds.Subscription.Editor.MaxBroadcastDate).value;
        const path = document.getElementById(DomIds.Subscription.Editor.Path).value;
        const enforce = document.getElementById(DomIds.Subscription.Editor.EnforceSeries).checked;
        const createNfo = document.getElementById(DomIds.Subscription.Editor.CreateNfo).checked;
        const allowAudio = document.getElementById(DomIds.Subscription.Editor.AllowAudioDesc).checked;
        const allowAbsolute = document.getElementById(DomIds.Subscription.Editor.AbsoluteEpisodeNumbering).checked;
        const appendDateToTitle = document.getElementById(DomIds.Subscription.Editor.AppendDate).checked;
        const appendTimeToTitle = document.getElementById(DomIds.Subscription.Editor.AppendTime).checked;
        const allowSignLanguage = document.getElementById(DomIds.Subscription.Editor.AllowSignLanguage).checked;
        const alwaysCreateSubfolder = document.getElementById(DomIds.Subscription.Editor.AlwaysCreateSubfolder).checked;
        const enhancedDuplicateDetection = document.getElementById(DomIds.Subscription.Editor.EnhancedDuplicateDetection).checked;
        const autoUpgradeToHigherQuality = document.getElementById(DomIds.Subscription.Editor.AutoUpgradeHigherQuality).checked;
        const treatNonEpisodesAsExtras = document.getElementById(DomIds.Subscription.Editor.TreatNonEpisodesAsExtras).checked;
        const saveTrailers = document.getElementById(DomIds.Subscription.Editor.SaveTrailers).checked;
        const saveInterviews = document.getElementById(DomIds.Subscription.Editor.SaveInterviews).checked;
        const saveGenericExtras = document.getElementById(DomIds.Subscription.Editor.SaveGenericExtras).checked;
        const saveExtrasAsStrm = document.getElementById(DomIds.Subscription.Editor.SaveExtrasAsStrm).checked;
        const useStreamingUrlFiles = document.getElementById(DomIds.Subscription.Editor.UseStreamingUrlFiles).checked;
        const downloadFullVideoForSecondaryAudio = document.getElementById(DomIds.Subscription.Editor.DownloadFullVideoSecondaryAudio).checked;
        const allowFallbackToLowerQuality = document.getElementById(DomIds.Subscription.Editor.AllowFallbackLowerQuality).checked;
        const qualityCheckWithUrl = document.getElementById(DomIds.Subscription.Editor.QualityCheckWithUrl).checked;

        const criteria = [];
        document.querySelectorAll('#' + DomIds.Subscription.Editor.QueriesContainer + ' .mvpl-query-row').forEach(function (row) {
            const queryText = row.querySelector('.subQueryText').value;
            if (queryText) {
                const fields = [];
                row.querySelectorAll('.subQueryField:checked').forEach(function (fieldCheckbox) {
                    fields.push(fieldCheckbox.value);
                });
                const isExclude = row.querySelector('.subQueryExcludeBtn').classList.contains('active');
                criteria.push({Query: queryText, Fields: fields, IsExclude: isExclude});
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
        const editor = document.getElementById(DomIds.Subscription.Editor.Container);
        const title = document.getElementById(DomIds.Subscription.Editor.Title);

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
        document.getElementById(DomIds.Subscription.Editor.Container).style.display = 'none';
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

            this.measureSpan.textContent = input.value || input.placeholder || '';

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
            const overlay = document.getElementById(DomIds.Common.CriticalErrorOverlay);
            const errorMsg = document.getElementById(DomIds.Common.CriticalErrorMessage);
            if (errorMessage) {
                overlay.classList.remove('mvpl-hidden');
                errorMsg.textContent = errorMessage;
                // Also disable the main form to be safe
                document.getElementById(DomIds.Settings.Form).style.pointerEvents = 'none';
                document.getElementById(DomIds.Settings.Form).style.opacity = '0.5';
            } else {
                overlay.classList.add('mvpl-hidden');
            }
        }).catch(err => console.error(Language.Subscription.ErrorInitializationStatus, err));

        // noinspection JSUnresolvedReference
        Dashboard.showLoadingMsg();
        // noinspection JSUnresolvedReference
        ApiClient.getPluginConfiguration(this.pluginId).then((config) => {
            this.currentConfig = config;

            document.getElementById(DomIds.Settings.Paths.DefaultDownload).value = config.Paths.DefaultDownloadPath || "";
            document.getElementById(DomIds.Settings.Paths.SubscriptionShow).value = config.Paths.DefaultSubscriptionShowPath || "";
            document.getElementById(DomIds.Settings.Paths.SubscriptionMovie).value = config.Paths.DefaultSubscriptionMoviePath || "";
            document.getElementById(DomIds.Settings.Paths.ManualShow).value = config.Paths.DefaultManualShowPath || "";
            document.getElementById(DomIds.Settings.Paths.ManualMovie).value = config.Paths.DefaultManualMoviePath || "";
            document.getElementById(DomIds.Settings.Paths.TempDownload).value = config.Paths.TempDownloadPath || "";
            document.getElementById(DomIds.Settings.Download.Subtitles).checked = config.Download.DownloadSubtitles;
            document.getElementById(DomIds.Settings.Network.AllowUnknownDomains).checked = config.Network.AllowUnknownDomains;
            document.getElementById(DomIds.Settings.Network.AllowHttp).checked = config.Network.AllowHttp;
            document.getElementById(DomIds.Settings.Download.ScanLibrary).checked = config.Download.ScanLibraryAfterDownload;
            document.getElementById(DomIds.Settings.Download.DirectAudioExtraction).checked = config.Download.EnableDirectAudioExtraction;
            document.getElementById(DomIds.Settings.Maintenance.StrmCleanup).checked = config.Maintenance.EnableStrmCleanup;
            document.getElementById(DomIds.Settings.Search.FetchStreamSizes).checked = config.Search.FetchStreamSizes;
            document.getElementById(DomIds.Settings.Search.SearchFutureBroadcasts).checked = config.Search.SearchInFutureBroadcasts;
            document.getElementById(DomIds.Settings.Maintenance.AllowDownloadUnknownDiskSpace).checked = config.Maintenance.AllowDownloadOnUnknownDiskSpace;
            document.getElementById(DomIds.Settings.Search.PageSize).value = config.Search.PageSize || 50;
            document.getElementById(DomIds.Settings.Search.MaxPages).value = config.Search.MaxPages || 5;
            document.getElementById(DomIds.Settings.Download.MinFreeDiskSpace).value = config.Download.MinFreeDiskSpaceBytes ? (config.Download.MinFreeDiskSpaceBytes / (1024 * 1024)) : "";
            document.getElementById(DomIds.Settings.Download.MaxBandwidth).value = config.Download.MaxBandwidthMBits || 0;
            document.getElementById(DomIds.Settings.LastRun).innerText = config.LastRun ? new Date(config.LastRun).toLocaleString() : Language.Subscription.Never;
            document.getElementById(DomIds.Settings.Paths.UseTopicForMoviePath).checked = config.Paths.UseTopicForMoviePath;

            // Load Subscription Defaults
            const def = config.SubscriptionDefaults || {};
            const defDl = def.DownloadSettings || {};
            const defSearch = def.SearchSettings || {};
            const defSeries = def.SeriesSettings || {};
            const defMeta = def.MetadataSettings || {};
            const defAccess = def.AccessibilitySettings || {};

            document.getElementById(DomIds.Settings.Defaults.MinDuration).value = defSearch.MinDurationMinutes || "";
            document.getElementById(DomIds.Settings.Defaults.MaxDuration).value = defSearch.MaxDurationMinutes || "";
            document.getElementById(DomIds.Settings.Defaults.UseStreamingUrlFiles).checked = defDl.UseStreamingUrlFiles || false;
            document.getElementById(DomIds.Settings.Defaults.DownloadFullVideoSecondaryAudio).checked = defDl.DownloadFullVideoForSecondaryAudio || false;
            document.getElementById(DomIds.Settings.Defaults.AlwaysCreateSubfolder).checked = defDl.AlwaysCreateSubfolder || false;
            document.getElementById(DomIds.Settings.Defaults.EnhancedDuplicateDetection).checked = defDl.EnhancedDuplicateDetection || false;
            document.getElementById(DomIds.Settings.Defaults.AutoUpgradeHigherQuality).checked = defDl.AutoUpgradeToHigherQuality || false;
            document.getElementById(DomIds.Settings.Defaults.AllowFallbackLowerQuality).checked = defDl.AllowFallbackToLowerQuality !== undefined ? defDl.AllowFallbackToLowerQuality : true;
            document.getElementById(DomIds.Settings.Defaults.QualityCheckWithUrl).checked = defDl.QualityCheckWithUrl || false;

            document.getElementById(DomIds.Settings.Defaults.EnforceSeries).checked = defSeries.EnforceSeriesParsing || false;
            document.getElementById(DomIds.Settings.Defaults.AbsoluteEpisodeNumbering).checked = defSeries.AllowAbsoluteEpisodeNumbering || false;
            document.getElementById(DomIds.Settings.Defaults.TreatNonEpisodesAsExtras).checked = defSeries.TreatNonEpisodesAsExtras || false;
            document.getElementById(DomIds.Settings.Defaults.SaveExtrasAsStrm).checked = defSeries.SaveExtrasAsStrm || false;
            document.getElementById(DomIds.Settings.Defaults.SaveTrailers).checked = defSeries.SaveTrailers !== undefined ? defSeries.SaveTrailers : true;
            document.getElementById(DomIds.Settings.Defaults.SaveInterviews).checked = defSeries.SaveInterviews !== undefined ? defSeries.SaveInterviews : true;
            document.getElementById(DomIds.Settings.Defaults.SaveGenericExtras).checked = defSeries.SaveGenericExtras !== undefined ? defSeries.SaveGenericExtras : true;

            document.getElementById(DomIds.Settings.Defaults.OriginalLanguage).value = defMeta.OriginalLanguage || "";
            document.getElementById(DomIds.Settings.Defaults.CreateNfo).checked = defMeta.CreateNfo || false;
            document.getElementById(DomIds.Settings.Defaults.AppendDate).checked = defMeta.AppendDateToTitle || false;
            document.getElementById(DomIds.Settings.Defaults.AppendTime).checked = defMeta.AppendTimeToTitle || false;

            document.getElementById(DomIds.Settings.Defaults.AllowAudioDesc).checked = defAccess.AllowAudioDescription || false;
            document.getElementById(DomIds.Settings.Defaults.AllowSignLanguage).checked = defAccess.AllowSignLanguage || false;

            this.renderSubscriptionsList();
            this.adoptionController.populateAbos(config.Subscriptions);
            this.updateSearchTotalItemsInfo();
            // noinspection JSUnresolvedReference
            Dashboard.hideLoadingMsg();
        });
    }

    /**
     * Updates the info text showing the total potential search results.
     */
    updateSearchTotalItemsInfo() {
        const pageSize = parseInt(document.getElementById(DomIds.Settings.Search.PageSize).value, 10) || 0;
        const maxPages = parseInt(document.getElementById(DomIds.Settings.Search.MaxPages).value, 10) || 0;
        const total = pageSize * maxPages;

        const label = document.getElementById(DomIds.Settings.Search.TotalItemsInfo);
        if (label) {
            label.textContent = Language.Search.TotalItemsInfo(total);
        }
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
        document.getElementById(DomIds.Tabs.Prefix + tabId).style.display = 'block';

        const buttons = document.querySelectorAll('.' + DomIds.Tabs.Container + ' button');
        buttons.forEach(btn => {
            btn.classList.remove('selected');
            btn.setAttribute('aria-selected', 'false');
        });

        const selectedBtn = document.getElementById(DomIds.Tabs.ButtonPrefix + tabId);
        if (selectedBtn) {
            selectedBtn.classList.add('selected');
            selectedBtn.setAttribute('aria-selected', 'true');
        }

        if (tabId === DomIds.Tabs.Downloads) {
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
        const list = document.getElementById(DomIds.Subscription.List);
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
                }).then(() => {
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
            query = {Query: '', Fields: ['Title', 'Topic'], IsExclude: false};
        }
        const queryText = query ? query.Query : '';
        const fields = query ? query.Fields : ['Title', 'Topic'];
        const isExclude = query ? query.IsExclude : false;

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

        const btnExclude = this.dom.create('button', {
            type: 'button',
            className: 'subQueryExcludeBtn' + (isExclude ? ' active' : ''),
            text: 'NOT',
            attributes: {
                'is': 'emby-button',
                'title': isExclude ? Language.Subscription.Not.On : Language.Subscription.Not.Off
            },
            onClick: (e) => {
                const btn = e.currentTarget;
                const active = btn.classList.toggle('active');
                btn.title = active ? Language.Subscription.Not.On : Language.Subscription.Not.Off;
            }
        });

        const removeBtn = this.dom.createIconButton(Icons.Remove, Language.Subscription.RemoveQuery, (e) => {
            e.target.closest('.mvpl-query-row').remove();
        });
        removeBtn.classList.add('btnRemoveQuery');

        const newRow = this.dom.create('div', {
            className: 'mvpl-query-row',
            children: [
                btnExclude,
                this.dom.create('div', {className: 'flex-grow', children: [input]}),
                this.dom.create('div', {
                    className: 'query-checkboxes',
                    children: [cbTitle, cbTopic, cbDescription, cbChannel]
                }),
                removeBtn
            ]
        });

        document.getElementById(DomIds.Subscription.Editor.QueriesContainer).appendChild(newRow);
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
        document.getElementById(DomIds.AdvancedDownload.Index).value = ""; // Not needed anymore contextually but keeping element

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
                        document.getElementById(DomIds.AdvancedDownload.Filename).value = recommended.FileName;
                    }
                    if (recommended.Path) {
                        document.getElementById(DomIds.AdvancedDownload.Path).value = recommended.Path;
                    }
                }
            });
        });


        let advDlSub = document.getElementById(DomIds.AdvancedDownload.Subtitles);
        let advDlSubDesc = document.getElementById(DomIds.AdvancedDownload.SubtitlesDesc);
        const subtitleUrls = this.currentItemForAdvancedDl.SubtitleUrls || [];
        if (subtitleUrls.length === 0) {
            advDlSub.checked = false;
            advDlSub.disabled = true;
            advDlSubDesc.textContent = Language.Search.NoSubtitles;
        } else {
            advDlSub.disabled = false;
            advDlSubDesc.textContent = "";
        }


        document.getElementById(DomIds.AdvancedDownload.Modal).style.display = 'flex';
    }

    closeAdvancedDownloadDialog() {
        document.getElementById(DomIds.AdvancedDownload.Modal).style.display = 'none';
    }

    performAdvancedDownload() {
        if (!this.currentItemForAdvancedDl) return;

        const downloadOptions = {
            item: this.currentItemForAdvancedDl,
            downloadPath: document.getElementById(DomIds.AdvancedDownload.Path).value,
            fileName: document.getElementById(DomIds.AdvancedDownload.Filename).value,
            downloadSubtitles: document.getElementById(DomIds.AdvancedDownload.Subtitles).checked
        };

        const url = ApiClient.getUrl('/' + this.pluginName + '/AdvancedDownload');

        Dashboard.showLoadingMsg();
        ApiClient.ajax({
            type: "POST",
            url: url,
            data: JSON.stringify(downloadOptions),
            contentType: 'application/json'
        }).then(() => {
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
            document.getElementById(DomIds.Subscription.TestModal.Container).style.display = 'flex';
        }).catch((err) => {
            Dashboard.hideLoadingMsg();
            console.error("Test subscription error:", err);
            Helper.showError(err, Language.Search.ErrorTestingAbo);
        });
    }

    closeTestSubscriptionDialog() {
        document.getElementById(DomIds.Subscription.TestModal.Container).style.display = 'none';
    }

    renderTestResults(results) {
        const container = document.getElementById(DomIds.Subscription.TestModal.Results);
        const countContainer = document.getElementById(DomIds.Subscription.TestModal.Count);
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
        document.getElementById(DomIds.Common.View).addEventListener('pageshow', () => {
            this.loadConfig();
        });

        // Tab Navigation
        document.getElementById(DomIds.Tabs.Buttons.Search).addEventListener('click', () => this.switchTab(DomIds.Tabs.Search));
        document.getElementById(DomIds.Tabs.Buttons.Settings).addEventListener('click', () => this.switchTab(DomIds.Tabs.Settings));
        document.getElementById(DomIds.Tabs.Buttons.Subscriptions).addEventListener('click', () => this.switchTab(DomIds.Tabs.Subscriptions));
        document.getElementById(DomIds.Tabs.Buttons.Downloads).addEventListener('click', () => this.switchTab(DomIds.Tabs.Downloads));
        document.getElementById(DomIds.Tabs.Buttons.Adoption).addEventListener('click', () => this.switchTab(DomIds.Tabs.Adoption));

        // Main Config Form
        document.getElementById(DomIds.Settings.Form).addEventListener('submit', (e) => {
            e.preventDefault();
            this.currentConfig.Paths.DefaultDownloadPath = document.getElementById(DomIds.Settings.Paths.DefaultDownload).value;
            this.currentConfig.Paths.DefaultSubscriptionShowPath = document.getElementById(DomIds.Settings.Paths.SubscriptionShow).value;
            this.currentConfig.Paths.DefaultSubscriptionMoviePath = document.getElementById(DomIds.Settings.Paths.SubscriptionMovie).value;
            this.currentConfig.Paths.DefaultManualShowPath = document.getElementById(DomIds.Settings.Paths.ManualShow).value;
            this.currentConfig.Paths.DefaultManualMoviePath = document.getElementById(DomIds.Settings.Paths.ManualMovie).value;
            this.currentConfig.Paths.TempDownloadPath = document.getElementById(DomIds.Settings.Paths.TempDownload).value;

            this.currentConfig.Download.DownloadSubtitles = document.getElementById(DomIds.Settings.Download.Subtitles).checked;
            this.currentConfig.Network.AllowUnknownDomains = document.getElementById(DomIds.Settings.Network.AllowUnknownDomains).checked;
            this.currentConfig.Network.AllowHttp = document.getElementById(DomIds.Settings.Network.AllowHttp).checked;
            this.currentConfig.Download.ScanLibraryAfterDownload = document.getElementById(DomIds.Settings.Download.ScanLibrary).checked;
            this.currentConfig.Download.EnableDirectAudioExtraction = document.getElementById(DomIds.Settings.Download.DirectAudioExtraction).checked;
            this.currentConfig.Maintenance.EnableStrmCleanup = document.getElementById(DomIds.Settings.Maintenance.StrmCleanup).checked;
            this.currentConfig.Search.FetchStreamSizes = document.getElementById(DomIds.Settings.Search.FetchStreamSizes).checked;
            this.currentConfig.Search.SearchInFutureBroadcasts = document.getElementById(DomIds.Settings.Search.SearchFutureBroadcasts).checked;
            this.currentConfig.Search.PageSize = parseInt(document.getElementById(DomIds.Settings.Search.PageSize).value, 10) || 50;
            this.currentConfig.Search.MaxPages = parseInt(document.getElementById(DomIds.Settings.Search.MaxPages).value, 10) || 5;
            this.currentConfig.Maintenance.AllowDownloadOnUnknownDiskSpace = document.getElementById(DomIds.Settings.Maintenance.AllowDownloadUnknownDiskSpace).checked;

            const minFreeSpaceMiB = parseInt(document.getElementById(DomIds.Settings.Download.MinFreeDiskSpace).value, 10);
            this.currentConfig.Download.MinFreeDiskSpaceBytes = isNaN(minFreeSpaceMiB) ? (1.5 * 1024 * 1024 * 1024) : (minFreeSpaceMiB * 1024 * 1024);
            this.currentConfig.Paths.UseTopicForMoviePath = document.getElementById(DomIds.Settings.Paths.UseTopicForMoviePath).checked;

            const maxBandwidth = parseInt(document.getElementById(DomIds.Settings.Download.MaxBandwidth).value, 10);
            this.currentConfig.Download.MaxBandwidthMBits = isNaN(maxBandwidth) ? 0 : maxBandwidth;

            // Save Subscription Defaults
            this.currentConfig.SubscriptionDefaults = {
                DownloadSettings: {
                    UseStreamingUrlFiles: document.getElementById(DomIds.Settings.Defaults.UseStreamingUrlFiles).checked,
                    DownloadFullVideoForSecondaryAudio: document.getElementById(DomIds.Settings.Defaults.DownloadFullVideoSecondaryAudio).checked,
                    AlwaysCreateSubfolder: document.getElementById(DomIds.Settings.Defaults.AlwaysCreateSubfolder).checked,
                    EnhancedDuplicateDetection: document.getElementById(DomIds.Settings.Defaults.EnhancedDuplicateDetection).checked,
                    AutoUpgradeToHigherQuality: document.getElementById(DomIds.Settings.Defaults.AutoUpgradeHigherQuality).checked,
                    AllowFallbackToLowerQuality: document.getElementById(DomIds.Settings.Defaults.AllowFallbackLowerQuality).checked,
                    QualityCheckWithUrl: document.getElementById(DomIds.Settings.Defaults.QualityCheckWithUrl).checked
                },
                SearchSettings: {
                    MinDurationMinutes: document.getElementById(DomIds.Settings.Defaults.MinDuration).value ? parseInt(document.getElementById(DomIds.Settings.Defaults.MinDuration).value, 10) : null,
                    MaxDurationMinutes: document.getElementById(DomIds.Settings.Defaults.MaxDuration).value ? parseInt(document.getElementById(DomIds.Settings.Defaults.MaxDuration).value, 10) : null
                },
                SeriesSettings: {
                    EnforceSeriesParsing: document.getElementById(DomIds.Settings.Defaults.EnforceSeries).checked,
                    AllowAbsoluteEpisodeNumbering: document.getElementById(DomIds.Settings.Defaults.AbsoluteEpisodeNumbering).checked,
                    TreatNonEpisodesAsExtras: document.getElementById(DomIds.Settings.Defaults.TreatNonEpisodesAsExtras).checked,
                    SaveExtrasAsStrm: document.getElementById(DomIds.Settings.Defaults.SaveExtrasAsStrm).checked,
                    SaveTrailers: document.getElementById(DomIds.Settings.Defaults.SaveTrailers).checked,
                    SaveInterviews: document.getElementById(DomIds.Settings.Defaults.SaveInterviews).checked,
                    SaveGenericExtras: document.getElementById(DomIds.Settings.Defaults.SaveGenericExtras).checked
                },
                MetadataSettings: {
                    OriginalLanguage: document.getElementById(DomIds.Settings.Defaults.OriginalLanguage).value,
                    CreateNfo: document.getElementById(DomIds.Settings.Defaults.CreateNfo).checked,
                    AppendDateToTitle: document.getElementById(DomIds.Settings.Defaults.AppendDate).checked,
                    AppendTimeToTitle: document.getElementById(DomIds.Settings.Defaults.AppendTime).checked
                },
                AccessibilitySettings: {
                    AllowAudioDescription: document.getElementById(DomIds.Settings.Defaults.AllowAudioDesc).checked,
                    AllowSignLanguage: document.getElementById(DomIds.Settings.Defaults.AllowSignLanguage).checked
                }
            };

            this.subscriptionEditor.updateSubPathHoverText();
            this.saveGlobalConfig();
            return false;
        });

        // Path selector in main config
        document.getElementById(DomIds.Settings.Paths.Buttons.SelectDefaultDownload).addEventListener('click', () => {
            Helper.openFolderDialog(DomIds.Settings.Paths.DefaultDownload, Language.General.SelectGlobalDefaultDownloadPath);
        });
        document.getElementById(DomIds.Settings.Paths.Buttons.SelectSubscriptionShow).addEventListener('click', () => {
            Helper.openFolderDialog(DomIds.Settings.Paths.SubscriptionShow, Language.General.SelectDefaultShowPathAbo);
        });
        document.getElementById(DomIds.Settings.Paths.Buttons.SelectSubscriptionMovie).addEventListener('click', () => {
            Helper.openFolderDialog(DomIds.Settings.Paths.SubscriptionMovie, Language.General.SelectDefaultMoviePathAbo);
        });
        document.getElementById(DomIds.Settings.Paths.Buttons.SelectManualShow).addEventListener('click', () => {
            Helper.openFolderDialog(DomIds.Settings.Paths.ManualShow, Language.General.SelectDefaultShowPathManual);
        });
        document.getElementById(DomIds.Settings.Paths.Buttons.SelectManualMovie).addEventListener('click', () => {
            Helper.openFolderDialog(DomIds.Settings.Paths.ManualMovie, Language.General.SelectDefaultMoviePathManual);
        });
        document.getElementById(DomIds.Settings.Paths.Buttons.SelectTemp).addEventListener('click', () => {
            Helper.openFolderDialog(DomIds.Settings.Paths.TempDownload, Language.General.SelectTempDownloadPath);
        });

        document.getElementById(DomIds.Settings.Search.PageSize).addEventListener('input', () => this.updateSearchTotalItemsInfo());
        document.getElementById(DomIds.Settings.Search.MaxPages).addEventListener('input', () => this.updateSearchTotalItemsInfo());

        document.getElementById(DomIds.Settings.Paths.UseTopicForMoviePath).addEventListener('change', (e) => {
            this.currentConfig.Paths.UseTopicForMoviePath = e.target.checked;
            this.subscriptionEditor.updateSubPathHoverText();
        });

        // Path selectors in subscription editor
        document.getElementById(DomIds.Subscription.Editor.BtnSelectPath).addEventListener('click', () => {
            Helper.openFolderDialog(DomIds.Subscription.Editor.Path, Language.Subscription.SelectAboPath);
            this.subscriptionEditor.updateSubPathHoverText();
        });
        document.getElementById(DomIds.Subscription.Editor.Path).addEventListener('input', () => {
            this.subscriptionEditor.updateSubPathHoverText();
        });
        document.getElementById(DomIds.Subscription.Editor.Name).addEventListener('input', () => {
            this.subscriptionEditor.updateSubPathHoverText();
        });
        document.getElementById(DomIds.Subscription.Editor.AlwaysCreateSubfolder).addEventListener('change', () => {
            this.subscriptionEditor.updateSubPathHoverText();
        });
        // Path selector in advanced download dialog
        document.getElementById(DomIds.AdvancedDownload.BtnSelectPath).addEventListener('click', () => {
            Helper.openFolderDialog(DomIds.AdvancedDownload.Path, Language.AdvancedDownload.SelectDownloadPath);
        });

        // Subscription Management
        document.getElementById(DomIds.Subscription.BtnNew).addEventListener('click', () => {
            this.subscriptionEditor.show(null);
        });

        document.getElementById(DomIds.Subscription.Editor.Form).addEventListener('submit', (e) => {
            e.preventDefault();
            this.saveSubscription();
            return false;
        });

        document.getElementById(DomIds.Subscription.Editor.BtnAddQuery).addEventListener('click', () => {
            this.addQueryRow();
        });

        document.getElementById(DomIds.Search.BtnHelp).addEventListener('click', () => {
            window.open('https://github.com/mediathekview/mediathekviewweb/blob/master/README.md#suchlogik-anwenden', '_blank');
        });

        document.getElementById(DomIds.Subscription.Editor.BtnTest).addEventListener('click', () => {
            this.testSubscription();
        });

        document.getElementById(DomIds.Subscription.Editor.BtnCancel).addEventListener('click', () => {
            this.subscriptionEditor.close();
        });

        // Test Results
        document.getElementById(DomIds.Subscription.TestModal.BtnClose).addEventListener('click', () => {
            this.closeTestSubscriptionDialog();
        });

        // Advanced Download
        document.getElementById(DomIds.AdvancedDownload.Form).addEventListener('submit', (e) => {
            e.preventDefault();
            this.performAdvancedDownload();
            return false;
        });

        document.getElementById(DomIds.AdvancedDownload.BtnClose).addEventListener('click', () => {
            this.closeAdvancedDownloadDialog();
        });

        document.getElementById(DomIds.AdvancedDownload.BtnDuckDuckGoTmdb).addEventListener('click', () => {
            const query = this.currentItemForAdvancedDl.Topic + ' ' + this.currentItemForAdvancedDl.Title;
            Helper.openDuckDuckGoSearch(query, 'themoviedb.org', true);
        });

        document.getElementById(DomIds.AdvancedDownload.BtnDuckDuckGo).addEventListener('click', () => {
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
