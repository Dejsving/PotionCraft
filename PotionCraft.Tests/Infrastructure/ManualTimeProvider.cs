namespace PotionCraft.Tests.Infrastructure;

/// <summary>
/// Управляемый источник времени для тестов: время меняется только явным вызовом <see cref="Advance"/>.
/// </summary>
public sealed class ManualTimeProvider : TimeProvider
{
    /// <summary>Текущее время в UTC.</summary>
    private DateTimeOffset _now;

    /// <summary>
    /// Создаёт источник времени с заданным стартовым моментом.
    /// </summary>
    /// <param name="start">Стартовый момент времени.</param>
    public ManualTimeProvider(DateTimeOffset start)
    {
        _now = start;
    }

    /// <summary>
    /// Возвращает текущее управляемое время в UTC.
    /// </summary>
    public override DateTimeOffset GetUtcNow() => _now;

    /// <summary>
    /// Сдвигает время на заданный интервал.
    /// </summary>
    /// <param name="delta">Интервал, на который сдвигается время.</param>
    public void Advance(TimeSpan delta)
    {
        _now = _now.Add(delta);
    }
}
