import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { MutationCache, QueryCache, QueryClient, QueryClientProvider } from '@tanstack/react-query'
import './index.css'
import App from './App.tsx'
import { ErrorBoundary } from './components/ErrorBoundary.tsx'
import { ApiError } from './api/client.ts'
import { currentUserQueryKey } from './hooks/useCurrentUser.ts'

/** Сбрасывает текущего пользователя при 401, чтобы маршруты перенаправили на страницу входа. */
function handleUnauthorized(error: unknown, queryKey?: readonly unknown[]): void {
  if (!(error instanceof ApiError) || error.status !== 401) {
    return
  }
  if (queryKey && queryKey[0] === currentUserQueryKey[0] && queryKey[1] === currentUserQueryKey[1]) {
    return
  }
  queryClient.setQueryData(currentUserQueryKey, null)
}

/** Клиент TanStack Query с глобальной обработкой истёкшей сессии (401). */
const queryClient = new QueryClient({
  queryCache: new QueryCache({
    onError: (error, query) => handleUnauthorized(error, query.queryKey),
  }),
  mutationCache: new MutationCache({
    onError: (error) => handleUnauthorized(error),
  }),
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <App />
        </BrowserRouter>
      </QueryClientProvider>
    </ErrorBoundary>
  </StrictMode>
)
