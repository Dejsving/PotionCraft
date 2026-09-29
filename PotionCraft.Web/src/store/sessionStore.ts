import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { PlayerCharacter } from '../types/models'

interface SessionState {
  /** Идентификатор игрока, генерируется один раз и сохраняется в localStorage. */
  playerId: string
  /** Текущий выбранный персонаж игрока. */
  activeCharacter: PlayerCharacter | null
  setActiveCharacter: (character: PlayerCharacter | null) => void
  clearActiveCharacter: () => void
}

/** Хранилище сессии игрока (playerId и активный персонаж), сохраняется в localStorage. */
export const useSessionStore = create<SessionState>()(
  persist(
    (set) => ({
      playerId: crypto.randomUUID(),
      activeCharacter: null,
      setActiveCharacter: (character) => set({ activeCharacter: character }),
      clearActiveCharacter: () => set({ activeCharacter: null }),
    }),
    {
      name: 'potioncraft-session',
    }
  )
)
