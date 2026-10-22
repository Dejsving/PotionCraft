import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { getReturnPath } from '../auth/returnPath'
import { getRoleLabel, hasRole } from '../auth/roles'
import { useCurrentUser } from '../hooks/useCurrentUser'
import type { UserRole } from '../types/models'

/** Индикатор проверки сессии на время загрузки текущего пользователя. */
function SessionLoading() {
  return (
    <p role="status" className="py-12 text-center text-slate-600">
      Проверка сессии...
    </p>
  )
}

/** Пропускает только авторизованных пользователей; остальных отправляет на /login, запомнив исходный путь. */
export function RequireAuth() {
  const location = useLocation()
  const { data: user, isPending, isError, refetch } = useCurrentUser()

  if (isPending) {
    return <SessionLoading />
  }

  if (isError) {
    return (
      <div role="alert" className="rounded-md border border-rose-300 bg-rose-50 p-4 text-rose-800">
        <p>Не удалось проверить сессию.</p>
        <button
          type="button"
          className="mt-3 font-semibold underline underline-offset-2"
          onClick={() => void refetch()}
        >
          Повторить
        </button>
      </div>
    )
  }

  if (!user) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  return <Outlet />
}

/** Пропускает только пользователей с указанной ролью; остальным показывает сообщение об отсутствии доступа. */
export function RequireRole({ role }: { role: UserRole }) {
  const location = useLocation()
  const { data: user, isPending } = useCurrentUser()

  if (isPending) {
    return <SessionLoading />
  }

  if (!user) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  if (!hasRole(user, role)) {
    return (
      <section className="mx-auto max-w-xl py-12 text-center">
        <h1 className="mb-4 text-3xl font-bold text-slate-900">Нет доступа</h1>
        <p className="text-slate-600">
          Эта страница доступна только для роли «{getRoleLabel(role)}».
        </p>
      </section>
    )
  }

  return <Outlet />
}

/** Для страниц входа и регистрации: авторизованного пользователя перенаправляет на главную или на исходный путь. */
export function RedirectIfAuthenticated() {
  const location = useLocation()
  const { data: user, isPending } = useCurrentUser()

  if (isPending) {
    return <SessionLoading />
  }

  if (user) {
    return <Navigate to={getReturnPath(location.state)} replace />
  }

  return <Outlet />
}
