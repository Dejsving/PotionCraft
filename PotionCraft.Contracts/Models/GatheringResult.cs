namespace PotionCraft.Contracts.Models;

/// <summary>
/// Результат сбора трав.
/// </summary>
public class GatheringResult
{
    /// <summary>
    /// Собранная трава (ингредиент).
    /// </summary>
    public Herb? Herb { get; set; }

    /// <summary>
    /// Количество собранного.
    /// </summary>
    public int Quantity { get; set; }
}