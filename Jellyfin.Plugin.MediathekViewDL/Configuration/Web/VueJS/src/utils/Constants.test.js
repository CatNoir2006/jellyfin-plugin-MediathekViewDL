import { describe, it, expect } from 'vitest'
import { MS_PER_DAY_MINUS_ONE } from './Constants.js'

describe('Constants', () => {
    it('MS_PER_DAY_MINUS_ONE_ShouldEqual86399999_WhenExported', () => {
        // Arrange / Act / Assert
        expect(MS_PER_DAY_MINUS_ONE).toBe(86399999)
    })

    it('MS_PER_DAY_MINUS_ONE_ShouldEqualOneDayMinusOneMs_WhenConvertedToSeconds', () => {
        // Arrange
        const expectedSecondsInDay = 24 * 60 * 60
        // Act
        const seconds = (MS_PER_DAY_MINUS_ONE + 1) / 1000
        // Assert
        expect(seconds).toBe(expectedSecondsInDay)
    })
})
