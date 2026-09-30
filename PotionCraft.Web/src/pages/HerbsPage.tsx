import { useState } from 'react'
import type { ReactNode } from 'react'
import { useHerbs } from '../hooks/useHerbs'
import {
  HERB_TYPE_NAMES,
  HerbTypeEnum,
  RARITY_NAMES,
  RarityEnum,
  TERRAIN_NAMES,
  TerrainEnum,
  hasHerbType,
} from '../types/enums'
import type { Herb } from '../types/models'

type SortColumn = 'name' | 'description' | 'herbType' | 'rarity' | 'effect' | 'difficulty' | 'habitat'

type SortState = {
  column: SortColumn | null
  descending: boolean
}

type Filters = {
  name: string
  description: string
  herbType: string
  rarity: string
  effect: string
  difficulty: string
  habitat: string
}

const initialFilters: Filters = {
  name: '',
  description: '',
  herbType: '',
  rarity: '',
  effect: '',
  difficulty: '',
  habitat: '',
}

const herbTypeOptions = Object.values(HerbTypeEnum).filter(
  (value): value is HerbTypeEnum => typeof value === 'number' && value !== HerbTypeEnum.None,
)
const rarityOptions = Object.values(RarityEnum).filter(
  (value): value is RarityEnum => typeof value === 'number',
)
const terrainOptions = Object.values(TerrainEnum).filter(
  (value): value is TerrainEnum => typeof value === 'number',
)

function getHabitatEntries(herb: Herb): Array<[TerrainEnum, number]> {
  return Object.entries(herb.habitats)
    .map(([terrain, difficulty]) => [Number(terrain), difficulty] as [TerrainEnum, number])
    .filter(([terrain]) => Number.isInteger(terrain) && terrain in TERRAIN_NAMES)
}

function getHerbTypeText(herb: Herb): string {
  return herbTypeOptions
    .filter((type) => hasHerbType(herb.herbType, type))
    .map((type) => HERB_TYPE_NAMES[type])
    .join(', ')
}

function getHabitatText(herb: Herb): string {
  return getHabitatEntries(herb)
    .map(([terrain, difficulty]) => `${TERRAIN_NAMES[terrain]} (${difficulty})`)
    .join(', ')
}

function getHabitatSortValue(herb: Herb): number {
  const terrains = getHabitatEntries(herb).map(([terrain]) => terrain)
  return terrains.length > 0 ? Math.min(...terrains) : Number.POSITIVE_INFINITY
}

/** Страница справочника трав с клиентской фильтрацией и сортировкой. */
export function HerbsPage() {
  const herbsQuery = useHerbs()
  const [filters, setFilters] = useState<Filters>(initialFilters)
  const [sort, setSort] = useState<SortState>({ column: null, descending: false })

  const herbs = herbsQuery.data ?? []
  const filteredHerbs = herbs.filter((herb) => {
    const difficulty = filters.difficulty === '' || herb.difficulty === Number(filters.difficulty)
    const habitat = filters.habitat === '' || Object.prototype.hasOwnProperty.call(herb.habitats, filters.habitat)

    return (
      herb.name.toLocaleLowerCase().includes(filters.name.toLocaleLowerCase()) &&
      herb.description.toLocaleLowerCase().includes(filters.description.toLocaleLowerCase()) &&
      (filters.herbType === '' || hasHerbType(herb.herbType, Number(filters.herbType))) &&
      (filters.rarity === '' || herb.rarity === Number(filters.rarity)) &&
      herb.effect.toLocaleLowerCase().includes(filters.effect.toLocaleLowerCase()) &&
      difficulty &&
      habitat
    )
  })

  const sortedHerbs = [...filteredHerbs].sort((left, right) => {
    if (!sort.column) return 0

    let result: number
    switch (sort.column) {
      case 'name':
        result = left.name.localeCompare(right.name, 'ru', { sensitivity: 'base' })
        break
      case 'description':
        result = left.description.localeCompare(right.description, 'ru', { sensitivity: 'base' })
        break
      case 'herbType':
        result = left.herbType - right.herbType
        break
      case 'rarity':
        result = left.rarity - right.rarity
        break
      case 'effect':
        result = left.effect.localeCompare(right.effect, 'ru', { sensitivity: 'base' })
        break
      case 'difficulty':
        result = left.difficulty - right.difficulty
        break
      case 'habitat':
        result = getHabitatSortValue(left) - getHabitatSortValue(right)
        break
    }

    return sort.descending ? -result : result
  })

  /** Обновляет одно поле клиентского фильтра. */
  function updateFilter(field: keyof Filters, value: string) {
    setFilters((current) => ({ ...current, [field]: value }))
  }

  /** Меняет направление сортировки выбранного столбца. */
  function sortBy(column: SortColumn) {
    setSort((current) => ({
      column,
      descending: current.column === column ? !current.descending : false,
    }))
  }

  /** Возвращает заголовок столбца с индикатором активного направления. */
  function sortLabel(column: SortColumn, label: string) {
    if (sort.column !== column) return label
    return `${label} ${sort.descending ? '▼' : '▲'}`
  }

  function renderHeader(column: SortColumn, label: string) {
    return (
      <th scope="col" className="whitespace-nowrap px-4 py-3 text-left">
        <button
          type="button"
          onClick={() => sortBy(column)}
          className="font-semibold text-white underline-offset-4 hover:underline"
        >
          {sortLabel(column, label)}
        </button>
      </th>
    )
  }

  return (
    <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6">
      <header className="mb-7 border-b border-stone-300 pb-5">
        <p className="text-sm font-semibold uppercase text-emerald-800">Справочник</p>
        <h1 className="mt-1 text-3xl font-semibold text-stone-900">Список трав</h1>
        <p className="mt-2 text-stone-600">Найдите нужное растение по свойствам, редкости и среде обитания.</p>
      </header>

      <div className="mb-6 grid gap-4 border-y border-stone-200 py-5 sm:grid-cols-2 lg:grid-cols-4">
        <FilterInput label="Название" value={filters.name} onChange={(value) => updateFilter('name', value)} />
        <FilterInput label="Описание" value={filters.description} onChange={(value) => updateFilter('description', value)} />
        <FilterSelect label="Тип" value={filters.herbType} onChange={(value) => updateFilter('herbType', value)}>
          <option value="">Все типы</option>
          {herbTypeOptions.map((type) => <option key={type} value={type}>{HERB_TYPE_NAMES[type]}</option>)}
        </FilterSelect>
        <FilterSelect label="Редкость" value={filters.rarity} onChange={(value) => updateFilter('rarity', value)}>
          <option value="">Все редкости</option>
          {rarityOptions.map((rarity) => <option key={rarity} value={rarity}>{RARITY_NAMES[rarity]}</option>)}
        </FilterSelect>
        <FilterInput label="Эффект" value={filters.effect} onChange={(value) => updateFilter('effect', value)} />
        <FilterInput label="Сложность" type="number" value={filters.difficulty} onChange={(value) => updateFilter('difficulty', value)} />
        <FilterSelect label="Среда обитания" value={filters.habitat} onChange={(value) => updateFilter('habitat', value)}>
          <option value="">Все среды</option>
          {terrainOptions.map((terrain) => <option key={terrain} value={terrain}>{TERRAIN_NAMES[terrain]}</option>)}
        </FilterSelect>
        <button
          type="button"
          onClick={() => setFilters(initialFilters)}
          className="self-end rounded-md border border-stone-400 px-3 py-2 text-sm font-semibold text-stone-700 hover:bg-stone-100"
        >
          Сбросить фильтры
        </button>
      </div>

      {herbsQuery.isLoading ? (
        <p className="py-10 text-center text-stone-600" role="status">Загрузка справочника трав…</p>
      ) : herbsQuery.isError ? (
        <p className="border-l-4 border-red-700 bg-red-50 p-4 text-red-900" role="alert">
          Не удалось загрузить список трав: {herbsQuery.error.message}
        </p>
      ) : sortedHerbs.length === 0 ? (
        <p className="border-l-4 border-amber-600 bg-amber-50 p-4 text-amber-900">Травы не найдены.</p>
      ) : (
        <div className="overflow-x-auto border border-stone-200">
          <table className="min-w-full divide-y divide-stone-200 text-sm">
            <thead className="bg-stone-800 text-xs uppercase tracking-wide text-white">
              <tr>
                {renderHeader('name', 'Название')}
                {renderHeader('description', 'Описание')}
                {renderHeader('herbType', 'Тип')}
                {renderHeader('rarity', 'Редкость')}
                {renderHeader('effect', 'Эффект')}
                {renderHeader('difficulty', 'Сложность')}
                {renderHeader('habitat', 'Среда обитания')}
              </tr>
            </thead>
            <tbody className="divide-y divide-stone-200 bg-white">
              {sortedHerbs.map((herb) => (
                <tr key={herb.id} className="align-top hover:bg-emerald-50/50">
                  <td className="px-4 py-3 font-semibold text-stone-900">{herb.name}</td>
                  <td className="max-w-sm px-4 py-3 text-stone-700">{herb.description}</td>
                  <td className="px-4 py-3 text-stone-700">{getHerbTypeText(herb)}</td>
                  <td className="whitespace-nowrap px-4 py-3 text-stone-700">{RARITY_NAMES[herb.rarity]}</td>
                  <td className="max-w-sm px-4 py-3 text-stone-700">{herb.effect}</td>
                  <td className="px-4 py-3 text-stone-700">{herb.difficulty}</td>
                  <td className="min-w-48 px-4 py-3 text-stone-700">{getHabitatText(herb)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </main>
  )
}

type FilterInputProps = {
  label: string
  type?: 'text' | 'number'
  value: string
  onChange: (value: string) => void
}

function FilterInput({ label, type = 'text', value, onChange }: FilterInputProps) {
  return (
    <label className="block text-sm font-semibold text-stone-700">
      {label}
      <input
        type={type}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="mt-2 min-h-10 w-full rounded-md border border-stone-300 bg-white px-3 font-normal text-stone-900 focus:border-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-700/20"
      />
    </label>
  )
}

type FilterSelectProps = {
  label: string
  value: string
  onChange: (value: string) => void
  children: ReactNode
}

function FilterSelect({ label, value, onChange, children }: FilterSelectProps) {
  return (
    <label className="block text-sm font-semibold text-stone-700">
      {label}
      <select
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="mt-2 min-h-10 w-full rounded-md border border-stone-300 bg-white px-3 font-normal text-stone-900 focus:border-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-700/20"
      >
        {children}
      </select>
    </label>
  )
}
