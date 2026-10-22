import type { QueryClient } from '@tanstack/react-query'
import { currentUserQueryKey } from '../hooks/useCurrentUser'
import { useSessionStore } from '../store/sessionStore'
import type { CurrentUser } from '../types/models'

/**
 * Применяет успешный вход или регистрацию: сохраняет пользователя в кэше сессии
 * и сбрасывает активного персонажа, выбранного предыдущим пользователем.
 */
export function applySignedInUser(queryClient: QueryClient, user: CurrentUser): void {
  queryClient.setQueryData(currentUserQueryKey, user)
  useSessionStore.getState().clearActiveCharacter()
}
