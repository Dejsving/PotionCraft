import { describe, expect, it } from 'vitest'
import { emailCodeFormSchema, loginFormSchema, registerFormSchema } from './authSchemas'

const validRegister = {
  userName: 'alchemist',
  email: 'alchemist@example.com',
  password: 'secret123',
  confirmPassword: 'secret123',
}

describe('loginFormSchema', () => {
  it('accepts login and password', () => {
    expect(loginFormSchema.safeParse({ login: 'alchemist', password: 'x', rememberMe: true }).success).toBe(true)
  })

  it('requires login and password', () => {
    const result = loginFormSchema.safeParse({ login: '   ', password: '', rememberMe: false })
    expect(result.success).toBe(false)
    expect(result.error?.issues.map((issue) => issue.path[0])).toEqual(['login', 'password'])
  })
})

describe('registerFormSchema', () => {
  it('accepts valid registration data', () => {
    expect(registerFormSchema.safeParse(validRegister).success).toBe(true)
  })

  it('rejects an invalid email', () => {
    const result = registerFormSchema.safeParse({ ...validRegister, email: 'not-an-email' })
    expect(result.success).toBe(false)
    expect(result.error?.issues[0]?.path[0]).toBe('email')
  })

  it('requires password of at least 8 characters', () => {
    const result = registerFormSchema.safeParse({ ...validRegister, password: 'short', confirmPassword: 'short' })
    expect(result.success).toBe(false)
    expect(result.error?.issues[0]?.message).toBe('Пароль должен быть не короче 8 символов.')
  })

  it('requires the password confirmation to match', () => {
    const result = registerFormSchema.safeParse({ ...validRegister, confirmPassword: 'secret124' })
    expect(result.success).toBe(false)
    expect(result.error?.issues[0]?.path).toEqual(['confirmPassword'])
    expect(result.error?.issues[0]?.message).toBe('Пароли не совпадают.')
  })

  it('requires all fields', () => {
    const result = registerFormSchema.safeParse({ userName: '', email: '', password: '', confirmPassword: '' })
    expect(result.success).toBe(false)
    expect(result.error?.issues.map((issue) => issue.path[0])).toEqual(
      expect.arrayContaining(['userName', 'email', 'password', 'confirmPassword'])
    )
  })
})

describe('emailCodeFormSchema', () => {
  it('accepts exactly 6 digits', () => {
    expect(emailCodeFormSchema.safeParse({ code: '042917' }).success).toBe(true)
  })

  it('rejects codes that are not 6 digits', () => {
    for (const code of ['', '12345', '1234567', '12a456']) {
      const result = emailCodeFormSchema.safeParse({ code })
      expect(result.success, code).toBe(false)
      expect(result.error?.issues[0]?.message).toBe('Введите 6 цифр кода.')
    }
  })
})
