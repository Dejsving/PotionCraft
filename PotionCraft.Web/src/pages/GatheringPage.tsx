import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { getHerbs } from '../api/herbs'
import { gatherHerbs } from '../api/gathering'
import { TERRAIN_NAMES, TerrainEnum } from '../types/enums'
import type { GatheringApiRequest, GatheringApiResponse } from '../types/models'
import { useSessionStore } from '../store/sessionStore'

type BooleanChoiceProps = {
  legend: string
  name: string
  value: boolean
  options: [boolean, string][]
  onChange: (value: boolean) => void
}

/** Группа радиокнопок с единым оформлением для параметров сбора. */
function BooleanChoice({ legend, name, value, options, onChange }: BooleanChoiceProps) {
  return (
    <fieldset>
      <legend className="mb-2 text-sm font-semibold text-stone-700">{legend}</legend>
      <div className="flex flex-wrap gap-2">
        {options.map(([optionValue, label]) => (
          <label key={label} className="cursor-pointer">
            <input
              className="peer sr-only"
              type="radio"
              name={name}
              value={String(optionValue)}
              checked={value === optionValue}
              onChange={() => onChange(optionValue)}
            />
            <span className="inline-flex min-h-10 items-center rounded-md border border-stone-300 bg-white px-3 text-sm text-stone-700 transition peer-checked:border-emerald-800 peer-checked:bg-emerald-800 peer-checked:text-white peer-focus-visible:outline-2 peer-focus-visible:outline-offset-2 peer-focus-visible:outline-emerald-700">
              {label}
            </span>
          </label>
        ))}
      </div>
    </fieldset>
  )
}

/** Страница настройки и разрешения бросков на сбор трав. */
export function GatheringPage() {
  const activeCharacter = useSessionStore((state) => state.activeCharacter)
  const [isDay, setIsDay] = useState(true)
  const [isRaining, setIsRaining] = useState(false)
  const [isCave, setIsCave] = useState(false)
  const [hasProvisions, setHasProvisions] = useState(true)
  const [difficulty, setDifficulty] = useState(20)
  const [rollsCount, setRollsCount] = useState(1)
  const [terrain, setTerrain] = useState(TerrainEnum.Everewhere)
  const [copyState, setCopyState] = useState<'idle' | 'copied' | 'error'>('idle')

  const herbsQuery = useQuery({
    queryKey: ['herbs'],
    queryFn: getHerbs,
    enabled: Boolean(activeCharacter),
  })
  const gatheringMutation = useMutation({
    mutationFn: (request: GatheringApiRequest) =>
      gatherHerbs(activeCharacter!.id, request),
  })

  const terrainOptions = Object.values(TerrainEnum).filter(
    (value): value is TerrainEnum => typeof value === 'number'
  )
  const result = gatheringMutation.data
  const herbNames = new Map((herbsQuery.data ?? []).map((herb) => [herb.id.toLowerCase(), herb.name]))
  const gatheredHerbs = Object.entries(result?.gatheredHerbs ?? {})
    .map(([herbId, gatheringResult]) => ({
      herbId,
      name:
        herbNames.get(herbId.toLowerCase()) ??
        gatheringResult.herb?.name ??
        `Трава ${herbId}`,
      quantity: gatheringResult.quantity,
      rarity: gatheringResult.herb?.rarity ?? 0,
    }))
    .sort((left, right) => left.rarity - right.rarity || left.name.localeCompare(right.name, 'ru'))

  /** Формирует краткую текстовую сводку для копирования в заметки игры. */
  function getCopyText(gatheringResult: GatheringApiResponse): string {
    const gathered = Object.entries(gatheringResult.gatheredHerbs)
      .map(([herbId, item]) => {
        const name =
          herbNames.get(herbId.toLowerCase()) ?? item.herb?.name ?? `Трава ${herbId}`
        return `- ${name} x ${item.quantity}`
      })
      .join('\n')

    return [
      `Сбор трав: ${gatheringResult.characterName}`,
      `Успехи: ${gatheringResult.totalSuccesses} из ${gatheringResult.rollResults.length}`,
      `Броски: ${gatheringResult.rollResults.join(', ') || 'нет'}`,
      'Собранные травы:',
      gathered || '- Ничего не найдено.',
    ].join('\n')
  }

  /** Копирует сводку результата в системный буфер обмена. */
  async function copyResult() {
    if (!result || !navigator.clipboard) {
      setCopyState('error')
      return
    }

    try {
      await navigator.clipboard.writeText(getCopyText(result))
      setCopyState('copied')
    } catch {
      setCopyState('error')
    }
  }

  if (!activeCharacter) {
    return (
      <main className="mx-auto max-w-4xl px-4 py-10">
        <h1 className="text-3xl font-semibold text-stone-900">Сбор трав</h1>
        <section className="mt-6 border-l-4 border-amber-600 bg-amber-50 p-5 text-stone-800" role="status">
          <p className="font-semibold">Сначала выберите персонажа.</p>
          <p className="mt-1 text-sm">Для броска на сбор трав нужен активный персонаж.</p>
          <Link
            to="/characters/select"
            className="mt-4 inline-flex min-h-10 items-center rounded-md bg-emerald-800 px-4 text-sm font-semibold text-white hover:bg-emerald-900"
          >
            Выбрать персонажа
          </Link>
        </section>
      </main>
    )
  }

  function submitGathering(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setCopyState('idle')
    gatheringMutation.mutate({
      terrain,
      isRaining,
      isNight: !isDay,
      isCave,
      hasProvisions,
      rollsCount,
      difficulty,
    })
  }

  return (
    <main className="mx-auto max-w-5xl px-4 py-8 sm:px-6">
      <header className="mb-7 border-b border-stone-300 pb-5">
        <p className="text-sm font-semibold uppercase text-emerald-800">Экспедиция</p>
        <h1 className="mt-1 text-3xl font-semibold text-stone-900">Сбор трав</h1>
        <p className="mt-2 text-stone-600">Персонаж: {activeCharacter.name}</p>
      </header>

      <form onSubmit={submitGathering} className="space-y-6">
        <section className="grid gap-6 border-b border-stone-200 pb-6 sm:grid-cols-2 xl:grid-cols-4">
          <BooleanChoice
            legend="Время суток"
            name="timeOfDay"
            value={isDay}
            options={[[true, 'День'], [false, 'Ночь']]}
            onChange={setIsDay}
          />
          <BooleanChoice
            legend="Погода"
            name="weather"
            value={isRaining}
            options={[[false, 'Не дождь'], [true, 'Дождь']]}
            onChange={setIsRaining}
          />
          <BooleanChoice
            legend="Местоположение"
            name="location"
            value={isCave}
            options={[[false, 'Не пещера'], [true, 'Пещера']]}
            onChange={setIsCave}
          />
          <BooleanChoice
            legend="Использование провизии"
            name="provisions"
            value={hasProvisions}
            options={[[true, 'Да'], [false, 'Нет']]}
            onChange={setHasProvisions}
          />
        </section>

        <section className="grid gap-5 sm:grid-cols-3">
          <label className="block text-sm font-semibold text-stone-700">
            Сложность поиска
            <input
              className="mt-2 block min-h-11 w-full rounded-md border border-stone-300 bg-white px-3 text-base font-normal text-stone-900 focus:border-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-700/20"
              type="number"
              min={1}
              max={30}
              required
              value={difficulty}
              onChange={(event) => setDifficulty(Number(event.target.value))}
            />
          </label>
          <label className="block text-sm font-semibold text-stone-700">
            Количество бросков
            <input
              className="mt-2 block min-h-11 w-full rounded-md border border-stone-300 bg-white px-3 text-base font-normal text-stone-900 focus:border-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-700/20"
              type="number"
              min={1}
              required
              value={rollsCount}
              onChange={(event) => setRollsCount(Number(event.target.value))}
            />
          </label>
          <label className="block text-sm font-semibold text-stone-700">
            Среда обитания
            <select
              className="mt-2 block min-h-11 w-full rounded-md border border-stone-300 bg-white px-3 text-base font-normal text-stone-900 focus:border-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-700/20"
              value={terrain}
              onChange={(event) => setTerrain(Number(event.target.value) as TerrainEnum)}
            >
              {terrainOptions.map((option) => (
                <option key={option} value={option}>
                  {TERRAIN_NAMES[option]}
                </option>
              ))}
            </select>
          </label>
        </section>

        {gatheringMutation.isError && (
          <p className="border-l-4 border-red-700 bg-red-50 p-3 text-sm text-red-900" role="alert">
            Ошибка при обращении к серверу: {gatheringMutation.error.message}
          </p>
        )}

        <button
          type="submit"
          disabled={gatheringMutation.isPending}
          className="flex min-h-12 w-full items-center justify-center rounded-md bg-emerald-800 px-5 text-base font-semibold text-white transition hover:bg-emerald-900 disabled:cursor-wait disabled:opacity-60"
        >
          {gatheringMutation.isPending ? 'Выполняется бросок…' : 'Бросок на сбор трав'}
        </button>
      </form>

      {result ? (
        <section className="mt-10 border-t-2 border-emerald-800 pt-6" aria-live="polite">
          <div className="flex flex-wrap items-start justify-between gap-4">
            <div>
              <p className="text-sm font-semibold uppercase text-emerald-800">Результат экспедиции</p>
              <h2 className="mt-1 text-2xl font-semibold text-stone-900">{result.characterName}</h2>
            </div>
            <button
              type="button"
              onClick={() => void copyResult()}
              className="inline-flex min-h-10 items-center gap-2 rounded-md border border-stone-400 bg-white px-3 text-sm font-semibold text-stone-800 hover:bg-stone-100 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-700"
              title="Копировать результат"
            >
              <span aria-hidden="true">▢</span>
              {copyState === 'copied' ? 'Скопировано' : 'Копировать результат'}
            </button>
          </div>
          {copyState === 'error' && (
            <p className="mt-3 text-sm text-red-800" role="alert">
              Не удалось скопировать: браузер не разрешил доступ к буферу обмена.
            </p>
          )}

          <div className="mt-6 grid gap-8 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.4fr)]">
            <section>
              <div className="flex items-baseline gap-2">
                <span className="text-4xl font-semibold tabular-nums text-emerald-800">
                  {result.totalSuccesses}
                </span>
                <span className="text-stone-600">успехов из {result.rollResults.length}</span>
              </div>
              <h3 className="mt-5 text-sm font-semibold text-stone-800">Результаты бросков</h3>
              <p className="mt-1 text-sm text-stone-600">
                Значения включают модификатор Гербализма. Сложность: {difficulty}.
              </p>
              <div className="mt-3 flex flex-wrap gap-2">
                {result.rollResults.map((roll, index) => (
                  <span
                    key={`${index}-${roll}`}
                    className={`inline-flex min-h-10 min-w-10 items-center justify-center rounded-md px-3 font-semibold tabular-nums text-white ${roll >= difficulty ? 'bg-emerald-700' : 'bg-stone-500'}`}
                    aria-label={`Бросок ${index + 1}: ${roll}, ${roll >= difficulty ? 'успех' : 'неудача'}`}
                  >
                    {roll}
                  </span>
                ))}
              </div>
            </section>

            <section>
              <h3 className="text-sm font-semibold text-stone-800">Собранные травы</h3>
              {herbsQuery.isLoading && (
                <p className="mt-3 text-sm text-stone-600">Загружаются названия трав…</p>
              )}
              {herbsQuery.isError && (
                <p className="mt-3 text-sm text-amber-800" role="status">
                  Справочник трав недоступен; показаны названия из результата сбора, если они есть.
                </p>
              )}
              {gatheredHerbs.length ? (
                <div className="mt-3 overflow-x-auto border-y border-stone-300">
                  <table className="w-full text-left text-sm">
                    <thead className="bg-stone-100 text-xs uppercase text-stone-600">
                      <tr>
                        <th scope="col" className="px-3 py-2 font-semibold">Трава</th>
                        <th scope="col" className="px-3 py-2 text-right font-semibold">Количество</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-stone-200">
                      {gatheredHerbs.map((herb) => (
                        <tr key={herb.herbId}>
                          <th scope="row" className="px-3 py-3 font-medium text-stone-800">
                            {herb.name}
                          </th>
                          <td className="px-3 py-3 text-right font-semibold tabular-nums text-emerald-800">
                            x {herb.quantity}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <p className="mt-3 border-y border-stone-200 py-4 text-sm text-stone-600">
                  Ничего не найдено.
                </p>
              )}
            </section>
          </div>
        </section>
      ) : !gatheringMutation.isPending ? (
        <p className="mt-8 border-t border-stone-200 pt-5 text-center text-sm text-stone-500">
          Настройте параметры и выполните бросок, чтобы увидеть результат сбора.
        </p>
      ) : null}
    </main>
  )
}
