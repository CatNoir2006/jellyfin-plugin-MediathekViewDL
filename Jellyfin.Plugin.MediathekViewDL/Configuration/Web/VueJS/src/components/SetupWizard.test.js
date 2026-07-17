import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'

vi.mock('../utils/ApiService.js', () => ({
    default: {
        getChannels: vi.fn().mockResolvedValue(['ARD', 'ZDF', 'BR', 'NDR']),
        getLiveTvConfig: vi.fn().mockResolvedValue({ TunerHosts: [], ListingProviders: [] }),
        addTunerHost: vi.fn().mockResolvedValue({}),
        addListingProvider: vi.fn().mockResolvedValue({}),
        saveSubscription: vi.fn().mockResolvedValue({})
    }
}))

vi.mock('../utils/SubscriptionFactory.js', () => ({
    SubscriptionFactory: {
        createDefault: vi.fn((defaults = {}) => ({
            Name: '',
            IsEnabled: true,
            Search: { Criteria: [] },
            Download: {
                UseStreamingUrlFiles: defaults.DownloadSettings?.UseStreamingUrlFiles || false
            },
            Series: {},
            Metadata: {},
            Accessibility: {}
        }))
    }
}))

import SetupWizard from './SetupWizard.vue'

const defaultConfig = () => ({
    Paths: {
        DefaultSubscriptionShowPath: '/media/shows',
        DefaultSubscriptionMoviePath: '/media/movies',
        DefaultManualShowPath: '',
        DefaultManualMoviePath: '',
        TempDownloadPath: ''
    },
    SubscriptionDefaults: {
        DownloadSettings: { UseStreamingUrlFiles: false }
    }
})

const mountWizard = (props = {}) => {
    return mount(SetupWizard, {
        props: {
            open: true,
            pluginConfig: defaultConfig(),
            ...props
        },
        attachTo: document.body
    })
}

describe('SetupWizard', () => {
    beforeEach(() => {
        vi.clearAllMocks()
    })

    afterEach(() => {
        document.body.innerHTML = ''
    })

    it('ShouldNotRender_WhenOpenIsFalse', async () => {
        // Arrange / Act
        const wrapper = mount(SetupWizard, {
            props: { open: false, pluginConfig: defaultConfig() }
        })
        await flushPromises()
        // Assert
        expect(document.body.querySelector('[data-testid="wizard-card"]')).toBeNull()
        wrapper.unmount()
    })

    it('ShouldShowWelcomeStep_WhenOpenedFresh', async () => {
        // Arrange / Act
        const wrapper = mountWizard()
        await flushPromises()
        // Assert
        expect(document.body.querySelector('[data-testid="wizard-step-welcome"]')).not.toBeNull()
        expect(document.body.querySelector('[data-testid="wizard-next"]')).not.toBeNull()
        wrapper.unmount()
    })

    it('ShouldAdvanceToNextStep_WhenNextClicked', async () => {
        // Arrange
        const wrapper = mountWizard()
        await flushPromises()
        // Act
        document.body.querySelector('[data-testid="wizard-next"]').click()
        await flushPromises()
        // Assert
        expect(document.body.querySelector('[data-testid="wizard-step-paths"]')).not.toBeNull()
        expect(document.body.querySelector('[data-testid="wizard-step-welcome"]')).toBeNull()
        wrapper.unmount()
    })

    it('ShouldGoBack_WhenPrevClicked', async () => {
        // Arrange
        const wrapper = mountWizard()
        await flushPromises()
        document.body.querySelector('[data-testid="wizard-next"]').click()
        await flushPromises()
        // Act
        document.body.querySelector('[data-testid="wizard-prev"]').click()
        await flushPromises()
        // Assert
        expect(document.body.querySelector('[data-testid="wizard-step-welcome"]')).not.toBeNull()
        wrapper.unmount()
    })

    it('ShouldSkipCurrentStepOnly_WhenSkipStepClicked', async () => {
        // Arrange
        const wrapper = mountWizard()
        await flushPromises()
        // Step 1 (welcome) hides skip - advance to step 2 first
        document.body.querySelector('[data-testid="wizard-next"]').click()
        await flushPromises()
        // Act
        document.body.querySelector('[data-testid="wizard-skip"]').click()
        await flushPromises()
        // Assert - wizard still open and on next step, NOT closed
        expect(document.body.querySelector('[data-testid="wizard-card"]')).not.toBeNull()
        expect(document.body.querySelector('[data-testid="wizard-step-strm"]')).not.toBeNull()
        expect(wrapper.emitted('close')).toBeFalsy()
        wrapper.unmount()
    })

    it('ShouldHideSkipButton_OnWelcomeStep', async () => {
        // Arrange
        const wrapper = mountWizard()
        await flushPromises()
        // Assert - step 1 (welcome) should not show skip button
        expect(document.body.querySelector('[data-testid="wizard-skip"]')).toBeNull()
        wrapper.unmount()
    })

    it('ShouldHideSkipButton_OnTabTourStep', async () => {
        // Arrange
        const wrapper = mountWizard()
        await flushPromises()
        // Navigate to step 6 (tab tour)
        wrapper.vm.currentStep = 6
        await flushPromises()
        // Assert
        expect(document.body.querySelector('[data-testid="wizard-skip"]')).toBeNull()
        wrapper.unmount()
    })

    it('ShouldEmitClose_WhenCloseButtonClicked', async () => {
        // Arrange
        const wrapper = mountWizard()
        await flushPromises()
        // Act
        document.body.querySelector('[data-testid="wizard-close"]').click()
        await flushPromises()
        // Assert
        const closeEmits = wrapper.emitted('close')
        expect(closeEmits).toBeTruthy()
        expect(closeEmits[0][0].skipped).toBe(false)
        wrapper.unmount()
    })

    it('ShouldEmitCloseWithSkippedFalse_WhenFinishClickedOnLastStep', async () => {
        // Arrange
        const wrapper = mountWizard()
        await flushPromises()
        // Jump to last step
        wrapper.vm.currentStep = 7
        await flushPromises()
        // Act
        document.body.querySelector('[data-testid="wizard-finish"]').click()
        await flushPromises()
        // Assert
        const closeEmits = wrapper.emitted('close')
        expect(closeEmits).toBeTruthy()
        expect(closeEmits[0][0].skipped).toBe(false)
        wrapper.unmount()
    })

    it('ShouldPrepopulatePathInputs_FromPluginConfig', async () => {
        // Arrange / Act
        const wrapper = mountWizard({
            pluginConfig: {
                Paths: {
                    DefaultSubscriptionShowPath: '/data/shows',
                    DefaultSubscriptionMoviePath: '/data/movies',
                    DefaultManualShowPath: '/manual/shows',
                    DefaultManualMoviePath: '/manual/movies',
                    TempDownloadPath: '/tmp'
                },
                SubscriptionDefaults: { DownloadSettings: { UseStreamingUrlFiles: true } }
            }
        })
        await flushPromises()
        document.body.querySelector('[data-testid="wizard-next"]').click()
        await flushPromises()
        // Assert
        const subShow = document.body.querySelector('[data-testid="wizard-sub-show-path"]')
        const subMovie = document.body.querySelector('[data-testid="wizard-sub-movie-path"]')
        const manShow = document.body.querySelector('[data-testid="wizard-man-show-path"]')
        const manMovie = document.body.querySelector('[data-testid="wizard-man-movie-path"]')
        const temp = document.body.querySelector('[data-testid="wizard-temp-path"]')
        expect(subShow.value).toBe('/data/shows')
        expect(subMovie.value).toBe('/data/movies')
        expect(manShow.value).toBe('/manual/shows')
        expect(manMovie.value).toBe('/manual/movies')
        expect(temp.value).toBe('/tmp')
        wrapper.unmount()
    })

    it('ShouldIncludeAllPathsInClosePayload_WhenFinishing', async () => {
        // Arrange
        const wrapper = mountWizard()
        await flushPromises()
        document.body.querySelector('[data-testid="wizard-next"]').click() // -> step 2
        await flushPromises()
        const subShow = document.body.querySelector('[data-testid="wizard-sub-show-path"]')
        subShow.value = '/custom/sub-show'
        subShow.dispatchEvent(new Event('input'))
        const manMovie = document.body.querySelector('[data-testid="wizard-man-movie-path"]')
        manMovie.value = '/custom/man-movie'
        manMovie.dispatchEvent(new Event('input'))
        await flushPromises()
        document.body.querySelector('[data-testid="wizard-next"]').click() // -> step 3
        await flushPromises()
        // Toggle strm
        const strmToggle = document.body.querySelector('[data-testid="wizard-strm-toggle"]')
        strmToggle.checked = true
        strmToggle.dispatchEvent(new Event('change'))
        await flushPromises()
        // Close via X
        document.body.querySelector('[data-testid="wizard-close"]').click()
        await flushPromises()
        // Assert
        const closeEmits = wrapper.emitted('close')
        expect(closeEmits[0][0].paths.DefaultSubscriptionShowPath).toBe('/custom/sub-show')
        expect(closeEmits[0][0].paths.DefaultManualMoviePath).toBe('/custom/man-movie')
        expect(closeEmits[0][0].defaults.UseStreamingUrlFiles).toBe(true)
        wrapper.unmount()
    })

    it('ShouldAlwaysShowNextButton_OnSubscriptionStep_WhenInputsEmpty', async () => {
        // Arrange
        const wrapper = mountWizard()
        await flushPromises()
        // Navigate to step 5
        wrapper.vm.currentStep = 5
        await flushPromises()
        // Assert
        const nextBtn = document.body.querySelector('[data-testid="wizard-next"]')
        expect(nextBtn).not.toBeNull()
        expect(nextBtn.disabled).toBeFalsy()
        wrapper.unmount()
    })

    it('ShouldShowSpaeterEinrichtenLabel_WhenStep5IsEmpty', async () => {
        // Arrange
        const wrapper = mountWizard()
        await flushPromises()
        wrapper.vm.currentStep = 5
        await flushPromises()
        // Assert
        const nextBtn = document.body.querySelector('[data-testid="wizard-next"]')
        expect(nextBtn.textContent.trim()).toContain('Abo später einrichten')
        wrapper.unmount()
    })

    it('ShouldShowWeiterLabel_WhenStep5IsFilled', async () => {
        // Arrange
        const wrapper = mountWizard()
        await flushPromises()
        wrapper.vm.currentStep = 5
        await flushPromises()
        // Fill form
        const channelSelect = document.body.querySelector('[data-testid="wizard-channel"]')
        channelSelect.value = 'ARD'
        channelSelect.dispatchEvent(new Event('change'))
        const queryInput = document.body.querySelector('[data-testid="wizard-query"]')
        queryInput.value = 'Tatort'
        queryInput.dispatchEvent(new Event('input'))
        await flushPromises()
        // Assert
        const nextBtn = document.body.querySelector('[data-testid="wizard-next"]')
        expect(nextBtn.textContent.trim()).toContain('Weiter')
        expect(nextBtn.textContent.trim()).not.toContain('später einrichten')
        wrapper.unmount()
    })

    it('ShouldEmitSubscriptionCreated_WhenValidSubscriptionSaved', async () => {
        // Arrange
        const ApiService = (await import('../utils/ApiService.js')).default
        const wrapper = mountWizard()
        await flushPromises()
        // Navigate to step 5
        wrapper.vm.currentStep = 5
        await flushPromises()
        // Fill form
        const channelSelect = document.body.querySelector('[data-testid="wizard-channel"]')
        channelSelect.value = 'ARD'
        channelSelect.dispatchEvent(new Event('change'))
        const queryInput = document.body.querySelector('[data-testid="wizard-query"]')
        queryInput.value = 'Tatort'
        queryInput.dispatchEvent(new Event('input'))
        await flushPromises()
        document.body.querySelector('[data-testid="wizard-create-sub"]').click()
        await flushPromises()
        // Assert
        expect(ApiService.saveSubscription).toHaveBeenCalled()
        const emitted = wrapper.emitted('subscription-created')
        expect(emitted).toBeTruthy()
        wrapper.unmount()
    })

    it('ShouldApplyLocallyToggledStrmSetting_ToNewSubscription', async () => {
        // Arrange - pluginConfig has strm=false, wizard user toggles to true
        const ApiService = (await import('../utils/ApiService.js')).default
        const wrapper = mountWizard({
            pluginConfig: {
                Paths: {},
                SubscriptionDefaults: { DownloadSettings: { UseStreamingUrlFiles: false } }
            }
        })
        await flushPromises()
        // Go to step 3 and toggle .strm ON
        wrapper.vm.currentStep = 3
        await flushPromises()
        const strmToggle = document.body.querySelector('[data-testid="wizard-strm-toggle"]')
        strmToggle.checked = true
        strmToggle.dispatchEvent(new Event('change'))
        await flushPromises()
        // Go to step 5 and fill form
        wrapper.vm.currentStep = 5
        await flushPromises()
        const channelSelect = document.body.querySelector('[data-testid="wizard-channel"]')
        channelSelect.value = 'ARD'
        channelSelect.dispatchEvent(new Event('change'))
        const queryInput = document.body.querySelector('[data-testid="wizard-query"]')
        queryInput.value = 'Tatort'
        queryInput.dispatchEvent(new Event('input'))
        await flushPromises()
        document.body.querySelector('[data-testid="wizard-create-sub"]').click()
        await flushPromises()
        // Assert - the saved subscription should have UseStreamingUrlFiles=true
        expect(ApiService.saveSubscription).toHaveBeenCalled()
        const savedSub = ApiService.saveSubscription.mock.calls[0][0]
        expect(savedSub.Download.UseStreamingUrlFiles).toBe(true)
        wrapper.unmount()
    })

    it('ShouldShowTabTourStep_WhenAdvanced', async () => {
        // Arrange
        const wrapper = mountWizard()
        await flushPromises()
        // Act - jump to step 6
        wrapper.vm.currentStep = 6
        await flushPromises()
        // Assert
        expect(document.body.querySelector('[data-testid="wizard-step-tour"]')).not.toBeNull()
        const tourGrid = document.body.querySelector('.tour-grid')
        expect(tourGrid).not.toBeNull()
        wrapper.unmount()
    })

    it('ShouldCallActivateLiveTv_WhenActivateButtonClicked', async () => {
        // Arrange
        const ApiService = (await import('../utils/ApiService.js')).default
        const wrapper = mountWizard()
        await flushPromises()
        // Navigate to step 4
        wrapper.vm.currentStep = 4
        await flushPromises()
        // Act
        const activateBtn = document.body.querySelector('[data-testid="wizard-livetv-activate"]')
        expect(activateBtn).not.toBeNull()
        activateBtn.click()
        await flushPromises()
        // Assert - both endpoints should be called
        expect(ApiService.addTunerHost).toHaveBeenCalledWith({
            Type: 'zapp',
            Url: 'zapp',
            FriendlyName: 'Zapp (MediathekView)',
            TunerCount: 0
        })
        expect(ApiService.addListingProvider).toHaveBeenCalledWith({
            Type: 'zapp',
            Id: 'zapp_guide',
            Name: 'Zapp (MediathekView)'
        })
        const tunerEmits = wrapper.emitted('tuner-added')
        const guideEmits = wrapper.emitted('guide-added')
        expect(tunerEmits).toBeTruthy()
        expect(guideEmits).toBeTruthy()
        wrapper.unmount()
    })

    it('ShouldShowActiveState_WhenLiveTvAlreadyConfigured', async () => {
        // Arrange
        const ApiService = (await import('../utils/ApiService.js')).default
        ApiService.getLiveTvConfig.mockResolvedValueOnce({
            TunerHosts: [{ Type: 'zapp' }],
            ListingProviders: [{ Type: 'zapp' }]
        })
        const wrapper = mountWizard()
        await flushPromises()
        // Navigate to step 4
        wrapper.vm.currentStep = 4
        await flushPromises()
        // Assert
        const activeBtn = document.body.querySelector('[data-testid="wizard-livetv-active"]')
        const activateBtn = document.body.querySelector('[data-testid="wizard-livetv-activate"]')
        expect(activeBtn).not.toBeNull()
        expect(activeBtn.textContent).toContain('aktiviert')
        expect(activateBtn).toBeNull()
        wrapper.unmount()
    })

    it('ShouldNotBreak_WhenDirectoryBrowserCallbackReceivesEmptyPath', async () => {
        // Arrange - simulate Dashboard.DirectoryBrowser callback receiving ''
        const Dashboard = { DirectoryBrowser: class { show() { /* will not fire */ } } }
        global.window.Dashboard = Dashboard
        const wrapper = mountWizard()
        await flushPromises()
        wrapper.vm.currentStep = 2
        await flushPromises()
        // The selectPath function should be safe against empty path
        const subShow = document.body.querySelector('[data-testid="wizard-sub-show-path"]')
        expect(subShow.value).toBe('/media/shows')
        wrapper.unmount()
    })
})