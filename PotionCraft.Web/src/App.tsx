import { Route, Routes } from 'react-router-dom'
import { AppLayout } from './components/AppLayout'
import { HomePage } from './pages/HomePage'
import { SelectCharacterPage } from './pages/characters/SelectCharacterPage'
import { CreateCharacterPage } from './pages/characters/CreateCharacterPage'
import { GatheringPage } from './pages/GatheringPage'
import { AlchemyPage } from './pages/AlchemyPage'
import { HerbsPage } from './pages/HerbsPage'
import { ShopPage } from './pages/ShopPage'
import { SourceAndIdeaPage } from './pages/SourceAndIdeaPage'
import { NotFoundPage } from './pages/NotFoundPage'

/** Корневой компонент приложения с маршрутизацией. */
function App() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route index element={<HomePage />} />
        <Route path="characters/select" element={<SelectCharacterPage />} />
        <Route path="characters/create" element={<CreateCharacterPage />} />
        <Route path="gathering" element={<GatheringPage />} />
        <Route path="alchemy" element={<AlchemyPage />} />
        <Route path="herbs" element={<HerbsPage />} />
        <Route path="shop" element={<ShopPage />} />
        <Route path="source-and-idea" element={<SourceAndIdeaPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}

export default App
