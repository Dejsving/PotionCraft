import { useState } from 'react'
import { NavLink, Outlet } from 'react-router-dom'
import { useSessionStore } from '../store/sessionStore'
import { ActiveCharacterBadge } from './ActiveCharacterBadge'
import { BagPanel } from './BagPanel'

const NAV_LINKS = [
  { to: '/', label: 'Home', end: true },
  { to: '/characters/select', label: 'Персонажи' },
  { to: '/gathering', label: 'Сбор трав' },
  { to: '/alchemy', label: 'Алхимия' },
  { to: '/herbs', label: 'Травы' },
  { to: '/shop', label: 'Магазин' },
  { to: '/source-and-idea', label: 'Источник и идея' },
]

/** Общий layout приложения: навигация, badge персонажа и офканвас сумки. Аналог _Layout.cshtml. */
export function AppLayout() {
  const activeCharacter = useSessionStore((state) => state.activeCharacter)
  const [isBagOpen, setIsBagOpen] = useState(false)

  return (
    <div className="flex min-h-screen flex-col bg-slate-50">
      <header className="border-b border-slate-200 bg-white shadow-sm">
        <nav className="container mx-auto flex flex-wrap items-center justify-between gap-3 px-4 py-3">
          <div className="flex flex-wrap items-center gap-4">
            <NavLink to="/" className="text-lg font-bold text-violet-700">
              PotionCraft
            </NavLink>
            <ul className="flex flex-wrap gap-3">
              {NAV_LINKS.map((link) => (
                <li key={link.to}>
                  <NavLink
                    to={link.to}
                    end={link.end}
                    className={({ isActive }) =>
                      `text-sm font-medium ${isActive ? 'text-violet-700' : 'text-slate-700 hover:text-violet-600'}`
                    }
                  >
                    {link.label}
                  </NavLink>
                </li>
              ))}
            </ul>
          </div>
          <div className="flex items-center gap-4">
            {activeCharacter && (
              <button
                type="button"
                className="text-xl"
                title="Сумка"
                onClick={() => setIsBagOpen(true)}
              >
                🎒
              </button>
            )}
            <ActiveCharacterBadge />
          </div>
        </nav>
      </header>

      <main className="container mx-auto flex-1 px-4 py-6">
        <Outlet />
      </main>

      <footer className="border-t border-slate-200 bg-white py-4 text-center text-sm text-slate-500">
        © 2026 - PotionCraft -{' '}
        <NavLink to="/source-and-idea" className="text-violet-700 hover:underline">
          Источник и идея
        </NavLink>
      </footer>

      <BagPanel
        characterId={activeCharacter?.id ?? null}
        isOpen={isBagOpen}
        onClose={() => setIsBagOpen(false)}
      />
    </div>
  )
}
