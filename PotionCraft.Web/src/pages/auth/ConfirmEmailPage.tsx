import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router-dom'
import { confirmEmail, resendConfirmationCode } from '../../api/auth'
import { getErrorMessage } from '../../api/client'
import { emailCodeFormSchema, type EmailCodeFormValues } from '../../auth/authSchemas'
import { applySignedInUser } from '../../auth/session'
import { getReturnPath } from '../../auth/returnPath'
import { getRemainingSeconds, RESEND_COOLDOWN_SECONDS } from '../../auth/resendCooldown'
import { AuthCard } from '../../components/AuthCard'

const RESEND_COOLDOWN_MS = RESEND_COOLDOWN_SECONDS * 1000

/** Страница подтверждения email: ввод 6-значного кода и повторная отправка с паузой. */
export function ConfirmEmailPage() {
  const [searchParams] = useSearchParams()
  const email = searchParams.get('email')?.trim() ?? ''

  return (
    <AuthCard title="Подтверждение email">
      {email ? (
        <ConfirmEmailForm email={email} />
      ) : (
        <div className="space-y-4 text-center text-sm text-slate-700">
          <p role="alert" className="rounded-md border border-rose-300 bg-rose-50 px-4 py-3 text-rose-800">
            Не указан email для подтверждения. Войдите или зарегистрируйтесь заново.
          </p>
          <Link to="/login" className="font-semibold text-violet-700 hover:underline">
            Перейти ко входу
          </Link>
        </div>
      )}
    </AuthCard>
  )
}

interface ConfirmEmailFormProps {
  email: string
}

/** Форма ввода кода подтверждения для указанного email. */
function ConfirmEmailForm({ email }: ConfirmEmailFormProps) {
  const location = useLocation()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const state = location.state as { from?: unknown; rememberMe?: unknown } | null
  const rememberMe = state?.rememberMe === true

  // Код только что отправлен сервером (при регистрации или неподтверждённом входе), поэтому пауза начинается сразу.
  const [now, setNow] = useState(() => Date.now())
  const [cooldownEndsAt, setCooldownEndsAt] = useState(() => Date.now() + RESEND_COOLDOWN_MS)
  const [resendInfo, setResendInfo] = useState<string | null>(null)
  const remainingSeconds = getRemainingSeconds(cooldownEndsAt, now)
  const isCoolingDown = remainingSeconds > 0

  useEffect(() => {
    if (!isCoolingDown) {
      return
    }
    const timerId = window.setInterval(() => setNow(Date.now()), 1000)
    return () => window.clearInterval(timerId)
  }, [isCoolingDown])

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<EmailCodeFormValues>({
    resolver: zodResolver(emailCodeFormSchema),
    defaultValues: { code: '' },
  })

  const confirmMutation = useMutation({
    mutationFn: (values: EmailCodeFormValues) => confirmEmail({ email, code: values.code, rememberMe }),
    onSuccess: (user) => {
      applySignedInUser(queryClient, user)
      navigate(getReturnPath(location.state), { replace: true })
    },
  })

  const resendMutation = useMutation({
    mutationFn: () => resendConfirmationCode(email),
    onMutate: () => setResendInfo(null),
    onSuccess: () => {
      const current = Date.now()
      setNow(current)
      setCooldownEndsAt(current + RESEND_COOLDOWN_MS)
      setResendInfo('Новый код отправлен.')
    },
  })

  const submitError = confirmMutation.error
    ? getErrorMessage(confirmMutation.error, 'Не удалось подтвердить код. Попробуйте позже.')
    : null
  const resendError = resendMutation.error
    ? getErrorMessage(resendMutation.error, 'Не удалось отправить код. Попробуйте позже.')
    : null

  const onSubmit = handleSubmit((values) => {
    confirmMutation.mutate(values)
  })

  return (
    <>
      <p className="mb-6 text-center text-sm text-slate-700">
        Мы отправили 6-значный код на <b>{email}</b>. Код действует 10 минут.
      </p>

      {import.meta.env.DEV && (
        <p className="mb-6 text-center text-xs text-slate-500">
          Режим разработки: письмо не отправляется, код выводится в консоль сервера.
        </p>
      )}

      <form onSubmit={onSubmit} noValidate className="space-y-5">
        <div>
          <label htmlFor="code" className="block text-sm font-medium text-slate-700">
            Код подтверждения
          </label>
          <input
            id="code"
            type="text"
            inputMode="numeric"
            autoComplete="one-time-code"
            maxLength={6}
            autoFocus
            className="mt-1 block w-full rounded-md border border-slate-300 px-3 py-2 text-center text-2xl tracking-[0.5em] shadow-sm focus:border-emerald-600 focus:outline-none"
            {...register('code')}
          />
          {errors.code && <p className="mt-1 text-sm text-rose-700">{errors.code.message}</p>}
        </div>

        {submitError && (
          <p role="alert" className="rounded-md border border-rose-300 bg-rose-50 px-4 py-3 text-sm text-rose-800">
            {submitError}
          </p>
        )}

        <button
          type="submit"
          disabled={confirmMutation.isPending}
          className="w-full rounded-md bg-emerald-700 px-4 py-2 text-sm font-semibold text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:opacity-70"
        >
          {confirmMutation.isPending ? 'Проверяем...' : 'Подтвердить'}
        </button>
      </form>

      <div className="mt-6 space-y-3 text-center text-sm">
        <button
          type="button"
          onClick={() => resendMutation.mutate()}
          disabled={isCoolingDown || resendMutation.isPending}
          className="font-semibold text-violet-700 hover:underline disabled:cursor-not-allowed disabled:text-slate-400 disabled:no-underline"
        >
          {isCoolingDown ? `Отправить повторно через ${remainingSeconds} с` : 'Отправить код повторно'}
        </button>
        {resendInfo && <p className="text-emerald-700">{resendInfo}</p>}
        {resendError && (
          <p role="alert" className="text-rose-700">
            {resendError}
          </p>
        )}
        <p>
          <Link to="/login" className="font-semibold text-violet-700 hover:underline">
            Вернуться ко входу
          </Link>
        </p>
      </div>
    </>
  )
}
