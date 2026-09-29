import { useQuery } from '@tanstack/react-query'
import { getCharacterBag } from '../api/characters'
import { RARITY_BADGE_CLASSES, RARITY_NAMES, RarityEnum } from '../types/enums'
import type { CharacterBag, GatheringResult, PotionBagItem } from '../types/models'

interface BagPanelProps {
  characterId: string | null
  isOpen: boolean
  onClose: () => void
}

/** Панель сумки (offcanvas), аналог #bagPanel из _Layout.cshtml. */
export function BagPanel({ characterId, isOpen, onClose }: BagPanelProps) {
  const { data: bag, isLoading } = useQuery({
    queryKey: ['character-bag', characterId],
    queryFn: () => getCharacterBag(characterId!),
    enabled: isOpen && !!characterId,
  })

  return (
    <>
      {isOpen && (
        <div className="fixed inset-0 z-30 bg-black/40" onClick={onClose} aria-hidden="true" />
      )}
      <div
        className={`fixed right-0 top-0 z-40 h-full w-80 max-w-full transform bg-white shadow-xl transition-transform duration-300 ${
          isOpen ? 'translate-x-0' : 'translate-x-full'
        }`}
      >
        <div className="flex items-center justify-between border-b border-slate-200 p-4">
          <h2 className="text-lg font-semibold">🎒 Сумка</h2>
          <button
            type="button"
            className="rounded p-1 text-slate-500 hover:bg-slate-100"
            onClick={onClose}
            aria-label="Закрыть"
          >
            ✕
          </button>
        </div>
        <div className="h-[calc(100%-64px)] overflow-y-auto p-4">
          {!characterId && <p className="text-center text-slate-500">Персонаж не выбран</p>}
          {characterId && isLoading && <p className="text-center text-slate-500">Загрузка...</p>}
          {bag && <BagContent bag={bag} />}
        </div>
      </div>
    </>
  )
}

function BagContent({ bag }: { bag: CharacterBag }) {
  return (
    <div className="space-y-4 text-sm">
      <div>
        💰 {bag.goldCoins} зм {bag.silverCoins} см {bag.copperCoins} мм
      </div>
      <BagSection
        title="Травы"
        icon="🌿"
        items={bag.herbs}
        getName={(i) => i.herb?.name ?? '?'}
        getRarity={(i) => i.herb?.rarity ?? 0}
      />
      <BagSection
        title="Зелья"
        icon="🧪"
        items={bag.potions}
        getName={(i) => i.potion?.name ?? '?'}
        getRarity={() => 0}
      />
      <BagSection
        title="Яды"
        icon="☠️"
        items={bag.poisons}
        getName={(i) => i.potion?.name ?? '?'}
        getRarity={() => 0}
      />
    </div>
  )
}

interface BagSectionProps<T> {
  title: string
  icon: string
  items: Record<string, T & { quantity: number }>
  getName: (item: T & { quantity: number }) => string
  getRarity: (item: T & { quantity: number }) => RarityEnum
}

function BagSection<T extends GatheringResult | PotionBagItem>({
  title,
  icon,
  items,
  getName,
  getRarity,
}: BagSectionProps<T>) {
  const entries = Object.values(items)

  return (
    <div>
      <h3 className="mb-1 font-semibold text-slate-800">
        {icon} {title}
      </h3>
      {entries.length === 0 ? (
        <p className="ml-2 text-slate-500">Пусто</p>
      ) : (
        <ul className="ml-2 space-y-1">
          {entries.map((item, index) => (
            <li key={index} className="flex items-center justify-between">
              <span>
                {getName(item)}{' '}
                <span
                  className={`ml-1 rounded px-1.5 py-0.5 text-xs text-white ${RARITY_BADGE_CLASSES[getRarity(item)]}`}
                >
                  {RARITY_NAMES[getRarity(item)]}
                </span>
              </span>
              <span className="font-mono">{item.quantity}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
