namespace PotionCraft.Contracts.Models;

/// <summary>
/// Результат выполнения сделки в магазине.
/// </summary>
public class TradeResult
{
    /// <summary>
    /// Признак успешного завершения сделки.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Изменение баланса в медных монетах (положительное — доход, отрицательное — расход).
    /// </summary>
    public int BalanceChange { get; set; }

    /// <summary>
    /// Сообщение о результате сделки.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Обновлённая сумка персонажа после сделки.
    /// </summary>
    public CharacterBag? UpdatedBag { get; set; }
}
