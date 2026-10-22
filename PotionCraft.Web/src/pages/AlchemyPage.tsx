import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { getCharacterBag } from '../api/characters'
import { resolvePotionEffect } from '../api/alchemy'
import { HERB_TYPE_NAMES, HerbTypeEnum, hasHerbType } from '../types/enums'
import type { Herb } from '../types/models'
import { useHerbs } from '../hooks/useHerbs'
import { useSessionStore } from '../store/sessionStore'
import { getBasePotionTypeLabel } from '../lib/alchemy'

const BASE_DIFFICULTY = 10
const MAX_MODIFIERS = 3

type ModifierSlot = {
  id: number
  herbId: string
}

/** Страница составления зелья и отображения рассчитанного сервером эффекта. */
export function AlchemyPage() {
  const activeCharacter = useSessionStore((state) => state.activeCharacter)
  const herbsQuery = useHerbs()
  const [baseHerbId, setBaseHerbId] = useState('')
  const [modifierSlots, setModifierSlots] = useState<ModifierSlot[]>([])
  const [nextSlotId, setNextSlotId] = useState(0)
  const [bagOnly, setBagOnly] = useState(false)

  const bagQuery = useQuery({
    queryKey: ['characterBag', activeCharacter?.id],
    queryFn: () => getCharacterBag(activeCharacter!.id),
    enabled: bagOnly && Boolean(activeCharacter),
  })

  const herbs = herbsQuery.data ?? []
  const baseHerbs = herbs.filter(
    (herb) =>
      hasHerbType(herb.herbType, HerbTypeEnum.HealingBase) ||
      hasHerbType(herb.herbType, HerbTypeEnum.PoisonBase) ||
      hasHerbType(herb.herbType, HerbTypeEnum.Magic)
  )
  const selectedBase = herbs.find((herb) => herb.id === baseHerbId)
  const bagQuantities = new Map(
    Object.entries(bagQuery.data?.herbs ?? {}).map(([id, item]) => [id.toLowerCase(), item.quantity])
  )

  /** Возвращает количество травы в сумке либо ноль, если её там нет. */
  function getBagQuantity(herbId: string): number {
    return bagQuantities.get(herbId.toLowerCase()) ?? 0
  }

  /** Считает количество выбранных экземпляров, кроме редактируемого слота. */
  function getUsedCount(herbId: string, excludedSlotId?: number): number {
    let count = selectedBase?.id === herbId ? 1 : 0
    for (const slot of modifierSlots) {
      if (slot.id !== excludedSlotId && slot.herbId === herbId) count += 1
    }
    return count
  }

  /** Проверяет, останется ли указанная трава доступна с учётом выбранного состава. */
  function isAvailable(herbId: string, excludedSlotId?: number): boolean {
    return !bagOnly || getBagQuantity(herbId) > getUsedCount(herbId, excludedSlotId)
  }

  const baseIsAvailable = Boolean(selectedBase) && (!bagOnly || getBagQuantity(baseHerbId) > 0)
  const activeBaseId = baseIsAvailable ? baseHerbId : ''
  const activeBase = baseIsAvailable ? selectedBase : undefined
  const modifierType = activeBase
    ? hasHerbType(activeBase.herbType, HerbTypeEnum.HealingBase)
      ? HerbTypeEnum.HealingModifier
      : hasHerbType(activeBase.herbType, HerbTypeEnum.PoisonBase)
        ? HerbTypeEnum.PoisonModifier
        : null
    : null
  const modifierHerbs = modifierType !== null
    ? herbs.filter((herb) => hasHerbType(herb.herbType, modifierType))
    : []

  const usedModifierCounts = new Map<string, number>()
  const modifierHerbIds = modifierSlots.flatMap((slot) => {
    if (!slot.herbId || !modifierHerbs.some((herb) => herb.id === slot.herbId)) return []

    const alreadyUsed = (usedModifierCounts.get(slot.herbId) ?? 0) +
      (activeBase?.id === slot.herbId ? 1 : 0)
    if (bagOnly && getBagQuantity(slot.herbId) <= alreadyUsed) return []

    usedModifierCounts.set(slot.herbId, (usedModifierCounts.get(slot.herbId) ?? 0) + 1)
    return [slot.herbId]
  })

  const resolveQuery = useQuery({
    queryKey: ['alchemyResolve', activeBaseId, modifierHerbIds],
    queryFn: () => resolvePotionEffect({ baseHerbId: activeBaseId, modifierHerbIds }),
    enabled: Boolean(activeBaseId),
  })

  const composition = [activeBase, ...modifierHerbIds.map((id) => herbs.find((herb) => herb.id === id))]
    .filter((herb): herb is Herb => Boolean(herb))
  const difficulty = BASE_DIFFICULTY + composition.reduce((total, herb) => total + herb.difficulty, 0)

  /** Обновляет выбранную траву в строке модификатора, сохраняя порядок добавления. */
  function updateModifier(slotId: number, herbId: string) {
    setModifierSlots((slots) =>
      slots.map((slot) => (slot.id === slotId ? { ...slot, herbId } : slot))
    )
  }

  /** Добавляет пустую строку модификатора, если выбрана основа и лимит не достигнут. */
  function addModifier() {
    if (!activeBase || modifierSlots.length >= MAX_MODIFIERS) return
    setModifierSlots((slots) => [...slots, { id: nextSlotId, herbId: '' }])
    setNextSlotId((id) => id + 1)
  }

  /** Удаляет строку модификатора, после чего состав пересчитывается сервером. */
  function removeModifier(slotId: number) {
    setModifierSlots((slots) => slots.filter((slot) => slot.id !== slotId))
  }

  /** Возвращает доступные варианты модификатора для конкретного слота. */
  function getModifierOptions(slot: ModifierSlot): Herb[] {
    return modifierHerbs.filter(
      (herb) =>
        (!bagOnly || isAvailable(herb.id, slot.id) || herb.id === slot.herbId) &&
        (herb.id === slot.herbId || !bagOnly || getBagQuantity(herb.id) > 0)
    )
  }

  return (
    <main className="mx-auto max-w-6xl px-4 py-8 sm:px-6">
      <header className="mb-7 border-b border-stone-300 pb-5">
        <p className="text-sm font-semibold uppercase text-emerald-800">Лаборатория</p>
        <h1 className="mt-1 text-3xl font-semibold text-stone-900">Алхимия</h1>
        <p className="mt-2 text-stone-600">Составьте формулу из основы и подходящих модификаторов.</p>
      </header>

      <div className="mb-6 flex flex-wrap items-center gap-x-4 gap-y-2 border-b border-stone-200 pb-5">
        <label className="inline-flex min-h-10 cursor-pointer items-center gap-3 text-sm font-semibold text-stone-800">
          <input
            type="checkbox"
            checked={bagOnly}
            onChange={(event) => setBagOnly(event.target.checked)}
            className="size-4 accent-emerald-800"
          />
          Только содержимое сумки
        </label>
        {bagOnly && !activeCharacter && (
          <p className="text-sm text-amber-800" role="status">
            Персонаж не выбран. <Link className="font-semibold underline" to="/characters/select">Выберите персонажа</Link>, чтобы загрузить сумку.
          </p>
        )}
        {bagOnly && activeCharacter && bagQuery.isLoading && (
          <p className="text-sm text-stone-600" role="status">Загрузка сумки…</p>
        )}
        {bagOnly && bagQuery.isError && (
          <p className="text-sm text-red-800" role="alert">Не удалось загрузить содержимое сумки.</p>
        )}
      </div>

      {herbsQuery.isLoading ? (
        <p className="py-10 text-center text-stone-600" role="status">Загрузка справочника трав…</p>
      ) : herbsQuery.isError ? (
        <p className="border-l-4 border-red-700 bg-red-50 p-4 text-red-900" role="alert">
          Не удалось загрузить список трав: {herbsQuery.error.message}
        </p>
      ) : (
        <>
          <div className="grid gap-8 lg:grid-cols-[minmax(0,0.9fr)_minmax(0,1.1fr)]">
            <section className="border-t-2 border-emerald-800 pt-4">
              <h2 className="text-lg font-semibold text-stone-900">Основной ингредиент</h2>
              <label className="mt-4 block text-sm font-semibold text-stone-700">
                Трава-основа определяет тип зелья
                <select
                  className="mt-2 block min-h-11 w-full rounded-md border border-stone-300 bg-white px-3 text-base font-normal text-stone-900 focus:border-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-700/20"
                  value={activeBaseId}
                  onChange={(event) => {
                    setBaseHerbId(event.target.value)
                    setModifierSlots([])
                  }}
                >
                  <option value="">— Выберите основу —</option>
                  {baseHerbs
                    .filter((herb) => !bagOnly || getBagQuantity(herb.id) > 0 || herb.id === baseHerbId)
                    .sort((left, right) => left.name.localeCompare(right.name, 'ru'))
                    .map((herb) => (
                      <option key={herb.id} value={herb.id}>
                        {herb.name} — {getBasePotionTypeLabel(herb.herbType)}
                        {bagOnly ? ` (x${getBagQuantity(herb.id)})` : ''}
                      </option>
                    ))}
                </select>
              </label>
              {selectedBase && bagOnly && !baseIsAvailable && (
                <p className="mt-2 text-sm text-amber-800">Выбранной основы нет в сумке.</p>
              )}
              {activeBase && (
                <>
                  <p className="mt-3 text-sm font-semibold text-emerald-800">
                    {getBasePotionTypeLabel(activeBase.herbType)}
                  </p>
                  <p className="mt-1 text-sm leading-6 text-stone-600">{activeBase.effect}</p>
                </>
              )}
            </section>

            <section className="border-t-2 border-teal-700 pt-4">
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div>
                  <h2 className="text-lg font-semibold text-stone-900">Модификаторы</h2>
                  <p className="mt-1 text-sm text-stone-600">Выбрано строк: {modifierSlots.length} / {MAX_MODIFIERS}</p>
                </div>
                <button
                  type="button"
                  onClick={addModifier}
                  disabled={!activeBase || modifierSlots.length >= MAX_MODIFIERS}
                  className="inline-flex min-h-10 items-center gap-2 rounded-md border border-teal-800 px-3 text-sm font-semibold text-teal-900 hover:bg-teal-50 disabled:cursor-not-allowed disabled:opacity-45"
                >
                  <span aria-hidden="true">+</span>
                  Добавить
                </button>
              </div>
              {!activeBase ? (
                <p className="mt-5 text-sm text-stone-500">Сначала выберите основной ингредиент.</p>
              ) : modifierSlots.length === 0 ? (
                <p className="mt-5 text-sm text-stone-500">Нажмите «Добавить», чтобы выбрать модификаторы.</p>
              ) : (
                <div className="mt-4 space-y-3">
                  {modifierSlots.map((slot, index) => (
                    <div key={slot.id} className="flex items-center gap-2">
                      <label className="min-w-0 flex-1">
                        <span className="sr-only">Модификатор {index + 1}</span>
                        <select
                          className="block min-h-11 w-full rounded-md border border-stone-300 bg-white px-3 text-sm text-stone-900 focus:border-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-700/20"
                          value={slot.herbId}
                          onChange={(event) => updateModifier(slot.id, event.target.value)}
                        >
                          <option value="">— Выберите модификатор —</option>
                          {getModifierOptions(slot)
                            .sort((left, right) => left.name.localeCompare(right.name, 'ru'))
                            .map((herb) => (
                              <option key={herb.id} value={herb.id}>
                                {herb.name}{bagOnly ? ` (x${getBagQuantity(herb.id)})` : ''}
                              </option>
                            ))}
                        </select>
                      </label>
                      <button
                        type="button"
                        onClick={() => removeModifier(slot.id)}
                        className="inline-flex size-10 shrink-0 items-center justify-center rounded-md border border-red-300 text-lg text-red-800 hover:bg-red-50"
                        aria-label={`Удалить модификатор ${index + 1}`}
                        title="Удалить модификатор"
                      >
                        ×
                      </button>
                    </div>
                  ))}
                </div>
              )}
              {activeBase && modifierSlots.length === MAX_MODIFIERS && (
                <p className="mt-3 text-sm text-amber-800">
                  Достигнут максимум: зелье с более чем тремя модификаторами нестабильно.
                </p>
              )}
            </section>
          </div>

          <section className="mt-9 border-t-2 border-stone-800 pt-5" aria-live="polite">
            <div className="flex flex-wrap items-baseline justify-between gap-3">
              <h2 className="text-xl font-semibold text-stone-900">Параметры зелья</h2>
              <p className="text-sm font-semibold text-stone-700">
                Сложность: <span className="ml-1 text-xl tabular-nums text-emerald-800">{difficulty}</span>
              </p>
            </div>

            {!activeBase ? (
              <p className="mt-5 text-sm italic text-stone-500">Добавьте ингредиенты, чтобы увидеть эффекты зелья.</p>
            ) : resolveQuery.isLoading ? (
              <p className="mt-5 text-sm text-stone-600" role="status">Расчёт эффекта…</p>
            ) : resolveQuery.isError ? (
              <p className="mt-5 border-l-4 border-red-700 bg-red-50 p-3 text-sm text-red-900" role="alert">
                Не удалось рассчитать эффект зелья: {resolveQuery.error.message}
              </p>
            ) : resolveQuery.data ? (
              <div className="mt-5 space-y-4">
                <div>
                  <h3 className="text-sm font-semibold text-stone-800">Состав зелья</h3>
                  <ul className="mt-2 divide-y divide-stone-200 border-y border-stone-200">
                    {composition.map((herb, index) => (
                      <li key={`${herb.id}-${index}`} className="py-3">
                        <p className="font-semibold text-stone-900">{herb.name}</p>
                        {herb.effect && <p className="mt-1 text-sm text-stone-600">{herb.effect}</p>}
                        {index === 0 && (
                          <p className="mt-1 text-xs text-stone-500">
                            {HERB_TYPE_NAMES[herb.herbType as HerbTypeEnum] ?? 'Основа зелья'}
                          </p>
                        )}
                      </li>
                    ))}
                  </ul>
                </div>
                <div>
                  <h3 className="text-sm font-semibold text-stone-800">Итоговый эффект зелья</h3>
                  <p className="mt-2 rounded-md bg-emerald-50 px-4 py-3 font-semibold leading-6 text-stone-900">
                    {resolveQuery.data.formulaText}
                  </p>
                  {resolveQuery.data.generatedDescription && (
                    <p className="mt-2 text-sm leading-6 text-stone-700">
                      {resolveQuery.data.generatedDescription}
                    </p>
                  )}
                  {resolveQuery.data.notes.map((note, index) => (
                    <p key={`${index}-${note}`} className="mt-2 border-l-2 border-stone-400 bg-stone-50 px-3 py-2 text-sm text-stone-700">
                      {note}
                    </p>
                  ))}
                  {resolveQuery.data.dmJudgementNote && (
                    <p className="mt-2 border-l-4 border-amber-600 bg-amber-50 px-3 py-2 text-sm text-amber-950">
                      {resolveQuery.data.dmJudgementNote}
                    </p>
                  )}
                </div>
              </div>
            ) : null}
          </section>
        </>
      )}
    </main>
  )
}
