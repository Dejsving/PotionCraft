import { useState } from 'react'
import { useSessionStore } from '../store/sessionStore'

/** Badge активного персонажа с popover-подсказкой характеристик, аналог badge из _Layout.cshtml. */
export function ActiveCharacterBadge() {
  const activeCharacter = useSessionStore((state) => state.activeCharacter)
  const [isPopoverOpen, setIsPopoverOpen] = useState(false)

  if (!activeCharacter) {
    return <span className="text-sm font-semibold text-slate-700">Персонаж не выбран</span>
  }

  return (
    <div className="relative">
      <button
        type="button"
        className="text-sm font-semibold text-violet-700 hover:text-violet-900"
        onClick={() => setIsPopoverOpen((open) => !open)}
        onBlur={() => setIsPopoverOpen(false)}
      >
        👤 {activeCharacter.name}
      </button>
      {isPopoverOpen && (
        <div className="absolute right-0 top-full z-20 mt-2 w-56 rounded-md border border-slate-200 bg-white p-3 text-left text-sm shadow-lg">
          <p className="mb-2 font-semibold text-slate-900">Характеристики</p>
          <div className="space-y-1 text-slate-700">
            <div>
              <strong>Интеллект:</strong> {activeCharacter.intelligence}
            </div>
            <div>
              <strong>Мудрость:</strong> {activeCharacter.wisdom}
            </div>
            <div>
              <strong>Бонус мастерства:</strong> +{activeCharacter.proficiencyBonus}
            </div>
            <hr className="my-1 border-slate-200" />
            <div>
              <strong>Алхимия:</strong> {activeCharacter.alchemistModify >= 0 ? '+' : ''}
              {activeCharacter.alchemistModify}
            </div>
            <div>
              <strong>Травничество:</strong> {activeCharacter.herbalismModify >= 0 ? '+' : ''}
              {activeCharacter.herbalismModify}
            </div>
            <div>
              <strong>Отравитель:</strong> {activeCharacter.poisonerModify >= 0 ? '+' : ''}
              {activeCharacter.poisonerModify}
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
