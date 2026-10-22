import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { Controller, useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { createCharacter } from '../../api/characters'
import { ApiError } from '../../api/client'

/** Уровень владения инструментом: 0 — нет, 1 — владение, 2 — мастерство. */
const proficiencyLevelSchema = z.union([z.literal(0), z.literal(1), z.literal(2)])

const createCharacterSchema = z.object({
  name: z.string().trim().min(1, 'Необходимо ввести имя персонажа.').max(100, 'Имя слишком длинное.'),
  intelligence: z
    .number()
    .int()
    .min(0, 'Значение интеллекта должно быть от 0 до 100.')
    .max(100, 'Значение интеллекта должно быть от 0 до 100.'),
  wisdom: z
    .number()
    .int()
    .min(0, 'Значение мудрости должно быть от 0 до 100.')
    .max(100, 'Значение мудрости должно быть от 0 до 100.'),
  proficiencyBonus: z
    .number()
    .int()
    .min(2, 'Бонус мастерства должен быть от 2 до 6.')
    .max(6, 'Бонус мастерства должен быть от 2 до 6.'),
  herbalismProficiencyLevel: proficiencyLevelSchema,
  herbalismToolModifier: z
    .number()
    .int()
    .min(-3, 'Модификатор набора травника должен быть от -3 до 3.')
    .max(3, 'Модификатор набора травника должен быть от -3 до 3.'),
  alchemistProficiencyLevel: proficiencyLevelSchema,
  alchemistToolModifier: z
    .number()
    .int()
    .min(-3, 'Модификатор инструментов алхимика должен быть от -3 до 3.')
    .max(3, 'Модификатор инструментов алхимика должен быть от -3 до 3.'),
  poisonerProficiencyLevel: proficiencyLevelSchema,
  poisonerToolModifier: z
    .number()
    .int()
    .min(-3, 'Модификатор инструментов отравителя должен быть от -3 до 3.')
    .max(3, 'Модификатор инструментов отравителя должен быть от -3 до 3.'),
})

type CreateCharacterFormValues = z.infer<typeof createCharacterSchema>

const defaultValues: CreateCharacterFormValues = {
  name: '',
  intelligence: 10,
  wisdom: 10,
  proficiencyBonus: 2,
  herbalismProficiencyLevel: 0,
  herbalismToolModifier: 0,
  alchemistProficiencyLevel: 0,
  alchemistToolModifier: 0,
  poisonerProficiencyLevel: 0,
  poisonerToolModifier: 0,
}

interface ToolFieldConfig {
  title: string
  levelField: 'herbalismProficiencyLevel' | 'alchemistProficiencyLevel' | 'poisonerProficiencyLevel'
  modifierField: 'herbalismToolModifier' | 'alchemistToolModifier' | 'poisonerToolModifier'
}

const toolFields: ToolFieldConfig[] = [
  { title: 'Травник', levelField: 'herbalismProficiencyLevel', modifierField: 'herbalismToolModifier' },
  { title: 'Алхимик', levelField: 'alchemistProficiencyLevel', modifierField: 'alchemistToolModifier' },
  { title: 'Отравитель', levelField: 'poisonerProficiencyLevel', modifierField: 'poisonerToolModifier' },
]

/** Страница создания персонажа с формой характеристик и владения инструментами. */
export function CreateCharacterPage() {
  const navigate = useNavigate()
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateCharacterFormValues>({
    resolver: zodResolver(createCharacterSchema),
    defaultValues,
  })

  const createMutation = useMutation({
    mutationFn: createCharacter,
    onSuccess: () => {
      navigate('/characters/select')
    },
  })

  const submitError = createMutation.error
    ? createMutation.error instanceof ApiError
      ? createMutation.error.message
      : 'Ошибка при создании персонажа.'
    : null

  const onSubmit = handleSubmit((values) => {
    createMutation.mutate(values)
  })

  return (
    <section className="mx-auto max-w-2xl">
      <h1 className="mb-6 text-2xl font-bold text-slate-900">Создание персонажа</h1>

      <form onSubmit={onSubmit} noValidate className="space-y-6">
        <div>
          <label htmlFor="name" className="block text-sm font-medium text-slate-700">
            Имя
          </label>
          <input
            id="name"
            type="text"
            className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 shadow-sm focus:border-emerald-600 focus:outline-none"
            {...register('name')}
          />
          {errors.name && <p className="mt-1 text-sm text-rose-700">{errors.name.message}</p>}
        </div>

        <div className="grid gap-4 sm:grid-cols-3">
          <div>
            <label htmlFor="intelligence" className="block text-sm font-medium text-slate-700">
              Интеллект
            </label>
            <input
              id="intelligence"
              type="number"
              min={0}
              max={100}
              className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 shadow-sm focus:border-emerald-600 focus:outline-none"
              {...register('intelligence', { valueAsNumber: true })}
            />
            {errors.intelligence && <p className="mt-1 text-sm text-rose-700">{errors.intelligence.message}</p>}
          </div>

          <div>
            <label htmlFor="wisdom" className="block text-sm font-medium text-slate-700">
              Мудрость
            </label>
            <input
              id="wisdom"
              type="number"
              min={0}
              max={100}
              className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 shadow-sm focus:border-emerald-600 focus:outline-none"
              {...register('wisdom', { valueAsNumber: true })}
            />
            {errors.wisdom && <p className="mt-1 text-sm text-rose-700">{errors.wisdom.message}</p>}
          </div>

          <div>
            <label htmlFor="proficiencyBonus" className="block text-sm font-medium text-slate-700">
              Бонус мастерства
            </label>
            <input
              id="proficiencyBonus"
              type="number"
              min={2}
              max={6}
              className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 shadow-sm focus:border-emerald-600 focus:outline-none"
              {...register('proficiencyBonus', { valueAsNumber: true })}
            />
            {errors.proficiencyBonus && (
              <p className="mt-1 text-sm text-rose-700">{errors.proficiencyBonus.message}</p>
            )}
          </div>
        </div>

        <div className="space-y-4">
          {toolFields.map((tool) => (
            <fieldset key={tool.title} className="rounded-md border border-slate-300 p-4">
              <legend className="px-1 text-sm font-semibold text-slate-800">{tool.title}</legend>

              <Controller
                control={control}
                name={tool.levelField}
                render={({ field }) => (
                  <div className="flex flex-wrap gap-4">
                    {(
                      [
                        [0, 'Нет'],
                        [1, 'Владение'],
                        [2, 'Мастерство'],
                      ] as const
                    ).map(([value, label]) => (
                      <label key={value} className="flex items-center gap-2 text-sm text-slate-700">
                        <input
                          type="radio"
                          name={field.name}
                          checked={field.value === value}
                          onChange={() => field.onChange(value)}
                          className="h-4 w-4"
                        />
                        {label}
                      </label>
                    ))}
                  </div>
                )}
              />

              <div className="mt-3 max-w-[10rem]">
                <label htmlFor={tool.modifierField} className="block text-sm font-medium text-slate-700">
                  Модификатор инструмента
                </label>
                <input
                  id={tool.modifierField}
                  type="number"
                  min={-3}
                  max={3}
                  className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 shadow-sm focus:border-emerald-600 focus:outline-none"
                  {...register(tool.modifierField, { valueAsNumber: true })}
                />
                {errors[tool.modifierField] && (
                  <p className="mt-1 text-sm text-rose-700">{errors[tool.modifierField]?.message}</p>
                )}
              </div>
            </fieldset>
          ))}
        </div>

        {submitError && (
          <p role="alert" className="rounded-md border border-rose-300 bg-rose-50 px-4 py-3 text-sm text-rose-800">
            {submitError}
          </p>
        )}

        <button
          type="submit"
          disabled={createMutation.isPending}
          className="w-full rounded-md bg-emerald-700 px-4 py-2 text-sm font-semibold text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-70"
        >
          {createMutation.isPending ? 'Создаём...' : 'Создать персонажа'}
        </button>
      </form>
    </section>
  )
}
