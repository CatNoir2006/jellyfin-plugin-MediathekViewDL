import { describe, it, expect } from 'vitest'
import { SubscriptionFactory } from './SubscriptionFactory.js'

describe('SubscriptionFactory', () => {
    describe('createDefault', () => {
        it('ShouldReturnObjectWithEmptyNameAndEnabled_WhenNoDefaultsProvided', () => {
            // Arrange / Act
            const subscription = SubscriptionFactory.createDefault()
            // Assert
            expect(subscription.Name).toBe('')
            expect(subscription.IsEnabled).toBe(true)
        })

        it('ShouldInitializeSearchCriteriaWithTitleAndTopic_WhenNoDefaultsProvided', () => {
            // Arrange / Act
            const subscription = SubscriptionFactory.createDefault()
            // Assert
            expect(subscription.Search.Criteria).toHaveLength(1)
            expect(subscription.Search.Criteria[0].Fields).toEqual(['Title', 'Topic'])
            expect(subscription.Search.Criteria[0].Query).toBe('')
            expect(subscription.Search.Criteria[0].IsExclude).toBe(false)
        })

        it('ShouldUseSearchSettingsDurations_WhenProvided', () => {
            // Arrange
            const defaults = { SearchSettings: { MinDurationMinutes: 5, MaxDurationMinutes: 60 } }
            // Act
            const subscription = SubscriptionFactory.createDefault(defaults)
            // Assert
            expect(subscription.Search.MinDurationMinutes).toBe(5)
            expect(subscription.Search.MaxDurationMinutes).toBe(60)
        })

        it('ShouldUseDownloadSettingsFlags_WhenProvided', () => {
            // Arrange
            const defaults = {
                DownloadSettings: {
                    UseStreamingUrlFiles: true,
                    AlwaysCreateSubfolder: true,
                    MinHeight: 720,
                    MaxHeight: 2160,
                    SelectionOrder: 'LowestToHighest',
                    StrictResolution: true,
                    SkipVideoQualityProbing: false,
                    EnhancedDuplicateDetection: true,
                    DownloadFullVideoForSecondaryAudio: true
                }
            }
            // Act
            const subscription = SubscriptionFactory.createDefault(defaults)
            // Assert
            expect(subscription.Download.UseStreamingUrlFiles).toBe(true)
            expect(subscription.Download.AlwaysCreateSubfolder).toBe(true)
            expect(subscription.Download.MinHeight).toBe(720)
            expect(subscription.Download.MaxHeight).toBe(2160)
            expect(subscription.Download.SelectionOrder).toBe('LowestToHighest')
            expect(subscription.Download.StrictResolution).toBe(true)
            expect(subscription.Download.SkipVideoQualityProbing).toBe(false)
            expect(subscription.Download.EnhancedDuplicateDetection).toBe(true)
            expect(subscription.Download.DownloadFullVideoForSecondaryAudio).toBe(true)
        })

        it('ShouldDefaultHeightRangeToNullAndOrderToHighestToLowest_WhenNotProvided', () => {
            // Arrange / Act
            const subscription = SubscriptionFactory.createDefault({})
            // Assert
            expect(subscription.Download.MinHeight).toBeNull()
            expect(subscription.Download.MaxHeight).toBeNull()
            expect(subscription.Download.SelectionOrder).toBe('HighestToLowest')
            expect(subscription.Download.StrictResolution).toBe(false)
            expect(subscription.Download.SkipVideoQualityProbing).toBe(true)
        })

        it('ShouldUseSeriesSettingsFlags_WhenProvided', () => {
            // Arrange
            const defaults = {
                SeriesSettings: {
                    EnforceSeriesParsing: true,
                    AllowAbsoluteEpisodeNumbering: true,
                    TreatNonEpisodesAsExtras: true,
                    SaveTrailers: false,
                    SaveInterviews: false,
                    SaveGenericExtras: false,
                    SaveExtrasAsStrm: true
                }
            }
            // Act
            const subscription = SubscriptionFactory.createDefault(defaults)
            // Assert
            expect(subscription.Series.EnforceSeriesParsing).toBe(true)
            expect(subscription.Series.AllowAbsoluteEpisodeNumbering).toBe(true)
            expect(subscription.Series.TreatNonEpisodesAsExtras).toBe(true)
            expect(subscription.Series.SaveTrailers).toBe(false)
            expect(subscription.Series.SaveInterviews).toBe(false)
            expect(subscription.Series.SaveGenericExtras).toBe(false)
            expect(subscription.Series.SaveExtrasAsStrm).toBe(true)
        })

        it('ShouldDefaultSaveTrailersAndInterviewsToTrue_WhenNotProvided', () => {
            // Arrange / Act
            const subscription = SubscriptionFactory.createDefault({})
            // Assert
            expect(subscription.Series.SaveTrailers).toBe(true)
            expect(subscription.Series.SaveInterviews).toBe(true)
            expect(subscription.Series.SaveGenericExtras).toBe(true)
        })

        it('ShouldUseMetadataSettingsFlags_WhenProvided', () => {
            // Arrange
            const defaults = {
                MetadataSettings: {
                    OriginalLanguage: 'deu',
                    CreateNfo: true,
                    AppendDateToTitle: true,
                    KeepOriginalTitle: true,
                    AppendTimeToTitle: true
                }
            }
            // Act
            const subscription = SubscriptionFactory.createDefault(defaults)
            // Assert
            expect(subscription.Metadata.OriginalLanguage).toBe('deu')
            expect(subscription.Metadata.CreateNfo).toBe(true)
            expect(subscription.Metadata.AppendDateToTitle).toBe(true)
            expect(subscription.Metadata.KeepOriginalTitle).toBe(true)
            expect(subscription.Metadata.AppendTimeToTitle).toBe(true)
        })

        it('ShouldUseAccessibilitySettingsFlags_WhenProvided', () => {
            // Arrange
            const defaults = {
                AccessibilitySettings: {
                    AllowAudioDescription: true,
                    AllowSignLanguage: true
                }
            }
            // Act
            const subscription = SubscriptionFactory.createDefault(defaults)
            // Assert
            expect(subscription.Accessibility.AllowAudioDescription).toBe(true)
            expect(subscription.Accessibility.AllowSignLanguage).toBe(true)
        })

        it('ShouldReturnIndependentObjects_WhenCalledMultipleTimes', () => {
            // Arrange / Act
            const a = SubscriptionFactory.createDefault()
            const b = SubscriptionFactory.createDefault()
            a.Name = 'changed'
            // Assert
            expect(b.Name).toBe('')
            expect(a.Search).not.toBe(b.Search)
        })

        it('ShouldSetDownloadPathToEmptyString_WhenCalled', () => {
            // Arrange / Act
            const subscription = SubscriptionFactory.createDefault()
            // Assert
            expect(subscription.Download.DownloadPath).toBe('')
        })
    })
})
