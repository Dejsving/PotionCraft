using PotionCraft.Contracts.DiceRolls;
using PotionCraft.Contracts.Services;

namespace PotionCraft.Tests.Services;

/// <summary>
/// Тесты для DiceRoller: проверка корректности бросков кубиков.
/// </summary>
public class DiceRollerTests
{
    /// <summary>
    /// Экземпляр DiceRoller для тестирования.
    /// </summary>
    private readonly DiceRoller _diceRoller = new();

    /// <summary>
    /// Проверяет, что бросок D20 возвращает значение в диапазоне [1, 20].
    /// </summary>
    [Fact]
    public void Roll_D20_ReturnsValueInRange()
    {
        for (int i = 0; i < 100; i++)
        {
            var result = _diceRoller.Roll(DiceRoll.D20);
            Assert.InRange(result, 1, 20);
        }
    }

    /// <summary>
    /// Проверяет, что бросок 2D6 возвращает значение в диапазоне [2, 12].
    /// </summary>
    [Fact]
    public void Roll_TwoD6_ReturnsValueInRange()
    {
        for (int i = 0; i < 100; i++)
        {
            var result = _diceRoller.Roll(DiceRoll.TwoD6);
            Assert.InRange(result, 2, 12);
        }
    }

    /// <summary>
    /// Проверяет, что детальный бросок возвращает правильное количество отдельных бросков.
    /// </summary>
    [Fact]
    public void RollDetailed_TwoD6_ReturnsTwoIndividualRolls()
    {
        var result = _diceRoller.RollDetailed(DiceRoll.TwoD6);

        Assert.Equal(2, result.IndividualRolls.Count);
        Assert.Equal(result.IndividualRolls.Sum(), result.TotalSum);
        foreach (var roll in result.IndividualRolls)
        {
            Assert.InRange(roll, 1, 6);
        }
    }

    /// <summary>
    /// Проверяет, что NextDouble возвращает значение в диапазоне [0.0, 1.0).
    /// </summary>
    [Fact]
    public void NextDouble_ReturnsValueInRange()
    {
        for (int i = 0; i < 100; i++)
        {
            var result = _diceRoller.NextDouble();
            Assert.InRange(result, 0.0, 1.0);
        }
    }

    /// <summary>
    /// Проверяет, что Next возвращает значение в указанном диапазоне.
    /// </summary>
    [Fact]
    public void Next_ReturnsValueInRange()
    {
        for (int i = 0; i < 100; i++)
        {
            var result = _diceRoller.Next(5, 10);
            Assert.InRange(result, 5, 9);
        }
    }
}
