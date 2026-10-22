import { ApiError } from '../api/client'
import type { EmailConfirmationRequiredBody } from '../types/models'

/**
 * Проверяет, что вход отклонён из-за неподтверждённого email (403 с requiresEmailConfirmation).
 * Тогда в теле ответа лежит email, на который отправлен код.
 */
export function isEmailConfirmationRequired(
  error: unknown
): error is ApiError & { body: EmailConfirmationRequiredBody } {
  if (!(error instanceof ApiError) || error.status !== 403) {
    return false
  }
  const body = error.body as { requiresEmailConfirmation?: unknown } | null | undefined
  return typeof body === 'object' && body !== null && body.requiresEmailConfirmation === true
}
