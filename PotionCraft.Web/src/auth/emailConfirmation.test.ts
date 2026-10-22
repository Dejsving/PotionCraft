import { describe, expect, it } from 'vitest'
import { ApiError } from '../api/client'
import { isEmailConfirmationRequired } from './emailConfirmation'

describe('isEmailConfirmationRequired', () => {
  it('recognizes a 403 response that requires email confirmation', () => {
    const error = new ApiError('Подтвердите email.', 403, [], {
      message: 'Подтвердите email.',
      requiresEmailConfirmation: true,
      email: 'alchemist@example.com',
    })
    expect(isEmailConfirmationRequired(error)).toBe(true)
  })

  it('rejects other statuses even with the same body flag', () => {
    const error = new ApiError('Ошибка', 401, [], { requiresEmailConfirmation: true })
    expect(isEmailConfirmationRequired(error)).toBe(false)
  })

  it('rejects a 403 without the confirmation flag', () => {
    expect(isEmailConfirmationRequired(new ApiError('Запрещено', 403, [], { message: 'Запрещено' }))).toBe(false)
    expect(isEmailConfirmationRequired(new ApiError('Запрещено', 403, [], null))).toBe(false)
    expect(isEmailConfirmationRequired(new ApiError('Запрещено', 403))).toBe(false)
  })

  it('rejects non-ApiError values', () => {
    expect(isEmailConfirmationRequired(new Error('Ошибка сети'))).toBe(false)
    expect(isEmailConfirmationRequired({ status: 403, body: { requiresEmailConfirmation: true } })).toBe(false)
    expect(isEmailConfirmationRequired(undefined)).toBe(false)
  })
})
