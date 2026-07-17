import { describe, it, expect } from 'vitest'
import apiService from './ApiService.js'

describe('ApiService.buildQueryParams', () => {
    it('ShouldReturnEmptyString_WhenParamsIsEmpty', () => {
        // Arrange / Act
        const result = apiService.buildQueryParams({})
        // Assert
        expect(result).toBe('')
    })

    it('ShouldReturnEmptyString_WhenParamsIsNull', () => {
        // Arrange / Act
        const result = apiService.buildQueryParams(null)
        // Assert
        expect(result).toBe('')
    })

    it('ShouldReturnEmptyString_WhenParamsIsUndefined', () => {
        // Arrange / Act
        const result = apiService.buildQueryParams(undefined)
        // Assert
        expect(result).toBe('')
    })

    it('ShouldStartWithQuestionMark_WhenParamsProvided', () => {
        // Arrange / Act
        const result = apiService.buildQueryParams({ searchTerm: 'tagesschau' })
        // Assert
        expect(result.startsWith('?')).toBe(true)
    })

    it('ShouldIncludeKeyValue_WhenSingleParamProvided', () => {
        // Arrange / Act
        const result = apiService.buildQueryParams({ searchTerm: 'tagesschau' })
        // Assert
        expect(result).toContain('searchTerm=tagesschau')
    })

    it('ShouldJoinMultipleParamsWithAmpersand_WhenMultipleParamsProvided', () => {
        // Arrange / Act
        const result = apiService.buildQueryParams({ a: '1', b: '2' })
        // Assert
        expect(result).toContain('&')
        expect(result).toContain('a=1')
        expect(result).toContain('b=2')
    })

    it('ShouldOmitNullValues_WhenParamsContainNull', () => {
        // Arrange / Act
        const result = apiService.buildQueryParams({ a: '1', b: null })
        // Assert
        expect(result).toContain('a=1')
        expect(result).not.toContain('b=')
    })

    it('ShouldOmitUndefinedValues_WhenParamsContainUndefined', () => {
        // Arrange / Act
        const result = apiService.buildQueryParams({ a: '1', b: undefined })
        // Assert
        expect(result).toContain('a=1')
        expect(result).not.toContain('b=')
    })

    it('ShouldOmitEmptyStringValues_WhenParamsContainEmptyString', () => {
        // Arrange / Act
        const result = apiService.buildQueryParams({ a: '1', b: '' })
        // Assert
        expect(result).toContain('a=1')
        expect(result).not.toContain('b=')
    })

    it('ShouldEncodeSpecialCharacters_WhenValueContainsUrlUnsafeChars', () => {
        // Arrange / Act
        const result = apiService.buildQueryParams({ q: 'a&b c' })
        // Assert
        expect(result).toContain('q=a%26b+c')
    })

    it('ShouldKeepZeroValues_WhenValueIsZero', () => {
        // Arrange / Act
        const result = apiService.buildQueryParams({ offset: 0 })
        // Assert
        expect(result).toContain('offset=0')
    })

    it('ShouldKeepFalseValues_WhenValueIsFalse', () => {
        // Arrange / Act
        const result = apiService.buildQueryParams({ includeTrailer: false })
        // Assert
        expect(result).toContain('includeTrailer=false')
    })
})
