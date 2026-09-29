import { Link } from 'react-router-dom'

/** Приветственная страница с описанием проекта и навигацией к выбору/созданию персонажа. */
export function HomePage() {
  return (
    <div className="text-center">
      <h1 className="mb-4 text-3xl font-bold text-slate-900">Добро пожаловать в PotionCraft</h1>
      <p className="mx-auto max-w-2xl text-lg text-slate-600">
        Система для автоматизации рутины при броске костей в игре DnD для дополнительных правил
        гербализма и алхимии.
      </p>

      <blockquote className="mx-auto mt-8 max-w-md text-left font-serif italic text-slate-700">
        <p>«РАСКРОЙ ИСТИННЫЙ ПОТЕНЦИАЛ.</p>
        <p className="mt-2">
          Раус Албрайт был очень умел с растениями и маслами. Он знал, как извлечь их истинную
          силу и создать могущественные смеси.
        </p>
        <p className="mt-2">
          Многие пытались создать и украсть эти великолепные отвары, пока он не начал продавать
          свои секреты для массового использования. Сегодня даже самый обычный горожанин может
          создать простой отвар».
        </p>
        <footer className="mt-2 text-right not-italic">
          — Проф. Клэйтон <cite>«Компендиум тайных наук»</cite>
        </footer>
      </blockquote>

      <div className="mt-10 grid gap-6 sm:grid-cols-2 sm:justify-items-center">
        <div className="flex h-full w-full max-w-sm flex-col rounded-lg border border-slate-200 bg-white p-6 text-left shadow-sm">
          <h2 className="text-lg font-semibold text-slate-900">Выбрать персонажа</h2>
          <p className="mt-2 flex-1 text-slate-600">Выбрать вашего персонажа.</p>
          <Link
            to="/characters/select"
            className="mt-4 inline-block rounded-md bg-violet-700 px-4 py-2 text-center font-medium text-white hover:bg-violet-800"
          >
            Выбрать
          </Link>
        </div>
        <div className="flex h-full w-full max-w-sm flex-col rounded-lg border border-slate-200 bg-white p-6 text-left shadow-sm">
          <h2 className="text-lg font-semibold text-slate-900">Создать персонажа</h2>
          <p className="mt-2 flex-1 text-slate-600">Создайте нового персонажа для бросков.</p>
          <Link
            to="/characters/create"
            className="mt-4 inline-block rounded-md bg-emerald-600 px-4 py-2 text-center font-medium text-white hover:bg-emerald-700"
          >
            Создать
          </Link>
        </div>
      </div>
    </div>
  )
}
