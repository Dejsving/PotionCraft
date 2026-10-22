const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''

/** Ошибка обращения к API сервера с сохранением HTTP-статуса ответа и списка ошибок валидации. */
export class ApiError extends Error {
  readonly status: number
  /** Детали ошибок валидации (например, при регистрации); пустой массив, если их нет. */
  readonly errors: string[]
  /** Распарсенное JSON-тело ответа (undefined, если тела нет или это не JSON). */
  readonly body: unknown

  constructor(message: string, status: number, errors: string[] = [], body?: unknown) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.errors = errors
    this.body = body
  }
}

/** Выполняет HTTP-запрос к API с cookie сессии и превращает не-успешные ответы в ApiError. */
async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
      ...init?.headers,
    },
  })

  if (!response.ok) {
    let message = response.statusText
    let errors: string[] = []
    let body: unknown
    try {
      body = await response.json()
      const data = body as { message?: string; errors?: string[] } | null
      message = data?.message ?? message
      const list = data?.errors
      errors = Array.isArray(list) ? list : []
    } catch {
      // тело ответа отсутствует или не JSON — используем statusText
    }
    throw new ApiError(message, response.status, errors, body)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

/** Возвращает текст ошибки для показа пользователю: сообщение ApiError или запасной текст. */
export function getErrorMessage(error: unknown, fallback: string): string {
  return error instanceof ApiError && error.message ? error.message : fallback
}

/** Обёртка над fetch с базовым URL из VITE_API_BASE_URL и типизированными методами. */
export const apiClient = {
  get: <T>(path: string): Promise<T> => request<T>(path, { method: 'GET' }),
  post: <T>(path: string, body?: unknown): Promise<T> =>
    request<T>(path, {
      method: 'POST',
      body: body !== undefined ? JSON.stringify(body) : undefined,
    }),
  put: <T>(path: string, body?: unknown): Promise<T> =>
    request<T>(path, {
      method: 'PUT',
      body: body !== undefined ? JSON.stringify(body) : undefined,
    }),
}
