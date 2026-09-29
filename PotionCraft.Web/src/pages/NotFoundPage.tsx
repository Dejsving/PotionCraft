import { NavLink } from 'react-router-dom'

/** Страница 404 / catch-all маршрут. */
export function NotFoundPage() {
  return (
    <div className="text-center">
      <h1 className="mb-4 text-3xl font-bold text-slate-900">Страница не найдена</h1>
      <NavLink to="/" className="text-violet-700 hover:underline">
        Вернуться на главную
      </NavLink>
    </div>
  )
}
