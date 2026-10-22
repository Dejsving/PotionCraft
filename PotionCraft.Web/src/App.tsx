import { Route, Routes } from 'react-router-dom'
import { AppLayout } from './components/AppLayout'
import { RedirectIfAuthenticated, RequireAuth, RequireRole } from './components/RouteGuards'
import { HomePage } from './pages/HomePage'
import { SelectCharacterPage } from './pages/characters/SelectCharacterPage'
import { CreateCharacterPage } from './pages/characters/CreateCharacterPage'
import { GatheringPage } from './pages/GatheringPage'
import { AlchemyPage } from './pages/AlchemyPage'
import { HerbsPage } from './pages/HerbsPage'
import { ShopPage } from './pages/ShopPage'
import { SourceAndIdeaPage } from './pages/SourceAndIdeaPage'
import { NotFoundPage } from './pages/NotFoundPage'
import { LoginPage } from './pages/auth/LoginPage'
import { RegisterPage } from './pages/auth/RegisterPage'
import { ConfirmEmailPage } from './pages/auth/ConfirmEmailPage'
import { AdminUsersPage } from './pages/admin/AdminUsersPage'

/** Корневой компонент приложения с маршрутизацией. Без входа доступны только /login и /register. */
function App() {
  return (
    <Routes>
      <Route element={<RedirectIfAuthenticated />}>
        <Route path="login" element={<LoginPage />} />
        <Route path="register" element={<RegisterPage />} />
        <Route path="confirm-email" element={<ConfirmEmailPage />} />
      </Route>

      <Route element={<RequireAuth />}>
        <Route element={<AppLayout />}>
          <Route index element={<HomePage />} />
          <Route path="characters/select" element={<SelectCharacterPage />} />
          <Route path="characters/create" element={<CreateCharacterPage />} />
          <Route path="gathering" element={<GatheringPage />} />
          <Route path="alchemy" element={<AlchemyPage />} />
          <Route path="herbs" element={<HerbsPage />} />
          <Route path="shop" element={<ShopPage />} />
          <Route path="source-and-idea" element={<SourceAndIdeaPage />} />
          <Route element={<RequireRole role="Admin" />}>
            <Route path="admin/users" element={<AdminUsersPage />} />
          </Route>
          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Route>
    </Routes>
  )
}

export default App
