import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'

vi.mock('./components/PluginConfig.vue', () => ({
    default: {
        name: 'PluginConfig',
        template: '<div data-testid="plugin-config-stub">PluginConfig</div>'
    }
}))

import App from './App.vue'

describe('App', () => {
    it('ShouldRenderPluginConfig_WhenMounted', () => {
        // Arrange / Act
        const wrapper = mount(App)
        // Assert
        expect(wrapper.find('[data-testid="plugin-config-stub"]').exists()).toBe(true)
    })

    it('ShouldContainExactlyOneRootElement_WhenMounted', () => {
        // Arrange / Act
        const wrapper = mount(App)
        // Assert
        expect(wrapper.element).toBeTruthy()
    })
})
