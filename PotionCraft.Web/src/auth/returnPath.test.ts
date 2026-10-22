import { describe, expect, it } from 'vitest'
import { getReturnPath } from './returnPath'

describe('getReturnPath', () => {
  it('returns the saved location with search and hash', () => {
    expect(getReturnPath({ from: { pathname: '/herbs', search: '?a=1', hash: '#top' } })).toBe(
      '/herbs?a=1#top'
    )
  })

  it('falls back to the home page without saved location', () => {
    expect(getReturnPath(null)).toBe('/')
    expect(getReturnPath(undefined)).toBe('/')
    expect(getReturnPath({})).toBe('/')
  })

  it('never returns to the auth pages', () => {
    expect(getReturnPath({ from: { pathname: '/login' } })).toBe('/')
    expect(getReturnPath({ from: { pathname: '/register' } })).toBe('/')
  })
})
