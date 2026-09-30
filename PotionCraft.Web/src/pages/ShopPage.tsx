import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { getCharacterBag } from '../api/characters'
import { getShopInventory, tradeShopItems } from '../api/shop'
import { calcBuyTotal, calcSellTotal } from '../lib/priceCalculator'
import { useSessionStore } from '../store/sessionStore'
import type { CharacterBag, ShopItem, TradeItem } from '../types/models'

type ItemCategory = ShopItem['category']
type CategoryFilter = 'all' | ItemCategory

interface SellLine {
  id: string
  name: string
  category: ItemCategory
  rarity: number
  sellPrice: number
  quantity: number
}

const categoryNames: Record<ItemCategory, string> = {
  herb: 'Травы',
  potion: 'Зелья',
  poison: 'Яды',
}

const categoryFilters: Array<{ value: CategoryFilter; label: string }> = [
  { value: 'all', label: 'Все' },
  { value: 'herb', label: 'Травы' },
  { value: 'potion', label: 'Зелья' },
  { value: 'poison', label: 'Яды' },
]

/** Страница магазина: ассортимент, сделка и сумка активного персонажа. */
export function ShopPage() {
  const queryClient = useQueryClient()
  const activeCharacter = useSessionStore((state) => state.activeCharacter)
  const setActiveCharacter = useSessionStore((state) => state.setActiveCharacter)
  const [filter, setFilter] = useState<CategoryFilter>('all')
  const [buyLines, setBuyLines] = useState<Map<string, { item: ShopItem; quantity: number }>>(
    () => new Map(),
  )
  const [sellLines, setSellLines] = useState<Map<string, SellLine>>(() => new Map())
  const [tradeMessage, setTradeMessage] = useState('')

  const inventoryQuery = useQuery({
    queryKey: ['shopInventory'],
    queryFn: getShopInventory,
  })
  const bagQuery = useQuery({
    queryKey: ['characterBag', activeCharacter?.id],
    queryFn: () => getCharacterBag(activeCharacter!.id),
    enabled: Boolean(activeCharacter),
  })
  const bag = bagQuery.data
  const inventory = inventoryQuery.data ?? []
  const buyTotal = [...buyLines.values()].reduce(
    (total, line) => total + calcBuyTotal(line.item.buyPrice, line.item.rarity, line.quantity),
    0,
  )
  const sellTotal = [...sellLines.values()].reduce(
    (total, line) => total + calcSellTotal(line.sellPrice, line.rarity, line.quantity),
    0,
  )
  const balance = sellTotal - buyTotal
  const playerCoins = bag?.coins ?? 0
  const tradeItemsExist = buyLines.size > 0 || sellLines.size > 0
  const canAffordPreview = balance >= 0 || Math.round(Math.abs(balance) * 100) <= playerCoins

  const tradeMutation = useMutation({
    mutationFn: tradeShopItems,
    onSuccess: (result) => {
      if (!result.success || !result.updatedBag) {
        setTradeMessage(result.message || 'Сделка не выполнена.')
        return
      }

      const updatedBag = result.updatedBag
      queryClient.setQueryData(['characterBag', activeCharacter?.id], updatedBag)
      queryClient.setQueryData(['character-bag', activeCharacter?.id], updatedBag)
      if (activeCharacter) {
        setActiveCharacter({ ...activeCharacter, bag: updatedBag })
      }
      queryClient.invalidateQueries({ queryKey: ['shopInventory'] })
      setBuyLines(new Map())
      setSellLines(new Map())
      setTradeMessage(result.message)
    },
    onError: (error: Error) => {
      setTradeMessage(error.message || 'Ошибка соединения с сервером.')
    },
  })

  /** Добавляет единицу товара в покупку, не превышая остаток торговца. */
  function addBuyItem(item: ShopItem) {
    setTradeMessage('')
    setBuyLines((current) => {
      const next = new Map(current)
      const quantity = next.get(item.id)?.quantity ?? 0
      if (quantity < item.availableQuantity) next.set(item.id, { item, quantity: quantity + 1 })
      return next
    })
  }

  /** Добавляет единицу травы в продажу, не превышая количество в сумке. */
  function addSellItem(line: SellLine, available: number) {
    if (available <= 0 || line.category !== 'herb' || line.sellPrice <= 0) return
    setTradeMessage('')
    setSellLines((current) => {
      const next = new Map(current)
      const quantity = next.get(line.id)?.quantity ?? 0
      if (quantity < available) next.set(line.id, { ...line, quantity: quantity + 1 })
      return next
    })
  }

  /** Удаляет строку покупки или продажи из текущей сделки. */
  function removeLine(kind: 'buy' | 'sell', id: string) {
    if (kind === 'buy') {
      setBuyLines((current) => {
        const next = new Map(current)
        next.delete(id)
        return next
      })
    } else {
      setSellLines((current) => {
        const next = new Map(current)
        next.delete(id)
        return next
      })
    }
  }

  /** Обновляет количество строки сделки, удаляя её при нулевом значении. */
  function updateLineQuantity(kind: 'buy' | 'sell', id: string, value: number, max: number) {
    const quantity = Math.min(Math.max(Math.trunc(value) || 0, 0), max)
    if (kind === 'buy') {
      setBuyLines((current) => {
        const next = new Map(current)
        const line = next.get(id)
        if (!line || quantity === 0) next.delete(id)
        else next.set(id, { ...line, quantity })
        return next
      })
    } else {
      setSellLines((current) => {
        const next = new Map(current)
        const line = next.get(id)
        if (!line || quantity === 0) next.delete(id)
        else next.set(id, { ...line, quantity })
        return next
      })
    }
  }

  /** Создаёт запрос из обеих частей сделки и передаёт его серверу. */
  function confirmTrade() {
    if (!activeCharacter || !tradeItemsExist) return
    setTradeMessage('')
    const itemsToBuy: TradeItem[] = [...buyLines.values()].map(({ item, quantity }) => ({
      itemId: item.id,
      quantity,
      category: item.category,
    }))
    const itemsToSell: TradeItem[] = [...sellLines.values()].map((line) => ({
      itemId: line.id,
      quantity: line.quantity,
      category: line.category,
    }))
    tradeMutation.mutate({ characterId: activeCharacter.id, itemsToBuy, itemsToSell })
  }

  const playerItems = getBagItems(bag, inventory)

  return (
    <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6">
      <header className="mb-7 border-b border-stone-300 pb-5">
        <p className="text-sm font-semibold uppercase text-emerald-800">Торговая площадь</p>
        <h1 className="mt-1 text-3xl font-semibold text-stone-900">Магазин</h1>
        <p className="mt-2 text-stone-600">Цены сделки предварительные; расчёт и проверку выполняет сервер.</p>
      </header>

      <div className="grid items-start gap-8 lg:grid-cols-3">
        <section className="min-w-0 border-t-2 border-emerald-800 pt-4">
          <div className="flex flex-wrap items-baseline justify-between gap-2">
            <h2 className="text-lg font-semibold text-stone-900">Ассортимент</h2>
            <span className="text-sm text-stone-500">Товары торговца</span>
          </div>
          <div className="mt-4 flex flex-wrap gap-2" aria-label="Фильтр ассортимента">
            {categoryFilters.map((option) => (
              <button
                key={option.value}
                type="button"
                aria-pressed={filter === option.value}
                onClick={() => setFilter(option.value)}
                className={`min-h-9 border px-3 text-sm font-semibold transition-colors ${
                  filter === option.value
                    ? 'border-emerald-900 bg-emerald-900 text-white'
                    : 'border-stone-300 text-stone-700 hover:bg-stone-100'
                }`}
              >
                {option.label}
              </button>
            ))}
          </div>
          {inventoryQuery.isLoading ? (
            <p className="py-8 text-center text-sm text-stone-600" role="status">Загрузка ассортимента…</p>
          ) : inventoryQuery.isError ? (
            <p className="mt-4 border-l-4 border-red-700 bg-red-50 p-3 text-sm text-red-900" role="alert">
              Не удалось загрузить ассортимент: {inventoryQuery.error.message}
            </p>
          ) : (
            <ul className="mt-3 divide-y divide-stone-200 border-y border-stone-200">
              {inventory
                .filter((item) => item.availableQuantity > 0)
                .filter((item) => filter === 'all' || item.category === filter)
                .map((item) => {
                  const inDeal = buyLines.get(item.id)?.quantity ?? 0
                  const remaining = item.availableQuantity - inDeal
                  return (
                    <li key={item.id}>
                      <button
                        type="button"
                        disabled={remaining <= 0}
                        onClick={() => addBuyItem(item)}
                        className="flex min-h-16 w-full items-center justify-between gap-3 py-2 text-left hover:bg-emerald-50 disabled:cursor-not-allowed disabled:opacity-45"
                      >
                        <span className="min-w-0">
                          <span className="block truncate text-sm font-semibold text-stone-900">{item.name}</span>
                          <span className="text-xs text-stone-500">{item.rarityName} · {categoryNames[item.category]}</span>
                        </span>
                        <span className="shrink-0 text-right">
                          <span className="block text-sm font-semibold text-red-800">{formatGold(item.buyPrice)}</span>
                          <span className="text-xs text-stone-500">В наличии: {remaining}</span>
                        </span>
                      </button>
                    </li>
                  )
                })}
              {inventory.filter((item) => item.availableQuantity > 0 && (filter === 'all' || item.category === filter)).length === 0 && (
                <li className="py-8 text-center text-sm text-stone-500">Нет товаров в этой категории.</li>
              )}
            </ul>
          )}
        </section>

        <section className="min-w-0 border-t-2 border-teal-700 pt-4">
          <div className="flex items-baseline justify-between gap-2">
            <h2 className="text-lg font-semibold text-stone-900">Сделка</h2>
            <span className="text-sm text-stone-500">{buyLines.size + sellLines.size} поз.</span>
          </div>

          <h3 className="mt-5 text-sm font-bold uppercase text-red-800">Покупка</h3>
          {buyLines.size === 0 ? (
            <p className="mt-2 text-sm text-stone-500">Добавьте товары из ассортимента.</p>
          ) : (
            <ul className="mt-2 divide-y divide-stone-200 border-y border-stone-200">
              {[...buyLines.entries()].map(([id, line]) => (
                <li key={id} className="flex items-center gap-2 py-2">
                  <span className="min-w-0 flex-1 truncate text-sm text-stone-800">{line.item.name}</span>
                  <span className="w-20 shrink-0 text-right text-sm text-red-800">
                    {formatGold(calcBuyTotal(line.item.buyPrice, line.item.rarity, line.quantity))}
                  </span>
                  <QuantityInput
                    label={`Количество ${line.item.name}`}
                    quantity={line.quantity}
                    max={line.item.availableQuantity}
                    onChange={(value) => updateLineQuantity('buy', id, value, line.item.availableQuantity)}
                  />
                  <RemoveButton label={`Убрать ${line.item.name}`} onClick={() => removeLine('buy', id)} />
                </li>
              ))}
            </ul>
          )}

          <h3 className="mt-5 text-sm font-bold uppercase text-emerald-800">Продажа</h3>
          {sellLines.size === 0 ? (
            <p className="mt-2 text-sm text-stone-500">Добавьте травы из сумки персонажа.</p>
          ) : (
            <ul className="mt-2 divide-y divide-stone-200 border-y border-stone-200">
              {[...sellLines.entries()].map(([id, line]) => (
                <li key={id} className="flex items-center gap-2 py-2">
                  <span className="min-w-0 flex-1 truncate text-sm text-stone-800">{line.name}</span>
                  <span className="w-20 shrink-0 text-right text-sm text-emerald-800">
                    {formatGold(calcSellTotal(line.sellPrice, line.rarity, line.quantity))}
                  </span>
                  <QuantityInput
                    label={`Количество ${line.name}`}
                    quantity={line.quantity}
                    max={getBagQuantity(bag, id)}
                    onChange={(value) => updateLineQuantity('sell', id, value, getBagQuantity(bag, id))}
                  />
                  <RemoveButton label={`Убрать ${line.name}`} onClick={() => removeLine('sell', id)} />
                </li>
              ))}
            </ul>
          )}

          <dl className="mt-5 space-y-2 border-t border-stone-300 pt-4 text-sm">
            <div className="flex justify-between gap-3"><dt>Покупка</dt><dd className="font-mono text-red-800">{formatGold(buyTotal)}</dd></div>
            <div className="flex justify-between gap-3"><dt>Продажа</dt><dd className="font-mono text-emerald-800">{formatGold(sellTotal)}</dd></div>
            <div className="flex justify-between gap-3 border-t border-stone-200 pt-2 font-bold">
              <dt>{balance >= 0 ? 'Ваш доход' : 'К оплате'}</dt>
              <dd className={`font-mono ${balance >= 0 ? 'text-emerald-800' : 'text-red-800'}`}>{formatGold(Math.abs(balance))}</dd>
            </div>
          </dl>
          {!activeCharacter && (
            <p className="mt-4 text-sm text-amber-900" role="status">
              Для сделки выберите <Link className="font-semibold underline" to="/characters/select">персонажа</Link>.
            </p>
          )}
          {tradeMessage && (
            <p className={`mt-4 border-l-4 p-3 text-sm ${tradeMutation.isError || !tradeMutation.data?.success ? 'border-red-700 bg-red-50 text-red-900' : 'border-emerald-700 bg-emerald-50 text-emerald-900'}`} role={tradeMutation.isError || !tradeMutation.data?.success ? 'alert' : 'status'}>
              {tradeMessage}
            </p>
          )}
          <button
            type="button"
            onClick={confirmTrade}
            disabled={!activeCharacter || !tradeItemsExist || !canAffordPreview || tradeMutation.isPending || bagQuery.isLoading}
            className="mt-4 min-h-11 w-full bg-emerald-900 px-4 text-sm font-bold text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-45"
          >
            {tradeMutation.isPending ? 'Обработка…' : 'Совершить сделку'}
          </button>
          {activeCharacter && !bagQuery.isLoading && !canAffordPreview && (
            <p className="mt-2 text-sm text-red-800">Предварительно недостаточно средств. Сервер проверит итоговую сумму.</p>
          )}
          {bagQuery.isError && <p className="mt-2 text-sm text-red-800" role="alert">Не удалось загрузить сумку: {bagQuery.error.message}</p>}
        </section>

        <section className="min-w-0 border-t-2 border-amber-700 pt-4">
          <div className="flex flex-wrap items-baseline justify-between gap-2">
            <h2 className="text-lg font-semibold text-stone-900">Сумка персонажа</h2>
            {bag && <span className="text-sm font-semibold text-stone-700">{activeCharacter?.name}</span>}
          </div>
          {!activeCharacter ? (
            <p className="py-8 text-center text-sm text-stone-500">Сначала выберите персонажа.</p>
          ) : bagQuery.isLoading ? (
            <p className="py-8 text-center text-sm text-stone-600" role="status">Загрузка сумки…</p>
          ) : bagQuery.isError ? (
            <p className="mt-4 border-l-4 border-red-700 bg-red-50 p-3 text-sm text-red-900" role="alert">Не удалось загрузить содержимое сумки.</p>
          ) : (
            <>
              {bag && <CoinBalance bag={bag} />}
              {(['herb', 'potion', 'poison'] as const).map((category) => {
                const items = playerItems.filter((item) => item.category === category)
                return (
                  <div key={category} className="mt-5">
                    <h3 className="border-b border-stone-300 pb-1 text-sm font-bold text-stone-800">{categoryNames[category]}</h3>
                    {items.length === 0 ? (
                      <p className="py-3 text-sm text-stone-500">Пусто.</p>
                    ) : (
                      <ul className="divide-y divide-stone-200">
                        {items.map((item) => {
                          const inDeal = sellLines.get(item.id)?.quantity ?? 0
                          const available = item.quantity - inDeal
                          const canSell = category === 'herb' && item.sellPrice > 0
                          return (
                            <li key={item.id}>
                              <button
                                type="button"
                                disabled={!canSell || available <= 0}
                                onClick={() => addSellItem(item, item.quantity)}
                                className="flex min-h-14 w-full items-center justify-between gap-3 py-2 text-left hover:bg-amber-50 disabled:cursor-not-allowed disabled:opacity-50"
                              >
                                <span className="min-w-0">
                                  <span className="block truncate text-sm font-semibold text-stone-900">{item.name}</span>
                                  <span className="text-xs text-stone-500">
                                    {canSell ? `Продажа ${formatGold(item.sellPrice)}` : 'Продажа недоступна'}
                                  </span>
                                </span>
                                <span className="shrink-0 text-right text-sm text-stone-600">
                                  {available} / {item.quantity}
                                </span>
                              </button>
                            </li>
                          )
                        })}
                      </ul>
                    )}
                  </div>
                )
              })}
            </>
          )}
        </section>
      </div>
    </main>
  )
}

/** Возвращает отображаемые предметы сумки и цену продажи для трав. */
function getBagItems(bag: CharacterBag | undefined, inventory: ShopItem[]): SellLine[] {
  if (!bag) return []
  const herbs: SellLine[] = Object.entries(bag.herbs).flatMap(([id, entry]) => {
    if (!entry.herb || entry.quantity <= 0) return []
    const shopItem = inventory.find((item) => item.id.toLowerCase() === id.toLowerCase())
    return [{
      id,
      name: entry.herb.name,
      category: 'herb',
      rarity: entry.herb.rarity,
      sellPrice: shopItem?.sellPrice ?? 0,
      quantity: entry.quantity,
    }]
  })
  const potions: SellLine[] = Object.entries(bag.potions).flatMap(([id, entry]) =>
    entry.potion && entry.quantity > 0
      ? [{ id, name: entry.potion.name, category: 'potion', rarity: 0, sellPrice: 0, quantity: entry.quantity }]
      : [],
  )
  const poisons: SellLine[] = Object.entries(bag.poisons).flatMap(([id, entry]) =>
    entry.potion && entry.quantity > 0
      ? [{ id, name: entry.potion.name, category: 'poison', rarity: 0, sellPrice: 0, quantity: entry.quantity }]
      : [],
  )
  return [...herbs, ...potions, ...poisons]
}

/** Возвращает количество указанной травы в сумке. */
function getBagQuantity(bag: CharacterBag | undefined, id: string): number {
  const entry = Object.entries(bag?.herbs ?? {}).find(([herbId]) => herbId.toLowerCase() === id.toLowerCase())
  return entry?.[1].quantity ?? 0
}

/** Форматирует цену в золотых монетах по правилам старого клиента. */
function formatGold(value: number): string {
  return `${value.toFixed(2)} зм`
}

/** Показывает баланс сумки в золотых, серебряных и медных монетах. */
function CoinBalance({ bag }: { bag: CharacterBag }) {
  return (
    <dl className="mt-4 grid grid-cols-3 gap-2 border-y border-stone-200 py-3 text-center">
      <div><dt className="text-xs text-stone-500">Золото</dt><dd className="font-mono font-semibold">{bag.goldCoins}</dd></div>
      <div><dt className="text-xs text-stone-500">Серебро</dt><dd className="font-mono font-semibold">{bag.silverCoins}</dd></div>
      <div><dt className="text-xs text-stone-500">Медь</dt><dd className="font-mono font-semibold">{bag.copperCoins}</dd></div>
    </dl>
  )
}

/** Числовое поле количества товара в корзине сделки. */
function QuantityInput({
  label,
  quantity,
  max,
  onChange,
}: {
  label: string
  quantity: number
  max: number
  onChange: (value: number) => void
}) {
  return (
    <input
      type="number"
      aria-label={label}
      min={1}
      max={max}
      value={quantity}
      onChange={(event) => onChange(Number(event.target.value))}
      className="h-9 w-14 shrink-0 border border-stone-300 bg-white text-center font-mono text-sm focus:border-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-700/20"
    />
  )
}

/** Кнопка удаления строки сделки с доступным названием. */
function RemoveButton({ label, onClick }: { label: string; onClick: () => void }) {
  return (
    <button
      type="button"
      aria-label={label}
      title={label}
      onClick={onClick}
      className="size-9 shrink-0 border border-stone-300 text-lg leading-none text-stone-700 hover:border-red-700 hover:text-red-800"
    >
      ×
    </button>
  )
}
