/** Страницы входа и регистрации: на них не возвращаем пользователя после входа. */
const AUTH_PATHS = ['/login', '/register']

/**
 * Возвращает путь, на который нужно вернуть пользователя после входа.
 * Берётся из location.state.from (сохраняется RequireAuth); по умолчанию — главная страница.
 */
export function getReturnPath(state: unknown): string {
  const from = (state as { from?: { pathname?: string; search?: string; hash?: string } } | null)?.from
  if (!from?.pathname || AUTH_PATHS.includes(from.pathname)) {
    return '/'
  }
  return `${from.pathname}${from.search ?? ''}${from.hash ?? ''}`
}
