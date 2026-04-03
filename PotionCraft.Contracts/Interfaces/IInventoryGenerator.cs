using PotionCraft.Contracts.Enums;

namespace PotionCraft.Contracts.Interfaces;

/// <summary>
/// Интерфейс генератора инвентаря магазина. Позволяет подменять логику генерации в тестах.
/// </summary>
public interface IInventoryGenerator
{
    /// <summary>
    /// Генерирует количество товара для заполнения магазина на основе редкости.
    /// </summary>
    /// <param name="rarity">Уровень редкости предмета.</param>
    /// <returns>Сгенерированное количество товара (может быть 0).</returns>
    int GetQuantityForShop(RarityEnum rarity);
}
