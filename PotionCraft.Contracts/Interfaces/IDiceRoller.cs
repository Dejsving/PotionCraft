using PotionCraft.Contracts.DiceRolls;

namespace PotionCraft.Contracts.Interfaces;

/// <summary>
/// Интерфейс сервиса бросков кубиков. Позволяет подменять генератор случайных чисел в тестах.
/// </summary>
public interface IDiceRoller
{
    /// <summary>
    /// Совершает бросок кубиков и возвращает итоговую сумму.
    /// </summary>
    /// <param name="dice">Описание броска (количество граней и кубиков).</param>
    /// <returns>Сумма выпавших значений.</returns>
    int Roll(DiceRoll dice);

    /// <summary>
    /// Совершает бросок кубиков и возвращает детальный результат.
    /// </summary>
    /// <param name="dice">Описание броска (количество граней и кубиков).</param>
    /// <returns>Результат с суммой и значениями каждого кубика.</returns>
    DiceRollResult RollDetailed(DiceRoll dice);

    /// <summary>
    /// Возвращает случайное вещественное число в диапазоне [0.0, 1.0).
    /// </summary>
    /// <returns>Случайное число от 0.0 до 1.0.</returns>
    double NextDouble();

    /// <summary>
    /// Возвращает случайное целое число в диапазоне [minValue, maxValue).
    /// </summary>
    /// <param name="minValue">Нижняя граница (включительно).</param>
    /// <param name="maxValue">Верхняя граница (исключительно).</param>
    /// <returns>Случайное целое число.</returns>
    int Next(int minValue, int maxValue);
}
