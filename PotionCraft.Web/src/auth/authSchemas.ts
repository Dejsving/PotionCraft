import { z } from 'zod'

/** Минимальная длина пароля при регистрации. */
export const MIN_PASSWORD_LENGTH = 8

/** Схема формы входа: имя пользователя или email и пароль. */
export const loginFormSchema = z.object({
  login: z.string().trim().min(1, 'Введите имя пользователя или email.'),
  password: z.string().min(1, 'Введите пароль.'),
  rememberMe: z.boolean(),
})

/** Значения формы входа. */
export type LoginFormValues = z.infer<typeof loginFormSchema>

/** Схема формы регистрации: все поля обязательны, email корректный, пароли совпадают. */
export const registerFormSchema = z
  .object({
    userName: z.string().trim().min(1, 'Введите имя пользователя.'),
    email: z
      .string()
      .trim()
      .min(1, 'Введите email.')
      .pipe(z.email('Введите корректный email.')),
    password: z
      .string()
      .min(MIN_PASSWORD_LENGTH, `Пароль должен быть не короче ${MIN_PASSWORD_LENGTH} символов.`),
    confirmPassword: z.string().min(1, 'Подтвердите пароль.'),
  })
  .refine((values) => values.password === values.confirmPassword, {
    message: 'Пароли не совпадают.',
    path: ['confirmPassword'],
  })

/** Значения формы регистрации. */
export type RegisterFormValues = z.infer<typeof registerFormSchema>

/** Схема формы кода подтверждения email: ровно 6 цифр. */
export const emailCodeFormSchema = z.object({
  code: z.string().trim().regex(/^\d{6}$/, 'Введите 6 цифр кода.'),
})

/** Значения формы кода подтверждения email. */
export type EmailCodeFormValues = z.infer<typeof emailCodeFormSchema>
