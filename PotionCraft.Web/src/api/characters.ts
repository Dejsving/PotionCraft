import { apiClient } from './client'
import type {
  CharacterBag,
  CoinsUpdateRequest,
  CreateCharacterRequest,
  DeselectCharacterRequest,
  PlayerCharacter,
  SelectCharacterRequest,
} from '../types/models'

/** Возвращает список всех персонажей. GET /api/characters */
export function getCharacters(): Promise<PlayerCharacter[]> {
  return apiClient.get<PlayerCharacter[]>('/api/characters')
}

/** Создаёт нового персонажа. POST /api/characters */
export function createCharacter(request: CreateCharacterRequest): Promise<PlayerCharacter> {
  return apiClient.post<PlayerCharacter>('/api/characters', request)
}

/** Выбирает персонажа для текущего игрока. POST /api/characters/{id}/select */
export function selectCharacter(
  id: string,
  request: SelectCharacterRequest
): Promise<PlayerCharacter> {
  return apiClient.post<PlayerCharacter>(`/api/characters/${id}/select`, request)
}

/** Снимает выбор персонажа для текущего игрока. POST /api/characters/{id}/deselect */
export function deselectCharacter(id: string, request: DeselectCharacterRequest): Promise<void> {
  return apiClient.post<void>(`/api/characters/${id}/deselect`, request)
}

/** Возвращает сумку персонажа. GET /api/characters/{id}/bag */
export function getCharacterBag(id: string): Promise<CharacterBag> {
  return apiClient.get<CharacterBag>(`/api/characters/${id}/bag`)
}

/** Изменяет количество монет в сумке персонажа. POST /api/characters/{id}/bag/coins */
export function updateCharacterCoins(
  id: string,
  request: CoinsUpdateRequest
): Promise<CharacterBag> {
  return apiClient.post<CharacterBag>(`/api/characters/${id}/bag/coins`, request)
}
