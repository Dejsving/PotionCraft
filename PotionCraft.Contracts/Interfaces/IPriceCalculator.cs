using PotionCraft.Contracts.Enums;

namespace PotionCraft.Contracts.Interfaces;

/// <summary>
/// Интерфейс калькулятора цен на предметы. Позволяет подменять стратегию расчёта в тестах.
/// </summary>
public interface IPriceCalculator
{
    /// <summary>
    /// Рассчитывает цену продажи товара на основе его редкости и количества.
    /// </summary>
    /// <param name="rarity">Уровень редкости предмета.</param>
    /// <param name="quantity">Количество товаров.</param>
    /// <returns>Рассчитанная цена продажи.</returns>
    double GetSellPrice(RarityEnum rarity, int quantity);

    /// <summary>
    /// Рассчитывает цену покупки товара на основе его редкости и количества.
    /// </summary>
    /// <param name="rarity">Уровень редкости предмета.</param>
    /// <param name="quantity">Количество товаров.</param>
    /// <returns>Рассчитанная цена покупки.</returns>
    double GetBuyPrice(RarityEnum rarity, int quantity);
}
