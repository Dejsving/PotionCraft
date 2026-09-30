import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { ApiError } from '../../api/client'
import { getCharacters, selectCharacter } from '../../api/characters'
import { useSessionStore } from '../../store/sessionStore'
import type { PlayerCharacter } from '../../types/models'

/** Страница списка и выбора персонажей. */
export function SelectCharacterPage() {
  const queryClient = useQueryClient()
  const playerId = useSessionStore((state) => state.playerId)
  const activeCharacter = useSessionStore((state) => state.activeCharacter)
  const setActiveCharacter = useSessionStore((state) => state.setActiveCharacter)
  const charactersQuery = useQuery({
    queryKey: ['characters'],
    queryFn: getCharacters,
  })
  const selectionMutation = useMutation({
    mutationFn: (characterId: string) => selectCharacter(characterId, { playerId }),
    onSuccess: async (character) => {
      setActiveCharacter(character)
      await queryClient.invalidateQueries({ queryKey: ['characters'] })
    },
  })

  const selectionError = selectionMutation.error
    ? selectionMutation.error instanceof ApiError && selectionMutation.error.status === 409
      ? 'Персонаж уже занят.'
      : `Не удалось выбрать персонажа. ${selectionMutation.error.message}`
    : null

  return (
    <section>
      <div className="mb-6 flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Выбор персонажа</h1>
          <p className="mt-1 text-sm text-slate-600">
            Выберите персонажа для сбора трав, алхимии и торговли.
          </p>
        </div>
        <Link
          to="/characters/create"
          className="rounded-md bg-emerald-700 px-4 py-2 text-sm font-semibold text-white hover:bg-emerald-800"
        >
          Создать персонажа
        </Link>
      </div>

      {selectionError && (
        <p role="alert" className="mb-4 rounded-md border border-rose-300 bg-rose-50 px-4 py-3 text-sm text-rose-800">
          {selectionError}
        </p>
      )}
      {selectionMutation.isSuccess && (
        <p role="status" className="mb-4 rounded-md border border-emerald-300 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
          Персонаж успешно выбран.
        </p>
      )}

      {charactersQuery.isPending && (
        <p role="status" className="py-8 text-center text-slate-600">Загрузка персонажей...</p>
      )}
      {charactersQuery.isError && (
        <div role="alert" className="rounded-md border border-rose-300 bg-rose-50 p-4 text-rose-800">
          <p>Не удалось загрузить список персонажей. {charactersQuery.error.message}</p>
          <button
            type="button"
            className="mt-3 font-semibold underline underline-offset-2"
            onClick={() => void charactersQuery.refetch()}
          >
            Повторить
          </button>
        </div>
      )}
      {charactersQuery.data?.length === 0 && (
        <p className="border-y border-slate-200 py-8 text-center text-slate-600">
          Персонажей пока нет. Создайте первого персонажа.
        </p>
      )}

      {charactersQuery.data && charactersQuery.data.length > 0 && (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {charactersQuery.data.map((character) => (
            <CharacterCard
              key={character.id}
              character={character}
              playerId={playerId}
              activeCharacter={activeCharacter}
              isSelecting={selectionMutation.isPending && selectionMutation.variables === character.id}
              isSelectionPending={selectionMutation.isPending}
              onSelect={() => {
                selectionMutation.reset()
                selectionMutation.mutate(character.id)
              }}
            />
          ))}
        </div>
      )}
    </section>
  )
}

interface CharacterCardProps {
  character: PlayerCharacter
  playerId: string
  activeCharacter: PlayerCharacter | null
  isSelecting: boolean
  isSelectionPending: boolean
  onSelect: () => void
}

function CharacterCard({
  character,
  playerId,
  activeCharacter,
  isSelecting,
  isSelectionPending,
  onSelect,
}: CharacterCardProps) {
  const isSelectedByCurrentPlayer =
    character.selectedBy?.toLowerCase() === playerId.toLowerCase() ||
    activeCharacter?.id === character.id
  const isTakenByOtherPlayer = Boolean(character.selectedBy) && !isSelectedByCurrentPlayer
  const cardStatusClass = isSelectedByCurrentPlayer
    ? 'border-blue-500 bg-blue-50'
    : isTakenByOtherPlayer
      ? 'border-slate-400 bg-slate-100 text-slate-600'
      : 'border-emerald-600 bg-white'

  return (
    <article className={`flex min-h-56 flex-col rounded-md border-2 p-4 shadow-sm ${cardStatusClass}`}>
      <div className="mb-4 flex items-start justify-between gap-3">
        <h2 className="text-lg font-semibold text-slate-900">
          {character.name}
          {isSelectedByCurrentPlayer && <span className="ml-2 text-sm font-medium text-blue-700">(Ваш)</span>}
        </h2>
        {isTakenByOtherPlayer && (
          <span className="shrink-0 rounded bg-slate-200 px-2 py-1 text-xs font-semibold text-slate-700">
            Занят
          </span>
        )}
      </div>

      <div className="grid flex-1 grid-cols-[1fr_auto] gap-x-4 gap-y-2 text-sm">
        <span>Интеллект</span>
        <span className="text-right font-medium">
          {character.intelligence} ({character.intelligenceModifier >= 0 ? '+' : ''}{character.intelligenceModifier})
        </span>
        <span>Мудрость</span>
        <span className="text-right font-medium">
          {character.wisdom} ({character.wisdomModifier >= 0 ? '+' : ''}{character.wisdomModifier})
        </span>
        <span>Бонус мастерства</span>
        <span className="text-right font-medium">+{character.proficiencyBonus}</span>
        <span>Алхимик</span>
        <span className="text-right font-medium">{formatModifier(character.alchemistModify)}</span>
        <span>Травник</span>
        <span className="text-right font-medium">{formatModifier(character.herbalismModify)}</span>
        <span>Отравитель</span>
        <span className="text-right font-medium">{formatModifier(character.poisonerModify)}</span>
      </div>

      <button
        type="button"
        className={`mt-5 w-full rounded-md px-3 py-2 text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-70 ${
          isSelectedByCurrentPlayer
            ? 'bg-blue-700'
            : isTakenByOtherPlayer
              ? 'bg-slate-400'
              : 'bg-emerald-700 hover:bg-emerald-800'
        }`}
        disabled={isSelectedByCurrentPlayer || isTakenByOtherPlayer || isSelectionPending}
        onClick={onSelect}
      >
        {isSelecting ? 'Выбираем...' : isSelectedByCurrentPlayer ? 'Выбран' : isTakenByOtherPlayer ? 'Занят' : 'Выбрать'}
      </button>
    </article>
  )
}

function formatModifier(value: number): string {
  return value >= 0 ? `+${value}` : String(value)
}
