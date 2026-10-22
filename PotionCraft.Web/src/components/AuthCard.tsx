import type { ReactNode } from 'react'

interface AuthCardProps {
  title: string
  children: ReactNode
}

/** Центрированная карточка для страниц входа и регистрации без общего меню приложения. */
export function AuthCard({ title, children }: AuthCardProps) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-50 px-4 py-10">
      <section className="w-full max-w-md rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <h1 className="mb-6 text-center text-2xl font-bold text-violet-700">{title}</h1>
        {children}
      </section>
    </div>
  )
}
