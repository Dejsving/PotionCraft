import { ApiError, apiClient } from './client'
import type {
  ConfirmEmailRequest,
  CurrentUser,
  LoginRequest,
  RegisterRequest,
  RegisterResponse,
} from '../types/models'

/** Регистрирует нового пользователя; сессия не выставляется, нужно подтвердить email кодом. POST /api/auth/register */
export function register(request: RegisterRequest): Promise<RegisterResponse> {
  return apiClient.post<RegisterResponse>('/api/auth/register', request)
}

/** Подтверждает email 6-значным кодом и выставляет cookie сессии. POST /api/auth/confirm-email */
export function confirmEmail(request: ConfirmEmailRequest): Promise<CurrentUser> {
  return apiClient.post<CurrentUser>('/api/auth/confirm-email', request)
}

/** Повторно отправляет код подтверждения (сервер отправляет не чаще раза в 60 секунд). POST /api/auth/resend-code */
export function resendConfirmationCode(email: string): Promise<void> {
  return apiClient.post<void>('/api/auth/resend-code', { email })
}

/** Выполняет вход по имени пользователя или email. POST /api/auth/login */
export function login(request: LoginRequest): Promise<CurrentUser> {
  return apiClient.post<CurrentUser>('/api/auth/login', request)
}

/** Завершает текущую сессию. POST /api/auth/logout */
export function logout(): Promise<void> {
  return apiClient.post<void>('/api/auth/logout')
}

/**
 * Возвращает текущего пользователя или null, если сессии нет (401).
 * GET /api/auth/me
 */
export async function getCurrentUser(): Promise<CurrentUser | null> {
  try {
    return await apiClient.get<CurrentUser>('/api/auth/me')
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) {
      return null
    }
    throw error
  }
}
