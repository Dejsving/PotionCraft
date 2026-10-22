import { queryOptions, useQuery } from '@tanstack/react-query'
import { getCurrentUser } from '../api/auth'

/** Ключ кэша текущего пользователя в TanStack Query. */
export const currentUserQueryKey = ['auth', 'me'] as const

/** Опции запроса текущего пользователя: null означает, что сессии нет. */
export const currentUserQuery = queryOptions({
  queryKey: currentUserQueryKey,
  queryFn: getCurrentUser,
  staleTime: 5 * 60 * 1000,
  retry: false,
})

/** Загружает и кэширует текущего пользователя (null — не авторизован). */
export function useCurrentUser() {
  return useQuery(currentUserQuery)
}
