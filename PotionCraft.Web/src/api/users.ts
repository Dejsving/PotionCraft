import { apiClient } from './client'
import type { UserRole, UserSummary } from '../types/models'

/** Возвращает список пользователей (только для Admin). GET /api/users */
export function getUsers(): Promise<UserSummary[]> {
  return apiClient.get<UserSummary[]>('/api/users')
}

/** Меняет роль пользователя (только для Admin). PUT /api/users/{id}/role */
export function setUserRole(id: string, role: UserRole): Promise<UserSummary> {
  return apiClient.put<UserSummary>(`/api/users/${id}/role`, { role })
}
