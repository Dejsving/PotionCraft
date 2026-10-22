using Microsoft.Extensions.Logging;

namespace PotionCraft.Tests.Infrastructure;

/// <summary>
/// Логгер для тестов: сохраняет уровень и отформатированный текст каждой записи.
/// </summary>
/// <typeparam name="T">Категория логгера.</typeparam>
public sealed class ListLogger<T> : ILogger<T>
{
    /// <summary>Записи лога в порядке записи.</summary>
    private readonly List<(LogLevel Level, string Message)> _entries = new();

    /// <summary>Записи лога в порядке записи.</summary>
    public IReadOnlyList<(LogLevel Level, string Message)> Entries => _entries;

    /// <summary>
    /// Области видимости не поддерживаются.
    /// </summary>
    /// <typeparam name="TState">Тип состояния области.</typeparam>
    /// <param name="state">Состояние области.</param>
    /// <returns>Всегда <c>null</c>.</returns>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    /// <summary>
    /// Все уровни логирования включены.
    /// </summary>
    /// <param name="logLevel">Уровень записи.</param>
    /// <returns>Всегда <c>true</c>.</returns>
    public bool IsEnabled(LogLevel logLevel) => true;

    /// <summary>
    /// Сохраняет отформатированную запись.
    /// </summary>
    /// <typeparam name="TState">Тип состояния записи.</typeparam>
    /// <param name="logLevel">Уровень записи.</param>
    /// <param name="eventId">Идентификатор события.</param>
    /// <param name="state">Состояние записи.</param>
    /// <param name="exception">Исключение, если есть.</param>
    /// <param name="formatter">Функция форматирования записи.</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        _entries.Add((logLevel, formatter(state, exception)));
    }
}
