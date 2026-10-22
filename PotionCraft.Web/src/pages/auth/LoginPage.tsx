import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { login } from '../../api/auth'
import { getErrorMessage } from '../../api/client'
import { applySignedInUser } from '../../auth/session'
import { getReturnPath } from '../../auth/returnPath'
import { isEmailConfirmationRequired } from '../../auth/emailConfirmation'
import { loginFormSchema, type LoginFormValues } from '../../auth/authSchemas'
import { AuthCard } from '../../components/AuthCard'

const defaultValues: LoginFormValues = {
  login: '',
  password: '',
  rememberMe: false,
}

/** Страница входа по имени пользователя или email. */
export function LoginPage() {
  const location = useLocation()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginFormSchema),
    defaultValues,
  })

  const loginMutation = useMutation({
    mutationFn: login,
    onSuccess: (user) => {
      applySignedInUser(queryClient, user)
      navigate(getReturnPath(location.state), { replace: true })
    },
    onError: (error, variables) => {
      if (isEmailConfirmationRequired(error)) {
        const from = (location.state as { from?: unknown } | null)?.from
        navigate(`/confirm-email?email=${encodeURIComponent(error.body.email)}`, {
          replace: true,
          state: { from, rememberMe: variables.rememberMe },
        })
      }
    },
  })

  const submitError = loginMutation.error
    ? getErrorMessage(loginMutation.error, 'Не удалось войти. Попробуйте позже.')
    : null

  const onSubmit = handleSubmit((values) => {
    loginMutation.mutate(values)
  })

  return (
    <AuthCard title="Вход">
      <form onSubmit={onSubmit} noValidate className="space-y-5">
        <div>
          <label htmlFor="login" className="block text-sm font-medium text-slate-700">
            Имя пользователя или email
          </label>
          <input
            id="login"
            type="text"
            autoComplete="username"
            className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 shadow-sm focus:border-emerald-600 focus:outline-none"
            {...register('login')}
          />
          {errors.login && <p className="mt-1 text-sm text-rose-700">{errors.login.message}</p>}
        </div>

        <div>
          <label htmlFor="password" className="block text-sm font-medium text-slate-700">
            Пароль
          </label>
          <input
            id="password"
            type="password"
            autoComplete="current-password"
            className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 shadow-sm focus:border-emerald-600 focus:outline-none"
            {...register('password')}
          />
          {errors.password && <p className="mt-1 text-sm text-rose-700">{errors.password.message}</p>}
        </div>

        <label className="flex items-center gap-2 text-sm text-slate-700">
          <input type="checkbox" className="h-4 w-4" {...register('rememberMe')} />
          Запомнить меня
        </label>

        {submitError && (
          <p role="alert" className="rounded-md border border-rose-300 bg-rose-50 px-4 py-3 text-sm text-rose-800">
            {submitError}
          </p>
        )}

        <button
          type="submit"
          disabled={loginMutation.isPending}
          className="w-full rounded-md bg-emerald-700 px-4 py-2 text-sm font-semibold text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-70"
        >
          {loginMutation.isPending ? 'Входим...' : 'Войти'}
        </button>
      </form>

      <p className="mt-6 text-center text-sm text-slate-600">
        Нет аккаунта?{' '}
        <Link to="/register" state={location.state} className="font-semibold text-violet-700 hover:underline">
          Зарегистрироваться
        </Link>
      </p>
    </AuthCard>
  )
}
