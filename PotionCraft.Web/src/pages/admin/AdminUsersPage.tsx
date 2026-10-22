import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { getErrorMessage } from '../../api/client'
import { getUsers, setUserRole } from '../../api/users'
import { getPrimaryRole, getRoleLabel, USER_ROLES } from '../../auth/roles'
import { currentUserQueryKey, useCurrentUser } from '../../hooks/useCurrentUser'
import type { UserRole, UserSummary } from '../../types/models'

/** Страница администратора: список пользователей и смена их ролей. */
export function AdminUsersPage() {
  const queryClient = useQueryClient()
  const { data: currentUser } = useCurrentUser()
  const usersQuery = useQuery({
    queryKey: ['users'],
    queryFn: getUsers,
  })

  const roleMutation = useMutation({
    mutationFn: ({ id, role }: { id: string; role: UserRole }) => setUserRole(id, role),
    onSuccess: async (updatedUser) => {
      queryClient.setQueryData<UserSummary[]>(['users'], (users) =>
        users?.map((user) => (user.id === updatedUser.id ? updatedUser : user))
      )
      if (updatedUser.id === currentUser?.id) {
        await queryClient.invalidateQueries({ queryKey: currentUserQueryKey })
      }
    },
  })

  const mutationError = roleMutation.error
    ? getErrorMessage(roleMutation.error, 'Не удалось изменить роль пользователя.')
    : null

  return (
    <section>
      <h1 className="mb-2 text-2xl font-bold text-slate-900">Пользователи</h1>
      <p className="mb-6 text-sm text-slate-600">Просмотр пользователей и назначение им ролей.</p>

      {mutationError && (
        <p role="alert" className="mb-4 rounded-md border border-rose-300 bg-rose-50 px-4 py-3 text-sm text-rose-800">
          {mutationError}
        </p>
      )}

      {usersQuery.isPending && (
        <p role="status" className="py-8 text-center text-slate-600">Загрузка пользователей...</p>
      )}
      {usersQuery.isError && (
        <div role="alert" className="rounded-md border border-rose-300 bg-rose-50 p-4 text-rose-800">
          <p>Не удалось загрузить пользователей. {usersQuery.error.message}</p>
          <button
            type="button"
            className="mt-3 font-semibold underline underline-offset-2"
            onClick={() => void usersQuery.refetch()}
          >
            Повторить
          </button>
        </div>
      )}

      {usersQuery.data && (
        <div className="overflow-x-auto rounded-md border border-slate-200 bg-white shadow-sm">
          <table className="min-w-full divide-y divide-slate-200 text-sm">
            <thead className="bg-slate-50 text-left font-semibold text-slate-700">
              <tr>
                <th className="px-4 py-3">Имя пользователя</th>
                <th className="px-4 py-3">Email</th>
                <th className="px-4 py-3">Роль</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {usersQuery.data.map((user) => {
                const primaryRole = getPrimaryRole(user.roles)
                const isUpdating = roleMutation.isPending && roleMutation.variables?.id === user.id
                return (
                  <tr key={user.id}>
                    <td className="px-4 py-3 font-medium text-slate-900">
                      {user.userName}
                      {user.id === currentUser?.id && (
                        <span className="ml-2 text-xs font-medium text-blue-700">(Вы)</span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-slate-700">{user.email}</td>
                    <td className="px-4 py-3">
                      <label htmlFor={`role-${user.id}`} className="sr-only">
                        Роль пользователя {user.userName}
                      </label>
                      <select
                        id={`role-${user.id}`}
                        className="rounded-md border border-slate-300 px-2 py-1 focus:border-emerald-600 focus:outline-none disabled:opacity-70"
                        value={primaryRole ?? ''}
                        disabled={isUpdating}
                        onChange={(event) =>
                          roleMutation.mutate({ id: user.id, role: event.target.value as UserRole })
                        }
                      >
                        {!primaryRole && (
                          <option value="" disabled>
                            Без роли
                          </option>
                        )}
                        {USER_ROLES.map((role) => (
                          <option key={role} value={role}>
                            {getRoleLabel(role)}
                          </option>
                        ))}
                      </select>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
