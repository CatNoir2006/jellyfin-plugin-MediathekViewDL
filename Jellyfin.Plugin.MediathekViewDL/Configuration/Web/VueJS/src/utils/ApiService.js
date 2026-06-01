/**
 * API Service for communicating with the MediathekViewDL API
 * Provides helper functions for API communication
 */
class ApiService {
    constructor() {
        this.apiClient = window.ApiClient ?? null
    }

    /**
     * Build query string from object
     * @private
     */
    buildQueryParams(params = {}) {
        if (!params || Object.keys(params).length === 0) {
            return ''
        }

        const searchParams = new URLSearchParams()
        Object.entries(params).forEach(([key, value]) => {
            if (value !== null && value !== undefined && value !== '') {
                searchParams.append(key, value)
            }
        })

        const queryString = searchParams.toString()
        return queryString ? '?' + queryString : ''
    }

    /**
     * Search for media items
     * @param {Object} filters - Search filters
     * @returns {Promise<Array>} - List of items
     */
    async search(filters = {}) {
        if (!this.apiClient) throw new Error('ApiClient not available')

        const queryString = this.buildQueryParams(filters)
        const url = this.apiClient.getUrl('MediathekViewDL/Search' + queryString)
        console.log(`📥 GET MediathekViewDL/Search`, filters)

        try {
            const response = await this.apiClient.getJSON(url)
            console.log(`✅ Search success`)
            return response
        } catch (error) {
            console.error(`❌ Search failed:`, error)
            throw error
        }
    }

    /**
     * Get search criteria
     * @param {Object} params - Search parameters
     * @returns {Promise<Array>} - Criteria objects
     */
    async getSearchCriteria(params = {}) {
        if (!this.apiClient) throw new Error('ApiClient not available')

        const queryString = this.buildQueryParams(params)
        const url = this.apiClient.getUrl('MediathekViewDL/Search/Criteria' + queryString)
        console.log(`📥 GET MediathekViewDL/Search/Criteria`, params)

        try {
            const response = await this.apiClient.getJSON(url)
            console.log(`✅ GetSearchCriteria success`)
            return response
        } catch (error) {
            console.error(`❌ GetSearchCriteria failed:`, error)
            throw error
        }
    }

    /**
     * Get recommended download path for item
     * @param {Object} videoInfo - Video information (Topic, Title)
     * @returns {Promise<Object>} - Recommended path object
     */
    async getRecommendedPath(videoInfo) {
        if (!this.apiClient) throw new Error('ApiClient not available')

        const url = this.apiClient.getUrl('MediathekViewDL/Items/RecommendedPath')
        console.log(`📤 POST Items/RecommendedPath`, videoInfo)

        try {
            const response = await this.apiClient.ajax({
                type: 'POST',
                url: url,
                data: JSON.stringify(videoInfo),
                contentType: 'application/json',
                processData: false
            })
            console.log(`✅ GetRecommendedPath success:`, response)
            return response
        } catch (error) {
            console.error(`❌ GetRecommendedPath failed:`, error)
            throw error
        }
    }

    /**
     * Start simple download for item
     * @param {Object} item - Item to download
     * @returns {Promise<string>} - Response message
     */
    async downloadItem(item) {
        if (!this.apiClient) throw new Error('ApiClient not available')

        const url = this.apiClient.getUrl('MediathekViewDL/Download')
        console.log(`📤 POST Download`, item.Title)

        try {
            const response = await this.apiClient.ajax({
                type: 'POST',
                url: url,
                data: JSON.stringify(item),
                contentType: 'application/json',
                processData: false
            })
            console.log(`✅ DownloadItem success`)
            return response
        } catch (error) {
            console.error(`❌ DownloadItem failed:`, error)
            throw error
        }
    }

    /**
     * Start advanced download with custom options
     * @param {Object} options - Download options
     * @returns {Promise<string>} - Response message
     */
    async advancedDownload(options) {
        if (!this.apiClient) throw new Error('ApiClient not available')

        const url = this.apiClient.getUrl('MediathekViewDL/AdvancedDownload')
        console.log(`📤 POST AdvancedDownload`, options.FileName)

        try {
            const response = await this.apiClient.ajax({
                type: 'POST',
                url: url,
                data: JSON.stringify(options),
                contentType: 'application/json',
                processData: false
            })
            console.log(`✅ AdvancedDownload success`)
            return response
        } catch (error) {
            console.error(`❌ AdvancedDownload failed:`, error)
            throw error
        }
    }

    /**
     * Get active downloads
     * @returns {Promise<Array>} - List of active downloads
     */
    async getActiveDownloads() {
        if (!this.apiClient) throw new Error('ApiClient not available')

        const url = this.apiClient.getUrl('MediathekViewDL/Downloads/Active')
        console.log(`📥 GET Downloads/Active`)

        try {
            const response = await this.apiClient.getJSON(url)
            console.log(`✅ GetActiveDownloads success`)
            return response
        } catch (error) {
            console.error(`❌ GetActiveDownloads failed:`, error)
            throw error
        }
    }

    /**
     * Get grouped download history
     * @returns {Promise<Array>} - Grouped download history
     */
    async getDownloadHistory() {
        if (!this.apiClient) throw new Error('ApiClient not available')

        const url = this.apiClient.getUrl('MediathekViewDL/Downloads/History/Grouped')
        console.log(`📥 GET Downloads/History/Grouped`)

        try {
            const response = await this.apiClient.getJSON(url)
            console.log(`✅ GetDownloadHistory success`)
            return response
        } catch (error) {
            console.error(`❌ GetDownloadHistory failed:`, error)
            throw error
        }
    }

    /**
     * Cancel a specific download
     * @param {string} id - Download ID
     * @returns {Promise<any>} - Response
     */
    async cancelDownload(id) {
        if (!this.apiClient) throw new Error('ApiClient not available')

        const url = this.apiClient.getUrl('MediathekViewDL/Downloads/' + id)
        console.log(`🗑️ DELETE Downloads/${id}`)

        try {
            const response = await this.apiClient.ajax({
                type: 'DELETE',
                url: url
            })
            console.log(`✅ CancelDownload success`)
            return response
        } catch (error) {
            console.error(`❌ CancelDownload failed:`, error)
            throw error
        }
    }

    /**
     * Cancel all downloads
     * @returns {Promise<any>} - Response
     */
    async cancelAllDownloads() {
        if (!this.apiClient) throw new Error('ApiClient not available')

        const url = this.apiClient.getUrl('MediathekViewDL/Downloads')
        console.log(`🗑️ DELETE Downloads (all)`)

        try {
            const response = await this.apiClient.ajax({
                type: 'DELETE',
                url: url
            })
            console.log(`✅ CancelAllDownloads success`)
            return response
        } catch (error) {
            console.error(`❌ CancelAllDownloads failed:`, error)
            throw error
        }
    }

    /**
     * Clear inactive downloads
     * @returns {Promise<any>} - Response
     */
    async clearInactiveDownloads() {
        if (!this.apiClient) throw new Error('ApiClient not available')

        const url = this.apiClient.getUrl('MediathekViewDL/Downloads/ClearInactive')
        console.log(`📤 POST Downloads/ClearInactive`)

        try {
            const response = await this.apiClient.ajax({
                type: 'POST',
                url: url
            })
            console.log(`✅ ClearInactiveDownloads success`)
            return response
        } catch (error) {
            console.error(`❌ ClearInactiveDownloads failed:`, error)
            throw error
        }
    }

    /**
     * Get all subscriptions
     * @returns {Promise<Array>} - List of subscriptions
     */
    async getSubscriptions() {
        if (!this.apiClient) throw new Error('ApiClient not available')

        const url = this.apiClient.getUrl('MediathekViewDL/Subscriptions')
        console.log(`📥 GET Subscriptions`)

        try {
            const response = await this.apiClient.getJSON(url)
            console.log(`✅ GetSubscriptions success`)
            return response
        } catch (error) {
            console.error(`❌ GetSubscriptions failed:`, error)
            throw error
        }
    }

    /**
     * Delete a subscription
     * @param {string} id - Subscription ID
     * @returns {Promise<any>} - Response
     */
    async deleteSubscription(id) {
        if (!this.apiClient) throw new Error('ApiClient not available')

        const url = this.apiClient.getUrl('MediathekViewDL/Subscriptions/' + id)
        console.log(`🗑️ DELETE Subscriptions/${id}`)

        try {
            const response = await this.apiClient.ajax({
                type: 'DELETE',
                url: url
            })
            console.log(`✅ DeleteSubscription success`)
            return response
        } catch (error) {
            console.error(`❌ DeleteSubscription failed:`, error)
            throw error
        }
    }

    /**
     * Reset subscription history
     * @param {string} id - Subscription ID
     * @returns {Promise<any>} - Response
     */
    async resetSubscriptionHistory(id) {
        if (!this.apiClient) throw new Error('ApiClient not available')

        const url = this.apiClient.getUrl('MediathekViewDL/Subscriptions/' + id + '/ResetHistory')
        console.log(`📤 POST Subscriptions/${id}/ResetHistory`)

        try {
            const response = await this.apiClient.ajax({
                type: 'POST',
                url: url
            })
            console.log(`✅ ResetSubscriptionHistory success`, response)
            return response
        } catch (error) {
            console.error(`❌ ResetSubscriptionHistory failed:`, error)
            throw error
        }
    }

    /**
     * Process subscription
     * @param {string} id - Subscription ID
     * @returns {Promise<any>} - Response
     */
    async processSubscription(id) {
        if (!this.apiClient) throw new Error('ApiClient not available')

        const url = this.apiClient.getUrl('MediathekViewDL/Subscriptions/' + id + '/Process')
        console.log(`📤 POST Subscriptions/${id}/Process`)

        try {
            const response = await this.apiClient.ajax({
                type: 'POST',
                url: url
            })
            console.log(`✅ ProcessSubscription success`, response)
            return response
        } catch (error) {
            console.error(`❌ ProcessSubscription failed:`, error)
            throw error
        }
    }

    /**
     * Set subscription active state
     * @param {string} id - Subscription ID
     * @param {boolean} active - Active state
     * @returns {Promise<boolean>} - New active state
     */
    async setSubscriptionActive(id, active) {
        if (!this.apiClient) throw new Error('ApiClient not available')

        const url = this.apiClient.getUrl('MediathekViewDL/Subscriptions/' + id + '/Active?active=' + active)
        console.log(`📤 POST Subscriptions/${id}/Active?active=${active}`)

        try {
            const response = await this.apiClient.ajax({
                type: 'POST',
                url: url
            })
            console.log(`✅ SetSubscriptionActive success, response:`, response)

            // Return the boolean state - response should be a boolean or string 'true'/'false'
            if (typeof response === 'boolean') {
                return response
            }
            if (typeof response === 'string') {
                return response.toLowerCase() === 'true'
            }
            if (typeof response === 'number') {
                return response === 1
            }
            return active
        } catch (error) {
            console.error(`❌ SetSubscriptionActive failed:`, error)
            throw error
        }
    }
}

// Export singleton instance
export default new ApiService()




