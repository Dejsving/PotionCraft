import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { PlayerCharacter } from '../types/models'

/** Версия схемы хранилища сессии. Версия 1 удалила playerId (заменён id авторизованного пользователя). */
const SESSION_STORE_VERSION = 1

interface SessionState {
  /** Текущий выбранный персонаж игрока. */
  activeCharacter: PlayerCharacter | null
  setActiveCharacter: (character: PlayerCharacter | null) => void
  clearActiveCharacter: () => void
}

/** Хранилище сессии игрока (активный персонаж), сохраняется в localStorage. */
export const useSessionStore = create<SessionState>()(
  persist<SessionState, [], [], Pick<SessionState, 'activeCharacter'>>(
    (set) => ({
      activeCharacter: null,
      setActiveCharacter: (character) => set({ activeCharacter: character }),
      clearActiveCharacter: () => set({ activeCharacter: null }),
    }),
    {
      name: 'potioncraft-session',
      version: SESSION_STORE_VERSION,
      // При миграции со старой версии сбрасываем активного персонажа: он привязан к анонимному playerId.
      migrate: () => ({ activeCharacter: null }),
    }
  )
)
