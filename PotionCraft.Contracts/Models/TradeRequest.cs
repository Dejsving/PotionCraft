namespace PotionCraft.Contracts.Models;

/// <summary>
/// Запрос на выполнение сделки в магазине.
/// </summary>
public class TradeRequest
{
    /// <summary>
    /// Идентификатор персонажа, совершающего сделку.
    /// </summary>
    public Guid CharacterId { get; set; }

    /// <summary>
    /// Список предметов, которые персонаж покупает у торговца.
    /// </summary>
    public List<TradeItem> ItemsToBuy { get; set; } = new();

    /// <summary>
    /// Список предметов, которые персонаж продаёт торговцу.
    /// </summary>
    public List<TradeItem> ItemsToSell { get; set; } = new();
}

/// <summary>
/// Элемент сделки — предмет с указанием количества.
/// </summary>
public class TradeItem
{
    /// <summary>
    /// Идентификатор предмета.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Количество единиц товара в сделке.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Категория предмета: "herb", "potion", "poison".
    /// </summary>
    public string Category { get; set; } = "herb";
}
