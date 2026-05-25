export const SubscriptionFactory = {
  createDefault: (defaults = {}) => ({
    Name: '',
    IsEnabled: true,
    Search: {
      Criteria: [{ Fields: ['Title', 'Topic'], Query: '', IsExclude: false }],
      MinDurationMinutes: defaults.SearchSettings?.MinDurationMinutes || null,
      MaxDurationMinutes: defaults.SearchSettings?.MaxDurationMinutes || null,
      MinBroadcastDate: null,
      MaxBroadcastDate: null
    },
    Download: {
      DownloadPath: '',
      UseStreamingUrlFiles: defaults.DownloadSettings?.UseStreamingUrlFiles || false,
      AlwaysCreateSubfolder: defaults.DownloadSettings?.AlwaysCreateSubfolder || false,
      AllowFallbackToLowerQuality: defaults.DownloadSettings?.AllowFallbackToLowerQuality ?? true,
      EnhancedDuplicateDetection: defaults.DownloadSettings?.EnhancedDuplicateDetection || false,
      QualityCheckWithUrl: defaults.DownloadSettings?.QualityCheckWithUrl || false,
      DownloadFullVideoForSecondaryAudio: defaults.DownloadSettings?.DownloadFullVideoForSecondaryAudio || false
    },
    Series: {
      EnforceSeriesParsing: defaults.SeriesSettings?.EnforceSeriesParsing || false,
      AllowAbsoluteEpisodeNumbering: defaults.SeriesSettings?.AllowAbsoluteEpisodeNumbering || false,
      TreatNonEpisodesAsExtras: defaults.SeriesSettings?.TreatNonEpisodesAsExtras || false,
      SaveTrailers: defaults.SeriesSettings?.SaveTrailers ?? true,
      SaveInterviews: defaults.SeriesSettings?.SaveInterviews ?? true,
      SaveGenericExtras: defaults.SeriesSettings?.SaveGenericExtras ?? true,
      SaveExtrasAsStrm: defaults.SeriesSettings?.SaveExtrasAsStrm || false
    },
    Metadata: {
      OriginalLanguage: defaults.MetadataSettings?.OriginalLanguage || '',
      CreateNfo: defaults.MetadataSettings?.CreateNfo || false,
      AppendDateToTitle: defaults.MetadataSettings?.AppendDateToTitle || false,
      KeepOriginalTitle: defaults.MetadataSettings?.KeepOriginalTitle || false,
      AppendTimeToTitle: defaults.MetadataSettings?.AppendTimeToTitle || false
    },
    Accessibility: {
      AllowAudioDescription: defaults.AccessibilitySettings?.AllowAudioDescription || false,
      AllowSignLanguage: defaults.AccessibilitySettings?.AllowSignLanguage || false
    }
  })
};
