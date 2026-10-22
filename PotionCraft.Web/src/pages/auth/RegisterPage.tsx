import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { register as registerAccount } from '../../api/auth'
import { ApiError, getErrorMessage } from '../../api/client'
import { registerFormSchema, type RegisterFormValues } from '../../auth/authSchemas'
import { AuthCard } from '../../components/AuthCard'

const defaultValues: RegisterFormValues = {
  userName: '',
  email: '',
  password: '',
  confirmPassword: '',
}

/** Страница регистрации нового пользователя (роль Player назначает сервер). */
export function RegisterPage() {
  const location = useLocation()
  const navigate = useNavigate()
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerFormSchema),
    defaultValues,
  })

  const registerMutation = useMutation({
    mutationFn: (values: RegisterFormValues) =>
      registerAccount({
        userName: values.userName,
        email: values.email,
        password: values.password,
      }),
    onSuccess: (response) => {
      const from = (location.state as { from?: unknown } | null)?.from
      navigate(`/confirm-email?email=${encodeURIComponent(response.email)}`, {
        replace: true,
        state: { from },
      })
    },
  })

  const submitError = registerMutation.error
    ? getErrorMessage(registerMutation.error, 'Не удалось зарегистрироваться. Попробуйте позже.')
    : null
  const validationErrors =
    registerMutation.error instanceof ApiError ? registerMutation.error.errors : []

  const onSubmit = handleSubmit((values) => {
    registerMutation.mutate(values)
  })

  const inputClass =
    'mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 shadow-sm focus:border-emerald-600 focus:outline-none'

  return (
    <AuthCard title="Регистрация">
      <form onSubmit={onSubmit} noValidate className="space-y-5">
        <div>
          <label htmlFor="userName" className="block text-sm font-medium text-slate-700">
            Имя пользователя
          </label>
          <input id="userName" type="text" autoComplete="username" className={inputClass} {...register('userName')} />
          {errors.userName && <p className="mt-1 text-sm text-rose-700">{errors.userName.message}</p>}
        </div>

        <div>
          <label htmlFor="email" className="block text-sm font-medium text-slate-700">
            Email
          </label>
          <input id="email" type="email" autoComplete="email" className={inputClass} {...register('email')} />
          {errors.email && <p className="mt-1 text-sm text-rose-700">{errors.email.message}</p>}
        </div>

        <div>
          <label htmlFor="password" className="block text-sm font-medium text-slate-700">
            Пароль
          </label>
          <input
            id="password"
            type="password"
            autoComplete="new-password"
            className={inputClass}
            {...register('password')}
          />
          {errors.password && <p className="mt-1 text-sm text-rose-700">{errors.password.message}</p>}
        </div>

        <div>
          <label htmlFor="confirmPassword" className="block text-sm font-medium text-slate-700">
            Подтверждение пароля
          </label>
          <input
            id="confirmPassword"
            type="password"
            autoComplete="new-password"
            className={inputClass}
            {...register('confirmPassword')}
          />
          {errors.confirmPassword && (
            <p className="mt-1 text-sm text-rose-700">{errors.confirmPassword.message}</p>
          )}
        </div>

        {submitError && (
          <div role="alert" className="rounded-md border border-rose-300 bg-rose-50 px-4 py-3 text-sm text-rose-800">
            <p>{submitError}</p>
            {validationErrors.length > 0 && (
              <ul className="mt-2 list-disc pl-5">
                {validationErrors.map((message) => (
                  <li key={message}>{message}</li>
                ))}
              </ul>
            )}
          </div>
        )}

        <button
          type="submit"
          disabled={registerMutation.isPending}
          className="w-full rounded-md bg-emerald-700 px-4 py-2 text-sm font-semibold text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-70"
        >
          {registerMutation.isPending ? 'Регистрируем...' : 'Зарегистрироваться'}
        </button>
      </form>

      <p className="mt-6 text-center text-sm text-slate-600">
        Уже есть аккаунт?{' '}
        <Link to="/login" state={location.state} className="font-semibold text-violet-700 hover:underline">
          Войти
        </Link>
      </p>
    </AuthCard>
  )
}
