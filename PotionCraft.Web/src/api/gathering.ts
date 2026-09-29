import { apiClient } from './client'
import type { GatheringApiRequest, GatheringApiResponse } from '../types/models'

/** Выполняет броски на сбор трав для персонажа. POST /api/gathering/{characterId} */
export function gatherHerbs(
  characterId: string,
  request: GatheringApiRequest
): Promise<GatheringApiResponse> {
  return apiClient.post<GatheringApiResponse>(`/api/gathering/${characterId}`, request)
}
