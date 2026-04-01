namespace PotionCraft.Contracts.Models;

/// <summary>
/// Элемент сумки — зелье или яд с указанием количества.
/// </summary>
public class PotionBagItem
{
    /// <summary>
    /// Зелье или яд.
    /// </summary>
    public Potion? Potion { get; set; }

    /// <summary>
    /// Количество единиц.
    /// </summary>
    public int Quantity { get; set; }
}
