import { apiClient } from './client'
import type { Herb } from '../types/models'

/** Возвращает список всех трав. GET /api/herbs */
export function getHerbs(): Promise<Herb[]> {
  return apiClient.get<Herb[]>('/api/herbs')
}
