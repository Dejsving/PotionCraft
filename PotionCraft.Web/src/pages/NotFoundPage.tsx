import { NavLink } from 'react-router-dom'

/** Страница 404 / catch-all маршрут — замена PotionCraft.Client/Pages/Error.cshtml для несуществующих путей. */
export function NotFoundPage() {
  return (
    <div className="mx-auto max-w-2xl py-12 text-center">
      <h1 className="mb-4 text-3xl font-bold text-slate-900">Страница не найдена</h1>
      <p className="mb-6 text-slate-600">Запрошенная страница не существует или была перемещена.</p>
      <NavLink to="/" className="text-violet-700 hover:underline">
        Вернуться на главную
      </NavLink>
    </div>
  )
}

