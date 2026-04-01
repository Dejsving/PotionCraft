namespace PotionCraft.Contracts.Models;

/// <summary>
/// Элемент ассортимента магазина — товар, доступный для покупки/продажи.
/// </summary>
public class ShopItem
{
    /// <summary>
    /// Уникальный идентификатор предмета.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Название предмета.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Категория предмета: "herb", "potion", "poison".
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Числовое значение редкости (индекс RarityEnum).
    /// </summary>
    public int Rarity { get; set; }

    /// <summary>
    /// Локализованное название редкости.
    /// </summary>
    public string RarityName { get; set; } = string.Empty;

    /// <summary>
    /// Цена покупки у торговца (в золотых монетах за 1 шт.).
    /// </summary>
    public double BuyPrice { get; set; }

    /// <summary>
    /// Цена продажи торговцу (в золотых монетах за 1 шт.).
    /// </summary>
    public double SellPrice { get; set; }

    /// <summary>
    /// Доступное количество товара у торговца.
    /// </summary>
    public int AvailableQuantity { get; set; }

    /// <summary>
    /// Тип травы (флаги HerbTypeEnum). Используется для фильтрации.
    /// </summary>
    public int HerbType { get; set; }
}
