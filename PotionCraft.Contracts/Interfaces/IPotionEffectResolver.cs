using PotionCraft.Contracts.Models;

namespace PotionCraft.Contracts.Interfaces;

/// <summary>
/// Сервис применения трав-модификаторов к базовому эффекту травы-основы для получения итогового описания зелья.
/// </summary>
public interface IPotionEffectResolver
{
    /// <summary>
    /// Вычисляет итоговый эффект зелья на основе травы-основы и упорядоченного списка модификаторов.
    /// </summary>
    /// <param name="baseHerb">Трава-основа (HealingBase или PoisonBase).</param>
    /// <param name="orderedModifiers">Модификаторы в порядке их добавления игроком.</param>
    /// <returns>Итоговый вычисленный эффект.</returns>
    PotionEffectResult Resolve(Herb baseHerb, IReadOnlyList<Herb> orderedModifiers);
}
