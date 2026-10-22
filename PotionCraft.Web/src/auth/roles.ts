import type { CurrentUser, UserRole } from '../types/models'

/** Все роли приложения в порядке приоритета (от наивысшей к низшей). */
export const USER_ROLES: readonly UserRole[] = ['Admin', 'Master', 'Player']

/** Русские названия ролей для отображения в интерфейсе. */
export const USER_ROLE_LABELS: Record<UserRole, string> = {
  Admin: 'Администратор',
  Master: 'Мастер',
  Player: 'Игрок',
}

/** Проверяет, является ли строка известной ролью приложения. */
export function isUserRole(value: string): value is UserRole {
  return (USER_ROLES as readonly string[]).includes(value)
}

/** Возвращает русское название роли; для неизвестной роли возвращает исходную строку. */
export function getRoleLabel(role: string): string {
  return isUserRole(role) ? USER_ROLE_LABELS[role] : role
}

/** Возвращает наивысшую известную роль из списка ролей пользователя или null, если ролей нет. */
export function getPrimaryRole(roles: readonly string[]): UserRole | null {
  return USER_ROLES.find((role) => roles.includes(role)) ?? null
}

/** Проверяет, есть ли у пользователя указанная роль. Для отсутствующего пользователя возвращает false. */
export function hasRole(user: Pick<CurrentUser, 'roles'> | null | undefined, role: UserRole): boolean {
  return user?.roles.includes(role) ?? false
}
