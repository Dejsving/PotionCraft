import { apiClient } from './client'
import type { PotionEffectResult, ResolvePotionEffectRequest } from '../types/models'

/** Вычисляет итоговый эффект зелья по травам. POST /api/alchemy/resolve */
export function resolvePotionEffect(
  request: ResolvePotionEffectRequest
): Promise<PotionEffectResult> {
  return apiClient.post<PotionEffectResult>('/api/alchemy/resolve', request)
}
