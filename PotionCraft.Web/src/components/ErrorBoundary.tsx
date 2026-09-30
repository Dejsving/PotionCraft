import { Component, type ErrorInfo, type ReactNode } from 'react'

interface Props {
  children: ReactNode
}

interface State {
  error: Error | null
  requestId: string | null
}

/** Границa ошибок React-дерева — замена страницы Error.cshtml, показывает сообщение и id запроса. */
export class ErrorBoundary extends Component<Props, State> {
  state: State = { error: null, requestId: null }

  static getDerivedStateFromError(error: Error): State {
    return { error, requestId: crypto.randomUUID() }
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Необработанная ошибка в приложении:', error, errorInfo)
  }

  render() {
    const { error, requestId } = this.state
    if (!error) {
      return this.props.children
    }

    return (
      <div className="mx-auto max-w-2xl py-12 text-center">
        <h1 className="text-3xl font-bold text-red-700">Ошибка.</h1>
        <h2 className="mt-2 text-xl font-semibold text-red-700">
          При обработке вашего запроса произошла ошибка.
        </h2>
        {requestId && (
          <p className="mt-4 text-slate-700">
            <strong>Request ID:</strong> <code>{requestId}</code>
          </p>
        )}
        <p className="mt-6 text-slate-600">{error.message}</p>
        <a href="/" className="mt-6 inline-block text-violet-700 hover:underline">
          Вернуться на главную
        </a>
      </div>
    )
  }
}
