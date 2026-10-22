import { afterEach, describe, expect, it, vi } from 'vitest'
import { confirmEmail, getCurrentUser, login, register, resendConfirmationCode } from './auth'
import { ApiError } from './client'

const currentUser = {
  id: '3f2b1c4e-0000-4000-8000-000000000001',
  userName: 'alchemist',
  email: 'alchemist@example.com',
  roles: ['Player'],
}

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('email confirmation API', () => {
  it('register returns the email and does not expect a session', async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      new Response(JSON.stringify({ email: 'alchemist@example.com', requiresEmailConfirmation: true }), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      })
    )
    vi.stubGlobal('fetch', fetchMock)

    await expect(
      register({ userName: 'alchemist', email: 'alchemist@example.com', password: 'secret123' })
    ).resolves.toEqual({ email: 'alchemist@example.com', requiresEmailConfirmation: true })
  })

  it('login exposes the parsed 403 body on ApiError', async () => {
    const body = { message: 'Подтвердите email.', requiresEmailConfirmation: true, email: 'a@example.com' }
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        new Response(JSON.stringify(body), { status: 403, headers: { 'Content-Type': 'application/json' } })
      )
    )

    const error = await login({ login: 'alchemist', password: 'secret123', rememberMe: false }).catch(
      (reason: unknown) => reason
    )
    expect(error).toBeInstanceOf(ApiError)
    expect((error as ApiError).status).toBe(403)
    expect((error as ApiError).body).toEqual(body)
  })

  it('confirmEmail posts the code and returns the signed-in user', async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      new Response(JSON.stringify(currentUser), { status: 200, headers: { 'Content-Type': 'application/json' } })
    )
    vi.stubGlobal('fetch', fetchMock)

    await expect(
      confirmEmail({ email: 'alchemist@example.com', code: '123456', rememberMe: true })
    ).resolves.toEqual(currentUser)
    expect(fetchMock).toHaveBeenCalledWith(
      expect.stringContaining('/api/auth/confirm-email'),
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({ email: 'alchemist@example.com', code: '123456', rememberMe: true }),
      })
    )
  })

  it('resendConfirmationCode posts the email and accepts 204', async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response(null, { status: 204 }))
    vi.stubGlobal('fetch', fetchMock)

    await expect(resendConfirmationCode('alchemist@example.com')).resolves.toBeUndefined()
    expect(fetchMock).toHaveBeenCalledWith(
      expect.stringContaining('/api/auth/resend-code'),
      expect.objectContaining({ method: 'POST', body: JSON.stringify({ email: 'alchemist@example.com' }) })
    )
  })
})

describe('getCurrentUser', () => {
  it('returns the user and sends the session cookie', async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      new Response(JSON.stringify(currentUser), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      })
    )
    vi.stubGlobal('fetch', fetchMock)

    await expect(getCurrentUser()).resolves.toEqual(currentUser)
    expect(fetchMock).toHaveBeenCalledWith(
      expect.stringContaining('/api/auth/me'),
      expect.objectContaining({ credentials: 'include' })
    )
  })

  it('returns null when the session is missing (401)', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(null, { status: 401 })))

    await expect(getCurrentUser()).resolves.toBeNull()
  })

  it('rethrows other errors as ApiError', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        new Response(JSON.stringify({ message: 'Внутренняя ошибка' }), {
          status: 500,
          headers: { 'Content-Type': 'application/json' },
        })
      )
    )

    const error = await getCurrentUser().catch((reason: unknown) => reason)
    expect(error).toBeInstanceOf(ApiError)
    expect((error as ApiError).status).toBe(500)
    expect((error as ApiError).message).toBe('Внутренняя ошибка')
  })
})
