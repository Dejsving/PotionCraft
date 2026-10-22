import { describe, expect, it } from 'vitest'
import { getPrimaryRole, getRoleLabel, hasRole, isUserRole } from './roles'

describe('roles', () => {
  it('returns Russian labels for known roles and keeps unknown ones as is', () => {
    expect(getRoleLabel('Admin')).toBe('Администратор')
    expect(getRoleLabel('Master')).toBe('Мастер')
    expect(getRoleLabel('Player')).toBe('Игрок')
    expect(getRoleLabel('Guest')).toBe('Guest')
  })

  it('recognizes only known roles', () => {
    expect(isUserRole('Admin')).toBe(true)
    expect(isUserRole('admin')).toBe(false)
  })

  it('checks whether the user has a role', () => {
    const user = { roles: ['Player', 'Admin'] }
    expect(hasRole(user, 'Admin')).toBe(true)
    expect(hasRole(user, 'Master')).toBe(false)
    expect(hasRole(null, 'Admin')).toBe(false)
    expect(hasRole(undefined, 'Player')).toBe(false)
  })

  it('picks the highest known role and null when there is none', () => {
    expect(getPrimaryRole(['Player', 'Master'])).toBe('Master')
    expect(getPrimaryRole(['Player', 'Admin'])).toBe('Admin')
    expect(getPrimaryRole(['Unknown'])).toBeNull()
    expect(getPrimaryRole([])).toBeNull()
  })
})
